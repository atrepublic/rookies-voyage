/*
 * CharacterPanelUI.cs
 * ---------------------
 * 이 스크립트는 캐릭터 선택 및 업그레이드 UI 패널의 개별 캐릭터 항목을 제어합니다.
 * 캐릭터의 미리보기 이미지, 이름, 업그레이드 상태, 구매 버튼, 잠금 상태 등을 표시하고
 * 캐릭터 선택 및 업그레이드 로직과 상호작용합니다.
 * UIUpgradeAbstractPanel을 상속받아 공통 UI 패널 기능을 활용합니다.
 */

using System.Collections.Generic;
using TMPro; // TextMesh Pro 네임스페이스
using UnityEngine;
using UnityEngine.UI; // Unity UI 네임스페이스
using Watermelon; // Watermelon 프레임워크 네임스페이스

namespace Watermelon.SquadShooter
{
    // 캐릭터 UI 패널 클래스 정의, UIUpgradeAbstractPanel 상속
    public class CharacterPanelUI : UIUpgradeAbstractPanel
    {
        // 상수 정의
        private const string LOCKED_NAME = "???"; // 잠긴 캐릭터 이름
        private const string UPGRADE_TEXT = "UPGRADE"; // 업그레이드 버튼 텍스트
        private const string EVOLVE_TEXT = "EVOLVE"; // 진화(스테이지 변경) 버튼 텍스트

        [Tooltip("캐릭터 미리보기 이미지")]
        [SerializeField] Image previewImage;
        [Tooltip("캐릭터 이름 또는 잠김 상태 텍스트")]
        [SerializeField] TextMeshProUGUI titleText;
        [Tooltip("캐릭터 패널 전체를 선택하기 위한 메인 버튼")]
        [SerializeField] Button mainButton;

        [Header("업그레이드 관련 UI 요소")]
        [Tooltip("업그레이드 관련 UI 요소들을 담는 부모 게임 오브젝트")]
        [SerializeField] GameObject upgradesStateObject;

        [Space] // 인스펙터 공백
        [Tooltip("업그레이드 단계가 활성화되었을 때의 색상")]
        [SerializeField] Color upgradeStateActiveColor = Color.white;
        [Tooltip("업그레이드 단계를 시각적으로 표시하는 이미지 배열")]
        [SerializeField] Image[] upgradesStatesImages;

        [Space] // 인스펙터 공백
        [Tooltip("업그레이드 또는 진화 구매 버튼")]
        [SerializeField] Button upgradesBuyButton;
        [Tooltip("구매 버튼의 배경 이미지 (활성/비활성 상태 표시)")]
        [SerializeField] Image upgradesBuyButtonImage;
        [Tooltip("구매에 필요한 재화 아이콘 이미지")]
        [SerializeField] Image upgradesBuyCurrencyImage;
        [Tooltip("구매 버튼에 표시될 가격 텍스트")]
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [Tooltip("구매 버튼이 활성화되었을 때의 스프라이트")]
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [Tooltip("구매 버튼이 비활성화되었을 때의 스프라이트")]
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;
        [Tooltip("구매 버튼 상단에 표시될 텍스트 (UPGRADE 또는 EVOLVE)")]
        [SerializeField] TextMeshProUGUI upgradesText;

        [Space] // 인스펙터 공백
        [Tooltip("최대 업그레이드 상태일 때 표시될 게임 오브젝트")]
        [SerializeField] GameObject upgradesMaxObject;

        [Header("잠금 상태 관련 UI 요소")]
        [Tooltip("캐릭터가 잠겨있을 때 표시될 UI 요소들을 담는 부모 게임 오브젝트")]
        [SerializeField] GameObject lockedStateObject;
        [Tooltip("잠금 해제에 필요한 레벨을 표시하는 텍스트")]
        [SerializeField] TextMeshProUGUI lockedStateText;
        [Tooltip("잠겨있을 때 미리보기 이미지의 색상")]
        [SerializeField] Color lockedPreviewColor = Color.white;

