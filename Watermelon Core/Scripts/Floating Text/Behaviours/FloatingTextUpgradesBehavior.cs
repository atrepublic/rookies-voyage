// FloatingTextUpgradesBehavior.cs
// 이 스크립트는 업그레이드 시 표시되는 부동 텍스트 및 아이콘을 관리하며,
// 지정한 대상(transform)에 고정하거나 위치 이동, 스케일 및 페이드 애니메이션을 실행하여
// UI 피드백을 제공합니다.

#pragma warning disable 0618

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class FloatingTextUpgradesBehavior : FloatingTextBaseBehavior
    {
        [Header("컨테이너 및 UI 컴포넌트")]
        [SerializeField]
        [Tooltip("부동 텍스트 및 아이콘을 포함하는 컨테이너 Transform 참조")]
        private Transform containerTransform;

        [SerializeField]
        [Tooltip("컨테이너의 CanvasGroup 컴포넌트 (페이드 애니메이션에 사용)")]
        private CanvasGroup containerCanvasGroup;

        [SerializeField]
        [Tooltip("업그레이드 아이콘을 표시할 Image 컴포넌트")]        
        private Image iconImage;

        [SerializeField]
        [Tooltip("표시할 부동 텍스트(Text 컴포넌트)")]
        private Text floatingText;

        [Space, Header("이동 애니메이션 설정")]
        [SerializeField]
        [Tooltip("컨테이너의 이동 오프셋(로컬 좌표 기준)")]
        private Vector3 offset;

        [SerializeField]
        [Tooltip("이동 애니메이션 실행 시간(초)")]
        private float time;

        [SerializeField]
        [Tooltip("이징 타입 (이동 애니메이션)")]
        private Ease.Type easing;

        [Space, Header("스케일 애니메이션 설정")]
        [SerializeField]
        [Tooltip("스케일 애니메이션 실행 시간(초)")]
        private float scaleTime;

        [SerializeField]
        [Tooltip("스케일 애니메이션 커브 설정")]
        private AnimationCurve scaleAnimationCurve;

        [Space, Header("페이드 애니메이션 설정")]
        [SerializeField]
        [Tooltip("페이드 아웃 애니메이션 실행 시간(초)")]
        private float fadeTime;

        [SerializeField]
        [Tooltip("이징 타입 (페이드 애니메이션)")]
        private Ease.Type fadeEasing;

        // FixToTarget 호출 시 대상 Transform
        private Transform targetTransform;
        // FixToTarget 호출 시 대상 오프셋
        private Vector3 targetOffset;
        // 대상 고정 여부 플래그
        private bool fixToTarget;

        /// <summary>
        /// LateUpdate: FixToTarget이 설정된 경우 매 프레임 대상 위치에 컨테이너를 고정합니다.
        /// </summary>
        private void LateUpdate()
        {
            if (fixToTarget)
                transform.position = targetTransform.position + targetOffset;
        }

        /// <summary>
        /// SetIconAndColor: 업그레이드 아이콘과 텍스트 색상을 설정합니다.
        /// </summary>
        /// <param name="icon">표시할 아이콘 스프라이트</param>
        /// <param name="color">아이콘 및 텍스트 색상</param>
        public void SetIconAndColor(Sprite icon, Color color)
        {
            iconImage.sprite = icon;
            iconImage.color = color;

            floatingText.color = color;
        }

        /// <summary>
        /// Activate: 텍스트와 아이콘을 컨테이너에 초기화하고,
        /// 스케일, 이동, 페이드 애니메이션을 순차적으로 실행합니다.
        /// 애니메이션 완료 시 오브젝트를 비활성화하고 부모 해제 후 완료 이벤트를 호출합니다.
        /// </summary>
        /// <param name="text">표시할 문자열</param>
        /// <param name="scaleMultiplier">스케일 배율 (기본 대비 곱 적용)</param>
        /// <param name="color">텍스트 및 아이콘 색상</param>
        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            // 고정 모드 해제
            fixToTarget = false;

            // 텍스트 설정
            floatingText.text = text;
            floatingText.color = color;

            // 페이드 초기화
            containerCanvasGroup.alpha = 1.0f;

            // 스케일 초기화 및 트윈 실행
            containerTransform.localScale = Vector3.zero;
            containerTransform.DOScale(Vector3.one * scaleMultiplier, scaleTime)
                .SetCurveEasing(scaleAnimationCurve);

            // 페이드 아웃 실행
            containerCanvasGroup.DOFade(0.0f, fadeTime)
                .SetEasing(fadeEasing);

            // 이동 초기화 및 트윈 실행
            containerTransform.localPosition = Vector3.zero;
            containerTransform.DOLocalMove(offset, time)
                .SetEasing(easing)
                .OnComplete(delegate
            {
                gameObject.SetActive(false);
                transform.SetParent(null);
                InvokeCompleteEvent();
            });
        }

        /// <summary>
        /// FixToTarget: 지정한 대상 Transform과 오프셋 값으로 고정 모드를 활성화합니다.
        /// </summary>
        /// <param name="target">고정할 대상 Transform</param>
        /// <param name="offset">대상 좌표 대비 오프셋</param>
        public void FixToTarget(Transform target, Vector3 offset)
        {
            fixToTarget = true;
            targetOffset = offset;
            targetTransform = target;
        }
    }
}
