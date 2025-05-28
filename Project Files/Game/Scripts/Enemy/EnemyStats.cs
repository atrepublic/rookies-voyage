// ==============================================
// 📌 EnemyStats.cs
// ✅ 개별 적 유닛의 능력치 및 난이도 기반 동적 계산 시스템
// ✅ 플레이어 능력치와 무기에 따라 적 체력/공격력/회복량 계산
// ✅ Lerp 기반의 범위형 수치 구조 사용 (DuoInt)
// ==============================================

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class EnemyStats
    {
        [Header("기본 체력 및 시야")]
        [Tooltip("기본 체력")]
        [SerializeField] private float hp;
        private int calculatedHp;

        /// <summary>실제 체력 (난이도 배수 포함)</summary>
        public float Hp => calculatedHp * difficulty.HealthMult;

        [Tooltip("적의 시야 범위")]
        [SerializeField] private float visionRange;
        public float VisionRange => visionRange;

        [Header("공격 및 도주")]
        [Tooltip("공격 거리")]
        [SerializeField] private float attackDistance;
        public float AttackDistance => attackDistance;

        [Tooltip("도주 거리")]
        [SerializeField] private float fleeDistance;
        public float FleeDistance => fleeDistance;

        [Tooltip("공격력 범위")]
        [SerializeField] private DuoInt damage;
        private DuoInt calculatedDamage;
        public DuoInt Damage => calculatedDamage * difficulty.DamageMult;

        [Tooltip("조준 유지 시간")]
        [SerializeField] private float aimDuration;
        public float AimDuration => aimDuration;

        [Header("이동 관련")]
        [Tooltip("이동 속도")]
        [SerializeField] private float moveSpeed;
        public float MoveSpeed => moveSpeed;

        [Tooltip("순찰 시 이동 속도")]
        [SerializeField] private float patrollingSpeed;
        public float PatrollingSpeed => patrollingSpeed;

        [Tooltip("순찰 시 애니메이션 속도 배수")]
        [SerializeField] private float patrollingMutliplier;
        public float PatrollingMutliplier => patrollingMutliplier;

        [Tooltip("순찰 지점 도착 후 대기 시간")]
        [SerializeField] private float patrollingIdleDuration;
        public float PatrollingIdleDuration => patrollingIdleDuration;

        [Tooltip("회전 속도")]
        [SerializeField] private float angularSpeed;
        public float AngularSpeed => angularSpeed;

        [Tooltip("플레이어와 유지하고 싶은 거리")]
        [SerializeField] private float preferedDistanceToPlayer;
        public float PreferedDistanceToPlayer => preferedDistanceToPlayer;

        [Header("엘리트 관련")]
        [Tooltip("적 레벨 (편집용)")]
        [SerializeField] private int level;
        public float Level => level;

        [Tooltip("엘리트 체력 배율")]
        [SerializeField] private float eliteHealthMult;
        public float EliteHealthMult => eliteHealthMult;

        [Tooltip("엘리트 공격력 배율")]
        [SerializeField] private float eliteDamageMult;
        public float EliteDamageMult => eliteDamageMult;

        [Header("기타")]
        [Tooltip("플레이어 회복용 수치")]
        [SerializeField] private DuoInt healForPlayer;

        private DuoInt calculatedHpForPlayer;
        public DuoInt HpForPlayer => calculatedHpForPlayer * difficulty.RestoredHpMult;

        [Space(5)]
        [Tooltip("타겟링 크기")]
        [SerializeField] private float targetRingSize = 1.0f;
        public float TargetRingSize => targetRingSize;

        [Tooltip("데미지 텍스트 Y 위치 오프셋")]
        [SerializeField] private float hitTextOffsetY = 17f;
        public float HitTextOffsetY => hitTextOffsetY;

        [Tooltip("데미지 텍스트 Z 오프셋")]
        [SerializeField] private float hitTextOffsetForward = 0f;
        public float HitTextOffsetForward => hitTextOffsetForward;

        // 계산용 내부 변수들
        private float enemyDmgToPlayerHp;
        private List<HpToWeaponRelation> enemyHpToCreatureDmgRelations;
        private float restoredHpToDamage;
        private float creatureDamage;
        private DifficultySettings difficulty;

        /// <summary>
        /// 📌 플레이어 체력 기준으로 적의 능력치 관계를 초기화
        /// </summary>
        public void InitStatsRelation(int baseCreatureHealth)
        {
            enemyDmgToPlayerHp = baseCreatureHealth / damage.Lerp(0.5f);

            enemyHpToCreatureDmgRelations = new List<HpToWeaponRelation>();
            foreach (WeaponData weapon in WeaponsController.Weapons)
            {
                var firstStage = weapon.Upgrades[1];
                float relation = hp / firstStage.Damage.Lerp(0.5f);
                enemyHpToCreatureDmgRelations.Add(new HpToWeaponRelation(weapon, relation));
            }

            restoredHpToDamage = healForPlayer.Lerp(0.5f) / damage.Lerp(0.5f);
        }

        /// <summary>
        /// 📌 현재 캐릭터 체력 및 무기 데미지를 기준으로 적 능력치를 계산하여 적용
        /// </summary>
        public void SetCurrentCreatureStats(int characterHealth, int weaponDmg, DifficultySettings difficulty)
        {
            this.creatureDamage = weaponDmg;
            this.difficulty = difficulty;

            WeaponData currentWeapon = WeaponsController.GetCurrentWeapon();
            HpToWeaponRelation relation = enemyHpToCreatureDmgRelations.Find(r => r.weapon.Equals(currentWeapon));

            // 체력 계산
            calculatedHp = Mathf.RoundToInt(creatureDamage * relation.enemyHpToCreatureDmg);

            // 데미지 계산 (범위 포함)
            float dmgMid = characterHealth / enemyDmgToPlayerHp;
            float damageSpreadUp = damage.secondValue / damage.Lerp(0.5f);
            float damageSpreadDown = damage.firstValue / damage.Lerp(0.5f);
            calculatedDamage = new DuoInt(
                Mathf.RoundToInt(dmgMid * damageSpreadDown),
                Mathf.RoundToInt(dmgMid * damageSpreadUp)
            );

            // 회복량 계산
            float restoredHpMid = dmgMid * restoredHpToDamage;
            float hpSpreadUp = healForPlayer.secondValue / healForPlayer.Lerp(0.5f);
            float hpSpreadDown = healForPlayer.firstValue / healForPlayer.Lerp(0.5f);
            calculatedHpForPlayer = new DuoInt(
                Mathf.RoundToInt(restoredHpMid * hpSpreadDown),
                Mathf.RoundToInt(restoredHpMid * hpSpreadUp)
            );
        }

        /// <summary>
        /// 무기와 적의 체력 관계를 나타내는 내부 구조체
        /// </summary>
        [System.Serializable]
        private class HpToWeaponRelation
        {
            public WeaponData weapon;
            public float enemyHpToCreatureDmg;

            public HpToWeaponRelation(WeaponData weapon, float relation)
            {
                this.weapon = weapon;
                this.enemyHpToCreatureDmg = relation;
            }
        }
    }

    /// <summary>
    /// 적 티어 등급 (일반 / 엘리트 / 보스)
    /// </summary>
    public enum EnemyTier
    {
        Regular = 0,
        Elite = 1,
        Boss = 2,
    }
}
