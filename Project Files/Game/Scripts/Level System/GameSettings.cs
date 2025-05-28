// GameSettings.cs
// 이 스크립트는 게임의 다양한 설정 및 데이터베이스에 대한 참조를 관리하는 ScriptableObject입니다.
// 레벨, 캐릭터, 무기, 경험치, 밸런스, 적 데이터베이스 등 게임 전반에 걸쳐 사용되는 중요한 데이터를 포함하고 로드/언로드 기능을 제공합니다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMeshData를 위해 필요
using Watermelon.SquadShooter; // 관련 데이터베이스 및 열거형을 위해 필요

namespace Watermelon.LevelSystem
{
    // Unity 에디터에서 "Data/Game Settings" 메뉴를 통해 이 ScriptableObject를 생성할 수 있도록 합니다.
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Data/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        // 게임 설정의 싱글턴 인스턴스
        private static GameSettings settings;

        [LineSpacer("Databases")] // 에디터에서 시각적인 구분을 위한 커스텀 속성 (가정)
        [SerializeField]
        [Tooltip("레벨 관련 데이터를 관리하는 데이터베이스")] // levelsDatabase 변수에 대한 툴팁
        private LevelsDatabase levelsDatabase;
        public LevelsDatabase LevelsDatabase => levelsDatabase;

        [SerializeField]
        [Tooltip("캐릭터 관련 데이터를 관리하는 데이터베이스")] // charactersDatabase 변수에 대한 툴팁
        private CharactersDatabase charactersDatabase;
        public CharactersDatabase CharactersDatabase => charactersDatabase;

        [SerializeField]
        [Tooltip("무기 관련 데이터를 관리하는 데이터베이스")] // weaponDatabase 변수에 대한 툴팁
        private WeaponDatabase weaponDatabase;
        public WeaponDatabase WeaponDatabase => weaponDatabase;

        [SerializeField]
        [Tooltip("경험치 및 레벨업 관련 데이터를 관리하는 데이터베이스")] // experienceDatabase 변수에 대한 툴팁
        private ExperienceDatabase experienceDatabase;
        public ExperienceDatabase ExperienceDatabase => experienceDatabase;

        [SerializeField]
        [Tooltip("게임 밸런스 관련 데이터를 관리하는 데이터베이스")] // balanceDatabase 변수에 대한 툴팁
        private BalanceDatabase balanceDatabase;
        public BalanceDatabase BalanceDatabase => balanceDatabase;

        [SerializeField]
        [Tooltip("적 관련 데이터를 관리하는 데이터베이스")] // enemiesDatabase 변수에 대한 툴팁
        private EnemiesDatabase enemiesDatabase;
        public EnemiesDatabase EnemiesDatabase => enemiesDatabase;

                // GameSettings.cs 맨 위 필드 추가 예시
        [SerializeField] [Tooltip("펫 데이터베이스")]
        private UC_PetDatabase petDatabase;
        public UC_PetDatabase PetDatabase => petDatabase;

        [LineSpacer("Player")] // 에디터에서 시각적인 구분을 위한 커스텀 속성 (가정)
        [SerializeField]
        [Tooltip("플레이어 게임 오브젝트 프리팹")] // playerPrefab 변수에 대한 툴팁
        private GameObject playerPrefab;
        public GameObject PlayerPrefab => playerPrefab;

        [SerializeField]
        [Tooltip("부활 후 무적 시간 (초)")] // invulnerabilityAfrerReviveDuration 변수에 대한 툴팁
        private float invulnerabilityAfrerReviveDuration;
        public float InvulnerabilityAfrerReviveDuration => invulnerabilityAfrerReviveDuration;

        [SerializeField]
        [Tooltip("공격 버튼 사용 여부")] // useAttackButton 변수에 대한 툴팁
        private bool useAttackButton;
        public bool UseAttackButton => useAttackButton;

        [LineSpacer("Environment")] // 에디터에서 시각적인 구분을 위한 커스텀 속성 (가정)
        [SerializeField]
        [Tooltip("내비게이션 메쉬 데이터")] // navMeshData 변수에 대한 툴팁
        private NavMeshData navMeshData;
        public NavMeshData NavMeshData => navMeshData;

        [SerializeField]
        [Tooltip("맵 뒤쪽 벽 콜라이더")] // backWallCollider 변수에 대한 툴팁
        private GameObject backWallCollider;
        public GameObject BackWallCollider => backWallCollider;

        [Space(5f)] // 에디터에서 시각적인 간격 조절
        [SerializeField]
        [Tooltip("레벨별 상자 데이터 배열")] // chestData 변수에 대한 툴팁
        private ChestData[] chestData;
        public ChestData[] ChestData => chestData;

        [LineSpacer("Drop")] // 에디터에서 시각적인 구분을 위한 커스텀 속성 (가정)
        [SerializeField]
        [Tooltip("아이템 드롭 관련 설정")] // dropSettings 변수에 대한 툴팁
        private DropableItemSettings dropSettings;

        [LineSpacer("Minimap")] // 에디터에서 시각적인 구분을 위한 커스텀 속성 (가정)
        [SerializeField]
        [Tooltip("레벨 유형별 설정 배열")] // levelTypes 변수에 대한 툴팁
        private LevelTypeSettings[] levelTypes;

        [SerializeField]
        [Tooltip("미니맵에 사용될 기본 월드 스프라이트")] // defaultWorldSprite 변수에 대한 툴팁
        private Sprite defaultWorldSprite;
        public Sprite DefaultWorldSprite => defaultWorldSprite;