        // 이 캐릭터 패널이 나타내는 캐릭터가 잠금 해제되었는지 여부 (UIUpgradeAbstractPanel 오버라이드)
        public override bool IsUnlocked => Character.IsUnlocked();

        [Tooltip("이 UI 패널이 담당하는 캐릭터의 데이터")]
        private CharacterData character;
        // 외부에서 캐릭터 데이터에 접근하기 위한 프로퍼티
        public CharacterData Character => character;

        [Tooltip("패널이 처음 초기화될 때 캐릭터의 잠금 상태를 저장")]
        private bool storedIsLocked;

        // 외부(예: 튜토리얼)에서 업그레이드 버튼의 Transform에 접근하기 위한 프로퍼티
        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        [Tooltip("이 패널을 관리하는 상위 Characters 패널 UI 참조")]
        private UICharactersPanel charactersPanel;

        [Tooltip("게임패드 네비게이션을 위한 버튼 컴포넌트")]
        private UIGamepadButton gamepadButton;
        // 외부에서 게임패드 버튼 컴포넌트에 접근하기 위한 프로퍼티
        public UIGamepadButton GamepadButton => gamepadButton;

        // 현재 선택된 캐릭터 패널 UI (정적 변수)
        private static CharacterPanelUI selectedCharacterPanelUI;

        // 이 패널이 현재 선택된 패널인지 확인하는 함수
        public bool IsSelected() => selectedCharacterPanelUI == this;

        /// <summary>
        /// 캐릭터 패널 UI를 초기화합니다.
        /// </summary>
        /// <param name="character">이 패널에 표시할 캐릭터 데이터</param>
        /// <param name="charactersPanel">상위 캐릭터 패널 UI</param>
        public void Init(CharacterData character, UICharactersPanel charactersPanel)
        {
            this.character = character;
            this.charactersPanel = charactersPanel;

            panelRectTransform = (RectTransform)transform; // RectTransform 캐싱 (부모 클래스 변수)
            gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>(); // 게임패드 버튼 컴포넌트 캐싱

            previewImage.sprite = character.GetCurrentStage().PreviewSprite; // 현재 스테이지 미리보기 이미지 설정

            // 업그레이드 단계 시각화 (진화 단계에 별 표시 추가)
            for (int i = 0; i < upgradesStatesImages.Length; i++)
            {
                // 업그레이드 데이터 배열 범위 확인
                if (character.Upgrades.IsInRange(i + 1))
                {
                    // 해당 업그레이드가 스테이지 변경(진화)을 포함하는지 확인
                    if (character.Upgrades[i + 1].ChangeStage)
                    {
                        // 상위 패널에서 별 모양 오브젝트 가져오기
                        GameObject stageStarObject = charactersPanel.GetStageStarObject();
                        stageStarObject.transform.SetParent(upgradesStatesImages[i].rectTransform); // 해당 업그레이드 이미지 자식으로 설정
                        stageStarObject.transform.ResetLocal(); // 로컬 변환 초기화

                        // 별 이미지 크기 및 위치 조정
                        RectTransform stageStarRectTransform = (RectTransform)stageStarObject.transform;
                        stageStarRectTransform.sizeDelta = new Vector2(19.4f, 19.4f);
                        stageStarRectTransform.anchoredPosition = Vector2.zero;

                        stageStarObject.SetActive(true); // 별 오브젝트 활성화
                    }
                }
            }

            // 캐릭터 잠금 해제 상태에 따라 UI 초기화
            if (character.IsUnlocked())
            {
                titleText.text = character.CharacterName.ToUpper(); // 캐릭터 이름 표시 (대문자)
                storedIsLocked = false; // 초기 잠금 상태: 해제됨

                // 이 캐릭터가 현재 선택된 캐릭터면 선택 상태로 표시
                if (CharactersController.SelectedCharacter == character)
                    Select();

                // UI 요소 활성화/비활성화
                lockedStateObject.SetActive(false); // 잠금 상태 UI 비활성화
                upgradesStateObject.SetActive(true); // 업그레이드 상태 UI 활성화
                powerObject.SetActive(true); // 파워 표시 UI 활성화 (부모 클래스 변수)

                // 업그레이드 및 파워 정보 갱신
                RedrawUpgradeElements();
                RedrawPower();
            }
            else // 캐릭터가 잠겨있는 경우
            {
                titleText.text = LOCKED_NAME; // "???" 표시
                storedIsLocked = true; // 초기 잠금 상태: 잠김

                powerObject.SetActive(false); // 파워 표시 UI 비활성화

                // 잠금 상태 스프라이트 및 색상 설정
                previewImage.sprite = character.LockedSprite;
                previewImage.color = lockedPreviewColor;

                // 잠금 해제 조건(레벨) 설정
                SetRequiredLevel(character.RequiredLevel);
            }

            // 메인 버튼 클릭 시 OnSelectButtonClicked 함수 호출하도록 리스너 추가
            mainButton.onClick.AddListener(OnSelectButtonClicked);
        }

