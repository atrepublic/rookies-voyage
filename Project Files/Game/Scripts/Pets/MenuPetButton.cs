// MenuPetButton.cs
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.LevelSystem;  // for GameSettings if needed

namespace Watermelon.SquadShooter
{
    /// <summary>메인 메뉴 하단의 '펫' 탭 버튼</summary>
    public sealed class MenuPetButton : MenuPanelButton
    {
        private UIPetsPage petsPage;

        public override void Init()
        {
            base.Init();
            petsPage = UIController.GetPage<UIPetsPage>();
        }

        /// <summary>펫 페이지에 수행 가능한 액션이 있으면 하이라이트</summary>
        protected override bool IsHighlightRequired() =>
            petsPage != null && petsPage.IsAnyActionAvailable();

        protected override void OnButtonClicked()
        {
            UIController.HidePage<UIMainMenu>(() =>
                UIController.ShowPage<UIPetsPage>());
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}
