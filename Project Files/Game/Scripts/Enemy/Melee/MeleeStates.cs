// ==============================================
// 📌 MeleeStates.cs
// ✅ 근접 공격 적 유닛의 상태(enum) 및 공격 상태 동작 정의
// ==============================================

using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Melee
{
    /// <summary>
    /// 근접 공격 적이 사용하는 상태 enum
    /// </summary>
    public enum State
    {
        Patrolling,
        Attacking,
    }

    /// <summary>
    /// 근접 적이 플레이어를 추적하고 공격하는 상태
    /// </summary>
    public class MeleeFollowAttackState : StateBehavior<MeleeEnemyBehaviour>
    {
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 cachedTargetPos;
        private bool isSlowed = false;
        private bool isAttacking = false;

        public MeleeFollowAttackState(MeleeEnemyBehaviour melee) : base(melee) { }

        /// <summary>
        /// 📌 추적 시작 시 타겟 위치 기억 및 이동
        /// </summary>
        public override void OnStart()
        {
            cachedTargetPos = Target.Target.position;

            isSlowed = Target.IsWalking;
            Target.NavMeshAgent.speed = isSlowed ? Target.Stats.PatrollingSpeed : Target.Stats.MoveSpeed;

            Target.MoveToPoint(cachedTargetPos);
            isAttacking = false;
        }

        /// <summary>
        /// 📌 타겟 위치 변경 시 재이동 및 공격 시도
        /// </summary>
        public override void OnUpdate()
        {
            if (Vector3.Distance(Target.Target.position, cachedTargetPos) > 0.1f)
            {
                cachedTargetPos = Target.Target.position;
                Target.MoveToPoint(cachedTargetPos);
            }

            // 슬로우 속도 전환 감지
            if (isSlowed != Target.IsWalking)
            {
                isSlowed = Target.IsWalking;
                Target.NavMeshAgent.speed = isSlowed ? Target.Stats.PatrollingSpeed : Target.Stats.MoveSpeed;
            }

            float speedRatio = Target.NavMeshAgent.velocity.magnitude / Target.NavMeshAgent.speed;
            float speedMult = isSlowed ? Target.Stats.PatrollingMutliplier : 1f;
            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, speedRatio * speedMult);

            // 공격 조건 만족 시 공격 실행
            if (Target.IsTargetInAttackRange && !isAttacking && !CharacterBehaviour.IsDead)
            {
                isAttacking = true;
                Target.Attack();
                Target.OnAttackFinished += OnAttackFinished;
            }
        }

        private void OnAttackFinished()
        {
            Target.OnAttackFinished -= OnAttackFinished;
            isAttacking = false;
        }

        /// <summary>
        /// 📌 상태 종료 시 이동 및 이벤트 정리
        /// </summary>
        public override void OnEnd()
        {
            Target.OnAttackFinished -= OnAttackFinished;
            Target.StopMoving();
        }
    }
}
