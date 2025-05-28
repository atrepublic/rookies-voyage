/*
📄 UISettings.cs 요약 정리
이 스크립트는 게임의 설정(Settings) UI 페이지 자체를 관리하는 핵심 클래스입니다.
UIPage를 상속받아 UI 시스템의 일부로 작동하며, 설정 페이지의 초기화,
화면에 나타나고 사라질 때의 애니메이션 처리, 내부 UI 요소(패널, 콘텐츠 영역)의 크기 조절,
그리고 닫기 버튼 및 배경 클릭을 통한 페이지 닫기 기능을 담당합니다.

⭐ 주요 기능
- 설정 UI 페이지에 필요한 UI 요소(배경 이미지, 패널, 콘텐츠 영역, 닫기 버튼 등)에 대한 참조를 관리합니다.
- 페이지 초기화(Init) 시 버튼 이벤트 리스너를 등록하고 배경 이미지의 기본 알파(투명도) 값을 저장합니다.
- 페이지가 화면에 표시될 때(PlayShowAnimation)와 사라질 때(PlayHideAnimation) DOTween과 같은
  트위닝 라이브러리를 사용하여 부드러운 애니메이션 효과를 재생합니다.
- 설정 항목들의 내용에 따라 패널의 전체 높이를 동적으로 재계산(RecalculatePanelSize)하여 UI 레이아웃을 조절합니다.
- 사용자가 닫기 버튼을 클릭하거나 패널 외부의 배경 영역을 클릭했을 때 페이지를 닫는 상호작용을 처리합니다.

🛠️ 사용 용도
- 게임 내의 주 설정 화면 UI를 구현하는 데 사용됩니다.
- UIController에 의해 관리되며, 다른 UI 요소(예: 메인 메뉴의 설정 버튼)에 의해 표시되거나 숨겨질 수 있습니다.
- GamepadUISettings와 같은 다른 스크립트와 연동되어 게임패드 조작을 지원하는 설정 화면의 기반이 될 수 있습니다.
*/

using UnityEngine;
using UnityEngine.EventSystems; // EventTriggerType, PointerEventData 사용을 위해 필요
using UnityEngine.UI;         // Image, Button, RectTransform 등 UI 관련 클래스 사용을 위해 필요
// using DG.Tweening; // DOTween 사용 시 필요할 수 있습니다. (예: DOAnchoredPosition, DOFade, SetEase/SetEasing)

namespace Watermelon
{
    /// <summary>
    /// 게임 설정을 위한 UI 페이지를 나타내는 클래스입니다.
    /// UIPage를 상속받아 UI 시스템의 페이지 관리 로직을 따릅니다.
    /// </summary>
    public class UISettings : UIPage // UIPage는 Watermelon 프레임워크 또는 프로젝트 내 정의된 UI 페이지 기본 클래스로 가정합니다.
    {
        [Tooltip("설정 패널의 배경 이미지 컴포넌트입니다. 투명도 애니메이션 및 클릭 시 패널 닫기 이벤트에 사용됩니다.")]
        [BoxGroup("References", "References")] // Odin Inspector 등 에셋에서 사용되는 그룹화 속성일 수 있습니다.
        [SerializeField] Image backgroundImage;

        [Tooltip("설정 UI 요소들을 담고 있는 주 패널의 RectTransform 컴포넌트입니다. 위치 및 크기 애니메이션에 사용됩니다.")]
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform panelRectTransform;

        [Tooltip("실제 설정 항목(버튼, 그룹 등)들이 배치되는 스크롤 가능한 콘텐츠 영역의 RectTransform 컴포넌트입니다. 패널 크기 동적 계산에 사용됩니다.")]
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform contentRectTransform;
        /// <summary>
        /// 설정 항목들이 포함된 콘텐츠 영역의 RectTransform을 외부로 제공하는 프로퍼티입니다.
        /// GamepadUISettings 등 다른 스크립트에서 이 영역 내의 UI 요소들을 참조할 때 사용될 수 있습니다.
        /// </summary>
        public RectTransform ContentRectTransform => contentRectTransform;

