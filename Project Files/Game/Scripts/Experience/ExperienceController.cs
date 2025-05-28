// ExperienceController.cs v1.03
// 📌 경험치 획득 및 레벨업 시스템 관리 클래스
// • 레벨업 시 펫 자동 언락 기능 포함

using System;
using UnityEngine;
using Watermelon;                   // SaveController, ExperienceSave
using Watermelon.LevelSystem;       // GameSettings
using Watermelon.SquadShooter;      // UC_PetDatabase, UC_PetSave, UIPetsPage

namespace Watermelon
{
    /// <summary>
    /// 📌 경험치 획득 및 레벨업 시스템 관리 클래스
    /// - 경험치 수집, 적용, 레벨업 판정 및 이벤트 발생 기능 제공
    /// - 레벨업 시 자동 펫 언락 로직 포함
    /// </summary>
    public class ExperienceController : MonoBehaviour
    {
        [Tooltip("경험치 획득 시 표시할 플로팅 텍스트 해시 (Stars)")]
        private static readonly int FLOATING_TEXT_HASH = "Stars".GetHashCode();

        [Tooltip("레벨별 경험치 요구량을 관리하는 데이터베이스")]
        private static ExperienceDatabase database;

        [Tooltip("플레이어 경험치 및 레벨 정보 저장 객체")]
        private static ExperienceSave save;

        [Tooltip("펫 언락 조건 확인을 위한 펫 데이터베이스")]
        private UC_PetDatabase petDatabase;

        /// <summary>현재 플레이어 레벨</summary>
        public static int CurrentLevel
        {
            get => save.CurrentLevel;
            private set => save.CurrentLevel = value;
        }

        /// <summary>현재 누적된 경험치 포인트</summary>
        public static int ExperiencePoints
        {
            get => save.CurrentExperiencePoints;
            private set => save.CurrentExperiencePoints = value;
        }

        /// <summary>이번 세션 중 수집한 경험치 (아직 적용되지 않은)</summary>
        public static int CollectedExperiencePoints
        {
            get => save.CollectedExperiencePoints;
            private set => save.CollectedExperiencePoints = value;
        }

        /// <summary>현재 레벨의 데이터 (ExperienceDatabase에서 조회)</summary>
        public static ExperienceLevelData CurrentLevelData => database.GetDataForLevel(CurrentLevel);

        /// <summary>다음 레벨의 데이터 (ExperienceDatabase에서 조회)</summary>
        public static ExperienceLevelData NextLevelData => database.GetDataForLevel(CurrentLevel + 1);

        /// <summary>경험치 획득 시 발생하는 이벤트 (획득량 전달)</summary>
        public static event SimpleIntCallback ExperienceGained;

        /// <summary>레벨업 시 발생하는 이벤트</summary>
        public static event SimpleCallback LevelIncreased;

        #region Unity Lifecycle
        private void Awake()
        {
            // 펫 데이터베이스 참조 및 레벨업 이벤트 구독
            petDatabase = GameSettings.GetSettings().PetDatabase;
            LevelIncreased += OnPlayerLevelUp;
        }
        #endregion

        #region Initialization
        /// <summary>경험치 시스템 초기화</summary>
        public void Init(ExperienceDatabase db)
        {
            database = db;
            database.Init();
            save = SaveController.GetSaveObject<ExperienceSave>("experience");
        }
        #endregion

        #region Experience Handling
        /// <summary>경험치 획득 및 플로팅 텍스트 표시</summary>

        public static void GainExperience(int amount)
        {
            CollectedExperiencePoints += amount;
            FloatingTextController.SpawnFloatingText(
                FLOATING_TEXT_HASH,
                $"+{amount}",
                CharacterBehaviour.Transform.position + new Vector3(3, 6, 0),
                Quaternion.identity,
                1f,
                Color.white
            );
        }

        /// <summary>누적된 경험치 적용 및 레벨업 판정</summary>
        public static void ApplyExperience()
        {
            if (CollectedExperiencePoints <= 0) return;

            int gained = CollectedExperiencePoints;
            ExperiencePoints += gained;
            CollectedExperiencePoints = 0;

            if (ExperiencePoints >= NextLevelData.ExperienceRequired)
            {
                CurrentLevel++;
                LevelIncreased?.Invoke();
            }

            ExperienceGained?.Invoke(gained);
        }
        #endregion

        /// <summary>지정된 레벨에 필요한 경험치 양 반환</summary>
        public static int GetXpPointsRequiredForLevel(int level)
        {
            return database.GetDataForLevel(level).ExperienceRequired;
        }

    #if UNITY_EDITOR
        /// <summary>개발용 강제 레벨 세팅 (디버그 목적)</summary>
        public static void SetLevelDev(int level)
        {
            CurrentLevel = level;
            ExperiencePoints = database.GetDataForLevel(level).ExperienceRequired;
            LevelIncreased?.Invoke();
        }
    #endif

        #region Pet Auto-Unlock Logic
        /// <summary>레벨업 시 자동 펫 언락 처리</summary>
        private void OnPlayerLevelUp()
        {
            bool unlockedAny = false;
            var petSave = SaveController.GetSaveObject<UC_PetSave>("pet");

            foreach (var pet in petDatabase.GetAllPets())
            {
                if (!petSave.HasPet(pet.petID) && CurrentLevel >= pet.requiredPlayerLevel)
                {
                    petSave.UnlockPet(pet.petID);
                    unlockedAny = true;
                }
            }

            if (unlockedAny)
            {
                SaveController.Save(forceSave: true);
                UIController.GetPage<UIPetsPage>()?.RefreshPanels();
            }
        }
        #endregion
    }
}
