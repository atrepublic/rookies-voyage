// ==============================================
// 📌 BossSniperStates.cs
// ✅ 스나이퍼 보스의 상태 및 각 상태별 로직을 정의한 스크립트
// ✅ StateMachine과 연동되어 이동 → 조준 → 발사 흐름을 구현
// ==============================================

using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.BossSniper
{
    /// <summary>
    /// 보스 스나이퍼가 다음 공격 위치로 이동하는 상태
    /// </summary>
    public class BossSniperChangingPositionState : StateBehavior<BossSniperBehavior>
    {
        private int positionId = 0;
        private Vector3 nextPosition;

        // 애니메이터 파라미터 해시
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        public BossSniperChangingPositionState(BossSniperBehavior enemy) : base(enemy) { }

        /// <summary>
        /// 📌 가장 가까운 다음 순찰 지점으로 이동 시작
        /// </summary>
        public override void OnStart()
        {
            positionId++;
            if (positionId >= Target.PatrollingPoints.Length)
                positionId = 0;

            nextPosition = Target.PatrollingPoints[positionId];
            Target.MoveToPoint(nextPosition);
        }

        /// <summary>
        /// 📌 목표 지점에 도달하면 상태 종료
        /// </summary>
        public override void OnUpdate()
        {
            if (Vector3.Distance(Target.Position, nextPosition) < 1f && !CharacterBehaviour.IsDead)
            {
                InvokeOnFinished();
            }

            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, Target.NavMeshAgent.velocity.magnitude / Target.Stats.MoveSpeed);
        }

        /// <summary>
        /// 📌 이동 종료 후 정지
        /// </summary>
        public override void OnEnd()
        {
            Target.StopMoving();
        }
    }

    /// <summary>
    /// 보스 스나이퍼가 조준하는 상태 (노란 → 빨간 레이저 단계)
    /// </summary>
    public class BossSniperAimState : StateBehavior<BossSniperBehavior>
    {
        private BossSniperBehavior boss;

        private bool isYellow;
        private TweenCase delayedCase;
        private float startAimingTime;
        private bool isAimingFinished = false;

        public BossSniperAimState(BossSniperBehavior enemy) : base(enemy)
        {
            this.boss = enemy;
        }

        /// <summary>
        /// 📌 노란 레이저 → 빨간 레이저 단계로 전환하며 조준 시작
        /// </summary>
        public override void OnStart()
        {
            isAimingFinished = false;
            isYellow = true;

            boss.MakeLaserYellow();

            delayedCase = Tween.DelayedCall(boss.YellowLaserAinimgDuration, () =>
            {
                isYellow = false;
                boss.MakeLaserRed();

                delayedCase = Tween.DelayedCall(boss.RedLaserAimingDuration, () =>
                {
                    isAimingFinished = true;
                });
            });

            startAimingTime = Time.time;
            boss.Animator.SetBool("Aim", true);
        }

        /// <summary>
        /// 📌 레이저가 목표 방향으로 조준하며 경로 계산
        /// </summary>
        public override void OnUpdate()
        {
            if (isYellow || (!isYellow && boss.CanAimDuringRedLaserStage))
            {
                boss.Rotation = Quaternion.Lerp(
                    boss.Rotation,
                    Quaternion.LookRotation((Target.TargetPosition - Position).normalized),
                    Time.deltaTime * 5f
                );
            }

            if (Time.time > startAimingTime + 0.25f)
            {
                boss.AimLaser();
            }

            if (isAimingFinished && !CharacterBehaviour.IsDead)
            {
                InvokeOnFinished();
            }
        }

        /// <summary>
        /// 📌 조준 종료 시 레이저 비활성화 및 트윈 제거
        /// </summary>
        public override void OnEnd()
        {
            delayedCase.KillActive();
            boss.DisableLaser();
            boss.Animator.SetBool("Aim", false);
        }
    }

    /// <summary>
    /// 보스 스나이퍼가 실제 발사를 수행하는 상태
    /// </summary>
    public class BossSniperAttackState : StateBehavior<BossSniperBehavior>
    {
        public BossSniperAttackState(BossSniperBehavior enemy) : base(enemy) { }

        /// <summary>
        /// 📌 공격 애니메이션 트리거 및 콜백 등록
        /// </summary>
        public override void OnStart()
        {
            Target.Attack();
            Target.OnAttackFinished += OnAttackEnded;
        }

        /// <summary>
        /// 📌 발사 완료 시 상태 전이 호출
        /// </summary>
        private void OnAttackEnded()
        {
            InvokeOnFinished();
        }

        /// <summary>
        /// 📌 상태 종료 시 콜백 해제
        /// </summary>
        public override void OnEnd()
        {
            Target.OnAttackFinished -= OnAttackEnded;
        }
    }

    /// <summary>
    /// 보스 스나이퍼 상태 종류 열거형
    /// </summary>
    public enum BossSniperStates
    {
        ChangingPosition,
        Aiming,
        Shooting
    }
}
