// LavaGunBehavior.cs
// 이 스크립트는 용암 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 용암 투사체(LavaBulletBehavior) 발사 로직,
// 목표 추적, 파티클 및 사운드 효과를 구현합니다.
// 플로팅 텍스트 생성 책임은 LavaBulletBehavior로 이전되었습니다.
using UnityEngine;
using Watermelon; // Pool, PoolManager, Tween, ParticlesController, AudioController 등
using Watermelon.LevelSystem; // ActiveRoom 사용을 위해 필요 (현재 코드에서는 직접 사용 안함)

namespace Watermelon.SquadShooter
{
    public class LavaGunBehavior : BaseGunBehavior
    {
        [Header("용암탄 총 전용 설정")]
        [LineSpacer]
        [Tooltip("총기의 그래픽스 부분을 나타내는 트랜스폼입니다 (예: 발사 애니메이션에 사용).")]
        [SerializeField] Transform graphicsTransform;
        [Tooltip("총기 발사 시 재생될 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem shootParticleSystem;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다 (예: 적, 장애물).")]
        [SerializeField] LayerMask targetLayers; // 현재 발사 로직에서 명시적으로 사용되진 않지만, 향후 확장 가능성 있음
        [Tooltip("용암 투사체 폭발 시 피해를 줄 반경입니다.")]
        [SerializeField] float explosionRadius = 2.0f;
        [Tooltip("용암 투사체의 곡선 이동 높이 범위 (최소/최대 값)입니다.")]
        [SerializeField] DuoFloat bulletHeight = new DuoFloat(2.0f, 4.0f);

        // 총기 발사 사이의 지연 시간 (공격 속도에 반비례)입니다.
        private float attackDelay;
        // 용암 투사체의 이동 속도 범위 (최소/최대 값)입니다.
        private DuoFloat bulletSpeed;

        // 다음 발사가 가능한 시간입니다.
        private float nextShootTime;
        // 마지막 발사 시간입니다. (재장전 UI 등에 사용될 수 있습니다)
        private float lastShootTime;

        // 용암 투사체 객체 풀입니다.
        private Pool bulletPool;

        // 총기 그래픽스 이동 애니메이션을 제어하는 TweenCase 객체입니다.
        private TweenCase shootTweenCase;

        // 캐릭터의 적 감지 반경 (발사 가능 사거리로 사용될 수 있습니다).
        private float currentShootingRadius; // Init에서 설정됨

        /// <summary>
        /// 용암 총의 동작을 초기화합니다.
        /// </summary>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon);

            if (weapon == null || weapon.GetCurrentUpgrade() == null || weapon.GetCurrentUpgrade().BulletPrefab == null)
            {
                Debug.LogError($"[LavaGunBehavior] Init: WeaponData 또는 BulletPrefab이 유효하지 않습니다! 무기명: {(weapon != null ? weapon.WeaponName : "N/A")}");
                this.enabled = false; // 핵심 데이터 없으면 비활성화
                return;
            }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletObj = currentUpgrade.BulletPrefab;

            if (bulletObj.GetComponent<LavaBulletBehavior>() == null)
            {
                Debug.LogError($"[LavaGunBehavior] Init: BulletPrefab '{bulletObj.name}'에 LavaBulletBehavior 컴포넌트가 없습니다!");
                this.enabled = false;
                return;
            }

            bulletPool = new Pool(bulletObj, $"{bulletObj.name}_LavaPool_{this.GetInstanceID()}");

            if (this.characterBehaviour != null && this.characterBehaviour.EnemyDetector != null)
            {
                currentShootingRadius = this.characterBehaviour.EnemyDetector.DetectorRadius;
            }
            else
            {
                currentShootingRadius = 15f; // 기본값 또는 경고 처리 (캐릭터가 없을 수 있는 상황 고려)
                if(this.characterBehaviour != null) Debug.LogWarning("[LavaGunBehavior] Init: EnemyDetector를 찾을 수 없어 shootingRadius 기본값을 사용합니다.");
            }

