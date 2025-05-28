/*
 * MenuStoreButton.cs
 * ---------------------
 * 이 스크립트는 메인 메뉴 화면에서 '상점(Store)' 패널로 이동하는 버튼의 동작을 정의합니다.
 * MenuPanelButton을 상속받아 기본적인 메뉴 버튼 기능을 활용하며,
 * 상점 패널에서 무료 코인 등 확인 가능한 항목이 있을 경우 버튼에 하이라이트 효과를 표시하고,
 * 수익화(Monetization) 기능 활성화 여부에 따라 버튼 자체의 활성 상태를 결정하며,
 * 버튼 클릭 시 상점 패널 UI를 표시하는 로직을 구현합니다.
 */

using Watermelon.IAPStore; // 인앱 결제 스토어 관련 네임스페이스
using UnityEngine;
using Watermelon; // Watermelon 프레임워크 네임스페이스 (UIController, Monetization 등)

namespace Watermelon.SquadShooter
{
    // 상점 메뉴 버튼 클래스 정의, MenuPanelButton 상속
    // sealed 키워드는 이 클래스가 더 이상 상속될 수 없음을 나타냅니다.
    public sealed class MenuStoreButton : MenuPanelButton
    {
        [Tooltip("상점 패널 UI(UIStore) 컴포넌트 참조")]
        private UIStore storePanel; // UIStore는 상점 화면 전체 UI

        /// <summary>
        /// 버튼 초기화 시 호출됩니다. (MenuPanelButton 오버라이드)
        /// </summary>
        public override void Init()
        {
            base.Init(); // 부모 클래스의 Init 호출

            // UI 컨트롤러를 통해 상점 패널 UI 컴포넌트 가져오기
            storePanel = UIController.GetPage<UIStore>();
        }

        /// <summary>
        /// 이 버튼이 활성화되어야 하는지 여부를 반환합니다. (MenuPanelButton 오버라이드)
        /// 수익화 기능이 활성화된 경우에만 버튼이 활성화됩니다.
        /// </summary>
        /// <returns>수익화 기능이 활성화되었으면 true</returns>
        public override bool IsActive()
        {
            // Watermelon 프레임워크의 Monetization 클래스를 통해 수익화 기능 활성 여부 확인
            return Monetization.IsActive;
        }

        /// <summary>
        /// 이 버튼에 하이라이트(알림) 표시가 필요한지 여부를 결정합니다. (MenuPanelButton 오버라이드)
        /// 상점 패널에 무료 코인 획득이 가능한 경우 하이라이트를 표시합니다.
        /// </summary>
        /// <returns>무료 코인 획득이 가능하면 true</returns>
        protected override bool IsHighlightRequired()
        {
            // 상점 패널 참조가 유효하고
            if (storePanel != null)
                // 상점 패널의 IsFreeCoinsAvailable() 메서드를 호출하여 무료 코인 가능 여부 확인
                return storePanel.IsFreeCoinsAvailable();

            // 상점 패널 참조가 없으면 하이라이트 불필요
            return false;
        }

        /// <summary>
        /// 버튼이 클릭되었을 때 호출되는 로직입니다. (MenuPanelButton 오버라이드)
        /// 메인 메뉴를 숨기고 상점 패널을 표시합니다.
        /// </summary>
        protected override void OnButtonClicked()
        {
            // 현재 활성화된 메인 메뉴 UI(UIMainMenu)를 숨기고,
            // 숨겨진 후 콜백 함수로 상점 패널 UI(UIStore)를 표시합니다.
            UIController.HidePage<UIMainMenu>(() =>
            {
                UIController.ShowPage<UIStore>();
            });

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}