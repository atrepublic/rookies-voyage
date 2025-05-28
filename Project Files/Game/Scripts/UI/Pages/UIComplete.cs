//====================================================================================================
// 해당 스크립트: UIComplete.cs
// 기능: 레벨 완료 시 결과 화면 UI를 관리하고 표시합니다.
// 용도: 플레이어가 레벨을 클리어했을 때 획득한 보상(경험치, 돈, 카드)을 보여주고, 다음 단계로
//      진행할 수 있는 '계속' 버튼을 제공합니다.
//====================================================================================================
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Watermelon.SquadShooter;
using UnityEngine.UI;
using System;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        private const string LEVEL_TEXT = "LEVEL {0}-{1}"; // 레벨 표시 형식 문자열
        private const string PLUS_TEXT = "+{0}"; // 획득량 표시 형식 문자열

        [Tooltip("배경의 점 애니메이션을 관리하는 DotsBackground 컴포넌트입니다.")]
        [SerializeField] private DotsBackground dotsBackground;
        [Tooltip("완료 패널의 RectTransform입니다.")]
        [SerializeField] private RectTransform panelRectTransform;

        [Tooltip("패널 콘텐츠의 투명도를 조절하는 CanvasGroup 컴포넌트입니다.")]
        [SerializeField] private CanvasGroup panelContentCanvasGroup;
        [Tooltip("현재 레벨 정보를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Space]
        [Tooltip("드롭된 카드 UI 프리팹입니다.")]
        [SerializeField] private GameObject dropCardPrefab;
        [Tooltip("드롭된 카드 UI가 배치될 컨테이너 Transform입니다.")]
        [SerializeField] private Transform cardsContainerTransform;

        [Space]
        [Tooltip("획득한 경험치를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI experienceGainedText;
        [Tooltip("획득한 돈을 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI moneyGainedText;

        [Space]
        [Tooltip("다음 단계로 진행하는 버튼입니다.")]
        [SerializeField] private Button continueButton;

        private int currentWorld; // 현재 월드 번호
        private int currentLevel; // 현재 레벨 번호
        private int collectedMoney; // 획득한 돈
        private int collectedExperience; // 획득한 경험치
        private List<WeaponData> collectedCards; // 획득한 카드 목록

        private Pool cardsUIPool; // 드롭된 카드 UI 오브젝트 풀

        /// <summary>
        /// UI 완료 패널을 초기화하는 함수입니다.
        /// 카드 UI 오브젝트 풀을 생성하고 '계속' 버튼 클릭 이벤트를 설정합니다.
        /// </summary>
        public override void Init()
        {
            // 드롭된 카드 UI 오브젝트 풀 생성
            cardsUIPool = new Pool(dropCardPrefab, dropCardPrefab.name, cardsContainerTransform);

            // '계속' 버튼 클릭 이벤트에 핸들러 함수 연결
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        /// <summary>
        /// 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// 생성된 카드 UI 오브젝트 풀을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 카드 UI 오브젝트 풀이 존재하면 파괴
            if(cardsUIPool != null)
            {
                PoolManager.DestroyPool(cardsUIPool);
            }
        }

        /// <summary>
        /// 완료 패널에 표시될 데이터를 설정하는 함수입니다.
        /// 현재 레벨 정보 및 획득한 보상 데이터를 받습니다.
        /// </summary>
        /// <param name="currentWorld">현재 월드 번호</param>
        /// <param name="currentLevel">현재 레벨 번호</param>
        /// <param name="collectedMoney">획득한 돈</param>
        /// <param name="collectedExperience">획득한 경험치</param>
        /// <param name="collectedCards">획득한 카드 목록</param>
        public void SetData(int currentWorld, int currentLevel, int collectedMoney, int collectedExperience, List<WeaponData> collectedCards)
        {
            this.currentWorld = currentWorld; // 현재 월드 번호 설정
            this.currentLevel = currentLevel; // 현재 레벨 번호 설정
            this.collectedMoney = collectedMoney; // 획득한 돈 설정
            this.collectedExperience = collectedExperience; // 획득한 경험치 설정
            this.collectedCards = collectedCards; // 획득한 카드 목록 설정
        }

        #region Show/Hide
        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 배경 애니메이션, 패널 콘텐츠 페이드 인, 획득 보상 숫자 애니메이션, 드롭된 카드 표시 등을 처리합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            float showTime = 0.7f; // 기본 표시 애니메이션 시간

            dotsBackground.ApplyParams(); // 배경 애니메이션 파라미터 적용

            cardsUIPool.ReturnToPoolEverything(); // 풀링된 카드 UI 오브젝트 모두 반환

            continueButton.interactable = false; // 초기에는 '계속' 버튼 비활성화

            // 초기 UI 요소 상태 리셋
            panelRectTransform.sizeDelta = new Vector2(0, 335f); // 패널 초기 높이 설정
            dotsBackground.BackgroundImage.color = Color.white.SetAlpha(0.0f); // 배경 이미지 투명도 0으로 설정
            panelContentCanvasGroup.alpha = 0; // 패널 콘텐츠 투명도 0으로 설정

            // 레벨 정보 텍스트 업데이트
            levelText.text = string.Format(LEVEL_TEXT, currentWorld, currentLevel);

            // 배경 이미지 색상 애니메이션 및 패널 콘텐츠 페이드 인 애니메이션
            dotsBackground.BackgroundImage.DOColor(Color.white, 0.1f);
            panelContentCanvasGroup.DOFade(1.0f, 0.3f, 0.1f);

            // 획득 돈 숫자 애니메이션
            moneyGainedText.text = "0";
            Tween.DoFloat(0, collectedMoney, 0.4f, (result) =>
            {
                moneyGainedText.text = string.Format(PLUS_TEXT, result.ToString("00"));
            }, 0.2f);

            // 획득 경험치 숫자 애니메이션
            experienceGainedText.text = "0";
            Tween.DoFloat(0, collectedExperience, 0.4f, (result) =>
            {
                experienceGainedText.text = string.Format(PLUS_TEXT, result.ToString("00"));
            }, 0.3f);

            // 드롭된 카드가 있는지 확인
            bool cardsDropped = !collectedCards.IsNullOrEmpty();
            if(cardsDropped)
            {
                // 중복되지 않는 획득 카드 목록 생성
                List<WeaponData> uniqueCards = new List<WeaponData>();
                for(int i = 0; i < collectedCards.Count; i++)
                {
                    if(uniqueCards.FindIndex(x => x == collectedCards[i]) == -1)
                    {
                        uniqueCards.Add(collectedCards[i]);
                    }
                }

                // 각 획득 카드에 대해 UI 오브젝트 생성 및 초기화, 애니메이션 실행
                for (int i = 0; i < uniqueCards.Count; i++)
                {
                    GameObject cardUIObject = cardsUIPool.GetPooledObject(); // 풀에서 카드 UI 오브젝트 가져오기
                    cardUIObject.SetActive(true); // 오브젝트 활성화

                    DroppedCardPanel droppedCardPanel = cardUIObject.GetComponent<DroppedCardPanel>(); // DroppedCardPanel 컴포넌트 가져오기
                    droppedCardPanel.Init(uniqueCards[i]); // 카드 데이터로 패널 초기화

                    CanvasGroup droppedCardCanvasGroup = droppedCardPanel.CanvasGroup; // 카드 패널의 CanvasGroup 가져오기
                    droppedCardCanvasGroup.alpha = 0.0f; // 초기 투명도 0으로 설정
                    // 카드 패널 페이드 인 애니메이션 (딜레이 적용) 및 표시 완료 시 OnDisplayed 호출
                    droppedCardCanvasGroup.DOFade(1.0f, 0.5f, 0.1f * i + 0.45f).OnComplete(delegate
                    {
                        droppedCardPanel.OnDisplayed();
                    });
                }

                // 드롭된 카드가 있을 경우 패널 높이 애니메이션
                panelRectTransform.DOSize(new Vector2(0, 815), 0.4f).SetEasing(Ease.Type.BackOut);

                showTime = 1.1f; // 드롭된 카드가 있으면 표시 애니메이션 시간 증가
            }

            // 지정된 시간 후 페이지 열림 처리 및 '계속' 버튼 활성화
            Tween.DelayedCall(showTime, () => {
                UIController.OnPageOpened(this); // UI 컨트롤러에 페이지 열림 이벤트 알림

                UIGamepadButton.EnableTag(UIGamepadButtonTag.Complete); // 완료 페이지 관련 게임패드 버튼 태그 활성화

                continueButton.interactable = true; // '계속' 버튼 상호작용 가능하도록 설정
            });

            // 게임 관련 게임패드 버튼 태그 비활성화
            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다. (현재는 즉시 닫힘)
        /// 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            // UI 컨트롤러에 페이지 닫힘 이벤트 알림
            UIController.OnPageClosed(this);
        }

        #endregion

        #region Experience
        /// <summary>
        /// 경험치 표시 레이블을 업데이트하는 함수입니다.
        /// </summary>
        /// <param name="experienceGained">업데이트할 경험치 값</param>
        public void UpdateExperienceLabel(int experienceGained)
        {
            experienceGainedText.text = experienceGained.ToString(); // 경험치 텍스트 업데이트
        }

        #endregion

        #region Buttons
        /// <summary>
        /// '계속' 버튼 클릭 시 호출되는 함수입니다.
        /// 버튼 클릭 사운드를 재생하고 레벨 완료 닫힘 이벤트를 호출합니다.
        /// </summary>
        private void OnContinueButtonClicked()
        {
            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // GameController에 레벨 완료 닫힘 이벤트 알림
            GameController.OnLevelCompleteClosed();
        }
        #endregion
    }
}