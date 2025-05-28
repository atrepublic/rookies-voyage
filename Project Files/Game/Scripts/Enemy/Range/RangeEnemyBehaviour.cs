// ==============================================
// 📌 RangeEnemyBehaviour.cs
// ✅ 원거리 적 유닛의 사격, 재장전, 이펙트 등을 처리하는 스크립트
// ✅ 애니메이션 이벤트 기반 공격 및 상태 처리 포함
// ==============================================

using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class RangeEnemyBehaviour : BaseEnemyBehavior
    {
        [Header("Fighting")]
        [Tooltip("발사할 총알 프리팹")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("총알 속도")]
        [SerializeField] private float bulletSpeed;

        [Header("Weapon")]
        [Tooltip("총알 발사 위치 트랜스폼")]
        [SerializeField] private Transform shootPointTransform;

        [Space]
        [Tooltip("총격 이펙트")]
        [SerializeField] private ParticleSystem gunFireParticle;

        [Space]
        [Tooltip("재장전이 가능한지 여부")]
        [SerializeField] private bool canReload;
        public bool CanReload => canReload;

        /// <summary>
        /// 📌 공격 트리거 실행 (Shoot 애니메이션 시작)
        /// </summary>
        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        /// <summary>
        /// 📌 체력바 위치 갱신
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        /// <summary>
        /// 📌 애니메이션 이벤트 콜백 처리 (사격, 재장전 등)
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    var bullet = Instantiate(bulletPrefab)
                        .SetPosition(shootPointTransform.position)
                        .SetEulerAngles(shootPointTransform.eulerAngles)
                        .GetComponent<EnemyBulletBehavior>();

                    bullet.transform.forward = transform.forward.SetY(0).normalized;
                    bullet.Init(GetCurrentDamage(), bulletSpeed, 200);

                    gunFireParticle.Play();
                    AudioController.PlaySound(AudioController.AudioClips.enemyShot);
                    break;

                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;

                case EnemyCallbackType.ReloadFinished:
                    InvokeOnReloadFinished();
                    break;
            }
        }
    }
}
