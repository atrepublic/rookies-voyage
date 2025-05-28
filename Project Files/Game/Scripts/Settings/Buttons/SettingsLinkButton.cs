/*
📄 SettingsLinkButton.cs 요약 정리
이 스크립트는 SettingsButtonBase를 상속받아, 클릭 시 지정된 웹 URL을 여는 링크 버튼 기능을 구현합니다.

⭐ 주요 기능
- 인스펙터에서 설정 가능한 'url' 필드를 통해 열고자 하는 웹 페이지 주소를 지정받습니다.
- 사용자가 버튼을 클릭하면 Application.OpenURL() 메서드를 사용하여 해당 URL을 기본 웹 브라우저에서 엽니다.
- 버튼 클릭 시 지정된 사운드를 재생합니다.

🛠️ 사용 용도
- 게임 설정 UI 내에서 외부 웹사이트(예: 개발사 홈페이지, 커뮤니티 포럼, 특정 정보 페이지 등)로 사용자를 안내하는
  링크 버튼으로 사용됩니다.
*/

#pragma warning disable 0649 // 할당되지 않은 필드에 대한 경고를 비활성화합니다. (SerializeField로 Unity 에디터에서 할당됨)

using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 클릭 시 지정된 URL을 여는 링크 버튼 클래스입니다.
    /// SettingsButtonBase를 상속받습니다.
    /// </summary>
    public class SettingsLinkButton : SettingsButtonBase
    {
        [Tooltip("이 버튼을 클릭했을 때 열릴 웹 URL 주소입니다. 인스펙터에서 설정합니다.")]
        [SerializeField] string url;

        /// <summary>
        /// SettingsButtonBase로부터 상속받은 초기화 메서드입니다.
        /// 이 클래스에서는 특별한 초기화 로직이 필요하지 않아 비워둡니다.
        /// </summary>
        public override void Init()
        {
            // 이 링크 버튼 타입에 대한 특정 초기화 로직이 있다면 여기에 작성합니다.
        }

        /// <summary>
        /// 버튼이 클릭되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 구현)
        /// 지정된 URL을 웹 브라우저에서 열고 버튼 클릭 사운드를 재생합니다.
        /// </summary>
        public override void OnClick()
        {
            // url 필드에 지정된 웹 주소를 기본 웹 브라우저를 통해 엽니다.
            Application.OpenURL(url);

            // AudioController를 사용하여 버튼 클릭 사운드를 재생합니다.
            // AudioController와 AudioClips.buttonSound는 프로젝트의 사운드 시스템 및 오디오 클립으로 가정합니다.
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}