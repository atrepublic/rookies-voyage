// ==============================================
// 📌 ShotgunerEnemyBehavior.cs
// ✅ 샷건 적 유닛의 공격 및 애니메이션 이벤트 처리 스크립트
// ✅ 탄환 퍼짐, 연발 사격, 파티클 재생 포함
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunerEnemyBehavior : BaseEnemyBehavior
    {
        [Header("Shotgun Settings")]
        [Tooltip("총알이 나가는 지점")]
        [SerializeField] private Transform shootPoint;

        [Tooltip("사격 시 출력되는 이펙트")]
        [SerializeField] private ParticleSystem shootParticle;

        [Tooltip("재장전 시 출력되는 이펙트")]
        [SerializeField] private ParticleSystem reloadParticle;

        [Tooltip("발사할 총알 프리팹")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("총알 속도")]
        [SerializeField] private float bulletSpeed;

        [Tooltip("총알 퍼짐 각도")]
        [SerializeField] private float spreadAngle;

        [Tooltip("발사할 총알 개수 (랜덤 범위)")]
        [SerializeField] private DuoInt bulletsCount;

        /// <summary>
        /// 📌 공격 트리거 애니메이션 시작
        /// </summary>
        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        /// <summary>
        /// 📌 애니메이션 이벤트 콜백 처리 (발사/종료/재장전)
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    int count = bulletsCount.Random();
                    for (int i = 0; i < count; i++)
                    {
                        var bullet = Instantiate(bulletPrefab)
                            .SetPosition(shootPoint.position)
                            .SetEulerAngles(shootPoint.eulerAngles)
                            .GetComponent<EnemyBulletBehavior>();

                        bullet.transform.LookAt(target.position.SetY(shootPoint.position.y));
                        bullet.transform.Rotate(new Vector3(
                            0f,
                            i == 0 ? 0f : Random.Range(spreadAngle * 0.25f, spreadAngle * 0.5f) * (Random.Range(0, 2) == 0 ? -1f : 1f),
                            0f
                        ));

                        bullet.Init(GetCurrentDamage(), bulletSpeed, Stats.AttackDistance + 10f);
                    }

                    shootParticle.Play();
                    AudioController.PlaySound(AudioController.AudioClips.shotShotgun);
                    break;

                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;

                case EnemyCallbackType.ReloadFinished:
                    reloadParticle.Play();
                    break;
            }
        }

        /// <summary>
        /// 📌 체력바 위치 갱신
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            healthbarBehaviour.FollowUpdate();
        }
    }
}
