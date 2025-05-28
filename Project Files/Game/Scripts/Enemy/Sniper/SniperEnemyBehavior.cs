// ==============================================
// 📌 SniperEnemyBehavior.cs
// ✅ 스나이퍼 적 유닛의 조준, 레이저, 발사 동작 제어
// ✅ 레이저 시각화 및 Raycast 기반 경로 계산 포함
// ==============================================

using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class SniperEnemyBehavior : BaseEnemyBehavior
    {
        [Header("Transforms")]
        [Tooltip("총알이 발사되는 위치")]
        [SerializeField] private Transform weaponExit;
        public Transform WeaponExit => weaponExit;

        [Tooltip("레이저 메쉬 위치")]
        [SerializeField] private Transform laserTransform;

        [Tooltip("레이저 렌더러")]
        [SerializeField] private MeshRenderer laserRenderer;

        [Tooltip("노란 조준 상태의 색상")]
        [SerializeField] private Color alertColor;

        [Tooltip("빨간 조준 상태의 색상")]
        [SerializeField] private Color redColor;

        [Header("Fighting")]
        [Tooltip("총알 프리팹")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("총알 속도")]
        [SerializeField] private float bulletSpeed;

        [Tooltip("노란색 조준 시간")]
        [SerializeField] private float yellowAimingDuration;

        [Tooltip("빨간색 조준 시간")]
        [SerializeField] private float redAimingDuration;

        [Tooltip("빨간색 레이저 고정 여부")]
        [SerializeField] private bool isRedStatic;

        [Header("FX")]
        [Tooltip("총 발사 파티클")]
        [SerializeField] private ParticleSystem gunFireParticle;

        public float YellowAimingDuration => yellowAimingDuration;
        public float RedAimingDuration => redAimingDuration;
        public bool IsRedStatic => isRedStatic;
        public Transform LaserTransform => laserTransform;

        protected override void Awake()
        {
            base.Awake();
            laserRenderer.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    var bullet = Instantiate(bulletPrefab)
                        .SetPosition(weaponExit.position)
                        .SetEulerAngles(weaponExit.eulerAngles)
                        .GetComponent<EnemyBulletBehavior>();

                    bullet.transform.forward = transform.forward;
                    bullet.Init(GetCurrentDamage(), bulletSpeed, 200);

                    gunFireParticle.Play();
                    AudioController.PlaySound(AudioController.AudioClips.enemySniperShoot);
                    break;

                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;
            }
        }

        public void EnableLaser()
        {
            laserRenderer.gameObject.SetActive(true);
            laserRenderer.material.SetColor("_BaseColor", alertColor);
        }

        public void MakeLaserRed()
        {
            laserRenderer.material.SetColor("_BaseColor", redColor);
        }

        public void DisableLaser()
        {
            laserRenderer.gameObject.SetActive(false);
        }

        public void AimLaser()
        {
            Vector3 startPos = weaponExit.position;
            Vector3 direction = transform.forward;

            if (Physics.Raycast(startPos - direction * 2f, direction, out var hit, 150f, LayerMask.GetMask("Obstacle")))
            {
                Vector3 mid = (startPos + hit.point) * 0.5f;
                laserTransform.position = mid;
                laserTransform.localScale = new Vector3(0.05f, 0.05f, Vector3.Distance(startPos, hit.point));
            }
            else
            {
                Vector3 end = startPos + direction * 150f;
                Vector3 mid = (startPos + end) * 0.5f;
                laserTransform.position = mid;
                laserTransform.localScale = new Vector3(0.05f, 0.05f, Vector3.Distance(startPos, end));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StateMachine.StopMachine();
        }
    }
}
