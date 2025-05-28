// 스크립트 설명: 게임패드 입력과 연동되는 UI 버튼의 동작을 처리하는 클래스입니다.
// 특정 게임패드 버튼 입력 시 Unity UI Button의 Click 이벤트와 연동되며, 태그 시스템을 사용하여 버튼의 활성화 상태를 관리합니다.
using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위한 네임스페이스
using UnityEngine.EventSystems; // UI 이벤트 시스템 인터페이스 사용을 위한 네임스페이스
using System.Collections.Generic; // List 사용을 위한 네임스페이스 (Editor Tool 영역)

// MODULE_INPUT_SYSTEM 정의되어 있을 경우 (Input System 패키지 설치 시)
#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem; // Input System 네임스페이스 사용
#endif

namespace Watermelon
{
    // Button 컴포넌트가 필요함을 명시
    [RequireComponent(typeof(Button))]
    public class UIGamepadButton : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("이 UI 버튼에 연결될 게임패드 버튼 타입")] // 주요 변수 한글 툴팁
        GamepadButtonType buttonType; // 연결된 게임패드 버튼 타입 (GamepadButtonType은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정)
        // 연결된 게임패드 버튼 타입에 접근하기 위한 프로퍼티
        public GamepadButtonType ButtonType => buttonType;

        [SerializeField]
        [Tooltip("이 UI 버튼에 할당된 태그 (UIGamepadButtonTag 열거형 값)")] // 주요 변수 한글 툴팁
        UIGamepadButtonTag buttonTag; // 버튼 태그 (UIGamepadButtonTag는 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정)

        [Space] // 인스펙터에 공간 추가
        [SerializeField]
        [Tooltip("이 버튼 클릭 시 비활성화할 태그 조합")] // 주요 변수 한글 툴팁
        UIGamepadButtonTag buttonsToDisable; // 클릭 시 비활성화할 태그

        [SerializeField]
        [Tooltip("이 버튼 클릭 시 활성화할 태그 조합")] // 주요 변수 한글 툴팁
        UIGamepadButtonTag buttonsToEnable; // 클릭 시 활성화할 태그

        [Space] // 인스펙터에 공간 추가
        // 버튼의 태그가 활성화되어 있더라도 수동으로 버튼의 활성 상태를 제어하는 데 도움이 됩니다.
        // Helps to manualy controll if the button is active even if it's tag is active - 원본 주석 번역
        [SerializeField]
        [Tooltip("이 버튼이 현재 포커스 상태인지 (게임패드로 선택 가능한지) 여부")] // 주요 변수 한글 툴팁
        bool isInFocus = true; // 포커스 상태 여부
        // 포커스 상태에 접근하기 위한 프로퍼티
        public bool IsInFocus { get => isInFocus; private set => isInFocus = value; }

        [Space] // 인스펙터에 공간 추가
        [SerializeField]
        [Tooltip("게임패드 사용 시 버튼 아이콘을 표시하는 Image 컴포넌트")] // 주요 변수 한글 툴팁
        Image gamepadButtonIcon; // 게임패드 버튼 아이콘 이미지

        private Button button; // 이 게임 오브젝트에 연결된 Button 컴포넌트

        // 현재 활성화된 UIGamepadButtonTag 조합 (정적 변수)
        private static UIGamepadButtonTag ActiveButtonTags { get; set; } = UIGamepadButtonTag.MainMenu; // 초기값은 메인 메뉴 태그

        /// <summary>
        /// 스크립트 인스턴스가 로드될 때 처음 호출됩니다.
        /// 필요한 컴포넌트를 가져오고, 입력 변경 이벤트에 구독하며, 버튼 클릭 리스너를 추가합니다.
        /// </summary>
        private void Awake()
        {
            button = GetComponent<Button>(); // Button 컴포넌트 가져오기

            // 초기 입력 타입에 따라 버튼 아이콘 표시 상태 설정. 이후 입력 변경 이벤트 발생 시 자동으로 처리됩니다.
            //Initialising input for the first time. Later it will happen automatically, when the event is triggered - 원본 주석 번역
            OnInputChanged(Control.InputType); // 초기 입력 타입으로 OnInputChanged 호출 (Control에 정의된 InputType 사용)
            Control.OnInputChanged += OnInputChanged; // Control의 입력 변경 이벤트에 OnInputChanged 메서드 구독 (Control에 정의된 OnInputChanged 이벤트 사용)

            button.onClick.AddListener(OnButtonClick); // Button 컴포넌트의 클릭 이벤트에 OnButtonClick 메서드 추가

            EventSystem.current?.SetSelectedGameObject(null); // 현재 선택된 UI 오브젝트를 null로 설정 (null 조건부 연산자 사용)
        }

        /// <summary>
        /// 오브젝트가 파괴될 때 호출됩니다.
        /// 입력 변경 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        /// </summary>
        private void OnDestroy()
        {
            Control.OnInputChanged -= OnInputChanged; // Control의 입력 변경 이벤트 구독 해제
        }

        /// <summary>
        /// 매 프레임마다 호출됩니다.
        /// 현재 입력 타입이 게임패드이고, 버튼의 태그가 활성화 태그에 포함되며, 버튼이 포커스 상태일 때 게임패드 버튼 입력을 감지합니다.
        /// </summary>
        private void Update()
        {
            // 현재 입력 타입이 게임패드가 아니거나, 버튼의 태그가 활성화 태그에 포함되지 않거나, 버튼이 포커스 상태가 아니면 처리 중지
            // 비트 연산 (ActiveButtonTags & buttonTag) 결과가 0이 아니면 버튼 태그가 활성화 태그에 포함됨
            if (Control.InputType != InputType.Gamepad || (ActiveButtonTags & buttonTag) == 0 || !IsInFocus) return; // Control, InputType은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정

            // Input System 모듈이 설치된 경우에만 게임패드 버튼 입력 감지
#if MODULE_INPUT_SYSTEM
            // 연결된 게임패드 버튼이 현재 프레임에 눌러졌는지 확인 (GamepadControl에 정의된 WasButtonPressedThisFrame 사용)
            if (GamepadControl.WasButtonPressedThisFrame(buttonType))
            {
                // 이 버튼과 동일한 게임패드 버튼을 사용하고 새로 활성화된 태그를 가진 UI 버튼이
                // 이 버튼과 같은 프레임에 트리거되는 상황을 피하기 위해 다음 프레임에 태그를 변경합니다.
                // Changing tags next frame to avoid a situation where a ui button with the same gamepad button and a newly enabled tag triggers on the same frame as this button.
                // 버튼은 게임이 일시 정지된 상태에서도 작동해야 하므로 unscaledTime을 true로 설정합니다.
                // Buttons have to still work with the game paused, thus uscaledTime is true - 원본 주석 번역
                Tween.NextFrame(() => { // Tween에 정의된 NextFrame 사용
                    DisableTag(buttonsToDisable); // 비활성화할 태그 비활성화
                    EnableTag(buttonsToEnable); // 활성화할 태그 활성화
                }, unscaledTime: true, framesOffset: 2); // 스케일되지 않은 시간 사용, 2프레임 지연

                button.ClickButton(); // Unity UI Button의 클릭 이벤트 강제 호출 (ClickButton 확장 메서드 사용, Watermelon 네임스페이스에 정의된 것으로 가정)
            }
#endif
        }

        /// <summary>
        /// 입력 타입이 변경되었을 때 호출됩니다.
        /// 현재 입력 타입에 따라 게임패드 버튼 아이콘의 표시 상태를 업데이트합니다.
        /// </summary>
        /// <param name="type">새로운 입력 타입.</param>
        private void OnInputChanged(InputType type)
        {
            // 현재 입력 타입이 게임패드이고 버튼이 포커스 상태이면 버튼 아이콘 활성화 및 스프라이트 설정
            if (Control.InputType == InputType.Gamepad && IsInFocus) // Control, InputType, GamepadData, GetButtonIcon은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
            {
                gamepadButtonIcon.enabled = true; // 아이콘 이미지 활성화

                if (Control.GamepadData != null) gamepadButtonIcon.sprite = Control.GamepadData.GetButtonIcon(buttonType); // GamepadData에서 해당 버튼 아이콘 가져와 설정
            }
            else // 게임패드 입력이 아니거나 포커스 상태가 아니면 버튼 아이콘 비활성화
            {
                gamepadButtonIcon.enabled = false; // 아이콘 이미지 비활성화
            }
        }

        /// <summary>
        /// Unity UI Button의 클릭 이벤트 발생 시 호출됩니다.
        /// 게임패드 입력이 아닌 경우에도 태그를 변경할 수 있도록 합니다.
        /// </summary>
        // 입력 타입을 게임패드에서 키보드로 또는 그 반대로 전환할 수 있도록 활성 버튼을 계속 추적해야 합니다.
        // We still need to keep track of the active buttons in order to be able to swap the control from gamepad to keyboard and vice versa - 원본 주석 번역
        private void OnButtonClick()
        {
            // 현재 입력 타입이 게임패드가 아닌 경우에만 태그 변경 로직 실행
            if(Control.InputType != InputType.Gamepad) // Control, InputType은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
            {
                DisableTag(buttonsToDisable); // 비활성화할 태그 비활성화
                EnableTag(buttonsToEnable); // 활성화할 태그 활성화
            }
        }

        /// <summary>
        /// 버튼의 포커스 상태를 설정합니다.
        /// 버튼 태그가 활성화되어 있더라도 버튼의 시각적 표시를 제어하는 데 유용합니다. (예: 목록, 스크롤 뷰 등)
        /// Gives the ability to control the button visibility even if it's tag is active. Useful for lists, scroll views, etc. - 원본 주석 번역
        /// </summary>
        /// <param name="focus">설정할 포커스 상태 (true: 포커스 됨, false: 포커스 해제).</param>
        public void SetFocus(bool focus)
        {
            IsInFocus = focus; // 포커스 상태 업데이트

            // 포커스 상태이고 게임패드 입력일 때만 버튼 아이콘 활성화
            gamepadButtonIcon.enabled = focus && Control.InputType == InputType.Gamepad; // Control, InputType은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
        }

        /// <summary>
        /// 지정된 태그를 활성 UIGamepadButtonTag 조합에 추가합니다.
        /// 비트 연산(OR)을 사용하여 태그를 추가합니다.
        /// Binary operation to add values to the flag - 원본 주석 번역
        /// </summary>
        /// <param name="tagToEnable">활성화할 태그.</param>
        public static void EnableTag(UIGamepadButtonTag tagToEnable)
        {
            ActiveButtonTags |= tagToEnable; // 비트 OR 연산으로 태그 추가
        }

        /// <summary>
        /// 지정된 태그를 활성 UIGamepadButtonTag 조합에서 제거합니다.
        /// 비트 연산(AND 및 NOT)을 사용하여 태그를 제거합니다.
        /// Binary operation to remove values from the flag - 원본 주석 번역
        /// </summary>
        /// <param name="tagToDisable">비활성화할 태그.</param>
        public static void DisableTag(UIGamepadButtonTag tagToDisable)
        {
            ActiveButtonTags &= ~tagToDisable; // 비트 AND 및 NOT 연산으로 태그 제거
        }

        /// <summary>
        /// 모든 UIGamepadButtonTag를 비활성화합니다.
        /// </summary>
        public static void DisableAllTags()
        {
            ActiveButtonTags &= 0; // 모든 비트를 0으로 설정하여 모든 태그 비활성화
        }

        #region 하이라이트 (Highlight)

        private TweenCase highlightScaleCase; // 하이라이트 스케일 애니메이션 트윈 케이스 (Tween에 정의된 것으로 가정)
        private TweenCase returnCase; // 원래 스케일로 돌아가는 트윈 케이스 (Tween에 정의된 것으로 가정)
        private bool isHighlightActive; // 하이라이트 애니메이션 활성 상태 여부

        /// <summary>
        /// 버튼에 하이라이트 애니메이션을 시작합니다.
        /// 이미 활성화된 경우 중복 시작하지 않습니다.
        /// </summary>
        public void StartHighlight()
        {
            if (isHighlightActive) return; // 이미 활성 상태이면 중복 시작 방지
            isHighlightActive = true; // 하이라이트 활성 상태로 설정

            // returnCase 트윈이 활성화되어 있을 경우 중지합니다. 안전을 위해.
            // Killing return case just to be sure - 원본 주석 번역
            returnCase.KillActive(); // returnCase 트윈 중지 (TweenCase에 정의된 KillActive 사용)

            PingPongAnimation(); // 핑퐁 애니메이션 시작
        }

        /// <summary>
        /// 버튼의 하이라이트 애니메이션을 중지하고 원래 스케일로 되돌립니다.
        /// </summary>
        public void StopHighLight()
        {
            if (!isHighlightActive) return; // 활성 상태가 아니면 중지 처리 중지
            isHighlightActive = false; // 하이라이트 비활성 상태로 설정

            // highlightCase 트윈은 반드시 중지해야 합니다.
            // Definitely should kill highlight case - 원본 주석 번역
            highlightScaleCase.KillActive(); // highlightScaleCase 트윈 중지 (TweenCase에 정의된 KillActive 사용)

            // 원래 스케일(1f)로 부드럽게 돌아가는 트윈 시작 (Tween에 정의된 DOScale, SetEasing 사용)
            returnCase = gamepadButtonIcon.DOScale(1f, 0.2f).SetEasing(Ease.Type.SineOut); // Ease.Type은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
        }

        /// <summary>
        /// 버튼 아이콘의 스케일을 일정 범위(0.9f ~ 1.2f)로 핑퐁(점점 커졌다 작아졌다 반복) 애니메이션을 재생합니다.
        /// 애니메이션 완료 시 자신을 다시 호출하여 무한 반복합니다.
        /// </summary>
        // 트윈 작동 방식 때문에 스택 오버플로우를 유발하지 않습니다.
        // Shouldn trigger stack overflown because of how tween works - 원본 주석 번역
        private void PingPongAnimation()
        {
            // gamepadButtonIcon의 스케일을 0.9f와 1.2f 사이에서 1초 동안 핑퐁하는 트윈 시작 (Tween에 정의된 DOPingPongScale, OnComplete 사용)
            // 애니메이션 완료 시 PingPongAnimation 메서드를 다시 호출
            highlightScaleCase = gamepadButtonIcon.DOPingPongScale(0.9f, 1.2f, 1, Ease.Type.SineInOut, Ease.Type.SineInOut).OnComplete(PingPongAnimation); // Ease.Type은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
        }

        #endregion

        #region 에디터 도구 (Editor Tool)

        // UNITY_EDITOR 심볼이 정의되어 있을 경우에만 아래 코드 실행 (Unity 에디터 환경)
