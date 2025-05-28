// 스크립트 기능 요약:
// 이 스크립트는 IPool 인터페이스를 구현하는 다중 오브젝트 풀 클래스입니다.
// 여러 종류의 프리팹을 포함하는 풀을 생성하고 관리하는 데 사용됩니다.
// 각 프리팹에는 가중치(Weight)를 부여하여 오브젝트를 가져올 때 가중치에 따라 다른 프리팹의 오브젝트가 선택될 수 있도록 합니다.
// 단일 풀과 마찬가지로 오브젝트 생성, 가져오기, 반환, 초기 크기 설정, 최대 크기 제한 기능을 제공합니다.

using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 확장을 사용하기 위해 추가

namespace Watermelon
{
    // PoolMultiple 클래스는 여러 종류의 프리팹을 관리하는 오브젝트 풀 클래스입니다.
    [System.Serializable]
    public sealed class PoolMultiple : IPool
    {
        // multiPoolPrefabsList: 이 다중 풀에 포함될 프리팹 목록과 각 프리팹의 가중치를 정의하는 리스트입니다.
        [SerializeField]
        [Tooltip("이 다중 풀에 포함될 프리팹 목록 및 각 프리팹의 가중치")]
        List<MultiPoolPrefab> multiPoolPrefabsList = new List<MultiPoolPrefab>();
        // capSize: 풀의 최대 크기를 제한할지 여부를 나타내는 플래그입니다. true이면 maxSize에 의해 제한됩니다.
        [SerializeField]
        [Tooltip("풀의 최대 크기 제한 활성화 여부")]
        bool capSize = false;
        // maxSize: capSize가 true일 때 이 다중 풀에 포함될 수 있는 모든 타입의 오브젝트를 합한 최대 개수입니다.
        [SerializeField]
        [Tooltip("풀의 최대 크기 (Cap Size 활성화 시 적용), 모든 타입의 오브젝트 합계")]
        int maxSize = 10;

        // objectsContainer: 이 풀에서 생성된 오브젝트들이 계층 구조에서 속하게 될 부모 Transform입니다.
        // 설정되지 않으면 PoolManager의 기본 컨테이너를 사용합니다.
        [SerializeField]
        [Tooltip("풀링된 오브젝트들을 담을 부모 Transform (설정하지 않으면 PoolManager 기본 컨테이너 사용)")]
        Transform objectsContainer = null;

        // name: 이 다중 풀을 식별하는 고유한 이름입니다. PoolManager에서 풀을 찾을 때 사용됩니다.
        [SerializeField]
        [Tooltip("이 풀의 고유 이름 (ID)")]
        string name;

        // Name 속성: 풀의 이름을 반환합니다.
        public string Name => name;

        // multiPooledObjects: 각 프리팹 타입별로 풀링된 오브젝트들을 저장하는 중첩 리스트입니다.
        // 외부 리스트는 multiPoolPrefabsList의 인덱스와 일치하며, 내부 리스트는 해당 타입의 오브젝트들을 관리합니다.
        [Tooltip("각 프리팹 타입별 풀링된 오브젝트를 저장하는 리스트")]
        private List<List<GameObject>> multiPooledObjects;
        // inited: 풀이 성공적으로 초기화되었는지 여부를 나타내는 플래그입니다.
        [Tooltip("풀 초기화 완료 여부")]
        private bool inited = false;

        /// <summary>
        /// 다중 풀 객체를 생성하는 생성자입니다.
        /// 사용할 프리팹 목록, 풀 이름, 오브젝트 컨테이너를 지정합니다.
        /// </summary>
        /// <param name="multiPoolPrefabs">이 다중 풀에 포함될 MultiPoolPrefab 리스트</param>
        /// <param name="name">이 풀의 고유 이름 (ID)</param>
        /// <param name="container">풀링된 오브젝트들이 배치될 부모 Transform (선택 사항, 기본값: null)</param>
        public PoolMultiple(List<MultiPoolPrefab> multiPoolPrefabs, string name, Transform container = null)
        {
            this.multiPoolPrefabsList = multiPoolPrefabs; // 프리팹 목록 설정
            this.name = name; // 풀 이름 설정
            this.objectsContainer = PoolManager.GetContainer(container); // 오브젝트 컨테이너 설정 (없으면 기본값 사용)

            Init(); // 풀 초기화
        }

