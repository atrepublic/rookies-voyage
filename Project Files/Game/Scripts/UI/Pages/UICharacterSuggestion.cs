//====================================================================================================
// 해당 스크립트: UICharacterSuggestion.cs
// 기능: 새로운 캐릭터 잠금 해제 또는 캐릭터 진행 상황을 보여주는 제안 UI를 관리합니다.
// 용도: 플레이어가 특정 캐릭터의 잠금 해제에 가까워지거나 잠금 해제했을 때 이를 알려주고
//      진행 상황을 시각적으로 표시하는 팝업 UI를 제공합니다.
//====================================================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UICharacterSuggestion : UIPage
    {
        [Tooltip("제안 패널의 RectTransform입니다.")]
        [SerializeField] private RectTransform panelRectTransform;
        [Tooltip("다음 캐릭터 제안 텍스트 게임 오브젝트입니다.")]
        [SerializeField] private GameObject nextCharacterText;
        [Tooltip("캐릭터 잠금 해제 완료 텍스트 게임 오브젝트입니다.")]
        [SerializeField] private GameObject characterUnlockedText;
        [Tooltip("캐릭터 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] private Image characterImage;
        [Tooltip("진행 상황 막대 게임 오브젝트입니다.")]
        [SerializeField] private GameObject fillbarObject;
        [Tooltip("캐릭터 이미지 위에 겹쳐져서 진행 상황을 나타내는 Image 컴포넌트입니다.")]
        [SerializeField] private Image characterFillImage;
        [Tooltip("캐릭터 잠금 해제 시 표시되는 빛 줄기 이미지 게임 오브젝트입니다.")]
        [SerializeField] private GameObject lightBeamImage;
        [Tooltip("진행 상황을 시각적으로 채우는 SlicedFilledImage 컴포넌트입니다.")]
        [SerializeField] private SlicedFilledImage fillbarImage;
        [Tooltip("캐릭터 잠금 해제 시 표시되는 'Unlocked' 텍스트 게임 오브젝트입니다.")]
        [SerializeField] private GameObject unlockedTextObject;
        [Tooltip("계속 진행 텍스트 게임 오브젝트입니다.")]
        [SerializeField] private GameObject continueText;
        [Tooltip("진행 상황 퍼센티지를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI persentageText;

        private static float lastProgression; // 이전 진행 상황 값 (0 ~ 1)
        private static float currentProgression; // 현재 진행 상황 값 (0 ~ 1)
        private static CharacterData characterData; // 제안 대상 캐릭터 데이터

        /// <summary>
        /// UI 페이지 초기화 함수입니다. (현재는 특별한 초기화 로직 없음)
        /// </summary>
        public override void Init()
        {
            // 초기화 로직 없음
        }

        /// <summary>
        /// 제안 UI에 표시될 데이터를 설정하는 함수입니다.
        /// 이전 진행 상황, 현재 진행 상황, 대상 캐릭터 데이터를 받습니다.
        /// </summary>
        /// <param name="lastProgression">이전 진행 상황 값 (0 ~ 1)</param>
        /// <param name="currentProgression">현재 진행 상황 값 (0 ~ 1)</param>
        /// <param name="characterData">제안 대상 캐릭터 데이터</param>
        public static void SetData(float lastProgression, float currentProgression, CharacterData characterData)
        {
            UICharacterSuggestion.lastProgression = lastProgression; // 이전 진행 상황 값 설정
            UICharacterSuggestion.currentProgression = currentProgression; // 현재 진행 상황 값 설정
            UICharacterSuggestion.characterData = characterData; // 대상 캐릭터 데이터 설정
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 패널 크기 조절, 이미지 설정, 진행 상황 채우기 애니메이션 등을 처리합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            // 초기 UI 요소 상태 설정
            nextCharacterText.SetActive(true); // "다음 캐릭터" 텍스트 활성화
            characterUnlockedText.SetActive(false); // "캐릭터 잠금 해제" 텍스트 비활성화
            lightBeamImage.SetActive(false); // 빛 줄기 이미지 비활성화
            unlockedTextObject.SetActive(false); // "Unlocked" 텍스트 비활성화
            continueText.SetActive(false); // "계속" 텍스트 비활성화
            fillbarObject.SetActive(true); // 진행 상황 막대 활성화

            // 패널의 초기 높이 설정
            panelRectTransform.sizeDelta = new Vector2(0, 335f);

            // 캐릭터 이미지 및 채우기 이미지 스프라이트 설정
            characterImage.sprite = characterData.Stages[characterData.Stages.Length - 1].PreviewSprite; // 캐릭터 미리보기 스프라이트
            characterFillImage.sprite = characterData.Stages[characterData.Stages.Length - 1].LockedSprite; // 잠금 상태 스프라이트

            // 초기 채우기 이미지 및 진행 막대 값 설정
            characterFillImage.fillAmount = 1f - lastProgression; // 채우기 이미지: 이전 진행 상황의 반대 값으로 설정
            fillbarImage.fillAmount = lastProgression; // 진행 막대: 이전 진행 상황 값으로 설정

            // 퍼센티지 텍스트 업데이트
            persentageText.text = ((int)(currentProgression * 100)).ToString() + "%";

            // 패널 높이 애니메이션 시작 (BackOut 보간)
            panelRectTransform.DOSize(new Vector2(0, 915), 0.4f).SetEasing(Ease.Type.BackOut).OnComplete(() =>
            {
                // 진행 막대 채우기 애니메이션 시작 (CubicOut 보간)
                fillbarImage.DOFillAmount(currentProgression, 0.6f).SetEasing(Ease.Type.CubicOut);
                // 캐릭터 채우기 이미지 채우기 애니메이션 시작 (CubicOut 보간)
                characterFillImage.DOFillAmount(1f - currentProgression, 0.6f).SetEasing(Ease.Type.CubicOut).OnComplete(() =>
                {
                    continueText.SetActive(true); // "계속" 텍스트 활성화

                    // 진행 상황이 100% (잠금 해제)인 경우 추가 애니메이션 및 UI 변경
                    if (currentProgression >= 1f)
                    {
                        nextCharacterText.SetActive(false); // "다음 캐릭터" 텍스트 비활성화
                        characterUnlockedText.SetActive(true); // "캐릭터 잠금 해제" 텍스트 활성화
                        lightBeamImage.SetActive(true); // 빛 줄기 이미지 활성화
                        unlockedTextObject.SetActive(true); // "Unlocked" 텍스트 활성화
                        continueText.SetActive(true); // "계속" 텍스트 활성화 유지
                        fillbarObject.SetActive(false); // 진행 상황 막대 비활성화

                        // 빛 줄기 이미지 스케일 애니메이션 시작 (BackOut 보간)
                        lightBeamImage.transform.localScale = Vector3.zero;
                        lightBeamImage.transform.DOScale(1f, 0.3f).SetEasing(Ease.Type.BackOut);
                    }

                    // 게임패드 버튼 태그 설정
                    UIGamepadButton.EnableTag(UIGamepadButtonTag.CharacterSuggestion);
                    // UI 컨트롤러에 페이지 열림 이벤트 알림
                    UIController.OnPageOpened(this);
                });
            });

            // 게임 관련 게임패드 버튼 태그 비활성화
            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            // UI 컨트롤러에 페이지 닫힘 이벤트 알림
            UIController.OnPageClosed(this);
        }

        #region Buttons
        /// <summary>
        /// '계속' 버튼 클릭 시 호출되는 함수입니다.
        /// 제안 페이지를 숨기고 메인 메뉴 페이지를 표시합니다.
        /// </summary>
        public void ContinueButton()
        {
            // UIController를 사용하여 UICharacterSuggestion 페이지 숨김
            UIController.HidePage<UICharacterSuggestion>();
            // UIController를 사용하여 UIMainMenu 페이지 표시
            UIController.ShowPage<UIMainMenu>();

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
        #endregion
    }
}