/*
📄 SettingsPrivacyButton.cs 요약 정리
이 스크립트는 SettingsButtonBase를 상속받아, 개인정보 처리방침(Privacy Policy) 페이지로 연결되는 버튼 기능을 구현합니다.
버튼의 활성화 여부 및 연결될 URL은 Monetization 모듈의 설정에 따라 동적으로 결정될 수 있습니다.

⭐ 주요 기능
- MODULE_MONETIZATION 전처리기 지시문을 사용하여 Monetization 모듈의 활성화 여부에 따라 동작이 변경됩니다.
- Monetization 모듈이 활성화되어 있고 개인정보 처리방침 링크가 설정되어 있으면 해당 URL을 사용합니다.
  - 링크가 없거나 모듈이 비활성화된 경우, 버튼 자체가 비활성화됩니다.
- 사용자가 버튼을 클릭하면 저장된 URL을 기본 웹 브라우저에서 엽니다.
- 게임패드 등으로 버튼이 선택/선택 해제될 때, Unity의 EventSystem을 직접 제어하여
  선택 상태를 명확히 관리하는 커스텀 로직을 포함합니다.

🛠️ 사용 용도
- 게임 설정 UI에서 사용자의 개인정보 처리방침을 안내하는 링크 버튼으로 사용됩니다.
- 특히 광고나 인앱 결제 등 Monetization 기능과 연관된 개인정보 처리방침을 제공해야 할 때 유용합니다.
*/

using UnityEngine;
using UnityEngine.EventSystems; // EventSystem 사용을 위해 필요합니다.

namespace Watermelon
{
    /// <summary>
    /// 개인정보 처리방침(Privacy Policy) 페이지로 연결되는 버튼 클래스입니다.
    /// SettingsButtonBase를 상속받으며, Monetization 모듈의 설정에 따라 URL 및 활성화 상태가 결정될 수 있습니다.
    /// </summary>
    public class SettingsPrivacyButton : SettingsButtonBase
    {
        // 개인정보 처리방침 페이지의 URL을 저장하는 변수입니다. Init() 메서드에서 설정됩니다.
        private string url;

        /// <summary>
        /// SettingsButtonBase로부터 상속받은 초기화 메서드입니다.
        /// Monetization 모듈의 상태에 따라 URL을 설정하고 버튼의 활성화 여부를 결정합니다.
        /// </summary>
        public override void Init()
        {
// MODULE_MONETIZATION 전처리기 심볼이 정의되어 있는 경우에만 아래 코드를 컴파일합니다.
#if MODULE_MONETIZATION
            // Monetization 모듈이 활성화 상태인지 확인합니다. (Monetization은 정적 클래스 또는 싱글톤으로 가정)
            if(Monetization.IsActive)
            {
                // Monetization 설정에서 개인정보 처리방침 링크(PrivacyLink)를 가져옵니다.
                url = Monetization.Settings.PrivacyLink;
                // URL이 비어있거나 null이면 이 버튼 게임 오브젝트를 비활성화합니다.
                if(string.IsNullOrEmpty(url))
                    gameObject.SetActive(false);
            }
            else // Monetization 모듈이 활성화되어 있지 않은 경우
            {
                gameObject.SetActive(false); // 버튼을 비활성화합니다.
            }
#else // MODULE_MONETIZATION 심볼이 정의되어 있지 않은 경우
            // Monetization 모듈 자체가 없으므로 버튼을 비활성화합니다.
            gameObject.SetActive(false);
#endif
        }

        /// <summary>
        /// 버튼이 클릭되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 구현)
        /// URL이 유효하면 해당 URL을 웹 브라우저에서 열고 버튼 클릭 사운드를 재생합니다.
        /// </summary>
        public override void OnClick()
        {
            // URL이 비어있거나 null이면 아무 작업도 수행하지 않고 반환합니다.
            if (string.IsNullOrEmpty(url)) return;

            // 저장된 URL을 기본 웹 브라우저를 통해 엽니다.
            Application.OpenURL(url);

            // AudioController를 사용하여 버튼 클릭 사운드를 재생합니다.
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        /// <summary>
        /// 이 버튼이 선택되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 재정의)
        /// 선택 상태를 업데이트하고, Unity의 EventSystem을 사용하여 이 버튼을 명시적으로 선택된 게임 오브젝트로 설정합니다.
        /// </summary>
        public override void Select()
        {
            base.IsSelected = true; // IsSelected를 직접 설정 (원본 코드에서는 base.Select()가 아님)

            // 연결된 UnityEngine.UI.Button 컴포넌트의 Select() 메서드를 호출하여
            // 버튼이 네비게이션 시스템 상에서도 선택된 상태로 처리되도록 합니다.
            if (Button != null) // 원본에는 없지만, 안전을 위해 추가 (주석처리: 원본 유지)
            {
                 Button.Select();
            }


            // EventSystem에서 이전에 선택된 게임 오브젝트를 해제합니다 (권장 사항).
            EventSystem.current.SetSelectedGameObject(null);
            // 이 버튼의 게임 오브젝트를 EventSystem의 현재 선택된 게임 오브젝트로 설정합니다.
            EventSystem.current.SetSelectedGameObject(Button.gameObject, new BaseEventData(EventSystem.current));
        }

        /// <summary>
        /// 이 버튼이 선택 해제되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 재정의)
        /// 선택 상태를 업데이트하고, EventSystem에서 현재 선택된 게임 오브젝트를 해제합니다.
        /// </summary>
        public override void Deselect()
        {
            base.IsSelected = false; // IsSelected를 직접 설정 (원본 코드에서는 base.Deselect()가 아님)

            // EventSystem에서 현재 선택된 게임 오브젝트를 null로 설정하여 선택을 해제합니다.
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}