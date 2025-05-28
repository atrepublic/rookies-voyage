// GateExitPointBehaviour.cs
// 이 스크립트는 문 형태의 레벨 출구 지점의 동작을 구현합니다.
// ExitPointBehaviour 추상 클래스를 상속받아 문 애니메이션 재생, 효과음 출력 등의 구체적인 동작을 정의합니다.
using UnityEngine;
// Tween, RingEffectController, AudioController, LevelController 클래스는 외부 정의가 필요합니다.

namespace Watermelon.LevelSystem
{
    // GateExitPointBehaviour 클래스는 ExitPointBehaviour를 상속받으며 더 이상 상속될 수 없습니다.
    public sealed class GateExitPointBehaviour : ExitPointBehaviour
    {
        // Animator 컨트롤러에서 사용할 애니메이션 해시 값 미리 계산
        private readonly int IDLE_HASH = Animator.StringToHash("Idle"); // 대기 애니메이션 해시 값
        private readonly int OPEN_HASH = Animator.StringToHash("Open"); // 열림 애니메이션 해시 값

        [SerializeField]
        [Tooltip("문의 애니메이터 컴포넌트")] // gatesAnimator 변수에 대한 툴팁
        private Animator gatesAnimator;

        /// <summary>
        /// 출구 지점을 초기 상태로 초기화합니다.
        /// 문 애니메이터를 대기 상태로 설정합니다.
        /// </summary>
        public override void Init()
        {
            // 문 애니메이터를 대기 상태 애니메이션으로 재생
            gatesAnimator.Play(IDLE_HASH);
        }

        /// <summary>
        /// 출구 지점이 활성화될 때 호출됩니다.
        /// 출구 활성화 상태를 true로 설정하고, 문 열림 애니메이션을 재생하며, 효과음과 시각 효과를 발생시킵니다.
        /// </summary>
        public override void OnExitActivated()
        {
            // 출구 활성화 상태를 true로 설정
            isExitActivated = true;

            // 문 열림 애니메이션 재생
            gatesAnimator.Play(OPEN_HASH);

            // 고리 형태의 시각 효과 발생 (RingEffectController 클래스는 외부 정의가 필요합니다.)
            RingEffectController.SpawnEffect(transform.position.SetY(0.1f), 4.5f, 2, Ease.Type.Linear); // SetY와 Ease.Type은 외부 정의가 필요할 수 있습니다.

            // 완료 효과음 재생 (AudioController 클래스는 외부 정의가 필요합니다.)
            AudioController.PlaySound(AudioController.AudioClips.complete); // AudioClips 열거형은 외부 정의가 필요합니다.

            // 0.15초 지연 후 문 소리 효과음 재생 (Tween 클래스는 외부 정의가 필요합니다.)
            Tween.DelayedCall(0.15f, () =>
            {
                AudioController.PlaySound(AudioController.AudioClips.door); // AudioClips 열거형은 외부 정의가 필요합니다.
            });
        }

        /// <summary>
        /// 플레이어가 출구 지점 트리거 영역에 진입했을 때 호출됩니다.
        /// 출구 활성화 상태를 false로 설정하고, 레벨 컨트롤러에 플레이어의 레벨 이탈을 알립니다.
        /// </summary>
        public override void OnPlayerEnteredExit()
        {
            // 출구 활성화 상태를 false로 설정하여 추가 트리거 방지
            isExitActivated = false;

            // 레벨 컨트롤러에 플레이어가 레벨을 나갔음을 알림 (LevelController 클래스는 외부 정의가 필요합니다.)
            LevelController.OnPlayerExitLevel();
        }

        /// <summary>
        /// 출구 지점을 언로드하거나 정리할 때 호출됩니다. (현재 구현에서는 비어 있습니다.)
        /// </summary>
        public override void Unload()
        {
            // 현재는 특별한 언로드 로직 없음
        }
    }
}