//====================================================================================================
// 해당 스크립트: DroppedCardPanel.cs
// 기능: 드롭된 카드 아이템 패널을 관리하고 표시합니다.
// 용도: 플레이어가 아이템(무기 카드)을 획득했을 때 해당 아이템의 정보(이름, 이미지, 희귀도, 카드 개수)를
//      표시하고, 필요 시 무기를 장착할 수 있는 기능을 제공합니다.
//====================================================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class DroppedCardPanel : MonoBehaviour
    {
        private const string CARDS_TEXT = "{0}/{1}"; // 현재 카드 개수와 다음 업그레이드에 필요한 카드 개수를 표시하기 위한 형식 문자열

        [Tooltip("무기 이름을 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI titleText;
        [Tooltip("무기 미리보기 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] private Image weaponPreviewImage;
        [Tooltip("무기 배경 이미지를 표시하는 Image 컴포넌트입니다. 희귀도에 따라 색상이 변경됩니다.")]
        [SerializeField] private Image weaponBackgroundImage;
        [Tooltip("무기 희귀도 텍스트를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI rarityText;

        [Tooltip("새로운 무기임을 나타내는 리본 게임 오브젝트입니다.")]
        [SerializeField] private GameObject newRibbonObject;

        [Space]
        [Tooltip("카드 진행 상태를 표시하는 패널 게임 오브젝트입니다.")]
        [SerializeField] private GameObject progressPanelObject;
        [Tooltip("카드 진행 상태를 채우는 막대 게임 오브젝트입니다.")]
        [SerializeField] private GameObject progressFillbarObject;
        [Tooltip("카드 진행 상태를 시각적으로 채우는 SlicedFilledImage 컴포넌트입니다.")]
        [SerializeField] private SlicedFilledImage progressFillbarImage;
        [Tooltip("카드 진행 상태 텍스트 (현재/필요 카드 개수)를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI progressFillbarText;
        [Tooltip("무기 장착 버튼 게임 오브젝트입니다. 카드가 충분하면 활성화됩니다.")]
        [SerializeField] private GameObject progressEquipButtonObject;
        [Tooltip("무기 장착 버튼의 텍스트를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI progressEquipButtonText;
        [Tooltip("무기가 장착되었음을 나타내는 게임 오브젝트입니다.")]
        [SerializeField] private GameObject progressEquipedObject;

        private CanvasGroup canvasGroup; // 패널의 투명도 및 상호작용을 제어하는 CanvasGroup 컴포넌트
        /// <summary>
        /// 패널의 CanvasGroup 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public CanvasGroup CanvasGroup => canvasGroup;

        private WeaponData weapon; // 현재 드롭된 무기 데이터
        private RarityData rarityData; // 현재 무기의 희귀도 데이터

        private int currentCardsAmount; // 현재 보유한 무기 카드 개수

        /// <summary>
        /// 드롭된 카드 패널을 초기화하는 함수입니다.
        /// 제공된 무기 데이터를 기반으로 패널의 UI 요소를 설정합니다.
        /// </summary>
        /// <param name="weapon">초기화에 사용할 무기 데이터</param>
        public void Init(WeaponData weapon)
        {
            this.weapon = weapon;

            // CanvasGroup 컴포넌트 가져오기
            canvasGroup = GetComponent<CanvasGroup>();

            // 무기 희귀도 데이터 가져오기
            rarityData = WeaponsController.GetRarityData(weapon.Rarity);

            // 현재 무기 카드 개수 설정
            currentCardsAmount = weapon.CardsAmount;

            // UI 요소에 무기 정보 표시
            titleText.text = weapon.WeaponName; // 무기 이름 설정

            weaponPreviewImage.sprite = weapon.Icon; // 무기 아이콘 설정

            rarityText.text = rarityData.Name; // 희귀도 이름 설정
            rarityText.color = rarityData.TextColor; // 희귀도 텍스트 색상 설정

            weaponBackgroundImage.color = rarityData.MainColor; // 무기 배경 색상 설정

            // 진행 패널 초기 비활성화
            progressPanelObject.SetActive(false);
        }

        /// <summary>
        /// 드롭된 카드 패널이 화면에 표시될 때 호출되는 함수입니다.
        /// 카드 진행 상태를 애니메이션과 함께 표시합니다.
        /// </summary>
        public void OnDisplayed()
        {
            // 다음 업그레이드에 필요한 카드 개수 가져오기
            int target = weapon.Upgrades[1].Price;

            // 진행 패널 및 진행 막대 활성화
            progressPanelObject.SetActive(true);
            progressFillbarObject.SetActive(true);

            // 장착 버튼 및 장착 완료 상태 초기 비활성화
            progressEquipButtonObject.SetActive(false);
            progressEquipedObject.SetActive(false);

            // 진행 막대 텍스트 업데이트 (현재/필요 카드 개수)
            progressFillbarText.text = string.Format(CARDS_TEXT, currentCardsAmount, target);

            // 진행 패널 크기 애니메이션
            progressPanelObject.transform.localScale = Vector3.one * 0.8f;
            progressPanelObject.transform.DOScale(Vector3.one, 0.15f).SetEasing(Ease.Type.BackOut);

            // 진행 막대 채우기 애니메이션
            progressFillbarImage.fillAmount = 0.0f;
            progressFillbarImage.DOFillAmount((float)currentCardsAmount / target, 0.4f, 0.1f).OnComplete(delegate
            {
                // 필요한 카드 개수를 충족했을 경우
                if (currentCardsAmount >= target)
                {
                    // 딜레이 후 장착 버튼 활성화 및 애니메이션
                    Tween.DelayedCall(0.5f, delegate
                    {
                        progressFillbarObject.SetActive(false); // 진행 막대 비활성화

                        progressEquipButtonObject.SetActive(true); // 장착 버튼 활성화
                        progressEquipButtonObject.transform.localScale = Vector3.one * 0.7f;
                        progressEquipButtonObject.transform.DOScale(Vector3.one, 0.25f).SetEasing(Ease.Type.BackOut);

                        // 입력 타입에 따라 장착 버튼 텍스트 설정
                        if(Control.InputType == InputType.Gamepad)
                        {
                            progressEquipButtonText.text = "UNLOCKED";
                        } else
                        {
                            progressEquipButtonText.text = "EQUIP";
                        }

                        // 입력 방식 변경 이벤트 구독
                        Control.OnInputChanged += OnInputChanged;
                    });
                }
            });
        }

        /// <summary>
        /// 입력 방식이 변경되었을 때 호출되는 함수입니다.
        /// 변경된 입력 방식에 따라 장착 버튼의 텍스트를 업데이트합니다.
        /// </summary>
        /// <param name="input">변경될 입력 방식</param>
        private void OnInputChanged(InputType input)
        {
            // 변경된 입력 방식에 따라 장착 버튼 텍스트 업데이트
            if (Control.InputType == InputType.Gamepad)
            {
                progressEquipButtonText.text = "UNLOCKED";
            }
            else
            {
                progressEquipButtonText.text = "EQUIP";
            }
        }

        /// <summary>
        /// 게임 오브젝트가 비활성화될 때 호출되는 함수입니다.
        /// 입력 방식 변경 이벤트 구독을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            // 입력 방식 변경 이벤트 구독 해제
            Control.OnInputChanged -= OnInputChanged;
        }

        /// <summary>
        /// 장착 버튼 클릭 시 호출되는 함수입니다.
        /// 해당 무기를 장착하고 UI 상태를 업데이트합니다.
        /// </summary>
        public void OnEquipButtonClicked()
        {
            // 무기 장착
            WeaponsController.SelectWeapon(weapon);

            // 장착 버튼 비활성화 및 장착 완료 상태 활성화
            progressEquipButtonObject.SetActive(false);
            progressEquipedObject.SetActive(true);

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}