        [LineSpacer("Rewarded Video")] // 에디터에서 시각적인 구분을 위한 커스텀 속성 (가정)
        [SerializeField]
        [Tooltip("부활에 필요한 재화 가격")] // revivePrice 변수에 대한 툴팁
        private CurrencyPrice revivePrice; // CurrencyPrice 클래스는 외부 정의가 필요합니다.
        public CurrencyPrice RevivePrice => revivePrice;

        // 레벨 유형을 인덱스로 빠르게 찾기 위한 딕셔너리
        private Dictionary<LevelType, int> levelTypesLink;



        /// <summary>
        /// 게임 설정을 초기화하고 필요한 데이터를 로드합니다.
        /// 싱글턴 인스턴스를 설정하고, 레벨 유형 설정을 초기화하며, 드롭 및 상자 데이터를 설정합니다.
        /// </summary>
        public void Init()
        {
            // 싱글턴 인스턴스 설정
            settings = this;

            // 레벨 유형 링크 딕셔너리 초기화 및 설정
            levelTypesLink = new Dictionary<LevelType, int>();
            for (int i = 0; i < levelTypes.Length; i++)
            {
                // 이미 존재하는 레벨 유형인지 확인
                if (!levelTypesLink.ContainsKey(levelTypes[i].LevelType))
                {
                    // 레벨 유형 설정 초기화
                    levelTypes[i].Init();
                    // 딕셔너리에 추가
                    levelTypesLink.Add(levelTypes[i].LevelType, i);
                }
                else
                {
                    // 중복된 레벨 유형이 발견되면 오류 로그 출력
                    Debug.LogError(string.Format("[Levels]: Duplicate is found - {0}", levelTypes[i].LevelType));
                }
            }

            // 드롭 시스템 초기화
            Drop.Init(dropSettings); // Drop 클래스와 Init 메서드는 외부 정의가 필요합니다.

            // 각 상자 데이터 초기화
            for (int i = 0; i < chestData.Length; i++)
            {
                chestData[i].Init(); // ChestData 클래스와 Init 메서드는 외부 정의가 필요합니다.
            }

            // ★ 펫설정  추가 ★
            petDatabase.Init();  // GameSettings.Instance.PetDatabase.Init();
        }

        /// <summary>
        /// 게임 설정을 언로드하고 메모리 정리를 수행합니다.
        /// 레벨 유형 설정과 상자 데이터를 언로드하고, 레벨 유형 링크 딕셔너리를 비웁니다.
        /// </summary>
        public void Unload()
        {
            // 각 레벨 유형 설정 언로드
            foreach (var levelType in levelTypes)
            {
                levelType.Unload(); // LevelTypeSettings 클래스와 Unload 메서드는 외부 정의가 필요합니다.
            }
            // 레벨 유형 링크 딕셔너리 비우기
            levelTypesLink.Clear();

            // 각 상자 데이터 언로드
            foreach (var chest in chestData)
            {
                chest.Unload(); // ChestData 클래스와 Unload 메서드는 외부 정의가 필요합니다.
            }

            // 드롭 시스템 언로드
            Drop.Unload(); // Drop 클래스와 Unload 메서드는 외부 정의가 필요합니다.
        }

        /// <summary>
        /// 특정 레벨 유형에 해당하는 레벨 설정 데이터를 가져옵니다.
        /// </summary>
        /// <param name="levelType">가져올 레벨 설정의 유형</param>
        /// <returns>해당 레벨 유형의 LevelTypeSettings, 없으면 null 반환 및 오류 로그 출력</returns>
        public LevelTypeSettings GetLevelSettings(LevelType levelType)
        {
            // 딕셔너리에 레벨 유형이 존재하는지 확인
            if (levelTypesLink.ContainsKey(levelType))
                // 존재하면 해당 레벨 유형 설정 반환
                return levelTypes[levelTypesLink[levelType]];

            // 레벨 유형이 없으면 오류 로그 출력
            Debug.LogError(string.Format("[Levels]: Level with type '{0}' is missing", levelType));

            // 해당 레벨 유형 설정이 없으므로 null 반환
            return null;
        }

        /// <summary>
        /// 특정 상자 유형에 해당하는 상자 데이터(ChestData)를 가져옵니다.
        /// </summary>
        /// <param name="chestType">가져올 상자 데이터의 유형</param>
        /// <returns>해당 상자 유형의 ChestData, 없으면 null 반환 및 오류 로그 출력</returns>
        public ChestData GetChestData(LevelChestType chestType)
        {
            // 모든 상자 데이터를 순회하며 일치하는 유형 찾기
            for (int i = 0; i < chestData.Length; i++)
            {
                var data = chestData[i];
                if (chestType == data.Type)
                {
                    // 일치하는 상자 데이터를 찾으면 반환
                    return data;
                }
            }

            // 해당 상자 유형의 데이터가 없으면 오류 로그 출력
            Debug.LogError(string.Format("[Level]: Chest preset with type {0} is missing!", chestType));

            // 해당 상자 데이터가 없으므로 null 반환
            return null;
        }

        /// <summary>
        /// 게임 설정의 싱글턴 인스턴스를 가져옵니다.
        /// </summary>
        /// <returns>GameSettings의 싱글턴 인스턴스</returns>
        public static GameSettings GetSettings()
        {
            // 싱글턴 인스턴스 반환
            return settings;
        }
    }
}