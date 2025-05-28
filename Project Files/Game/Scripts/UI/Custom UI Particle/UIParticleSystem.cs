#pragma warning disable 0414
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    using UI.Particle;

    /// <summary>
    ///   UI 파티클 시스템 클래스.
    ///   UI 환경에서 파티클 효과를 생성하고 관리합니다.
    ///   파티클 생성, 풀링, Burst 효과 등을 지원합니다.
    /// </summary>
    public class UIParticleSystem : MonoBehaviour
    {
        /// <summary>
        ///   파티클 시스템이 부착된 RectTransform 컴포넌트.
        ///   파티클의 위치 및 부모 설정을 위해 사용됩니다.
        /// </summary>
        [Tooltip("파티클 시스템이 부착된 RectTransform 컴포넌트")]
        private RectTransform rectTransform;

        /// <summary>
        ///   파티클 생성 및 동작 관련 설정 데이터.
        ///   UIParticleSettings 스크립터블 오브젝트를 통해 설정됩니다.
        /// </summary>
        [Tooltip("파티클 생성 및 동작 관련 설정 데이터")]
        [SerializeField] UIParticleSettings settings;

        /// <summary>
        ///   파티클 오브젝트 풀.
        ///   파티클 재사용을 위해 사용되며, 런타임에 생성 및 관리됩니다.
        /// </summary>
        [Tooltip("파티클 오브젝트 풀")]
        private RuntimeGenericPool<UIParticle> particlesPool;

        /// <summary>
        ///   파티클 시스템 재생 여부.
        ///   true이면 파티클이 생성되고, false이면 생성되지 않습니다.
        /// </summary>
        [Tooltip("파티클 시스템 재생 여부")]
        public bool IsPlaying { get; set; }

        /// <summary>
        ///   현재 활성화된 파티클 목록.
        ///   생존 시간 관리 및 업데이트를 위해 사용됩니다.
        /// </summary>
        [Tooltip("현재 활성화된 파티클 목록")]
        private List<UIParticle> particles;

        /// <summary>
        ///   모든 파티클이 소멸했을 때 게임 오브젝트를 비활성화할지 여부.
        ///   true이면 비활성화, false이면 유지.
        /// </summary>
        [Tooltip("모든 파티클이 소멸했을 때 게임 오브젝트를 비활성화할지 여부")]
        public bool DisableWhenReady { get; set; }

        /// <summary>
        ///   파티클 생성 간격 (초).
        ///   emissionPerSecond 설정에 따라 계산됩니다.
        /// </summary>
        [Tooltip("파티클 생성 간격 (초)")]
        private float spawnRate;

        /// <summary>
        ///   마지막 파티클 생성 시간.
        ///   spawnRate에 따라 파티클 생성을 제어하는 데 사용됩니다.
        /// </summary>
        [Tooltip("마지막 파티클 생성 시간")]
        private float lastSpawnTime;

        /// <summary>
        ///   Burst 데이터 목록.
        ///   Burst 이벤트 발생 시점을 관리하는 데 사용됩니다.
        /// </summary>
        [Tooltip("Burst 데이터 목록")]
        private List<BurstData> burstsData = new List<BurstData>();

        /// <summary>
        ///   초기화 여부 플래그.
        ///   중복 초기화를 방지하기 위해 사용됩니다.
        /// </summary>
        [Tooltip("초기화 여부 플래그")]
        private bool isInited = false;

        /// <summary>
        ///   Awake: 초기화 함수 호출.
        ///   컴포넌트가 활성화될 때 한 번 호출됩니다.
        /// </summary>
        private void Awake()
        {
            Init();
        }

        /// <summary>
        ///   Init: 파티클 시스템 초기화.
        ///   RectTransform, 파티클 풀, 생성 간격, Burst 데이터 등을 초기화합니다.
        /// </summary>
        private void Init()
        {
            rectTransform = GetComponent<RectTransform>();

            // 최대 파티클 개수 계산
            int maxCount = 0;
            if (settings.emissionPerSecond > 0)
            {
                spawnRate = 1f / settings.emissionPerSecond;
                maxCount = Mathf.CeilToInt(settings.emissionPerSecond * settings.lifetime.Max * 1.2f);
            }
            else
            {
                spawnRate = float.PositiveInfinity;
            }

            // Burst 데이터 초기화
            if (!settings.bursts.IsNullOrEmpty())
            {
                for (int i = 0; i < settings.bursts.Length; i++)
                {
                    var burst = settings.bursts[i];

                    var burstCount = settings.lifetime.Max / burst.interval;
                    if (burst.loopsCount < 0)
                    {
                        maxCount += Mathf.CeilToInt(burstCount) * burst.count;
                    }
                    else
                    {
                        if (burstCount > burst.loopsCount)
                        {
                            maxCount += burst.loopsCount * burst.count;
                        }
                        else
                        {
                            maxCount += Mathf.CeilToInt(burstCount) * burst.count;
                        }
                    }

                    burstsData.Add(new BurstData
                    {
                        isActive = burst.loopsCount != 0,
                        burstSettings = burst,
                        counter = 0,
                        timeToSpawn = Time.time + burst.delay
                    });
                }
            }

            Debug.Log(maxCount);
            if (maxCount == 0) maxCount = 1;

            // 파티클 풀 생성
            particlesPool = new RuntimeGenericPool<UIParticle>(settings.uiParticlePrefab, maxCount, rectTransform);

            lastSpawnTime = Time.time + settings.startDelay;

            particles = new List<UIParticle>();

            isInited = true;
        }

        /// <summary>
        ///   OnEnable: 컴포넌트 활성화 시 처리.
        ///   playOnAwake 설정에 따라 파티클 시스템을 재생합니다.
        /// </summary>
        private void OnEnable()
        {
            if (settings.playOnAwake)
            {
                IsPlaying = true;
                lastSpawnTime = Time.time + settings.startDelay;
                DisableWhenReady = false;
            }
        }

        /// <summary>
        ///   LateUpdate: 프레임 후반에 실행되는 업데이트.
        ///   파티클 생존 여부 확인, 소멸 처리, 파티클 생성 등을 처리합니다.
        /// </summary>
        private void LateUpdate()
        {
            // 활성화된 파티클 업데이트 및 소멸 처리
            for (int i = 0; i < particles.Count; i++)
            {
                var particle = particles[i];

                if (particle.Tick())
                {
                    particles.RemoveAt(i);
                    i--;

                    particle.IsActive = false;

                    continue;
                }
            }

            // 파티클 시스템 재생이 끝나고 모든 파티클이 소멸하면 게임 오브젝트 비활성화
            if (DisableWhenReady && particles.Count == 0) gameObject.SetActive(false);

            // 파티클 시스템이 재생 중이 아니면 파티클 생성 중지
            if (!IsPlaying)
            {
                lastSpawnTime = Time.time;

                return;
            }

            // emissionPerSecond 설정에 따라 파티클 생성
            var timeSpend = Time.time - lastSpawnTime;

            if (settings.emissionPerSecond > 0 && timeSpend >= spawnRate)
            {
                do
                {
                    timeSpend -= spawnRate;

                    SpawnParticle(timeSpend);

                } while (timeSpend >= spawnRate);

                lastSpawnTime = Time.time - timeSpend;
            }

            // Burst 이벤트 처리
            for (int i = 0; i < burstsData.Count; i++)
            {
                var burst = burstsData[i];
                if (!burst.isActive) return;

                if (Time.time >= burst.timeToSpawn)
                {
                    timeSpend = Time.time - burst.timeToSpawn;

                    for (int j = 0; j < burst.burstSettings.count; j++)
                    {
                        SpawnParticle(timeSpend);
                    }

                    burst.counter++;

                    if (burst.counter >= burst.burstSettings.loopsCount && burst.burstSettings.loopsCount >= 0)
                    {
                        burst.isActive = false;
                    }
                    else
                    {
                        burst.timeToSpawn += burst.burstSettings.interval;
                    }

                }
            }
        }

        /// <summary>
        ///   SpawnParticle: 파티클 생성 함수.
        ///   설정에 따라 파티클을 생성하고 초기화합니다.
        /// </summary>
        /// <param name="timeSpend">파티클 생성 지연 시간</param>
        private void SpawnParticle(float timeSpend)
        {
            var particle = particlesPool.GetComponent();

            var spawnPos = Vector2.zero;

            switch (settings.shape)
            {
                case UIParticleSettings.Shape.Circle:
                    spawnPos = Random.insideUnitCircle * settings.circleRadius;
                    break;
                case UIParticleSettings.Shape.Rect:
                    var halfSize = settings.rectSize / 2f;
                    spawnPos = new Vector2(Random.Range(-halfSize.x, halfSize.x), Random.Range(-halfSize.y, halfSize.y));
                    break;
            }

            particle.Init(settings, timeSpend, spawnPos, Vector2.up);

            particles.Add(particle);
        }
    }

    /// <summary>
    ///   Burst 데이터 클래스.
    ///   Burst 이벤트 관련 정보를 저장합니다.
    /// </summary>
    public class BurstData
    {
        /// <summary>
        ///   Burst 이벤트 활성화 여부.
        /// </summary>
        public bool isActive;

        /// <summary>
        ///   Burst 이벤트 설정 데이터.
        /// </summary>
        public UIParticleSettings.BurstSettings burstSettings;

        /// <summary>
        ///   Burst 이벤트 발생 횟수 카운터.
        /// </summary>
        public int counter;

        /// <summary>
        ///   다음 Burst 이벤트 발생 시간.
        /// </summary>
        public float timeToSpawn;
    }

    /// <summary>
    ///   RuntimeGenericPool: 런타임에 생성되는 오브젝트 풀 클래스.
    ///   제네릭을 사용하여 다양한 타입의 컴포넌트를 풀링할 수 있습니다.
    /// </summary>
    /// <typeparam name="T">풀링할 컴포넌트 타입</typeparam>
    public class RuntimeGenericPool<T> where T : MonoBehaviour
    {
        /// <summary>
        ///   풀링된 컴포넌트 목록.
        /// </summary>
        private List<T> pooledComponents;

        /// <summary>
        ///   풀링할 프리팹.
        /// </summary>
        private GameObject prefab;

        /// <summary>
        ///   생성된 오브젝트의 부모 Transform.
        /// </summary>
        private Transform parent;

        /// <summary>
        ///   RuntimeGenericPool 생성자.
        ///   풀을 초기화하고, 초기 개수만큼 오브젝트를 생성합니다.
        /// </summary>
        /// <param name="prefab">풀링할 프리팹</param>
        /// <param name="maxCount">최대 풀 크기</param>
        /// <param name="parent">생성된 오브젝트의 부모 Transform</param>
        public RuntimeGenericPool(GameObject prefab, int maxCount, Transform parent)
        {
            pooledComponents = new List<T>();

            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < maxCount; i++)
            {
                InstantiateComponent(prefab, parent);
            }
        }

        /// <summary>
        ///   GetComponent: 풀에서 사용 가능한 컴포넌트를 가져옵니다.
        ///   사용 가능한 오브젝트가 없으면 새로 생성합니다.
        /// </summary>
        /// <returns>사용 가능한 컴포넌트</returns>
        public T GetComponent()
        {
            for (int i = 0; i < pooledComponents.Count; i++)
            {
                var component = pooledComponents[i];
                if (!component.gameObject.activeSelf)
                {
                    component.gameObject.SetActive(true);
                    return component;
                }
            }

            return InstantiateComponent(prefab, parent, false);
        }

        /// <summary>
        ///   InstantiateComponent: 새로운 컴포넌트를 생성합니다.
        /// </summary>
        /// <param name="prefab">생성할 프리팹</param>
        /// <param name="parent">생성된 오브젝트의 부모 Transform</param>
        /// <param name="reset">생성 후 비활성화 여부</param>
        /// <returns>생성된 컴포넌트</returns>
        private T InstantiateComponent(GameObject prefab, Transform parent, bool reset = true)
        {
            var component = Object.Instantiate(prefab, parent).GetComponent<T>();

            if (reset)
            {
                component.gameObject.SetActive(false);
            }

            pooledComponents.Add(component);

            return component;
        }
    }
}