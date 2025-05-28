// ==============================================
// 📌 CowboyEnemyBehavior.cs
// ✅ 카우보이 스타일 적 캐릭터의 전투 행동을 정의한 스크립트
// ✅ BaseEnemyBehavior를 상속하며, 애니메이션 콜백 기반 양손 사격 구현
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class CowboyEnemyBehavior : BaseEnemyBehavior
    {
        [Header("Bullet")]
        [Tooltip("총알 프리팹")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("총알 속도")]
        [SerializeField] private float bulletSpeed;

        [Header("Left side")]
        [Tooltip("왼손 총알 발사 위치")]
        [SerializeField] private Transform leftShootPoint;

        [Tooltip("왼손 총격 이펙트")]
        [SerializeField] private ParticleSystem leftGunFireParticle;

        [Header("Right side")]
        [Tooltip("오른손 총알 발사 위치")]
        [SerializeField] private Transform rightShootPoint;

        [Tooltip("오른손 총격 이펙트")]
        [SerializeField] private ParticleSystem rightGunFireParticle;

        /// <summary>
        /// 📌 물리 프레임마다 체력바 UI를 따라다니도록 업데이트
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            healthbarBehaviour.FollowUpdate();
        }

        /// <summary>
        /// 📌 애니메이션 기반 공격 시작 (Shoot 트리거)
        /// </summary>
        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        /// <summary>
        /// 📌 애니메이션 이벤트로 사격 실행
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            EnemyBulletBehavior bullet;

            switch (enemyCallbackType)
            {
                // 왼쪽 총 발사
                case EnemyCallbackType.LeftHit:
                    bullet = Instantiate(bulletPrefab)
                        .SetPosition(leftShootPoint.position)
                        .SetEulerAngles(leftShootPoint.eulerAngles)
                        .GetComponent<EnemyBulletBehavior>();

                    bullet.transform.LookAt(target.position.SetY(leftShootPoint.position.y));
                    bullet.Init(GetCurrentDamage(), bulletSpeed, 200);

                    leftGunFireParticle.Play();
                    AudioController.PlaySound(AudioController.AudioClips.enemyShot);
                    break;

                // 오른쪽 총 발사
                case EnemyCallbackType.RightHit:
                    bullet = Instantiate(bulletPrefab)
                        .SetPosition(rightShootPoint.position)
                        .SetEulerAngles(rightShootPoint.eulerAngles)
                        .GetComponent<EnemyBulletBehavior>();

                    bullet.transform.LookAt(target.position.SetY(rightShootPoint.position.y));
                    bullet.Init(GetCurrentDamage(), bulletSpeed, 200);

                    rightGunFireParticle.Play();
                    AudioController.PlaySound(AudioController.AudioClips.enemyShot);
                    break;

                // 공격 종료 콜백
                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;
            }
        }
    }
}
