// WorldType.cs
// 이 스크립트는 게임에 존재하는 월드의 다양한 유형을 정의하는 열거형입니다.
namespace Watermelon.LevelSystem
{
    // 이 열거형은 Unity 에디터에서 표시 및 직렬화 가능하도록 설정됩니다.
    [System.Serializable]
    public enum WorldType
    {
        // 첫 번째 월드 유형
        World_01 = 1,
        // 두 번째 월드 유형
        World_02 = 2,

        AT_World_01 = 3,
        // 필요한 경우 여기에 다른 월드 유형을 추가할 수 있습니다.
    }
}