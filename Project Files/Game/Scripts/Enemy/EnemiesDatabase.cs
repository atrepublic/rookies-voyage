// ==============================================
// 📌 EnemiesDatabase.cs
// ✅ 게임 내 등장하는 모든 적 유닛 데이터를 보관하는 데이터베이스
// ✅ ScriptableObject로 제작되어 프로젝트 설정에서 쉽게 관리 가능
// ✅ 캐릭터 능력치에 따른 적 능력치 자동 조정 기능 포함
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 적 유닛 데이터를 저장하는 데이터베이스 클래스 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "Enemies Database", menuName = "Data/Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [Tooltip("게임에 등장하는 적 유닛들의 데이터 배열")]
        [SerializeField] private EnemyData[] enemies;

        /// <summary>
        /// 등록된 모든 적 데이터
        /// </summary>
        public EnemyData[] Enemies => enemies;

        /// <summary>
        /// 📌 적 능력치 간 관계를 캐릭터 기본 체력을 기준으로 초기화
        /// </summary>
        /// <param name="baseCharacterHealth">기준이 되는 캐릭터 체력</param>
        public void InitStatsRealation(int baseCharacterHealth)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].Stats.InitStatsRelation(baseCharacterHealth);
            }
        }

        /// <summary>
        /// 📌 현재 캐릭터 체력 및 무기 공격력을 기반으로 적 능력치를 재설정
        /// </summary>
        /// <param name="characterHealth">현재 캐릭터 체력</param>
        /// <param name="weaponDmg">현재 무기 공격력</param>
        public void SetCurrentCharacterStats(int characterHealth, int weaponDmg)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].Stats.SetCurrentCreatureStats(characterHealth, weaponDmg, BalanceController.CurrentDifficulty);
            }
        }

        /// <summary>
        /// 📌 특정 타입의 적 데이터 반환
        /// </summary>
        /// <param name="type">찾고자 하는 적 타입</param>
        /// <returns>해당 타입의 EnemyData</returns>
        public EnemyData GetEnemyData(EnemyType type)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType.Equals(type))
                    return enemies[i];
            }

            Debug.LogError("[Enemies Database] Enemy of type " + type + " is not found!");
            return enemies.Length > 0 ? enemies[0] : null;
        }
    }
}
