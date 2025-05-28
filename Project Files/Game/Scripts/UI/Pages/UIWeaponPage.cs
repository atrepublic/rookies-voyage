//====================================================================================================
// 해당 스크립트: UIWeaponPage.cs
// 기능: 무기 업그레이드 패널 UI를 관리하고 표시합니다.
// 용도: 플레이어가 보유한 무기 목록을 보여주고, 각 무기의 상태(잠금 해제, 업그레이드 가능 여부)를 표시하며,
//      무기 선택 및 업그레이드 기능을 제공합니다. UIUpgradesAbstractPage를 상속받아 공통 기능을 활용합니다.
//====================================================================================================
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 무기 패널 UI (WeaponPanelUI)와 무기 데이터 (WeaponData)를 사용하여 UIUpgradesAbstractPage를 상속
    public class UIWeaponPage : UIUpgradesAbstractPage<WeaponPanelUI, WeaponData>
    {
        /// <summary>
        /// 현재 선택된 무기의 인덱스를 가져오는 프로퍼티입니다.
        /// WeaponsController에서 현재 선택된 무기 인덱스를 안전하게 반환합니다.
        /// </summary>
        protected override int SelectedIndex => Mathf.Clamp(WeaponsController.SelectedWeaponIndex, 0, int.MaxValue);

        /// <summary>
        /// 모든 무기 패널 UI를 업데이트하는 함수입니다.
        /// </summary>
        public void UpdateUI() => itemPanels.ForEach(panel => panel.UpdateUI());

        /// <summary>
        /// 특정 무기 데이터에 해당하는 무기 패널 UI를 가져오는 함수입니다.
        /// </summary>
        /// <param name="weapon">찾으려는 무기 데이터</param>
        /// <returns>해당 무기 데이터와 일치하는 WeaponPanelUI 객체, 없으면 null 반환</returns>
        public override WeaponPanelUI GetPanel(WeaponData weapon)
        {
            // 모든 아이템 패널을 순회하며 해당 무기 데이터와 일치하는 패널 찾기
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].Data == weapon)
                    return itemPanels[i]; // 일치하는 패널 찾으면 반환
            }

            return null; // 일치하는 패널이 없으면 null 반환
        }

        /// <summary>
        /// 현재 무기 패널들 중에 플레이어가 수행할 수 있는 액션(업그레이드)이 있는지 확인하는 함수입니다.
        /// </summary>
        /// <returns>수행 가능한 업그레이드 액션이 하나라도 있으면 true, 없으면 false를 반환합니다.</returns>
        public bool IsAnyActionAvailable()
        {
            // 모든 아이템 패널을 순회하며 다음 업그레이드 가능 여부 확인
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].IsNextUpgradeCanBePurchased())
                    return true; // 업그레이드 가능하면 true 반환
            }

            return false; // 수행 가능한 액션이 없으면 false 반환
        }

        /// <summary>
        /// 이 페이지에서 사용되는 게임패드 버튼 태그를 활성화하는 함수입니다.
        /// 무기 페이지 관련 게임패드 버튼 태그를 설정합니다.
        /// </summary>
        protected override void EnableGamepadButtonTag()
        {
            // UI 게임패드 버튼의 Weapons 태그 활성화
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Weapons);
        }

        #region UI Page

        /// <summary>
        /// UI 페이지 초기화 함수입니다.
        /// 기본 초기화 후 모든 무기에 대한 패널을 생성하고 초기화하며,
        /// 무기 잠금 해제 및 업그레이드 이벤트를 구독합니다.
        /// </summary>
        public override void Init()
        {
            base.Init(); // 부모 클래스의 Init 호출

            // 모든 무기에 대해 무기 패널 생성 및 초기화
            for (int i = 0; i < WeaponsController.Weapons.Length; i++)
            {
                WeaponData weapon = WeaponsController.Weapons[i]; // 현재 무기 데이터 가져오기

                WeaponPanelUI newPanel = AddNewPanel(); // 새로운 무기 패널 추가
                newPanel.Init(weapon, i); // 무기 데이터와 인덱스와 함께 패널 초기화
            }

            // 무기 잠금 해제 및 업그레이드 이벤트 구독
            WeaponsController.WeaponUnlocked += OnWeaponUnlocked;
            WeaponsController.WeaponUpgraded += UpdateUI; // 무기 업그레이드 시 UI 업데이트 함수 연결
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 기본 표시 애니메이션을 실행하고 UI를 업데이트합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            base.PlayShowAnimation(); // 부모 클래스의 표시 애니메이션 호출

            UpdateUI(); // UI 업데이트 함수 호출
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// 기본 숨김 애니메이션을 실행하고 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation(); // 부모 클래스의 숨김 애니메이션 호출

            UIController.OnPageClosed(this); // UI 컨트롤러에 페이지 닫힘 이벤트 알림
        }

        /// <summary>
        /// UI 페이지를 숨기는 함수입니다.
        /// UIController를 통해 이 페이지를 숨깁니다.
        /// </summary>
        /// <param name="onFinish">숨김 애니메이션 완료 시 호출될 콜백 함수</param>
        protected override void HidePage(SimpleCallback onFinish)
        {
            // UIController를 사용하여 UIWeaponPage 페이지 숨김
            UIController.HidePage<UIWeaponPage>(onFinish);
        }

        /// <summary>
        /// 무기 잠금 해제 시 호출되는 함수입니다.
        /// UI를 업데이트합니다.
        /// </summary>
        /// <param name="weapon">잠금 해제된 무기 데이터</param>
        private void OnWeaponUnlocked(WeaponData weapon)
        {
            UpdateUI(); // UI 업데이트 함수 호출
        }

        /// <summary>
        /// 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// 무기 잠금 해제 및 업그레이드 이벤트 구독을 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 무기 잠금 해제 및 업그레이드 이벤트 구독 해제
            WeaponsController.WeaponUnlocked -= OnWeaponUnlocked;
            WeaponsController.WeaponUpgraded -= UpdateUI;
        }
        #endregion
    }
}