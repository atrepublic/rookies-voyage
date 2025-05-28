//====================================================================================================
// 해당 스크립트: UIMainMenuButton.cs
// 기능: 메인 메뉴 버튼의 UI 애니메이션(표시/숨기기)을 관리하는 클래스입니다.
// 용도: 버튼이 화면 안팎으로 움직이는 애니메이션을 처리하여 메뉴 전환 시 시각적 효과를 제공합니다.
//====================================================================================================
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UIMainMenuButton
    {
        [Tooltip("애니메이션을 적용할 UI 요소의 RectTransform입니다.")]
        [SerializeField] private RectTransform rect;

        [Tooltip("버튼이 나타날 때(Show) 사용되는 애니메이션 커브입니다.")]
        [SerializeField] private AnimationCurve showStoreAdButtonsCurve;
        [Tooltip("버튼이 숨겨질 때(Hide) 사용되는 애니메이션 커브입니다.")]
        [SerializeField] private AnimationCurve hideStoreAdButtonsCurve;
        [Tooltip("버튼 표시/숨기기 애니메이션의 지속 시간입니다.")]
        [SerializeField] private float showHideDuration;

        private float savedRectPosX; // 버튼의 원래 X축 위치
        private float rectXPosBehindOfTheScreen; // 화면 밖으로 이동했을 때의 X축 위치

        private TweenCase showHideCase; // 버튼 표시/숨기기 애니메이션 트윈 케이스

        /// <summary>
        /// UI 메인 메뉴 버튼을 초기화하는 함수입니다.
        /// 버튼이 화면 밖으로 이동할 위치와 원래 위치를 설정합니다.
        /// </summary>
        /// <param name="rectXPosBehindOfTheScreen">버튼이 화면 밖으로 이동할 때의 X축 위치</param>
        public void Init(float rectXPosBehindOfTheScreen)
        {
            this.rectXPosBehindOfTheScreen = rectXPosBehindOfTheScreen; // 화면 밖 X축 위치 저장
            savedRectPosX = rect.anchoredPosition.x; // 버튼의 원래 X축 위치 저장
        }

        /// <summary>
        /// 버튼을 화면에 표시하는 함수입니다.
        /// 즉시 표시하거나 애니메이션을 통해 표시할 수 있습니다.
        /// </summary>
        /// <param name="immediately">true이면 즉시 표시, false이면 애니메이션 사용</param>
        public void Show(bool immediately = false)
        {
            // 현재 애니메이션이 실행 중이면 중복 실행 방지
            if (showHideCase != null && showHideCase.IsActive) return;

            // 즉시 표시 옵션 처리
            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX); // 원래 위치로 즉시 이동
                return;
            }

            // 애니메이션 시작 전 위치 초기화 (화면 밖으로 이동)
            rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);

            // 원래 위치로 이동하는 애니메이션 시작 (설정된 애니메이션 커브 사용)
            showHideCase = rect.DOAnchoredPosition(rect.anchoredPosition.SetX(savedRectPosX), showHideDuration).SetCurveEasing(showStoreAdButtonsCurve);
        }

        /// <summary>
        /// 버튼을 화면 밖으로 숨기는 함수입니다.
        /// 즉시 숨기거나 애니메이션을 통해 숨길 수 있습니다.
        /// </summary>
        /// <param name="immediately">true이면 즉시 숨기기, false이면 애니메이션 사용</param>
        public void Hide(bool immediately = false)
        {
            // 현재 애니메이션이 실행 중이면 중복 실행 방지
            if (showHideCase != null && showHideCase.IsActive) return;

            // 즉시 숨기기 옵션 처리
            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen); // 화면 밖으로 즉시 이동
                return;
            }

            // 애니메이션 시작 전 위치 초기화 (원래 위치로 이동)
            rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);

            // 화면 밖 위치로 이동하는 애니메이션 시작 (설정된 애니메이션 커브 사용)
            showHideCase = rect.DOAnchoredPosition(rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen), showHideDuration).SetCurveEasing(hideStoreAdButtonsCurve);
        }
    }
}