            RecalculateDamage();
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출됩니다. 풀 및 트윈을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null;
            }
            shootTweenCase.KillActive();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            if (weapon == null) { Debug.LogError("[LavaGunBehavior] RecalculateDamage: weapon이 null입니다."); return; }
            WeaponUpgrade upgrade = weapon.GetCurrentUpgrade();
            if (upgrade == null) { Debug.LogError($"[LavaGunBehavior] RecalculateDamage: {weapon.name}의 currentUpgrade가 null입니다."); return; }

            // this.damage (BaseGunBehavior의 DuoInt)는 LavaBulletBehavior.Init의 explosionDamageRange로 전달됨
            this.damage = upgrade.Damage;
            attackDelay = 1f / upgrade.FireRate;
            bulletSpeed = upgrade.BulletSpeed;
            // explosionRadius와 bulletHeight는 인스펙터에서 설정된 값을 우선 사용.
            // 필요하다면 여기서 WeaponUpgrade의 특정 필드로부터 값을 읽어와 덮어쓸 수 있습니다.
            // 예: if(upgrade.HasCustomExplosionRadius) this.explosionRadius = upgrade.CustomExplosionRadius;
        }

        public override void GunUpdate()
        {
            float reloadProgress = (nextShootTime > lastShootTime && (nextShootTime - lastShootTime) > 0.01f) ? (1f - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime)) : 1f;
            AttackButtonBehavior.SetReloadFill(Mathf.Clamp01(reloadProgress));

            if (characterBehaviour == null || !characterBehaviour.IsCloseEnemyFound)
                return;

            if (nextShootTime > Time.timeSinceLevelLoad || !characterBehaviour.IsAttackingAllowed)
                return;
            
            if (characterBehaviour.ClosestEnemyBehaviour == null)
            {
                characterBehaviour.SetTargetUnreachable();
                return;
            }

            AttackButtonBehavior.SetReloadFill(0);

            Vector3 targetDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
            
            // 레이캐스트 발사 지점을 총구(shootPoint)로 변경하고, 방향은 targetDirection 사용
            if (Physics.Raycast(shootPoint.position, targetDirection.normalized, out RaycastHit hitInfo, currentShootingRadius + 5f, targetLayers)) // 사거리 고려
            {
                // 레이캐스트가 적에게 명중했는지, 또는 다른 장애물에 막혔는지에 따라 분기할 수 있으나,
                // 라바건은 곡사로 발사되므로, 적이 시야각 내에 있고 사거리 내에 있다면 발사하는 것으로 단순화 가능.
                // 여기서는 가장 가까운 적이 감지되었다면 발사각 체크 후 발사.
                if (Vector3.Angle(targetDirection.normalized, transform.forward.normalized) < 45f) // 발사각 여유롭게 설정
                {
                    characterBehaviour.SetTargetActive();

                    shootTweenCase.KillActive();
                    if (graphicsTransform != null)
                    {
                        shootTweenCase = graphicsTransform.DOLocalMoveZ(-0.15f, attackDelay * 0.1f).OnComplete(delegate
                        {
                            if (graphicsTransform != null)
                            {
                                shootTweenCase = graphicsTransform.DOLocalMoveZ(0, attackDelay * 0.15f);
                            }
                        });
                    }

                    if (shootParticleSystem != null) shootParticleSystem.Play();
                    nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                    lastShootTime = Time.timeSinceLevelLoad;

                    WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
                    int bulletsToShoot = currentUpgrade.BulletsPerShot.Random(); // 여러 발 발사 가능성

                    for (int i = 0; i < bulletsToShoot; i++)
                    {
                        if (bulletPool == null) { Debug.LogError("[LavaGunBehavior] GunUpdate: bulletPool이 null입니다!"); continue; }
                        GameObject bulletGO = bulletPool.GetPooledObject();
                        if (bulletGO == null) { Debug.LogError("[LavaGunBehavior] GunUpdate: bulletPool에서 null 오브젝트를 반환받았습니다."); continue; }

                        // 총알의 초기 위치와 회전 설정
                        bulletGO.transform.SetPositionAndRotation(shootPoint.position, shootPoint.rotation);
                        
                        LavaBulletBehavior bullet = bulletGO.GetComponent<LavaBulletBehavior>();
                        if (bullet == null)
                        {
                            Debug.LogError($"[LavaGunBehavior] 총알 프리팹 '{bulletGO.name}'에 LavaBulletBehavior 컴포넌트가 없습니다!");
                            bulletGO.SetActive(false);
                            continue;
                        }

                        // 발사 시점의 치명타 여부 계산 (LavaBulletBehavior.Init의 gunShotWasCritical 인자로 사용)
                        var (_, isGunShotCrit) = CalculateFinalDamageWithCrit();
                        
                        // LavaBulletBehavior의 Init 메서드 호출
                        bullet.Init(
                            this.damage,                    // DuoInt explosionDamage (this.damage는 RecalculateDamage에서 WeaponUpgrade.Damage로 설정됨)
                            bulletSpeed.Random(),           // float travelSpeed
                            characterBehaviour.ClosestEnemyBehaviour, // BaseEnemyBehavior initialTargetForProjectile
                            -1f,                            // float projectileAutoDisableTime (-1f는 자동 비활성화 없음, 폭발 시 자체 처리)
                            false,                          // bool projectileDisableOnHit (용암탄은 보통 false)
                            isGunShotCrit,                  // bool gunShotWasCritical
                            this.characterBehaviour,        // CharacterBehaviour projectileOwner
                            currentShootingRadius,          // float lavaShootingRadius
                            this.bulletHeight,              // DuoFloat lavaBulletHeight (클래스 필드)
                            this.explosionRadius            // float actualExplosionRadius (클래스 필드)
                        );

                        // 플로팅 텍스트 생성은 LavaBulletBehavior.OnEnemyHitted에서 처리하므로 여기서는 호출하지 않음.
                    }

                    characterBehaviour.OnGunShooted();

                    VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
                    if(gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                    AudioController.PlaySound(AudioController.AudioClips.shotLavagun, 0.8f);
                }
                // else: 시야각 벗어남
            }
            else // 레이캐스트 미명중 또는 장애물에 막힘 (단, 라바건은 곡사이므로 이 체크가 항상 유효하진 않을 수 있음)
            {
                // 만약 레이캐스트에 의존하지 않고 무조건 발사하려면 이 else 블록 및 if(Physics.Raycast) 조건 자체를 제거/수정
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
            if (characterGraphics == null || characterGraphics.RocketHolderTransform == null) // 라바건은 로켓 홀더 사용 가정
            {
                Debug.LogError($"[LavaGunBehavior] PlaceGun: characterGraphics 또는 RocketHolderTransform이 null입니다!");
                return;
            }
            transform.SetParent(characterGraphics.RocketHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            // 라바건은 일반적으로 재장전 개념이 없을 수 있으나, 풀 반환 로직은 유지
            bulletPool?.ReturnToPoolEverything();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || characterBehaviour == null || shootPoint == null) return;

            if (characterBehaviour.ClosestEnemyBehaviour != null)
            {
                Color defCol = Gizmos.color;
                Gizmos.color = Color.red; // 라바 색상에 맞춰 변경 가능
                Vector3 targetPos = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y);
                Vector3 currentShootDirectionGizmo = targetPos - shootPoint.position;
                if (currentShootDirectionGizmo.sqrMagnitude > 0.01f)
                {
                    Gizmos.DrawRay(shootPoint.position, currentShootDirectionGizmo.normalized * currentShootingRadius); // 사거리까지 표시
                }
                Gizmos.color = defCol;
            }
        }
#endif
    }
}