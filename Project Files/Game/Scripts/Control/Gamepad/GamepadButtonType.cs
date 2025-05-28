// 스크립트 설명: 게임패드의 다양한 버튼 타입을 정의하는 열거형(Enum)입니다.
// Input System 패키지와 연동하여 게임패드 입력을 처리하는 데 사용됩니다.
namespace Watermelon
{
    // 게임패드의 각 버튼 타입을 나타내는 열거형
    public enum GamepadButtonType
    {
        // 액션 버튼 (주로 우측에 위치)
        A,
        B,
        X,
        Y,

        // Start 버튼
        Start,

        // D-패드 방향 버튼
        DLeft,
        DRight,
        DUp,
        DDown,

        // 숄더 버튼 (위쪽 버튼)
        RB, // Right Bumper (오른쪽 숄더)
        LB, // Left Bumper (왼쪽 숄더)

        // 스틱 버튼 (스틱 누르기)
        L3, // Left Stick Button (왼쪽 스틱 누르기)
        R3, // Right Stick Button (오른쪽 스틱 누르기)

        // 트리거 버튼 (아래쪽 버튼)
        RT, // Right Trigger (오른쪽 트리거)
        LT, // Left Trigger (왼쪽 트리거)

        // Select 또는 Back 버튼
        Select,

        // 필요한 다른 버튼 타입이 있다면 여기에 추가
    }
}