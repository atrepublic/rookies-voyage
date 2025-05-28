//====================================================================================================
// 해당 스크립트: UIGameOver.cs
// 기능: 게임 오버 시 결과 화면 UI를 관리하고 표시합니다.
// 용도: 플레이어가 게임 오버되었을 때 부활하거나 레벨을 다시 시작할 수 있는 옵션을 제공하는 팝업 UI를 표시합니다.
//====================================================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [Tooltip("배경의 점 애니메이션을 관리하는 DotsBackground 컴포넌트입니다.")]
        [SerializeField] private DotsBackground dotsBackground;
        [Tooltip("게임 오버 콘텐츠의 투명도를 조절하는 CanvasGroup 컴포넌트입니다.")]
        [SerializeField] private CanvasGroup contentCanvasGroup;

        [Space]
        [Tooltip("보상형 광고 시청 후 부활하는 버튼입니다.")]
        [SerializeField] private RewardedVideoButton reviveButton;
        [Tooltip("게임을 다시 시작하는 버튼입니다.")]
        [SerializeField] private Button continueButton;
        [Tooltip("'탭하여 계속' 게임패드 버튼 컴포넌트입니다.")]
        [SerializeField] private UIGamepadButton tapToContinueGamepadButton;

        [Space]
        [Tooltip("'탭하여 계속' 텍스트를 표시하는 TextMeshPro 텍스트 컴포넌트입니다.")]
        [SerializeField] private TMP_Text tapToContinueText;

        /// <summary>
        /// UI 게임 오버 패널을 초기화하는 함수입니다.
        /// 부활 버튼을 초기화하고 '계속' 버튼 클릭 이벤트를 설정합니다.
        /// </summary>
        public override void Init()
        {
            // 부활 버튼 초기화 (부활 함수와 가격 전달)
            reviveButton.Init(Revive, GameSettings.GetSettings().RevivePrice);

            // '계속' 버튼 클릭 이벤트에 다시 시작 함수 연결
            continueButton.onClick.AddListener(Replay);
        }

        #region Show/Hide
        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 배경 애니메이션, 콘텐츠 페이드 인, 버튼 활성화 및 게임패드 태그 설정을 처리합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            dotsBackground.ApplyParams(); // 배경 애니메이션 파라미터 적용

            contentCanvasGroup.alpha = 0.0f; // 콘텐츠 투명도 0으로 설정
            contentCanvasGroup.DOFade(1.0f, 0.4f).SetDelay(0.1f); // 콘텐츠 페이드 인 애니메이션 (딜레이 적용)

            dotsBackground.BackgroundImage.color = Color.white.SetAlpha(0.0f); // 배경 이미지 투명도 0으로 설정
            dotsBackground.BackgroundImage.DOFade(1.0f, 0.5f).OnComplete(delegate
            {
                reviveButton.enabled = true; // 부활 버튼 활성화

                UIController.OnPageOpened(this); // UI 컨트롤러에 페이지 열림 이벤트 알림
                UIGamepadButton.EnableTag(UIGamepadButtonTag.GameOver); // 게임 오버 페이지 관련 게임패드 버튼 태그 활성화
            });

            continueButton.enabled = false; // '계속' 버튼 초기 비활성화
            reviveButton.enabled = false; // 부활 버튼 초기 비활성화 (애니메이션 완료 후 활성화)

            tapToContinueText.alpha = 0; // '탭하여 계속' 텍스트 투명도 0으로 설정
            // '탭하여 계속' 텍스트 페이드 인 애니메이션 (딜레이 및 반복 적용)
            tapToContinueText.DOFade(1, 0.5f, 3).OnComplete(() => {
                continueButton.enabled = true; // 애니메이션 완료 후 '계속' 버튼 활성화
                tapToContinueGamepadButton.SetFocus(true); // '탭하여 계속' 게임패드 버튼에 포커스 설정
            });

            // 게임 관련 게임패드 버튼 태그 비활성화
            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// 콘텐츠 페이드 아웃, 배경 색상 변경 애니메이션을 처리하고 완료 시 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            contentCanvasGroup.DOFade(0.0f, 0.2f); // 콘텐츠 페이드 아웃 애니메이션

            // 배경 이미지 색상 변경 애니메이션 (검은색으로) 및 완료 시 페이지 닫힘 처리
            dotsBackground.BackgroundImage.DOColor(Color.black, 0.3f).OnComplete(delegate
            {
                tapToContinueGamepadButton.SetFocus(false); // '탭하여 계속' 게임패드 버튼 포커스 해제

                UIController.OnPageClosed(this); // UI 컨트롤러에 페이지 닫힘 이벤트 알림
            });
        }
        #endregion

        #region Buttons
        /// <summary>
        /// 레벨을 다시 시작하는 함수입니다.
        /// 버튼 클릭 사운드를 재생하고 게임 컨트롤러에 레벨 다시 시작 이벤트 알림을 보냅니다.
        /// 이 함수는 '계속' 버튼 클릭 시 호출됩니다.
        /// </summary>
        public void Replay()
        {
            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // 게임 컨트롤러에 레벨 다시 시작 이벤트 알림
            GameController.OnReplayLevel();
        }

        /// <summary>
        /// 플레이어를 부활시키는 함수입니다.
        /// 보상형 광고 시청 성공 여부에 따라 부활 또는 레벨 다시 시작을 처리합니다.
        /// 이 함수는 부활 버튼 클릭 시 호출됩니다.
        /// </summary>
        /// <param name="success">보상형 광고 시청 성공 여부</param>
        public void Revive(bool success)
        {
            // 광고 시청 성공 시 부활, 실패 시 레벨 다시 시작
            if (success)
            {
                GameController.OnRevive(); // 게임 컨트롤러에 부활 이벤트 알림
            }
            else
            {
                GameController.OnReplayLevel(); // 게임 컨트롤러에 레벨 다시 시작 이벤트 알림
            }
        }
        #endregion
    }
}