        [Tooltip("설정 패널을 닫는 기능을 수행하는 Button 컴포넌트입니다.")]
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;

        // 배경 이미지의 기본 알파(투명도) 값입니다. 페이지 표시/숨김 애니메이션 시 원래 투명도로 복원하는 데 사용됩니다.
        private float defaultAlpha;

        /// <summary>
        /// UI 페이지가 처음 초기화될 때 호출됩니다. (UIPage에서 상속)
        /// 주로 이벤트 리스너 등록, 초기값 설정 등의 작업을 수행합니다.
        /// </summary>
        public override void Init()
        {
            // 닫기 버튼의 onClick 이벤트에 OnCloseButtonClicked 메서드를 리스너로 등록합니다.
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            // 배경 이미지에 PointerDown (마우스 클릭 또는 터치 시작) 이벤트가 발생하면 OnBackgroundClicked 메서드를 호출하도록 설정합니다.
            // backgroundImage.AddEvent는 Image 컴포넌트에 대한 확장 메서드이거나 Watermelon 프레임워크에서 제공하는 기능으로 가정합니다.
            backgroundImage.AddEvent(EventTriggerType.PointerDown, OnBackgroundClicked);

            // 배경 이미지의 현재 알파값을 defaultAlpha 변수에 저장해 둡니다. 이는 나중에 애니메이션에서 사용됩니다.
            defaultAlpha = backgroundImage.color.a;
        }

        /// <summary>
        /// UI 페이지가 화면에 나타날 때 재생될 애니메이션을 정의합니다. (UIPage에서 상속)
        /// UIController에 의해 호출되어 페이지 표시 애니메이션을 시작합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            // 실제 콘텐츠 내용에 따라 패널의 높이를 다시 계산하여 업데이트합니다.
            RecalculatePanelSize();

            // 패널을 화면 아래쪽 보이지 않는 위치(Y 값: -2000)에서 시작하도록 설정합니다.
            panelRectTransform.anchoredPosition = Vector2.down * 2000;
            // 패널을 원래 위치(anchoredPosition = Vector2.zero)로 0.3초 동안 SineOut 이징을 사용해 부드럽게 이동시킵니다.
            // DOAnchoredPosition, SetEasing은 DOTween과 같은 트위닝 라이브러리의 기능으로 가정합니다.
            panelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.SineOut);

