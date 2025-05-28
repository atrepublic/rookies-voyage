// 이 스크립트는 내비메시 표면의 비동기 업데이트를 트윈 애니메이션처럼 처리하기 위한 커스텀 TweenCase입니다.
// Unity의 비동기 작업(AsyncOperation)을 사용하여 내비메시 빌드가 완료될 때까지 대기하고 완료를 알립니다.
using Unity.AI.Navigation;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // 내비메시 표면 업데이트를 비동기적으로 처리하는 트윈 케이스입니다.
    // TweenCase 클래스를 상속받으며, 내비메시 빌드 작업의 완료를 감지합니다.
    public class NavMeshSurfaceTweenCase : TweenCase
    {
        // 내비메시 비동기 업데이트 작업에 대한 참조입니다.
        private AsyncOperation asyncOperation;

        // NavMeshSurfaceTweenCase 클래스의 생성자입니다.
        // 내비메시 표면의 비동기 업데이트 작업을 시작합니다.
        // navMeshSurface: 업데이트할 NavMeshSurface 컴포넌트
        public NavMeshSurfaceTweenCase(NavMeshSurface navMeshSurface)
        {
            // 트윈의 지속 시간을 무한대로 설정하여 비동기 작업 완료 시까지 대기하도록 합니다.
            duration = float.MaxValue;

            // NavMeshSurface의 내비메시 비동기 업데이트를 시작하고 AsyncOperation 객체를 저장합니다.
            // navMeshSurface.navMeshData는 현재 NavMeshSurface에 연결된 NavMeshData입니다.
            asyncOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        // 트윈이 기본적으로 완료되었을 때 호출되는 메소드입니다.
        // 현재 구현은 비어 있습니다.
        public override void DefaultComplete()
        {
            // 기본 완료 로직 (필요하다면 추가)
        }

        // 매 프레임마다 호출되어 트윈의 진행 상태를 업데이트하는 메소드입니다.
        // deltaTime: 이전 프레임 이후 경과된 시간
        public override void Invoke(float deltaTime)
        {
            // 비동기 작업이 완료되었는지 확인합니다.
            if (asyncOperation.isDone)
                // 작업이 완료되었다면 트윈도 완료 상태로 만듭니다.
                Complete();
        }

        // 트윈이 유효한 상태인지 확인하는 메소드입니다.
        // 현재는 항상 유효한 것으로 간주합니다.
        public override bool Validate()
        {
            return true;
        }
    }
}