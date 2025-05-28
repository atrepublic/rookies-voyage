using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    /// <summary>
    /// ActiveRoom 클래스는 현재 활성화된 게임 방(Room)의 상태를 관리합니다.
    /// 이 클래스는 레벨 오브젝트, 적 캐릭터, 상자, 출구 포인트 등 방 내부의 모든 요소를 
    /// 생성, 관리, 제거하는 기능을 제공합니다.
    /// 또한 레벨 데이터와 방 데이터를 저장하고 현재 월드 및 레벨 인덱스를 추적합니다.
    /// </summary>
    public static class ActiveRoom
    {
        /// <summary>
        /// 현재 활성화된 레벨의 게임 오브젝트입니다.
        /// </summary>
        private static GameObject levelObject;

        /// <summary>
        /// 현재 방의 데이터 정보를 저장합니다.
        /// </summary>
        private static RoomData roomData;
        
        /// <summary>
        /// 현재 방의 데이터에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static RoomData RoomData => roomData;

        /// <summary>
        /// 현재 레벨의 데이터 정보를 저장합니다.
        /// </summary>
        private static LevelData levelData;
        
        /// <summary>
        /// 현재 레벨의 데이터에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static LevelData LevelData => levelData;

        /// <summary>
        /// 방 내에 생성된 활성 오브젝트들의 목록입니다.
        /// </summary>
        private static List<GameObject> activeObjects;

        /// <summary>
        /// 방 내에 생성된 적 캐릭터들의 목록입니다.
        /// </summary>
        private static List<BaseEnemyBehavior> enemies;
        
        /// <summary>
        /// 방 내 적 캐릭터 목록에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static List<BaseEnemyBehavior> Enemies => enemies;

        /// <summary>
        /// 방 내에 생성된 상자들의 목록입니다.
        /// </summary>
        private static List<AbstractChestBehavior> chests;
        
        /// <summary>
        /// 방 내 상자 목록에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static List<AbstractChestBehavior> Chests => chests;

        /// <summary>
        /// 현재 레벨의 인덱스 번호입니다.
        /// </summary>
        private static int currentLevelIndex;
        
        /// <summary>
        /// 현재 레벨 인덱스에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static int CurrentLevelIndex => currentLevelIndex;

        /// <summary>
        /// 현재 월드의 인덱스 번호입니다.
        /// </summary>
        private static int currentWorldIndex;
        
        /// <summary>
        /// 현재 월드 인덱스에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static int CurrentWorldIndex => currentWorldIndex;

        /// <summary>
        /// 방 내의 출구 포인트들의 목록입니다.
        /// </summary>
        private static List<ExitPointBehaviour> exitPoints;
        
        /// <summary>
        /// 방 내 출구 포인트 목록에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static List<ExitPointBehaviour> ExitPoints => exitPoints;

        /// <summary>
        /// 방 내에 생성된 커스텀 오브젝트들의 목록입니다.
        /// </summary>
        private static List<GameObject> customObjects;
        
        /// <summary>
        /// 방 내 커스텀 오브젝트 목록에 접근할 수 있는 프로퍼티입니다.
        /// </summary>
        public static List<GameObject> CustomObjects => customObjects;

        /// <summary>
        /// ActiveRoom 클래스를 초기화합니다.
        /// 필요한 모든 리스트를 생성하고 레벨 오브젝트를 설정합니다.
        /// </summary>
        /// <param name="levelObject">활성화할 레벨 오브젝트</param>
        public static void Init(GameObject levelObject)
        {
            ActiveRoom.levelObject = levelObject;

            activeObjects = new List<GameObject>();
            enemies = new List<BaseEnemyBehavior>();
            chests = new List<AbstractChestBehavior>();
            customObjects = new List<GameObject>();
            exitPoints = new List<ExitPointBehaviour>();
        }

        /// <summary>
        /// 현재 월드와 레벨의 인덱스를 설정합니다.
        /// </summary>
        /// <param name="currentWorldIndex">설정할 월드 인덱스</param>
        /// <param name="currentLevelIndex">설정할 레벨 인덱스</param>
        public static void SetLevelData(int currentWorldIndex, int currentLevelIndex)
        {
            ActiveRoom.currentWorldIndex = currentWorldIndex;
            ActiveRoom.currentLevelIndex = currentLevelIndex;
        }

        /// <summary>
        /// 레벨 데이터를 설정합니다.
        /// </summary>
        /// <param name="levelData">설정할 레벨 데이터</param>
        public static void SetLevelData(LevelData levelData)
        {
            ActiveRoom.levelData = levelData;
        }

        /// <summary>
        /// 방 데이터를 설정합니다.
        /// </summary>
        /// <param name="roomData">설정할 방 데이터</param>
        public static void SetRoomData(RoomData roomData)
        {
            ActiveRoom.roomData = roomData;
        }

        /// <summary>
        /// 현재 활성화된 방의 모든 요소를 언로드합니다.
        /// 장애물, 상자, 적, 출구 포인트, 커스텀 오브젝트 등을 정리합니다.
        /// </summary>
        public static void Unload()
        {
            // 생성된 장애물 언로드
            if(!activeObjects.IsNullOrEmpty())
            {
                for (int i = 0; i < activeObjects.Count; i++)
                {
                    if(activeObjects[i] != null)
                    {
                        activeObjects[i].transform.SetParent(null);
                        activeObjects[i].SetActive(false);
                    }
                }

                activeObjects.Clear();
            }

            // 생성된 상자 언로드
            if (!chests.IsNullOrEmpty())
            {
                for (int i = 0; i < chests.Count; i++)
                {
                    if (chests[i] != null)
                    {
                        Object.Destroy(chests[i].gameObject);
                    }
                }

                chests.Clear();
            }

            // 적 언로드
            if (!enemies.IsNullOrEmpty())
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] != null)
                    {
                        enemies[i].Unload();

                        Object.Destroy(enemies[i].gameObject);
                    }
                }

                enemies.Clear();
            }

            // 출구 포인트 언로드
            if(!exitPoints.IsNullOrEmpty())
            {
                foreach(ExitPointBehaviour exitPoint in exitPoints)
                {
                    if(exitPoint != null)
                        exitPoint.Unload();
                }

                exitPoints.Clear();
            }

            // 커스텀 오브젝트 언로드
            UnloadCustomObjects();
        }

        #region 환경/장애물
        /// <summary>
        /// 아이템을 방에 스폰합니다.
        /// </summary>
        /// <param name="item">스폰할 아이템</param>
        /// <param name="itemEntityData">아이템의 위치, 회전, 크기 등의 데이터</param>
        public static void SpawnItem(LevelItem item, ItemEntityData itemEntityData)
        {
            GameObject itemObject = item.Pool.GetPooledObject();
            itemObject.transform.SetParent(levelObject.transform);
            itemObject.transform.SetPositionAndRotation(itemEntityData.Position, itemEntityData.Rotation);
            itemObject.transform.localScale = itemEntityData.Scale;
            itemObject.SetActive(true);

            activeObjects.Add(itemObject);
        }

        /// <summary>
        /// 출구 포인트를 등록하고 초기화합니다.
        /// </summary>
        /// <param name="exitPointBehaviour">등록할 출구 포인트 행동</param>
        public static void RegisterExitPoint(ExitPointBehaviour exitPointBehaviour)
        {
            exitPoints.Add(exitPointBehaviour);

            exitPointBehaviour.Init();
        }

        /// <summary>
        /// 상자를 방에 스폰합니다.
        /// </summary>
        /// <param name="chestEntityData">상자의 위치, 회전, 크기 등의 데이터</param>
        /// <param name="chestData">상자의 프리팹 및 기타 데이터</param>
        public static void SpawnChest(ChestEntityData chestEntityData, ChestData chestData)
        {
            GameObject chestObject = GameObject.Instantiate(chestData.Prefab);
            chestObject.transform.SetParent(levelObject.transform);
            chestObject.transform.SetPositionAndRotation(chestEntityData.Position, chestEntityData.Rotation);
            chestObject.transform.localScale = chestEntityData.Scale;
            chestObject.SetActive(true);

            chests.Add(chestObject.GetComponent<AbstractChestBehavior>());
        }

        #endregion

        #region 적 관련
        /// <summary>
        /// 적을 방에 생성합니다.
        /// </summary>
        /// <param name="enemyData">적의 데이터</param>
        /// <param name="enemyEntityData">적의 위치, 회전, 크기, 패트롤 경로 등의 데이터</param>
        /// <param name="isActive">생성 후 바로 활성화할지 여부</param>
        /// <returns>생성된 적의 행동 컴포넌트</returns>
        public static BaseEnemyBehavior SpawnEnemy(EnemyData enemyData, EnemyEntityData enemyEntityData, bool isActive)
        {
            BaseEnemyBehavior enemy = Object.Instantiate(enemyData.Prefab, enemyEntityData.Position, enemyEntityData.Rotation, levelObject.transform).GetComponent<BaseEnemyBehavior>();
            enemy.transform.localScale = enemyEntityData.Scale;
            enemy.SetEnemyData(enemyData, enemyEntityData.IsElite);
            enemy.SetPatrollingPoints(enemyEntityData.PathPoints);

            // 패스 포인트가 2개 이상이면 적을 첫 번째와 두 번째 웨이포인트 사이 중간에 배치
            if (enemyEntityData.PathPoints.Length > 1)
                enemy.transform.position = enemyEntityData.PathPoints[0] + (enemyEntityData.PathPoints[1] - enemyEntityData.PathPoints[0]) * 0.5f;

            if (isActive)
                enemy.Init();

            enemies.Add(enemy);

            return enemy;
        }

        /// <summary>
        /// 방에 있는 모든 적을 활성화합니다.
        /// </summary>
        public static void ActivateEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Init();
            }
        }

        /// <summary>
        /// 방에 있는 모든 적을 제거합니다.
        /// </summary>
        public static void ClearEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Unload();

                Object.Destroy(enemies[i].gameObject);
            }

            enemies.Clear();
        }

        /// <summary>
        /// 특별 보상을 드롭할 적을 선택합니다.
        /// 보스 > 엘리트 > 가장 멀리 있는 일반 적 순으로 선택됩니다.
        /// </summary>
        /// <returns>특별 보상을 드롭할 적</returns>
        public static BaseEnemyBehavior GetEnemyForSpecialReward()
        {
            BaseEnemyBehavior result = enemies.Find(e => e.Tier == EnemyTier.Boss);

            if (result != null)
                return result;

            result = enemies.Find(e => e.Tier == EnemyTier.Elite);

            if (result != null)
                return result;

            result = enemies[0];

            for (int i = 1; i < enemies.Count; i++)
            {
                if (enemies[i].transform.position.z > result.transform.position.z)
                {
                    result = enemies[i];
                }
            }

            return result;
        }

        /// <summary>
        /// 적과 상자의 드롭 아이템을 초기화합니다.
        /// </summary>
        /// <param name="enemyDrop">적이 드롭할 아이템 목록</param>
        /// <param name="chestDrop">상자에서 드롭할 아이템 목록</param>
        public static void InitDrop(List<DropData> enemyDrop, List<DropData> chestDrop)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].ResetDrop();
            }

            for (int i = 0; i < enemyDrop.Count; i++)
            {
                if (enemyDrop[i].DropType == DropableItemType.Currency && enemyDrop[i].CurrencyType == CurrencyType.Coins)
                {
                    List<int> coins = LevelController.SplitIntEqually(enemyDrop[i].Amount, enemies.Count);

                    for (int j = 0; j < enemies.Count; j++)
                    {
                        enemies[j].AddDrop(new DropData() { DropType = DropableItemType.Currency, CurrencyType = CurrencyType.Coins, Amount = coins[j] });
                    }
                }
                else
                {
                    GetEnemyForSpecialReward().AddDrop(enemyDrop[i]);
                }
            }

            for (int i = 0; i < chests.Count; i++)
            {
                chests[i].Init(chestDrop);
            }
        }

        /// <summary>
        /// 살아있는 적 목록을 반환합니다.
        /// </summary>
        /// <returns>살아있는 적 목록</returns>
        public static List<BaseEnemyBehavior> GetAliveEnemies()
        {
            List<BaseEnemyBehavior> result = new List<BaseEnemyBehavior>();

            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDead)
                {
                    result.Add(enemies[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// 모든 적이 죽었는지 확인합니다.
        /// </summary>
        /// <returns>모든 적이 죽었으면 true, 아니면 false</returns>
        public static bool AreAllEnemiesDead()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDead)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region 커스텀 오브젝트

        /// <summary>
        /// 커스텀 오브젝트를 방에 스폰합니다.
        /// </summary>
        /// <param name="objectData">스폰할 커스텀 오브젝트 데이터</param>
        public static void SpawnCustomObject(CustomObjectData objectData)
        {
            GameObject customObject = GameObject.Instantiate(objectData.PrefabRef);
            customObject.transform.SetParent(levelObject.transform);
            customObject.transform.SetPositionAndRotation(objectData.Position, objectData.Rotation);
            customObject.transform.localScale = objectData.Scale;
            customObject.SetActive(true);

            customObjects.Add(customObject);
        }

        /// <summary>
        /// 모든 커스텀 오브젝트를 언로드합니다.
        /// </summary>
        public static void UnloadCustomObjects()
        {
            if (customObjects.IsNullOrEmpty())
                return;

            for (int i = 0; i < customObjects.Count; i++)
            {
                if (customObjects[i] != null)
                    GameObject.Destroy(customObjects[i]);
            }

            customObjects.Clear();
        }

        #endregion
    }
}