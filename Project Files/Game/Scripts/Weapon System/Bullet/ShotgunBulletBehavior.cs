// 이 스크립트는 샷건 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 명중 시 특정 파티클, 트레일 관리 및 초기 크기 애니메이션을 추가합니다.
using UnityEngine;
// using DG.Tweening; 네임스페이스는 DOTween 플러그인 사용에 필요하지만, 현재 코드에 명시적으로 포함되어 있지 않습니다.
// Tweening 관련 기능(DOScale, SetEasing 등)은 해당 플러그인이 프로젝트에 추가되어 있음을 가정합니다.

namespace Watermelon.SquadShooter
{
    // PlayerBulletBehavior를 상속받아 플레이어 투사체의 기본 기능을 활용합니다.
    public class ShotgunBulletBehavior : PlayerBulletBehavior
    {
        // 적 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_HIT_HASH = "Shotgun Hit".GetHashCode();
        // 벽 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_WALL_HIT_HASH = "Shotgun Wall Hit".GetHashCode();

        [Tooltip("투사체 이동 경로를 시각적으로 표시하는 트레일 렌더러 컴포넌트입니다.")]
        [SerializeField] TrailRenderer trailRenderer;
        [Tooltip("투사체의 그래픽을 나타내는 트랜스폼입니다 (크기 애니메이션 등에 사용될 수 있습니다).")]
        [SerializeField] Transform graphicsTransform; // 현재 코드에서는 graphicsTransform 변수가 직접 사용되지 않지만, 필드로 남겨둡니다.

        /// <summary>
        /// 샷건 투사체를 초기화합니다.
        /// 기본 투사체 정보 설정 후 트레일 렌더러를 초기화하고 크기 애니메이션을 시작합니다.
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

            // 투사체의 초기 스케일을 작게 설정하고, 짧은 시간 동안 원래 크기로 커지는 애니메이션을 실행합니다.
            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn);
        }

        /// <summary>
        /// 적에게 명중했을 때 호출됩니다.
        /// 특정 명중 파티클을 재생하고 트레일 렌더러를 초기화합니다.
        /// </summary>
        /// <param name="baseEnemyBehavior">명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            // 샷건 명중 파티클을 재생합니다.
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

            // 샷건 벽 충돌 파티클을 재생합니다.
            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            // 트레일 렌더러의 경로를 지웁니다.
            trailRenderer.Clear();
        }
    }
}