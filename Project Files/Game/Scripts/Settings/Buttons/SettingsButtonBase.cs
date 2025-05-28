/*
📄 SettingsButtonBase.cs 요약 정리
이 스크립트는 설정 UI에서 사용되는 다양한 버튼들의 공통 기능을 정의하는 추상 기본 클래스입니다.
모든 설정 버튼들은 이 클래스를 상속받아 구현됩니다.

⭐ 주요 기능
- 각 버튼의 RectTransform과 UnityEngine.UI.Button 컴포넌트에 대한 참조를 자동으로 관리합니다.
- 버튼의 선택 상태(IsSelected)를 추적합니다.
- Awake() 메서드에서 기본적인 초기화(컴포넌트 가져오기, 클릭 리스너 연결)를 수행합니다.
- 하위 클래스에서 구체적인 초기화 로직을 구현할 수 있도록 추상 메서드 Init()을 제공합니다.
- 하위 클래스에서 버튼 클릭 시의 특정 동작을 구현할 수 있도록 추상 메서드 OnClick()을 제공합니다.
- 게임패드 등으로 버튼을 선택하거나 선택 해제할 때 호출되는 가상 메서드 Select()와 Deselect()를 제공합니다.

🛠️ 사용 용도
- 설정 화면 내의 모든 종류의 버튼(예: 토글 버튼, 링크 버튼, 개인정보처리방침 버튼 등)을 만들 때
  이 클래스를 부모 클래스로 사용하여 공통된 초기화 및 상태 관리 로직을 재사용합니다.
*/

using UnityEngine;
using UnityEngine.UI; // Button 컴포넌트 사용을 위해 필요합니다.

namespace Watermelon
{
    /// <summary>
    /// 설정 UI에서 사용되는 모든 버튼의 기본 추상 클래스입니다.
    /// 공통적인 초기화, 선택 상태 관리 및 UI 요소 참조 기능을 제공합니다.
    /// </summary>
    public abstract class SettingsButtonBase : MonoBehaviour
    {
        /// <summary>
        /// 이 버튼의 RectTransform 컴포넌트입니다. UI 위치 및 크기 조절에 사용됩니다.
        /// Awake 시점에 자동으로 할당됩니다.
        /// </summary>
        public RectTransform RectTransform { get; protected set; }

        /// <summary>
        /// 이 버튼의 UnityEngine.UI.Button 컴포넌트입니다. 클릭 이벤트 처리에 사용됩니다.
        /// Awake 시점에 자동으로 할당됩니다.
        /// </summary>
        public Button Button { get; protected set; }

        /// <summary>
        /// 이 버튼이 현재 선택된 상태인지 여부를 나타냅니다.
        /// 주로 게임패드 네비게이션 시 시각적 피드백에 사용됩니다.
        /// </summary>
        public bool IsSelected { get; protected set; }

        /// <summary>
        /// Unity 생명주기 메서드: 스크립트 인스턴스가 로드될 때 호출됩니다.
        /// RectTransform과 Button 컴포넌트를 가져오고, 클릭 리스너를 연결하며,
        /// 초기 선택 상태를 설정하고, 하위 클래스의 Init() 메서드를 호출합니다.
        /// </summary>
        private void Awake()
        {
            // 현재 게임 오브젝트의 Transform을 RectTransform으로 캐스팅하여 할당합니다.
            RectTransform = (RectTransform)transform;

            // 이 게임 오브젝트에 부착된 Button 컴포넌트를 가져옵니다.
            Button = GetComponent<Button>();
            // Button 컴포넌트의 onClick 이벤트에 이 클래스의 OnClick 메서드(하위 클래스에서 구현됨)를 리스너로 추가합니다.
            if (Button != null) // Button 컴포넌트가 없는 경우를 대비한 방어 코드 (주석 처리됨: 원본 유지)
            {
                Button.onClick.AddListener(OnClick);
            }


            // 초기 선택 상태를 false로 설정합니다.
            IsSelected = false;

            // 초기 시각적 상태를 선택되지 않은 상태로 설정하기 위해 Deselect()를 호출합니다.
            Deselect();

            // 하위 클래스에서 정의된 특정 초기화 로직을 호출합니다.
            Init();
        }

        /// <summary>
        /// 하위 클래스에서 구체적인 초기화 로직을 구현해야 하는 추상 메서드입니다.
        /// Awake()의 마지막 단계에서 호출됩니다.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 하위 클래스에서 버튼 클릭 시 수행할 특정 동작을 구현해야 하는 추상 메서드입니다.
        /// Button 컴포넌트의 onClick 이벤트에 연결됩니다.
        /// </summary>
        public abstract void OnClick();

        /// <summary>
        /// 이 버튼이 선택되었을 때 호출되는 가상 메서드입니다.
        /// IsSelected 상태를 true로 설정합니다. 하위 클래스에서 시각적 변경 등을 추가로 구현할 수 있습니다.
        /// </summary>
        public virtual void Select()
        {
            IsSelected = true;
        }

        /// <summary>
        /// 이 버튼이 선택 해제되었을 때 호출되는 가상 메서드입니다.
        /// IsSelected 상태를 false로 설정합니다. 하위 클래스에서 시각적 변경 해제 등을 추가로 구현할 수 있습니다.
        /// </summary>
        public virtual void Deselect()
        {
            IsSelected = false;
        }
    }
}