            // 배경 이미지를 완전히 투명한 상태(알파 0)에서 시작하도록 설정합니다.
            // backgroundImage.SetAlpha는 Image 컴포넌트의 알파 값을 설정하는 확장 메서드 또는 Watermelon 프레임워크 기능으로 가정합니다.
            backgroundImage.SetAlpha(0);
            // 배경 이미지를 저장된 기본 알파값(defaultAlpha)으로 0.3초 동안 서서히 나타나게 합니다. (DOTween 사용 가정)
            // 애니메이션 완료 후 UIController.OnPageOpened를 호출하여 페이지가 완전히 열렸음을 알립니다.
            backgroundImage.DOFade(defaultAlpha, 0.3f).OnComplete(() => UIController.OnPageOpened(this));
        }

        /// <summary>
        /// UI 페이지가 화면에서 사라질 때 재생될 애니메이션을 정의합니다. (UIPage에서 상속)
        /// UIController에 의해 호출되어 페이지 숨김 애니메이션을 시작합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            // 패널을 화면 아래쪽(Y 값: -2000)으로 0.3초 동안 SineIn 이징을 사용해 부드럽게 이동시켜 사라지게 합니다. (DOTween 사용 가정)
            panelRectTransform.DOAnchoredPosition(Vector2.down * 2000, 0.3f).SetEasing(Ease.Type.SineIn);

            // 배경 이미지를 0.3초 동안 서서히 투명하게(알파 0) 만듭니다. (DOTween 사용 가정)
            // 애니메이션 완료 후 UIController.OnPageClosed를 호출하여 페이지가 완전히 닫혔음을 알립니다.
            backgroundImage.DOFade(0, 0.3f).OnComplete(() => UIController.OnPageClosed(this));
        }

        /// <summary>
        /// contentRectTransform 내부의 활성화된 자식 요소들의 높이를 합산하여
        /// panelRectTransform의 전체 높이를 동적으로 재계산합니다.
        /// 이를 통해 설정 항목의 수나 종류에 따라 패널 크기가 유동적으로 조절됩니다.
        /// </summary>
        private void RecalculatePanelSize()
        {
            // contentRectTransform 자체의 기본 높이 (예: VerticalLayoutGroup의 패딩, 또는 고정된 최소 높이)를 초기 높이로 사용합니다.
            // sizeDelta.y는 레이아웃 설정에 따라 음수일 수 있으므로 절대값을 사용합니다.
            float height = Mathf.Abs(contentRectTransform.sizeDelta.y);

            int childCount = contentRectTransform.childCount;
            for(int i = 0; i < childCount; i++)
            {
                Transform childTransform = contentRectTransform.GetChild(i);
                if (childTransform != null) // 자식 Transform이 유효한지 확인
                {
                    // 자식 요소가 SettingsElementsGroup 타입인지 확인합니다.
                    SettingsElementsGroup settingsElementsGroup = childTransform.GetComponent<SettingsElementsGroup>();
                    if(settingsElementsGroup != null) // SettingsElementsGroup 컴포넌트가 있는 경우
                    {
                        // 해당 그룹이 활성화 상태(내부에 활성화된 요소가 있는 경우)일 때만 높이에 추가합니다.
                        if (settingsElementsGroup.IsGroupActive())
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y; // 그룹의 높이를 더합니다.
                        }
                    }
                    else // SettingsElementsGroup이 아닌 일반 자식 요소인 경우
                    {
                        // 해당 자식 게임 오브젝트가 활성화되어 있을 때만 높이에 추가합니다.
                        if(childTransform.gameObject.activeSelf)
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y; // 자식 요소의 높이를 더합니다.
                        }
                    }
                }
            }

            // 계산된 총 높이로 panelRectTransform의 높이(sizeDelta.y)를 설정합니다. 너비(sizeDelta.x)는 기존 값을 유지합니다.
            panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, height);
        }

        /// <summary>
        /// 닫기 버튼(closeButton)이 클릭되었을 때 호출되는 메서드입니다.
        /// 클릭 사운드를 재생하고, UIController를 통해 이 설정 페이지를 숨기도록 요청합니다.
        /// </summary>
        public void OnCloseButtonClicked()
        {
            // AudioController를 사용하여 지정된 버튼 클릭 사운드를 재생합니다.
            // AudioController와 AudioClips.buttonSound는 프로젝트의 사운드 시스템 및 오디오 클립 리소스로 가정합니다.
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // UIController를 통해 현재 페이지(UISettings 타입)를 숨기도록 요청합니다.
            // HidePage는 UIController의 페이지 숨김 기능으로 가정합니다.
            UIController.HidePage<UISettings>();
        }

        /// <summary>
        /// 배경 이미지(backgroundImage)가 클릭되었을 때 호출되는 메서드입니다.
        /// (Init 메서드에서 EventTriggerType.PointerDown 이벤트에 연결됨)
        /// UIController를 통해 이 설정 페이지를 숨기도록 요청합니다. 이는 팝업 외부 영역 클릭 시 닫히는 일반적인 UX를 구현합니다.
        /// </summary>
        /// <param name="data">포인터 이벤트 데이터입니다. 여기서는 사용되지 않지만, 이벤트 핸들러의 표준 매개변수 형태입니다.</param>
        private void OnBackgroundClicked(PointerEventData data)
        {
            UIController.HidePage<UISettings>();
        }
    }
}