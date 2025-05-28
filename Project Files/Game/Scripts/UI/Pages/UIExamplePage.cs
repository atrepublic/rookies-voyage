//====================================================================================================
// 해당 스크립트: UIExamplePage.cs
// 기능: UI 페이지 시스템의 예시로 사용되는 스크립트입니다.
// 용도: 기본적인 UI 페이지의 초기화, 표시, 숨김 애니메이션 및 버튼 이벤트를 구현하는 방법을 보여줍니다.
//====================================================================================================
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIExamplePage : UIPage
    {
        [Tooltip("배경 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] private Image backgroundImage;

        private RectTransform pageRectTransform; // 페이지의 RectTransform
        private Color defaultBackgroundColor; // 배경 이미지의 기본 색상

        private UIGame gamePage; // UIGame 페이지 참조

        /// <summary>
        /// UI 페이지 초기화 함수입니다.
        /// 필요한 컴포넌트를 캐싱하고 다른 페이지의 참조를 가져옵니다.
        /// </summary>
        public override void Init()
        {
            // 변수 캐싱
            pageRectTransform = (RectTransform)transform; // 현재 오브젝트의 RectTransform 가져오기
            defaultBackgroundColor = backgroundImage.color; // 배경 이미지의 기본 색상 저장

            // 또는 다른 컴포넌트에서 가져오기
            gamePage = UIController.GetPage<UIGame>(); // UIGame 페이지 참조 가져오기
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 배경 이미지의 페이드 인 애니메이션을 처리하고 완료 시 페이지 열림 이벤트를 호출합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            // 컴포넌트 리셋
            backgroundImage.color = defaultBackgroundColor.SetAlpha(0.0f); // 배경 이미지 투명도 0으로 설정

            // 애니메이션 재생
            backgroundImage.DOColor(defaultBackgroundColor, 0.3f).OnComplete(delegate
            {
                // 중요: UIController 이벤트의 올바른 작동을 위해 페이지 애니메이션 완료 직후 UIController.OnPageOpened(this) 메서드를 호출해야 합니다.
                UIController.OnPageOpened(this);
            });
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// 배경 이미지의 페이드 아웃 애니메이션을 처리하고 완료 시 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            // 애니메이션 재생
            backgroundImage.DOFade(0.0f, 0.3f).OnComplete(delegate
            {
                // 중요: UIController 이벤트의 올바른 작동을 위해 페이지 애니메이션 완료 직후 UIController.OnPageClosed(this) 메서드를 호출해야 합니다.
                UIController.OnPageClosed(this);
            });
        }

        #region Buttons
        /// <summary>
        /// 닫기 버튼 클릭 시 호출되는 함수입니다.
        /// UIController를 통해 이 페이지를 숨깁니다.
        /// </summary>
        public void OnCloseButtonClicked()
        {
            // 또는 UIController 정적 메서드를 사용하고 OnPageHidded 콜백을 추가할 수 있습니다.
            UIController.HidePage<UIExamplePage>(() =>
            {
                // UIExamplePage 페이지가 숨겨졌을 때 호출됩니다.
            });
        }
        #endregion
    }
}