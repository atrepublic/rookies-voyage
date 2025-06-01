/*
 * FloatingTextController.cs
 * 이 스크립트는 게임 내에서 사용되는 플로팅 텍스트(예: 데미지 수치)의 생성 및 관리를 담당합니다.
 * 다양한 종류의 플로팅 텍스트를 미리 정의된 케이스에 따라 풀링하여 사용하며,
 * 중앙 집중식 빈도 조절 기능을 통해 동일 대상에 대한 텍스트 과다 출력을 방지합니다.
 */
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter; // BaseEnemyBehavior 등 SquadShooter 네임스페이스의 타입을 사용하기 위함

namespace Watermelon
{
    /// <summary>
    /// 게임 내 플로팅 텍스트의 생성, 풀링, 표시를 중앙에서 관리하는 컨트롤러입니다.
    /// </summary>
    public class FloatingTextController : MonoBehaviour
    {
        // 싱글톤 인스턴스
        private static FloatingTextController instance;

        [SerializeField]
        [Tooltip("사전 정의된 플로팅 텍스트 종류(케이스) 배열입니다. 인스펙터에서 설정합니다.")]
        private FloatingTextCase[] floatingTextCases;

        // 플로팅 텍스트 케이스 이름의 해시값을 키로 사용하는 딕셔너리 (빠른 접근용)
        private Dictionary<int, FloatingTextCase> floatingTextLink;

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 빈도 조절용 정적 멤버 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        /// <summary>
        /// 특정 게임 오브젝트 인스턴스 ID별로 마지막 플로팅 텍스트 생성 시간을 기록하는 딕셔너리입니다.
        /// </summary>
        private static Dictionary<int, float> lastTextSpawnTimePerInstanceId = new Dictionary<int, float>();

