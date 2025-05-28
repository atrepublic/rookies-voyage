// ==============================================
// 📌 RangeStateMachine.cs
// ✅ 원거리 적 AI의 상태 전이 제어 스크립트
// ✅ 순찰 → 추적 → 공격 → 도주 상태 간의 전이를 정의함
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Range
{
    [RequireComponent(typeof(RangeEnemyBehaviour))]
    public class RangeStateMachine : AbstractStateMachine<State>
    {
        [Tooltip("제어할 RangeEnemy 객체")]
        private RangeEnemyBehaviour enemy;

        /// <summary>
        /// 📌 상태 초기화 및 각 상태 전이 설정
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<RangeEnemyBehaviour>();

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

            states.Add(State.Fleeing, new StateCase
            {
                state = new FleeingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(FleeingStateTransition)
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
        /// 📌 순찰 상태 전이 조건 처리
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            if (!(enemy.IsTargetInVisionRange || enemy.HasTakenDamage))
            {
                nextState = State.Patrolling;
                return false;
            }

            nextState = enemy.IsTargetInFleeRange ? State.Fleeing :
                        enemy.IsTargetInAttackRange && enemy.IsTargetInSight() ? State.Attacking : State.Following;
            return true;
        }

        /// <summary>
        /// 📌 추적 상태 전이 조건 처리
        /// </summary>
        private bool FollowingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInFleeRange)
            {
                nextState = State.Fleeing;
                return true;
            }

            if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
            {
                nextState = State.Attacking;
                return true;
            }

            nextState = State.Following;
            return false;
        }

        /// <summary>
        /// 📌 도주 상태 전이 조건 처리
        /// </summary>
        private bool FleeingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInAttackRange)
            {
                nextState = State.Fleeing;
                return false;
            }

            nextState = State.Following;
            return true;
        }

        /// <summary>
        /// 📌 공격 상태 전이 조건 처리
        /// </summary>
        private bool AttackingStateTransition(out State nextState)
        {
            var attackingState = states[State.Attacking].state as AimAndAttackState;

            if (attackingState.IsFinished && !CharacterBehaviour.IsDead)
            {
                nextState = enemy.IsTargetInFleeRange ? State.Fleeing :
                            enemy.IsTargetInAttackRange && enemy.IsTargetInSight() ? State.Attacking :
                            State.Following;

                return true;
            }

            nextState = State.Attacking;
            return false;
        }
    }
}
