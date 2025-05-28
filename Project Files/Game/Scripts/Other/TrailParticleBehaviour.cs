// 이 스크립트는 트레일 파티클 시스템의 동작을 정의합니다.
// ParticleBehaviour 기본 클래스를 상속받아, 파티클이 비활성화될 때 트레일을 초기화하는 기능을 구현합니다.
using UnityEngine;

namespace Watermelon
{
    // 트레일 파티클의 동작을 관리하는 클래스입니다.
    // ParticleBehaviour를 상속받아 파티클 시스템 이벤트에 반응합니다.
    // sealed 키워드는 이 클래스가 더 이상 상속될 수 없음을 나타냅니다.
    public sealed class TrailParticleBehaviour : ParticleBehaviour
    {
        // 관리할 트레일 렌더러(TrailRenderer) 컴포넌트 배열입니다.
        [Tooltip("관리할 트레일 렌더러 컴포넌트 배열입니다.")]
        [SerializeField] TrailRenderer[] trails;

        // ParticleBehaviour의 오버라이드 메소드: 파티클이 활성화될 때 호출됩니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 활성화 로직을 추가할 수 있습니다.
        public override void OnParticleActivated()
        {
            // 파티클 활성화 시 동작 (필요하다면 추가)
        }

        // ParticleBehaviour의 오버라이드 메소드: 파티클이 비활성화될 때 호출됩니다.
        // 연결된 모든 트레일 렌더러의 트레일 데이터를 초기화하여 잔상이 남지 않도록 합니다.
        public override void OnParticleDisabled()
        {
            // 트레일 렌더러 배열을 순회하며 각 트레일을 초기화합니다.
            for (int i = 0; i < trails.Length; i++)
            {
                // 현재 트레일 렌더러가 유효한지 확인합니다.
                if (trails[i] != null)
                {
                    // 트레일 데이터를 초기화합니다.
                    trails[i].Clear();
                }
            }
        }
    }
}