// 이 스크립트는 샷건 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 산탄 형태로 투사체 발사, 파티클, 사운드 및 에디터 디버그 기능을 구현합니다.
using UnityEngine;
using Watermelon; // Watermelon 네임스페이스의 다른 기능(예: Pool, PoolManager, Tween, ParticlesController, AudioController)을 사용하기 위해 필요합니다.

namespace Watermelon.SquadShooter
{
    // BaseGunBehavior를 상속받아 기본 총기 기능을 활용합니다.
    public class ShotgunBehavior : BaseGunBehavior
    {
        [LineSpacer] // 인스펙터에 라인 구분자를 추가하는 사용자 정의 속성일 수 있습니다.
        [Tooltip("총기 발사 시 재생될 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem shootParticleSystem;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다 (예: 적, 장애물).")]
        [SerializeField] LayerMask targetLayers;
        [Tooltip("투사체가 자동으로 비활성화될 시간입니다.")]
        [SerializeField] float bulletDisableTime;

        // 총기 발사 사이의 지연 시간 (공격 속도에 반비례)입니다.
        private float attackDelay;
        // 투사체의 이동 속도 범위 (최소/최대 값)입니다.
        private DuoFloat bulletSpeed; // DuoFloat는 두 개의 float 값을 저장하는 사용자 정의 구조체일 수 있습니다.
        // 투사체 산탄의 최대 각도입니다.
        private float bulletSpreadAngle;

        // 다음 발사가 가능한 시간입니다.
        private float nextShootTime;
        // 마지막 발사 시간입니다. (재장전 UI 등에 사용될 수 있습니다)
        private float lastShootTime;

        // 샷건 투사체 객체 풀입니다.
        private Pool bulletPool;

        // 총기 그래픽스 이동 애니메이션을 제어하는 TweenCase 객체입니다.
        private TweenCase shootTweenCase;
        // 현재 적을 향하는 발사 방향 벡터입니다.
        private Vector3 shootDirection;
        
        // 추가: 치명타 텍스트 색상
        private static readonly Color critColor = new Color(1f, 0.4f, 0f);

        /// <summary>
        /// 샷건 총의 동작을 초기화합니다.
        /// 기본 총기 정보 설정 후 투사체 풀을 생성하고 데미지를 계산합니다.
        /// </summary>
        /// <param name="characterBehaviour">총기를 장착할 캐릭터 행동 컴포넌트</param>
        /// <param name="weapon">이 총기의 무기 데이터</param>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon);
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletObj = currentUpgrade.BulletPrefab;
            bulletPool = new Pool(bulletObj, bulletObj.name);
            RecalculateDamage();
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// 투사체 객체 풀을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (bulletPool != null)
                PoolManager.DestroyPool(bulletPool);
        }

        /// <summary>
        /// 레벨이 로드될 때 호출될 수 있는 함수입니다.
        /// 데미지 및 관련 스탯을 다시 계산합니다.
        /// </summary>
        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        /// <summary>
        /// 현재 무기 강화 상태에 따라 총기의 데미지 및 관련 스탯(퍼짐, 공격 속도, 투사체 속도)을 다시 계산합니다.
        /// </summary>
        public override void RecalculateDamage()
        {
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            damage = currentUpgrade.Damage;
            bulletSpreadAngle = currentUpgrade.Spread;
            attackDelay = 1f / currentUpgrade.FireRate;
            bulletSpeed = currentUpgrade.BulletSpeed;
        }

        /// <summary>
        /// 매 프레임 샷건의 동작을 업데이트합니다.
        /// 재장전 UI 업데이트, 적 감지 확인, 발사 로직 등을 처리합니다.
        /// </summary>
        public override void GunUpdate()
        {
            AttackButtonBehavior.SetReloadFill(1 - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime));

            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            if (nextShootTime >= Time.timeSinceLevelLoad) return;
            if (!characterBehaviour.IsAttackingAllowed) return;

            AttackButtonBehavior.SetReloadFill(0);

            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                    {
                        characterBehaviour.SetTargetActive();
                        shootTweenCase.KillActive();
                        shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate
                        {
                            shootTweenCase = transform.DOLocalMoveZ(0, 0.15f);
                        });

                        shootParticleSystem.Play();
                        nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                        lastShootTime = Time.timeSinceLevelLoad;

                        int bulletsNumber = weapon.GetCurrentUpgrade().BulletsPerShot.Random();
                        Debug.Log($"[ShotgunBehavior] 발사될 총알 수 (bulletsNumber): {bulletsNumber}"); // <--- 여기에 Debug.Log 추가!

                        for (int i = 0; i < bulletsNumber; i++)
                        {
                            PlayerBulletBehavior bullet = bulletPool.GetPooledObject().SetPosition(shootPoint.position).SetEulerAngles(characterBehaviour.transform.eulerAngles).GetComponent<PlayerBulletBehavior>();

                            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 치명타 및 플로팅 텍스트 로직 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                            var (damageValue, isCritical) = CalculateFinalDamageWithCrit();
                            float finalDamage = damageValue * characterBehaviour.Stats.BulletDamageMultiplier;

                            bullet.Init(finalDamage, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);

                            Color textColor = isCritical ? critColor : Color.white;
                            FloatingTextController.SpawnFloatingText(
                                "Hit", // MiniGunBehavior에서 사용하는 이름과 동일하게 (첨부 파일 기준)
                                finalDamage.ToString(),
                                characterBehaviour.ClosestEnemyBehaviour.transform.position,
                                Quaternion.identity,
                                1.0f,
                                textColor,
                                isCritical
                            );
                            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로직 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

                            bullet.transform.Rotate(new Vector3(0f, i == 0 ? 0f : Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f), 0f));
                        }

                        characterBehaviour.OnGunShooted();
                        VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
                        gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
                        AudioController.PlaySound(AudioController.AudioClips.shotShotgun);
                    }
                }
                else
                {
                    characterBehaviour.SetTargetUnreachable();
                }
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        /// <summary>
        /// 에디터에서 기즈모를 그릴 때 호출됩니다.
        /// 발사 방향을 시각적으로 표시합니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (characterBehaviour == null)
                return;

            if (characterBehaviour.ClosestEnemyBehaviour == null)
                return;

            Color defCol = Gizmos.color;
            Gizmos.color = Color.red;
            Vector3 shootDirectionGizmo = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
            Gizmos.DrawLine(shootPoint.position - shootDirectionGizmo.normalized * 10f, characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y));
            Gizmos.color = defCol;
        }

        /// <summary>
        /// 총기가 해제될 때 호출됩니다.
        /// 투사체 객체 풀을 파괴합니다.
        /// </summary>
        public override void OnGunUnloaded()
        {
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null;
            }
        }

        /// <summary>
        /// 캐릭터 그래픽스의 샷건 홀더 트랜스폼에 총기를 장착시킵니다.
        /// </summary>
        /// <param name="characterGraphics">총기를 장착할 캐릭터 그래픽스 컴포넌트</param>
        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal();
        }

        /// <summary>
        /// 총기를 재장전합니다.
        /// 모든 투사체를 객체 풀로 반환합니다.
        /// </summary>
        public override void Reload()
        {
            bulletPool?.ReturnToPoolEverything();
        }
    }
}