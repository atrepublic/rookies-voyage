// ==============================================
// 📌 GrenaderStateMachine.cs
// ✅ 수류탄 적 유닛(Grenader)의 상태 전이 로직을 관리하는 스크립트
// ✅ 순찰 → 추적 → 공격/도주 상태를 전이 조건에 따라 제어
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Grenader
{
    [RequireComponent(typeof(GrenaderEnemyBehavior))]
    public class GrenaderStateMachine : AbstractStateMachine<State>
    {
        [Tooltip("제어 대상인 Grenader 적 유닛")]
        private GrenaderEnemyBehavior enemy;

        /// <summary>
        /// 📌 상태 등록 및 전이 조건 초기화
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<GrenaderEnemyBehavior>();

            var patrolling = new StateCase
            {
                state = new PatrollingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(PatrollingStateTransition)
                }
            };

            var following = new StateCase
            {
                state = new FollowingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(FollowingStateTransition)
                }
            };

            var fleeing = new StateCase
            {
                state = new FleeingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(FleeingStateTransition)
                }
            };

            var attacking = new StateCase
            {
                state = new AttackingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(AttackingStateTransition)
                }
            };

            states.Add(State.Patrolling, patrolling);
            states.Add(State.Following, following);
            states.Add(State.Fleeing, fleeing);
            states.Add(State.Attacking, attacking);
        }

        /// <summary>
        /// 📌 순찰 중 타겟 발견 → 추적/공격/도주로 전이
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            if (!(enemy.IsTargetInVisionRange || enemy.HasTakenDamage))
            {
                nextState = State.Patrolling;
                return false;
            }

            if (enemy.IsTargetInFleeRange)
                nextState = State.Fleeing;
            else if (enemy.IsTargetInAttackRange)
                nextState = State.Attacking;
            else
                nextState = State.Following;

            return true;
        }

        /// <summary>
        /// 📌 추적 중 도망거리 또는 공격 가능 거리 진입 시 전이
        /// </summary>
        private bool FollowingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInFleeRange)
            {
                nextState = State.Fleeing;
                return true;
            }

            if (enemy.IsTargetInAttackRange && !CharacterBehaviour.IsDead)
            {
                nextState = State.Attacking;
                return true;
            }

            nextState = State.Following;
            return false;
        }

        /// <summary>
        /// 📌 도주 중 공격 가능 거리 진입하면 도주 유지, 아니면 추적으로 전환
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
        /// 📌 공격 완료 시 다음 상태 전이 판단
        /// </summary>
        private bool AttackingStateTransition(out State nextState)
        {
            if ((states[State.Attacking].state as AttackingState).IsFinished && !CharacterBehaviour.IsDead)
            {
                if (enemy.IsTargetInFleeRange)
                    nextState = State.Fleeing;
                else if (enemy.IsTargetInAttackRange)
                    nextState = State.Attacking;
                else
                    nextState = State.Following;

                return true;
            }

            nextState = State.Attacking;
            return false;
        }
    }
}
