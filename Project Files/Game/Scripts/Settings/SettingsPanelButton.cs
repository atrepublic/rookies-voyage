/*
📄 SettingsPanelButton.cs 요약 정리
이 스크립트는 UI 버튼에 부착되어, 해당 버튼을 클릭했을 때
설정(Settings) UI 페이지를 열고 버튼 클릭 사운드를 재생하는 기능을 수행합니다.

⭐ 주요 기능
- UnityEngine.UI.Button 컴포넌트에 대한 참조를 자동으로 가져옵니다.
- 버튼 클릭 이벤트(onClick)에 자체 OnClick 메서드를 리스너로 등록합니다.
- OnClick 메서드가 호출되면, UIController를 통해 UISettings 페이지를 표시하도록 요청합니다.
- 버튼 클릭 시 지정된 사운드(buttonSound)를 AudioController를 통해 재생합니다.

🛠️ 사용 용도
- 게임 내에서 설정 화면으로 진입하는 모든 버튼(예: 메인 메뉴의 "설정" 버튼, 게임 중 "일시정지 메뉴"의 "설정" 버튼 등)에 이 스크립트를 부착하여 사용합니다.
*/

using UnityEngine;
using UnityEngine.UI; // UnityEngine.UI 네임스페이스를 사용하기 위해 필요합니다. (Button 클래스 등)

namespace Watermelon
{
    /// <summary>
    /// 이 컴포넌트가 부착된 게임 오브젝트에는 반드시 UnityEngine.UI.Button 컴포넌트가 있어야 함을 나타냅니다.
    /// Button 컴포넌트가 없으면 Unity 에디터에서 경고를 표시합니다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    /// <summary>
    /// 설정(Settings) UI 패널을 여는 기능을 하는 버튼에 사용되는 스크립트입니다.
    /// 클릭 시 설정 UI를 표시하고 클릭 사운드를 재생합니다.
    /// </summary>
    public class SettingsPanelButton : MonoBehaviour
    {
        /// <summary>
        /// 이 스크립트가 제어하는 UnityEngine.UI.Button 컴포넌트입니다.
        /// Awake 시점에 자동으로 할당되며, 외부에서는 읽기만 가능합니다.
        /// </summary>
        [Tooltip("이 게임오브젝트에 연결된 Unity UI Button 컴포넌트입니다. 클릭 이벤트를 처리합니다.")]
        public Button Button { get; private set; }

        /// <summary>
        /// Unity 생명주기 메서드: 스크립트 인스턴스가 로드될 때 호출됩니다.
        /// Button 컴포넌트를 가져오고, 클릭 이벤트에 리스너를 등록합니다.
        /// </summary>
        private void Awake()
        {
            // 이 게임 오브젝트에 부착된 Button 컴포넌트를 가져와 Button 프로퍼티에 할당합니다.
            Button = GetComponent<Button>();
            // Button 컴포넌트의 onClick 이벤트에 OnClick 메서드를 리스너로 추가합니다.
            // 이렇게 하면 사용자가 이 UI 버튼을 클릭할 때마다 OnClick 메서드가 호출됩니다.
            if (Button != null) // 안전을 위해 Button이 null이 아닌지 확인
            {
                Button.onClick.AddListener(OnClick);
            }
        }

        /// <summary>
        /// 이 UI 버튼이 클릭되었을 때 호출되는 메서드입니다.
        /// 설정 UI 페이지를 표시하고 버튼 클릭 사운드를 재생합니다.
        /// </summary>
        private void OnClick()
        {
            // UIController를 사용하여 UISettings 타입의 UI 페이지를 화면에 표시하도록 요청합니다.
            // UIController와 UISettings는 Watermelon 프레임워크 또는 프로젝트의 일부로 가정합니다.
            UIController.ShowPage<UISettings>();

            // AudioController를 사용하여 지정된 버튼 클릭 사운드를 재생합니다.
            // AudioController와 AudioClips.buttonSound는 사운드 재생 시스템의 일부로 가정합니다.
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        /// <summary>
        /// Unity 생명주기 메서드: 게임 오브젝트가 파괴될 때 호출됩니다.
        /// 등록된 리스너를 해제하여 메모리 누수를 방지합니다. (선택적이지만 좋은 습관)
        /// </summary>
        private void OnDestroy()
        {
            if (Button != null)
            {
                Button.onClick.RemoveListener(OnClick);
            }
        }
    }
}