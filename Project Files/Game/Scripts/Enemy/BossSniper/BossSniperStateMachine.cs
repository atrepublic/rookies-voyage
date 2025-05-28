// ==============================================
// 📌 BossSniperStateMachine.cs
// ✅ 보스 스나이퍼 전용 상태머신
// ✅ 위치 이동 → 조준 → 사격 사이클을 상태 기반으로 제어
// ✅ 상태별 전이 조건과 상태 등록을 관리함
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.BossSniper
{
    [RequireComponent(typeof(BossSniperBehavior))]
    public class BossSniperStateMachine : AbstractStateMachine<BossSniperStates>
    {
        [Tooltip("현재 상태머신이 제어하는 스나이퍼 보스")]
        private BossSniperBehavior enemy;

        private BossSniperChangingPositionState changePosState;
        private BossSniperAimState aimState;
        private BossSniperAttackState attackState;

        /// <summary>
        /// 📌 상태머신 초기화 및 각 상태 등록
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<BossSniperBehavior>();

            // 이동 상태 등록
            var changePosCase = new StateCase();
            changePosState = new BossSniperChangingPositionState(enemy);
            changePosCase.state = changePosState;
            changePosCase.transitions = new List<StateTransition<BossSniperStates>>()
            {
                new StateTransition<BossSniperStates>(ChangePosTransition, StateTransitionType.OnFinish)
            };

            // 조준 상태 등록
            var aimCase = new StateCase();
            aimState = new BossSniperAimState(enemy);
            aimCase.state = aimState;
            aimCase.transitions = new List<StateTransition<BossSniperStates>>()
            {
                new StateTransition<BossSniperStates>(AimTransition, StateTransitionType.OnFinish)
            };

            // 공격 상태 등록
            var shootCase = new StateCase();
            attackState = new BossSniperAttackState(enemy);
            shootCase.state = attackState;
            shootCase.transitions = new List<StateTransition<BossSniperStates>>()
            {
                new StateTransition<BossSniperStates>(ShootTransition, StateTransitionType.OnFinish)
            };

            // 상태 등록
            states.Add(BossSniperStates.ChangingPosition, changePosCase);
            states.Add(BossSniperStates.Aiming, aimCase);
            states.Add(BossSniperStates.Shooting, shootCase);
        }

        /// <summary>
        /// 📌 이동 후 → 조준 상태로 전이
        /// </summary>
        private bool ChangePosTransition(out BossSniperStates nextState)
        {
            nextState = BossSniperStates.Aiming;
            return true;
        }

        /// <summary>
        /// 📌 조준 후 → 공격 상태로 전이
        /// </summary>
        private bool AimTransition(out BossSniperStates nextState)
        {
            nextState = BossSniperStates.Shooting;
            return true;
        }

        // 내부 공격 횟수 체크용 변수
        private int shootCount = 0;

        /// <summary>
        /// 📌 공격 후 → 다시 조준 또는 위치 이동 상태로 전이 (첫 사격 후 이동)
        /// </summary>
        private bool ShootTransition(out BossSniperStates nextState)
        {
            shootCount++;

            if (shootCount == 1)
            {
                shootCount = 0;
                nextState = BossSniperStates.ChangingPosition;
            }
            else
            {
                nextState = BossSniperStates.Aiming;
            }

            return true;
        }
    }
}
