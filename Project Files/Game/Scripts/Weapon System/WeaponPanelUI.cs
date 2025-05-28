// 이 스크립트는 개별 무기 패널 UI를 관리하며 무기 정보 표시 및 강화 기능을 처리합니다.
// 무기 이름, 아이콘, 희귀도, 잠금/강화 상태, 강화 버튼 등을 업데이트합니다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class WeaponPanelUI : UIUpgradeAbstractPanel
    {
        [Tooltip("무기 이름을 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI weaponName;
        [Tooltip("무기 아이콘을 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] Image weaponImage;
        [Tooltip("무기 패널의 배경 이미지를 표시하는 Image 컴포넌트입니다 (주로 희귀도 색상 표시).")]
        [SerializeField] Image weaponBackImage;
        [Tooltip("무기 희귀도 텍스트를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI rarityText;

        [Header("잠금 상태")]
        [Tooltip("무기가 잠금 상태일 때 활성화되는 GameObject입니다.")]
        [SerializeField] GameObject lockedStateObject;
        [Tooltip("카드 수량에 따라 채워지는 이미지 컴포넌트입니다.")]
        [SerializeField] SlicedFilledImage cardsFillImage;
        [Tooltip("현재 보유한 카드 수량을 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI cardsAmountText;

        [Header("강화 상태")]
        [Tooltip("무기 레벨을 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI levelText;
        [Tooltip("무기가 강화 상태일 때 활성화되는 GameObject입니다.")]
        [SerializeField] GameObject upgradeStateObject;
        [Tooltip("강화 비용을 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI upgradePriceText;
        [Tooltip("강화에 사용되는 화폐 아이콘을 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] Image upgradeCurrencyImage;

        [Space]
        [Tooltip("강화 상태가 활성화되었을 때의 색상입니다.")]
        [SerializeField] Color upgradeStateActiveColor = Color.white;
        [Tooltip("강화 상태가 비활성화되었을 때의 색상입니다.")]
        [SerializeField] Color upgradeStateUnactiveColor = Color.white;
        [Tooltip("강화 상태를 시각적으로 나타내는 이미지 배열입니다.")]
        [SerializeField] Image[] upgradesStatesImages;

        // 이 패널이 나타내는 무기 데이터입니다.
        public WeaponData Data { get; private set; }

        [Space]
        [Tooltip("강화 구매 버튼 컴포넌트입니다.")]
        [SerializeField] Button upgradesBuyButton;
        [Tooltip("강화 구매 버튼의 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] Image upgradesBuyButtonImage;
        [Tooltip("강화 구매 버튼의 텍스트를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [Tooltip("강화 구매 버튼이 활성화 상태일 때의 Sprite입니다.")]
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [Tooltip("강화 구매 버튼이 비활성화 상태일 때의 Sprite입니다.")]
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;

        [Space]
        [Tooltip("무기가 최대 강화 레벨일 때 활성화되는 GameObject입니다.")]
        [SerializeField] GameObject upgradesMaxObject;

        // 무기가 잠금 해제되었는지 (최소 1레벨 이상인지) 여부를 반환합니다.
        public override bool IsUnlocked => Data.UpgradeLevel > 0;
        // 이 패널이 데이터베이스에서 몇 번째 무기를 나타내는지의 인덱스입니다.
        private int weaponIndex;
        public int WeaponIndex => weaponIndex;

        // 게임패드 조작을 위한 UIGamepadButton 컴포넌트입니다.
        private UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        // 강화 버튼의 Transform 컴포넌트입니다.
        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        /// <summary>
        /// 무기 패널 UI를 초기화합니다.
        /// </summary>
        /// <param name="data">표시할 무기 데이터</param>
        /// <param name="weaponIndex">무기 데이터베이스에서의 인덱스</param>
        public void Init(WeaponData data, int weaponIndex)
        {
            Data = data;
            // RectTransform 컴포넌트를 가져옵니다.
            panelRectTransform = (RectTransform)transform;
            // GamepadButton 컴포넌트를 가져옵니다.
            gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();

            // 무기 인덱스를 설정합니다.
            this.weaponIndex = weaponIndex;

            // 무기 데이터를 사용하여 UI 요소를 업데이트합니다.
            weaponName.text = data.WeaponName;
            weaponImage.sprite = data.Icon;
            weaponBackImage.color = data.RarityData.MainColor;
            rarityText.text = data.RarityData.Name;
            rarityText.color = data.RarityData.TextColor;

            // UI 상태를 업데이트합니다.
            UpdateUI();
            // 선택 상태를 업데이트합니다.
            UpdateSelectionState();

            // 무기 선택 변경 이벤트에 핸들러를 등록합니다.
            WeaponsController.NewWeaponSelected += UpdateSelectionState;
        }

        /// <summary>
        /// 다음 강화 레벨을 구매할 수 있는지 확인합니다.
        /// </summary>
        /// <returns>구매 가능하면 true, 아니면 false</returns>
        public bool IsNextUpgradeCanBePurchased()
        {
            // 무기가 잠금 해제된 상태인지 확인합니다.
            if (IsUnlocked)
            {
                // 무기가 최대 강화 레벨이 아닌지 확인합니다.
                if (!Data.IsMaxUpgrade())
                {
                    // 다음 강화 비용만큼 화폐를 가지고 있는지 확인합니다.
                    if (CurrencyController.HasAmount(CurrencyType.Coins, Data.GetNextUpgrade().Price))
                        return true;
                }
            }

            // 구매할 수 없는 경우 false를 반환합니다.
            return false;
        }

        /// <summary>
        /// 현재 무기 상태에 따라 UI를 업데이트합니다 (잠금 또는 강화 상태).
        /// </summary>
        public void UpdateUI()
        {
            // 무기가 잠금 해제되었는지 확인합니다.
            if (IsUnlocked)
            {
                // 잠금 해제 상태이면 강화 상태 UI를 업데이트합니다.
                UpdateUpgradeState();
            }
            else
            {
                // 잠금 상태이면 잠금 상태 UI를 업데이트합니다.
                UpdateLockedState();
            }
        }

        /// <summary>
        /// 무기 패널의 선택 상태에 따라 UI를 업데이트합니다.
        /// </summary>
        private void UpdateSelectionState()
        {
            // 현재 무기 인덱스가 선택된 무기 인덱스와 같은지 확인합니다.
            if (weaponIndex == WeaponsController.SelectedWeaponIndex)
            {
                // 선택된 상태이면 선택 표시 이미지를 활성화하고 배경 크기를 조절합니다.
                selectionImage.gameObject.SetActive(true);
                backgroundTransform.localScale = Vector3.one;
            }
            else
            {
                // 선택되지 않은 상태이면 선택 표시 이미지를 비활성화하고 배경 크기를 기본으로 설정합니다.
                selectionImage.gameObject.SetActive(false);
                backgroundTransform.localScale = Vector3.one;
            }

            // UI 전체를 업데이트합니다.
            UpdateUI();
        }

        /// <summary>
        /// 무기가 잠금 상태일 때의 UI를 업데이트합니다.
        /// </summary>
        private void UpdateLockedState()
        {
            // 잠금 상태 관련 UI 오브젝트를 활성화하고 강화 상태 UI 오브젝트를 비활성화합니다.
            lockedStateObject.SetActive(true);
            upgradeStateObject.SetActive(false);

            // 현재 카드 수량과 다음 강화에 필요한 목표 수량을 가져옵니다.
            int currentAmount = Data.CardsAmount;
            int target = Data.GetNextUpgrade().Price; // 다음 강화 비용이 카드 목표 수량으로 사용됩니다.

            // 카드 채우기 이미지의 fillAmount를 업데이트하여 진행 상태를 표시합니다.
            cardsFillImage.fillAmount = (float)currentAmount / target;
            // 현재 카드 수량과 목표 수량을 텍스트로 표시합니다.
            cardsAmountText.text = currentAmount + "/" + target;

            // 파워 관련 UI를 비활성화합니다.
            powerObject.SetActive(false);
            powerText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 무기가 강화 상태일 때의 UI를 업데이트합니다.
        /// </summary>
        private void UpdateUpgradeState()
        {
            // 잠금 상태 관련 UI 오브젝트를 비활성화하고 강화 상태 UI 오브젝트를 활성화합니다.
            lockedStateObject.SetActive(false);
            upgradeStateObject.SetActive(true);

            // 다음 강화 데이터를 가져옵니다.
            WeaponUpgrade nextUpgrade = Data.GetNextUpgrade();
            // 다음 강화 데이터가 있는지 확인합니다.
            if (nextUpgrade != null)
            {
                // 다음 강화 비용을 텍스트로 표시합니다.
                upgradePriceText.text = nextUpgrade.Price.ToString();
                // 다음 강화에 사용되는 화폐 아이콘을 업데이트합니다.
                upgradeCurrencyImage.sprite = CurrencyController.GetCurrency(nextUpgrade.CurrencyType).Icon;
            }
            else
            {
                // 최대 강화 레벨인 경우 "MAXED OUT" 텍스트를 표시하고 화폐 아이콘을 비활성화합니다.
                upgradePriceText.text = "MAXED OUT";
                upgradeCurrencyImage.gameObject.SetActive(false);
            }

            // 파워 관련 UI를 활성화하고 현재 무기 파워를 텍스트로 표시합니다.
            powerObject.SetActive(true);
            powerText.gameObject.SetActive(true);
            powerText.text = Data.GetCurrentUpgrade().Power.ToString();

            // 강화 요소들을 다시 그립니다.
            RedrawUpgradeElements();
        }

        /// <summary>
        /// 강화 상태의 다양한 UI 요소들을 다시 그립니다.
        /// </summary>
        private void RedrawUpgradeElements()
        {
            // 현재 무기 레벨을 텍스트로 표시합니다.
            levelText.text = "LEVEL " + Data.UpgradeLevel;

            // 무기가 최대 강화 레벨인지 확인합니다.
            if (!Data.IsMaxUpgrade())
            {
                // 최대 강화 레벨이 아니면 최대 강화 UI를 비활성화하고 강화 구매 버튼을 활성화합니다.
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);

                // 강화 구매 버튼을 다시 그립니다.
                RedrawUpgradeButton();
            }
            else
            {
                // 최대 강화 레벨이면 최대 강화 UI를 활성화하고 강화 구매 버튼을 비활성화합니다.
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);

                // 게임패드 버튼이 있다면 포커스를 해제합니다.
                if (gamepadButton != null)
                    gamepadButton.SetFocus(false);
            }
        }

        /// <summary>
        /// 강화 구매 버튼의 상태 (활성화/비활성화 Sprite 및 텍스트)를 다시 그립니다.
        /// </summary>
        protected override void RedrawUpgradeButton()
        {
            // 무기가 최대 강화 레벨이 아닌지 확인합니다.
            if (!Data.IsMaxUpgrade())
            {
                // 다음 강화 데이터를 가져옵니다.
                WeaponUpgrade nextUpgrade = Data.GetNextUpgrade();

                // 다음 강화 비용과 화폐 타입을 가져옵니다.
                int price = nextUpgrade.Price;
                CurrencyType currencyType = nextUpgrade.CurrencyType;

                // 강화 비용만큼 화폐를 가지고 있는지 확인합니다.
                if (CurrencyController.HasAmount(currencyType, price))
                {
                    // 화폐가 충분하면 활성화 Sprite로 변경합니다.
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;

                    // 게임패드 버튼이 있다면 현재 무기 패널이 선택된 상태에 따라 포커스를 설정합니다.
                    if (gamepadButton != null)
                        gamepadButton.SetFocus(weaponIndex == WeaponsController.SelectedWeaponIndex);
                }
                else
                {
                    // 화폐가 부족하면 비활성화 Sprite로 변경합니다.
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;

                    // 게임패드 버튼이 있다면 포커스를 해제합니다.
                    if (gamepadButton != null)
                        gamepadButton.SetFocus(false);
                }

                // 강화 비용을 포맷하여 텍스트로 표시합니다.
                upgradesBuyButtonText.text = CurrencyHelper.Format(price);

            }
        }

        /// <summary>
        /// 이 무기 패널을 선택 상태로 만듭니다.
        /// </summary>
        public override void Select()
        {
            // 무기가 잠금 해제된 상태인지 확인합니다.
            if (IsUnlocked)
            {
                // 현재 무기 인덱스가 이미 선택된 무기 인덱스와 다른지 확인합니다.
                if (weaponIndex != WeaponsController.SelectedWeaponIndex)
                {
                    // 버튼 클릭 사운드를 재생합니다.
                    AudioController.PlaySound(AudioController.AudioClips.buttonSound);

                    // 해당 무기를 선택된 무기로 설정합니다.
                    WeaponsController.SelectWeapon(weaponIndex);
                }
            }
        }

        /// <summary>
        /// 강화 구매 버튼 클릭 시 호출되는 함수입니다.
        /// </summary>
        public void UpgradeButton()
        {
            // 다음 강화 데이터를 가져옵니다.
            WeaponUpgrade nextUpgrade = Data.GetNextUpgrade();
            // 다음 강화 비용이 현재 보유한 화폐보다 적거나 같은지 확인합니다.
            if (nextUpgrade.Price <= CurrencyController.GetCurrency(nextUpgrade.CurrencyType).Amount)
            {
                // 무기 패널을 선택 상태로 만듭니다.
                Select();

                // 강화 비용만큼 화폐를 차감합니다.
                CurrencyController.Substract(nextUpgrade.CurrencyType, nextUpgrade.Price);

                // 무기를 강화합니다.
                Data.Upgrade();

                // 버튼 클릭 사운드를 재생합니다.
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            }
        }

        /// <summary>
        /// 이 오브젝트가 비활성화될 때 호출되는 함수입니다.
        /// </summary>
        private void OnDisable()
        {
            // 무기 선택 변경 이벤트 핸들러 등록을 해제합니다.
            WeaponsController.NewWeaponSelected -= UpdateSelectionState;
        }
    }
}