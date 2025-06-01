//====================================================================================================
// 해당 스크립트: UICharactersPanel.cs
// 기능: 캐릭터 업그레이드 패널 UI를 관리하고 표시합니다.
// 용도: 캐릭터 목록을 보여주고, 각 캐릭터의 상태(잠금 해제, 업그레이드 가능 여부)를 표시하며,
//      캐릭터 선택 및 업그레이드 기능을 제공합니다. 또한 캐릭터 등장 애니메이션을 관리합니다.
//      [최종 수정] 선택된 캐릭터의 상세 능력치 표시, 업그레이드 시 강조 애니메이션 기능 통합 및 오류 수정.
//====================================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가
using TMPro;         // TextMeshProUGUI 사용을 위해 추가
using Watermelon;    // Watermelon 프레임워크

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIUpgradesAbstractPage<CharacterPanelUI, CharacterData>
    {
        [Space]
        [Tooltip("스테이지 별 프리팹입니다. 캐릭터 업그레이드 단계에 따라 표시됩니다.")]
        [SerializeField] private GameObject stageStarPrefab;

        // ▼▼▼ 상세 능력치를 표시할 UI 필드 (TextMeshProUGUI) ▼▼▼
        [Header("선택된 캐릭터 능력치 표시 UI (General Indicators)")]
        [SerializeField] private TextMeshProUGUI generalHealthText;
        [SerializeField] private TextMeshProUGUI generalMoveSpeedText;
        [SerializeField] private TextMeshProUGUI generalCritChanceText;
        [SerializeField] private TextMeshProUGUI generalCritMultiplierText;
        [SerializeField] private TextMeshProUGUI generalCurrentLevelText;
        // [SerializeField] private TextMeshProUGUI generalPowerText; // 필요시 주석 해제 및 에디터 연결

        // ▼▼▼ 각 능력치 인디케이터의 애니메이터(UIStatIndicatorAnimator) 참조 필드 ▼▼▼
        [Header("선택된 캐릭터 능력치 애니메이터 (General Indicators)")]
        [SerializeField] private UIStatIndicatorAnimator generalHealthAnimator;
        [SerializeField] private UIStatIndicatorAnimator generalMoveSpeedAnimator;
        [SerializeField] private UIStatIndicatorAnimator generalCritChanceAnimator;
        [SerializeField] private UIStatIndicatorAnimator generalCritMultiplierAnimator;
        [SerializeField] private UIStatIndicatorAnimator generalCurrentLevelAnimator;
        // [SerializeField] private UIStatIndicatorAnimator generalPowerAnimator; // 필요시 주석 해제 및 에디터 연결

        private CharactersDatabase charactersDatabase;
        private Pool stageStarPool;

        protected override int SelectedIndex => Mathf.Clamp(CharactersController.GetCharacterIndex(CharactersController.SelectedCharacter), 0, int.MaxValue);

        public GameObject GetStageStarObject()
        {
            return stageStarPool.GetPooledObject();
        }

        /// <summary>
        /// 현재 선택된 캐릭터의 상세 능력치를 "General Indicator" UI 영역에 업데이트합니다.
        /// </summary>
        private void UpdateSelectedCharacterStatsDisplay()
        {
            CharacterData selectedCharacter = CharactersController.SelectedCharacter;

            if (selectedCharacter == null || !selectedCharacter.IsUnlocked())
            {
                if (generalHealthText != null) generalHealthText.text = "-";
                if (generalMoveSpeedText != null) generalMoveSpeedText.text = "-";
                if (generalCritChanceText != null) generalCritChanceText.text = "-";
                if (generalCritMultiplierText != null) generalCritMultiplierText.text = "-";
                if (generalCurrentLevelText != null) generalCurrentLevelText.text = "LV. -";
                // if (generalPowerText != null) generalPowerText.text = "-"; 
                return;
            }

            CharacterUpgrade currentCharacterUpgrade = selectedCharacter.GetCurrentUpgrade();
            if (currentCharacterUpgrade == null) return;

            CharacterStats currentStats = currentCharacterUpgrade.Stats;
            if (currentStats == null) return;

            if (generalHealthText != null)
                generalHealthText.text = currentStats.Health.ToString();
            if (generalMoveSpeedText != null)
                generalMoveSpeedText.text = currentStats.MoveSpeed.ToString("F1");
            if (generalCritChanceText != null)
                generalCritChanceText.text = $"{currentStats.CritChance * 100:F0}%";
            if (generalCritMultiplierText != null)
                generalCritMultiplierText.text = $"{currentStats.CritMultiplier:F1}x";
            if (generalCurrentLevelText != null)
                generalCurrentLevelText.text = $"LV. {selectedCharacter.GetCurrentUpgradeIndex() + 1}";
            
            // if (generalPowerText != null) generalPowerText.text = currentStats.Power.ToString();
        }

        // 이벤트 구독 및 해제
        private void OnEnable()
        {
            CharactersController.OnCharacterSelectedEvent += OnCharacterSelected;
            CharactersController.OnCharacterUpgradedEvent += OnCharacterUpgraded;

            // 패널 활성화 시 현재 선택된 캐릭터 정보로 UI 즉시 업데이트
            if (CharactersController.SelectedCharacter != null)
            {
                UpdateSelectedCharacterStatsDisplay();
            }
        }

        private void OnDisable()
        {
            CharactersController.OnCharacterSelectedEvent -= OnCharacterSelected;
            CharactersController.OnCharacterUpgradedEvent -= OnCharacterUpgraded;
        }

        /// <summary>
        /// 캐릭터 선택 변경 시 호출되어 UI를 업데이트합니다.
        /// </summary>
        private void OnCharacterSelected(CharacterData selectedCharacter)
        {
            UpdateSelectedCharacterStatsDisplay();
        }

        /// <summary>
        /// 캐릭터 업그레이드 시 호출되어 UI 및 강조 애니메이션을 처리합니다.
        /// </summary>
        private void OnCharacterUpgraded(CharacterData upgradedCharacter)
        {
            if (CharactersController.SelectedCharacter == upgradedCharacter && upgradedCharacter != null && upgradedCharacter.IsUnlocked())
            {
                CharacterUpgrade currentCharacterUpgrade = upgradedCharacter.GetCurrentUpgrade();
                if (currentCharacterUpgrade == null) return;
                CharacterStats newStats = currentCharacterUpgrade.Stats;
                if (newStats == null) return;

                int newLevelIndex = upgradedCharacter.GetCurrentUpgradeIndex();
                CharacterStats oldStats = null;

                if (newLevelIndex > 0)
                {
                    CharacterUpgrade prevUpgradeData = upgradedCharacter.GetUpgrade(newLevelIndex - 1);
                    if (prevUpgradeData != null && prevUpgradeData.Stats != null)
                    {
                        oldStats = prevUpgradeData.Stats;
                    }
                }

                UpdateSelectedCharacterStatsDisplay(); // 텍스트 먼저 업데이트

                // 능력치 증가 시 강조 애니메이션 재생
                if (oldStats == null || (newStats.Health > oldStats.Health))
                {
                    if (generalHealthAnimator != null) generalHealthAnimator.PlayHighlightAnimation();
                }
                if (oldStats == null || (newStats.MoveSpeed > oldStats.MoveSpeed))
                {
                    if (generalMoveSpeedAnimator != null) generalMoveSpeedAnimator.PlayHighlightAnimation();
                }
                if (oldStats == null || (newStats.CritChance > oldStats.CritChance))
                {
                    if (generalCritChanceAnimator != null) generalCritChanceAnimator.PlayHighlightAnimation();
                }
                if (oldStats == null || (newStats.CritMultiplier > oldStats.CritMultiplier))
                {
                    if (generalCritMultiplierAnimator != null) generalCritMultiplierAnimator.PlayHighlightAnimation();
                }
                if (generalCurrentLevelAnimator != null) // 레벨은 항상 업데이트
                {
                     generalCurrentLevelAnimator.PlayHighlightAnimation();
                }
            }
        }

        public bool IsAnyActionAvailable()
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].IsNewCharacterOpened())
                    return true;
                if (itemPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }
            return false;
        }

        protected override void EnableGamepadButtonTag()
        {
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Characters);
        }

        #region Animation
        private bool isAnimationPlaying;
        private Coroutine animationCoroutine;
        private static bool isControlBlocked = false;
        public static bool IsControlBlocked => isControlBlocked;
        private static List<CharacterDynamicAnimation> characterDynamicAnimations = new List<CharacterDynamicAnimation>();

        private void ResetAnimations()
        {
            if (isAnimationPlaying)
            {
                if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                isAnimationPlaying = false;
                animationCoroutine = null;
            }
            characterDynamicAnimations = new List<CharacterDynamicAnimation>();
        }

        private void StartAnimations()
        {
            if (isAnimationPlaying)
                return;
            if (!characterDynamicAnimations.IsNullOrEmpty())
            {
                isControlBlocked = true;
                if (scrollView != null) scrollView.enabled = false;
                isAnimationPlaying = true;
                animationCoroutine = StartCoroutine(DynamicAnimationCoroutine());
            }
        }

        private IEnumerator ScrollCoroutine(CharacterPanelUI characterPanelUI)
        {
            if (characterPanelUI == null || characterPanelUI.RectTransform == null || scrollView == null || scrollView.content == null) yield break;
            float scrollOffsetX = -(characterPanelUI.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);
            float positionDiff = Mathf.Abs(scrollView.content.anchoredPosition.x - scrollOffsetX);
            if (positionDiff > 80)
            {
                Ease.IEasingFunction easeFunctionCubicIn = Ease.GetFunction(Ease.Type.CubicOut);
                Vector2 currentPosition = scrollView.content.anchoredPosition;
                Vector2 targetPosition = new Vector2(scrollOffsetX, 0);
                float speed = Mathf.Max(0.01f, positionDiff / 2500);
                for (float s = 0; s < 1.0f; s += Time.deltaTime / speed)
                {
                    if (scrollView == null || scrollView.content == null) yield break;
                    scrollView.content.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, easeFunctionCubicIn.Interpolate(s));
                    yield return null;
                }
                if (scrollView != null && scrollView.content != null) scrollView.content.anchoredPosition = targetPosition;
            }
        }

        private IEnumerator DynamicAnimationCoroutine()
        {
            int currentAnimationIndex = 0;
            CharacterDynamicAnimation tempAnimation;
            WaitForSeconds delayWait = new WaitForSeconds(0.4f);
            yield return delayWait;
            while (currentAnimationIndex < characterDynamicAnimations.Count)
            {
                tempAnimation = characterDynamicAnimations[currentAnimationIndex];
                if (tempAnimation == null || tempAnimation.CharacterPanel == null)
                {
                    currentAnimationIndex++;
                    continue;
                }
                delayWait = new WaitForSeconds(tempAnimation.Delay);
                yield return StartCoroutine(ScrollCoroutine(tempAnimation.CharacterPanel));
                tempAnimation.OnAnimationStarted?.Invoke();
                yield return delayWait;
                currentAnimationIndex++;
            }
            yield return null;
            isAnimationPlaying = false;
            isControlBlocked = false;
            if (scrollView != null) scrollView.enabled = true;
        }

        public void AddAnimations(List<CharacterDynamicAnimation> newAnimations, bool isPrioritize = false)
        {
            if(newAnimations.IsNullOrEmpty()) return;
            if (!isPrioritize)
            {
                characterDynamicAnimations.AddRange(newAnimations);
            }
            else
            {
                characterDynamicAnimations.InsertRange(0, newAnimations);
            }
        }
        #endregion

        #region UI Page
        public override void Init()
        {
            base.Init();
            charactersDatabase = CharactersController.GetDatabase();
            if (stageStarPrefab != null)
            {
                stageStarPool = new Pool(stageStarPrefab, stageStarPrefab.name);
            }
            else
            {
                Debug.LogError("[UICharactersPanel] stageStarPrefab이 할당되지 않았습니다!");
            }

            if (charactersDatabase != null && charactersDatabase.Characters != null)
            {
                for (int i = 0; i < charactersDatabase.Characters.Length; i++)
                {
                    if (charactersDatabase.Characters[i] == null) continue;
                    var newPanel = AddNewPanel();
                    if (newPanel != null)
                    {
                        newPanel.Init(charactersDatabase.Characters[i], this);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (stageStarPool != null)
            {
                PoolManager.DestroyPool(stageStarPool);
                stageStarPool = null;
            }
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            // OnDisable에서 이미 처리되므로 중복될 수 있지만, 안전을 위해 남겨둘 수 있습니다.
            // CharactersController.OnCharacterSelectedEvent -= OnCharacterSelected;
            // CharactersController.OnCharacterUpgradedEvent -= OnCharacterUpgraded;
        }

        public override void PlayShowAnimation()
        {
            ResetAnimations();
            base.PlayShowAnimation();
            StartAnimations();
            UpdateSelectedCharacterStatsDisplay(); // 페이지 표시 시 능력치 업데이트
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();
            if (backgroundPanelRectTransform != null)
            {
                 backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
                {
                    // CS0117 오류를 피하기 위해 UIController.IsInitialized 확인 제거
                    UIController.OnPageClosed(this);
                });
            }
            else
            {
                 // CS0117 오류를 피하기 위해 UIController.IsInitialized 확인 제거
                 UIController.OnPageClosed(this);
            }
        }

        protected override void HidePage(SimpleCallback onFinish)
        {
            // CS0117 오류를 피하기 위해 UIController.IsInitialized 확인 제거
            UIController.HidePage<UICharactersPanel>(onFinish);
            // 만약 UIController가 초기화되지 않은 상태에서 onFinish 콜백이 중요하다면,
            // else { onFinish?.Invoke(); } 와 같은 처리를 고려할 수 있으나,
            // 원본 코드의 직접 호출 방식을 따릅니다.
        }

        public override CharacterPanelUI GetPanel(CharacterData character)
        {
            if (character == null || itemPanels.IsNullOrEmpty()) return null;
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i] != null && itemPanels[i].Character == character)
                    return itemPanels[i];
            }
            return null;
        }
        #endregion
    }
}