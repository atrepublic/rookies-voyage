// 스크립트 설명: 게임패드 입력을 사용하여 플레이어 이동 및 버튼 입력을 처리하는 클래스입니다.
// Input System 모듈과 연동하여 게임패드 스틱 및 버튼 상태를 감지하고, 다른 입력 타입으로의 전환 기능을 포함합니다.
// IControlBehavior 인터페이스를 구현하여 Control 시스템과 연동됩니다.
#pragma warning disable 0067 // 사용되지 않는 이벤트 경고 비활성화 (OnMovementInputActivated)

using System.Collections.Generic; // Dictionary 사용을 위한 네임스페이스
using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 시스템 관련 네임스페이스 (사용되지 않으나 포함됨)

// MODULE_INPUT_SYSTEM 정의되어 있을 경우 (Input System 패키지 설치 시)
#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem; // Input System 네임스페이스 사용
using UnityEngine.UI; // UI 관련 네임스페이스 (사용되지 않으나 포함됨)
#endif

namespace Watermelon
{
    // IControlBehavior 인터페이스 구현 (Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정)
    public class GamepadControl : MonoBehaviour, IControlBehavior
    {
        // 왼쪽 스틱의 X 및 Y 축 입력을 나타내는 이동 방향 벡터
        // Left Stick x and y axes - 원본 주석 번역
        [Tooltip("게임패드 왼쪽 스틱의 현재 입력 방향 벡터")] // 주요 변수 한글 툴팁
        public Vector3 MovementInput { get; private set; } // 이동 입력 방향

        // 현재 게임패드 이동 입력(Vector3)의 크기가 0보다 큰지 여부
        [Tooltip("현재 게임패드 이동 입력(Vector3)의 크기가 0보다 큰지 여부")] // 주요 변수 한글 툴팁
        public bool IsMovementInputNonZero { get; private set; } // 이동 입력 존재 여부

        [Tooltip("게임패드 이동 컨트롤이 현재 활성화된 상태인지 여부")] // 주요 변수 한글 툴팁
        public bool IsMovementControlActive { get; private set; } // 이동 컨트롤 활성화 상태

        // 처음 이동 입력이 감지되었을 때 발생하는 이벤트 (현재 사용되지 않음)
        public event SimpleCallback OnMovementInputActivated; // 이동 입력 활성화 이벤트 (SimpleCallback은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정)

        // 게임패드 버튼이 눌러진 시간을 저장하는 딕셔너리
        private static Dictionary<GamepadButtonType, float> gamepadButtonPressedTime = new Dictionary<GamepadButtonType, float>();

        // 시야 입력 관련 프로퍼티 (게임패드 컨트롤은 오른쪽 스틱으로 시야 입력을 제공할 수 있으나, 이 스크립트에서는 처리하지 않음)
        public bool IsLookInputNonZero => false; // 시야 입력 존재 여부 (항상 false)
        public Vector3 LookInput => Vector3.zero; // 시야 입력 방향 (항상 Vector3.zero)


        /// <summary>
        /// 게임패드 컨트롤 시스템을 초기화하고 현재 입력 타입이 게임패드일 경우 이 컨트롤러를 활성화합니다.
        /// </summary>
        public void Init()
        {
            // 현재 입력 타입이 게임패드인지 확인 (Control, InputType은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정)
            if (Control.InputType == InputType.Gamepad)
            {
                Control.SetControl(this); // Control 시스템에 이 컨트롤러를 현재 컨트롤러로 설정 (Control에 정의된 것으로 가정)

                // MonoBehaviour의 enabled 변수를 활성화하여 Update 메서드가 실행되도록 함
                // As Behavior.enabled, inherited variable - 원본 주석 번역
                enabled = true; // MonoBehaviour 활성화
                IsMovementControlActive = true; // 이동 컨트롤 활성 상태로 설정
            }
            else
            {
                enabled = false; // 다른 입력 타입일 경우 MonoBehaviour 비활성화
            }
        }

