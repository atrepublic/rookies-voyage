// ShotgunBulletBehavior.cs
// 이 스크립트는 샷건 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 명중 시 특정 파티클, 트레일 관리 및 초기 크기 애니메이션을 추가합니다.
// PlayerBulletBehavior의 변경된 Init 시그니처를 따르도록 수정되었습니다.
using UnityEngine;
using Watermelon; // ParticlesController, Tween 등 Watermelon 프레임워크 사용

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 샷건 투사체의 특정 동작을 정의합니다.
    /// </summary>
    public class ShotgunBulletBehavior : PlayerBulletBehavior
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 수정된 부분 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 적 명중 시 재생할 파티클 시스템의 이름 해시 값입니다.
        // ParticlesController.GetHash() 대신 string.GetHashCode() 사용
        private static readonly int PARTICLE_HIT_HASH = "Shotgun Hit".GetHashCode();
        // 벽 명중 시 재생할 파티클 시스템의 이름 해시 값입니다.
        private static readonly int PARTICLE_WALL_HIT_HASH = "Shotgun Wall Hit".GetHashCode();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        [Header("샷건 투사체 전용 설정")]
        [Tooltip("투사체 이동 경로를 시각적으로 표시하는 트레일 렌더러 컴포넌트입니다.")]
        [SerializeField] TrailRenderer trailRenderer;

        // 주석: graphicsTransform 필드는 현재 코드에서 직접 사용되지 않으므로 제거하거나,
        //       향후 사용 계획이 있다면 유지할 수 있습니다. 여기서는 일단 남겨둡니다.
        [Tooltip("투사체의 그래픽을 나타내는 트랜스폼입니다 (크기 애니메이션 등에 사용될 수 있습니다).")]
        [SerializeField] Transform graphicsTransform; 

        /// <summary>
        /// 샷건 투사체를 초기화합니다.
        /// 기본 투사체 정보 설정 후 트레일 렌더러를 초기화하고 크기 애니메이션을 시작합니다.
        /// </summary>
        /// <param name="baseDamageFromGun">총기에서 계산된 초기 데미지 값</param>
        /// <param name="bulletSpeed">투사체의 이동 속도</param>
        /// <param name="initialTargetForProjectile">투사체의 초기 목표 적 (샷건은 보통 사용 안 함)</param>
        /// <param name="projectileAutoDisableTime">투사체가 자동으로 비활성화될 시간</param>
        /// <param name="projectileDisableOnHit">직접 충돌 시 투사체를 비활성화할지 여부 (샷건은 보통 true)</param>
        /// <param name="gunShotWasCritical">총구 발사 시점의 치명타 여부</param>
        /// <param name="projectileOwner">이 투사체를 발사한 캐릭터의 CharacterBehaviour</param>
        public override void Init(float baseDamageFromGun, float bulletSpeed, BaseEnemyBehavior initialTargetForProjectile, float projectileAutoDisableTime, bool projectileDisableOnHit, bool gunShotWasCritical, CharacterBehaviour projectileOwner)
        {
            // PlayerBulletBehavior의 Init 호출 (변경된 시그니처에 맞게 모든 인자 전달)
            base.Init(baseDamageFromGun, bulletSpeed, initialTargetForProjectile, projectileAutoDisableTime, projectileDisableOnHit, gunShotWasCritical, projectileOwner);

            if (trailRenderer == null)
            {
                Debug.LogWarning($"[ShotgunBulletBehavior] ({this.gameObject.name}): TrailRenderer가 할당되지 않았습니다.");
            }
            else
            {
                trailRenderer.Clear(); // 재사용 시 이전 트레일 효과 제거
            }

            // 투사체의 초기 스케일을 작게 설정하고, 짧은 시간 동안 원래 크기로 커지는 애니메이션 실행 (DOTween 사용 가정)
            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn); // DOTween 확장 메서드
        }

        /// <summary>
        /// 적에게 명중했을 때 호출됩니다. (PlayerBulletBehavior.OnTriggerEnter 내부에서 호출됨)
        /// 샷건 고유의 명중 파티클을 재생하고 트레일을 정리합니다.
        /// 플로팅 텍스트는 PlayerBulletBehavior.OnTriggerEnter에서 이미 생성됩니다.
        /// </summary>
        /// <param name="enemyHitByThisBullet">이번에 명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior enemyHitByThisBullet)
        {
            // 샷건 명중 파티클 재생
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH)?.SetPosition(transform.position); // null 체크 추가

            if (trailRenderer != null)
            {
                trailRenderer.Clear(); // 트레일 렌더러의 경로 지우기
            }
            // 데미지 텍스트 생성 로직은 PlayerBulletBehavior.OnTriggerEnter로 이전되었으므로 여기서는 호출하지 않습니다.
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다.
        /// 기본 장애물 충돌 처리(PlayerBulletBehavior.OnObstacleHitted)와 함께 샷건 고유의 벽 충돌 파티클을 재생합니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            // PlayerBulletBehavior의 OnObstacleHitted가 먼저 호출되어 기본적인 비활성화 및 트윈 중지 처리
            base.OnObstacleHitted();

            // 샷건 벽 충돌 파티클 재생
            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH)?.SetPosition(transform.position); // null 체크 추가

            if (trailRenderer != null)
            {
                trailRenderer.Clear(); // 트레일 렌더러의 경로 지우기
            }
        }
    }
}