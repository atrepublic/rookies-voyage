// 스크립트 기능 요약:
// 이 스크립트는 IPool 인터페이스를 구현하는 기본적인 오브젝트 풀 클래스입니다.
// 특정 프리팹을 기반으로 오브젝트 풀을 생성하고 관리하는 기능을 제공합니다.
// 오브젝트 생성, 가져오기, 반환, 초기 크기 설정, 최대 크기 제한 등의 기능을 포함하며,
// 풀링된 오브젝트들을 담을 컨테이너(Transform)를 관리합니다.

using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    /// <summary>
    /// 기본 풀 클래스입니다. 풀 설정 및 풀링된 오브젝트에 대한 참조를 포함합니다.
    /// 이 클래스는 특정 게임 오브젝트 프리팹을 풀링하는 데 사용됩니다.
    /// </summary>
    [System.Serializable]
    public sealed class Pool : IPool
    {
        // prefab: 이 풀에서 생성될 게임 오브젝트의 원본 프리팹입니다.
        [SerializeField]
        [Tooltip("이 풀에서 생성될 게임 오브젝트 프리팹")]
        GameObject prefab = null;
        // objectsContainer: 이 풀에서 생성된 오브젝트들이 계층 구조에서 속하게 될 부모 Transform입니다.
        // 설정되지 않으면 PoolManager의 기본 컨테이너를 사용합니다.
        [SerializeField]
        [Tooltip("풀링된 오브젝트들을 담을 부모 Transform (설정하지 않으면 PoolManager 기본 컨테이너 사용)")]
        Transform objectsContainer = null;
        // name: 이 풀을 식별하는 고유한 이름입니다. PoolManager에서 풀을 찾을 때 사용됩니다.
        [SerializeField]
        [Tooltip("이 풀의 고유 이름 (ID)")]
        string name;

        // capSize: 풀의 최대 크기를 제한할지 여부를 나타내는 플래그입니다. true이면 maxSize에 의해 제한됩니다.
        [SerializeField]
        [Tooltip("풀의 최대 크기 제한 활성화 여부")]
        bool capSize = false;
        // maxSize: capSize가 true일 때 풀에 존재할 수 있는 최대 오브젝트 개수입니다.
        [SerializeField]
        [Tooltip("풀의 최대 크기 (Cap Size 활성화 시 적용)")]
        int maxSize = 10;

        // Name 속성: 풀의 이름을 반환합니다.
        public string Name => name;
        // Prefab 속성: 이 풀의 원본 프리팹을 반환합니다.
        public GameObject Prefab => prefab;
        // MaxSize 속성: 풀의 최대 크기를 반환합니다.
        public int MaxSize => maxSize;
        // CapSize 속성: 풀의 최대 크기 제한 활성화 여부를 반환합니다.
        public bool CapSize => capSize;

        // ObjectsContainer 속성: 풀링된 오브젝트가 배치될 실제 부모 Transform을 반환합니다.
        // objectsContainer가 설정되어 있으면 그것을 사용하고, 아니면 PoolManager의 기본 컨테이너를 사용합니다.
        public Transform ObjectsContainer => PoolManager.GetContainer(objectsContainer);

        // pooledObjects: 현재 풀에 의해 관리되는 모든 게임 오브젝트(활성/비활성 포함) 리스트입니다.
        [Tooltip("풀에 의해 관리되는 오브젝트 리스트")]
        private List<GameObject> pooledObjects;
        // inited: 풀이 성공적으로 초기화되었는지 여부를 나타내는 플래그입니다.
        [Tooltip("풀 초기화 완료 여부")]
        private bool inited = false;

        /// <summary>
        /// 프리팹만 지정하여 Pool 객체를 생성하는 생성자입니다. 풀 이름은 프리팹 이름으로 자동 설정됩니다.
        /// </summary>
        /// <param name="prefab">이 풀에서 사용할 게임 오브젝트 프리팹</param>
        public Pool(GameObject prefab)
        {
            this.prefab = prefab;
            this.name = prefab.name; // 프리팹 이름으로 풀 이름 설정
            Init(); // 풀 초기화
        }

        /// <summary>
        /// 프리팹과 오브젝트 컨테이너를 지정하여 Pool 객체를 생성하는 생성자입니다. 풀 이름은 프리팹 이름으로 자동 설정됩니다.
        /// </summary>
        /// <param name="prefab">이 풀에서 사용할 게임 오브젝트 프리팹</param>
        /// <param name="objectsContainer">풀링된 오브젝트들이 배치될 부모 Transform</param>
        public Pool(GameObject prefab, Transform objectsContainer)
        {
            this.prefab = prefab;
            this.objectsContainer = objectsContainer;
            this.name = prefab.name; // 프리팹 이름으로 풀 이름 설정
            Init(); // 풀 초기화
        }

        /// <summary>
        /// 프리팹과 풀 이름을 지정하여 Pool 객체를 생성하는 생성자입니다. 오브젝트 컨테이너는 설정되지 않습니다.
        /// </summary>
        /// <param name="prefab">이 풀에서 사용할 게임 오브젝트 프리팹</param>
        /// <param name="name">이 풀의 고유 이름 (ID)</param>
        public Pool(GameObject prefab, string name)
        {
            this.prefab = prefab;
            this.name = name; // 풀 이름 설정
            Init(); // 풀 초기화
        }

        /// <summary>
        /// 프리팹, 풀 이름, 오브젝트 컨테이너를 모두 지정하여 Pool 객체를 생성하는 생성자입니다.
        /// </summary>
        /// <param name="prefab">이 풀에서 사용할 게임 오브젝트 프리팹</param>
        /// <param name="name">이 풀의 고유 이름 (ID)</param>
        /// <param name="objectsContainer">풀링된 오브젝트들이 배치될 부모 Transform</param>
        public Pool(GameObject prefab, string name, Transform objectsContainer)
        {
            this.name = name; // 풀 이름 설정
            this.prefab = prefab;
            this.objectsContainer = objectsContainer; // 오브젝트 컨테이너 설정
            Init(); // 풀 초기화
        }

        /// <summary>
        /// 풀을 초기화합니다.
        /// 이름 유효성 검사, PoolManager에 풀 등록 등의 초기 설정 작업을 수행합니다.
        /// 이미 초기화된 경우 아무 작업도 수행하지 않습니다.
        /// </summary>
        public void Init()
        {
            if (inited) return; // 이미 초기화된 경우 반환

            // 풀 이름이 유효한지 확인합니다.
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[Pool]: 풀 초기화에 실패했습니다. 고유한 이름(ID)이 제공되어야 합니다. 진행하기 전에 'name' 필드가 비어있지 않은지 확인하십시오.");
                return;
            }

            // PoolManager에 이미 동일한 이름의 풀이 있는지 확인합니다.
            if (PoolManager.HasPool(name))
            {
                Debug.LogError(string.Format("[Pool]: 풀 초기화에 실패했습니다. 이름 '{0}'을 가진 풀이 PoolManager에 이미 존재합니다. 충돌을 피하려면 각 풀에 고유한 이름을 사용하십시오.", name));
                return;
            }

            // 프리팹이 제대로 설정되었는지 확인합니다.
            if (prefab == null)
            {
                Debug.LogError(string.Format("[Pool] 풀 초기화에 실패했습니다. 풀 '{0}'에 연결된 프리팹이 없습니다.", name));
                return;
            }

            // 풀링된 오브젝트를 저장할 리스트를 초기화합니다.
            pooledObjects = new List<GameObject>();

            // 이 풀을 PoolManager에 등록합니다.
            PoolManager.AddPool(this);

            // 초기화 완료 플래그를 true로 설정합니다.
            inited = true;
        }

        /// <summary>
        /// 현재 사용 가능한 풀링된 오브젝트에 대한 참조를 반환합니다.
        /// 사용 가능한 오브젝트가 없지만 최대 크기 제한에 도달하지 않은 경우, 새로운 오브젝트를 생성하여 반환합니다.
        /// </summary>
        /// <returns>풀링된 GameObject 또는 사용 가능한 오브젝트가 없고 새로 생성할 수 없으면 null</returns>
        public GameObject GetPooledObject()
        {
            // 풀이 초기화되지 않았으면 초기화합니다.
            if (!inited)
                Init();

            // 풀링된 오브젝트 리스트를 순회하며 비활성화된 오브젝트를 찾습니다.
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                GameObject pooledObject = pooledObjects[i];

                // 풀링된 오브젝트가 외부에서 파괴되었는지 확인합니다.
                if (pooledObject == null)
                {
                    Debug.LogError(string.Format("[Pool]: 풀링된 오브젝트 ({0})가 외부에서 파괴되었습니다. 이는 오브젝트가 풀로 제대로 반환되지 않았거나 부모 오브젝트가 파괴되었음을 나타낼 수 있습니다. 의도치 않은 오브젝트 파괴를 방지하기 위해 오브젝트 관리 로직을 검토하십시오.", name));
                    continue; // 다음 오브젝트로 건너뜁니다.
                }

                // 오브젝트가 비활성화 상태(사용 가능)이면 활성화하고 반환합니다.
                if (!pooledObject.activeSelf)
                {
                    pooledObject.SetActive(true);
                    return pooledObject;
                }
            }

            // 사용 가능한 오브젝트가 없고, 최대 크기 제한이 없거나 아직 최대 크기에 도달하지 않았다면,
            // 새로운 오브젝트를 생성하여 풀에 추가하고 반환합니다.
            if (!capSize || pooledObjects.Count < maxSize)
            {
                return AddObjectToPool(true);
            }

            // 사용 가능한 오브젝트가 없고 최대 크기 제한에 도달했다면 null을 반환합니다.
            return null;
        }

        /// <summary>
        /// 풀에서 사용 가능한 오브젝트를 가져와 지정된 타입의 컴포넌트를 반환합니다.
        /// GetPooledObject()를 호출하여 오브젝트를 가져온 후 GetComponent<T>()를 수행합니다.
        /// </summary>
        /// <typeparam name="T">가져올 컴포넌트의 타입</typeparam>
        /// <returns>풀링된 오브젝트의 지정된 타입 컴포넌트 또는 사용 가능한 오브젝트가 없으면 null</returns>
        public T GetPooledComponent<T>() where T : Component
        {
            // 풀에서 게임 오브젝트를 가져옵니다.
            GameObject pooledObject = GetPooledObject();
            // 오브젝트를 성공적으로 가져왔으면 해당 오브젝트에서 컴포넌트를 찾아 반환합니다.
            if (pooledObject != null)
            {
                return pooledObject.GetComponent<T>();
            }

            // 오브젝트를 가져오지 못했으면 null을 반환합니다.
            return null;
        }

        /// <summary>
        /// 풀에 새로운 오브젝트를 하나 추가하고 반환합니다.
        /// 오브젝트를 인스턴스화하고, 이름을 설정하고, 초기 활성화 상태를 설정한 후 pooledObjects 리스트에 추가합니다.
        /// </summary>
        /// <param name="state">새로 생성된 오브젝트의 초기 활성화 상태</param>
        /// <returns>새로 풀에 추가된 GameObject의 참조</returns>
        private GameObject AddObjectToPool(bool state)
        {
            // 풀이 초기화되지 않았으면 초기화합니다.
            if (!inited)
                Init();

            // 프리팹을 인스턴스화하고 지정된 컨테이너의 자식으로 설정합니다.
            GameObject newObject = GameObject.Instantiate(prefab, ObjectsContainer);
            // 오브젝트 이름을 풀 이름과 현재 오브젝트 개수를 포함하여 설정합니다.
            newObject.name = PoolManager.FormatName(name, pooledObjects.Count);
            // 오브젝트의 초기 활성화 상태를 설정합니다.
            newObject.SetActive(state);

            // 새로 생성된 오브젝트를 pooledObjects 리스트에 추가합니다.
            pooledObjects.Add(newObject);

            // 새로 생성된 오브젝트의 참조를 반환합니다.
            return newObject;
        }

        /// <summary>
        /// 풀에 지정된 개수만큼의 오브젝트를 미리 생성하여 채워넣습니다.
        /// 이미 존재하는 오브젝트 수를 고려하여 필요한 만큼만 새로 생성합니다.
        /// </summary>
        /// <param name="count">최소한 확보하고자 하는 오브젝트의 총 개수</param>
        public void CreatePoolObjects(int count)
        {
            // 풀이 초기화되지 않았으면 초기화합니다.
            if (!inited)
                Init();

            // 목표 개수와 현재 오브젝트 개수의 차이를 계산합니다.
            int sizeDifference = count - pooledObjects.Count;
            // 차이가 양수이면 필요한 만큼 새로운 오브젝트를 생성하여 추가합니다.
            if (sizeDifference > 0)
            {
                for (int i = 0; i < sizeDifference; i++)
                {
                    AddObjectToPool(false); // 비활성 상태로 추가
                }
            }
        }

        /// <summary>
        /// 현재 풀에서 활성화 상태인 모든 오브젝트를 비활성화하여 풀로 반환합니다.
        /// </summary>
        /// <param name="resetParent">true이면 각 오브젝트의 부모를 풀의 기본 컨테이너로 재설정합니다.</param>
        public void ReturnToPoolEverything(bool resetParent = false)
        {
            // 풀이 초기화되지 않았으면 아무 작업도 수행하지 않고 반환합니다.
            if (!inited) return;

            // 풀링된 오브젝트 리스트를 순회합니다.
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                // resetParent가 true이면 오브젝트의 부모를 PoolManager의 기본 컨테이너로 재설정합니다.
                if (resetParent)
                {
                    // objectsContainer가 설정되어 있으면 그것을 사용하고, 아니면 PoolManager.DefaultContainer를 사용합니다.
                    pooledObjects[i].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.DefaultContainer);
                }

                // 오브젝트를 비활성화하여 풀로 반환합니다.
                pooledObjects[i].SetActive(false);
            }
        }

        /// <summary>
        /// 풀에 의해 관리되는 모든 오브젝트를 파괴합니다.
        /// 이 메서드는 GameObject.Destroy를 호출하므로 성능 부하가 클 수 있습니다. 풀 사용이 완전히 종료될 때만 사용해야 합니다.
        /// </summary>
        public void Clear()
        {
            // 풀이 초기화되지 않았으면 아무 작업도 수행하지 않고 반환합니다.
            if (!inited) return;

            // 풀링된 오브젝트 리스트를 순회하며 각 오브젝트를 파괴합니다.
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                // 오브젝트가 null이 아닌 경우에만 파괴를 시도합니다.
                if (pooledObjects[i] != null)
                    UnityEngine.Object.Destroy(pooledObjects[i]);
            }

            // pooledObjects 리스트를 비웁니다.
            pooledObjects.Clear();
        }
    }
}