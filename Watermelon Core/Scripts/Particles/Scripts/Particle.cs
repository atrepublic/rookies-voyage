// Particle.cs
// 이 스크립트는 파티클 효과를 관리하는 클래스로, 파티클 프리팹의 초기화, 재사용(오브젝트 풀링), 재생 및 파괴 기능을 제공합니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Particle
    {
        [SerializeField]
        [Tooltip("파티클 고유 이름(등록 및 식별 용)")]
        private string particleName;

        [SerializeField]
        [Tooltip("파티클 효과가 적용된 프리팹 (ParticleSystem을 포함해야 함)")]
        private GameObject particlePrefab;

        /// <summary>
        /// 파티클의 이름을 반환합니다.
        /// </summary>
        public string ParticleName => particleName;

        /// <summary>
        /// 파티클 효과를 포함한 프리팹을 반환합니다.
        /// </summary>
        public GameObject ParticlePrefab => particlePrefab;

        /// <summary>
        /// 파티클에 커스텀 동작 스크립트가 연결되어 있는지 여부
        /// </summary>
        public bool SpecialBehaviour { get; private set; }

        /// <summary>
        /// 파티클 인스턴스를 관리하는 오브젝트 풀
        /// </summary>
        public Pool ParticlePool { get; private set; }

        [System.NonSerialized]
        private bool isInitialized;

        /// <summary>
        /// 생성자: 파티클 이름과 프리팹을 설정하며 유효성 검사를 수행합니다.
        /// </summary>
        public Particle(string particleName, GameObject particlePrefab)
        {
            this.particleName = particleName;
            this.particlePrefab = particlePrefab;

            if (string.IsNullOrEmpty(particleName))
                Debug.LogError("[Particles]: Particle name can't be empty!");

            if (particlePrefab == null)
                Debug.LogError($"[Particles]: Prefab isn't linked for {particleName} particle");
        }

        /// <summary>
        /// 파티클을 초기화하여 풀을 생성하고, 특수 동작 여부를 확인합니다.
        /// </summary>
        public void Init()
        {
            if (string.IsNullOrEmpty(particleName))
            {
                Debug.LogError("[Particles]: Particle name can't be empty!");
                return;
            }

            if (particlePrefab == null)
            {
                Debug.LogError($"[Particles]: Prefab isn't linked for {particleName} particle");
                return;
            }

            if (isInitialized) return;
            isInitialized = true;

            // 오브젝트 풀 생성
            ParticlePool = new Pool(particlePrefab, $"Particle_{ParticleName}");

            // 특수 동작 스크립트 확인
            SpecialBehaviour = particlePrefab.GetComponent<ParticleBehaviour>() != null;

            // ParticleSystem 컴포넌트 확인
            if (particlePrefab.GetComponent<ParticleSystem>() == null)
            {
                Debug.LogError($"[Particles]: Particle ({particleName}) prefab doesn't contain a ParticleSystem component!", particlePrefab);
            }
        }

        /// <summary>
        /// 풀 및 초기화 상태를 해제합니다.
        /// </summary>
        public void Destroy()
        {
            isInitialized = false;
            PoolManager.DestroyPool(ParticlePool);
            ParticlePool = null;
        }

        /// <summary>
        /// 파티클 효과를 재생합니다. 지연(delay) 시간을 지정할 수 있습니다.
        /// </summary>
        /// <param name="delay">재생 전 대기 시간(초)</param>
        /// <returns>재생된 파티클 케이스 객체</returns>
        public ParticleCase Play(float delay = 0)
        {
            return ParticlesController.PlayParticle(this, delay);
        }
    }
}
