// 이 스크립트는 내비메시 초기화 완료 시 콜백 함수를 실행하는 NavMesh 에이전트 구현체입니다.
// INavMeshAgent 인터페이스를 구현하며, 내비메시가 업데이트될 때 초기화 콜백을 호출합니다.
namespace Watermelon.LevelSystem
{
    // 내비메시 초기화 완료 시 지정된 콜백 함수를 호출하는 클래스입니다.
    // INavMeshAgent 인터페이스를 구현하여 내비메시 업데이트 알림을 받습니다.
    public class NavMeshCallback : INavMeshAgent
    {
        // 내비메시 초기화가 완료되었을 때 호출될 콜백 함수입니다.
        private SimpleCallback onNavMeshInitialised;

        // NavMeshCallback 클래스의 생성자입니다.
        // 내비메시 초기화 완료 시 호출될 콜백 함수를 설정합니다.
        // onNavMeshInitialised: 내비메시 초기화 완료 시 호출될 콜백 함수
        public NavMeshCallback(SimpleCallback onNavMeshInitialised)
        {
            this.onNavMeshInitialised = onNavMeshInitialised;
        }

        // INavMeshAgent 인터페이스의 구현 메소드입니다.
        // 내비메시가 업데이트(초기화 포함)되었을 때 호출됩니다.
        // 설정된 초기화 콜백 함수를 호출합니다.
        public void OnNavMeshUpdated()
        {
            // 초기화 콜백 함수가 null이 아니면 호출합니다.
            onNavMeshInitialised?.Invoke();
        }
    }
}