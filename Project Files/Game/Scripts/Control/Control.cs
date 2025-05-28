// ==============================================
// 📌 Control.cs
// ✅ 입력 방식에 따라 적절한 컨트롤러(키보드/패드)를 설정하고 관리하는 시스템
// ✅ 현재 입력 방식, 컨트롤 객체, 게임패드 데이터 등을 통합적으로 관리
// ==============================================

using UnityEngine;

namespace Watermelon
{
    public static class Control
    {
        [Tooltip("현재 사용 중인 입력 타입")]
        public static InputType InputType { get; private set; }

        [Tooltip("현재 컨트롤 인터페이스 (키보드/패드 등)")]
        public static IControlBehavior CurrentControl { get; private set; }

        [Tooltip("게임패드 설정 데이터")]
        public static GamepadData GamepadData { get; private set; }

        public delegate void OnInputChangedCallback(InputType input);
        public static event OnInputChangedCallback OnInputChanged;

        [Tooltip("초기화 여부")]
        public static bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// 📌 컨트롤 시스템 초기화
        /// </summary>
        public static void Init(InputType inputType, GamepadData gamepadData)
        {
            InputType = inputType;
            GamepadData = gamepadData;

            if (GamepadData != null) 
                GamepadData.Init();

            IsInitialized = true;
        }

        /// <summary>
        /// 📌 입력 타입 변경 (Gamepad <-> Keyboard)
        /// </summary>
        public static void ChangeInputType(InputType inputType)
        {
            InputType = inputType;

            Object.Destroy(CurrentControl as MonoBehaviour);

            switch (inputType)
            {
                case InputType.Gamepad:
                    var gamepad = Initializer.GameObject.AddComponent<GamepadControl>();
                    gamepad.Init();
                    CurrentControl = gamepad;
                    break;

                case InputType.Keyboard:
                    var keyboard = Initializer.GameObject.AddComponent<KeyboardControl>();
                    keyboard.Init();
                    CurrentControl = keyboard;
                    break;
            }

            OnInputChanged?.Invoke(inputType);
        }

        /// <summary>
        /// 📌 외부에서 수동으로 컨트롤러 설정
        /// </summary>
        public static void SetControl(IControlBehavior controlBehavior)
        {
            CurrentControl = controlBehavior;
        }

        /// <summary>
        /// 📌 이동 입력 허용
        /// </summary>
        public static void EnableMovementControl()
        {
#if UNITY_EDITOR
            if (CurrentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");
                return;
            }
#endif
            CurrentControl.EnableMovementControl();
        }

        /// <summary>
        /// 📌 이동 입력 차단
        /// </summary>
        public static void DisableMovementControl()
        {
#if UNITY_EDITOR
            if (CurrentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");
                return;
            }
#endif
            CurrentControl.DisableMovementControl();
        }
    }
}
