// ==============================================
// 📌 ControlUtils.cs
// ✅ 현재 플랫폼 및 실행 환경에 맞는 기본 입력 타입 반환 도우미 클래스
// ==============================================

using UnityEngine;

namespace Watermelon
{
    public static class ControlUtils
    {
        /// <summary>
        /// 📌 현재 플랫폼에 따라 적절한 입력 타입 반환
        /// </summary>
        public static InputType GetCurrentInputType()
        {
#if UNITY_EDITOR
    #if UNITY_ANDROID || UNITY_IOS
            return InputType.UIJoystick;
    #else
            return InputType.Keyboard;
    #endif
#else
    #if UNITY_ANDROID || UNITY_IOS
            return InputType.UIJoystick;
    #elif UNITY_WEBGL
            return Application.isMobilePlatform ? InputType.UIJoystick : InputType.Keyboard;
    #else
            return InputType.Keyboard;
    #endif
#endif
        }
    }
}
