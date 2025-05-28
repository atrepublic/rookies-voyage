// 이 스크립트는 상태 머신에서 사용되는 상태(State) 및 상태 전이(State Transition)의 기본 구현과 관련 인터페이스/열거형을 정의합니다.
// StateBehavior는 특정 MonoBehaviour에 연결되어 해당 오브젝트의 상태 로직을 구현하며,
// StateTransition은 상태 전이 조건을 정의합니다.
using UnityEngine;
using System; // Enum 제약 조건에 필요

namespace Watermelon
{
    // 특정 MonoBehaviour 컴포넌트에 연결되어 해당 오브젝트의 상태 로직을 구현하는 기본 상태 동작 클래스입니다.
    // IStateBehavior 인터페이스를 구현합니다.
    // T: 이 상태 동작이 연결될 MonoBehaviour 타입. MonoBehaviour로 제약됩니다.
    public class StateBehavior<T> : IStateBehavior where T : MonoBehaviour
    {
        // 이 상태 동작이 연결된 대상 MonoBehaviour 컴포넌트입니다.
        public T Target { get; private set; }

        // 대상 MonoBehaviour의 현재 위치를 가져오는 프로퍼티입니다. 편의를 위해 제공됩니다.
        protected Vector3 Position => Target.transform.position;

        // 이 상태의 실행이 완료되었을 때 발생하는 이벤트입니다.
        public event SimpleCallback OnFinished; // SimpleCallback은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

        // OnFinished 이벤트를 발생시키는 보호된 메소드입니다.
        protected void InvokeOnFinished()
        {
            // 이벤트에 구독된 메소드가 있으면 호출합니다.
            OnFinished?.Invoke();
        }

        // StateBehavior 클래스의 생성자입니다.
        // 이 상태 동작이 연결될 대상 MonoBehaviour 컴포넌트를 설정합니다.
        // target: 이 상태 동작이 제어할 MonoBehaviour 컴포넌트
        public StateBehavior(T target)
        {
            Target = target;
        }

        // 상태가 시작될 때 호출되는 가상 메소드입니다.
        // 이 상태에 진입할 때 실행될 초기화 로직을 구현합니다.
        public virtual void OnStart()
        {

        }

        // 상태가 종료될 때 호출되는 가상 메소드입니다.
        // 이 상태에서 나갈 때 실행될 정리 로직을 구현합니다.
        public virtual void OnEnd()
        {

        }

        // 상태가 실행되는 동안 매 프레임마다 호출되는 가상 메소드입니다.
        // 이 상태의 주요 업데이트 로직을 구현합니다.
        public virtual void OnUpdate()
        {

        }

        // 상태 완료 이벤트(OnFinished)에 콜백 메소드를 구독하는 메소드입니다.
        // callback: 구독할 콜백 메소드
        public void SubscribeOnFinish(SimpleCallback callback) // SimpleCallback은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        {
            OnFinished += callback;
        }

        // 상태 완료 이벤트(OnFinished)에서 콜백 메소드 구독을 해제하는 메소드입니다.
        // callback: 구독 해제할 콜백 메소드
        public void UnsubscribeFromFinish(SimpleCallback callback) // SimpleCallback은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        {
            OnFinished -= callback;
        }
    }

    // 상태 머신에서 상태 전이 조건을 정의하는 클래스입니다.
    // 특정 조건을 평가하고 조건을 만족하면 다음 상태를 반환합니다.
    // T: 상태를 정의하는 데 사용될 Enum 타입. System.Enum으로 제약됩니다.
    public class StateTransition<T> where T : System.Enum
    {
        // 이 전이가 발생할 시점을 나타내는 전이 타입입니다. (독립적 또는 상태 완료 시)
        public StateTransitionType transitionType;
        // 전이 조건을 평가하고 다음 상태를 반환하는 델리게이트 타입입니다.
        public delegate bool EvaluateDelegate(out T nextState);
        // 전이 조건을 평가하는 실제 메소드에 대한 참조입니다.
        public EvaluateDelegate Evaluate { get; set; }

        // StateTransition 클래스의 생성자입니다.
        // 전이 조건 평가 메소드와 전이 타입을 설정합니다.
        // evaluate: 전이 조건을 평가하는 델리게이트 메소드
        // transitionType: 이 전이가 발생할 시점의 타입 (기본값: Independent)
        public StateTransition(EvaluateDelegate evaluate, StateTransitionType transitionType = StateTransitionType.Independent)
        {
            this.transitionType = transitionType;
            Evaluate = evaluate;
        }
    }

    // 상태 전이가 발생할 수 있는 시점을 정의하는 열거형입니다.
    public enum StateTransitionType
    {
        // 매 프레임마다 독립적으로 조건을 평가하는 전이입니다.
        Independent,
        // 현재 상태의 완료 이벤트 발생 시 조건을 평가하는 전이입니다.
        OnFinish,
    }

    // 상태 머신에서 사용되는 상태 동작이 구현해야 하는 인터페이스입니다.
    // 상태의 생명주기 메소드(업데이트, 시작, 종료)와 완료 이벤트 구독/해제 메소드를 정의합니다.
    public interface IStateBehavior
    {
        // 상태가 실행되는 동안 매 프레임마다 호출되는 메소드입니다.
        void OnUpdate();
        // 상태가 종료될 때 호출되는 메소드입니다.
        void OnEnd();
        // 상태가 시작될 때 호출되는 메소드입니다.
        void OnStart();

        // 상태 완료 이벤트에 콜백 메소드를 구독합니다.
        void SubscribeOnFinish(SimpleCallback callback); // SimpleCallback은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        // 상태 완료 이벤트에서 콜백 메소드 구독을 해제합니다.
        void UnsubscribeFromFinish(SimpleCallback callback); // SimpleCallback은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
    }
}