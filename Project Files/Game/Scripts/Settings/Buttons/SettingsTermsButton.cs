using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    /// <summary>
    ///   설정 이용 약관 버튼 스크립트.
    ///   이용 약관 링크를 열어주는 기능을 처리합니다.
    /// </summary>
    public class SettingsTermsButton : SettingsButtonBase
    {
        /// <summary>
        ///   이용 약관 링크 URL.
        /// </summary>
        [Tooltip("이용 약관 링크 URL")]
        private string url;

        /// <summary>
        ///   초기화 함수.
        ///   Monetization 모듈 활성화 여부에 따라 이용 약관 링크를 설정하고,
        ///   링크가 없으면 게임 오브젝트를 비활성화합니다.
        /// </summary>
        public override void Init()
        {
#if MODULE_MONETIZATION
            if (Monetization.IsActive)
            {
                url = Monetization.Settings.TermsOfUseLink;
                if (string.IsNullOrEmpty(url))
                    gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
#else
            gameObject.SetActive(false);
#endif
        }

        /// <summary>
        ///   버튼 클릭 시 호출되는 함수.
        ///   이용 약관 링크가 있으면 해당 URL을 엽니다.
        /// </summary>
        public override void OnClick()
        {
            if (string.IsNullOrEmpty(url)) return;

            Application.OpenURL(url);

            // 버튼 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        /// <summary>
        ///   버튼 선택 시 호출되는 함수.
        ///   버튼을 선택 상태로 변경하고, 현재 선택된 게임 오브젝트를 설정합니다.
        /// </summary>
        public override void Select()
        {
            IsSelected = true;

            // 이전 선택을 지우고 현재 버튼을 선택
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(Button.gameObject, new BaseEventData(EventSystem.current));
        }

        /// <summary>
        ///   버튼 선택 해제 시 호출되는 함수.
        ///   버튼을 비선택 상태로 변경합니다.
        /// </summary>
        public override void Deselect()
        {
            IsSelected = false;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}