using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class MiniGunBehavior : BaseGunBehavior
    {
        [SerializeField, Tooltip("미니건 총신 트랜스폼")]
        private Transform barrelTransform;

        [SerializeField, Tooltip("발사 파티클 시스템")]
        private ParticleSystem shootParticleSystem;

        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private float bulletDisableTime;

        [SerializeField] private float fireRotationSpeed;
        [SerializeField] private List<float> bulletStreamAngles;

        private float spread;
        private float attackDelay;
        private DuoFloat bulletSpeed;

        private float nextShootTime;
        private float lastShootTime;

        private Pool bulletPool;
        private Vector3 shootDirection;
        private TweenCase shootTweenCase;

        private static readonly Color critColor = new Color(1f, 0.4f, 0f);

        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon);
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletObj = currentUpgrade.BulletPrefab;

            bulletPool = new Pool(bulletObj, $"Minigun_{bulletObj.name}");
            RecalculateDamage();
        }

        private void OnDestroy()
        {
            if (bulletPool != null)
                PoolManager.DestroyPool(bulletPool);
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            damage = currentUpgrade.Damage;
            attackDelay = 1f / currentUpgrade.FireRate;
            spread = currentUpgrade.Spread;
            bulletSpeed = currentUpgrade.BulletSpeed;
        }

        public override void GunUpdate()
        {
            if (attackDelay > 0.2f)
            {
                AttackButtonBehavior.SetReloadFill(1 - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime));
            }

            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            barrelTransform.Rotate(Vector3.forward * fireRotationSpeed);

            if (nextShootTime >= Time.timeSinceLevelLoad || !characterBehaviour.IsAttackingAllowed)
                return;

            AttackButtonBehavior.SetReloadFill(0);

            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers) &&
                hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(shootDirection, transform.forward.SetY(0f)) < 40f)
                {
                    shootTweenCase.KillActive();
                    shootTweenCase = transform.DOLocalMoveZ(-0.0825f, attackDelay * 0.3f).OnComplete(() =>
                    {
                        shootTweenCase = transform.DOLocalMoveZ(0, attackDelay * 0.6f);
                    });

                    characterBehaviour.SetTargetActive();
                    shootParticleSystem.Play();

                    nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                    lastShootTime = Time.timeSinceLevelLoad;

                    if (bulletStreamAngles.IsNullOrEmpty())
                    {
                        bulletStreamAngles = new List<float> { 0 };
                    }

                    int bulletsNumber = weapon.GetCurrentUpgrade().BulletsPerShot.Random();

                    for (int k = 0; k < bulletsNumber; k++)
                    {
                        for (int i = 0; i < bulletStreamAngles.Count; i++)
                        {
                            var streamAngle = bulletStreamAngles[i];

                            var bulletObj = bulletPool
                                .GetPooledObject()
                                .SetPosition(shootPoint.position)
                                .SetEulerAngles(characterBehaviour.transform.eulerAngles + Vector3.up * (Random.Range(-spread, spread) + streamAngle));

                            PlayerBulletBehavior bullet = bulletObj.GetComponent<PlayerBulletBehavior>();

                            // [치명타 데미지 및 여부 계산]
                            var (damageValue, isCritical) = CalculateFinalDamageWithCrit();
                            float finalDamage = damageValue * characterBehaviour.Stats.BulletDamageMultiplier;

                            bullet.Init(finalDamage, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);

                            // [치명타 텍스트 출력]
                            Color textColor = isCritical ? critColor : Color.white;
                            FloatingTextController.SpawnFloatingText(
                                "Hit",
                                finalDamage.ToString(),
                                characterBehaviour.ClosestEnemyBehaviour.transform.position,
                                Quaternion.identity,
                                1.0f,
                                textColor,
                                isCritical
                            );
                        }
                    }

                    characterBehaviour.OnGunShooted();
                    AudioController.PlaySound(AudioController.AudioClips.shotMinigun);
                }
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        public override void OnGunUnloaded()
        {
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null;
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool?.ReturnToPoolEverything();
        }
    }
}
