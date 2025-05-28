// ==============================================
// 📌 DemoEnemyBehavior.cs
// ✅ 자폭형 데모 적 유닛의 행동 제어 스크립트
// ✅ 공격 시 자폭, 주변 적과 플레이어에게 범위 피해를 입힘
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class DemoEnemyBehavior : BaseEnemyBehavior
    {
        private static readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [Header("Explosion Settings")]
        [Tooltip("폭발 반경")]
        [SerializeField] private float explosionRadius;

        [Tooltip("폭발 반경 시각화 오브젝트")]
        [SerializeField] private GameObject explosionCircle;

        [Tooltip("폭발 기준 지점 (폭탄 본 위치)")]
        [SerializeField] private Transform bombBone;

        [Tooltip("폭탄 오브젝트")]
        [SerializeField] private GameObject bombObj;

        [Tooltip("퓨즈 오브젝트")]
        [SerializeField] private GameObject fuseObj;

        [Space]
        [Tooltip("무기 애니메이션 제어")]
        [SerializeField] private WeaponRigBehavior weaponRigBehavior;

        private TweenCase explosionRadiusScaleCase;
        private bool exploded = false;

        private int explosionParticleHash;
        private int explosionDecalParticleHash;

        /// <summary>
        /// 📌 폭발 이펙트 초기화 및 기본 설정
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            explosionParticleHash = "Bomber Explosion".GetHashCode();
            explosionDecalParticleHash = "Bomber Explosion Decal".GetHashCode();

            CanPursue = true;

            explosionCircle.SetActive(false);
        }

        /// <summary>
        /// 📌 초기화 시 퓨즈 비활성화 및 무기 연동
        /// </summary>
        public override void Init()
        {
            base.Init();
            weaponRigBehavior.enabled = true;
            fuseObj.SetActive(false);
        }

        /// <summary>
        /// 📌 자폭 애니메이션 종료 시 폭발 처리
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                // 이펙트
                ParticlesController.PlayParticle(explosionParticleHash)
                    .SetPosition(bombBone.position.SetY(0.1f))
                    .SetDuration(1f);

                ParticlesController.PlayParticle(explosionDecalParticleHash)
                    .SetRotation(Quaternion.Euler(-90, 0, 0))
                    .SetScale(new Vector3(10f, 10f, 10f))
                    .SetPosition(transform.position)
                    .SetDuration(5f);

                // 폭탄 비활성화
                bombObj.SetActive(false);

                // 플레이어 피해
                if (Vector3.Distance(transform.position, Target.position) <= explosionRadius)
                {
                    characterBehaviour.TakeDamage(GetCurrentDamage());
                }

                // 주변 적 피해
                foreach (var enemy in ActiveRoom.GetAliveEnemies())
                {
                    if (enemy == this) continue;

                    if (Vector3.Distance(transform.position, enemy.transform.position) <= explosionRadius)
                    {
                        Vector3 direction = (enemy.transform.position.SetY(0) - bombObj.transform.position.SetY(0)).normalized;
                        enemy.TakeDamage(GetCurrentDamage(), bombObj.transform.position, direction);
                    }
                }

                explosionCircle.SetActive(false);
                exploded = true;

                AudioController.PlaySound(AudioController.AudioClips.explode);
                OnDeath();
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 📌 체력 UI 위치 갱신
        /// </summary>
        private void Update()
        {
            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        /// <summary>
        /// 📌 퓨즈 점화 (상태 전이 시)
        /// </summary>
        public void LightUpFuse()
        {
            fuseObj.SetActive(true);
        }

        /// <summary>
        /// 📌 자폭 공격 시작
        /// </summary>
        public override void Attack()
        {
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);

            navMeshAgent.speed = 0;
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;

            CanPursue = false;
            CanMove = false;

            explosionCircle.SetActive(true);
            explosionCircle.transform.localScale = new Vector3(0f, 0.2f, 0f);

            explosionRadiusScaleCase = explosionCircle.transform.DOScale(
                new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f),
                1.66f
            ).SetEasing(Ease.Type.QuadOut);
        }

        /// <summary>
        /// 📌 사망 처리 시 폭발 중단 및 리소스 정리
        /// </summary>
        protected override void OnDeath()
        {
            base.OnDeath();

            explosionRadiusScaleCase.KillActive();
            explosionCircle.SetActive(false);
            fuseObj.SetActive(false);

            if (exploded)
                ragdollCase.KillActive();
            else
                weaponRigBehavior.enabled = false;
        }
    }
}