        /// <summary>
        /// 풀을 초기화합니다.
        /// 이름 및 프리팹 유효성 검사, 내부 오브젝트 리스트 초기화, PoolManager에 풀 등록 등의 초기 설정 작업을 수행합니다.
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

            // 프리팹 목록에 null인 항목이 있는지 확인합니다.
            bool hasNullPrefab = false;
            for (int i = 0; i < multiPoolPrefabsList.Count; i++)
            {
                if (multiPoolPrefabsList[i].Prefab == null)
                {
                    Debug.LogError(string.Format("[Pool] 풀 초기화에 실패했습니다. 풀 '{0}'의 {1}번째 항목에 연결된 프리팹이 없습니다.", name, i));
                    hasNullPrefab = true;
                }
            }

            // null 프리팹이 발견되면 초기화를 중단합니다.
            if (hasNullPrefab) return;

            // 각 프리팹 타입별 오브젝트 리스트를 초기화합니다.
            multiPooledObjects = new List<List<GameObject>>();
            for (int i = 0; i < multiPoolPrefabsList.Count; i++)
            {
                multiPooledObjects.Add(new List<GameObject>());
            }

            // 이 풀을 PoolManager에 등록합니다.
            PoolManager.AddPool(this);

            // 초기화 완료 플래그를 true로 설정합니다.
            inited = true;
        }

        /// <summary>
        /// 이 다중 풀에서 사용 가능한 오브젝트 하나를 가져오는 함수입니다.
        /// GetPooledObject(-1)를 호출하여 가중치에 따라 랜덤으로 오브젝트를 선택합니다.
        /// </summary>
        /// <returns>풀링된 GameObject 또는 사용 가능한 오브젝트가 없고 새로 생성할 수 없으면 null</returns>
        public GameObject GetPooledObject()
        {
            // 가중치에 따라 랜덤으로 오브젝트를 가져옵니다.
            return GetPooledObject(-1);
        }