        /// <summary>
        /// 캐릭터 잠금 해제 애니메이션을 재생합니다. (주로 상태 변경 시 호출)
        /// </summary>
        private void PlayOpenAnimation()
        {
            lockedStateObject.SetActive(false); // 잠금 UI 숨기기

            // 관련 UI 활성화
            powerObject.SetActive(true);
            upgradesStateObject.SetActive(true);

            titleText.text = character.CharacterName.ToUpper(); // 캐릭터 이름 표시

            // 미리보기 이미지 원래대로 설정 및 애니메이션
            previewImage.sprite = character.Stages[0].PreviewSprite; // 첫 번째 스테이지 스프라이트로 설정
            previewImage.DOColor(Color.white, 0.6f); // 흰색으로 부드럽게 변경

            // 업그레이드 및 파워 정보 갱신
            RedrawUpgradeElements();
            RedrawPower();
        }

        /// <summary>
        /// 캐릭터 진화(스테이지 변경) 시 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="stage">새로운 스테이지 인덱스</param>
        private void PlayUpgradeAnimation(int stage)
        {
            CharacterStageData tempStage = character.Stages[stage]; // 해당 스테이지 데이터 가져오기

            previewImage.sprite = tempStage.PreviewSprite; // 미리보기 이미지 변경
            previewImage.rectTransform.localScale = Vector2.one * 1.3f; // 크기 키웠다가

            // 원래 크기로 돌아오는 스케일 애니메이션 (SineIn 효과)
            previewImage.rectTransform.DOScale(1.0f, 0.2f, 0.03f).SetEasing(Ease.Type.SineIn);
        }

        /// <summary>
        /// 패널이 열릴 때 호출됩니다. (UIUpgradeAbstractPanel 오버라이드)
        /// 캐릭터 상태 변경(잠금 해제 등)을 감지하고 애니메이션을 예약합니다.
        /// </summary>
        public override void OnPanelOpened()
        {
            List<CharacterDynamicAnimation> dynamicAnimations = new List<CharacterDynamicAnimation>(); // 재생할 애니메이션 목록

            bool isSelected = character.IsSelected(); // 이 캐릭터가 현재 선택된 상태인지 확인

            // 패널이 처음 열렸을 때 잠겨있었다면
            if (storedIsLocked)
            {
                // 현재는 잠금 해제 되었다면
                if (character.IsUnlocked())
                {
                    storedIsLocked = false; // 저장된 상태 업데이트

                    // 잠금 해제 애니메이션 생성 및 목록에 추가
                    CharacterDynamicAnimation unlockAnimation = new CharacterDynamicAnimation(this, 0.5f, onAnimationStarted: PlayOpenAnimation);
                    dynamicAnimations.Add(unlockAnimation);
                }
            }

            // 재생할 애니메이션이 있다면 상위 패널에 전달
            if (!dynamicAnimations.IsNullOrEmpty())
            {
                charactersPanel.AddAnimations(dynamicAnimations, isSelected);
            }

            // 최신 정보로 UI 갱신
            RedrawUpgradeElements();
            RedrawPower();
        }

