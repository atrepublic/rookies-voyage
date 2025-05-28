// ==============================================
// 📌 BossSniperBehavior.cs
// ✅ 보스 스나이퍼 유닛의 공격, 조준, 레이저 연출 등을 제어하는 스크립트
// ✅ BaseEnemyBehavior를 상속하며, OnAnimatorCallback 기반 공격 실행
// ✅ 다단 반사되는 레이저 경로 계산 및 시각화 포함
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;
using Watermelon.Enemy.BossSniper;

namespace Watermelon.SquadShooter
{
    public class BossSniperBehavior : BaseEnemyBehavior
    {
        [Header("Bullet")]
        [Tooltip("발사할 총알 프리팹")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("총알 속도")]
        [SerializeField] private float bulletSpeed;

        [Tooltip("총알 발사 위치")]
        [SerializeField] private Transform shootingPoint;

        [Tooltip("총격 이펙트")]
        [SerializeField] private ParticleSystem gunFireParticle;

        [Tooltip("충돌 체크용 레이어 마스크")]
        [SerializeField] private LayerMask collisionLayer;

        [Header("Laser")]
        [Tooltip("레이저 연출용 메쉬 렌더러 목록")]
        [SerializeField] private List<MeshRenderer> laserRenderers;

        private List<BossSniperLaserLine> lasers;

        [Tooltip("노란색 조준 시간")]
        [SerializeField] private float yellowAimingDuration;
        public float YellowLaserAinimgDuration => yellowAimingDuration;

        [Tooltip("빨간색 조준 시간")]
        [SerializeField] private float redAimingDuration;
        public float RedLaserAimingDuration => redAimingDuration;

        [Tooltip("빨간 조준 중에도 조준 위치를 계속 변경할 수 있는가")]
        [SerializeField] private bool canAimDuringRedLaserStage;
        public bool CanAimDuringRedLaserStage => canAimDuringRedLaserStage;

        [Tooltip("레이저 굵기")]
        [SerializeField] private float laserThickness;

        [Tooltip("노란 레이저 색상")]
        [SerializeField] private Color yellowLaserColor;

        [Tooltip("빨간 레이저 색상")]
        [SerializeField] private Color redLaserColor;

        [Header("Other")]
        [Tooltip("보스 기운 효과 파티클")]
        [SerializeField] private GameObject auraParticle;

        /// <summary>
        /// 📌 레이저 객체 초기화
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            lasers = new List<BossSniperLaserLine>();
            foreach (var renderer in laserRenderers)
            {
                var laser = new BossSniperLaserLine();
                laser.Init(renderer);
                lasers.Add(laser);
            }
        }

        /// <summary>
        /// 📌 보스 공격 트리거 실행
        /// </summary>
        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        /// <summary>
        /// 📌 상태 업데이트 시 체력 UI 위치 갱신
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            healthbarBehaviour.FollowUpdate();
        }

        /// <summary>
        /// 📌 보스 활성화 시 아우라 이펙트 켜기
        /// </summary>
        public override void Init()
        {
            base.Init();
            auraParticle.SetActive(true);
        }

        /// <summary>
        /// 📌 사망 시 아우라 이펙트 끄기
        /// </summary>
        protected override void OnDeath()
        {
            base.OnDeath();
            auraParticle.SetActive(false);
        }

        /// <summary>
        /// 📌 애니메이션 이벤트 콜백 처리
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    var bullet = Instantiate(bulletPrefab)
                        .SetPosition(shootingPoint.position)
                        .SetEulerAngles(shootingPoint.eulerAngles)
                        .GetComponent<BossSniperBulletBehavior>();

                    bullet.transform.forward = transform.forward;
                    bullet.InitBullet(GetCurrentDamage(), bulletSpeed, 1000, lasetHitPoints);

                    gunFireParticle.Play();
                    AudioController.PlaySound(AudioController.AudioClips.enemySniperShoot);
                    break;

                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;
            }
        }

        // ==============================
        // 📌 조준 레이저 관련 기능들
        // ==============================

        private List<Vector3> lasetHitPoints;

        /// <summary>
        /// 📌 레이저를 노란색으로 설정
        /// </summary>
        public void MakeLaserYellow() => lasers.ForEach(laser => laser.SetColor(yellowLaserColor));

        /// <summary>
        /// 📌 레이저를 빨간색으로 설정
        /// </summary>
        public void MakeLaserRed() => lasers.ForEach(laser => laser.SetColor(redLaserColor));

        /// <summary>
        /// 📌 레이저 렌더러 활성화
        /// </summary>
        public void EnableLaser() => lasers.ForEach(laser => laser.SetActive(true));

        /// <summary>
        /// 📌 레이저 렌더러 비활성화
        /// </summary>
        public void DisableLaser() => lasers.ForEach(laser => laser.SetActive(false));

        /// <summary>
        /// 📌 레이저 반사 경로 계산 및 시각화
        /// </summary>
        public void AimLaser()
        {
            var laserStartPos = shootingPoint.position;
            var laserDirection = Rotation * Vector3.forward;

            lasetHitPoints = new List<Vector3>();

            for (int i = 0; i < lasers.Count; i++)
            {
                var laserObject = lasers[i];
                laserObject.SetActive(true);

                bool endCalculation = false;

                if (Physics.Raycast(laserStartPos, laserDirection, out var hitInfo, 300f, collisionLayer))
                {
                    float distance = Vector3.Distance(hitInfo.point, laserStartPos);

                    laserObject.Init(laserStartPos, hitInfo.point, new Vector3(laserThickness, laserThickness, distance));
                    laserStartPos = hitInfo.point - laserDirection * 0.2f;

                    var prevDir = laserDirection;
                    laserDirection = Vector3.Reflect(laserDirection, -hitInfo.normal);

                    var dot = Vector3.Dot(prevDir, laserDirection);
                    if (Mathf.Abs(dot) > 0.96f && i > 0)
                    {
                        endCalculation = true;
                        laserObject.SetActive(false);
                    }
                    else
                    {
                        lasetHitPoints.Add(laserStartPos);
                        if (hitInfo.collider.gameObject == Target.gameObject)
                            endCalculation = true;
                    }
                }
                else
                {
                    var endPos = laserStartPos + laserDirection * 300f;
                    lasetHitPoints.Add(endPos);

                    laserObject.Init(laserStartPos, endPos, new Vector3(laserThickness, laserThickness, 300f));
                    endCalculation = true;
                }

                if (endCalculation)
                {
                    for (int j = i + 1; j < lasers.Count; j++)
                        lasers[j].SetActive(false);
                    break;
                }
            }
        }
    }
}
