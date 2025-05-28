// 이 스크립트는 미니건 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 명중 시 특정 파티클 및 트레일 관리를 추가합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // PlayerBulletBehavior를 상속받아 플레이어 투사체의 기본 기능을 활용합니다.
    public class MinigunBulletBehavior : PlayerBulletBehavior
    {
        // 적 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_HIT_HASH = "Minigun Hit".GetHashCode();
        // 벽 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_WAll_HIT_HASH = "Minigun Wall Hit".GetHashCode();

        [Tooltip("투사체 이동 경로를 시각적으로 표시하는 트레일 렌더러 컴포넌트입니다.")]
        [SerializeField] TrailRenderer trailRenderer;

        /// <summary>
        /// 미니건 투사체를 초기화합니다.
        /// 기본 투사체 정보 설정 후 트레일 렌더러를 초기화합니다.
        /// </summary>
        /// <param name="damage">투사체의 데미지 값</param>
        /// <param name="speed">투사체의 이동 속도</param>
        /// <param name="currentTarget">투사체의 목표 적</param>
        /// <param name="autoDisableTime">자동 비활성화 시간</param>
        /// <param name="autoDisableOnHit">충돌 시 자동 비활성화 여부 (기본값 true)</param>
        public override void Init(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            // 상위 클래스의 Init 함수를 호출하여 기본 투사체 속성을 설정합니다.
            base.Init(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            // 트레일 렌더러의 이전에 그려진 경로를 지웁니다.
            trailRenderer.Clear();
        }

        /// <summary>
        /// 적에게 명중했을 때 호출됩니다.
        /// 특정 명중 파티클을 재생하고 트레일 렌더러를 초기화합니다.
        /// </summary>
        /// <param name="baseEnemyBehavior">명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            // 미니건 명중 파티클을 재생합니다.
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            // 트레일 렌더러의 경로를 지웁니다.
            trailRenderer.Clear();
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다.
        /// 기본 장애물 충돌 처리와 함께 특정 벽 충돌 파티클을 재생하고 트레일 렌더러를 초기화합니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            // 상위 클래스의 장애물 충돌 처리 함수를 호출합니다.
            base.OnObstacleHitted();

            // 미니건 벽 충돌 파티클을 재생합니다.
            ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
            // 트레일 렌더러의 경로를 지웁니다.
            trailRenderer.Clear();
        }
    }
}