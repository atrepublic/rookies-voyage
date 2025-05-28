using TMPro;
using UnityEngine;

namespace Watermelon
{
    public abstract class FloatingTextBaseBehavior : MonoBehaviour
    {
        [SerializeField] protected TMP_Text textRef;

        public SimpleCallback OnAnimationCompleted;

        /// <summary>
        /// 치명타 여부를 제외한 기본 텍스트 출력 함수
        /// </summary>
        public virtual void Activate(string text, float scaleMultiplier, Color color)
        {
            // [기본 처리] 치명타 여부가 false로 간주됨
            Activate(text, scaleMultiplier, color, false);
        }

        /// <summary>
        /// 치명타 여부를 포함한 텍스트 출력 함수 (색상 및 크기 강조 포함)
        /// </summary>
        public virtual void Activate(string text, float scaleMultiplier, Color color, bool isCritical)
        {
            textRef.text = text;
            textRef.color = color;

            // [치명타 강조 효과] 텍스트 크기를 1.5배로 확대
            transform.localScale = isCritical ? Vector3.one * 1.5f : Vector3.one * scaleMultiplier;

            InvokeCompleteEvent();
        }

        /// <summary>
        /// 텍스트 애니메이션 완료 시 호출할 콜백
        /// </summary>
        protected void InvokeCompleteEvent()
        {
            OnAnimationCompleted?.Invoke();
        }
    }
}