        /// <summary>
        /// 매 프레임마다 호출되며, 게임패드 입력을 감지하고 플레이어 이동 방향 벡터를 갱신합니다.
        /// 키보드 입력이 감지되면 키보드 컨트롤로 전환합니다.
        /// </summary>
        private void Update()
        {
            // Input System 모듈이 설치된 경우에만 아래 코드 실행
#if MODULE_INPUT_SYSTEM
            // 개발자 주석: 이 'if' 문이 모든 시나리오에서 100% 확실하게 작동하는지는 모르겠지만, 지금까지는 잘 작동한다.
            // Dev: not 100% sure this 'if' statement works in every scenario, but so far so good - 원본 주석 번역
            // 현재 게임패드가 연결되지 않았거나 (null) 키보드 입력이 감지되면 키보드 컨트롤로 전환
            if (Gamepad.current == null || (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)) // Keyboard.current는 Input System)
            {
                Control.ChangeInputType(InputType.Keyboard); // Control 시스템의 입력 타입 변경 (Control에 정의된 것으로 가정)

                return; // Update 함수 종료
            }

            // 게임패드 버튼 상태 업데이트 함수 호출
            GamepadButtonsUpdate();

            // 이동 컨트롤이 비활성 상태이면 처리 중지
            if (!IsMovementControlActive) return;

            // 게임패드 왼쪽 스틱의 X 및 Y 축 입력 값 가져오기
            float horizontalInput = Gamepad.current.leftStick.x.value;
            float verticalInput = Gamepad.current.leftStick.y.value;

            // 왼쪽 스틱 입력을 기반으로 이동 방향 벡터 설정
            // Vector3.ClampMagnitude를 사용하여 벡터 크기를 최대 1로 제한 (대각선 이동 시 속도 일정 유지)
            MovementInput = Vector3.ClampMagnitude(new Vector3(horizontalInput, 0, verticalInput), 1);

            // 처음 입력(벡터 크기가 0.1f 이상)이 감지되었을 때 이벤트 발생
            if (!IsMovementInputNonZero && MovementInput.magnitude > 0.1f)
            {
                IsMovementInputNonZero = true;

                OnMovementInputActivated?.Invoke(); // 이동 입력 활성화 이벤트 발생 (null 조건부 연산자 사용)
            }

            // 이동 입력 존재 여부 업데이트 (벡터 크기가 0.1f 이상일 때 true)
            IsMovementInputNonZero = MovementInput.magnitude > 0.1f;
#endif
        }

        /// <summary>
        /// 지정된 게임패드 버튼이 현재 프레임에 눌러졌는지 확인합니다.
        /// </summary>
        /// <param name="button">확인할 게임패드 버튼 타입.</param>
        /// <returns>버튼이 눌러졌으면 true, 그렇지 않으면 false.</returns>
        // 개발자 주석: 이 메서드를 추가한 이유 - UI 게임패드 버튼과 실제 게임패드 버튼을 추상화하고 코드를 더 깔끔하게 유지하는 데 도움이 된다.
        // Dev: reasons for adding this method: it helps to abstract ui gamepad buttons from the actual gamepad buttons, and keeps the code cleaner - 원본 주석 번역
        public static bool WasButtonPressedThisFrame(GamepadButtonType button)
        {
            // Input System 모듈이 설치된 경우에만 실행
#if MODULE_INPUT_SYSTEM
            if (Gamepad.current == null) return false; // 게임패드가 연결되지 않았다면 false 반환

            // 게임패드 버튼 타입에 따라 해당 버튼의 wasPressedThisFrame 속성 반환
            switch (button)
            {
                case GamepadButtonType.A: return Gamepad.current.aButton.wasPressedThisFrame;
                case GamepadButtonType.B: return Gamepad.current.bButton.wasPressedThisFrame;
                case GamepadButtonType.X: return Gamepad.current.xButton.wasPressedThisFrame;
                case GamepadButtonType.Y: return Gamepad.current.yButton.wasPressedThisFrame;

                case GamepadButtonType.Start: return Gamepad.current.startButton.wasPressedThisFrame;
                case GamepadButtonType.Select: return Gamepad.current.selectButton.wasPressedThisFrame;

                case GamepadButtonType.DDown: return Gamepad.current.dpad.down.wasPressedThisFrame;
                case GamepadButtonType.DUp: return Gamepad.current.dpad.up.wasPressedThisFrame;
                case GamepadButtonType.DLeft: return Gamepad.current.dpad.left.wasPressedThisFrame;
                case GamepadButtonType.DRight: return Gamepad.current.dpad.right.wasPressedThisFrame;

                case GamepadButtonType.LB: return Gamepad.current.leftShoulder.wasPressedThisFrame;
                case GamepadButtonType.RB: return Gamepad.current.rightShoulder.wasPressedThisFrame;

                case GamepadButtonType.L3: return Gamepad.current.leftStickButton.wasPressedThisFrame;
                case GamepadButtonType.R3: return Gamepad.current.rightStickButton.wasPressedThisFrame;

                default: return false; // 알 수 없는 버튼 타입일 경우 false 반환
            }
#else
            return false; // Input System 모듈이 설치되지 않았다면 false 반환
#endif
        }

