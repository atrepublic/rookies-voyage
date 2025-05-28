// ==============================================
// CharactersController.cs
// ==============================================
// 게임에 등장하는 모든 캐릭터의 정보를 관리하고,
// 선택/업그레이드/초기화 기능을 수행하는 캐릭터 시스템의 핵심 컨트롤러입니다.
// 전역 저장 데이터와 연동되어 마지막 선택된 캐릭터를 유지합니다.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class CharactersController : MonoBehaviour
    {
        [Tooltip("캐릭터 시스템의 기준 전투력 (Key Upgrade 0 기준)")]
        public static int BasePower { get; private set; }

        [Tooltip("현재 선택된 캐릭터 데이터")]
        private static CharacterData selectedCharacter;
        public static CharacterData SelectedCharacter => selectedCharacter;

        [Tooltip("현재 레벨에서 마지막으로 해금된 캐릭터")]
        public static CharacterData LastUnlockedCharacter => database.GetLastUnlockedCharacter();

        [Tooltip("다음 레벨에서 해금 예정인 캐릭터")]
        public static CharacterData NextCharacterToUnlock => database.GetNextCharacterToUnlock();

        [Tooltip("전역 캐릭터 저장 정보")]
        private static CharacterGlobalSave characterSave;

        [Tooltip("캐릭터 선택 시 호출되는 이벤트")]
        public static event CharacterCallback OnCharacterSelectedEvent;

        [Tooltip("캐릭터 업그레이드 시 호출되는 이벤트")]
        public static event CharacterCallback OnCharacterUpgradedEvent;

        [Tooltip("Key Upgrade가 설정된 캐릭터 업그레이드 목록")]
        private static List<CharacterUpgrade> keyUpgrades;

        [Tooltip("모든 캐릭터 정보를 담고 있는 데이터베이스")]
        private static CharactersDatabase database;

        /// <summary>
        /// 캐릭터 컨트롤러 초기화 함수
        /// </summary>
        public void Init(CharactersDatabase database)
        {
            CharactersController.database = database;

            // 캐릭터 데이터베이스 초기화
            database.Init();

            // 전역 저장 정보 불러오기
            characterSave = SaveController.GetSaveObject<CharacterGlobalSave>("characters");

            CharacterData saveCharacter = GetCharacter(characterSave.SelectedCharacterID);

            // Key Upgrade 리스트 수집
            keyUpgrades = new List<CharacterUpgrade>();
            foreach (var character in database.Characters)
            {
                foreach (var upgrade in character.Upgrades)
                {
                    if (upgrade.Stats.KeyUpgradeNumber != -1)
                    {
                        keyUpgrades.Add(upgrade);

                        if (upgrade.Stats.KeyUpgradeNumber == 0)
                        {
                            BasePower = upgrade.Stats.Power;
                        }
                    }
                }
            }

            // 정렬 (오름차순)
            keyUpgrades = keyUpgrades.OrderBy(u => u.Stats.KeyUpgradeNumber).ToList();

            // 저장된 캐릭터가 해금되어 있으면 로드, 아니면 기본 캐릭터 선택
            if (IsCharacterUnlocked(saveCharacter))
            {
                selectedCharacter = saveCharacter;
            }
            else
            {
                selectedCharacter = database.GetDefaultCharacter();
            }
        }

        /// <summary>
        /// 게임 종료 시 호출되어 참조 초기화
        /// </summary>
        private void OnDestroy()
        {
            BasePower = 0;
            selectedCharacter = null;
            characterSave = null;
            keyUpgrades = null;
            database = null;

            OnCharacterSelectedEvent = null;
            OnCharacterUpgradedEvent = null;
        }

        /// <summary>
        /// 해당 캐릭터가 현재 해금된 상태인지 판별
        /// </summary>
        public static bool IsCharacterUnlocked(CharacterData character)
        {
            return character != null && character.IsUnlocked();
        }

        /// <summary>
        /// 특정 캐릭터를 선택하고 적용
        /// </summary>
        public static void SelectCharacter(CharacterData character)
        {
            if (selectedCharacter == character || character == null)
                return;

            selectedCharacter = character;
            characterSave.SelectedCharacterID = character.ID;

            // 인게임 캐릭터 정보 적용
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                CharacterStageData characterStage = character.GetCurrentStage();
                CharacterUpgrade characterUpgrade = character.GetCurrentUpgrade();

                //characterBehaviour.SetStats(characterUpgrade.Stats);
                //characterBehaviour.SetGraphics(characterStage.Prefab, false, false);

                // [추가] 치명타 포함 전체 능력치 적용
                characterBehaviour.SetStats(characterUpgrade.Stats);
                characterBehaviour.SetGraphics(characterStage.Prefab, false, false);
            }

            // 선택 이벤트 호출
            OnCharacterSelectedEvent?.Invoke(selectedCharacter);
        }

        /// <summary>
        /// 캐릭터 업그레이드 시 호출
        /// </summary>
        public static void OnCharacterUpgraded(CharacterData character)
        {
            AudioController.PlaySound(AudioController.AudioClips.upgrade);
            OnCharacterUpgradedEvent?.Invoke(character);
        }

        /// <summary>
        /// 전체 캐릭터 데이터베이스 반환
        /// </summary>
        public static CharactersDatabase GetDatabase()
        {
            return database;
        }

        /// <summary>
        /// 캐릭터 ID로 해당 캐릭터 가져오기
        /// </summary>
        public static CharacterData GetCharacter(string characterID)
        {
            return database.GetCharacter(characterID);
        }

        /// <summary>
        /// 특정 캐릭터의 인덱스 반환
        /// </summary>
        public static int GetCharacterIndex(CharacterData character)
        {
            return System.Array.FindIndex(database.Characters, x => x == character);
        }

        /// <summary>
        /// 특정 KeyUpgrade 단계 이하에서 가장 높은 Power 반환
        /// </summary>
        public static int GetCeilingUpgradePower(int currentKeyUpgrade)
        {
            for (int i = keyUpgrades.Count - 1; i >= 0; i--)
            {
                if (keyUpgrades[i].Stats.KeyUpgradeNumber <= currentKeyUpgrade)
                {
                    return keyUpgrades[i].Stats.Power;
                }
            }

            return keyUpgrades[0].Stats.Power;
        }

        /// <summary>
        /// 캐릭터 선택/업그레이드 콜백 델리게이트
        /// </summary>
        public delegate void CharacterCallback(CharacterData character);
    }
}
