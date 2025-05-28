// 스크립트 설명: 광고 시청 등을 통해 보상을 획득할 수 있는 보상 상자의 동작을 처리하는 클래스입니다.
// 보상 상자 애니메이션, 광고 관련 UI 표시, 광고 시청 후 보상 드롭 기능을 구현합니다.
using System.Collections.Generic; // List 사용을 위한 네임스페이스
using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위한 네임스페이스
using Watermelon; // Tween, AdsManager, Haptic 관련 네임스페이스

namespace Watermelon.SquadShooter
{
    public class RewardedChestBehavior : AbstractChestBehavior // AbstractChestBehavior 상속 (이전 파일에서 정의된 것으로 가정)
    {
        // 애니메이터 상태 해시 값 (정적 상수)
        protected static readonly int IS_OPEN_HASH = Animator.StringToHash("IsOpen"); // 열림 상태 불리언 파라미터 해시

        [SerializeField]
        [Tooltip("보상 상자 UI 애니메이션을 제어하는 애니메이터 컴포넌트 참조")] // 주요 변수 한글 툴팁
        Animator rvAnimator; // 보상 상자 UI 애니메이터

        [SerializeField]
        [Tooltip("보상 획득을 위한 버튼 컴포넌트 참조")] // 주요 변수 한글 툴팁
        Button rvButton; // 보상 버튼

        [SerializeField]
        [Tooltip("보상 상자 UI 요소를 담는 트랜스폼 (광고 관련 UI)")] // 주요 변수 한글 툴팁
        Transform adHolder; // 광고 UI 컨테이너

        [SerializeField]
        [Tooltip("보상 상자 UI를 렌더링하는 Canvas 컴포넌트")] // 주요 변수 한글 툴팁
        Canvas adCanvas; // 광고 UI 캔버스

        [SerializeField]
        [Tooltip("게임패드 사용 시 보상 버튼에 포커스를 설정하는 UI 컴포넌트")] // 주요 변수 한글 툴팁
        UIGamepadButton gamepadButton; // 게임패드 버튼 (UIGamepadButton에 정의된 것으로 가정)

        /// <summary>
        /// 스크립트 인스턴스가 로드될 때 처음 호출됩니다.
        /// 보상 버튼 클릭 이벤트 리스너를 추가하고 광고 UI를 초기 상태로 설정합니다.
        /// </summary>
        private void Awake()
        {
            rvButton.onClick.AddListener(OnButtonClick); // 보상 버튼 클릭 시 OnButtonClick 메서드 호출
            adHolder.transform.localScale = Vector3.zero; // 광고 UI 초기 상태 (숨김)
        }

        /// <summary>
        /// 모든 Update 함수가 호출된 후 각 프레임마다 호출됩니다.
        /// 광고 UI 캔버스가 항상 메인 카메라를 바라보도록 설정합니다.
        /// </summary>
        private void LateUpdate()
        {
            // 광고 UI 캔버스의 정방향 벡터를 메인 카메라의 정방향 벡터와 일치시킵니다.
            if (Camera.main != null) // 메인 카메라가 존재하는지 확인
            {
                 adCanvas.transform.forward = Camera.main.transform.forward;
            }
        }

        /// <summary>
        /// 보상 상자 행동을 초기화합니다.
        /// 부모 클래스의 초기화와 함께 보상 상자 UI 상태를 설정합니다.
        /// </summary>
        /// <param name="drop">상자에서 드롭될 아이템 데이터 목록.</param>
        public override void Init(List<DropData> drop)
        {
            base.Init(drop); // 부모 클래스의 Init 메서드 호출

            rvAnimator.transform.localScale = Vector3.zero; // 보상 상자 UI 애니메이터 오브젝트 초기 상태 (숨김)

            isRewarded = true; // 보상 상자임을 표시
        }

        /// <summary>
        /// 캐릭터가 보상 상자에 접근했을 때 호출됩니다.
        /// 상자 애니메이션을 흔들림 상태로 변경하고 보상 상자 UI를 표시합니다.
        /// </summary>
        public override void ChestApproached()
        {
            if (opened) // 이미 개봉되었다면 처리 중지
                return;

            animatorRef.SetTrigger(SHAKE_HASH); // 상자 애니메이션을 흔들림 상태로 변경
            rvAnimator.SetBool(IS_OPEN_HASH, true); // 보상 상자 UI 애니메이션을 열림 상태로 변경

            gamepadButton.SetFocus(true); // 게임패드 사용 시 버튼에 포커스 설정 (gamepadButton에 정의된 것으로 가정)
        }

        /// <summary>
        /// 캐릭터가 보상 상자에서 멀어졌을 때 호출됩니다.
        /// 상자 애니메이션을 대기 상태로 변경하고 보상 상자 UI를 숨깁니다.
        /// </summary>
        public override void ChestLeft()
        {
            if (opened) // 이미 개봉되었다면 처리 중지
                return;

            animatorRef.SetTrigger(IDLE_HASH); // 상자 애니메이션을 대기 상태로 변경
            rvAnimator.SetBool(IS_OPEN_HASH, false); // 보상 상자 UI 애니메이션을 닫힘 상태로 변경

            gamepadButton.SetFocus(false); // 게임패드 사용 시 버튼 포커스 해제 (gamepadButton에 정의된 것으로 가정)
        }

        /// <summary>
        /// 보상 버튼 클릭 시 호출되는 메서드입니다.
        /// 보상형 광고를 재생하고, 광고 시청 성공 시 상자 개봉 및 보상 드롭을 처리합니다.
        /// </summary>
        private void OnButtonClick()
        {
            // 보상형 광고 재생 (AdsManager에 정의된 것으로 가정)
            AdsManager.ShowRewardBasedVideo((success) =>
            {
                if (success) // 광고 시청 성공 시
                {
                    opened = true; // 상자 개봉 상태로 변경

                    animatorRef.SetTrigger(OPEN_HASH); // 상자 열림 애니메이션 재생
                    rvAnimator.SetBool(IS_OPEN_HASH, false); // 보상 상자 UI 애니메이션을 닫힘 상태로 변경

                    // 잠시 지연 후 보상 드롭 및 파티클 비활성화
                    Tween.DelayedCall(0.3f, () => // Tween에 정의된 것으로 가정
                    {
                        DropResources(); // 보상 아이템 드롭 (BaseDropBehavior에 정의된 것으로 가정)
                        particle.SetActive(false); // 파티클 비활성화

#if MODULE_HAPTIC // Haptic 모듈이 활성화된 경우에만 실행
                        Haptic.Play(Haptic.HAPTIC_LIGHT); // 약한 진동 피드백 (Haptic에 정의된 것으로 가정)
#endif
                    });

                    gamepadButton.SetFocus(false); // 게임패드 사용 시 버튼 포커스 해제 (gamepadButton에 정의된 것으로 가정)
                }
            });
        }
    }
}