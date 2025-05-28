// 이 스크립트는 게임 내 레벨의 유형을 정의하는 열거형(Enum)입니다.
// 레벨이 일반 레벨인지, 보스 레벨인지 등을 구분하는 데 사용됩니다.
namespace Watermelon.SquadShooter
{
    // 게임 내 레벨의 다양한 유형을 정의하는 열거형입니다.
    public enum LevelType
    {
        // 일반 레벨 유형입니다.
        Default = 0,
        // 보스 레벨 유형입니다.
        Boss = 1,
        // 필요에 따라 다른 레벨 유형을 추가할 수 있습니다.
        // Example = 2,
    }
}