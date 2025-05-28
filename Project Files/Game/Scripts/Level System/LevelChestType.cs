// LevelChestType.cs
// 이 스크립트는 레벨에서 사용될 수 있는 상자의 유형을 정의하는 열거형입니다.
namespace Watermelon.LevelSystem
{
    // Unity 인스펙터에서 표시 및 직렬화 가능하도록 설정
    [System.Serializable]
    public enum LevelChestType
    {
        // 기본 상자 유형
        Standart = 0,
        // 보상형 광고 시청 등으로 얻는 상자 유형
        Rewarded = 1,
    }
}