// RoomData.cs
// 이 스크립트는 레벨 내 단일 방의 데이터 구조를 정의합니다.
// 플레이어 스폰 지점, 적 엔티티, 아이템 엔티티, 상자 엔티티, 방 사용자 지정 오브젝트 정보를 포함합니다.

/*
적 엔티티 (Enemy Entity):

코드 상에서는 EnemyEntityData 클래스로 표현됩니다.
이름에서 알 수 있듯이 레벨에 등장하는 적 캐릭터를 나타냅니다.
EnemyEntityData는 해당 적의 유형 (EnemyType), 방 안에서의 위치 (Position), 회전 (Rotation), 스케일 (Scale) 정보를 가지고 있습니다.
또한, 해당 적이 엘리트 몬스터인지 여부 (IsElite)와 적이 이동하거나 순찰할 경로 지점들 (PathPoints) 정보도 포함하고 있습니다.
즉, '적 엔티티'는 레벨의 특정 방에 어떤 종류의 적이 어떤 모습으로 어디에 배치되어 어떤 경로로 움직일지를 정의하는 데이터 덩어리입니다.
아이템 엔티티 (Item Entity):

코드 상에서는 ItemEntityData (이 클래스 자체는 첨부된 파일에 직접 포함되어 있지 않지만, RoomData.cs와 RoomEnvironmentPreset.cs에서 사용됨)와 LevelItem.cs의 LevelItem 클래스와 연관됩니다.
RoomData.cs의 itemEntities 배열과 RoomEnvironmentPreset.cs의 itemEntities 배열은 방에 배치될 아이템이나 환경 요소를 나타내는 데이터를 담고 있습니다.
LevelItem.cs의 LevelItem 클래스는 이러한 아이템의 프리팹, 고유한 해시 값, 그리고 아이템의 유형(LevelItemType - 장애물인지 환경 요소인지 등) 정보를 가지고 있습니다.
또한, LevelItem은 오브젝트 풀링을 위한 Pool 객체도 관리합니다. 이는 게임 실행 중 아이템 오브젝트를 효율적으로 생성하고 재활용하기 위함입니다.
따라서 '아이템 엔티티'는 레벨 내 특정 위치에 배치될 수 있는 상호작용 가능한 아이템 (예: 회복 포션, 탄약 등)이나 상호작용하지 않는 환경 오브젝트 (예: 박스, 벽 등)를 정의하는 데이터입니다.
상자 엔티티 (Chest Entity):

코드 상에서는 ChestEntityData (이 클래스 자체는 첨부된 파일에 직접 포함되어 있지 않지만, RoomData.cs에서 사용됨)와 LevelChestType.cs의 LevelChestType 열거형과 연관됩니다.
RoomData.cs의 chestEntities 배열은 방에 배치될 상자 오브젝트를 나타내는 데이터를 담고 있습니다.
LevelChestType.cs는 상자의 유형을 정의합니다 (예: 일반 상자 Standart, 보상형 상자 Rewarded 등).
LevelData.cs 파일에서는 GetChestsAmount 함수를 통해 특정 레벨에 있는 상자의 총 개수를 계산하는 로직에서 상자 엔티티가 사용되는 것을 볼 수 있습니다.
결론적으로 '상자 엔티티'는 레벨의 특정 방에 어떤 유형의 상자가 어떤 위치에 배치될지를 정의하며, 이 상자는 플레이어가 열어서 아이템이나 재화 등을 획득할 수 있는 게임 오브젝트입니다.
*/

using UnityEngine;

namespace Watermelon.LevelSystem
{
    // 이 클래스는 Unity 에디터에서 직렬화 및 편집 가능하도록 설정됩니다.
    [System.Serializable]
    public class RoomData
    {
        [SerializeField, Tooltip("플레이어가 이 방에서 스폰될 위치")] // spawnPoint 변수에 대한 툴팁
        private Vector3 spawnPoint;
        // 스폰 지점 위치에 접근하기 위한 속성
        public Vector3 SpawnPoint => spawnPoint;

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("이 방에 배치될 적 엔티티 데이터 배열")] // enemyEntities 변수에 대한 툴팁
        private EnemyEntityData[] enemyEntities; // EnemyEntityData 클래스는 외부 정의가 필요합니다.
        // 적 엔티티 데이터 배열에 접근하기 위한 속성
        public EnemyEntityData[] EnemyEntities => enemyEntities;

        [SerializeField, Tooltip("이 방에 배치될 아이템 엔티티 데이터 배열")] // itemEntities 변수에 대한 툴팁
        private ItemEntityData[] itemEntities; // ItemEntityData 클래스는 외부 정의가 필요합니다.
        // 아이템 엔티티 데이터 배열에 접근하기 위한 속성
        public ItemEntityData[] ItemEntities => itemEntities;

        [SerializeField, Tooltip("이 방에 배치될 상자 엔티티 데이터 배열")] // chestEntities 변수에 대한 툴팁
        private ChestEntityData[] chestEntities; // ChestEntityData 클래스는 외부 정의가 필요합니다.
        // 상자 엔티티 데이터 배열에 접근하기 위한 속성
        public ChestEntityData[] ChestEntities => chestEntities;

        [SerializeField, Tooltip("이 방에 배치될 사용자 지정 오브젝트 데이터 배열")] // roomCustomObjects 변수에 대한 툴팁
        private CustomObjectData[] roomCustomObjects; // CustomObjectData 클래스는 외부 정의가 필요합니다.
        // 방 사용자 지정 오브젝트 데이터 배열에 접근하기 위한 속성
        public CustomObjectData[] RoomCustomObjects => roomCustomObjects;

    }
}