/*
 * MenuPanelButton.cs
 * ---------------------
 * 이 추상 클래스는 메인 메뉴 화면의 측면 패널 버튼들(캐릭터, 상점, 무기 등)의
 * 공통적인 기능과 구조를 정의하는 기반 클래스입니다.
 * 버튼 클릭 처리, 하이라이트(알림) 표시, 활성/비활성 상태 관리,
 * 창 열림/닫힘 시 애니메이션 처리 등의 공통 로직을 포함합니다.
 * 각 메뉴 버튼은 이 클래스를 상속받아 구체적인 동작을 구현합니다.
 */

using UnityEngine;
using UnityEngine.UI;
using Watermelon; // Watermelon 프레임워크 네임스페이스 (TweenCase, UIGamepadButton 등)

namespace Watermelon.SquadShooter
{
    // 메뉴 패널 버튼의 기본 기능을 정의하는 추상 클래스
    public abstract class MenuPanelButton : MonoBehaviour
    {
        [Tooltip("실제 클릭 이벤트를 처리하는 UI 버튼 컴포넌트")]
        [SerializeField] protected Button button;
        [Tooltip("버튼의 배경 또는 탭 역할을 하는 이미지 컴포넌트")]
        [SerializeField] protected Image tabImage;
        [Tooltip("버튼의 기본 색상")]
        [SerializeField] protected Color defaultColor = Color.white; // 기본값 명시
        [Tooltip("알림(하이라이트) 상태일 때의 버튼 색상")]
        [SerializeField] protected Color notificationColor = Color.yellow; // 기본값 명시
        [Tooltip("버튼이 비활성화되었을 때의 색상")]
        [SerializeField] protected Color disabledColor = Color.gray; // 기본값 명시
        [Tooltip("알림(하이라이트) 아이콘 등을 표시하는 게임 오브젝트")]
        [SerializeField] protected GameObject notificationObject;

        [Tooltip("버튼 이동/색상 변경 애니메이션 트윈 케이스")]
        private TweenCase movementTweenCase;

        [Tooltip("버튼의 초기 Anchored Position 값")]
        private Vector2 defaultAnchoredPosition;

        [Tooltip("버튼의 RectTransform 컴포넌트")]
        private RectTransform rectTransform;
        // 외부에서 RectTransform에 접근하기 위한 프로퍼티
        public RectTransform RectTransform => rectTransform;

        // 외부에서 Button 컴포넌트에 접근하기 위한 프로퍼티
        public Button Button => button;

        [Tooltip("게임패드 네비게이션을 위한 버튼 컴포넌트")]
        private UIGamepadButton gamepadButton;
        // 외부에서 게임패드 버튼 컴포넌트에 접근하기 위한 프로퍼티
        public UIGamepadButton GamepadButton => gamepadButton;

        [Tooltip("버튼의 투명도 조절 등을 위한 CanvasGroup 컴포넌트")]
        private CanvasGroup canvasGroup;

        [Tooltip("버튼이 현재 활성화 상태인지 여부")]
        private bool isActive;

        /// <summary>
        /// 버튼을 초기화합니다. 상속받는 클래스에서 필요시 base.Init()을 호출해야 합니다.
        /// </summary>
        public virtual void Init()
        {
            canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroup 컴포넌트 가져오기
            gamepadButton = GetComponent<UIGamepadButton>(); // 게임패드 버튼 컴포넌트 가져오기

            rectTransform = (RectTransform)button.transform; // RectTransform 캐싱

            defaultAnchoredPosition = rectTransform.anchoredPosition; // 초기 위치 저장

            // 버튼 클릭 시 OnButtonClicked 추상 메서드를 호출하도록 리스너 추가
            button.onClick.AddListener(OnButtonClicked);

            isActive = true; // 초기 상태는 활성
        }

        /// <summary>
        /// 버튼이 현재 활성화되어야 하는지 여부를 반환합니다. (예: 특정 기능 해금 여부)
        /// 기본적으로는 항상 활성화 상태를 반환하며, 필요시 자식 클래스에서 오버라이드합니다.
        /// </summary>
        /// <returns>활성화되어야 하면 true</returns>
        public virtual bool IsActive()
        {
            return true;
        }

