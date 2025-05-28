// ==============================================
// 📌 EnemyController.cs
// ✅ 게임 내 적의 능력치를 초기화하고 관리하는 컨트롤러
// ✅ 게임 시작 시 캐릭터 능력치에 맞춰 적 능력치를 동기화
// ✅ EnemiesDatabase와 연결된 중앙 관리자
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 적 유닛 관련 데이터 초기화 및 통합 관리 컨트롤러
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Tooltip("적 유닛 데이터베이스 (EnemiesDatabase ScriptableObject)")]
        [SerializeField] private EnemiesDatabase database;

        /// <summary>
        /// 현재 등록된 적 데이터베이스에 접근하기 위한 정적 프로퍼티
        /// </summary>
        public static EnemiesDatabase Database => instance.database;

        // 싱글턴 인스턴스
        private static EnemyController instance;

        /// <summary>
        /// 📌 적 컨트롤러 초기화 (기본 캐릭터 체력을 기준으로 적 능력치 관계 설정)
        /// </summary>
        public void Init()
        {
            instance = this;

            // 기본 캐릭터의 초기 체력 기준으로 적 능력치 관계 설정
            CharacterData baseCharacter = CharactersController.GetDatabase().GetDefaultCharacter();
            database.InitStatsRealation(baseCharacter.Upgrades[0].Stats.Health);
        }

        /// <summary>
        /// 📌 스테이지 시작 직전, 현재 선택된 캐릭터와 무기의 능력치로 적 능력치 설정
        /// </summary>
        public static void OnLevelWillBeStarted()
        {
            // 현재 선택된 캐릭터의 능력치
            CharacterStats characterStats = CharactersController.SelectedCharacter.GetCurrentUpgrade().Stats;

            // 현재 무기 데미지 기준으로 적 능력치 설정
            int weaponDamage = Mathf.RoundToInt(CharacterBehaviour.GetBehaviour().Weapon.Damage.Lerp(0.5f));
            Database.SetCurrentCharacterStats(characterStats.Health, weaponDamage); 
        }
    }
}