        /// <summary>
        /// 동일 대상에 대한 플로팅 텍스트 생성 최소 간격 시간(초)입니다.
        /// </summary>
        private const float MIN_TEXT_INTERVAL = 0.2f;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 빈도 조절용 정적 멤버 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        /// <summary>
        /// MonoBehaviour의 Awake 메시지입니다. 싱글톤 인스턴스를 설정합니다.
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                // DontDestroyOnLoad(gameObject); // 필요에 따라 씬 전환 시 유지 설정
            }
            else if (instance != this)
            {
                Debug.LogWarning("[FloatingTextController] 중복된 인스턴스가 감지되어 현재 오브젝트를 파괴합니다.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// FloatingTextController를 초기화합니다. GameController 등에서 호출됩니다.
        /// 제공된 FloatingTextCase들을 기반으로 내부 링크(딕셔너리) 및 각 케이스의 풀을 설정합니다.
        /// </summary>
        /// <param name="initialFloatingTextCases">사용할 플로팅 텍스트 케이스 배열</param>
        public void Init(FloatingTextCase[] initialFloatingTextCases)
        {
            if(instance == null) instance = this; // 방어 코드: instance가 설정되지 않았을 경우

            Debug.Log($"[FloatingTextController] Init 시작. 설정된 floatingTextCases 개수: {(initialFloatingTextCases != null ? initialFloatingTextCases.Length : 0)}");
            this.floatingTextCases = initialFloatingTextCases;

            floatingTextLink = new Dictionary<int, FloatingTextCase>();
            if (this.floatingTextCases != null)
            {
                for (int i = 0; i < this.floatingTextCases.Length; i++)
                {
                    FloatingTextCase currentCase = this.floatingTextCases[i];
                    if (currentCase == null)
                    {
                        Debug.LogWarning($"[FloatingTextController] Init: floatingTextCases 배열의 {i}번째 요소가 null입니다.");
                        continue;
                    }

                    if (currentCase.Prefab != null)
                    {
                        currentCase.Initialise(); // 각 케이스의 풀 초기화
                        int hash = currentCase.Name.GetHashCode(); // 이름으로 해시 생성
                        if (!floatingTextLink.ContainsKey(hash))
                        {
                            floatingTextLink.Add(hash, currentCase);
                            Debug.Log($"[FloatingTextController] Init: '{currentCase.Name}' (Hash: {hash}) 케이스가 floatingTextLink에 추가됨.");
                        }
                        else
                        {
                            Debug.LogWarning($"[FloatingTextController] Init: 중복된 FloatingTextCase 이름 또는 해시값 발견 - {currentCase.Name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[FloatingTextController] Init: floatingTextCases 배열의 케이스 '{currentCase.Name}'(인덱스:{i})에 Prefab이 할당되지 않았습니다.");
                    }
                }
            }
            Debug.Log($"[FloatingTextController] Init 완료. 최종 floatingTextLink 개수: {(floatingTextLink != null ? floatingTextLink.Count : 0)}");
        }

        /// <summary>
        /// 내부용: 해시값을 사용하여 등록된 FloatingTextCase를 반환합니다.
        /// </summary>
        private FloatingTextCase GetFloatingTextCase(int nameHash)
        {
            if (floatingTextLink == null) // floatingTextLink가 초기화되지 않은 경우
            {
                Debug.LogError("[FloatingTextController] GetFloatingTextCase: floatingTextLink가 초기화되지 않았습니다. Init()이 먼저 호출되어야 합니다.");
                return null;
            }
            if (floatingTextLink.TryGetValue(nameHash, out FloatingTextCase foundCase))
            {
                return foundCase;
            }

            Debug.LogError($"[FloatingTextController] 이름 해시 '{nameHash}'에 해당하는 FloatingTextCase를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 내부용: 이름을 사용하여 등록된 FloatingTextCase를 반환합니다.
        /// </summary>
        private FloatingTextCase GetFloatingTextCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[FloatingTextController] GetFloatingTextCase: 이름이 null이거나 비어있습니다.");
                return null;
            }
            return GetFloatingTextCase(name.GetHashCode());
        }

        /// <summary>
        /// 내부용 핵심 로직: 지정된 타입의 플로팅 텍스트를 풀에서 가져와 설정하고 활성화합니다.
        /// 빈도 조절 기능이 이 메서드에서 처리됩니다.
        /// </summary>
        /// <returns>생성된 FloatingTextBaseBehavior 인스턴스, 또는 빈도 조절/오류로 생성 안 된 경우 null</returns>
        private FloatingTextBaseBehavior SpawnTextInternal(int floatingTextNameHash, GameObject subjectOfText = null)
        {
            if (instance == null) { Debug.LogError("[FloatingTextController] SpawnTextInternal: 인스턴스가 null입니다."); return null; }

            // 빈도 조절 로직
            if (subjectOfText != null)
            {
                int subjectInstanceId = subjectOfText.GetInstanceID();
                if (lastTextSpawnTimePerInstanceId.TryGetValue(subjectInstanceId, out float lastTime))
                {
                    if (Time.timeSinceLevelLoad < lastTime + MIN_TEXT_INTERVAL)
                    {
                        return null; // 쿨다운 중, 생성하지 않음
                    }
                }
                lastTextSpawnTimePerInstanceId[subjectInstanceId] = Time.timeSinceLevelLoad;
            }

            FloatingTextCase selectedCase = instance.GetFloatingTextCase(floatingTextNameHash);
            if (selectedCase == null || selectedCase.Pool == null)
            {
                Debug.LogError($"[FloatingTextController] SpawnTextInternal: FloatingTextCase 또는 그 내부 Pool이 null입니다. NameHash: {floatingTextNameHash}");
                return null;
            }

            GameObject floatingTextObject = selectedCase.Pool.GetPooledObject();
            if (floatingTextObject == null)
            {
                Debug.LogError($"[FloatingTextController] SpawnTextInternal: Pool에서 null 오브젝트를 반환받았습니다. NameHash: {floatingTextNameHash}");
                return null;
            }
            
            // 위치와 회전은 호출하는 쪽에서 설정 후 Activate 하도록 변경. 여기서는 오브젝트만 준비.
            // floatingTextObject.transform.position = position;
            // floatingTextObject.transform.rotation = rotation;
            floatingTextObject.SetActive(true); // 풀에서 가져온 후 활성화

            FloatingTextBaseBehavior behaviorScript = floatingTextObject.GetComponent<FloatingTextBaseBehavior>();
            if (behaviorScript == null)
            {
                 Debug.LogError($"[FloatingTextController] SpawnTextInternal: 생성된 오브젝트 '{floatingTextObject.name}'에 FloatingTextBaseBehavior 컴포넌트가 없습니다. NameHash: {floatingTextNameHash}");
                 floatingTextObject.SetActive(false); // 문제 있는 오브젝트는 다시 비활성화
                 return null;
            }
            
            return behaviorScript;
        }

        // --- public static SpawnFloatingText 오버로드 메서드들 ---
        // 이제 subjectOfText 파라미터를 받고, 내부적으로 SpawnTextInternal을 호출한 뒤 Activate를 호출합니다.

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, GameObject subjectOfText = null)
        {
            if (instance == null) { Debug.LogError("[FloatingTextController] Static SpawnFloatingText: Instance is null."); return null; }

            int nameHash = floatingTextName.GetHashCode();
            FloatingTextBaseBehavior behavior = instance.SpawnTextInternal(nameHash, subjectOfText);
            
            if(behavior != null)
            {
                behavior.transform.SetPositionAndRotation(position, rotation); // 위치/회전 설정
                // FloatingTextBaseBehavior의 Activate(string, float, Color)를 기본값으로 호출
                behavior.Activate(text, 1.0f, Color.white); 
            }
            return behavior;
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color, GameObject subjectOfText = null)
        {
            if (instance == null) { Debug.LogError("[FloatingTextController] Static SpawnFloatingText: Instance is null."); return null; }

            int nameHash = floatingTextName.GetHashCode();
            FloatingTextBaseBehavior behavior = instance.SpawnTextInternal(nameHash, subjectOfText);

            if (behavior != null)
            {
                behavior.transform.SetPositionAndRotation(position, rotation);
                behavior.Activate(text, scaleMultiplier, color);
            }
            return behavior;
        }
        
        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color, GameObject subjectOfText = null)
        {
            if (instance == null) { Debug.LogError("[FloatingTextController] Static SpawnFloatingText: Instance is null."); return null; }

            FloatingTextBaseBehavior behavior = instance.SpawnTextInternal(floatingTextNameHash, subjectOfText);

            if (behavior != null)
            {
                behavior.transform.SetPositionAndRotation(position, rotation);
                behavior.Activate(text, scaleMultiplier, color);
            }
            return behavior;
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color, bool isCritical, GameObject subjectOfText = null)
        {
            if (instance == null) { Debug.LogError("[FloatingTextController] Static SpawnFloatingText: Instance is null."); return null; }

            int nameHash = floatingTextName.GetHashCode();
            FloatingTextBaseBehavior behavior = instance.SpawnTextInternal(nameHash, subjectOfText);

            if (behavior != null)
            {
                behavior.transform.SetPositionAndRotation(position, rotation);
                // isCritical을 받는 Activate 오버로드가 FloatingTextBaseBehavior에 있으므로 그것을 사용
                // FloatingTextHitBehaviour로의 캐스팅은 해당 클래스에만 특화된 Activate 로직이 있을 경우에만 필요
                behavior.Activate(text, scaleMultiplier, color, isCritical);
            }
            return behavior;
        }
        
        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color, bool isCritical, GameObject subjectOfText = null)
        {
            if (instance == null) { Debug.LogError("[FloatingTextController] Static SpawnFloatingText: Instance is null."); return null; }

            FloatingTextBaseBehavior behavior = instance.SpawnTextInternal(floatingTextNameHash, subjectOfText);

            if (behavior != null)
            {
                behavior.transform.SetPositionAndRotation(position, rotation);
                behavior.Activate(text, scaleMultiplier, color, isCritical);
            }
            return behavior;
        }

