// LevelData.cs
// 이 스크립트는 단일 레벨의 데이터 구조를 정의합니다.
// 레벨 유형, 방 데이터, 경험치, 필요 업그레이드, 적 레벨, 드롭 아이템, 특수 동작 등 레벨을 구성하는 다양한 정보를 포함합니다.
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter; // 관련 데이터 및 열거형을 위해 필요

namespace Watermelon.LevelSystem
{
    // 이 클래스는 Unity 에디터에서 직렬화 및 편집 가능하도록 설정됩니다.
    [System.Serializable]
    public class LevelData
    {
        [SerializeField, Tooltip("레벨의 유형을 나타냅니다."), LevelEditorSetting] // LevelEditorSetting은 커스텀 에디터 속성으로 가정합니다.
        private LevelType type;
        // 레벨 유형에 접근하기 위한 속성
        public LevelType Type => type;

        [SerializeField, Tooltip("이 레벨에 포함된 방 데이터 배열입니다."), LevelEditorSetting]
        private RoomData[] rooms; // RoomData 클래스는 외부 정의가 필요합니다.
        // 방 데이터 배열에 접근하기 위한 속성
        public RoomData[] Rooms => rooms;

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("이 레벨 완료 시 획득할 경험치 양입니다."), LevelEditorSetting]
        private int xpAmount;
        // 경험치 양에 접근하기 위한 속성
        public int XPAmount => xpAmount;

        [SerializeField, Tooltip("이 레벨에 진입하기 위해 필요한 최소 업그레이드 레벨입니다."), LevelEditorSetting]
        private int requiredUpg;
        // 필요 업그레이드 레벨에 접근하기 위한 속성
        public int RequiredUpg => requiredUpg;

        [SerializeField, Tooltip("이 레벨에 등장하는 적들의 기본 레벨입니다."), LevelEditorSetting]
        private int enemiesLevel;
        // 적 레벨에 접근하기 위한 속성
        public int EnemiesLevel => enemiesLevel;

        [SerializeField, Tooltip("이 레벨에서 특정 캐릭터 추천이 있는지 여부입니다."), LevelEditorSetting]
        private bool hasCharacterSuggestion;
        // 캐릭터 추천 여부에 접근하기 위한 속성
        public bool HasCharacterSuggestion => hasCharacterSuggestion;

        [SerializeField, Tooltip("치유 아이템이 스폰될 확률 (0.0 ~ 1.0)입니다."), LevelEditorSetting, Range(0.0f, 1.0f)]
        private float healSpawnPercent = 0.5f; // 기본값 0.5f 설정
        // 치유 아이템 스폰 확률에 접근하기 위한 속성
        public float HealSpawnPercent => healSpawnPercent;

        [SerializeField, Tooltip("이 레벨에서 드롭될 수 있는 아이템 데이터 목록입니다."), LevelEditorSetting]
        private List<DropData> dropData = new List<DropData>(); // DropData 클래스는 외부 정의가 필요합니다.
        // 드롭 아이템 데이터 목록에 접근하기 위한 속성
        public List<DropData> DropData => dropData;

        [SerializeField, Tooltip("이 레벨의 특수 동작 스크립트 배열입니다."), LevelEditorSetting]
        private LevelSpecialBehaviour[] specialBehaviours; // LevelSpecialBehaviour 클래스는 외부 정의가 필요합니다.
        // 특수 동작 스크립트 배열에 접근하기 위한 속성
        public LevelSpecialBehaviour[] SpecialBehaviours => specialBehaviours;

        // 이 레벨이 속한 월드 데이터에 대한 참조
        private WorldData world; // WorldData 클래스는 외부 정의가 필요합니다.
        // 월드 데이터에 접근하기 위한 속성
        public WorldData World => world;

        /// <summary>
        /// 레벨 데이터를 초기화합니다.
        /// </summary>
        /// <param name="world">이 레벨이 속한 월드 데이터</param>
        public void Init(WorldData world)
        {
            this.world = world;
        }

        #region Special Behaviours callbacks
        // 특수 동작 콜백 함수들을 모아 놓은 영역

