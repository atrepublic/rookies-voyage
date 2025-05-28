// ==============================================
// 📌 RifleStateMachine.cs
// ✅ 라이플 적 AI의 상태 전이 제어 스크립트
// ✅ 상태별 조건에 따라 순찰 → 추적 → 공격/도주로 이동
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Rifle
{
    [RequireComponent(typeof(RifleEnemyBehavior))]
    public class RifleStateMachine : AbstractStateMachine<State>
    {
        [Tooltip("제어할 라이플 적 객체")]
        private RifleEnemyBehavior enemy;

        /// <summary>
        /// 📌 상태 등록 및 전이 조건 설정
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<RifleEnemyBehavior>();

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
        /// 📌 순찰 상태에서 타겟 발견 시 다음 상태 전이
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            if (!(enemy.IsTargetInVisionRange || enemy.HasTakenDamage))
            {
                nextState = State.Patrolling;
                return false;
            }

            nextState = enemy.IsTargetInFleeRange ? State.Fleeing :
                        enemy.IsTargetInAttackRange ? State.Attacking :
                        State.Following;

            return true;
        }

        /// <summary>
        /// 📌 추적 중 전투 또는 도주로 전이
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
        /// 📌 도주 중 추적으로 전환 조건
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
        /// 📌 공격 완료 시 다음 상태 결정
        /// </summary>
        private bool AttackingStateTransition(out State nextState)
        {
            var attackState = states[State.Attacking].state as AimAndAttackState;

            if (attackState.IsFinished && !CharacterBehaviour.IsDead)
            {
                nextState = enemy.IsTargetInFleeRange ? State.Fleeing :
                            enemy.IsTargetInAttackRange ? State.Attacking :
                            State.Following;
                return true;
            }

            nextState = State.Attacking;
            return false;
        }
    }
}
