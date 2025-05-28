using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    /// <summary>
    /// LevelController 클래스는 게임의 레벨 시스템을 총괄하는 정적 관리자입니다.
    /// 레벨의 로딩, 활성화, 언로딩, 플레이어 스폰, 적 관리, 보상 분배 등 게임 레벨과 관련된
    /// 모든 기능을 제공합니다. 또한 월드 데이터를 관리하고 플레이어의 진행 상황을 저장합니다.
    /// 이 클래스는 게임 전체의 레벨 흐름을 제어하고 플레이어와 적의 상호작용을 관리합니다.
    /// </summary>
    public static class LevelController
    {

        /// <summary>레벨이 시작된 직후 호출되는 이벤트</summary>
        public static event SimpleCallback OnLevelStartedEvent;

        // <summary>새 방(Room) 진입 시 호출되는 이벤트</summary>
        public static event SimpleCallback OnRoomStartedEvent;

        // 현재 활성화된 방의 인덱스(0부터 시작)
        public static int CurrentRoomIndex { get; private set; }


        /// <summary>
        /// 영역 표시 텍스트 형식 (예: "AREA 1-2")
        /// </summary>
        public const string AREA_TEXT = "AREA {0}-{1}";

        /// <summary>
        /// 레벨 데이터베이스 참조
        /// </summary>
        private static LevelsDatabase levelsDatabase;
        
        /// <summary>
        /// 레벨 데이터베이스에 접근하기 위한 프로퍼티
        /// </summary>
        public static LevelsDatabase LevelsDatabase => levelsDatabase;

        /// <summary>
        /// 게임의 레벨 관련 설정
        /// </summary>
        private static GameSettings levelSettings;
        
        /// <summary>
        /// 레벨 설정에 접근하기 위한 프로퍼티
        /// </summary>
        public static GameSettings LevelSettings => levelSettings;

        /// <summary>
        /// 레벨의 게임 오브젝트
        /// </summary>
        private static GameObject levelGameObject;
        
        /// <summary>
        /// 레벨 게임 오브젝트에 접근하기 위한 프로퍼티
        /// </summary>
        public static GameObject LevelGameObject => levelGameObject;

        /// <summary>
        /// 플레이어가 뒤로 가는 것을 방지하는 벽 콜라이더
        /// </summary>
        private static GameObject backWallCollider;

        /// <summary>
        /// 레벨이 로드되었는지 여부
        /// </summary>
        private static bool isLevelLoaded;
        
        /// <summary>
        /// 현재 로드된 레벨 데이터
        /// </summary>
        private static LevelData loadedLevel;

        /// <summary>
        /// 레벨 저장 데이터
        /// </summary>
        private static LevelSave levelSave;

        /// <summary>
        /// 현재 활성화된 레벨의 데이터
        /// </summary>
        private static LevelData currentLevelData;
        
        /// <summary>
        /// 현재 레벨 데이터에 접근하기 위한 프로퍼티
        /// </summary>
        public static LevelData CurrentLevelData => currentLevelData;

        /// <summary>
        /// 현재 방의 인덱스
        /// </summary>
        private static int currentRoomIndex;

        // 플레이어 관련
        /// <summary>
        /// 플레이어 캐릭터의 행동 컴포넌트
        /// </summary>
        private static CharacterBehaviour characterBehaviour;
        
        /// <summary>
        /// 플레이어 오브젝트
        /// </summary>
        private static GameObject playerObject;

        // 월드 데이터
        /// <summary>
        /// 현재 활성화된 월드의 데이터
        /// </summary>
        private static WorldData activeWorldData;

        // 게임플레이 관련
        /// <summary>
        /// 수동 출구 활성화 여부
        /// </summary>
        private static bool manualExitActivation;
        
        /// <summary>
        /// 출구가 사용되었는지 여부
        /// </summary>
        private static bool isExitEntered;

        /// <summary>
        /// 현재 레벨에서 수집한 코인의 총량
        /// </summary>
        private static int lastLevelMoneyCollected;

        /// <summary>
        /// 게임플레이가 활성화되었는지 여부
        /// </summary>
        private static bool isGameplayActive;
        
        /// <summary>
        /// 게임플레이 활성화 상태에 접근하기 위한 프로퍼티
        /// </summary>
        public static bool IsGameplayActive => isGameplayActive;

        /// <summary>
        /// 캐릭터 제안이 필요한지 여부
        /// </summary>
        private static bool needCharacterSugession;
        
        /// <summary>
        /// 캐릭터 제안 필요 여부에 접근하기 위한 프로퍼티
        /// </summary>
        public static bool NeedCharacterSugession => needCharacterSugession;

        // 드롭 관련
        /// <summary>
        /// 각 방의 보상 목록
        /// </summary>
        private static List<List<DropData>> roomRewards;
        
        /// <summary>
        /// 각 방의 상자 보상 목록
        /// </summary>
        private static List<List<DropData>> roomChestRewards;

        // 이벤트
        /// <summary>
        /// 플레이어가 레벨을 나갈 때 발생하는 이벤트
        /// </summary>
        public static event SimpleCallback OnPlayerExitLevelEvent;
        
        /// <summary>
        /// 플레이어가 사망했을 때 발생하는 이벤트
        /// </summary>
        public static event SimpleCallback OnPlayerDiedEvent;

        /// <summary>
        /// LevelController를 초기화합니다.
        /// 게임 세팅을 로드하고 레벨 데이터베이스를 초기화하며, 현재 레벨 정보를 설정합니다.
        /// </summary>
        public static void Init()
        {
            isLevelLoaded = false;
            isGameplayActive = false;
            isExitEntered = false;
            manualExitActivation = false;

            lastLevelMoneyCollected = 0;
            currentRoomIndex = 0;

            levelSettings = GameSettings.GetSettings();

            levelsDatabase = levelSettings.LevelsDatabase;
            levelsDatabase.Init();

            levelSave = SaveController.GetSaveObject<LevelSave>("level");

            // 현재 레벨 저장
            currentLevelData = levelsDatabase.GetLevel(levelSave.WorldIndex, levelSave.LevelIndex);
        }

        /// <summary>
        /// 레벨 오브젝트를 생성하고 관련 컴포넌트를 초기화합니다.
        /// 뒷벽 콜라이더를 배치하고 네비게이션 메시를 설정합니다.
        /// </summary>
        public static void CreateLevelObject()
        {
            levelGameObject = new GameObject("[LEVEL]");
            levelGameObject.transform.ResetGlobal();

            backWallCollider = Object.Instantiate(levelSettings.BackWallCollider, Vector3.forward * -1000f, Quaternion.identity, levelGameObject.transform);

            NavMeshController.Init(levelGameObject, levelSettings.NavMeshData);

            ActiveRoom.Init(levelGameObject);
        }

        /// <summary>
        /// 현재 레벨을 언로드하고 모든 데이터를 초기화합니다.
        /// 월드 데이터가 있다면 언로드합니다.
        /// </summary>
        public static void Unload()
        {
            // 활성화된 월드 언로드
            if (activeWorldData != null)
            {
                activeWorldData.UnloadWorld();
            }

            isLevelLoaded = false;
            isGameplayActive = false;
            isExitEntered = false;
            manualExitActivation = false;

            lastLevelMoneyCollected = 0;
            currentRoomIndex = 0;
        }

        /// <summary>
        /// 플레이어 캐릭터를 스폰하고 초기화합니다.
        /// 선택된 캐릭터의 스탯, 그래픽, 무기를 설정합니다.
        /// </summary>
        /// <returns>초기화된 캐릭터 행동 컴포넌트</returns>
        public static CharacterBehaviour SpawnPlayer()
        {
            CharacterData character = CharactersController.SelectedCharacter;
            
            CharacterStageData characterStage = character.GetCurrentStage();
            CharacterUpgrade characterUpgrade = character.GetCurrentUpgrade();

            // 플레이어 스폰
            playerObject = Object.Instantiate(levelSettings.PlayerPrefab);
            playerObject.name = "[CHARACTER]";

            characterBehaviour = playerObject.GetComponent<CharacterBehaviour>();
            characterBehaviour.SetStats(characterUpgrade.Stats);
            characterBehaviour.Init();

            characterBehaviour.SetGraphics(characterStage.Prefab, false, false);

            WeaponData weapon = WeaponsController.GetCurrentWeapon();

            characterBehaviour.SetGun(weapon, weapon.GetCurrentUpgrade());

            return characterBehaviour;
        }

        /// <summary>
        /// 현재 저장된 레벨 정보를 기반으로 레벨을 로드합니다.
        /// 또한 레벨의 저장 파일을 생성합니다.
        /// </summary>
        public static void LoadCurrentLevel()
        {
            LoadLevel(levelSave.WorldIndex, levelSave.LevelIndex);

            SavePresets.CreateSave("Level " + (levelSave.WorldIndex + 1).ToString("00") + "-" + (levelSave.LevelIndex + 1).ToString("00"), "Levels");
        }

        /// <summary>
        /// 지정된 월드와 레벨 인덱스에 해당하는 레벨을 로드합니다.
        /// 방 데이터를 초기화하고 레벨에 필요한 모든 설정을 준비합니다.
        /// </summary>
        /// <param name="worldIndex">로드할 월드 인덱스</param>
        /// <param name="levelIndex">로드할 레벨 인덱스</param>
        public static void LoadLevel(int worldIndex, int levelIndex)
        {
            if (isLevelLoaded)
                return;

            isLevelLoaded = true;
            isExitEntered = false;

            LevelData levelData = levelsDatabase.GetLevel(worldIndex, levelIndex);

            ActiveRoom.SetLevelData(levelData);

            currentLevelData = levelData;
            currentLevelData.OnLevelInitialised();

            ActiveRoom.SetLevelData(worldIndex, levelIndex);

            WorldData world = levelData.World;
            ActivateWorld(world);

            BalanceController.UpdateDifficulty(false);

            lastLevelMoneyCollected = 0;

            Control.DisableMovementControl();

            UIGame uiGame = UIController.GetPage<UIGame>();
            uiGame.UpdateCoinsText(CurrencyController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);
            uiGame.InitRoomsUI(levelData.Rooms);

            currentRoomIndex = 0;
            DistributeRewardBetweenRooms();

            // 첫 번째 방 로드
            LoadRoom(currentRoomIndex);

            characterBehaviour.DisableAgent();
        }

        /// <summary>
        /// 레벨을 활성화하고 게임플레이를 시작합니다.
        /// 카메라를 활성화하고 캐릭터를 배치한 후 적들을 활성화합니다.
        /// </summary>
        /// <param name="completeCallback">활성화 완료 후 호출될 콜백</param>
        public static void ActivateLevel(SimpleCallback completeCallback = null)
        {
            EnemyController.OnLevelWillBeStarted();

            GameController.PlayCustomMusic(activeWorldData.UniqueWorldMusicClip);

            isGameplayActive = true;

            CameraController.EnableCamera(CameraType.Game);

            currentRoomIndex = 0;
            lastLevelMoneyCollected = 0;

            UIGame uiGame = UIController.GetPage<UIGame>();
            uiGame.UpdateCoinsText(CurrencyController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);

            characterBehaviour.SetPosition(CurrentLevelData.Rooms[currentRoomIndex].SpawnPoint);

            NavMeshController.InvokeOrSubscribe(new NavMeshCallback(delegate
            {
                characterBehaviour.Activate();
                characterBehaviour.ActivateMovement();
                characterBehaviour.ActivateAgent(); 
                
                ActiveRoom.ActivateEnemies();

                Control.EnableMovementControl();

                currentLevelData.OnLevelStarted();

                // 📢 레벨 시작 이벤트 발행,레벨 시작 시점에만 발행
                OnLevelStartedEvent?.Invoke();
                completeCallback?.Invoke();


                UIGamepadButton.DisableAllTags();
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);

                completeCallback?.Invoke();
            }));
        }

        /// <summary>
        /// 레벨의 보상을 각 방과 상자에 분배합니다.
        /// 코인과 특별 보상을 방과 상자에 균등하게 나누어 할당합니다.
        /// </summary>
        private static void DistributeRewardBetweenRooms()
        {
            int roomsAmount = currentLevelData.Rooms.Length;
            int chestsAmount = currentLevelData.GetChestsAmount();

            List<int> moneyPerRoomOrChest = new List<int>();
            DropData coinsReward;

            // 코인 보상 금액 찾기
            coinsReward = currentLevelData.DropData.Find(d => d.DropType == DropableItemType.Currency && d.CurrencyType == CurrencyType.Coins);

            if (coinsReward != null)
            {
                // 모든 방과 상자에 코인 보상을 균등하게 분배
                moneyPerRoomOrChest = SplitIntEqually(coinsReward.Amount, roomsAmount + chestsAmount);
            }

            roomRewards = new List<List<DropData>>();
            roomChestRewards = new List<List<DropData>>();

            // 각 방에 개별적으로 보상 생성
            for (int i = 0; i < roomsAmount; i++)
            {
                roomRewards.Add(new List<DropData>());

                // 돈 보상이 있으면 이 방의 부분 할당
                if (moneyPerRoomOrChest.Count > 0)
                {
                    if (moneyPerRoomOrChest[i] > 0)
                    {
                        roomRewards[i].Add(new DropData() { DropType = DropableItemType.Currency, CurrencyType = CurrencyType.Coins, Amount = moneyPerRoomOrChest[i] });
                    }
                }

                // 마지막 방이면 특별 보상 제공
                if (i == roomsAmount - 1)
                {
                    for (int j = 0; j < currentLevelData.DropData.Count; j++)
                    {
                        // 코인이 아니면 특별 보상으로 간주
                        if (!(currentLevelData.DropData[j].DropType == DropableItemType.Currency && currentLevelData.DropData[j].CurrencyType == CurrencyType.Coins))
                        {
                            bool skipThisReward = false;

                            // 무기가 이미 잠금 해제되었으면 무기 카드 건너뛰기
                            if (currentLevelData.DropData[j].DropType == DropableItemType.WeaponCard && WeaponsController.IsWeaponUnlocked(currentLevelData.DropData[j].Weapon))
                            {
                                skipThisReward = true;
                            }

                            if (!skipThisReward)
                                roomRewards[i].Add(currentLevelData.DropData[j]);
                        }
                    }
                }
            }

            int chestsSpawned = 0;

            for (int i = 0; i < roomsAmount; i++)
            {
                var room = currentLevelData.Rooms[i];

                if (room.ChestEntities != null && room.ChestEntities.Length > 0)
                {
                    for (int j = 0; j < room.ChestEntities.Length; j++)
                    {
                        var chest = room.ChestEntities[j];

                        if (chest.IsInited)
                        {
                            if (chest.ChestType == LevelChestType.Standart)
                            {
                                roomChestRewards.Add(new List<DropData>()
                                {
                                    new DropData() { DropType = DropableItemType.Currency, CurrencyType = CurrencyType.Coins, Amount = moneyPerRoomOrChest[roomsAmount + chestsSpawned] }
                                });

                                chestsSpawned++;
                            }
                            else
                            {
                                roomChestRewards.Add(new List<DropData>()
                                {
                                    new DropData() { DropType = DropableItemType.Currency, CurrencyType = CurrencyType.Coins, Amount = coinsReward.Amount }
                                });
                            }
                        }
                        else
                        {
                            roomChestRewards.Add(new List<DropData>());
                        }
                    }
                }
                else
                {
                    roomChestRewards.Add(new List<DropData>());
                }
            }
        }

        /// <summary>
        /// 다음 방이 존재하는지 확인합니다.
        /// </summary>
        /// <returns>다음 방이 존재하면 true, 그렇지 않으면 false</returns>
        private static bool DoesNextRoomExist()
        {
            if (isLevelLoaded)
            {
                return currentLevelData.Rooms.IsInRange(currentRoomIndex + 1);
            }

            return false;
        }

        /// <summary>
        /// 지정된 인덱스의 방을 로드합니다.
        /// 방에 있는 모든 아이템, 적, 상자, 커스텀 오브젝트를 스폰합니다.
        /// </summary>
        /// <param name="index">로드할 방의 인덱스</param>
        private static void LoadRoom(int index)
        {
            // 전역 프로퍼티에 현재 방 번호를 기록
            CurrentRoomIndex = index;

            RoomData roomData = currentLevelData.Rooms[index];

            ActiveRoom.SetRoomData(roomData);

            Drop.OnRoomLoaded();

            backWallCollider.transform.localPosition = roomData.SpawnPoint;

            manualExitActivation = false;
            isExitEntered = false;

            // 플레이어 위치 재조정
            characterBehaviour.SetPosition(roomData.SpawnPoint);
            characterBehaviour.Reload(index == 0);

            NavMeshController.InvokeOrSubscribe(characterBehaviour);

            ItemEntityData[] items = roomData.ItemEntities;
            if (!items.IsNullOrEmpty())
            {
                for (int i = 0; i < items.Length; i++)
                {
                    LevelItem itemData = activeWorldData.GetLevelItem(items[i].Hash);

                    if (itemData == null)
                    {
                        Debug.Log("[Level Controller] Not found item with hash: " + items[i].Hash + " for the world: " + activeWorldData.name);
                        continue;
                    }

                    ActiveRoom.SpawnItem(itemData, items[i]);
                }
            }

            EnemyEntityData[] enemies = roomData.EnemyEntities;
            if (!enemies.IsNullOrEmpty())
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    ActiveRoom.SpawnEnemy(EnemyController.Database.GetEnemyData(enemies[i].EnemyType), enemies[i], false);
                }
            }

            if (!roomData.ChestEntities.IsNullOrEmpty())
            {
                for (int i = 0; i < roomData.ChestEntities.Length; i++)
                {
                    var chest = roomData.ChestEntities[i];

                    if (chest.IsInited)
                    {
                        ActiveRoom.SpawnChest(chest, LevelSettings.GetChestData(chest.ChestType));
                    }
                }
            }

            CustomObjectData[] roomCustomObjects = roomData.RoomCustomObjects;
            if (!roomCustomObjects.IsNullOrEmpty())
            {
                for (int i = 0; i < roomCustomObjects.Length; i++)
                {
                    ActiveRoom.SpawnCustomObject(roomCustomObjects[i]);
                }
            }

            CustomObjectData[] worldCustomObjects = levelsDatabase.GetWorld(levelSave.WorldIndex).WorldCustomObjects;
            if (!worldCustomObjects.IsNullOrEmpty())
            {
                for (int i = 0; i < worldCustomObjects.Length; i++)
                {
                    ActiveRoom.SpawnCustomObject(worldCustomObjects[i]);
                }
            }

            ActiveRoom.InitDrop(roomRewards[index], roomChestRewards[index]);

            currentLevelData.OnLevelLoaded();
            currentLevelData.OnRoomEntered();

            loadedLevel = currentLevelData;

            NavMeshController.RecalculateNavMesh(null);

            GameLoading.MarkAsReadyToHide();



            // ★ 방 진입 이벤트 발행
            OnRoomStartedEvent?.Invoke();
        }

        /// <summary>
        /// 사망한 캐릭터를 부활시킵니다.
        /// 플레이어를 현재 방의 스폰 포인트로 이동시키고 재활성화합니다.
        /// </summary>
        public static void ReviveCharacter()
        {
            characterBehaviour.SetPosition(CurrentLevelData.Rooms[currentRoomIndex].SpawnPoint);

            isGameplayActive = true;

            characterBehaviour.Reload();
            characterBehaviour.Activate(false);
            characterBehaviour.SetPosition(CurrentLevelData.Rooms[currentRoomIndex].SpawnPoint);
            characterBehaviour.ResetDetector();

            if (levelSettings.InvulnerabilityAfrerReviveDuration > 0) characterBehaviour.MakeInvulnerable(levelSettings.InvulnerabilityAfrerReviveDuration);

            Control.EnableMovementControl();
        }

        /// <summary>
        /// 레벨 실패 시 호출되는 메서드입니다.
        /// </summary>
        public static void OnLevelFailed()
        {
            currentLevelData.OnLevelFailed();
        }

        /// <summary>
        /// 현재 방을 다시 로드합니다.
        /// 모든 적과 오브젝트를 초기화하고 처음부터 시작합니다.
        /// </summary>
        public static void ReloadRoom()
        {
            if (!isLevelLoaded)
                return;

            NavMeshController.ClearAgents();

            characterBehaviour.Disable();
            characterBehaviour.Reload();

            // 모든 적 제거
            ActiveRoom.ClearEnemies();

            Drop.OnRoomLoaded();

            isExitEntered = false;

            currentRoomIndex = 0;

            UIGame uiGame = UIController.GetPage<UIGame>();
            uiGame.UpdateReachedRoomUI(currentRoomIndex);

            RoomData roomData = currentLevelData.Rooms[currentRoomIndex];

            EnemyEntityData[] enemies = roomData.EnemyEntities;
            for (int i = 0; i < enemies.Length; i++)
            {
                ActiveRoom.SpawnEnemy(EnemyController.Database.GetEnemyData(enemies[i].EnemyType), enemies[i], false);
            }

            ActiveRoom.InitDrop(roomRewards[currentRoomIndex], roomChestRewards[currentRoomIndex]);

            currentLevelData.OnRoomEntered();

            characterBehaviour.gameObject.SetActive(true);
            characterBehaviour.SetPosition(roomData.SpawnPoint);

            NavMeshController.InvokeOrSubscribe(characterBehaviour);
        }

        /// <summary>
        /// 현재 레벨을 언로드합니다.
        /// 모든 활성화된 요소를 정리하고 리소스를 해제합니다.
        /// </summary>
        public static void UnloadLevel()
        {
            if (!isLevelLoaded)
                return;

            NavMeshController.Reset();

            Drop.DestroyActiveObjects();

            characterBehaviour.Disable();

            loadedLevel.OnLevelUnloaded();

            ActiveRoom.Unload();

            isLevelLoaded = false;
            isExitEntered = false;
            loadedLevel = null;
        }

        /// <summary>
        /// 지정된 월드 데이터를 활성화합니다.
        /// 이전 월드가 있으면 언로드하고 새 월드를 로드합니다.
        /// </summary>
        /// <param name="data">활성화할 월드 데이터</param>
        private static void ActivateWorld(WorldData data)
        {
            if (activeWorldData != null && activeWorldData.Equals(data))
                return;

            // 활성화된 프리셋 언로드
            if (activeWorldData != null)
            {
                activeWorldData.UnloadWorld();
            }

            // 데이터베이스에서 새 프리셋 가져오기
            activeWorldData = data;

            // 새 프리셋 활성화
            activeWorldData.LoadWorld();
        }

        /// <summary>
        /// 수동 출구 활성화 모드를 활성화합니다.
        /// </summary>
        public static void EnableManualExitActivation()
        {
            manualExitActivation = true;
        }

        /// <summary>
        /// 출구를 활성화합니다.
        /// 모든 적이 죽었을 때만 출구가 활성화됩니다.
        /// </summary>
        public static void ActivateExit()
        {
            if (ActiveRoom.AreAllEnemiesDead())
            {
                List<ExitPointBehaviour> exitPoints = ActiveRoom.ExitPoints;
                if (!exitPoints.IsNullOrEmpty())
                {
                    foreach (ExitPointBehaviour exitPoint in exitPoints)
                    {
                        exitPoint.OnExitActivated();
                    }
                }
                else
                {
                    AudioController.PlaySound(AudioController.AudioClips.complete);

                    LevelController.OnPlayerExitLevel();
                }

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
            }
        }

        /// <summary>
        /// 플레이어가 레벨을 나갈 때 호출되는 메서드입니다.
        /// 현재 방에서 다음 방으로 이동하거나 레벨을 완료합니다.
        /// </summary>
        public static void OnPlayerExitLevel()
        {
            if (isExitEntered) return;

            isExitEntered = true;

            Drop.AutoCollect();

            OnPlayerExitLevelEvent?.Invoke();

            characterBehaviour.MoveForwardAndDisable(0.3f);

            Control.DisableMovementControl();

            currentRoomIndex++;

            currentLevelData.OnRoomLeaved();

            if (currentLevelData.Rooms.IsInRange(currentRoomIndex))
            {
                Overlay.Show(0.3f, () =>
                {
                    UIGame uiGame = UIController.GetPage<UIGame>();
                    uiGame.UpdateReachedRoomUI(currentRoomIndex);

                    ActiveRoom.Unload();

                    NavMeshController.Reset();

                    LoadRoom(currentRoomIndex);

                    NavMeshController.InvokeOrSubscribe(new NavMeshCallback(delegate
                    {
                        Control.EnableMovementControl();

                        characterBehaviour.Activate();
                        characterBehaviour.ActivateAgent();
                        ActiveRoom.ActivateEnemies();
                    }));

                    Overlay.Hide(0.3f, null);
                });
            }
            else
            {
                UIGame uiGame = UIController.GetPage<UIGame>();
                uiGame.UpdateReachedRoomUI(currentRoomIndex);

                OnLevelCompleted();
            }
        }

        /// <summary>
        /// 적이 죽었을 때 호출되는 메서드입니다.
        /// 수동 출구 활성화 모드가 아니면 출구를 활성화합니다.
        /// </summary>
        /// <param name="enemyBehavior">죽은 적의 행동 컴포넌트</param>
        public static void OnEnemyKilled(BaseEnemyBehavior enemyBehavior)
        {
            if (!manualExitActivation)
            {
                ActivateExit();
            }
        }

        /// <summary>
        /// 코인을 획득했을 때 호출되는 메서드입니다.
        /// </summary>
        /// <param name="amount">획득한 코인의 양</param>

        public static void OnCoinPicked(int amount)
        {
            lastLevelMoneyCollected += amount;

            // UIController를 통해 UIGame 페이지 인스턴스를 가져옵니다.
            UIGame uiGame = UIController.GetPage<UIGame>();
            if (uiGame != null) // UI 페이지가 활성화 상태인지 확인하는 것이 좋습니다.
            {
                // 현재 보유 코인 + 이번 레벨에서 얻은 코인을 합산하여 UI 텍스트를 업데이트합니다.
                uiGame.UpdateCoinsText(CurrencyController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);
            }
        }

        /// <summary>
        /// 보상형 코인(예: 광고 시청 보상) 획득 시 호출됩니다.
        /// 즉시 전체 코인 보유량에 추가하고, UI에 반영합니다.
        /// </summary>
        /// <param name="amount">획득한 보상 코인 개수</param>
        public static void OnRewardedCoinPicked(int amount)
        {
            // CurrencyController를 통해 코인 재화를 직접 증가시킵니다.
            CurrencyController.Add(CurrencyType.Coins, amount);

            // UIController를 통해 UIGame 페이지 인스턴스를 가져옵니다.
            UIGame uiGame = UIController.GetPage<UIGame>();
            if (uiGame != null)
            {
                // 현재 보유 코인 + 이번 레벨에서 얻은 코인을 합산하여 UI 텍스트를 업데이트합니다.
                // 참고: 보상형 코인은 lastLevelMoneyCollected와 별개로 즉시 반영되므로, UI 업데이트 로직 통일성을 위해 함께 표시될 수 있습니다.
                //       만약 보상 코인이 UI에 즉시 반영되는 것이 더 자연스럽다면 아래처럼 변경할 수 있습니다.
                //       uiGame.UpdateCoinsText(CurrencyController.Get(CurrencyType.Coins)); // 이 경우 lastLevelMoneyCollected는 더하지 않습니다.
                uiGame.UpdateCoinsText(CurrencyController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);
            }
        }

        /// <summary>
        /// 레벨 완료 시 호출됩니다.
        /// 게임 플레이 상태를 비활성화하고, 보상을 지급하며, UI를 업데이트하고, 다음 레벨 정보를 준비하며, 게임 상태를 저장합니다.
        /// </summary>
        public static void OnLevelCompleted()
        {
            isGameplayActive = false; // 게임 플레이 비활성화

            // 보상 적용
            // 현재 레벨 데이터에서 코인 보상을 가져와 재화에 추가합니다.
            CurrencyController.Add(CurrencyType.Coins, CurrentLevelData.GetCoinsReward());
            // 현재 레벨 데이터에서 카드 보상을 가져와 무기 카드에 추가합니다.
            WeaponsController.AddCards(CurrentLevelData.GetCardsReward());

            // 레벨 완료 UI 페이지를 가져옵니다.
            UIComplete uiComplete = UIController.GetPage<UIComplete>();
            if (uiComplete != null)
            {
                // 완료 UI에 획득한 경험치를 표시하도록 업데이트합니다.
                uiComplete.UpdateExperienceLabel(currentLevelData.XPAmount);
            }

            // 캐릭터 추천 UI 표시 여부를 초기화/설정합니다.
            InitCharacterSuggestion();

            // 저장 데이터의 레벨 인덱스를 다음 레벨로 증가시킵니다.
            IncreaseLevelInSave();

            // 변경 사항이 있으므로 저장이 필요함을 표시합니다.
            SaveController.MarkAsSaveIsRequired();

            // GameController에 레벨 완료 이벤트를 전달합니다.
            GameController.LevelComplete();

            // 현재 레벨 데이터 객체에 레벨 완료 처리를 위임합니다. (추가적인 레벨별 로직이 있을 수 있음)
            currentLevelData.OnLevelCompleted();
        }

        /// <summary>
        /// 외부에서 캐릭터 추천 UI 표시를 비활성화할 때 사용합니다.
        /// </summary>
        public static void DisableCharacterSuggestion()
        {
            needCharacterSugession = false;
        }

        /// <summary>
        /// 캐릭터 추천 UI를 표시할 조건인지 확인하고, 필요하다면 관련 데이터를 설정합니다.
        /// 현재 레벨이 캐릭터 추천 기능을 가지고 있고, 잠금 해제할 다음 캐릭터가 있을 때 활성화됩니다.
        /// </summary>
        private static void InitCharacterSuggestion()
        {
            // 현재 레벨 데이터에 캐릭터 추천 정보가 없으면 추천 기능을 사용하지 않습니다.
            if (!currentLevelData.HasCharacterSuggestion)
            {
                needCharacterSugession = false;
                return;
            }

            // 마지막으로 잠금 해제된 캐릭터와 다음에 해제될 캐릭터 정보를 가져옵니다.
            CharacterData lastUnlockedCharacter = CharactersController.LastUnlockedCharacter;
            CharacterData nextCharacterToUnlock = CharactersController.NextCharacterToUnlock;

            // 두 캐릭터 정보 중 하나라도 없으면 추천 기능을 사용하지 않습니다.
            if (lastUnlockedCharacter == null || nextCharacterToUnlock == null)
            {
                needCharacterSugession = false;
                return;
            }

            // 각 캐릭터 해금에 필요한 경험치 요구량을 가져옵니다.
            int lastXpRequirement = ExperienceController.GetXpPointsRequiredForLevel(lastUnlockedCharacter.RequiredLevel);
            int nextXpRequirement = ExperienceController.GetXpPointsRequiredForLevel(nextCharacterToUnlock.RequiredLevel);

            // 다음 캐릭터 해금까지의 진행률을 계산합니다. (레벨 완료 전과 후)
            // (다음 요구량 - 이전 요구량)이 0이 되는 경우를 방지하기 위해 분모 확인이 필요할 수 있습니다.
            float denominator = nextXpRequirement - lastXpRequirement;
            if (denominator <= 0) // 분모가 0 이하면 진행률 계산 불가
            {
                needCharacterSugession = false;
                return;
            }

            float lastProgression = (float)(ExperienceController.ExperiencePoints - lastXpRequirement) / denominator;
            float currentProgression = (float)(ExperienceController.ExperiencePoints + currentLevelData.XPAmount - lastXpRequirement) / denominator;

            // 캐릭터 추천 UI에 필요한 데이터(이전 진행률, 현재 진행률, 다음 해금 캐릭터)를 설정합니다.
            UICharacterSuggestion.SetData(lastProgression, currentProgression, nextCharacterToUnlock);

            // 캐릭터 추천 UI를 표시하도록 플래그를 설정합니다.
            needCharacterSugession = true;
        }

        /// <summary>
        /// 저장된 레벨 정보를 다음 레벨로 업데이트합니다.
        /// 현재 월드의 마지막 레벨이면 다음 월드의 첫 번째 레벨로 설정합니다.
        /// </summary>
        private static void IncreaseLevelInSave()
        {
            // 레벨 데이터베이스를 통해 현재 월드/레벨 다음에 레벨이 더 존재하는지 확인합니다.
            if (levelsDatabase.DoesNextLevelExist(levelSave.WorldIndex, levelSave.LevelIndex))
            {
                // 다음 레벨이 존재하면 레벨 인덱스만 증가시킵니다.
                levelSave.LevelIndex++;
            }
            else
            {
                // 다음 레벨이 없으면 (현재 월드의 마지막 레벨이면) 월드 인덱스를 증가시키고 레벨 인덱스를 0으로 초기화합니다.
                levelSave.WorldIndex++;
                levelSave.LevelIndex = 0;
            }
        }

        /// <summary>
        /// 플레이어 사망 시 호출됩니다.
        /// 게임 플레이 상태가 활성화되어 있을 때만 작동합니다.
        /// 게임 플레이를 중지하고, 사망 관련 이벤트를 호출하며, 이동 제어를 비활성화하고, 레벨 실패 처리를 시작합니다.
        /// </summary>
        public static void OnPlayerDied()
        {
            // 이미 게임 플레이가 비활성화 상태라면 아무것도 하지 않습니다. (중복 호출 방지)
            if (!IsGameplayActive)
                return;

            isGameplayActive = false; // 게임 플레이 비활성화

            // 플레이어 사망 관련 이벤트가 등록되어 있다면 호출합니다. (?. 연산자는 null 체크 축약형)
            OnPlayerDiedEvent?.Invoke();

            // 플레이어의 이동 제어를 비활성화합니다.
            Control.DisableMovementControl();

            // GameController에 레벨 실패 이벤트를 전달합니다.
            GameController.OnLevelFailded();
        }

        /// <summary>
        /// 주어진 정수 값을 지정된 개수만큼 최대한 동일하게 분할하여 리스트로 반환합니다.
        /// 예를 들어 10을 3개의 부분으로 나누면 [3, 3, 4] 를 반환합니다.
        /// </summary>
        /// <param name="value">분할할 전체 값</param>
        /// <param name="partsAmount">나눌 부분의 개수</param>
        /// <returns>분할된 값들이 담긴 정수 리스트</returns>
        public static List<int> SplitIntEqually(int value, int partsAmount)
        {
            List<int> result = new List<int>();

            // 나눌 개수가 0보다 클 때만 연산 수행
            if (partsAmount > 0)
            {
                // 각 부분에 할당될 기본 크기를 계산합니다. (소수점 이하 버림)
                int part = Mathf.FloorToInt((float)value / partsAmount);
                int sum = 0; // 분배된 값의 합계를 추적

                // 지정된 개수만큼 기본 크기(part)를 리스트에 추가합니다.
                for (int i = 0; i < partsAmount; i++)
                {
                    result.Add(part);
                    sum += part;
                }

                // 분배하고 남은 나머지 값(value - sum)이 있다면, 리스트의 마지막 요소에 더해줍니다.
                // 이렇게 하면 총합이 원래 값(value)과 같아집니다.
                if (sum < value)
                {
                    // result.Count - 1 은 리스트의 마지막 인덱스를 가리킵니다.
                    result[result.Count - 1] += value - sum;
                }
            }
            // partsAmount가 0 이하인 경우 빈 리스트가 반환됩니다.

            return result;
        }

        #region 개발용 함수 (Dev)
        // 이 영역의 함수들은 개발 및 테스트 목적으로만 사용됩니다.

        /// <summary>
        /// [개발용] 현재 레벨을 강제로 완료 처리하고 다음 레벨로 이동합니다.
        /// 캐릭터 추천 UI는 표시하지 않습니다.
        /// </summary>
        public static void NextLevelDev()
        {
            needCharacterSugession = false; // 캐릭터 추천 비활성화
            IncreaseLevelInSave(); // 저장된 레벨 증가
            GameController.OnLevelCompleteClosed(); // 레벨 완료 UI가 닫혔을 때의 후처리 호출 (가정)
        }

        /// <summary>
        /// [개발용] 이전 레벨로 강제로 이동합니다.
        /// 캐릭터 추천 UI는 표시하지 않습니다.
        /// </summary>
        public static void PrevLevelDev()
        {
            needCharacterSugession = false; // 캐릭터 추천 비활성화
            DecreaseLevelInSaveDev(); // 저장된 레벨 감소 (개발용 함수)
            GameController.OnLevelCompleteClosed(); // 레벨 완료 UI가 닫혔을 때의 후처리 호출 (가정)
        }

        /// <summary>
        /// [개발용] 저장된 레벨 정보를 이전 레벨로 업데이트합니다.
        /// 0월드 0레벨 미만으로 내려가지 않도록 방지합니다.
        /// </summary>
        private static void DecreaseLevelInSaveDev()
        {
            levelSave.LevelIndex--; // 레벨 인덱스 감소

            // 레벨 인덱스가 0보다 작아지면 (이전 월드로 이동)
            if (levelSave.LevelIndex < 0)
            {
                levelSave.WorldIndex--; // 월드 인덱스 감소

                // 월드 인덱스가 0보다 작아지면 0으로 유지 (가장 첫 월드)
                if (levelSave.WorldIndex < 0)
                {
                    levelSave.WorldIndex = 0;
                }

                // 이전 월드의 마지막 레벨 인덱스를 설정해야 하지만, 이 정보가 없으므로
                // 여기서는 간단하게 0으로 설정하거나, 실제 레벨 수를 기반으로 설정해야 합니다.
                // 예시: levelSave.LevelIndex = levelsDatabase.GetLastLevelIndex(levelSave.WorldIndex);
                // 여기서는 임시로 0으로 설정합니다. (주의: 실제 게임 로직에 맞게 수정 필요)
                levelSave.LevelIndex = 0; // 또는 이전 월드의 마지막 레벨 인덱스로 설정
            }
        }

        #endregion
    }
}