//====================================================================================================
// 해당 스크립트: MenuWeaponButton.cs
// 기능: 메인 메뉴에서 무기 업그레이드 페이지로 이동하는 버튼의 동작을 관리합니다.
// 용도: MenuPanelButton 추상 클래스를 상속받아 무기 버튼의 초기화, 강조 표시 필요 여부 확인,
//      그리고 버튼 클릭 시 무기 업그레이드 페이지로 전환하는 기능을 구현합니다.
//====================================================================================================
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // MenuPanelButton을 상속받는 무기 메뉴 버튼 클래스
    public sealed class MenuWeaponButton : MenuPanelButton
    {
        private UIWeaponPage weaponPage; // 무기 업그레이드 페이지 참조

        /// <summary>
        /// 무기 메뉴 버튼을 초기화하는 함수입니다.
        /// 기본 초기화 후 UIWeaponPage 페이지 참조를 가져옵니다.
        /// </summary>
        public override void Init()
        {
            base.Init(); // 부모 클래스의 Init 호출

            // UIController에서 UIWeaponPage 페이지 참조 가져오기
            weaponPage = UIController.GetPage<UIWeaponPage>();
        }

        /// <summary>
        /// 이 버튼에 강조 표시가 필요한지 확인하는 함수입니다.
        /// 무기 업그레이드 페이지에 업그레이드 가능한 무기가 있는지 여부를 확인합니다.
        /// </summary>
        /// <returns>강조 표시가 필요하면 true, 아니면 false를 반환합니다.</returns>
        protected override bool IsHighlightRequired()
        {
            // 무기 페이지에 업그레이드 가능한 무기가 있는지 확인하여 강조 표시 여부 결정
            return weaponPage.IsAnyActionAvailable();
        }

        /// <summary>
        /// 무기 메뉴 버튼 클릭 시 호출되는 함수입니다.
        /// 메인 메뉴 페이지를 숨기고 무기 업그레이드 페이지를 표시합니다.
        /// </summary>
        protected override void OnButtonClicked()
        {
            // 메인 메뉴 페이지 숨김 및 완료 시 무기 업그레이드 페이지 표시
            UIController.HidePage<UIMainMenu>(() =>
            {
                UIController.ShowPage<UIWeaponPage>();
            });

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}