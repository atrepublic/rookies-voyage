// 스크립트 설명: 게임 내 드롭 가능한 아이템의 타입을 정의하는 열거형(Enum)입니다.
namespace Watermelon.SquadShooter
{
    // 게임 내 드롭 가능한 아이템 타입을 정의하는 열거형
    public enum DropableItemType
    {
        // 기본값 또는 미지정 타입
        None = -1,

        // 화폐 타입 드롭
        Currency = 0,
        // 무기 카드 타입 드롭
        WeaponCard = 1,

        // 회복 아이템 타입 드롭
        Heal = 2,

        // 무기 타입 드롭
        Weapon = 3,
        // 캐릭터 타입 드롭
        Character = 4
    }
}