/*
📄 UIUpgradeAbstractPanel.cs 요약 정리
이 스크립트는 캐릭터 또는 무기 업그레이드 UI 패널의 공통적인 동작을 정의하는 추상 클래스입니다.
구체적인 업그레이드 패널(예: 캐릭터 업그레이드, 무기 업그레이드)을 만들 때 이 클래스를 상속받아 사용합니다.

⭐ 주요 기능
- 업그레이드 관련 정보(능력치 등)를 표시하는 UI 요소 관리 기능을 제공합니다.
- 패널 선택 상태에 따른 시각적 변화를 처리할 수 있는 기반을 제공합니다.
- 재화 변경 시 업그레이드 버튼의 상태를 갱신하는 로직을 포함합니다.
- 하위 클래스에서 구체적인 동작을 구현해야 하는 추상 메서드(Select)와 재정의 가능한 가상 메서드(RedrawUpgradeButton, OnPanelOpened)를 제공하여 확장성을 높입니다.

🛠️ 사용 용도
- 캐릭터 선택 및 업그레이드 UI 화면
- 무기 선택 및 업그레이드 UI 패널
- 기타 유사한 업그레이드 관련 UI 요소의 공통 로직을 처리하는 데 사용될 수 있습니다.
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// 업그레이드 UI 패널의 기본 동작을 정의하는 추상 클래스입니다.
    /// </summary>
    public abstract class UIUpgradeAbstractPanel : MonoBehaviour
    {
        [Header("능력치 표시")]
        [Tooltip("능력치 관련 UI 요소들의 부모 GameObject입니다. 활성화/비활성화 시 사용됩니다.")]
        [SerializeField] protected GameObject powerObject;
        [Tooltip("능력치 수치를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] protected TextMeshProUGUI powerText;

        [Header("선택 상태 표시")]
        [Tooltip("패널이 선택되었음을 나타내는 Image 컴포넌트입니다. 선택 시 활성화/색상 변경 등에 사용됩니다.")]
        [SerializeField] protected Image selectionImage;
        [Tooltip("패널 배경의 Transform 컴포넌트입니다. 선택 효과 등 시각적 변경에 사용될 수 있습니다.")]
        [SerializeField] protected Transform backgroundTransform;

        [Tooltip("이 UI 패널의 RectTransform 컴포넌트입니다. UI의 크기 및 위치를 제어합니다.")]
        protected RectTransform panelRectTransform;
        /// <summary>
        /// 이 UI 패널의 RectTransform 컴포넌트를 가져옵니다.
        /// </summary>
        public RectTransform RectTransform => panelRectTransform;

        /// <summary>
        /// 이 업그레이드가 현재 잠금 해제되었는지 여부를 나타냅니다.
        /// 하위 클래스에서 구체적인 잠금 해제 조건을 구현해야 합니다.
        /// </summary>
        public abstract bool IsUnlocked { get; }

        [Tooltip("현재 업그레이드 관련 애니메이션(예: 강화 연출)이 재생 중인지 여부를 나타냅니다.")]
        protected bool isUpgradeAnimationPlaying;

        /// <summary>
        /// MonoBehaviour의 Awake 메서드입니다.
        /// 주로 컴포넌트 초기화에 사용됩니다.
        /// </summary>
        protected virtual void Awake()
        {
            // panelRectTransform이 null일 경우에만 GetComponent를 호출하여 초기화합니다.
            // 이는 이미 외부에서 할당되었을 가능성을 고려한 것입니다.
            if (panelRectTransform == null)
            {
                panelRectTransform = GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// 사용자의 재화(돈) 보유량이 변경되었을 때 호출되는 함수입니다.
        /// 업그레이드 애니메이션이 재생 중이 아닐 경우, 업그레이드 버튼의 UI를 다시 그립니다.
        /// </summary>
        public void OnMoneyAmountChanged()
        {
            if (isUpgradeAnimationPlaying)
                return;

            RedrawUpgradeButton();
        }

        /// <summary>
        /// 업그레이드 버튼의 UI 상태(예: 구매 가능 여부, 비용 텍스트 등)를 갱신합니다.
        /// 이 메서드는 가상(virtual)이므로, 하위 클래스에서 특정 패널에 맞게 재정의할 수 있습니다.
        /// </summary>
        protected virtual void RedrawUpgradeButton()
        {
            // 하위 클래스에서 구체적인 업그레이드 버튼 갱신 로직을 구현합니다.
            // 예를 들어, 재화가 충분한지 확인하고 버튼의 상호작용 가능 여부나 색상을 변경할 수 있습니다.
        }

        /// <summary>
        /// 이 UI 패널이 화면에 표시되거나 활성화될 때 호출되는 함수입니다.
        /// 패널이 열릴 때 필요한 초기화 작업(예: 데이터 로드, UI 요소 초기 상태 설정)을 수행할 수 있습니다.
        /// 이 메서드는 가상(virtual)이므로, 하위 클래스에서 특정 패널에 맞게 재정의할 수 있습니다.
        /// </summary>
        public virtual void OnPanelOpened()
        {
            // 하위 클래스에서 패널이 열릴 때 수행할 특정 로직을 구현합니다.
        }

        /// <summary>
        /// 이 업그레이드 패널을 선택 상태로 만듭니다.
        /// 하위 클래스에서 이 패널이 선택되었을 때 수행할 구체적인 동작(예: UI 강조, 정보 표시 변경)을 구현해야 합니다.
        /// </summary>
        public abstract void Select();
    }
}