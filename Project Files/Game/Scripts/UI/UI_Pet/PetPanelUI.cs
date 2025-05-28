// PetPanelUI.cs v1.13
// ---------------------
// 로비의 펫 업그레이드·선택 패널을 제어하는 스크립트
// - UC_PetData 바인딩, 언락/업그레이드/선택 로직
// - 패널 클릭(mainButton)만으로 즉시 선택 처리
// - 화폐 변화 콜백으로 버튼 상태 자동 갱신

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Watermelon;                   // CurrencyController, Currency, Substract
using Watermelon.LevelSystem;       // CurrencyType, ExperienceController
using Watermelon.SquadShooter;      // UC_PetData, UC_PetSave, UC_PetGlobalSave, UIPetsPage, PedestalBehavior

namespace Watermelon.SquadShooter
{
    public class PetPanelUI : UIUpgradeAbstractPanel
    {
        [Header("Visual Elements")]
        [SerializeField] private Image              previewImage;      // 펫 미리보기 스프라이트
        [SerializeField] private TextMeshProUGUI    titleText;         // 펫 이름
        [SerializeField] private TextMeshProUGUI    levelText;         // 펫 레벨 표시

        [Header("Lock Feedback")]
        [SerializeField] private TextMeshProUGUI    lockMessageText;   // 레벨 부족 안내 메시지

        [Header("Action Buttons")]
        [SerializeField] private Button             unlockButton;      // 언락(Unlock) 버튼
        [SerializeField] private Button             upgradeButton;     // 업그레이드(Upgrade) 버튼
        [SerializeField] private TextMeshProUGUI    costText;          // 언락/업그레이드 비용 텍스트

        [Header("Main Button")]
        [SerializeField] private Button             mainButton;        // 패널 전체 클릭용 버튼 (Background)

        // 내부 참조
        private UC_PetData       petData;            // 이 패널이 담당하는 펫 데이터
        private UIPetsPage       parentPage;         // 상위 페이지 참조
        private UC_PetSave       petSave;            // 펫별 저장 데이터
        private UC_PetGlobalSave globalSave;         // 전역 선택 저장 데이터

        /// <summary>
        /// 패널이 표현하는 펫 데이터 객체 (UIPetsPage에서 panel.Data 로 접근) :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}
        /// </summary>
        public UC_PetData Data => petData;

        /// <summary>언락 여부</summary>
        public override bool IsUnlocked => petSave.HasPet(petData.petID);

        /// <summary>현재 업그레이드(레벨) 단계 반환</summary>
        public int GetLevel() => petSave.GetLevel(petData.petID);

        private void OnEnable()
        {
            // 전역 화폐 변화 구독
            CurrencyController.SubscribeGlobalCallback(OnCurrencyChanged);
        }

        private void OnDisable()
        {
            // 구독 해제
            CurrencyController.UnsubscribeGlobalCallback(OnCurrencyChanged);
        }

        /// <summary>
        /// 패널 초기화: 데이터 바인딩 및 버튼 리스너 설정
        /// </summary>
        public void Init(UC_PetData data, UIPetsPage parent)
        {
            petData    = data;
            parentPage = parent;
            petSave    = SaveController.GetSaveObject<UC_PetSave>("pet");
            globalSave = SaveController.GetSaveObject<UC_PetGlobalSave>("pet_global");

            // UI 초기값 세팅
            previewImage.sprite = petData.previewSprite;
            titleText.text      = petData.petName;

            // 버튼 클릭 리스너 등록
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);

            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(OnSelectButtonClicked);

            RedrawUI();
        }

        public override void OnPanelOpened()
        {
            base.OnPanelOpened();
            RedrawUI();
        }

        protected override void RedrawUpgradeButton()
        {
            // 화폐 변화 콜백에서 호출됨
            RedrawUI();
        }

