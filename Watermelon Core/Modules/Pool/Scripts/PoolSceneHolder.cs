// 스크립트 기능 요약:
// 이 스크립트는 씬(Scene)에 배치되어 해당 씬에서 사용될 오브젝트 풀(Pool)들을 관리하는 MonoBehaviour 클래스입니다.
// Inspector에서 Pool 배열을 설정하고, 씬 로드 시(Awake) 해당 풀들을 초기화하며,
// 씬 언로드 시(OnDestroy) 해당 풀들을 PoolManager에서 제거하고 관련 오브젝트를 파괴하는 역할을 수행합니다.
// DefaultExecutionOrder 속성을 통해 다른 스크립트보다 먼저 실행되도록 설정될 수 있습니다.

using UnityEngine;

namespace Watermelon
{
    // DefaultExecutionOrder 속성을 통해 이 스크립트의 실행 순서를 다른 스크립트보다 빠르게 (-5) 설정합니다.
    [DefaultExecutionOrder(-5)]
    // PoolSceneHolder 클래스는 특정 씬의 오브젝트 풀 관리를 담당하는 MonoBehaviour 컴포넌트입니다.
    public class PoolSceneHolder : MonoBehaviour
    {
        // pools: 이 씬 홀더에 의해 관리될 Pool 객체들의 배열입니다.
        // Inspector에서 직접 Pool 객체들을 할당하여 설정합니다.
        [SerializeField]
        [Tooltip("이 씬에서 관리될 Pool 객체 목록")]
        Pool[] pools;

        /// <summary>
        /// 오브젝트가 로드될 때(씬 로드 또는 인스턴스화 시) Awake 함수가 호출됩니다.
        /// 이 씬 홀더에 할당된 모든 Pool 객체를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            // 할당된 모든 Pool 객체를 순회하며 Init() 함수를 호출하여 초기화합니다.
            foreach (Pool pool in pools)
            {
                pool.Init();
            }
        }

        /// <summary>
        /// 오브젝트가 파괴될 때(씬 언로드, GameObject 파괴 등) OnDestroy 함수가 호출됩니다.
        /// 이 씬 홀더에 의해 관리되던 모든 Pool 객체를 PoolManager에서 제거하고 풀의 오브젝트를 파괴합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 할당된 모든 Pool 객체를 순회하며 PoolManager.DestroyPool() 함수를 호출하여 풀을 파괴합니다.
            foreach (Pool pool in pools)
            {
                // PoolManager를 통해 풀을 파괴하도록 요청합니다.
                PoolManager.DestroyPool(pool);
            }
        }
    }
}