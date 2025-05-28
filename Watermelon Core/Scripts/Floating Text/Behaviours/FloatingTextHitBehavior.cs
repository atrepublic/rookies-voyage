// FloatingTextHitBehavior.cs
// 이 스크립트는 공격 히트 표시용 부동 텍스트(FloatingText)를 관리하며, 지연 콜백과 회전, 스케일 애니메이션을 제어하여
// 텍스트가 나타난 후 자동으로 사라지도록 처리합니다.

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextHitBehavior : FloatingTextBaseBehavior
    {
        [Header("텍스트 설정")]
        [SerializeField]
        [Tooltip("표시할 TextMeshProUGUI 컴포넌트 참조")]
        private TextMeshProUGUI floatingText;

        [Space]
        [Header("애니메이션 지연 및 타이밍 설정")]
        [SerializeField]
        [Tooltip("Activate 호출 후 애니메이션 시작 전 지연 시간(초)")]
        private float delay;

        [SerializeField]
        [Tooltip("애니메이션 완료 후 오브젝트 비활성화 전 추가 지연 시간(초)")]
        private float disableDelay;

        [Header("스케일 및 회전 초기 설정")]
        [SerializeField]
        [Tooltip("초기 스케일 배율 (defaultScale 대비)")]
        private float startScale;

        [SerializeField]
        [Tooltip("회전 애니메이션 진행 시간(초)")]
        private float time;

        [SerializeField]
        [Tooltip("회전 애니메이션에 적용할 이징 타입")]
        private Ease.Type easing;

        [Space]
        [Header("스케일 애니메이션 설정")]
        [SerializeField]
        [Tooltip("스케일 애니메이션 진행 시간(초)")]
        private float scaleTime;

        [SerializeField]
        [Tooltip("스케일 애니메이션에 적용할 이징 타입")]
        private Ease.Type scaleEasing;

        // Awake 시점에 기본 스케일 값을 저장할 내부 변수
        private Vector3 defaultScale;

        /// <summary>
        /// Awake: 초기화 시 기본 스케일을 저장합니다.
        /// </summary>
        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        /// <summary>
        /// Activate: 텍스트 내용, 색상, 초기 스케일/회전 설정 후 애니메이션 및 지연 콜백을 실행합니다.
        /// </summary>
        /// <param name="text">표시할 문자열</param>
        /// <param name="scaleMultiplier">스케일 배율 (기본 스케일 대비)</param>
        /// <param name="color">텍스트 색상</param>
        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            // 텍스트 내용 및 색상 설정
            floatingText.text = text;
            floatingText.color = color;

            // 랜덤 방향 설정 (시계/반시계 회전)
            int sign = Random.value >= 0.5f ? 1 : -1;

            // 초기 스케일 및 회전 설정
            transform.localScale = defaultScale * startScale * scaleMultiplier;
            transform.localRotation = Quaternion.Euler(70, 0, 18 * sign);

            // 지연 호출 후 회전 및 스케일 애니메이션 실행
            Tween.DelayedCall(delay, delegate
            {
                // 회전 애니메이션: z축 회전 보정 후 비활성화
                transform.DOLocalRotate(Quaternion.Euler(70, 0, 0), time)
                         .SetEasing(easing)
                         .OnComplete(delegate
                {
                    // 비활성화 지연 후 오브젝트 비활성화 및 완료 이벤트 호출
                    Tween.DelayedCall(disableDelay, delegate
                    {
                        gameObject.SetActive(false);
                        InvokeCompleteEvent();
                    });
                });

                // 스케일 애니메이션: 기본 스케일 복원
                transform.DOScale(defaultScale, scaleTime).SetEasing(scaleEasing);
            });
        }
    }
}
