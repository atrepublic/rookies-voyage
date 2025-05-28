// ==============================================
// 📌 CommonStates.cs
// ✅ 모든 적 유닛이 공유하는 공통 상태(Patrol, Follow, Flee, Attack) 정의
// ✅ 상태머신에서 각 상태 인스턴스로 사용됨
// ==============================================

using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy
{
    /// <summary>
    /// 📌 순찰 상태 (순찰 지점을 따라 이동)
    /// </summary>
    public class PatrollingState : StateBehavior<BaseEnemyBehavior>
    {
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private bool isStationary = false;
        private int pointId = 0;
        private TweenCase idleCase;

        private Vector3 FirstPoint => Target.PatrollingPoints[0];

        public PatrollingState(BaseEnemyBehavior enemy) : base(enemy) { }

        public override void OnStart()
        {
            idleCase.KillActive();

            // 순찰 지점이 0개 또는 1개이며 현재 위치와 가까우면 정지 상태로 간주
            if (Target.PatrollingPoints.Length == 0 || (Target.PatrollingPoints.Length == 1 && Vector3.Distance(Position, FirstPoint) < 1))
            {
                isStationary = true;
            }
            else
            {
                pointId = 0;
                Target.NavMeshAgent.speed = Target.Stats.PatrollingSpeed;
                GoToPoint();
            }
        }

        private void GoToPoint()
        {
            var point = Target.PatrollingPoints[pointId];
            Target.MoveToPoint(point);
            isStationary = false;
        }

        public override void OnUpdate()
        {
            if (!isStationary && Vector3.Distance(Position, Target.PatrollingPoints[pointId]) < 1)
            {
                if (Target.PatrollingPoints.Length == 1)
                {
                    isStationary = true;
                    Target.NavMeshAgent.isStopped = true;
                }
                else
                {
                    pointId = (pointId + 1) % Target.PatrollingPoints.Length;
                    idleCase.KillActive();
                    idleCase = Tween.DelayedCall(Target.Stats.PatrollingIdleDuration, GoToPoint);
                }
            }

            float speed = Target.NavMeshAgent.velocity.magnitude / Target.NavMeshAgent.speed;
            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, speed * Target.Stats.PatrollingMutliplier);
        }

        public override void OnEnd()
        {
            Target.StopMoving();
            idleCase.KillActive();
        }
    }

    /// <summary>
    /// 📌 추적 상태 (타겟을 따라 이동)
    /// </summary>
    public class FollowingState : StateBehavior<BaseEnemyBehavior>
    {
        private readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");
        private Vector3 cachedTargetPos;
        private bool isSlowed = false;

        public FollowingState(BaseEnemyBehavior enemy) : base(enemy) { }

        public override void OnStart()
        {
            cachedTargetPos = Target.Target.position;
            isSlowed = Target.IsWalking;

            Target.NavMeshAgent.speed = isSlowed ? Target.Stats.PatrollingSpeed : Target.Stats.MoveSpeed;
            Target.MoveToPoint(cachedTargetPos);
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Target.Target.position, cachedTargetPos) > 0.5f)
            {
                cachedTargetPos = Target.Target.position;
                Target.MoveToPoint(cachedTargetPos);
            }

            bool nowWalking = Target.IsWalking;
            if (isSlowed && !nowWalking)
                Target.NavMeshAgent.speed = Target.Stats.MoveSpeed;
            else if (!isSlowed && nowWalking)
                Target.NavMeshAgent.speed = Target.Stats.PatrollingSpeed;

            isSlowed = nowWalking;

            float speed = Target.NavMeshAgent.velocity.magnitude / Target.NavMeshAgent.speed;
            float multiplier = isSlowed ? Target.Stats.PatrollingMutliplier : 1f;
            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, speed * multiplier);
        }

        public override void OnEnd()
        {
            Target.StopMoving();
        }
    }

    /// <summary>
    /// 📌 도주 상태 (플레이어로부터 일정 거리 유지하며 도망)
    /// </summary>
    public class FleeingState : StateBehavior<BaseEnemyBehavior>
    {
        private readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");
        private Vector3 fleePoint;

        public FleeingState(BaseEnemyBehavior enemy) : base(enemy) { }

        public override void OnStart()
        {
            Target.NavMeshAgent.speed = Target.Stats.MoveSpeed;
            fleePoint = GetRandomPointOnLevel();
            Target.MoveToPoint(fleePoint);
        }

        public override void OnUpdate()
        {
            float toFleePoint = Vector3.Distance(Target.transform.position, fleePoint);
            float toPlayer = Vector3.Distance(Target.TargetPosition, fleePoint);

            if (toFleePoint < 5f || toPlayer < Target.Stats.FleeDistance)
            {
                fleePoint = GetRandomPointOnLevel();
                Target.MoveToPoint(fleePoint);
            }

            float speed = Target.NavMeshAgent.velocity.magnitude / Target.Stats.MoveSpeed;
            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, speed);
        }

        public override void OnEnd()
        {
            Target.StopMoving();
        }

        private Vector3 GetRandomPointOnLevel()
        {
            int attempt = 0;

            while (attempt++ < 1000)
            {
                Vector3 testPoint = Target.Position + Random.onUnitSphere.SetY(0) * Random.Range(10, 100);

                if (UnityEngine.AI.NavMesh.SamplePosition(testPoint, out var hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    if (Vector3.Distance(Target.Target.position, testPoint) > Target.Stats.AttackDistance)
                        return testPoint;
                }
            }

            return Target.Position;
        }
    }

    /// <summary>
    /// 📌 단일 공격 상태 (애니메이션 기반 공격 1회 수행)
    /// </summary>
    public class AttackingState : StateBehavior<BaseEnemyBehavior>
    {
        public bool IsFinished { get; private set; }

        public AttackingState(BaseEnemyBehavior enemy) : base(enemy) { }

        public override void OnStart()
        {
            IsFinished = false;
            Target.OnAttackFinished += OnAttackFinished;
            Target.Attack();
        }

        public override void OnUpdate()
        {
            Vector3 direction = (Target.TargetPosition - Target.Position).SetY(0).normalized;
            Target.transform.rotation = Quaternion.LookRotation(direction);
        }

        public override void OnEnd()
        {
            Target.OnAttackFinished -= OnAttackFinished;
        }

        private void OnAttackFinished()
        {
            IsFinished = true;
        }
    }

    /// <summary>
    /// 📌 조준 후 공격 상태 (에임 후 공격 → 종료)
    /// </summary>
    public class AimAndAttackState : StateBehavior<BaseEnemyBehavior>
    {
        public bool IsFinished { get; private set; }

        private readonly RangeEnemyBehaviour rangeEnemy;
        private float nextAttackTime;
        private bool hasAttacked = false;

        private bool AimHash
        {
            set => Target.Animator.SetBool("Aim", value);
        }

        public AimAndAttackState(BaseEnemyBehavior enemy) : base(enemy)
        {
            rangeEnemy = enemy as RangeEnemyBehaviour;
        }

        public override void OnStart()
        {
            AimHash = true;
            IsFinished = false;

            Target.CanMove = false;
            nextAttackTime = Time.time + Target.Stats.AimDuration;

            if (rangeEnemy != null && rangeEnemy.CanReload)
                Target.OnReloadFinished += OnAttackFinished;
            else
                Target.OnAttackFinished += OnAttackFinished;

            hasAttacked = false;
        }

        public override void OnUpdate()
        {
            Quaternion targetRot = Quaternion.LookRotation((Target.TargetPosition - Position).normalized);
            Target.transform.rotation = Quaternion.Lerp(Target.transform.rotation, targetRot, Time.deltaTime * 10);

            if (!hasAttacked && Time.time > nextAttackTime)
            {
                Target.Attack();
                hasAttacked = true;
            }
        }

        public override void OnEnd()
        {
            AimHash = false;

            if (rangeEnemy != null && rangeEnemy.CanReload)
                Target.OnReloadFinished -= OnAttackFinished;
            else
                Target.OnAttackFinished -= OnAttackFinished;
        }

        private void OnAttackFinished()
        {
            IsFinished = true;
        }
    }
}
