// ==============================================
// 📌 ShotgunnerStateMachine.cs
// ✅ 샷건 적 AI의 상태머신 구현 및 전이 조건 설정
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Shotgunner
{
    [RequireComponent(typeof(ShotgunerEnemyBehavior))]
    public class ShotgunnerStateMachine : AbstractStateMachine<State>
    {
        private ShotgunerEnemyBehavior enemy;

        /// <summary>
        /// 📌 상태 등록 및 전이 조건 정의
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<ShotgunerEnemyBehavior>();

            states.Add(State.Patrolling, new StateCase
            {
                state = new PatrollingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(PatrollingStateTransition)
                }
            });

            states.Add(State.Following, new StateCase
            {
                state = new FollowingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(FollowingStateTransition)
                }
            });

            states.Add(State.Attacking, new StateCase
            {
                state = new AimAndAttackState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(AttackingStateTransition)
                }
            });
        }

        /// <summary>
        /// 📌 순찰 중 타겟 발견 시 다음 상태 결정
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            if (!(enemy.IsTargetInVisionRange || enemy.HasTakenDamage))
            {
                nextState = State.Patrolling;
                return false;
            }

            nextState = (enemy.IsTargetInAttackRange && enemy.IsTargetInSight()) ? State.Attacking : State.Following;
            return true;
        }

        /// <summary>
        /// 📌 추적 중 공격 조건 만족 시 공격 상태로 전이
        /// </summary>
        private bool FollowingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
            {
                nextState = State.Attacking;
                return true;
            }

            nextState = State.Following;
            return false;
        }

        /// <summary>
        /// 📌 공격 상태 완료 후 재공격 또는 추적으로 전이
        /// </summary>
        private bool AttackingStateTransition(out State nextState)
        {
            var attackState = states[State.Attacking].state as AimAndAttackState;

            if (attackState.IsFinished && !CharacterBehaviour.IsDead)
            {
                nextState = (enemy.IsTargetInAttackRange && enemy.IsTargetInSight()) ? State.Attacking : State.Following;
                return true;
            }

            nextState = State.Attacking;
            return false;
        }
    }
}