        /// <summary>
        /// 플로팅 텍스트 종류와 해당 프리팹, 풀 설정을 정의하는 중첩 클래스입니다.
        /// </summary>
        [System.Serializable]
        public class FloatingTextCase
        {
            [Tooltip("플로팅 텍스트 식별 이름입니다.")]
            [SerializeField]
            private string name;
            public string Name => name;

            [Tooltip("플로팅 텍스트 프리팹입니다. FloatingTextBaseBehavior를 상속받는 컴포넌트가 있어야 합니다.")]
            [SerializeField]
            private GameObject prefab;
            public GameObject Prefab => prefab;

            [Tooltip("미리 생성해 둘 플로팅 텍스트 오브젝트의 수입니다.")]
            [SerializeField]
            private int poolSize = 10;

            [System.NonSerialized] // 런타임에만 사용, 직렬화 불필요
            private Pool pool;
            public Pool Pool => pool;

            /// <summary>
            /// 이 플로팅 텍스트 케이스에 대한 오브젝트 풀을 초기화합니다.
            /// </summary>
            public void Initialise()
            {
                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogError("[FloatingTextCase] Initialise: 케이스 이름이 비어있습니다.");
                    return;
                }
                if (prefab == null)
                {
                    Debug.LogError($"[FloatingTextCase] '{name}' 케이스의 Prefab이 할당되지 않았습니다.");
                    return;
                }
                if (prefab.GetComponent<FloatingTextBaseBehavior>() == null)
                {
                    Debug.LogError($"[FloatingTextCase] '{name}' 케이스의 Prefab '{prefab.name}'에 FloatingTextBaseBehavior가 없습니다.");
                    return;
                }

