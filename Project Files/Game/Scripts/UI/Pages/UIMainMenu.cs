//====================================================================================================
// 해당 스크립트: UIMainMenu.cs
// 기능: 메인 메뉴 UI를 관리하고 표시합니다.
// 용도: 게임 시작, 설정, 상점, 캐릭터 업그레이드 등 메인 메뉴의 다양한 기능을 제공하며,
//      레벨 진행 상황 및 캐릭터 경험치 정보를 표시합니다.
//====================================================================================================
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        [Tooltip("노치 디자인 등 안전 영역을 고려하여 UI를 배치할 RectTransform입니다.")]
        [SerializeField] private RectTransform safeAreaRectTransform;

        [Tooltip("플레이어 경험치 UI를 관리하는 ExperienceUIController 컴포넌트입니다.")]
        [SerializeField] private ExperienceUIController experienceUIController;
        [Tooltip("레벨 진행 상황을 표시하는 LevelProgressionPanel 컴포넌트입니다.")]
        [SerializeField] private LevelProgressionPanel levelProgressionPanel;

        [Space]
        [Tooltip("현재 레벨/구역 정보를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI areaText;
        [Tooltip("권장 전투력 정보를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI recomendedPowerText;

        [Space]
        [Tooltip("설정 메뉴를 여는 버튼입니다.")]
        [SerializeField] private Button settingsButton;
        [Tooltip("광고 제거 상품 구매 버튼입니다.")]
        [SerializeField] private Button noAdsButton;
        [Tooltip("게임을 시작하는 버튼입니다.")]
        [SerializeField] private Button tapToPlayButton;

        [Space]
        [Tooltip("하단 메뉴 버튼들이 배치될 RectTransform입니다.")]
        [SerializeField] private RectTransform bottomPanelRectTransform;

        private RectTransform noAdsRectTransform; // 광고 제거 버튼의 RectTransform

        private LevelSave levelSave; // 레벨 저장 데이터

        /// <summary>
        /// 플레이어 경험치 UI를 관리하는 ExperienceUIController 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public ExperienceUIController ExperienceUIController => experienceUIController;
        /// <summary>
        /// 레벨 진행 상황을 표시하는 LevelProgressionPanel 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public LevelProgressionPanel LevelProgressionPanel => levelProgressionPanel;

        private List<MenuPanelButton> panelButtons; // 하단 메뉴 패널 버튼 목록

        private UIGamepadButton noAdsGamepadButton; // 광고 제거 버튼의 게임패드 버튼 컴포넌트
        /// <summary>
        /// 광고 제거 버튼의 UIGamepadButton 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public UIGamepadButton NoAdsGamepadButton => noAdsGamepadButton;

        private UIGamepadButton settingsGamepadButton; // 설정 버튼의 게임패드 버튼 컴포넌트
        /// <summary>
        /// 설정 버튼의 UIGamepadButton 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public UIGamepadButton SettingsGamepadButton => settingsGamepadButton;

        private UIGamepadButton playGamepadButton; // 게임 시작 버튼의 게임패드 버튼 컴포넌트
        /// <summary>
        /// 게임 시작 버튼의 UIGamepadButton 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public UIGamepadButton PlayGamepadButton => playGamepadButton;

        /// <summary>
        /// UI 메인 메뉴를 초기화하는 함수입니다.
        /// 레벨 저장 데이터를 가져오고, 광고 제거 버튼, 설정 및 플레이 버튼, 경험치 UI,
        /// 레벨 진행 패널, 하단 메뉴 버튼들을 설정합니다.
        /// </summary>
        public override void Init()
        {
            // 레벨 저장 데이터 가져오기
            levelSave = SaveController.GetSaveObject<LevelSave>("level");

            // 수익화가 활성화되어 있으면 광고 제거 버튼 설정
            if (Monetization.IsActive)
            {
                noAdsRectTransform = (RectTransform)noAdsButton.transform; // 광고 제거 버튼 RectTransform 가져오기
                noAdsButton.onClick.AddListener(() => OnNoAdsButtonClicked()); // 클릭 이벤트 리스너 추가
                noAdsButton.gameObject.SetActive(true); // 게임 오브젝트 활성화

                noAdsGamepadButton = noAdsButton.GetComponent<UIGamepadButton>(); // 게임패드 버튼 컴포넌트 가져오기
            }
            else
            {
                noAdsButton.gameObject.SetActive(false); // 수익화 비활성화 시 광고 제거 버튼 숨김
            }

            // 설정 및 플레이 버튼의 게임패드 버튼 컴포넌트 가져오기
            settingsGamepadButton = settingsButton.GetComponent<UIGamepadButton>();
            playGamepadButton = tapToPlayButton.GetComponent<UIGamepadButton>();

            experienceUIController.Init(); // 경험치 UI 초기화

            levelProgressionPanel.Init(); // 레벨 진행 패널 초기화
            levelProgressionPanel.LoadPanel(); // 레벨 진행 패널 로드

            // 하단 패널 버튼들 설정
            int buttonsChildElements = bottomPanelRectTransform.childCount; // 하단 패널의 자식 오브젝트 개수
            if (buttonsChildElements > 0)
            {
                panelButtons = new List<MenuPanelButton>(); // 패널 버튼 목록 초기화
                for (int i = 0; i < buttonsChildElements; i++)
                {
                    MenuPanelButton menuPanelButton = bottomPanelRectTransform.GetChild(i).GetComponent<MenuPanelButton>(); // MenuPanelButton 컴포넌트 가져오기
                    if (menuPanelButton != null)
                    {
                        if (menuPanelButton.IsActive()) // 버튼이 활성화 상태이면
                        {
                            menuPanelButton.Init(); // 버튼 초기화

                            panelButtons.Add(menuPanelButton); // 목록에 추가
                        }
                        else
                        {
                            Destroy(menuPanelButton.gameObject); // 활성화 상태가 아니면 오브젝트 파괴
                        }
                    }
                }
            }

            // 안전 영역(노치 디자인 등) 설정
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            // 태블릿 환경인 경우 하단 패널 크기 조절
            if (UIController.IsTablet)
            {
                var scrollSize = bottomPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                bottomPanelRectTransform.sizeDelta = scrollSize;
            }
        }

        /// <summary>
        /// 오브젝트가 활성화될 때 호출되는 함수입니다.
        /// 인앱 구매 완료 이벤트를 구독합니다.
        /// </summary>
        private void OnEnable()
        {
            // 인앱 구매 완료 이벤트 구독
            IAPManager.OnPurchaseComplete += OnPurchaseComplete;
        }

        /// <summary>
        /// 오브젝트가 비활성화될 때 호출되는 함수입니다.
        /// 인앱 구매 완료 이벤트 구독을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            // 인앱 구매 완료 이벤트 구독 해제
            IAPManager.OnPurchaseComplete -= OnPurchaseComplete;
        }

        /// <summary>
        /// 레벨 정보(현재 구역 및 권장 전투력) 텍스트를 업데이트하는 함수입니다.
        /// </summary>
        public void UpdateLevelText()
        {
            // 현재 레벨/구역 정보 텍스트 업데이트
            areaText.text = string.Format(LevelController.AREA_TEXT, levelSave.WorldIndex + 1, levelSave.LevelIndex + 1);
            // 권장 전투력 텍스트 업데이트
            recomendedPowerText.text = BalanceController.PowerRequirement.ToString();
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 하단 패널 버튼 애니메이션, 레벨 진행 패널 표시, 하단 패널 등장 애니메이션 등을 처리합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            // 하단 패널 버튼들의 등장 애니메이션 실행
            if (!panelButtons.IsNullOrEmpty())
            {
                foreach (MenuPanelButton button in panelButtons)
                {
                    button.OnWindowOpened();
                }
            }

            levelProgressionPanel.Show(); // 레벨 진행 패널 표시 애니메이션 실행

            // 하단 패널 등장 애니메이션 (CubicOut 보간)
            bottomPanelRectTransform.anchoredPosition = new Vector2(0, -500); // 초기 위치 설정
            bottomPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.CubicOut).OnComplete(() => {
                UIController.OnPageOpened(this); // UI 컨트롤러에 페이지 열림 이벤트 알림

                UIGamepadButton.EnableTag(UIGamepadButtonTag.MainMenu); // 메인 메뉴 관련 게임패드 버튼 태그 활성화
            });

            tapToPlayButton.gameObject.SetActive(true); // '탭하여 플레이' 버튼 활성화

            // 수익화가 활성화되어 있고 강제 광고가 활성화된 경우 광고 제거 버튼 애니메이션 처리
            if (Monetization.IsActive)
            {
                if (AdsManager.IsForcedAdEnabled())
                {
                    noAdsRectTransform.gameObject.SetActive(true); // 광고 제거 버튼 활성화
                    noAdsRectTransform.anchoredPosition = new Vector2(noAdsRectTransform.sizeDelta.x, noAdsRectTransform.anchoredPosition.y); // 초기 위치 설정 (화면 밖)
                    noAdsRectTransform.DOAnchoredPosition(new Vector2(-35, noAdsRectTransform.anchoredPosition.y), 0.5f).SetEasing(Ease.Type.CubicOut); // 등장 애니메이션
                }
                else
                {
                    noAdsRectTransform.gameObject.SetActive(false); // 강제 광고 비활성화 시 광고 제거 버튼 숨김
                }
            }

            UpdateLevelText(); // 레벨 정보 텍스트 업데이트

            ExperienceController.ApplyExperience(); // 경험치 적용 (UI 업데이트 포함)
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// '탭하여 플레이' 버튼을 비활성화하고 광고 제거 버튼 숨김 애니메이션을 처리합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            tapToPlayButton.gameObject.SetActive(false); // '탭하여 플레이' 버튼 비활성화

            // 수익화가 활성화되어 있고 강제 광고가 활성화된 경우 광고 제거 버튼 숨김 애니메이션 처리
            if (Monetization.IsActive)
            {
                if (AdsManager.IsForcedAdEnabled())
                {
                    noAdsRectTransform.gameObject.SetActive(true); // 광고 제거 버튼 활성화 상태 유지 (애니메이션용)
                    noAdsRectTransform.DOAnchoredPosition(new Vector2(noAdsRectTransform.sizeDelta.x, noAdsRectTransform.anchoredPosition.y), 0.3f).SetEasing(Ease.Type.CubicIn); // 숨김 애니메이션
                }
                else
                {
                    noAdsRectTransform.gameObject.SetActive(false); // 강제 광고 비활성화 시 광고 제거 버튼 숨김
                }
            }

            UIController.OnPageClosed(this); // UI 컨트롤러에 페이지 닫힘 이벤트 알림
        }

        /// <summary>
        /// 인앱 구매 완료 시 호출되는 함수입니다.
        /// 광고 제거 상품 구매 완료 시 광고 제거 버튼을 숨기고 강제 광고를 비활성화합니다.
        /// </summary>
        /// <param name="productKeyType">구매된 상품의 키 타입</param>
        private void OnPurchaseComplete(ProductKeyType productKeyType)
        {
            // 광고 제거 상품 구매 완료 시 처리
            if (productKeyType == ProductKeyType.NoAds)
            {
                noAdsRectTransform.gameObject.SetActive(false); // 광고 제거 버튼 숨김

                AdsManager.DisableForcedAd(); // 강제 광고 비활성화
            }
        }

        #region Buttons
        /// <summary>
        /// 광고 제거 버튼 클릭 시 호출되는 함수입니다.
        /// 버튼 클릭 사운드를 재생하고 광고 제거 상품 구매를 시도합니다.
        /// </summary>
        public void OnNoAdsButtonClicked()
        {
            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // 광고 제거 상품 구매 시도
            IAPManager.BuyProduct(ProductKeyType.NoAds);
        }

        /// <summary>
        /// 게임 시작 버튼 클릭 시 호출되는 함수입니다.
        /// 그래픽 레이캐스터를 비활성화하여 중복 클릭을 방지하고, 오버레이 표시 후 게임 씬을 로드합니다.
        /// </summary>
        public void PlayButton()
        {
            // 그래픽 레이캐스터 비활성화 (중복 클릭 방지)
            if (!graphicRaycaster.enabled) return;
            graphicRaycaster.enabled = false;

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // 오버레이 표시 후 게임 씬 로드
            Overlay.Show(0.3f, () =>
            {
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 추가된 부분 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                Debug.Log("[UIMainMenu] PlayButton - 게임 씬 로드 직전. PoolManager의 모든 풀을 정리합니다.");
                PoolManager.ClearAllPools(); // 플레이 씬 로드 전에 모든 풀 정리
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

                SceneManager.LoadScene("Game"); // "Game" 씬 로드

                Overlay.Hide(0.3f, null); // 오버레이 숨김
            });
        }
        #endregion
    }
}