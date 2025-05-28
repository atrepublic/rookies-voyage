// ==============================================
// 📌 CowboyStateMachine.cs
// ✅ 카우보이 적의 상태머신 컨트롤러
// ✅ 순찰, 추적, 도주, 공격 상태 간 전이를 관리
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Cowboy
{
    [RequireComponent(typeof(CowboyEnemyBehavior))]
    public class CowboyStateMachine : AbstractStateMachine<State>
    {
        [Tooltip("상태를 제어할 대상 카우보이 적 캐릭터")]
        private CowboyEnemyBehavior enemy;

        /// <summary>
        /// 📌 상태 초기화 및 상태별 전이 조건 등록
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<CowboyEnemyBehavior>();

            // 순찰 상태 정의 및 등록
            var patrollingStateCase = new StateCase
            {
                state = new PatrollingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(PatrollingStateTransition)
                }
            };

            // 추적 상태 정의 및 등록
            var followingStateCase = new StateCase
            {
                state = new FollowingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(FollowingStateTransition)
                }
            };

            // 도주 상태 정의 및 등록
            var fleeingStateCase = new StateCase
            {
                state = new FleeingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(FleeingStateTransition)
                }
            };

            // 공격 상태 정의 및 등록
            var attackingStateCase = new StateCase
            {
                state = new AimAndAttackState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(AttackingStateTransition)
                }
            };

            states.Add(State.Patrolling, patrollingStateCase);
            states.Add(State.Following, followingStateCase);
            states.Add(State.Fleeing, fleeingStateCase);
            states.Add(State.Attacking, attackingStateCase);
        }

        /// <summary>
        /// 📌 순찰 상태에서 타겟을 발견하면 상태 전이
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            bool isTargetSpotted = enemy.IsTargetInVisionRange || enemy.HasTakenDamage;

            if (!isTargetSpotted)
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
        /// 📌 추적 상태 중 도망거리 또는 공격 가능 거리 진입 시 전이
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
        /// 📌 도망 중 공격 거리 벗어나면 추적으로 전이
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
        /// 📌 공격이 끝난 후 상황에 따라 도주, 재공격, 추적 상태 전이
        /// </summary>
        private bool AttackingStateTransition(out State nextState)
        {
            var attackingState = states[State.Attacking].state;

            if ((attackingState as AimAndAttackState).IsFinished && !CharacterBehaviour.IsDead)
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
