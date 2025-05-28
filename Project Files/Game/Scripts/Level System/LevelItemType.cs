// LevelItemType.cs
// 이 스크립트는 레벨 환경에 배치될 수 있는 아이템의 다양한 유형을 정의하는 열거형입니다.
namespace Watermelon.LevelSystem
{
    // 이 열거형은 Unity 에디터에서 표시 및 직렬화 가능하도록 설정됩니다.
    [System.Serializable]
    public enum LevelItemType
    {
        // 플레이어 또는 적의 이동을 막는 장애물 유형
        Obstacle = 0,
        // 순수하게 시각적인 요소인 환경 요소 유형
        Environment = 1
    }
}