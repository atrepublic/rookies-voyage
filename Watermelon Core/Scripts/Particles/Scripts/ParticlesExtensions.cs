// ParticlesExtensions.cs
// 이 스크립트는 Unity의 ParticleSystem 클래스에 확장 메서드를 제공하여
// ParticlesController를 통해 지연 시간(delay) 옵션과 함께 파티클을 재생할 수 있도록 합니다.

using UnityEngine;

namespace Watermelon
{
    public static class ParticlesExtensions
    {
        /// <summary>
        /// ParticleSystem 인스턴스를 ParticlesController를 사용해 재생합니다.
        /// </summary>
        /// <param name="particleSystem">재생할 ParticleSystem 컴포넌트</param>
        /// <param name="delay">재생 전에 대기할 시간(초). 기본값은 0입니다.</param>
        /// <returns>활성화된 파티클을 관리하는 ParticleCase 객체</returns>
        public static ParticleCase PlayCase(this ParticleSystem particleSystem, float delay = 0)
        {
            return ParticlesController.PlayParticle(particleSystem, delay);
        }
    }
}