        /// <summary>
        /// 잠금 해제에 필요한 레벨을 UI에 설정합니다.
        /// </summary>
        /// <param name="level">필요 레벨</param>
        public void SetRequiredLevel(int level)
        {
            lockedStateObject.SetActive(true); // 잠금 상태 UI 활성화
            lockedStateText.text = level.ToString(); // 레벨 텍스트 설정
        }

        /// <summary>
        /// 이 캐릭터 패널을 선택 상태로 만듭니다. (UIUpgradeAbstractPanel 오버라이드)
        /// </summary>
        public override void Select()
        {
            bool firstSelect = selectedCharacterPanelUI == null; // 처음 선택되는 것인지 확인

            // 이전에 선택된 패널이 있다면 선택 해제 처리
            if (!firstSelect)
                selectedCharacterPanelUI.UnselectCharacter();

            selectionImage.gameObject.SetActive(true); // 선택 테두리 활성화 (부모 클래스 변수)

            selectedCharacterPanelUI = this; // 현재 선택된 패널로 자신을 지정

            RedrawUpgradeButton(); // 업그레이드 버튼 상태 갱신 (포커스 등)

            CharactersController.SelectCharacter(character); // 컨트롤러에 캐릭터 선택 알림

            // 처음 선택된 경우가 아닐 때 (다른 캐릭터에서 변경될 때)
            if (!firstSelect)
            {
                CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
                if (characterBehaviour != null)
                    characterBehaviour.Jump(); // 실제 게임 내 캐릭터 점프 애니메이션 실행
            }
        }

        /// <summary>
        /// 이 캐릭터 패널의 선택 상태를 해제합니다.
        /// </summary>
        public void UnselectCharacter()
        {
            selectionImage.gameObject.SetActive(false); // 선택 테두리 비활성화

            // 게임패드 포커스 해제
            if (gamepadButton != null)
                gamepadButton.SetFocus(false);
        }