        /// <summary>
        /// 지정된 게임패드 버튼이 현재 프레임에 떼어졌는지 확인합니다.
        /// </summary>
        /// <param name="button">확인할 게임패드 버튼 타입.</param>
        /// <returns>버튼이 떼어졌으면 true, 그렇지 않으면 false.</returns>
        public static bool WasButtonReleasedThisFrame(GamepadButtonType button)
        {
            // Input System 모듈이 설치된 경우에만 실행
#if MODULE_INPUT_SYSTEM
            if (Gamepad.current == null) return false; // 게임패드가 연결되지 않았다면 false 반환

            // 게임패드 버튼 타입에 따라 해당 버튼의 wasReleasedThisFrame 속성 반환
            switch (button)
            {
                case GamepadButtonType.A: return Gamepad.current.aButton.wasReleasedThisFrame;
                case GamepadButtonType.B: return Gamepad.current.bButton.wasReleasedThisFrame;
                case GamepadButtonType.X: return Gamepad.current.xButton.wasReleasedThisFrame;
                case GamepadButtonType.Y: return Gamepad.current.yButton.wasReleasedThisFrame;

                case GamepadButtonType.Start: return Gamepad.current.startButton.wasReleasedThisFrame;
                case GamepadButtonType.Select: return Gamepad.current.selectButton.wasReleasedThisFrame;

                case GamepadButtonType.DDown: return Gamepad.current.dpad.down.wasReleasedThisFrame;
                case GamepadButtonType.DUp: return Gamepad.current.dpad.up.wasReleasedThisFrame;
                case GamepadButtonType.DLeft: return Gamepad.current.dpad.left.wasReleasedThisFrame;
                case GamepadButtonType.DRight: return Gamepad.current.dpad.right.wasReleasedThisFrame;

                case GamepadButtonType.LB: return Gamepad.current.leftShoulder.wasReleasedThisFrame;
                case GamepadButtonType.RB: return Gamepad.current.rightShoulder.wasReleasedThisFrame;

                case GamepadButtonType.LT: return Gamepad.current.leftTrigger.wasReleasedThisFrame;
                case GamepadButtonType.RT: return Gamepad.current.rightTrigger.wasReleasedThisFrame;

                case GamepadButtonType.L3: return Gamepad.current.leftStickButton.wasReleasedThisFrame;
                case GamepadButtonType.R3: return Gamepad.current.rightStickButton.wasReleasedThisFrame;

                default: return false; // 알 수 없는 버튼 타입일 경우 false 반환
            }
#else
            return false; // Input System 모듈이 설치되지 않았다면 false 반환
#endif
        }

