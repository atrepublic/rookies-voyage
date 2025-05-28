// FollowingTextBehavior.cs
// 이 스크립트는 지정된 대상(Transform)을 따라다니는 부동 텍스트를 관리합니다.
// 텍스트의 스케일 애니메이션을 제어하고, 지정된 오프셋 위치에 지속적으로 위치를 갱신합니다.

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class FollowingTextBehavior : FloatingTextBaseBehavior
    {
        [Header("텍스트 컴포넌트 설정")]
        [SerializeField]
        [Tooltip("추적할 대상 위에 표시할 TMP_Text 컴포넌트 참조")]
        private TMP_Text floatingText;

        [Space, Header("스케일 애니메이션 설정")]
        [SerializeField]
        [Tooltip("스케일 애니메이션 실행 시간 (초)")]
        private float scaleTime;

        [SerializeField]
        [Tooltip("스케일 애니메이션에 사용할 곡선을 정의하는 AnimationCurve")]
        private AnimationCurve scaleAnimationCurve;

        // 원본 로컬 스케일 값을 저장하는 내부 변수
        private Vector3 defaultScale;

        // 따라다닐 대상 Transform
        private Transform followTransform;

        // 대상 좌표 대비 표시 오프셋
        private Vector3 followOffset;

        /// <summary>
        /// Awake: 초기화 시 원본 스케일 값을 저장합니다.
        /// </summary>
        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        /// <summary>
        /// Activate: 대상 Transform과 오프셋을 설정하고, 스케일 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="followTransform">따라다닐 대상 Transform</param>
        /// <param name="followOffset">대상 위치 대비 표시 오프셋</param>
        public void Activate(Transform followTransform, Vector3 followOffset)
        {
            this.followTransform = followTransform;
            this.followOffset = followOffset;

            // 시작 스케일을 0으로 초기화하고 확대 애니메이션 실행
            transform.localScale = Vector3.zero;
            transform.DOScale(defaultScale, scaleTime)
                     .SetCurveEasing(scaleAnimationCurve);
        }

        /// <summary>
        /// Update: 대상이 설정된 경우 매 프레임 대상 위치에 따라 텍스트 위치를 갱신합니다.
        /// </summary>
        private void Update()
        {
            if (followTransform == null)
                return;

            transform.position = followTransform.position + followOffset;
        }

        /// <summary>
        /// Unload: 대상 추적을 중단하고 텍스트 고정 모드를 해제합니다.
        /// </summary>
        public void Unload()
        {
            followTransform = null;
        }
    }
}
