using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    using Watermelon.UI.Particle;

    /// <summary>
    ///   UI 파티클 트레일 관리 클래스.
    ///   특정 RectTransform을 따라 파티클을 생성하고 관리합니다.
    /// </summary>
    public class UIParticleTrail : MonoBehaviour
    {
        /// <summary>
        ///   파티클 생성 및 동작 관련 설정.
        /// </summary>
        [Tooltip("파티클 생성 및 동작 관련 설정")]
        [SerializeField] UIParticleSettings settings;

        /// <summary>
        ///   파티클이 따라다닐 대상 RectTransform.
        /// </summary>
        [Tooltip("파티클이 따라다닐 대상 RectTransform")]
        [SerializeField] RectTransform targetRect;

        /// <summary>
        ///   대상 RectTransform의 AnchoredPosition.
        /// </summary>
        public Vector2 AnchoredPos { get => targetRect.anchoredPosition; set => targetRect.anchoredPosition = value; }

        /// <summary>
        ///   파티클 풀.
        ///   파티클 재사용을 위해 사용합니다.
        /// </summary>
        private PoolGeneric<UIParticle> particlePool;

        /// <summary>
        ///   트레일 재생 여부.
        ///   true이면 파티클이 생성되고, false이면 생성되지 않습니다.
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        ///   모든 파티클이 사라지면 게임 오브젝트를 비활성화할지 여부.
        ///   true이면 비활성화하고, false이면 유지합니다.
        /// </summary>
        public bool DisableWhenReady { get; set; }

        /// <summary>
        ///   파티클 생성 간격.
        /// </summary>
        private float spawnRate;

        /// <summary>
        ///   마지막 파티클 생성 시간.
        /// </summary>
        private float lastSpawnTime;

        /// <summary>
        ///   파티클 이동 방향 (정규화된 값).
        /// </summary>
        public Vector3 NormalizedVelocity { get; set; }

        /// <summary>
        ///   현재 활성화된 파티클 목록.
        /// </summary>
        private List<UIParticle> particles = new List<UIParticle>();

        /// <summary>
        ///   초기화.
        ///   파티클 생성 간격 계산 및 마지막 생성 시간 초기화.
        /// </summary>
        private void Awake()
        {
            spawnRate = 1f / settings.emissionPerSecond;

            lastSpawnTime = Time.time;
        }

        /// <summary>
        ///   파티클 풀 설정.
        /// </summary>
        /// <param name="particlesPool">파티클 풀</param>
        public void SetPool(PoolGeneric<UIParticle> particlesPool)
        {
            particlePool = particlesPool;
        }

        /// <summary>
        ///   초기화.
        ///   DisableWhenReady 플래그 초기화 및 마지막 생성 시간 초기화.
        /// </summary>
        public void Init()
        {
            DisableWhenReady = false;
            lastSpawnTime = Time.time;
        }

        /// <summary>
        ///   프레임 후반에 실행되는 업데이트.
        ///   파티클 생존 여부 확인 및 생성 처리.
        /// </summary>
        private void LateUpdate()
        {
            // 파티클 생존 여부 확인 및 제거
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

            // 모든 파티클이 사라지면 게임 오브젝트 비활성화 처리
            if (DisableWhenReady && particles.Count == 0) gameObject.SetActive(false);

            // 트레일이 재생 중이 아니면 파티클 생성 중지
            if (!IsPlaying)
            {
                lastSpawnTime = Time.time;

                return;
            }

            // 파티클 생성 간격에 따라 파티클 생성
            var timeSpend = Time.time - lastSpawnTime;

            if (timeSpend >= spawnRate)
            {
                do
                {
                    timeSpend -= spawnRate;

                    // 파티클 풀에서 파티클 가져와 초기화 후 활성화
                    var particle = particlePool.GetPooledComponent();

                    particle.Init(settings, timeSpend, targetRect.anchoredPosition, NormalizedVelocity);

                    particles.Add(particle);

                } while (timeSpend >= spawnRate);

                lastSpawnTime = Time.time - timeSpend;
            }
        }

    }
}