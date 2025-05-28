// ==============================================
// 📌 DemoStateMachine.cs
// ✅ 데모 자폭형 적 유닛의 상태 전이 로직을 관리하는 스크립트
// ✅ 순찰, 추적, 공격 상태 전환을 정의함
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Demo
{
    [RequireComponent(typeof(DemoEnemyBehavior))]
    public class DemoStateMachine : AbstractStateMachine<State>
    {
        [Tooltip("현재 상태머신이 제어할 데모 적 유닛")]
        private DemoEnemyBehavior enemy;

        /// <summary>
        /// 📌 상태머신 초기화 및 상태 정의
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<DemoEnemyBehavior>();

            var patrollingStateCase = new StateCase
            {
                state = new PatrollingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(PatrollingStateTransition)
                }
            };

            var followingStateCase = new StateCase
            {
                state = new FollowingState(enemy),
                transitions = new List<StateTransition<State>>
                {
                    new StateTransition<State>(FollowingStateTransition)
                }
            };

            var attackingStateCase = new StateCase
            {
                state = new AttackingState(enemy),
                transitions = new List<StateTransition<State>>() // 공격 후엔 바로 종료됨
            };

            states.Add(State.Patrolling, patrollingStateCase);
            states.Add(State.Following, followingStateCase);
            states.Add(State.Attacking, attackingStateCase);
        }

        /// <summary>
        /// 📌 순찰 중 적 발견 시 추적 또는 공격 상태로 전환
        /// </summary>
        private bool PatrollingStateTransition(out State nextState)
        {
            bool spotted = enemy.IsTargetInVisionRange || enemy.HasTakenDamage;

            if (!spotted)
            {
                nextState = State.Patrolling;
                return false;
            }

            enemy.LightUpFuse();

            nextState = enemy.IsTargetInAttackRange ? State.Attacking : State.Following;
            return true;
        }

        /// <summary>
        /// 📌 추적 중 공격 범위에 진입하면 공격 상태로 전환
        /// </summary>
        private bool FollowingStateTransition(out State nextState)
        {
            bool shouldAttack = enemy.IsTargetInAttackRange && enemy.IsTargetInSight() && !CharacterBehaviour.IsDead;
            nextState = shouldAttack ? State.Attacking : State.Following;
            return shouldAttack;
        }
    }
}
