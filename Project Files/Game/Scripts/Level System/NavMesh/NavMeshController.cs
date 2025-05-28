// 이 스크립트는 씬 내 내비메시를 관리하는 정적 컨트롤러 클래스입니다.
// 내비메시 표면 설정, 생성, 업데이트, 그리고 내비메시 업데이트 알림을 받는 에이전트 관리를 담당합니다.
// Unity.AI.Navigation 패키지를 사용하며, 비동기 내비메시 빌드를 지원합니다.
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.LevelSystem
{
    // 씬 내 내비메시 생성 및 관리를 위한 정적 클래스입니다.
    // NavMeshSurface 컴포넌트와 상호작용하며, 내비메시 업데이트 이벤트를 외부에 알립니다.
    public static class NavMeshController
    {
        // 내비메시 업데이트 알림을 받을 INavMeshAgent 목록입니다.
        private static List<INavMeshAgent> navMeshAgents;

        // 씬에 존재하는 NavMeshSurface 컴포넌트입니다.
        // 이 컴포넌트를 통해 내비메시 생성 및 업데이트가 이루어집니다.
        private static NavMeshSurface navMeshSurface;
        // NavMeshSurface 컴포넌트 인스턴스를 가져옵니다.
        public static NavMeshSurface NavMeshSurface => navMeshSurface;

        // 내비메시 계산이 완료되었는지 여부를 나타냅니다.
        private static bool isNavMeshCalculated;
        // 내비메시 계산 완료 상태를 가져옵니다.
        public static bool IsNavMeshCalculated => isNavMeshCalculated;

        // 내비메시가 다시 계산되었을 때 발생하는 이벤트입니다.
        public static event SimpleCallback OnNavMeshRecalculated;

        // 비동기 내비메시 재계산을 관리하는 트윈 케이스입니다.
        private static TweenCase navMeshTweenCase;
        // 내비메시가 현재 재계산 중인지 여부를 나타냅니다.
        private static bool navMeshRecalculating;

        // NavMeshController를 초기화하는 메소드입니다.
        // 내비메시 표면 컴포넌트를 추가하고 기본 설정을 적용합니다.
        // parentObject: NavMeshSurface 컴포넌트가 추가될 게임 오브젝트
        // navMeshData: 사용할 내비메시 데이터 (기존 데이터를 로드하거나 새로 생성 가능)
        public static void Init(GameObject parentObject, NavMeshData navMeshData)
        {
            // 부모 오브젝트에 NavMeshSurface 컴포넌트를 추가합니다.
            navMeshSurface = parentObject.AddComponent<NavMeshSurface>();
            // 초기에는 비활성화 상태로 시작합니다.
            navMeshSurface.enabled = false;

            // 내비메시 에이전트 유형 (예: Humanoid) 설정합니다.
            navMeshSurface.agentTypeID = 0; // Humanoid
            // 기본 이동 가능 영역 (예: Walkable) 설정합니다.
            navMeshSurface.defaultArea = 0; // Walkable
            // 내비메시 빌드 시 포함할 오브젝트 범위를 설정합니다. (자식 오브젝트 포함)
            navMeshSurface.collectObjects = CollectObjects.Children;
            // 내비메시 빌드에 사용할 레이어 마스크를 설정합니다. (예: Ground 레이어)
            // PhysicsHelper.LAYER_GROUND는 사용자의 정의에 따라 달라질 수 있습니다.
            navMeshSurface.layerMask = 1 << PhysicsHelper.LAYER_GROUND;
            // 내비메시 빌드에 사용할 지오메트리 소스를 설정합니다. (물리 콜라이더 사용)
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            // 복셀 크기를 오버라이드할지 설정합니다.
            navMeshSurface.overrideVoxelSize = true;
            // 내비메시 빌드에 사용될 복셀의 크기를 설정합니다. (정밀도에 영향)
            navMeshSurface.voxelSize = 0.2f;

            // 내비메시에서 작은 영역을 제거하는 최소 크기를 설정합니다.
            navMeshSurface.minRegionArea = 2;
            // 높이 메시를 빌드할지 여부를 설정합니다.
            navMeshSurface.buildHeightMesh = false;

            // 사용할 NavMeshData를 할당합니다.
            navMeshSurface.navMeshData = navMeshData;
            // 설정 완료 후 NavMeshSurface를 활성화합니다.
            navMeshSurface.enabled = true;

            // 내비메시 에이전트 목록을 초기화합니다.
            navMeshAgents = new List<INavMeshAgent>();
        }

        // 내비메시를 비동기적으로 다시 계산하도록 요청하는 메소드입니다.
        // simpleCallback: 내비메시 재계산 완료 후 호출될 콜백 함수
        public static void RecalculateNavMesh(SimpleCallback simpleCallback)
        {
            // 이미 재계산 중이면 무시합니다.
            if (navMeshRecalculating)
                return;

            // 재계산 중 상태로 설정합니다.
            navMeshRecalculating = true;

            // 내비메시 표면 재계산을 위한 트윈 케이스를 생성하고 시작합니다.
            // 재계산 완료 시 OnRecalculationFinished를 호출하고, 그 후 전달받은 콜백을 호출합니다.
            navMeshTweenCase = new NavMeshSurfaceTweenCase(navMeshSurface).OnComplete(delegate
            {
                OnRecalculationFinished();

                simpleCallback?.Invoke();
            }).StartTween();
        }

        // 내비메시 재계산이 완료되었을 때 호출되는 내부 메소드입니다.
        // 내비메시 계산 완료 상태를 설정하고, 등록된 에이전트들에게 업데이트를 알립니다.
        private static void OnRecalculationFinished()
        {
            // 내비메시 계산 완료 상태로 설정합니다.
            isNavMeshCalculated = true;

            // 등록된 모든 내비메시 에이전트를 순회하며 업데이트 알림 메소드를 호출합니다.
            for (int i = 0; i < navMeshAgents.Count; i++)
            {
                navMeshAgents[i].OnNavMeshUpdated();
            }

            // 재계산 중 상태를 해제합니다.
            navMeshRecalculating = false;

            // 트윈 케이스 참조를 해제합니다.
            navMeshTweenCase = null;

            // 내비메시 재계산 완료 이벤트를 발생시킵니다.
            OnNavMeshRecalculated?.Invoke();
        }

        // 내비메시 에이전트를 등록하거나, 내비메시가 이미 계산되었다면 즉시 업데이트를 호출하는 메소드입니다.
        // navMeshAgent: 등록하거나 업데이트를 호출할 INavMeshAgent
        public static void InvokeOrSubscribe(INavMeshAgent navMeshAgent)
        {
            // 내비메시가 이미 계산되었다면 즉시 업데이트를 호출합니다.
            if (isNavMeshCalculated)
            {
                navMeshAgent.OnNavMeshUpdated();
            }
            // 내비메시가 계산되지 않았다면 에이전트 목록에 추가하여 나중에 업데이트를 받도록 합니다.
            else
            {
                navMeshAgents.Add(navMeshAgent);
            }
        }

        // 내비메시 계산이 완료되지 않았거나 재계산 중일 때, 강제로 완료 상태로 만들고 에이전트들을 활성화하는 메소드입니다.
        public static void ForceActivation()
        {
            // 내비메시가 이미 계산되었다면 아무것도 하지 않습니다.
            if (isNavMeshCalculated)
                return;

            // 재계산 중인 트윈 케이스가 있다면 중지합니다.
            if (navMeshTweenCase != null)
            {
                navMeshTweenCase.Kill(); // Kill() 메소드는 외부 트위닝 시스템에 따라 다를 수 있습니다.
                navMeshTweenCase = null;
            }

            // 재계산 완료 처리를 강제로 수행합니다.
            OnRecalculationFinished();
        }

        // 등록된 모든 내비메시 에이전트 목록을 비웁니다.
        public static void ClearAgents()
        {
            navMeshAgents.Clear();
        }

        // NavMeshController의 상태를 초기 상태로 리셋합니다.
        // 재계산 상태, 계산 완료 상태를 초기화하고 에이전트 목록을 비웁니다.
        public static void Reset()
        {
            // 재계산 중인 트윈 케이스가 있다면 중지하고 참조를 해제합니다.
            if (navMeshTweenCase != null)
            {
                navMeshTweenCase.Kill(); // Kill() 메소드는 외부 트위닝 시스템에 따라 다를 수 있습니다.
                navMeshTweenCase = null;
            }

            // 재계산 중 상태와 계산 완료 상태를 초기화합니다.
            navMeshRecalculating = false;
            isNavMeshCalculated = false;

            // 에이전트 목록을 비웁니다.
            navMeshAgents.Clear();
        }
    }
}