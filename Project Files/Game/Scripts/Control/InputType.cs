// ==============================================
// 📌 InputType.cs
// ✅ 프로젝트에서 사용하는 입력 타입 정의 Enum
// ✅ 키보드, UI 조이스틱, 게임패드를 구분하는 데 사용됨
// ==============================================

namespace Watermelon
{
    /// <summary>
    /// 입력 방식 구분용 열거형 (컨트롤러 전환 시 사용)
    /// </summary>
    public enum InputType
    {
        Keyboard = 0,     // 키보드 입력 (PC)
        UIJoystick = 1,   // 화면 터치 조이스틱 (모바일)
        Gamepad = 2       // 게임패드 (콘솔/PC용)
    }
}
