// ==============================================
// 📌 SniperStateMachine.cs
// ✅ 스나이퍼 적 AI의 상태 전이 및 상태 정의 스크립트
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Sniper
{
    [RequireComponent(typeof(SniperEnemyBehavior))]
    public class SniperStateMachine : AbstractStateMachine<State>
    {
        private SniperEnemyBehavior enemy;

        /// <summary>
        /// 📌 상태 정의 및 전이 조건 초기화
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<SniperEnemyBehavior>();

            states.Add(State.Patrolling, new StateCase
            {
                state = new PatrollingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(PatrollingStateTransition)
                }
            });

            states.Add(State.Following, new StateCase
            {
                state = new FollowingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(FollowingStateTransition)
                }
            });

            states.Add(State.Fleeing, new StateCase
            {
                state = new FleeingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(FleeingStateTransition)
                }
            });

            states.Add(State.Attacking, new StateCase
            {
                state = new AimAndAttackState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(AttackingStateTransition)
                }
            });

            states.Add(State.Aiming, new StateCase
            {
                state = new SniperAimState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(AimTransition, StateTransitionType.OnFinish)
                }
            });
        }

        private bool PatrollingStateTransition(out State nextState)
        {
            if (!(enemy.IsTargetInVisionRange || enemy.HasTakenDamage))
            {
                nextState = State.Patrolling;
                return false;
            }

            nextState = enemy.IsTargetInFleeRange ? State.Fleeing :
                        enemy.IsTargetInAttackRange ? State.Aiming : State.Following;
            return true;
        }

        private bool FollowingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInFleeRange)
            {
                nextState = State.Fleeing;
                return true;
            }

            if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
            {
                nextState = State.Aiming;
                return true;
            }

            nextState = State.Following;
            return false;
        }

        private bool FleeingStateTransition(out State nextState)
        {
            nextState = enemy.IsTargetInAttackRange ? State.Fleeing : State.Following;
            return !enemy.IsTargetInAttackRange;
        }

        private bool AttackingStateTransition(out State nextState)
        {
            var attackState = states[State.Attacking].state as AimAndAttackState;

            if (attackState.IsFinished && !CharacterBehaviour.IsDead)
            {
                nextState = enemy.IsTargetInFleeRange ? State.Fleeing :
                            enemy.IsTargetInAttackRange ? State.Aiming :
                            State.Following;
                return true;
            }

            nextState = State.Attacking;
            return false;
        }

        private bool AimTransition(out State nextState)
        {
            nextState = State.Attacking;
            return true;
        }
    }
}