        /// <summary>
        /// 캐릭터 업그레이드 시 UI 애니메이션을 재생합니다. (업그레이드 단계 아이콘 색상 변경 등)
        /// </summary>
        private void PlayUpgradeAnimation()
        {
            // 현재 업그레이드 레벨에 해당하는 아이콘 인덱스 (0부터 시작)
            int upgradeStateIndex = character.GetCurrentUpgradeIndex() - 1;
            // 해당 아이콘 색상을 활성 색상으로 부드럽게 변경
            upgradesStatesImages[upgradeStateIndex].DOColor(upgradeStateActiveColor, 0.3f).OnComplete(delegate
            {
                // 애니메이션 완료 후
                isUpgradeAnimationPlaying = false; // 애니메이션 재생 플래그 해제 (부모 클래스 변수)
                RedrawUpgradeButton(); // 업그레이드 버튼 상태 갱신
            });

            // 최대 레벨이 아니면 구매 버튼 활성화, 최대 레벨이면 MAX 표시 활성화
            if (!character.IsMaxUpgrade())
            {
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);
            }
            else
            {
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 캐릭터의 현재 파워(전투력)를 UI에 갱신합니다.
        /// </summary>
        private void RedrawPower()
        {
            CharacterUpgrade currentUpgrade = character.GetCurrentUpgrade(); // 현재 업그레이드 정보 가져오기

            // 파워 텍스트 갱신
            powerText.text = currentUpgrade.Stats.Power.ToString(); // (powerText는 부모 클래스 변수)
        }

        /// <summary>
        /// 업그레이드/진화 버튼의 상태(활성/비활성, 가격, 아이콘 등)를 갱신합니다. (UIUpgradeAbstractPanel 오버라이드)
        /// </summary>
        protected override void RedrawUpgradeButton()
        {
            // 최대 업그레이드가 아닌 경우
            if (!character.IsMaxUpgrade())
            {
                // 다음 업그레이드 정보 가져오기
                CharacterUpgrade upgradeState = character.Upgrades[character.GetCurrentUpgradeIndex() + 1];
                Currency currency = CurrencyController.GetCurrency(upgradeState.CurrencyType); // 필요한 재화 정보 가져오기

                int price = upgradeState.Price; // 업그레이드 비용
                // 재화가 충분한지 확인
                if (CurrencyController.HasAmount(upgradeState.CurrencyType, price))
                {
                    // 활성 상태 버튼 스프라이트 설정
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;

                    // 게임패드 포커스 설정 (현재 선택된 패널일 경우에만 포커스)
                    if (gamepadButton != null)
                        gamepadButton.SetFocus(selectedCharacterPanelUI == this);
                }
                else // 재화가 부족한 경우
                {
                    // 비활성 상태 버튼 스프라이트 설정
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;

                    // 게임패드 포커스 해제
                    if (gamepadButton != null)
                        gamepadButton.SetFocus(false);
                }

                // 재화 아이콘 및 가격 텍스트 설정
                upgradesBuyCurrencyImage.sprite = currency.Icon;
                upgradesBuyButtonText.text = CurrencyHelper.Format(price); // 가격 포맷팅 적용

                // 다음 업그레이드가 스테이지 변경(진화)인지에 따라 버튼 텍스트 변경
                if (upgradeState.ChangeStage)
                {
                    upgradesText.text = EVOLVE_TEXT; // "EVOLVE"
                }
                else
                {
                    upgradesText.text = UPGRADE_TEXT; // "UPGRADE"
                }
            }
            // 최대 업그레이드인 경우는 RedrawUpgradeElements에서 처리 (버튼 숨김)
        }

        /// <summary>
        /// 이 캐릭터가 패널이 열리기 전에는 잠겨있다가 현재는 열렸는지 확인합니다. (새 캐릭터 잠금 해제 여부)
        /// </summary>
        /// <returns>새로 잠금 해제되었으면 true</returns>
        public bool IsNewCharacterOpened()
        {
            return storedIsLocked && character.IsUnlocked();
        }

        /// <summary>
        /// 다음 업그레이드를 구매할 수 있는지(재화 충분) 확인합니다.
        /// </summary>
        /// <returns>구매 가능하면 true</returns>
        public bool IsNextUpgradeCanBePurchased()
        {
            // 캐릭터가 잠금 해제 상태이고
            if (character.IsUnlocked())
            {
                // 최대 업그레이드가 아니면
                if (!character.IsMaxUpgrade())
                {
                    // 다음 업그레이드 정보 가져오기
                    CharacterUpgrade upgradeState = character.Upgrades[character.GetCurrentUpgradeIndex() + 1];

                    // 재화가 충분한지 확인
                    if (CurrencyController.HasAmount(upgradeState.CurrencyType, upgradeState.Price))
                        return true; // 구매 가능
                }
            }

            return false; // 구매 불가
        }

        /// <summary>
        /// 업그레이드 관련 UI 요소(단계 아이콘 색상, 구매 버튼/MAX 표시)를 현재 상태에 맞게 갱신합니다.
        /// </summary>
        private void RedrawUpgradeElements()
        {
            // 최대 업그레이드가 아닌 경우
            if (!character.IsMaxUpgrade())
            {
                int upgradeStateIndex = character.GetCurrentUpgradeIndex(); // 현재까지 완료된 업그레이드 인덱스
                // 완료된 업그레이드 단계 아이콘 색상 활성화
                for (int i = 0; i < upgradeStateIndex; i++)
                {
                    upgradesStatesImages[i].color = upgradeStateActiveColor;
                }
                // 아직 완료되지 않은 아이콘은 기본 색상 유지 (Inspector에서 설정된 색상)

                // MAX 표시 숨기고 구매 버튼 표시
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);

                // 구매 버튼 상태 갱신 (가격, 활성/비활성 등)
                RedrawUpgradeButton();
            }
            else // 최대 업그레이드인 경우
            {
                // 모든 업그레이드 단계 아이콘 색상 활성화
                for (int i = 0; i < upgradesStatesImages.Length; i++)
                {
                    upgradesStatesImages[i].color = upgradeStateActiveColor;
                }

                // 구매 버튼 숨기고 MAX 표시 활성화
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);

                // 게임패드 포커스 해제
                if (gamepadButton != null)
                    gamepadButton.SetFocus(false);
            }
        }

