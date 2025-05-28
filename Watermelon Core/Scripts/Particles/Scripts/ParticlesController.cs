// ============================================================
// ParticlesController.cs
// ------------------------------------------------------------
// 🔹 스크립트 요약
//   - 파티클 풀링(오브젝트 풀) 시스템의 **중앙 컨트롤러**입니다.
//   - Inspector에 등록된 파티클 프리팹을 이름/해시로 레지스트리에 저장하고,
//     PlayParticle() 계열 메서드로 재생·지연 재생·중복 체크를 관리합니다.
//   - 활성 파티클 상태를 주기적으로 검사하여 수명이 끝나면 자동으로 풀로
//     반환해 Instantiate/Destroy 호출을 최소화하고 FPS 드롭 및 GC Alloc을
//     방지합니다.
// ------------------------------------------------------------
// ✅ 작성 규칙
//   • 코드 로직은 **원본을 100% 유지**하고, 한글 주석·툴팁만 추가했습니다.
//   • Unity 2023 이상 권장 문법(지역 변수 var 지양, 명확한 Generic 표기)을
//     따릅니다.
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ParticlesController : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────
        // ▷ 인스펙터 노출 변수
        // ────────────────────────────────────────────────────────

        [SerializeField, Tooltip("초기 등록 및 관리될 파티클 프리셋 배열 (Inspector에서 설정)")]
        Particle[] particles;

        // ────────────────────────────────────────────────────────
        // ▷ 내부 관리 컬렉션
        // ────────────────────────────────────────────────────────

        // 각 파티클 이름 해시를 키로 하여 빠르게 검색·호출할 수 있도록 저장합니다.
        private static Dictionary<int, Particle> registerParticles = new Dictionary<int, Particle>();

        // 현재 활성화되어 있는 파티클 케이스 목록입니다.
        private static List<ParticleCase> activeParticles = new List<ParticleCase>();

        // 지연 활성화가 설정된 파티클용 트윈(딜레이) 리스트입니다.
        private static List<TweenCase> delayedParticles = new List<TweenCase>();

        // =========================================================
        // 초기화 & 종료
        // =========================================================

        // 🟢 컨트롤러 초기화 ---------------------------------------------------
        /// <summary>
        /// Initializes the particles controller by registering all particles in the array.
        /// <para>배열에 등록된 파티클 프리셋을 풀에 등록하고, 활성 파티클 감시 코루틴을 시작합니다.</para>
        /// </summary>
        public void Init()
        {
            // Register particles from the array.
            for (int i = 0; i < particles.Length; i++)
            {
                RegisterParticle(particles[i]);
            }

            // Start the coroutine to monitor active particles.
            StartCoroutine(CheckForActiveParticles());
        }

        // 🔴 컨트롤러 종료 정리 -----------------------------------------------
        /// <summary>
        /// Clears all active and delayed particles, stopping them and removing from lists.
        /// <para>씬 종료 시 지연·활성 파티클을 정리하고 풀을 해제합니다.</para>
        /// </summary>
        private void OnDestroy()
        {
            // Kill all delayed particles.
            for (int i = 0; i < delayedParticles.Count; i++)
            {
                delayedParticles[i].KillActive();
            }
            delayedParticles.Clear();

            // Remove all active particles.
            activeParticles.Clear();

            // Destroy pool objects
            foreach (Particle particle in registerParticles.Values)
            {
                particle.Destroy();
            }
            registerParticles.Clear();
        }

        // =========================================================
        // 활성 파티클 모니터링
        // =========================================================

        // 🔄 활성 파티클 체크 루프 --------------------------------------------
        /// <summary>
        /// Coroutine to check for active particles and manage their lifecycle.
        /// <para>활성 파티클의 수명과 강제 종료 조건을 주기적으로 검사합니다.</para>
        /// </summary>
        private IEnumerator CheckForActiveParticles()
        {
            while (true)
            {
                // Wait for several frames to allow for updates.
                yield return null; yield return null; yield return null;
                yield return null; yield return null; yield return null;
                yield return null;

                // Loop through active particles in reverse to avoid index issues during removal.
                for (int i = activeParticles.Count - 1; i >= 0; i--)
                {
                    // Check if the particle case is still valid.
                    if (activeParticles[i] != null && activeParticles[i].ParticleSystem != null)
                    {
                        // If the particle requires forced disable, stop it.
                        if (activeParticles[i].IsForceDisabledRequired())
                        {
                            activeParticles[i].ParticleSystem.Stop();
                            activeParticles.RemoveAt(i);
                            continue;
                        }

                        // If the particle system is not alive, disable and remove it.
                        if (!activeParticles[i].ParticleSystem.IsAlive(true))
                        {
                            activeParticles[i].OnDisable();
                            activeParticles.RemoveAt(i);
                        }
                    }
                    else
                    {
                        // If the particle case is null, remove it from the active list.
                        activeParticles.RemoveAt(i);
                    }
                }
            }
        }

        // =========================================================
        // 파티클 활성화 (내부 전용)
        // =========================================================

        /// <remarks>
        /// 내부에서만 호출되어 풀링 오브젝트를 활성화하고 ParticleCase를 반환합니다.
        /// </remarks>
        private static ParticleCase ActivateParticle(Particle particle, float delay = 0)
        {
            GameObject particleObject = particle.ParticlePool.GetPooledObject();
            ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
            return ActivateParticle(particleSystem, delay, true);
        }

        /// <remarks>
        /// ParticleSystem을 직접 받아 활성화합니다. resetParent가 true이면 재생 후 원래 부모로 복귀시킵니다.
        /// </remarks>
        private static ParticleCase ActivateParticle(ParticleSystem particleSystem, float delay = 0, bool resetParent = true)
        {
            bool isDelayed = delay > 0;

            // Activate game object
            particleSystem.gameObject.SetActive(true);

            // Create a new ParticleCase for the activated particle.
            ParticleCase particleCase = new ParticleCase(particleSystem, isDelayed, resetParent);

            if (isDelayed)
            {
                TweenCase delayTweenCase = null;
                delayTweenCase = Tween.DelayedCall(delay, () =>
                {
                    // Play the particle system.
                    particleCase.ParticleSystem.Play();
                    activeParticles.Add(particleCase);
                    delayedParticles.Remove(delayTweenCase);
                });

                delayedParticles.Add(delayTweenCase);
                return particleCase;
            }

            // Immediately add the active particle case to the list.
            activeParticles.Add(particleCase);
            return particleCase;
        }

        // =========================================================
        // Register
        // =========================================================

        /// <summary>
        /// Registers a particle, ensuring it has a valid name and prefab.
        /// <para>파티클 이름과 프리팹을 검증한 뒤 레지스트리에 등록합니다.</para>
        /// </summary>
        /// <param name="particle">The particle to register.</param>
        /// <returns>An integer hash code for the registered particle, or -1 if registration fails.</returns>
        public static int RegisterParticle(Particle particle)
        {
            // Validate particle name.
            if (string.IsNullOrEmpty(particle.ParticleName))
            {
                Debug.LogError("[Particle Controller]: Particle can't be initialized with empty name!");
                return -1;
            }

            // Validate particle prefab.
            if (particle.ParticlePrefab == null)
            {
                Debug.LogError("[Particle Controller]: Particle can't be initialized without linked prefab!");
                return -1;
            }

            // Get the hash of the particle name.
            int particleHash = particle.ParticleName.GetHashCode();
            if (!registerParticles.ContainsKey(particleHash))
            {
                particle.Init();
                registerParticles.Add(particleHash, particle);
                return particleHash;
            }
            else
            {
                Debug.LogError($"[Particle Controller]: Particle with name {particle.ParticleName} already register!");
            }

            return -1;
        }

        /// <summary>
        /// Registers a particle by name and prefab.
        /// <para>이름과 프리팹만으로 파티클을 간편하게 등록합니다.</para>
        /// </summary>
        public static int RegisterParticle(string particleName, GameObject particlePrefab)
        {
            return RegisterParticle(new Particle(particleName, particlePrefab));
        }

        // =========================================================
        // Play
        // =========================================================

        /// <summary>
        /// Plays a particle by its **name** with an optional delay.
        /// <para>파티클 이름으로 검색하여 재생합니다.</para>
        /// </summary>
        public static ParticleCase PlayParticle(string particleName, float delay = 0)
        {
            int particleHash = particleName.GetHashCode();

            if (registerParticles.ContainsKey(particleHash))
            {
                return ActivateParticle(registerParticles[particleHash], delay);
            }

            Debug.LogError($"[Particles System]: Particle with type {particleName} is missing!");
            return null;
        }

        /// <summary>
        /// Plays a particle by its **hash** with an optional delay.
        /// <para>파티클 이름 해시로 검색하여 재생합니다.</para>
        /// </summary>
        public static ParticleCase PlayParticle(int particleHash, float delay = 0)
        {
            if (registerParticles.ContainsKey(particleHash))
            {
                return ActivateParticle(registerParticles[particleHash], delay);
            }

            Debug.LogError($"[Particles System]: Particle with hash {particleHash} is missing!");
            return null;
        }

        /// <summary>
        /// Plays a **Particle** instance directly with an optional delay.
        /// <para>Particle 객체를 직접 넘겨 재생합니다.</para>
        /// </summary>
        public static ParticleCase PlayParticle(Particle particle, float delay = 0)
        {
            if (particle == null)
            {
                Debug.LogError("PlayParticle error: 'particle' cannot be null. Please provide a valid Particle instance.");
                return null;
            }

            return ActivateParticle(particle, delay);
        }

        /// <summary>
        /// Initiates playback of a specified **ParticleSystem** instance, allowing for an optional delay.
        /// <para>지정한 ParticleSystem 인스턴스를 재생하며, 선택적으로 지연 시간을 둘 수 있습니다.</para>
        /// </summary>
        public static ParticleCase PlayParticle(ParticleSystem particleSystem, float delay = 0)
        {
            if (particleSystem == null)
            {
                Debug.LogError("PlayParticle error: 'particleSystem' cannot be null. Please provide a valid ParticleSystem instance.");
                return null;
            }

            return ActivateParticle(particleSystem, delay, false);
        }

        // =========================================================
        // Check Has
        // =========================================================

        /// <summary>
        /// Checks if a particle with the specified **name** exists in the particle registry.
        /// <para>지정한 이름을 가진 파티클이 레지스트리에 존재하는지 확인합니다.</para>
        /// </summary>
        public static bool HasParticle(string particleName)
        {
            int particleHash = particleName.GetHashCode();
            return registerParticles.ContainsKey(particleHash);
        }

        /// <summary>
        /// Checks if a particle with the specified **hash** exists in the particle registry.
        /// <para>지정한 해시를 가진 파티클이 레지스트리에 존재하는지 확인합니다.</para>
        /// </summary>
        public static bool HasParticle(int particleHash)
        {
            return registerParticles.ContainsKey(particleHash);
        }
    }
}
