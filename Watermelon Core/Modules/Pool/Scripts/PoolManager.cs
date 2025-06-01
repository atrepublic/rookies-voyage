// 스크립트 기능 요약:
// 이 스크립트는 프로젝트의 모든 오브젝트 풀(Pool)을 중앙에서 관리하는 정적 클래스입니다.
// 다양한 타입의 풀(IPool 인터페이스를 구현하는 클래스들)을 등록하고, 이름으로 풀을 찾거나,
// 특정 풀을 파괴하고, 모든 풀의 오브젝트를 일괄적으로 풀로 반환하는 등의 기능을 제공합니다.
// 풀링된 오브젝트들을 계층 구조에서 정리하기 위한 기본 컨테이너 Transform도 관리합니다.

#pragma warning disable 0414 // 사용되지 않는 private 변수에 대한 경고를 비활성화합니다. (예: poolsList는 직접 사용되지 않지만 유지를 위해 남겨둘 수 있음)

using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    /// <summary>
    /// 모든 풀 작업을 관리하는 클래스입니다.
    /// 정적 클래스로서 애플리케이션 전반에서 단 하나의 인스턴스만 존재하며, 모든 풀에 대한 접근 및 관리를 담당합니다.
    /// </summary>
    [StaticUnload] // 정적 클래스 언로드 시 특정 작업을 수행할 수 있도록 하는 속성 (프레임워크 내부 기능)
    public static class PoolManager
    {
        // OBJECT_FORMAT: 풀링된 오브젝트의 이름을 형식화하는 데 사용되는 상수 문자열입니다.
        // {0}은 풀 이름, {1}은 오브젝트 인덱스로 대체됩니다.
        [Tooltip("풀링된 오브젝트 이름 형식")]
        private const string OBJECT_FORMAT = "{0} e{1}";

        // poolsList: 현재 PoolManager에 등록된 모든 IPool 객체들의 리스트입니다.
        // 리스트 형태의 접근이 필요할 때 사용될 수 있습니다.
        [Tooltip("현재 등록된 모든 풀의 리스트")]
        private static List<IPool> poolsList = new List<IPool>();

        // poolsDictionary: 풀 이름을 해시 코드로 변환한 값을 키로 사용하여 IPool 객체에 빠르게 접근하기 위한 딕셔너리입니다.
        // 풀 이름으로 풀을 찾는 GetPoolByName 등의 함수에서 사용됩니다.
        [Tooltip("풀 이름을 통해 빠르게 풀에 접근하기 위한 딕셔너리")]
        private static Dictionary<int, IPool> poolsDictionary;

        // DefaultContainer: 풀링된 오브젝트들이 계층 구조에서 기본적으로 배치될 부모 Transform입니다.
        // Pool 객체 생성 시 별도의 컨테이너가 지정되지 않으면 이 컨테이너를 사용합니다.
        [Tooltip("풀링된 오브젝트가 기본적으로 배치될 부모 Transform")]
        public static Transform DefaultContainer { get; private set; }

        /// <summary>
        /// PoolManager 정적 생성자입니다.
        /// 클래스가 처음 로드될 때 PoolManager가 사용하는 컬렉션(리스트, 딕셔너리)을 초기화합니다.
        /// </summary>
        static PoolManager()
        {
            poolsList = new List<IPool>(); // 풀 리스트 초기화
            poolsDictionary = new Dictionary<int, IPool>(); // 풀 딕셔너리 초기화
        }

        /// <summary>
        /// PoolManager에 등록된 모든 풀에 대해 ReturnToPoolEverything 함수를 호출하여
        /// 현재 사용 중인 모든 풀링된 오브젝트를 일괄적으로 풀로 반환(비활성화)합니다.
        /// 모든 오브젝트의 부모를 기본 컨테이너로 재설정합니다.
        /// </summary>
        public static void ReturnToPool()
        {
            // poolsList가 비어있지 않으면 각 풀에 대해 순회합니다.
            if (!poolsList.IsNullOrEmpty())
            {
                for (int i = 0; i < poolsList.Count; i++)
                {
                    // 각 풀의 모든 오브젝트를 풀로 반환하고 부모를 재설정합니다.
                    poolsList[i].ReturnToPoolEverything(true);
                }
            }
        }

        /// <summary>
        /// 지정된 이름(ID)을 가진 풀(IPool 객체)에 대한 참조를 반환합니다.
        /// PoolManager에 해당 이름의 풀이 등록되어 있어야 합니다.
        /// </summary>
        /// <param name="poolName">가져올 풀의 이름</param>
        /// <returns>해당 이름의 IPool 객체 참조 또는 해당 이름의 풀을 찾을 수 없으면 null</returns>
        public static IPool GetPoolByName(string poolName)
        {
            // 풀 이름을 해시 코드로 변환하여 딕셔너리 키로 사용합니다.
            int poolHash = poolName.GetHashCode();

            // 딕셔너리에 해당 해시 코드를 가진 키(풀 이름)가 있는지 확인합니다.
            if (poolsDictionary.ContainsKey(poolHash))
            {
                return poolsDictionary[poolHash]; // 해당 풀 반환
            }

            // 해당 이름의 풀을 찾을 수 없으면 오류 메시지를 출력하고 null을 반환합니다.
            Debug.LogError("[Pool] 이름: '" + poolName + "'을 가진 풀을 찾을 수 없습니다.");

            return null;
        }

        /// <summary>
        /// 지정된 IPool 객체를 PoolManager에 등록합니다.
        /// 풀의 이름이 이미 등록된 다른 풀의 이름과 중복되지 않아야 합니다.
        /// </summary>
        /// <param name="pool">등록할 IPool 객체</param>
        public static void AddPool(IPool pool)
        {
            // 추가하려는 풀 객체가 null인지 확인합니다.
            if (pool == null)
            {
                Debug.LogError("[Pool]: null 풀 참조를 추가하려고 시도했습니다. 유효한 IPool 인스턴스가 제공되었는지 확인하십시오.");
                return;
            }

            // 추가하려는 풀 이름의 해시 코드를 계산합니다.
            int poolHash = pool.Name.GetHashCode();

            // 딕셔너리에 이미 동일한 해시 코드를 가진 키(풀 이름)가 있는지 확인합니다.
            if (poolsDictionary.ContainsKey(poolHash))
            {
                Debug.LogError("[Pool] 새로운 풀 추가에 실패했습니다. 이름 \"" + pool.Name + "\"이 이미 존재합니다.");
                return;
            }

            // 딕셔너리와 리스트에 새로운 풀을 추가합니다.
            poolsDictionary.Add(poolHash, pool);
            poolsList.Add(pool);
        }

        /// <summary>
        /// 지정된 이름(ID)을 가진 풀이 PoolManager에 등록되어 있는지 확인합니다.
        /// </summary>
        /// <param name="name">확인할 풀의 이름</param>
        /// <returns>해당 이름의 풀이 등록되어 있으면 true, 그렇지 않으면 false</returns>
        public static bool HasPool(string name)
        {
            // 풀 이름을 해시 코드로 변환하여 딕셔너리에 해당 키가 있는지 확인합니다.
            return poolsDictionary.ContainsKey(name.GetHashCode());
        }

        /// <summary>
        /// 지정된 IPool 객체를 PoolManager에서 등록 해제하고 풀에 의해 관리되는 모든 오브젝트를 파괴합니다.
        /// </summary>
        /// <param name="pool">파괴할 IPool 객체</param>
        public static void DestroyPool(IPool pool)
        {
            // 파괴하려는 풀 객체가 null인지 확인합니다.
            if (pool == null)
            {
                Debug.LogError("[Pool]: null 풀 참조를 파괴하려고 시도했습니다. 유효한 IPool 인스턴스가 제공되었는지 확인하십시오.");
                return;
            }

            // 풀에 의해 관리되는 모든 오브젝트를 파괴합니다.
            pool.Clear();

            // 딕셔너리와 리스트에서 풀을 제거합니다.
            poolsDictionary.Remove(pool.Name.GetHashCode());
            poolsList.Remove(pool);
        }

        /// <summary>
        /// 지정된 이름(ID)을 가진 풀이 PoolManager에 존재하는지 확인합니다.
        /// HasPool 함수와 동일한 기능을 수행합니다.
        /// </summary>
        /// <param name="name">확인할 풀의 이름</param>
        /// <returns>해당 이름의 풀이 존재하면 true, 그렇지 않으면 false</returns>
        public static bool PoolExists(string name)
        {
            // 풀 이름을 해시 코드로 변환하여 딕셔너리에 해당 키가 있는지 확인합니다.
            return poolsDictionary.ContainsKey(name.GetHashCode());
        }

        /// <summary>
        /// 풀링된 오브젝트들이 배치될 부모 Transform을 가져옵니다.
        /// 풀 객체에서 지정된 컨테이너가 없거나 null인 경우 PoolManager의 기본 컨테이너를 생성하여 반환합니다.
        /// 에디터 모드에서만 기본 컨테이너를 생성하고 DontDestroyOnLoad를 적용합니다.
        /// </summary>
        /// <param name="poolContainer">풀 객체에서 지정된 부모 Transform</param>
        /// <returns>오브젝트가 배치될 실제 부모 Transform</returns>
        public static Transform GetContainer(Transform poolContainer)
        {
#if UNITY_EDITOR // Unity 에디터에서만 실행되는 코드 블록
            // 풀 객체에서 지정된 컨테이너가 없으면 기본 컨테이너를 사용하거나 생성합니다.
            if (poolContainer == null)
            {
                // 기본 컨테이너가 아직 생성되지 않았으면 새로 생성합니다.
                if (DefaultContainer == null)
                {
                    // "[POOL OBJECTS]" 이름의 새로운 게임 오브젝트를 생성합니다.
                    GameObject containerObject = new GameObject("[POOL OBJECTS]");
                    // 생성된 오브젝트의 Transform을 기본 컨테이너로 설정합니다.
                    DefaultContainer = containerObject.transform;
                    // 위치, 회전, 스케일을 기본값으로 설정합니다.
                    DefaultContainer.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    DefaultContainer.localScale = Vector3.one;

                    // 씬이 변경되어도 파괴되지 않도록 설정합니다.
                    GameObject.DontDestroyOnLoad(DefaultContainer);
                }

                return DefaultContainer; // 기본 컨테이너 반환
            }
#endif // UNITY_EDITOR 끝

            // 풀 객체에서 유효한 컨테이너가 지정되었으면 해당 컨테이너를 반환합니다.
            return poolContainer;
        }

        /// <summary>
        /// 풀링된 오브젝트의 이름을 형식화합니다.
        /// 풀 이름과 오브젝트의 인덱스를 사용하여 "{풀이름} e{인덱스}" 형식의 문자열을 생성합니다.
        /// </summary>
        /// <param name="name">풀 이름</param>
        /// <param name="elementIndex">오브젝트의 인덱스</param>
        /// <returns>형식화된 오브젝트 이름 문자열</returns>
        public static string FormatName(string name, int elementIndex)
        {
            return string.Format(OBJECT_FORMAT, name, elementIndex); // 형식화된 이름 반환
        }

        /// <summary>
        /// PoolManager가 언로드될 때 호출되는 정적 언로드 함수입니다.
        /// 풀 리스트와 딕셔너리를 비워 메모리를 해제합니다.
        /// </summary>
        private static void UnloadStatic()
        {
            poolsList.Clear(); // 풀 리스트 비우기
            poolsDictionary.Clear(); // 풀 딕셔너리 비우기
        }

        public static void ClearAllPools()
        {
            if (poolsList != null) // null 체크 추가
            {
                // 주의: 리스트를 순회하면서 제거하면 문제가 발생할 수 있으므로,
                // 뒤에서부터 제거하거나, 복사본을 사용하거나, DestroyPool이 내부적으로 안전하게 처리하는지 확인 필요.
                // DestroyPool이 poolsList와 poolsDictionary에서 해당 풀을 제거한다고 가정.
                for (int i = poolsList.Count - 1; i >= 0; i--)
                {
                    if (poolsList[i] != null) // 풀 자체가 null이 아닌지 확인
                    {
                        DestroyPool(poolsList[i]);
                    }
                }
                // DestroyPool에서 이미 제거하므로 아래 Clear는 필요 없을 수 있지만, 만약을 위해 추가
                poolsList.Clear();
                poolsDictionary.Clear(); 
            }
            else // poolsList가 null인 비정상적인 경우 대비
            {
                poolsList = new List<IPool>();
                poolsDictionary = new Dictionary<int, IPool>();
            }
            Debug.Log("[PoolManager] 모든 풀이 정리되었습니다.");
        }
    }
}