// RoomEnvironmentPresetType.cs
// 이 스크립트는 방 환경 사전 설정(프리셋)의 다양한 유형을 정의하는 열거형입니다.
namespace Watermelon.LevelSystem
{
    // 이 열거형은 Unity 에디터에서 표시 및 직렬화 가능하도록 설정됩니다.
    [System.Serializable]
    public enum RoomEnvironmentPresetType
    {
        // 작은 크기의 방 환경 프리셋 유형
        Small = 0,
        // 중간 너비의 방 환경 프리셋 유형
        WideMid = 1,
        // 긴 너비의 방 환경 프리셋 유형
        WideLong = 2,
    }
}