        // Input System 모듈이 설치된 경우에만 실행되는 버튼 상태 업데이트 메서드
#if MODULE_INPUT_SYSTEM
        /// <summary>
        /// 매 프레임 게임패드 버튼들의 현재 눌림 상태를 확인하고 기록합니다.
        /// </summary>
        private void GamepadButtonsUpdate()
        {
            // 각 게임패드 버튼의 현재 눌림 상태를 GamepadButtonUpdate 메서드로 전달하여 처리
            GamepadButtonUpdate(GamepadButtonType.A, Gamepad.current.aButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.B, Gamepad.current.bButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.X, Gamepad.current.xButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.Y, Gamepad.current.yButton.isPressed);

            GamepadButtonUpdate(GamepadButtonType.DDown, Gamepad.current.dpad.down.isPressed);
            GamepadButtonUpdate(GamepadButtonType.DUp, Gamepad.current.dpad.up.isPressed);
            GamepadButtonUpdate(GamepadButtonType.DLeft, Gamepad.current.dpad.left.isPressed);
            GamepadButtonUpdate(GamepadButtonType.DRight, Gamepad.current.dpad.right.isPressed);

            GamepadButtonUpdate(GamepadButtonType.LB, Gamepad.current.leftShoulder.isPressed);
            GamepadButtonUpdate(GamepadButtonType.RB, Gamepad.current.rightShoulder.isPressed);

            GamepadButtonUpdate(GamepadButtonType.LT, Gamepad.current.leftTrigger.isPressed);
            GamepadButtonUpdate(GamepadButtonType.RT, Gamepad.current.rightTrigger.isPressed);

            GamepadButtonUpdate(GamepadButtonType.L3, Gamepad.current.leftStickButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.R3, Gamepad.current.rightStickButton.isPressed);

            GamepadButtonUpdate(GamepadButtonType.Start, Gamepad.current.startButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.Select, Gamepad.current.selectButton.isPressed);
        }
#endif

        /// <summary>
        /// 특정 게임패드 버튼의 현재 눌림 상태를 기반으로 gamepadButtonPressedTime 딕셔너리를 업데이트합니다.
        /// 버튼이 눌러졌으면 현재 시간을 기록하고, 떼어졌으면 기록을 제거합니다.
        /// </summary>
        /// <param name="type">업데이트할 게임패드 버튼 타입.</param>
        /// <param name="isPressed">버튼이 현재 눌러져 있는지 여부.</param>
        private void GamepadButtonUpdate(GamepadButtonType type, bool isPressed)
        {
            if (isPressed) // 버튼이 눌러져 있다면
            {
                // 딕셔너리에 해당 버튼 타입의 키가 없으면 현재 시간과 함께 추가
                if (!gamepadButtonPressedTime.ContainsKey(type)) gamepadButtonPressedTime.Add(type, Time.time);
            }
            else // 버튼이 떼어져 있다면
            {
                // 딕셔너리에 해당 버튼 타입의 키가 있으면 제거
                if (gamepadButtonPressedTime.ContainsKey(type)) gamepadButtonPressedTime.Remove(type);
            }
        }

        #region Control 관리 함수

        /// <summary>
        /// 게임패드 이동 입력을 비활성화하고 현재 입력 값을 0으로 초기화합니다. (IControlBehavior 인터페이스 구현)
        /// </summary>
        public void DisableMovementControl()
        {
            IsMovementControlActive = false; // 이동 컨트롤 비활성화 상태로 설정

            MovementInput = Vector3.zero; // 이동 입력 값 초기화
        }

        /// <summary>
        /// 게임패드 이동 입력을 활성화합니다. (IControlBehavior 인터페이스 구현)
        /// </summary>
        public void EnableMovementControl()
        {
            IsMovementControlActive = true; // 이동 컨트롤 활성화 상태로 설정
        }

        /// <summary>
        /// 게임패드 이동 상태를 초기화합니다.
        /// 이동 입력 존재 여부와 이동 입력 값을 0으로 설정합니다. (IControlBehavior 인터페이스 구현)
        /// </summary>
        public void ResetControl()
        {
            IsMovementInputNonZero = false; // 이동 입력 존재 여부 false로 설정
            MovementInput = Vector3.zero; // 이동 입력 값 초기화
        }

        #endregion
    }
}