        /// <summary>
        /// 레벨이 초기화될 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnLevelInitialised()
        {
            // 모든 특수 동작 스크립트의 OnLevelInitialised 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelInitialised();
            }
        }

        /// <summary>
        /// 레벨이 로드될 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnLevelLoaded()
        {
            // 모든 특수 동작 스크립트의 OnLevelLoaded 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelLoaded();
            }
        }

        /// <summary>
        /// 레벨이 언로드될 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnLevelUnloaded()
        {
            // 모든 특수 동작 스크립트의 OnLevelUnloaded 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelUnloaded();
            }
        }

        /// <summary>
        /// 레벨이 시작될 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnLevelStarted()
        {
            // 모든 특수 동작 스크립트의 OnLevelStarted 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelStarted();
            }
        }

        /// <summary>
        /// 레벨 클리어에 실패했을 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnLevelFailed()
        {
            // 모든 특수 동작 스크립트의 OnLevelFailed 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelFailed();
            }
        }

        /// <summary>
        /// 레벨 클리어에 성공했을 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnLevelCompleted()
        {
            // 모든 특수 동작 스크립트의 OnLevelCompleted 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelCompleted();
            }
        }

        /// <summary>
        /// 플레이어가 방에 진입했을 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnRoomEntered()
        {
            // 모든 특수 동작 스크립트의 OnRoomEntered 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnRoomEntered();
            }
        }

        /// <summary>
        /// 플레이어가 방을 떠났을 때 특수 동작들에 대한 콜백을 호출합니다.
        /// </summary>
        public void OnRoomLeaved()
        {
            // 모든 특수 동작 스크립트의 OnRoomLeaved 메서드 호출
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnRoomLeaved();
            }
        }
        #endregion

        /// <summary>
        /// 이 레벨에 존재하는 상자의 총 개수를 반환합니다.
        /// </summary>
        /// <param name="includeRewarded">보상형 상자도 포함할지 여부</param>
        /// <returns>상자의 총 개수</returns>
        public int GetChestsAmount(bool includeRewarded = false)
        {
            int finalAmount = 0;

            // 모든 방을 순회하며 상자 개수 계산
            for (int i = 0; i < rooms.Length; i++)
            {
                var room = rooms[i];
                if (room.ChestEntities != null) // 상자 엔티티 배열이 null이 아닌지 확인
                {
                    for (int j = 0; j < room.ChestEntities.Length; j++)
                    {
                        var chest = room.ChestEntities[j];

                        // 상자가 초기화되었고 (보상형 포함 또는 보상형 제외 조건 만족) 경우에만 개수 증가
                        if (chest.IsInited && (includeRewarded || chest.ChestType != LevelChestType.Rewarded))
                        {
                            finalAmount++;
                        }
                    }
                }
            }

            return finalAmount;
        }

        /// <summary>
        /// 이 레벨 완료 시 획득할 코인 보상 양을 가져옵니다.
        /// </summary>
        /// <returns>코인 보상 양, 코인 드롭 데이터가 없으면 0</returns>
        public int GetCoinsReward()
        {
            // 드롭 데이터 목록을 순회하며 코인 드롭 데이터 찾기
            for (int i = 0; i < dropData.Count; i++)
            {
                // 드롭 타입이 Currency이고 통화 타입이 Coins인 경우 해당 양 반환
                if (dropData[i].DropType == DropableItemType.Currency && dropData[i].CurrencyType == CurrencyType.Coins)
                    return dropData[i].Amount;
            }

            // 코인 드롭 데이터가 없으면 0 반환
            return 0;
        }

        /// <summary>
        /// 이 레벨 완료 시 획득할 무기 카드 보상 목록을 가져옵니다.
        /// </summary>
        /// <returns>무기 카드 데이터 목록</returns>
        public List<WeaponData> GetCardsReward()
        {
            List<WeaponData> result = new List<WeaponData>();

            // 드롭 데이터 목록을 순회하며 무기 카드 드롭 데이터 찾기
            for (int i = 0; i < dropData.Count; i++)
            {
                // 드롭 타입이 WeaponCard인 경우
                if (dropData[i].DropType == DropableItemType.WeaponCard)
                {
                    WeaponData weapon = dropData[i].Weapon; // 해당 무기 데이터

                    // 해당 무기가 잠금 해제되었는지 확인 (WeaponsController 클래스는 외부 정의가 필요합니다.)
                    bool isWeaponUnlocked = WeaponsController.IsWeaponUnlocked(weapon);

                    // 무기가 잠금 해제되지 않았다면 보상 목록에 추가
                    if (!isWeaponUnlocked)
                    {
                        for (int j = 0; j < dropData[i].Amount; j++)
                        {
                            result.Add(weapon);
                        }
                    }
                }
            }

            return result;
        }
    }
}