#if UNITY_EDITOR

        // Unity 에디터의 인스펙터에 버튼으로 표시
        [Button] // Button 속성은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정 (EditorTool 관련 유틸리티)
        /// <summary>
        /// Unity 에디터 모드에서 씬의 모든 UIGamepadButton에 연결된 게임패드 아이콘 이미지를 활성화합니다.
        /// 게임 실행 중에는 작동하지 않습니다. 프리팹 내부에는 영향을 미치지 않습니다.
        /// </summary>
        public void ShowGamepadButtonsInEditor()
        {
            if (Application.isPlaying) return; // 게임 실행 중이면 작동 중지

            // 씬에 있는 모든 UIGamepadButton 컴포넌트를 찾아 게임패드 아이콘 이미지를 활성화합니다.
            // 비활성화된 오브젝트도 포함합니다. 프리팹 내부의 컴포넌트에는 영향을 주지 않습니다!
            // Enabling gamepad icon images in every UIGamepadButton on the scene.
            // WILL NOT ENABLE THEM INSIDE PREFABS! - 원본 주석 번역
            FindObjectsByType<UIGamepadButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).ForEach(button => button.gamepadButtonIcon.enabled = true); // FindObjectsByType, ForEach는 Unity 또는 다른 곳에 정의된 것으로 가정
        }

        // Unity 에디터의 인스펙터에 버튼으로 표시
        [Button] // Button 속성은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정 (EditorTool 관련 유틸리티)
        /// <summary>
        /// Unity 에디터 모드에서 씬의 모든 UIGamepadButton에 연결된 게임패드 아이콘 이미지를 비활성화합니다.
        /// 게임 실행 중에는 작동하지 않습니다. 프리팹 내부에는 영향을 미치지 않습니다.
        /// </summary>
        public void HideGamepadButtonsInEditor()
        {
            if (Application.isPlaying) return; // 게임 실행 중이면 작동 중지

            // 씬에 있는 모든 UIGamepadButton 컴포넌트를 찾아 게임패드 아이콘 이미지를 비활성화합니다.
            // 비활성화된 오브젝트도 포함합니다. 프리팹 내부의 컴포넌트에는 영향을 주지 않습니다!
            // Disabling gamepad icon images in every UIGamepadButton on the scene.
            // WILL NOT DISABLE THEM INSIDE PREFABS! - 원본 주석 번역
            FindObjectsByType<UIGamepadButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).ForEach(button => button.gamepadButtonIcon.enabled = false); // FindObjectsByType, ForEach는 Unity 또는 다른 곳에 정의된 것으로 가정
        }

        // Unity 에디터의 인스펙터에 버튼으로 표시
        [Button] // Button 속성은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정 (EditorTool 관련 유틸리티)
        /// <summary>
        /// 현재 활성화된 UIGamepadButtonTag 조합을 콘솔에 출력합니다. (디버깅용)
        /// </summary>
        public void PrintEnabledTags()
        {
            Debug.Log(ActiveButtonTags); // 현재 활성 태그 조합 출력
        }
#endif // UNITY_EDITOR 종료

        #endregion // 에디터 도구 영역 종료
    }
}