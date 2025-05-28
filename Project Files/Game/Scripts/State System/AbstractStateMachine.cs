// 이 스크립트는 제네릭을 사용하여 Enum 타입으로 상태를 정의하는 유한 상태 머신(Finite State Machine, FSM)의 추상 구현체입니다.
// MonoBehaviour를 상속받아 Unity 게임 오브젝트에 부착되어 사용될 수 있으며,
// 상태(State)와 상태 전이(Transition)를 관리하고 Update를 통해 상태 로직을 실행합니다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    // Enum 타입 T를 상태로 사용하는 추상 상태 머신 클래스입니다.
    // MonoBehaviour를 상속받아 Unity 씬에서 컴포넌트로 사용 가능하며, IStateMachine 인터페이스를 구현합니다.
    // T: 상태를 정의하는 데 사용될 Enum 타입. System.Enum으로 제약됩니다.
    public class AbstractStateMachine<T> : MonoBehaviour, IStateMachine where T : System.Enum
    {
        // 상태 머신의 초기 상태를 나타내는 변수입니다.
        // Unity 인스펙터에서 설정할 수 있도록 SerializeField로 표시됩니다.
        [Tooltip("상태 머신의 초기 상태를 설정합니다.")]
        [SerializeField] protected T startState;

        // Enum 상태 값에 해당하는 StateCase 객체들을 저장하는 딕셔너리입니다.
        // 각 StateCase는 해당 상태의 동작(IStateBehavior)과 상태 전이 규칙 목록을 포함합니다.
        protected Dictionary<T, StateCase> states = new Dictionary<T, StateCase>();

        // 상태 머신의 현재 상태를 추적하는 프로퍼티입니다.
        public T CurrentState { get; protected set; }

        // 상태 머신이 현재 실행 중인지 여부를 나타내는 플래그입니다.
        // 이 플래그를 통해 상태 머신의 시작과 중지를 제어합니다.
        public bool IsPlaying { get; protected set; }

        // 상태 머신을 시작하는 메소드입니다.
        // IsPlaying 플래그를 true로 설정하고, CurrentState를 startState로 설정한 후 StartState 메소드를 호출하여 초기 상태를 시작합니다.
        public void StartMachine()
        {
            // 상태 머신 실행 상태로 설정합니다.
            IsPlaying = true;

            // 현재 상태를 초기 상태로 설정합니다.
            CurrentState = startState;
            // 현재 상태의 시작 로직을 실행합니다.
            StartState();
        }

        // 현재 상태의 시작 로직을 실행하는 메소드입니다.
        // 해당 상태의 IStateBehavior 객체를 가져와 완료 이벤트에 구독하고 OnStart 메소드를 호출합니다.
        private void StartState()
        {
            // 현재 상태에 해당하는 StateCase에서 IStateBehavior 객체를 가져옵니다.
            var state = states[CurrentState].state;

            // 상태의 완료 이벤트(OnFinished)에 OnStateFinished 메소드를 구독하여 상태 완료 시 알림을 받습니다.
            state.SubscribeOnFinish(OnStateFinished);
            // 현재 상태의 시작 로직을 실행합니다.
            state.OnStart();
        }

        // 현재 상태의 종료 로직을 실행하는 메소드입니다.
        // 해당 상태의 IStateBehavior 객체를 가져와 완료 이벤트 구독을 해제하고 OnEnd 메소드를 호출합니다.
        private void EndState()
        {
            // 현재 상태에 해당하는 StateCase에서 IStateBehavior 객체를 가져옵니다.
            var state = states[CurrentState].state;

            // 상태의 완료 이벤트(OnFinished) 구독을 해제합니다.
            state.UnsubscribeFromFinish(OnStateFinished);
            // 현재 상태의 종료 로직을 실행합니다.
            state.OnEnd();
        }

        // 상태 머신을 중지하는 메소드입니다.
        // IsPlaying 플래그를 false로 설정하고 현재 상태의 OnEnd 메소드를 호출하여 상태를 종료합니다.
        public void StopMachine()
        {
            // 상태 머신 실행 상태를 중지합니다.
            IsPlaying = false;

            // 현재 상태의 종료 로직을 실행합니다.
            EndState();
        }

        // 현재 상태의 완료 이벤트가 트리거되었을 때 호출되는 메소드입니다.
        // 현재 상태의 전이 목록을 확인하여 OnFinish 타입의 전이 조건을 만족하는 경우 다음 상태로 전이합니다.
        private void OnStateFinished()
        {
            // 현재 상태에 해당하는 StateCase를 가져옵니다.
            var stateCase = states[CurrentState];

            // StateCase에서 IStateBehavior 객체와 전이 목록을 가져옵니다.
            var state = stateCase.state;
            var transitions = stateCase.transitions;

            // 전이 목록을 순회합니다.
            for (int i = 0; i < transitions.Count; i++)
            {
                var transition = transitions[i];
                // 전이 타입이 OnFinish이고 전이 조건을 만족하는 경우
                if (transition.transitionType == StateTransitionType.OnFinish && transition.Evaluate(out var nextState))
                {
                    // 현재 상태의 종료 로직을 실행합니다.
                    EndState();

                    // 다음 상태로 현재 상태를 업데이트합니다.
                    CurrentState = nextState;

                    // 다음 상태의 시작 로직을 실행합니다.
                    StartState();
                    // 전이가 발생했으므로 더 이상 순회하지 않고 종료합니다.
                    break;
                }
            }
        }

        // Unity 생명주기 메소드: 매 프레임마다 호출됩니다.
        // 상태 머신이 실행 중인 경우 현재 상태의 OnUpdate 메소드를 호출하고 Independent 타입의 전이 조건을 확인합니다.
        private void Update()
        {
            // 상태 머신이 실행 중일 때만 작동합니다.
            if (IsPlaying)
            {
                // 현재 상태에 해당하는 StateCase를 가져옵니다.
                var stateCase = states[CurrentState];

                // StateCase에서 IStateBehavior 객체와 전이 목록을 가져옵니다.
                var state = stateCase.state;
                var transitions = stateCase.transitions;

                // 현재 상태의 업데이트 로직을 실행합니다.
                state.OnUpdate();

                // 전이 목록을 순회합니다.
                for (int i = 0; i < transitions.Count; i++)
                {
                    var transition = transitions[i];
                    // 전이 타입이 Independent(매 프레임마다 확인)이고 전이 조건을 만족하는 경우
                    if (transition.transitionType == StateTransitionType.Independent && transition.Evaluate(out var nextState))
                    {
                        // 현재 상태의 종료 로직을 실행합니다.
                        EndState();

                        // 다음 상태로 현재 상태를 업데이트합니다.
                        CurrentState = nextState;

                        // 다음 상태의 시작 로직을 실행합니다.
                        StartState();
                        // 전이가 발생했으므로 더 이상 순회하지 않고 종료합니다.
                        break;
                    }
                }
            }
        }

        // 현재 상태를 Enum 타입으로 반환하는 메소드입니다.
        public Enum GetCurentState() => CurrentState;

        // 상태(IStateBehavior)와 해당 상태에서 나가는 전이(StateTransition) 목록을 담는 내부 클래스입니다.
        public class StateCase
        {
            // 이 StateCase가 나타내는 상태의 동작 인터페이스입니다.
            public IStateBehavior state;
            // 이 상태에서 나가는 가능한 전이 목록입니다.
            public List<StateTransition<T>> transitions;
        }
    }

    // 상태 머신이 구현해야 하는 기본 인터페이스입니다.
    // 상태 머신의 시작, 중지, 현재 상태 가져오기 메소드를 정의합니다.
    public interface IStateMachine
    {
        // 상태 머신을 시작합니다.
        void StartMachine();
        // 상태 머신을 중지합니다.
        void StopMachine();
        // 상태 머신의 현재 상태를 가져옵니다.
        Enum GetCurentState();
    }
}