        /// <summary>
        /// 업그레이드/진화 버튼 클릭 시 호출됩니다.
        /// </summary>
        public void OnUpgradeButtonClicked()
        {
            // UI 컨트롤이 잠겨있으면 반응 없음
            if (UICharactersPanel.IsControlBlocked)
                return;

            // 최대 업그레이드가 아닌 경우
            if (!character.IsMaxUpgrade())
            {
                // 버튼 클릭 시 해당 캐릭터를 먼저 선택 상태로 만듦
                OnSelectButtonClicked();

                int upgradeStateIndex = character.GetCurrentUpgradeIndex() + 1; // 다음 업그레이드 인덱스

                // 업그레이드 비용 및 재화 타입 가져오기
                int price = character.Upgrades[upgradeStateIndex].Price;
                CurrencyType currencyType = character.Upgrades[upgradeStateIndex].CurrencyType;

                // 재화가 충분한지 확인
                if (CurrencyController.HasAmount(currencyType, price))
                {
                    isUpgradeAnimationPlaying = true; // 업그레이드 애니메이션 재생 중 플래그 설정

                    CurrencyController.Substract(currencyType, price); // 재화 차감

                    character.UpgradeCharacter(); // 캐릭터 데이터 업그레이드

                    // 업그레이드한 캐릭터가 현재 선택된 캐릭터라면 게임 내 캐릭터에도 즉시 반영
                    if (CharactersController.SelectedCharacter == character)
                    {
                        CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour(); // 게임 내 캐릭터 비헤이비어 가져오기

                        CharacterUpgrade currentUpgrade = character.GetCurrentUpgrade(); // 방금 적용된 업그레이드 정보
                        // 스테이지 변경(진화)이 포함된 업그레이드인지 확인
                        if (currentUpgrade.ChangeStage)
                        {
                            // 진화 애니메이션 재생
                            PlayUpgradeAnimation(currentUpgrade.StageIndex);
                            // 게임 내 캐릭터 그래픽 변경 (파티클, 애니메이션 재생)
                            characterBehaviour.SetGraphics(character.Stages[currentUpgrade.StageIndex].Prefab, true, true);
                        }
                        else // 단순 능력치 업그레이드인 경우
                        {
                            // 게임 내 캐릭터에 파티클 및 바운스 애니메이션 재생
                            BaseCharacterGraphics characterGraphics = characterBehaviour.Graphics;
                            characterGraphics.PlayUpgradeParticle();
                            characterGraphics.PlayBounceAnimation();
                        }

                        // 게임 내 캐릭터 스탯 업데이트
                        characterBehaviour.SetStats(currentUpgrade.Stats);

                        // 캐릭터 점프 액션 실행
                        characterBehaviour.Jump();
                    }

                    // UI 업그레이드 애니메이션 재생 (아이콘 색상 변경 등)
                    PlayUpgradeAnimation();

                    // 업그레이드 버튼 상태 갱신
                    RedrawUpgradeButton();

                    // 파워(전투력) 텍스트 갱신
                    RedrawPower();
                }

                // 버튼 클릭 사운드 재생
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            }
        }

        /// <summary>
        /// 캐릭터 패널의 메인 영역(미리보기, 이름 등) 클릭 시 호출됩니다.
        /// 해당 캐릭터를 선택 상태로 만듭니다.
        /// </summary>
        public void OnSelectButtonClicked()
        {
            // UI 컨트롤이 잠겨있으면 반응 없음
            if (UICharactersPanel.IsControlBlocked)
                return;

            // 이미 선택된 캐릭터면 반응 없음
            if (character.IsSelected())
                return;

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // 캐릭터가 잠금 해제 상태인지 확인
            if (!character.IsUnlocked())
                return; // 잠겨있으면 선택 불가

            // 캐릭터 선택 처리 함수 호출
            Select();
        }
    }
}