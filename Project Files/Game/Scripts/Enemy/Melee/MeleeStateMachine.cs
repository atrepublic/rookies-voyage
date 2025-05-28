// ==============================================
// 📌 MeleeStateMachine.cs
// ✅ 근접 공격 적 유닛의 상태머신 구성 및 전이 조건 처리
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Melee
{
    [RequireComponent(typeof(MeleeEnemyBehaviour))]
    public class MeleeStateMachine : AbstractStateMachine<State>
    {
        private MeleeEnemyBehaviour enemy;

        /// <summary>
        /// 📌 상태 초기화 및 상태 전이 등록
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<MeleeEnemyBehaviour>();

            var patrollingStateCase = new StateCase
            {
                state = new PatrollingState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>(PatrollingStateTransition)
                }
            };

            var attackingStateCase = new StateCase
            {
                state = new MeleeFollowAttackState(enemy),
                transitions = new List<StateTransition<State>> {
                    new StateTransition<State>((out State nextState) => {
                        nextState = State.Attacking;
                        return false;
                    })
                }
            };

            states.Add(State.Patrolling, patrollingStateCase);
            states.Add(State.Attacking, attackingStateCase);
        }

        /// <summary>
        /// 📌 순찰 중 타겟 발견 시 공격 상태로 전이
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            bool foundTarget = enemy.IsTargetInVisionRange || enemy.HasTakenDamage;

            nextState = foundTarget ? State.Attacking : State.Patrolling;
            return foundTarget;
        }
    }
}
