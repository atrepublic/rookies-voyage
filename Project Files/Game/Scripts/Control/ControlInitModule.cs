// ==============================================
// 📌 ControlInitModule.cs
// ✅ 입력 타입에 따라 자동 또는 수동으로 컨트롤 시스템을 초기화하는 모듈
// ✅ 초기화 시 KeyboardControl 또는 GamepadControl 컴포넌트를 추가함
// ==============================================

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Control Manager")]
    public class ControlInitModule : InitModule
    {
        public override string ModuleName => "Control Manager";

        [Tooltip("입력 타입을 자동으로 선택할지 여부")]
        [SerializeField] private bool selectAutomatically = true;

        [Tooltip("수동 선택 시 사용할 입력 타입")]
        [HideIf("selectAutomatically")]
        [SerializeField] private InputType inputType;

        [Tooltip("게임패드 입력 설정 데이터")]
        [HideIf("IsJoystickCondition")]
        [SerializeField] private GamepadData gamepadData;

        /// <summary>
        /// 📌 컨트롤 컴포넌트 초기화
        /// </summary>
        public override void CreateComponent()
        {
            if (selectAutomatically)
                inputType = ControlUtils.GetCurrentInputType();

            Control.Init(inputType, gamepadData);

            if (inputType == InputType.Keyboard)
            {
                var keyboard = Initializer.GameObject.AddComponent<KeyboardControl>();
                keyboard.Init();
            }
            else if (inputType == InputType.Gamepad)
            {
                var gamepad = Initializer.GameObject.AddComponent<GamepadControl>();
                gamepad.Init();
            }
        }

        /// <summary>
        /// 📌 UIJoystick 입력 타입 조건 검사
        /// </summary>
        private bool IsJoystickCondition()
        {
            return selectAutomatically 
                ? ControlUtils.GetCurrentInputType() == InputType.UIJoystick
                : inputType == InputType.UIJoystick;
        }
    }
}
