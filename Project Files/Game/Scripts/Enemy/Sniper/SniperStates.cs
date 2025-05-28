// ==============================================
// 📌 SniperStates.cs
// ✅ 스나이퍼 적 유닛의 상태(enum) 및 조준 상태 구현 스크립트
// ==============================================

using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Sniper
{
    /// <summary>
    /// 스나이퍼 적이 사용할 수 있는 상태
    /// </summary>
    public enum State
    {
        Patrolling, // 순찰
        Following,  // 추적
        Attacking,  // 발사
        Aiming,     // 조준 (노랑 → 빨강)
        Fleeing     // 도주
    }

    /// <summary>
    /// 스나이퍼 적이 조준을 수행하는 상태 (노랑 → 빨강 → 완료)
    /// </summary>
    public class SniperAimState : StateBehavior<SniperEnemyBehavior>
    {
        private SniperEnemyBehavior enemy;

        private bool isYellow;
        private TweenCase delayedCase;
        private float startAimingTime;

        public SniperAimState(SniperEnemyBehavior enemy) : base(enemy)
        {
            this.enemy = enemy;
        }

        /// <summary>
        /// 📌 조준 상태 시작: 노란 레이저 켜고 타이머 시작
        /// </summary>
        public override void OnStart()
        {
            isYellow = true;

            enemy.EnableLaser();

            delayedCase = Tween.DelayedCall(enemy.YellowAimingDuration, () =>
            {
                isYellow = false;
                enemy.MakeLaserRed();

                delayedCase = Tween.DelayedCall(enemy.RedAimingDuration, () =>
                {
                    InvokeOnFinished(); // 조준 완료 후 공격 상태로 전이
                });
            });

            startAimingTime = Time.time;
            enemy.Animator.SetBool("Aim", true);
        }

        /// <summary>
        /// 📌 조준 중일 때 타겟 바라보며 레이저 목표 갱신
        /// </summary>
        public override void OnUpdate()
        {
            if (isYellow || (!isYellow && !enemy.IsRedStatic))
            {
                enemy.Rotation = Quaternion.Lerp(
                    enemy.Rotation,
                    Quaternion.LookRotation((Target.TargetPosition - Position).normalized),
                    Time.deltaTime * 5f
                );
            }

            if (Time.time > startAimingTime + 0.25f)
            {
                enemy.AimLaser();
            }
        }

        /// <summary>
        /// 📌 상태 종료 시 레이저 끄고 트윈 정리
        /// </summary>
        public override void OnEnd()
        {
            delayedCase.KillActive();
            enemy.DisableLaser();
            enemy.Animator.SetBool("Aim", false);
        }
    }
}
