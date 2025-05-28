// FloatingTextBehavior.cs
// 이 스크립트는 화면 상에 부동 텍스트를 표시하고, 확대 및 이동 애니메이션을 제어하여
// 텍스트가 나타난 후 자동으로 사라지도록 관리합니다.

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextBehavior : FloatingTextBaseBehavior
    {
        [Space]
        [SerializeField]
        [Tooltip("텍스트가 이동할 방향 오프셋 (월드 좌표 기준)")]
        private Vector3 offset;

        [SerializeField]
        [Tooltip("텍스트가 이동하여 사라지기까지 걸리는 시간 (초)")]
        private float time;

        [SerializeField]
        [Tooltip("이동 시 적용할 이징 함수 타입")]
        private Ease.Type easing;

        [Space]
        [SerializeField]
        [Tooltip("텍스트 스케일 확대/축소 애니메이션에 걸리는 시간 (초)")]
        private float scaleTime;

        [SerializeField]
        [Tooltip("스케일 애니메이션 곡선을 정의하는 AnimationCurve")]
        private AnimationCurve scaleAnimationCurve;

        // 기본 스케일 값을 저장하는 내부 변수
        private Vector3 defaultScale;

        // 스케일 애니메이션을 제어하는 TweenCase 참조
        private TweenCase scaleTween;

        // 이동 애니메이션을 제어하는 TweenCase 참조
        private TweenCase moveTween;

        /// <summary>
        /// Awake: 초기화 시점에 기본 스케일 값을 저장합니다.
        /// </summary>
        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        /// <summary>
        /// Activate: 텍스트, 색상, 스케일, 이동 애니메이션을 설정 및 실행합니다.
        /// </summary>
        /// <param name="text">표시할 문자열</param>
        /// <param name="scaleMultiplier">스케일 배율 (기본 스케일 대비)</param>
        /// <param name="color">텍스트 색상</param>
        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            // 텍스트 내용 및 색상 설정
            textRef.text = text;
            textRef.color = color;

            // 스케일 초기화 (0에서 시작)
            transform.localScale = Vector3.zero;

            // 스케일 트윈 애니메이션 실행
            scaleTween = transform.DOScale(defaultScale * scaleMultiplier, scaleTime)
                                   .SetCurveEasing(scaleAnimationCurve);

            // 위치 이동 애니메이션 실행 및 완료 시 비활성화
            moveTween = transform.DOMove(transform.position + offset, time)
                                   .SetEasing(easing)
                                   .OnComplete(delegate
            {
                gameObject.SetActive(false);
                // 완료 이벤트 호출
                InvokeCompleteEvent();
            });
        }

        /// <summary>
        /// AddOnTimeReached: 이동 애니메이션 중 특정 시간에 콜백을 추가합니다.
        /// </summary>
        /// <param name="time">애니메이션 시작 후 경과 시간 (초)</param>
        /// <param name="callback">실행할 콜백 함수</param>
        public void AddOnTimeReached(float time, SimpleCallback callback)
        {
            if (moveTween.ExistsAndActive())
            {
                moveTween.OnTimeReached(time, callback);
            }
        }

        /// <summary>
        /// SetText: 텍스트 내용을 변경합니다.
        /// </summary>
        /// <param name="text">새로 설정할 문자열</param>
        public void SetText(string text)
        {
            textRef.text = text;
        }

        /// <summary>
        /// Reset: 활성화된 애니메이션 트윈을 모두 종료하고 초기 상태로 복귀합니다.
        /// </summary>
        public void Reset()
        {
            scaleTween.KillActive();
            moveTween.KillActive();
        }
    }
}
