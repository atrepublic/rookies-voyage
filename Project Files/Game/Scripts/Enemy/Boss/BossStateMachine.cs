// ==============================================
// 📌 BossStateMachine.cs
// ✅ 보스 전용 상태머신 컨트롤러
// ✅ 상태 정의 및 상태 간 전이 조건을 처리함
// ✅ BossBomberBehaviour에 연결되어 AI 전투 로직을 제어
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Boss
{
    [RequireComponent(typeof(BossBomberBehaviour))]
    public class BossStateMachine : AbstractStateMachine<State>
    {
        [Tooltip("현재 상태머신이 제어하는 보스")]
        private BossBomberBehaviour enemy;

        // 상태 인스턴스들
        private StateBehavior<BossBomberBehaviour> hidingState;
        private EnteringState enteringState;
        private IdleState idleState;
        private ChasingState chasingState;
        private KikkingState hittingState;
        private ShootingState shootingState;

        /// <summary>
        /// 📌 상태머신 초기화 및 상태 등록
        /// </summary>
        private void Awake()
        {
            enemy = GetComponent<BossBomberBehaviour>();

            // 상태 전이 조건 정의 - OnFinish 기반
            var distanceBasedTransitionOnFinish = new List<StateTransition<State>>()
            {
                new StateTransition<State>(TransitionToIdle, StateTransitionType.OnFinish),
                new StateTransition<State>(TransitionToKicking, StateTransitionType.OnFinish),
                new StateTransition<State>(TransitionToShooting, StateTransitionType.OnFinish),
                new StateTransition<State>(InstantTransitionToChasing, StateTransitionType.OnFinish)
            };

            // 상태 전이 조건 정의 - Independent (상시 검사)
            var distanceBasedTransitionIndependent = new List<StateTransition<State>>()
            {
                new StateTransition<State>(TransitionToIdle, StateTransitionType.Independent),
                new StateTransition<State>(TransitionToKicking, StateTransitionType.Independent),
                new StateTransition<State>(TransitionToShooting, StateTransitionType.Independent),
                new StateTransition<State>(InstantTransitionToChasing, StateTransitionType.Independent)
            };

            // 숨김 상태
            var hidingCase = new StateCase();
            hidingState = new StateBehavior<BossBomberBehaviour>(enemy);
            hidingCase.state = hidingState;
            hidingCase.transitions = new List<StateTransition<State>>()
            {
                new StateTransition<State>(HidingTransition, StateTransitionType.Independent)
            };

            // 등장 상태
            var enteringCase = new StateCase();
            enteringState = new EnteringState(enemy);
            enteringCase.state = enteringState;
            enteringCase.transitions = distanceBasedTransitionOnFinish;

            // 대기 상태
            var idleCase = new StateCase();
            idleState = new IdleState(enemy);
            idleCase.state = idleState;
            idleCase.transitions = distanceBasedTransitionIndependent;

            // 추적 상태
            var chasingCase = new StateCase();
            chasingState = new ChasingState(enemy);
            chasingCase.state = chasingState;
            chasingCase.transitions = distanceBasedTransitionIndependent;

            // 발차기 상태
            var hittingCase = new StateCase();
            hittingState = new KikkingState(enemy);
            hittingCase.state = hittingState;
            hittingCase.transitions = distanceBasedTransitionOnFinish;

            // 폭탄 사격 상태
            var shootingCase = new StateCase();
            shootingState = new ShootingState(enemy);
            shootingCase.state = shootingState;
            shootingCase.transitions = distanceBasedTransitionOnFinish;

            // 상태 등록
            states.Add(State.Hidden, hidingCase);
            states.Add(State.Entering, enteringCase);
            states.Add(State.Idle, idleCase);
            states.Add(State.Chasing, chasingCase);
            states.Add(State.Hitting, hittingCase);
            states.Add(State.Shooting, shootingCase);
        }

        /// <summary>
        /// 📌 보스가 일정 범위에 들어오면 숨김 상태 → 등장 상태로 전이
        /// </summary>
        private bool HidingTransition(out State nextState)
        {
            nextState = State.Entering;

            float dist = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);
            if (dist < 8f)
            {
                enemy.Enter();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 📌 너무 멀면 추적을 중지하고 대기 상태로 전이
        /// </summary>
        private bool TransitionToIdle(out State state)
        {
            state = State.Idle;

            float distance = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);
            return distance > enemy.VisionRange;
        }

        /// <summary>
        /// 📌 근접하면 발차기 상태로 전이
        /// </summary>
        private bool TransitionToKicking(out State state)
        {
            state = State.Hitting;

            float distance = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);
            return distance < enemy.KickDistance;
        }

        /// <summary>
        /// 📌 중거리면 폭탄 사격 상태로 전이
        /// </summary>
        private bool TransitionToShooting(out State state)
        {
            state = State.Shooting;

            float distance = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);
            return distance > enemy.AttackDistanceMin && distance <= enemy.AttackDistanceMax;
        }

        /// <summary>
        /// 📌 기본적으로 추적 상태를 유지
        /// </summary>
        private bool InstantTransitionToChasing(out State state)
        {
            state = State.Chasing;
            return true;
        }
    }
}
