// ==============================================
// 📌 MeleeEnemyBehaviour.cs
// ✅ 근접 공격 적 유닛의 전투 및 공격 애니메이션 처리
// ✅ 슬로우 상태 적용 및 타격 이펙트 처리 포함
// ==============================================

using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class MeleeEnemyBehaviour : BaseEnemyBehavior
    {
        private static readonly int HIT_PARTICLE_HASH = "Enemy Melee Hit".GetHashCode();
        private readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [Header("Fighting")]
        [Tooltip("공격 유효 반경")]
        [SerializeField] private float hitRadius;

        [Tooltip("공격 후 감속 지속 시간")]
        [SerializeField] private DuoFloat slowDownDuration;

        [Tooltip("감속 시 이동 속도 배수")]
        [SerializeField] private float slowDownSpeedMult;

        [Space]
        [Tooltip("피격 이펙트 출력 위치")]
        [SerializeField] private Transform hitParticlePosition;

        private float slowRunningTimer;
        private bool isHitting = false;
        private bool isSlowRunning = false;

        /// <summary>
        /// 📌 공격 트리거. 중복 공격 방지
        /// </summary>
        public override void Attack()
        {
            if (isHitting) return;

            isHitting = true;
            ApplySlowDown();

            AudioController.PlaySound(AudioController.AudioClips.enemyMeleeHit, 0.5f);
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);
        }

        /// <summary>
        /// 📌 감속 적용
        /// </summary>
        private void ApplySlowDown()
        {
            isSlowRunning = true;
            IsWalking = true;
            slowRunningTimer = slowDownDuration.Random();
            navMeshAgent.speed = Stats.MoveSpeed * slowDownSpeedMult;
        }

        /// <summary>
        /// 📌 감속 해제
        /// </summary>
        private void DisableSlowDown()
        {
            isSlowRunning = false;
            IsWalking = false;
            navMeshAgent.speed = Stats.MoveSpeed;
        }

        /// <summary>
        /// 📌 체력바 위치 업데이트 및 감속 시간 계산
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isDead || !LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();

            if (isSlowRunning)
            {
                slowRunningTimer -= Time.deltaTime;
                if (slowRunningTimer <= 0)
                    DisableSlowDown();
            }
        }

        /// <summary>
        /// 📌 데미지 피격 처리 및 히트 애니메이션 재생
        /// </summary>
        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead) return;

            base.TakeDamage(damage, projectilePosition, projectileDirection);

            if (hitAnimationTime < Time.time)
                HitAnimation(Random.Range(0, 2));
        }

        /// <summary>
        /// 📌 공격 애니메이션 콜백 처리
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.Hit)
            {
                if (Vector3.Distance(transform.position, target.position) <= hitRadius)
                {
                    characterBehaviour.TakeDamage(GetCurrentDamage());
                    ParticlesController.PlayParticle(HIT_PARTICLE_HASH).SetPosition(hitParticlePosition.position);
                }
            }
            else if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                isHitting = false;
                InvokeOnAttackFinished();
            }
        }
    }
}