        /// <summary>
        /// 관련 창(메인 메뉴 등)이 열렸을 때 호출됩니다.
        /// 버튼 상태를 초기화하고 필요한 하이라이트 애니메이션을 재생합니다.
        /// </summary>
        public void OnWindowOpened()
        {
            // 비활성 상태면 아무것도 하지 않음
            if (!isActive)
                return;

            movementTweenCase.KillActive(); // 진행 중인 애니메이션 중지

            // 위치 및 색상 초기화
            rectTransform.anchoredPosition = defaultAnchoredPosition;
            tabImage.color = defaultColor;

            // 하이라이트(알림)가 필요한지 확인 (자식 클래스에서 구현)
            if (IsHighlightRequired())
            {
                notificationObject.SetActive(true); // 알림 오브젝트 활성화

                // 탭 이미지를 알림 색상으로 변경하는 애니메이션 시작
                movementTweenCase = tabImage.DOColor(notificationColor, 0.3f, 0.3f).OnComplete(delegate
                {
                    // 색상 변경 완료 후, 탭이 위아래로 움직이는 애니메이션 시작
                    // TabAnimation은 Watermelon 프레임워크의 커스텀 트윈 클래스일 수 있음
                    movementTweenCase = new TabAnimation(rectTransform, new Vector2(defaultAnchoredPosition.x, defaultAnchoredPosition.y + 30)).SetDuration(1.2f).SetUnscaledMode(false).SetUpdateMethod(UpdateMethod.Update).SetEasing(Ease.Type.QuadOutIn).StartTween();
                });
            }
            else // 하이라이트 불필요
            {
                notificationObject.SetActive(false); // 알림 오브젝트 비활성화
            }
        }

        /// <summary>
        /// 관련 창(메인 메뉴 등)이 닫혔을 때 호출됩니다.
        /// 애니메이션을 중지하고 버튼 위치를 초기화합니다.
        /// </summary>
        public void OnWindowClosed()
        {
            movementTweenCase.KillActive(); // 진행 중인 애니메이션 중지
            rectTransform.anchoredPosition = defaultAnchoredPosition; // 위치 초기화
        }

        /// <summary>
        /// 버튼을 비활성화 상태로 만듭니다.
        /// </summary>
        public void Disable()
        {
            isActive = false; // 상태 플래그 변경

            button.enabled = false; // 버튼 클릭 비활성화

            // 비활성화 색상 및 위치 설정
            tabImage.color = disabledColor;
            rectTransform.anchoredPosition = defaultAnchoredPosition;

            notificationObject.SetActive(false); // 알림 오브젝트 비활성화

            canvasGroup.alpha = 0.5f; // 반투명하게 처리

            movementTweenCase.KillActive(); // 진행 중인 애니메이션 중지
        }

        /// <summary>
        /// 버튼을 활성화 상태로 만듭니다.
        /// </summary>
        public void Activate()
        {
            isActive = true; // 상태 플래그 변경

            button.enabled = true; // 버튼 클릭 활성화

            canvasGroup.alpha = 1.0f; // 완전히 보이도록 처리

            OnWindowOpened(); // 창이 열렸을 때의 로직 실행 (하이라이트 확인 등)
        }

        /// <summary>
        /// (추상 메서드) 이 버튼에 하이라이트(알림) 표시가 필요한지 여부를 반환해야 합니다.
        /// 자식 클래스에서 구체적인 조건을 구현합니다.
        /// </summary>
        /// <returns>하이라이트가 필요하면 true</returns>
        protected abstract bool IsHighlightRequired();

        /// <summary>
        /// (추상 메서드) 버튼이 클릭되었을 때 실행될 로직입니다.
        /// 자식 클래스에서 구체적인 동작(패널 전환 등)을 구현합니다.
        /// </summary>
        protected abstract void OnButtonClicked();
    }
}

    // Watermelon 프레임워크에 포함된 것으로 추정되는 커스텀 트윈 클래스
    // 실제 구현은 Watermelon 프레임워크 소스 코드에 있음

    
   // public class TabAnimation : TweenCase
   // {
        // 생성자 및 필요한 메서드 구현 (프레임워크 내부 구현)
    //   public TabAnimation(RectTransform target, Vector2 endValue) { /* ... 프레임워크 내부 구현 ... */ }
    //  public TabAnimation SetDuration(float duration) { /* ... */ return this; }
    //  public TabAnimation SetUnscaledMode(bool unscaled) { /* ... */ return this; }
    //  public TabAnimation SetUpdateMethod(UpdateMethod method) { /* ... */ return this; }
    //  public TabAnimation SetEasing(Ease.Type easeType) { /* ... */ return this; }
    //  public TweenCase StartTween() { /* ... */ return this; }

        // TweenCase에서 상속받은 추상 메서드 구현
    //   public override void Animate() { /* ... 프레임워크 내부 구현 ... */ }
    //   public override void Kill() { /* ... 프레임워크 내부 구현 ... */ }
    //   public override void Complete() { /* ... 프레임워크 내부 구현 ... */ }
    //   public override void SetTime(float time) { /* ... 프레임워크 내부 구현 ... */ }
   // }