                string poolName = name + "_FloatingTextPool";

                // PoolManager에 이미 해당 이름의 풀이 있는지 확인
                if (PoolManager.HasPool(poolName))
                {
                    Debug.LogWarning($"[FloatingTextCase] '{name}': 풀 '{poolName}'이 이미 PoolManager에 존재합니다. 기존 풀을 사용하거나 확인이 필요합니다.");
                    // 필요하다면 기존 풀을 가져와서 재사용하는 로직 추가 가능
                    // pool = PoolManager.GetPoolByName(poolName) as Pool; // Pool 타입으로 캐스팅 필요
                    // if(pool == null) Debug.LogError($"기존 풀 {poolName}을 가져오는데 실패했습니다.");
                    // 현재는 이미 존재하면 새로 생성하지 않고, 기존 풀이 사용되길 기대하거나, 경고만 남깁니다.
                    // 혹은, ClearAllPools가 완벽히 동작했다면 이 경우는 발생하지 않아야 합니다.
                    // 만약 이 경고가 계속 뜬다면, ClearAllPools가 원하는 대로 동작하지 않는 것입니다.
                }
                else
                {
                    // 새 풀 생성 및 등록 (Pool 생성자에서 PoolManager.AddPool 자동 호출 가정)
                    pool = new Pool(prefab, poolName); 
                    Debug.Log($"[FloatingTextCase] '{name}': 새 풀 '{poolName}' 생성 시도.");
                }

                // 풀이 성공적으로 준비되었고 (생성 또는 기존 참조), poolSize가 0보다 클 때만 CreatePoolObjects 호출
                if (pool != null && PoolManager.HasPool(poolName)) // PoolManager 등록 여부 재확인
                {
                    if (poolSize > 0)
                    {
                        // CreatePoolObjects는 내부적으로 Init을 다시 호출할 수 있으므로,
                        // Pool.Init이 중복 호출되어도 문제가 없도록 설계되어야 함 (inited 플래그)
                        pool.CreatePoolObjects(poolSize);
                    }
                    Debug.Log($"[FloatingTextCase] '{name}' 케이스 풀 '{poolName}' 준비 완료. 요청 크기: {poolSize}");
                }
                else if (pool == null && !PoolManager.HasPool(poolName)) // 위에서 새로 생성 시도했는데 실패한 경우
                {
                    Debug.LogError($"[FloatingTextCase] '{name}' 케이스에 대한 풀 '{poolName}' 생성에 실패했습니다 (PoolManager에 없음).");
                }
                else if (pool != null && !PoolManager.HasPool(poolName)) // Pool 객체는 생성되었으나 Manager에 등록 실패
                {
                    Debug.LogError($"[FloatingTextCase] '{name}' 케이스에 대한 풀 '{poolName}'은 생성되었으나 PoolManager 등록에 실패했습니다.");
                }
            }
        }
    }
}