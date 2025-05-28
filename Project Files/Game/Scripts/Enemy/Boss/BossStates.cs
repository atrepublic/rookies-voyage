// ==============================================
// 📌 BossStates.cs
// ✅ 보스 적의 상태(enum) 및 각 상태 동작(StateBehavior) 클래스 정의
// ✅ 상태머신(BossStateMachine)과 연동되어 전투 흐름을 제어함
// ✅ Idle / Chasing / Shooting / Kicking / Entering / Hidden 상태 포함
// ==============================================

using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Boss
{
    /// <summary>
    /// 보스 상태 타입을 정의하는 열거형
    /// </summary>
    public enum State
    {
        Hidden,
        Entering,
        Idle,
        Chasing,
        Shooting,
        Hitting,
    }

    /// <summary>
    /// 모든 보스 상태의 기본 클래스
    /// </summary>
    public abstract class BossState : StateBehavior<BossBomberBehaviour>
    {
        [Tooltip("이 상태를 제어하는 보스 AI 객체")]
        protected BossBomberBehaviour boss;

        public BossState(BossBomberBehaviour enemy) : base(enemy)
        {
            boss = enemy;
        }
    }

    /// <summary>
    /// 📌 보스 등장 애니메이션 재생 상태
    /// </summary>
    public class EnteringState : BossState
    {
        public EnteringState(BossBomberBehaviour enemy) : base(enemy) { }

        /// <summary>
        /// 보스가 등장 애니메이션을 끝내면 상태 종료
        /// </summary>
        public override void OnStart()
        {
            boss.OnEntered += InvokeOnFinished;
        }

        public override void OnEnd()
        {
            boss.OnEntered -= InvokeOnFinished;
        }
    }

    /// <summary>
    /// 📌 대기 상태 (정지 및 비전투 상태)
    /// </summary>
    public class IdleState : BossState
    {
        public IdleState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.Idle();
        }
    }

    /// <summary>
    /// 📌 근접 발차기 상태
    /// </summary>
    public class KikkingState : BossState
    {
        private float kickEndTime;

        public KikkingState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.PerformKick();
            kickEndTime = Time.time + boss.HitCooldown;
        }

        public override void OnUpdate()
        {
            if (Time.time >= kickEndTime && !CharacterBehaviour.IsDead)
            {
                InvokeOnFinished();
            }
        }
    }

    /// <summary>
    /// 📌 폭탄 사격 상태
    /// </summary>
    public class ShootingState : BossState
    {
        private float shootingEndTime;

        public ShootingState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.PerformHit();
            shootingEndTime = Time.time + boss.HitCooldown;
        }

        public override void OnUpdate()
        {
            if (Time.time >= shootingEndTime && !CharacterBehaviour.IsDead)
            {
                InvokeOnFinished();
            }
        }
    }

    /// <summary>
    /// 📌 플레이어 추적 상태
    /// </summary>
    public class ChasingState : BossState
    {
        public ChasingState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.ChasePlayer();
        }
    }
}