        /// <summary>
        /// UI 전체 상태 갱신: 언락/업그레이드 버튼, 비용, 레벨, 선택 테두리 등
        /// </summary>
        public void RedrawUI()
        {
            bool unlocked      = IsUnlocked;
            int  lvl           = GetLevel();
            int  playerLevel   = ExperienceController.CurrentLevel;
            int  requiredLevel = petData.requiredPlayerLevel;

            // 레벨 표시
            levelText.text = unlocked ? $"Lv {lvl}" : string.Empty;

            // 선택 테두리 표시
            selectionImage.gameObject.SetActive(globalSave.SelectedPetID == petData.petID);

            // 언락 버튼
            unlockButton.gameObject.SetActive(!unlocked);
            bool canUnlock = playerLevel >= requiredLevel
                          && CurrencyController.HasAmount(CurrencyType.Coins, petData.unlockCost);
            unlockButton.interactable = canUnlock;
            if (!unlocked) costText.text = petData.unlockCost.ToString();

            // 레벨 부족 안내
            lockMessageText.gameObject.SetActive(!unlocked && playerLevel < requiredLevel);
            if (lockMessageText.gameObject.activeSelf)
                lockMessageText.text = $"Lv {requiredLevel} 달성 시 언락";

            // 업그레이드 버튼
            bool hasNext     = unlocked && lvl < petData.upgrades.Count;
            upgradeButton.gameObject.SetActive(hasNext);
            bool canUpgrade = hasNext
                           && CurrencyController.HasAmount(
                                CurrencyType.Coins,
                                petData.upgrades[lvl].cost
                              );
            upgradeButton.interactable = canUpgrade;
            if (hasNext)     costText.text = petData.upgrades[lvl].cost.ToString();
            else if (unlocked) costText.text = "MAX";
        }

        /// <summary>
        /// 전역 화폐 변경 콜백: 코인 변화 감지 시 UI 갱신
        /// </summary>
        private void OnCurrencyChanged(Currency currency, int difference)
        {
            if (currency.CurrencyType == CurrencyType.Coins)
                RedrawUI();
        }

        /// <summary>언락 버튼 클릭 처리</summary>
        private void OnUnlockClicked()
        {
            int playerLevel   = ExperienceController.CurrentLevel;
            int requiredLevel = petData.requiredPlayerLevel;

            if (playerLevel < requiredLevel) return;
            if (!CurrencyController.HasAmount(CurrencyType.Coins, petData.unlockCost)) return;

            CurrencyController.Substract(CurrencyType.Coins, petData.unlockCost);

            petSave.UnlockPet(petData.petID);
            SaveController.MarkAsSaveIsRequired();
            SaveController.Save();

            Select();
            parentPage.RefreshPanels();
        }

        /// <summary>업그레이드 버튼 클릭 처리</summary>
        private void OnUpgradeClicked()
        {
            int lvl = GetLevel();
            if (!CurrencyController.HasAmount(CurrencyType.Coins, petData.upgrades[lvl].cost)) return;

            CurrencyController.Substract(CurrencyType.Coins, petData.upgrades[lvl].cost);

            petSave.SetLevel(petData.petID, lvl + 1);
            SaveController.MarkAsSaveIsRequired();
            SaveController.Save();

            Select();
            parentPage.RefreshPanels();
        }

        /// <summary>패널 전체 클릭 시 선택 처리</summary>
        public void OnSelectButtonClicked()
        {
            if (!IsUnlocked) return;
            Select();
        }

        /// <summary>
        /// 패널 선택 로직
        /// - 전역 선택 저장
        /// - 디스크 저장
        /// - UI 갱신
        /// - 로비 미리보기 갱신
        /// </summary>
        public override void Select()
        {
            globalSave.SelectedPetID = petData.petID;
            SaveController.MarkAsSaveIsRequired();
            SaveController.Save();

            parentPage.RefreshPanels();

            var pedestal = Object.FindFirstObjectByType<PedestalBehavior>();
            pedestal?.ShowPreviewPet(petData.petID);
        }
    }
}
