// 스크립트 설명: 게임 플레이 중 필드에 배치되는 일반 상자의 동작을 처리하는 클래스입니다.
// 상자 개봉 시간, 개봉 애니메이션 및 UI 표시, 리소스 드롭 기능 등을 구현합니다.
using System.Collections; // 코루틴 사용을 위한 네임스페이스
using System.Collections.Generic; // List 사용을 위한 네임스페이스
using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위한 네임스페이스
using Watermelon; // Tween 관련 네임스페이스

namespace Watermelon.SquadShooter
{
    public class InGameChestBehavior : AbstractChestBehavior // AbstractChestBehavior 상속 (이전 파일에서 정의된 것으로 가정)
    {
        [SerializeField]
        [Tooltip("상자를 개봉하는 데 걸리는 시간 (초)")] // 주요 변수 한글 툴팁
        float openDuration = 3f; // 상자 개봉 시간

        [SerializeField]
        [Tooltip("상자 개봉 진행 상태를 표시하는 원형 UI의 부모 오브젝트")] // 주요 변수 한글 툴팁
        Transform fillCircleHolder; // 채우기 원형 UI 부모

        [SerializeField]
        [Tooltip("상자 개봉 진행 상태를 표시하는 원형 이미지 컴포넌트")] // 주요 변수 한글 툴팁
        Image fillCircleImage; // 채우기 원형 이미지

        private Coroutine openCoroutine; // 상자 개봉 코루틴 참조
        private TweenCase circleTween; // 채우기 원형 UI 스케일 애니메이션 트윈 케이스

        /// <summary>
        /// 인게임 상자 행동을 초기화합니다.
        /// 부모 클래스의 초기화와 함께 채우기 원형 UI 상태를 설정합니다.
        /// </summary>
        /// <param name="drop">상자에서 드롭될 아이템 데이터 목록.</param>
        public override void Init(List<DropData> drop)
        {
            base.Init(drop); // 부모 클래스의 Init 메서드 호출

            // 채우기 원형 UI 초기 상태 설정 (숨김)
            fillCircleHolder.localScale = Vector3.zero;
            fillCircleImage.fillAmount = 0f;

            isRewarded = false; // 일반 상자는 보상 상자가 아님
        }

        /// <summary>
        /// 캐릭터가 상자에 접근했을 때 호출됩니다.
        /// 상자 개봉 코루틴을 시작합니다.
        /// </summary>
        public override void ChestApproached()
        {
            if (opened) // 이미 개봉되었다면 처리 중지
                return;

            // 기존 개봉 코루틴이 있다면 중지
            if (openCoroutine != null)
            {
                StopCoroutine(openCoroutine);
                openCoroutine = null;
            }

            // 상자 개봉 코루틴 시작
            openCoroutine = StartCoroutine(ChestOpenCoroutine());
        }

        /// <summary>
        /// 상자를 개봉하는 과정을 처리하는 코루틴입니다.
        /// 흔들림 애니메이션 재생, 타이머 및 UI 업데이트, 보상 드롭 등을 수행합니다.
        /// </summary>
        private IEnumerator ChestOpenCoroutine()
        {
            animatorRef.SetTrigger(SHAKE_HASH); // 흔들림 애니메이션 재생

            float timer = 0; // 개봉 타이머

            circleTween.KillActive(); // 기존 채우기 원형 스케일 트윈 중지

            // 채우기 원형 UI를 나타내는 스케일 애니메이션 시작 (Tween에 정의된 것으로 가정)
            circleTween = fillCircleHolder.DOScale(1f, 0.2f).SetEasing(Ease.Type.CubicOut);

            // 개봉 시간 동안 타이머 및 UI 업데이트
            while (timer < openDuration)
            {
                timer += Time.deltaTime; // 시간 경과 업데이트

                fillCircleImage.fillAmount = timer / openDuration; // 원형 이미지 채우기 양 업데이트
                yield return null; // 다음 프레임까지 대기
            }

            opened = true; // 상자 개봉 상태로 변경

            animatorRef.SetTrigger(OPEN_HASH); // 상자 열림 애니메이션 재생
            fillCircleHolder.localScale = Vector3.zero; // 채우기 원형 UI 숨김

            // 잠시 지연 후 보상 드롭 및 파티클 비활성화
            Tween.DelayedCall(0.3f, () => // Tween에 정의된 것으로 가정
            {
                DropResources(); // 보상 아이템 드롭 (BaseDropBehavior에 정의된 것으로 가정)
                particle.SetActive(false); // 파티클 비활성화

#if MODULE_HAPTIC // Haptic 모듈이 활성화된 경우에만 실행
                Haptic.Play(Haptic.HAPTIC_LIGHT); // 약한 진동 피드백 (Haptic에 정의된 것으로 가정)
#endif
            });

            openCoroutine = null; // 코루틴 참조 해제
        }

        /// <summary>
        /// 캐릭터가 상자에서 멀어졌을 때 호출됩니다.
        /// 개봉 진행 상태를 초기화하고 애니메이션을 대기 상태로 되돌립니다.
        /// </summary>
        public override void ChestLeft()
        {
            if (opened) // 이미 개봉되었다면 처리 중지
                return;

            circleTween.KillActive(); // 채우기 원형 스케일 트윈 중지

            // 채우기 원형 UI를 숨기는 스케일 애니메이션 시작 (Tween에 정의된 것으로 가정)
            circleTween = fillCircleHolder.DOScale(0f, 0.2f).SetEasing(Ease.Type.CubicOut);

            animatorRef.SetTrigger(IDLE_HASH); // 애니메이션을 대기 상태로 되돌림

            // 개봉 코루틴이 실행 중이라면 중지
            if (openCoroutine != null)
            {
                StopCoroutine(openCoroutine);
                openCoroutine = null;
            }
        }
    }
}