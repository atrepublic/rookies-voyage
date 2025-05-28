// PoolGeneric.cs v1.00
// 이 스크립트는 제네릭 풀링 시스템을 제공합니다.
// 지정된 컴포넌트 타입(T)을 캐시하여 매번 GetComponent 호출 없이 재사용할 수 있습니다.
// PoolManager에 자동으로 등록되어 관리되며, 다양한 생성자 오버로드를 통해 편리하게 풀을 생성/초기화할 수 있습니다.

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 제네릭 풀 클래스입니다.
    /// T 타입 컴포넌트를 풀링하여 재사용하며, 최대 크기 제한, 컨테이너 설정 기능을 제공합니다.
    /// </summary>
    /// <typeparam name="T">풀링할 컴포넌트 타입</typeparam>
    public class PoolGeneric<T> : IPool where T : Component
    {
        [Tooltip("풀링할 오브젝트의 프리팹")]        
        private GameObject prefab = null;

        [Tooltip("풀링된 오브젝트들을 포함할 컨테이너 Transform (null이면 기본 컨테이너 사용)")]
        private Transform objectsContainer = null;

        [Tooltip("풀의 고유 이름(식별자)")]
        private string name;

        [Tooltip("풀 크기 제한 여부 (true면 MaxSize까지만 생성)")]
        private bool capSize = false;

        [Tooltip("생성 가능한 최대 오브젝트 수 (capSize가 true일 때 적용)")]
        private int maxSize = 10;

        /// <summary>
        /// 풀 이름을 반환합니다.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 원본 프리팹을 반환합니다.
        /// </summary>
        public GameObject Prefab => prefab;

        /// <summary>
        /// 최대 생성 크기를 반환합니다.
        /// </summary>
        public int MaxSize => maxSize;

        /// <summary>
        /// 크기 제한 여부를 반환합니다.
        /// </summary>
        public bool CapSize => capSize;

        /// <summary>
        /// 실제 오브젝트들이 위치할 컨테이너 Transform을 반환합니다.
        /// </summary>
        public Transform ObjectsContainer => PoolManager.GetContainer(objectsContainer);

        // 초기화 플래그
        private bool inited = false;

        [Tooltip("풀에 있는 모든 컴포넌트 인스턴스 리스트")]        
        public List<T> pooledObjects;

        /// <summary>
        /// 단일 파라미터 생성자: 프리팹만 지정하여 풀을 생성합니다. 풀 이름은 프리팹 이름으로 설정됩니다.
        /// </summary>
        public PoolGeneric(GameObject prefab)
        {
            this.prefab = prefab;
            this.name = prefab.name;
            Init();
        }

        /// <summary>
        /// 두 파라미터 생성자: 프리팹과 풀 이름을 지정하여 풀을 생성합니다.
        /// </summary>
        public PoolGeneric(GameObject prefab, string name)
        {
            this.prefab = prefab;
            this.name = name;
            Init();
        }

        /// <summary>
        /// 두 파라미터 생성자: 프리팹과 컨테이너를 지정하여 풀을 생성합니다.
        /// </summary>
        public PoolGeneric(GameObject prefab, Transform objectsContainer)
        {
            this.prefab = prefab;
            this.objectsContainer = objectsContainer;
            this.name = prefab.name;
            Init();
        }

        /// <summary>
        /// 세 파라미터 생성자: 프리팹, 풀 이름, 컨테이너를 지정하여 풀을 생성합니다.
        /// </summary>
        public PoolGeneric(GameObject prefab, string name, Transform objectsContainer)
        {
            this.prefab = prefab;
            this.name = name;
            this.objectsContainer = objectsContainer;
            Init();
        }

        /// <summary>
        /// 풀을 초기화하고 PoolManager에 등록합니다.
        /// </summary>
        public void Init()
        {
            if (inited) return;

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[Pool]: 풀 초기화 실패. 유효한 고유 이름을 제공해야 합니다.");
                return;
            }

            if (PoolManager.HasPool(name))
            {
                Debug.LogError($"[Pool]: 풀 초기화 실패. '{name}' 이름의 풀이 이미 존재합니다.");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError($"[Pool]: 풀 초기화 실패. '{name}' 풀에 프리팹이 할당되지 않았습니다.");
                return;
            }

            if (prefab.GetComponent<T>() == null)
            {
                Debug.LogError($"[Pool]: 풀 초기화 실패. '{name}' 풀의 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
                return;
            }

            pooledObjects = new List<T>();
            PoolManager.AddPool(this);
            inited = true;
        }

        /// <summary>
        /// 활성화되지 않은 오브젝트를 풀에서 가져오거나, 필요 시 새로 생성하여 반환합니다.
        /// </summary>
        /// <returns>풀 오브젝트 GameObject 또는 null</returns>
        public GameObject GetPooledObject()
        {
            if (!inited) Init();

            for (int i = 0; i < pooledObjects.Count; i++)
            {
                var comp = pooledObjects[i];
                if (comp == null || comp.gameObject == null)
                {
                    Debug.LogError($"[Pool]: '{name}' 풀의 객체가 외부에서 파괴되었습니다.");
                    continue;
                }

                if (!comp.gameObject.activeSelf)
                {
                    comp.gameObject.SetActive(true);
                    return comp.gameObject;
                }
            }

            if (!capSize || pooledObjects.Count < maxSize)
                return AddObjectToPool(true).gameObject;

            return null;
        }

        /// <summary>
        /// 활성화되지 않은 컴포넌트를 풀에서 가져오거나, 필요 시 새로 생성하여 반환합니다.
        /// </summary>
        /// <returns>풀 오브젝트의 컴포넌트 T 또는 null</returns>
        public T GetPooledComponent()
        {
            if (!inited) Init();

            for (int i = 0; i < pooledObjects.Count; i++)
            {
                var comp = pooledObjects[i];
                if (comp == null)
                {
                    Debug.LogError($"[Pool]: '{name}' 풀의 객체가 외부에서 파괴되었습니다.");
                    continue;
                }

                if (!comp.gameObject.activeSelf)
                {
                    comp.gameObject.SetActive(true);
                    return comp;
                }
            }

            if (!capSize || pooledObjects.Count < maxSize)
                return AddObjectToPool(true);

            return null;
        }

        /// <summary>
        /// 새로운 풀 오브젝트를 생성하여 리스트에 추가합니다.
        /// </summary>
        private T AddObjectToPool(bool active)
        {
            if (!inited) Init();

            var obj = Object.Instantiate(prefab, ObjectsContainer);
            obj.name = PoolManager.FormatName(name, pooledObjects.Count);
            obj.SetActive(active);

            var comp = obj.GetComponent<T>();
            pooledObjects.Add(comp);
            return comp;
        }

        /// <summary>
        /// 지정된 개수만큼 오브젝트를 미리 생성합니다.
        /// </summary>
        public void CreatePoolObjects(int count)
        {
            if (!inited) Init();

            int diff = count - pooledObjects.Count;
            for (int i = 0; i < diff; i++)
                AddObjectToPool(false);
        }

        /// <summary>
        /// 모든 오브젝트를 비활성화 상태로 반환합니다.
        /// </summary>
        public void ReturnToPoolEverything(bool resetParent = false)
        {
            if (!inited) return;

            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (resetParent)
                    pooledObjects[i].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.DefaultContainer);

                pooledObjects[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 풀에 있는 모든 오브젝트를 파괴하고 리스트를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            if (!inited) return;

            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (pooledObjects[i] != null)
                    Object.Destroy(pooledObjects[i].gameObject);
            }

            pooledObjects.Clear();
        }
    }
}