        /// <summary>
        /// 다중 타입 풀에서 풀링된 오브젝트를 가져오는 내부 구현 함수입니다.
        /// 특정 인덱스의 풀에서 가져오거나 가중치에 따라 랜덤으로 가져올 수 있습니다.
        /// </summary>
        /// <param name="poolIndex">가져올 특정 풀의 인덱스 (음수이면 가중치에 따라 랜덤 선택)</param>
        /// <returns>풀링된 GameObject 또는 사용 가능한 오브젝트가 없고 새로 생성할 수 없으면 null</returns>
        private GameObject GetPooledObject(int poolIndex)
        {
            // 풀이 초기화되지 않았으면 초기화합니다.
            if (!inited)
                Init();

            int chosenPoolIndex = 0; // 선택된 풀의 인덱스

            // poolIndex가 유효한 인덱스이면 해당 인덱스의 풀을 선택합니다.
            if (poolIndex != -1)
            {
                chosenPoolIndex = poolIndex;
            }
            else // poolIndex가 음수이면 가중치에 따라 랜덤으로 풀을 선택합니다.
            {
                // 모든 프리팹의 가중치 합계를 계산합니다.
                int totalWeight = multiPoolPrefabsList.Sum(x => x.Weight);
                // 1부터 totalWeight까지의 랜덤 값을 생성합니다.
                int randomValue = Random.Range(1, totalWeight + 1);
                int currentWeight = 0;

                // 프리팹 목록을 순회하며 랜덤 값이 누적 가중치보다 작거나 같아지는 시점의 프리팹을 선택합니다.
                for (int i = 0; i < multiPoolPrefabsList.Count; i++)
                {
                    currentWeight += multiPoolPrefabsList[i].Weight;

                    if (currentWeight >= randomValue)
                    {
                        chosenPoolIndex = i; // 해당 인덱스 선택
                        break; // 루프 종료
                    }
                }
            }

            // 선택된 풀(프리팹 타입)의 오브젝트 리스트를 가져옵니다.
            List<GameObject> objectsList = multiPooledObjects[chosenPoolIndex];
            // 해당 리스트를 순회하며 비활성화된 오브젝트를 찾습니다.
            for (int i = 0; i < objectsList.Count; i++)
            {
                GameObject pooledObject = objectsList[i];

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

            // 사용 가능한 오브젝트가 없고, 최대 크기 제한이 없거나 아직 해당 타입의 오브젝트 수가 maxSize에 도달하지 않았다면,
            // 새로운 오브젝트를 생성하여 풀에 추가하고 반환합니다.
            // (주의: PoolMultiple의 maxSize는 전체 오브젝트 수에 대한 제한일 수 있으므로 이 로직은 확인 필요)
            if (!capSize || objectsList.Count < maxSize) // 이 maxSize 조건은 단일 풀 기준이므로 다중 풀에서는 총 오브젝트 수와 비교해야 함.
                                                        // 현재 코드는 각 타입별 오브젝트 수와 비교하고 있음. (원본 코드 로직 유지)
            {
                return AddObjectToPool(chosenPoolIndex, true); // 선택된 인덱스의 풀에 오브젝트 추가
            }

            // 사용 가능한 오브젝트가 없고 최대 크기 제한에 도달했다면 null을 반환합니다.
            return null;
        }

        /// <summary>
        /// 다중 타입 풀의 특정 인덱스에 해당하는 풀에 새로운 오브젝트를 하나 추가하고 반환합니다.
        /// 오브젝트를 인스턴스화하고, 이름을 설정하고, 초기 활성화 상태를 설정한 후 해당 타입의 오브젝트 리스트에 추가합니다.
        /// </summary>
        /// <param name="poolIndex">오브젝트를 추가할 풀(프리팹 타입)의 인덱스</param>
        /// <param name="state">새로 생성된 오브젝트의 초기 활성화 상태</param>
        /// <returns>새로 풀에 추가된 GameObject의 참조</returns>
        private GameObject AddObjectToPool(int poolIndex, bool state)
        {
            // 풀이 초기화되지 않았으면 초기화합니다.
            if (!inited)
                Init();

            // 지정된 인덱스의 프리팹을 인스턴스화하고 objectsContainer의 자식으로 설정합니다.
            GameObject newObject = GameObject.Instantiate(multiPoolPrefabsList[poolIndex].Prefab, objectsContainer);
            // 오브젝트 이름을 풀 이름과 해당 타입 오브젝트 리스트의 개수를 포함하여 설정합니다.
            newObject.name = PoolManager.FormatName(name, multiPooledObjects[poolIndex].Count);
            // 오브젝트의 초기 활성화 상태를 설정합니다.
            newObject.SetActive(state);

            // 새로 생성된 오브젝트를 해당 타입의 오브젝트 리스트에 추가합니다.
            multiPooledObjects[poolIndex].Add(newObject);

            // 새로 생성된 오브젝트의 참조를 반환합니다.
            return newObject;
        }

        /// <summary>
        /// 이 다중 풀에 있는 모든 활성화된 오브젝트를 비활성화하여 풀로 반환합니다.
        /// 각 프리팹 타입의 모든 오브젝트 리스트를 순회하며 처리합니다.
        /// </summary>
        /// <param name="resetParent">true이면 각 오브젝트의 부모를 풀의 기본 컨테이너로 재설정합니다.</param>
        public void ReturnToPoolEverything(bool resetParent = false)
        {
            // 각 프리팹 타입의 오브젝트 리스트를 순회합니다.
            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                // 해당 타입의 오브젝트 리스트를 순회합니다.
                for (int j = 0; j < multiPooledObjects[i].Count; j++)
                {
                    // resetParent가 true이면 오브젝트의 부모를 objectsContainer 또는 PoolManager.DefaultContainer로 재설정합니다.
                    if (resetParent)
                    {
                        multiPooledObjects[i][j].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.DefaultContainer);
                    }

                    // 오브젝트를 비활성화하여 풀로 반환합니다.
                    multiPooledObjects[i][j].SetActive(false);
                }
            }
        }

        /// <summary>
        /// 이 다중 풀에 의해 관리되는 모든 오브젝트를 파괴합니다.
        /// 이 메서드는 GameObject.Destroy를 호출하므로 성능 부하가 클 수 있습니다. 풀 사용이 완전히 종료될 때만 사용해야 합니다.
        /// </summary>
        public void Clear()
        {
            // 각 프리팹 타입의 오브젝트 리스트를 순회합니다.
            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                // 해당 타입의 오브젝트 리스트를 순회하며 각 오브젝트를 파괴합니다.
                for (int j = 0; j < multiPooledObjects[i].Count; j++)
                {
                    // 오브젝트가 null이 아닌 경우에만 파괴를 시도합니다.
                    if (multiPooledObjects[i][j] != null)
                        UnityEngine.Object.Destroy(multiPooledObjects[i][j]);
                }

                // 해당 타입의 오브젝트 리스트를 비웁니다.
                multiPooledObjects[i].Clear();
            }
        }

        /// <summary>
        /// 다중 타입 풀에서 지정된 인덱스에 해당하는 MultiPoolPrefab 정보를 반환합니다.
        /// </summary>
        /// <param name="index">가져올 MultiPoolPrefab의 인덱스</param>
        /// <returns>지정된 인덱스의 MultiPoolPrefab</returns>
        public MultiPoolPrefab GetPrefabByIndex(int index)
        {
            return multiPoolPrefabsList[index]; // 해당 인덱스의 MultiPoolPrefab 반환
        }

        /// <summary>
        /// 각 프리팹 타입별로 풀에 지정된 개수만큼의 오브젝트를 미리 생성하여 채워넣습니다.
        /// 이미 존재하는 오브젝트 수를 고려하여 필요한 만큼만 새로 생성합니다.
        /// </summary>
        /// <param name="count">각 타입별로 최소한 확보하고자 하는 오브젝트의 총 개수</param>
        public void CreatePoolObjects(int count)
        {
            // 풀이 초기화되지 않았으면 초기화합니다.
            if (!inited)
                Init();

            // 각 프리팹 타입의 오브젝트 리스트를 순회합니다.
            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                // 목표 개수와 현재 해당 타입 오브젝트 개수의 차이를 계산합니다.
                int sizeDifference = count - multiPooledObjects[i].Count;
                // 차이가 양수이면 필요한 만큼 새로운 오브젝트를 생성하여 추가합니다.
                if (sizeDifference > 0)
                {
                    for (int j = 0; j < sizeDifference; j++)
                    {
                        AddObjectToPool(i, false); // 해당 타입의 풀에 비활성 상태로 오브젝트 추가
                    }
                }
            }
        }

        // MultiPoolPrefab 구조체는 다중 풀에서 사용될 각 프리팹과 관련 설정을 정의합니다.
        [System.Serializable]
        public struct MultiPoolPrefab
        {
            // Prefab: 이 구조체와 연결된 게임 오브젝트 프리팹입니다.
            [Tooltip("풀링될 게임 오브젝트 프리팹")]
            public GameObject Prefab;
            // Weight: 이 프리팹이 GetPooledObject() 호출 시 선택될 확률에 영향을 주는 가중치입니다.
            [Tooltip("이 프리팹이 선택될 확률에 영향을 주는 가중치")]
            public int Weight;

            // isWeightLocked: 에디터 상에서 이 가중치 값이 고정되어 편집되지 않도록 할지 여부를 나타내는 플래그입니다.
            // (Editor 전용 기능일 수 있음)
            [Tooltip("에디터에서 가중치 값 고정 여부")]
            public bool isWeightLocked;

            /// <summary>
            /// MultiPoolPrefab 구조체의 생성자입니다.
            /// </summary>
            /// <param name="prefab">연결할 게임 오브젝트 프리팹</param>
            /// <param name="weight">설정할 가중치</param>
            /// <param name="isWeightLocked">가중치 고정 여부</param>
            public MultiPoolPrefab(GameObject prefab, int weight, bool isWeightLocked)
            {
                this.Prefab = prefab;
                this.Weight = weight;
                this.isWeightLocked = isWeightLocked;
            }
        }
    }
}