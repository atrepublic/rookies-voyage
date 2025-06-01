// MinigunBulletBehavior.cs
// 이 스크립트는 미니건 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 명중 시 특정 파티클 및 트레일 관리를 추가합니다.
// PlayerBulletBehavior의 변경된 Init 시그니처를 따르도록 수정되었습니다.
using UnityEngine;
using Watermelon; // ParticlesController 등 Watermelon 프레임워크 사용

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 미니건 투사체의 특정 동작을 정의합니다.
    /// </summary>
    public class MinigunBulletBehavior : PlayerBulletBehavior
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 수정된 부분 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 적 명중 시 재생할 파티클 시스템의 이름 해시 값입니다.
        // ParticlesController.GetHash() 대신 string.GetHashCode() 사용
        private static readonly int PARTICLE_HIT_HASH = "Minigun Hit".GetHashCode();
        // 벽 명중 시 재생할 파티클 시스템의 이름 해시 값입니다. (WAll -> Wall 오타 수정 및 GetHashCode() 사용)
        private static readonly int PARTICLE_WALL_HIT_HASH = "Minigun Wall Hit".GetHashCode();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        [Header("미니건 투사체 전용 설정")]
        [Tooltip("투사체 이동 경로를 시각적으로 표시하는 트레일 렌더러 컴포넌트입니다.")]
        [SerializeField] TrailRenderer trailRenderer;

        /// <summary>
        /// 미니건 투사체를 초기화합니다.
        /// 기본 투사체 정보 설정 후 트레일 렌더러를 초기화합니다.
        /// </summary>
        /// <param name="baseDamageFromGun">총기에서 계산된 초기 데미지 값</param>
        /// <param name="bulletSpeed">투사체의 이동 속도</param>
        /// <param name="initialTargetForProjectile">투사체의 초기 목표 적 (미니건은 보통 직선 발사)</param>
        /// <param name="projectileAutoDisableTime">투사체가 자동으로 비활성화될 시간</param>
        /// <param name="projectileDisableOnHit">직접 충돌 시 투사체를 비활성화할지 여부 (미니건은 보통 true)</param>
        /// <param name="gunShotWasCritical">총구 발사 시점의 치명타 여부</param>
        /// <param name="projectileOwner">이 투사체를 발사한 캐릭터의 CharacterBehaviour</param>
        public override void Init(float baseDamageFromGun, float bulletSpeed, BaseEnemyBehavior initialTargetForProjectile, float projectileAutoDisableTime, bool projectileDisableOnHit, bool gunShotWasCritical, CharacterBehaviour projectileOwner)
        {
            // PlayerBulletBehavior의 Init 호출 (변경된 시그니처에 맞게 모든 인자 전달)
            base.Init(baseDamageFromGun, bulletSpeed, initialTargetForProjectile, projectileAutoDisableTime, projectileDisableOnHit, gunShotWasCritical, projectileOwner);

            if (trailRenderer == null)
            {
                Debug.LogWarning($"[MinigunBulletBehavior] ({this.gameObject.name}): TrailRenderer가 할당되지 않았습니다.");
            }
            else
            {
                trailRenderer.Clear(); // 재사용 시 이전 트레일 효과 제거
            }
            // 미니건 총알은 별도의 크기 애니메이션이 필요하지 않을 수 있으므로, 해당 로직은 추가하지 않습니다.
            // 필요하다면 ShotgunBulletBehavior처럼 DOTween 스케일 애니메이션을 추가할 수 있습니다.
        }

        /// <summary>
        /// 적에게 명중했을 때 호출됩니다. (PlayerBulletBehavior.OnTriggerEnter 내부에서 호출됨)
        /// 미니건 고유의 명중 파티클을 재생하고 트레일을 정리합니다.
        /// 플로팅 텍스트는 PlayerBulletBehavior.OnTriggerEnter에서 이미 생성됩니다.
        /// </summary>
        /// <param name="enemyHitByThisBullet">이번에 명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior enemyHitByThisBullet)
        {
            // 미니건 명중 파티클 재생
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH)?.SetPosition(transform.position); // null 체크 추가

            if (trailRenderer != null)
            {
                trailRenderer.Clear(); // 트레일 렌더러의 경로 지우기
            }
            // 데미지 텍스트 생성 로직은 PlayerBulletBehavior.OnTriggerEnter로 이전되었으므로 여기서는 호출하지 않습니다.
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다.
        /// 기본 장애물 충돌 처리(PlayerBulletBehavior.OnObstacleHitted)와 함께 미니건 고유의 벽 충돌 파티클을 재생합니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            // PlayerBulletBehavior의 OnObstacleHitted가 먼저 호출되어 기본적인 비활성화 및 트윈 중지 처리
            base.OnObstacleHitted();

            // 미니건 벽 충돌 파티클 재생
            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH)?.SetPosition(transform.position); // null 체크 추가

            if (trailRenderer != null)
            {
                trailRenderer.Clear(); // 트레일 렌더러의 경로 지우기
            }
        }
    }
}