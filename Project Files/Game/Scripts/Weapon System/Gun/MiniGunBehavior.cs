// MinigunBehavior.cs
// 이 스크립트는 미니건 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 총신 회전, 빠른 연사, 파티클 및 사운드 효과를 구현합니다.
// 플로팅 텍스트 생성 책임은 PlayerBulletBehavior로 이전되었습니다.
using System.Collections.Generic;
using UnityEngine;
using Watermelon; // Pool, PoolManager, Tween, ParticlesController, AudioController 등

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 미니건의 발사 로직, 총신 회전 및 관련 효과를 처리하는 클래스입니다.
    /// BaseGunBehavior를 상속받습니다.
    /// </summary>
    public class MinigunBehavior : BaseGunBehavior
    {
        [Header("미니건 전용 설정")]
        [Tooltip("미니건 총신(배럴) 트랜스폼입니다. 발사 시 회전합니다.")]
        [SerializeField] private Transform barrelTransform;

        [Tooltip("총기 발사 시 재생될 파티클 시스템입니다.")]
        [SerializeField] private ParticleSystem shootParticleSystem;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다.")]
        [SerializeField] private LayerMask targetLayers;
        [Tooltip("투사체가 자동으로 비활성화될 때까지의 시간입니다.")]
        [SerializeField] private float bulletDisableTime = 1.5f;

        [Tooltip("총신 회전 속도입니다 (초당 각도).")]
        [SerializeField] private float fireRotationSpeed = 1000f;
        [Tooltip("여러 줄기로 발사될 경우 각 줄기의 발사 각도 오프셋 리스트입니다. 비어있으면 단일 줄기로 발사됩니다.")]
        [SerializeField] private List<float> bulletStreamAngles;

        // 투사체 퍼짐(spread) 각도입니다. WeaponUpgrade에서 설정됩니다.
        private float spread;
        // 총기 발사 사이의 지연 시간 (공격 속도에 반비례)입니다.
        private float attackDelay;
        // 투사체의 이동 속도 범위 (최소/최대 값)입니다.
        private DuoFloat bulletSpeed;

        // 다음 발사가 가능한 시간입니다.
        private float nextShootTime;
        // 마지막 발사 시간입니다. (재장전 UI 등에 사용될 수 있습니다)
        private float lastShootTime;

        // 미니건 투사체 객체 풀입니다.
        private Pool bulletPool;
        // 현재 적을 향하는 발사 방향 벡터입니다.
        private Vector3 currentShootDirection;
        // 총기 그래픽스 이동 애니메이션(반동)을 제어하는 TweenCase 객체입니다.
        private TweenCase shootTweenCase;

        /// <summary>
        /// 미니건의 동작을 초기화합니다.
        /// </summary>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon);

            // 필수 데이터 유효성 검사
            if (weapon == null || weapon.GetCurrentUpgrade() == null || weapon.GetCurrentUpgrade().BulletPrefab == null)
            {
                Debug.LogError($"[MinigunBehavior] Init: WeaponData 또는 BulletPrefab이 유효하지 않습니다! 무기명: {(weapon != null ? weapon.WeaponName : "N/A")}");
                this.enabled = false; // 핵심 데이터 없으면 컴포넌트 비활성화
                return;
            }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletPrefab = currentUpgrade.BulletPrefab;

            if (bulletPrefab.GetComponent<PlayerBulletBehavior>() == null) // MinigunBulletBehavior도 PlayerBulletBehavior를 상속
            {
                Debug.LogError($"[MinigunBehavior] Init: BulletPrefab '{bulletPrefab.name}'에 PlayerBulletBehavior 또는 이를 상속하는 컴포넌트가 없습니다!");
                this.enabled = false;
                return;
            }

            // 풀 이름에 총기 타입과 인스턴스 ID를 추가하여 고유성 확보
            bulletPool = new Pool(bulletPrefab, $"{bulletPrefab.name}_MinigunPool_{this.GetInstanceID()}");
            RecalculateDamage(); // 데미지 및 관련 스탯 계산
        }

        /// <summary>
        /// 이 게임 오브젝트가 파괴될 때 호출됩니다.
        /// 할당된 투사체 풀을 정리하고, 진행 중인 트윈 애니메이션을 중지합니다.
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

        /// <summary>
        /// 레벨이 로드될 때 호출될 수 있는 콜백입니다.
        /// 무기 스탯을 다시 계산합니다.
        /// </summary>
        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        /// <summary>
        /// 현재 무기 강화 상태에 따라 총기의 데미지 및 관련 스탯(퍼짐 각도, 공격 속도, 투사체 속도)을 다시 계산합니다.
        /// </summary>
        public override void RecalculateDamage()
        {
            if (weapon == null) { Debug.LogError("[MinigunBehavior] RecalculateDamage: weapon 데이터가 없습니다."); return; }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            if (currentUpgrade == null) { Debug.LogError($"[MinigunBehavior] RecalculateDamage: {weapon.name}의 현재 강화 정보를 가져올 수 없습니다."); return; }

            damage = currentUpgrade.Damage; // BaseGunBehavior의 DuoInt damage (기본 공격력 범위)
            attackDelay = 1f / currentUpgrade.FireRate; // 연사 간격
            spread = currentUpgrade.Spread; // 퍼짐 각도
            bulletSpeed = currentUpgrade.BulletSpeed; // 총알 속도
        }

        /// <summary>
        /// 매 프레임 미니건의 발사 로직 및 관련 업데이트를 처리합니다.
        /// </summary>
        public override void GunUpdate()
        {
            // 재장전 UI 업데이트 (미니건은 연사력이 매우 빨라 다른 방식 고려 가능)
            if (attackDelay > 0.15f) // 연사 간격이 어느 정도 있을 때만 의미 있는 UI 업데이트
            {
                float reloadProgress = (nextShootTime > lastShootTime && (nextShootTime - lastShootTime) > 0.01f)
                                     ? (1f - Mathf.Clamp01((Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime)))
                                     : 1f;
                AttackButtonBehavior.SetReloadFill(reloadProgress);
            }
            else // 연사력이 매우 빠르면 재장전 UI를 항상 최대로 표시하거나 숨김
            {
                AttackButtonBehavior.SetReloadFill(1f);
            }

            // 총신 회전 (적 유무나 발사 여부와 관계없이 회전 가능, 또는 조건부 회전)
            if (barrelTransform != null)
            {
                float currentRotationSpeed = fireRotationSpeed * Time.deltaTime;
                // 적이 없거나 공격 중이 아닐 때는 회전 속도를 줄이는 등의 디테일 추가 가능
                if (characterBehaviour == null || !characterBehaviour.IsAttackingAllowed || !characterBehaviour.IsCloseEnemyFound)
                {
                    currentRotationSpeed *= 0.2f; // 예: 평소엔 20% 속도로 회전
                }
                barrelTransform.Rotate(Vector3.forward, currentRotationSpeed, Space.Self); // 자신의 Z축(앞쪽) 기준 회전
            }

            if (characterBehaviour == null || !characterBehaviour.IsCloseEnemyFound)
                return;

            if (nextShootTime > Time.timeSinceLevelLoad || !characterBehaviour.IsAttackingAllowed)
                return;

            if (characterBehaviour.ClosestEnemyBehaviour == null)
            {
                characterBehaviour.SetTargetUnreachable();
                return;
            }

            // 미니건은 AttackButtonBehavior.SetReloadFill(0)을 매번 호출하지 않을 수 있음 (계속 발사 상태 유지)

            currentShootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            // 레이캐스트를 사용하여 발사 경로에 적이 있는지, 시야각 내에 있는지 확인
            if (Physics.Raycast(shootPoint.position, currentShootDirection.normalized, out RaycastHit hitInfo, 300f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    if (Vector3.Angle(currentShootDirection.normalized, transform.forward.normalized) < 45f) // 발사각 제한 완화 가능
                    {
                        characterBehaviour.SetTargetActive();

                        // 미니건 반동 애니메이션 (더 약하게 또는 다르게 표현 가능)
                        shootTweenCase.KillActive();
                        shootTweenCase = transform.DOLocalMoveZ(-0.05f, attackDelay * 0.4f).OnComplete(delegate // 반동 시간 약간 늘림
                        {
                            shootTweenCase = transform.DOLocalMoveZ(0, attackDelay * 0.5f);
                        });

                        if(shootParticleSystem != null) shootParticleSystem.Play();
                        
                        nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                        lastShootTime = Time.timeSinceLevelLoad;

                        // bulletStreamAngles가 null이거나 비어있으면 기본 단일 스트림(0도)으로 설정
                        if (bulletStreamAngles == null || bulletStreamAngles.Count == 0)
                        {
                            bulletStreamAngles = new List<float> { 0f };
                        }

                        int bulletsPerShotFromData = weapon.GetCurrentUpgrade().BulletsPerShot.Random();

                        for (int k = 0; k < bulletsPerShotFromData; k++) // 보통 미니건은 이 루프가 1번 (k=0)
                        {
                            foreach (float streamAngle in bulletStreamAngles) // 인스펙터에서 설정한 각도 스트림 (보통 미니건은 1개 스트림)
                            {
                                if (bulletPool == null) { Debug.LogError($"[MinigunBehavior] GunUpdate: bulletPool이 null입니다!"); continue; }
                                GameObject bulletGO = bulletPool.GetPooledObject();
                                if (bulletGO == null) { Debug.LogError($"[MinigunBehavior] GunUpdate: bulletPool에서 null 오브젝트를 반환받았습니다."); continue; }

                                // 총알 초기 위치는 shootPoint, 초기 방향은 캐릭터 정면에 streamAngle 적용
                                Quaternion initialBulletRotation = Quaternion.LookRotation(characterBehaviour.transform.forward.SetY(0)) * Quaternion.Euler(0, streamAngle, 0);
                                bulletGO.transform.SetPositionAndRotation(shootPoint.position, initialBulletRotation);
                                
                                PlayerBulletBehavior bulletScript = bulletGO.GetComponent<PlayerBulletBehavior>(); // MinigunBulletBehavior도 PlayerBulletBehavior를 상속
                                if (bulletScript == null)
                                {
                                    Debug.LogError($"[MinigunBehavior] 총알 프리팹 '{bulletGO.name}'에 PlayerBulletBehavior 또는 상속 스크립트가 없습니다!");
                                    bulletGO.SetActive(false);
                                    continue;
                                }

                                // 1. 총구 발사 시점의 데미지 및 치명타 계산
                                var (gunCalculatedDamage, isGunCritical) = CalculateFinalDamageWithCrit();
                                // 2. 캐릭터의 최종 데미지 배율 등 적용
                                float damageToPassToBullet = (float)gunCalculatedDamage * characterBehaviour.Stats.BulletDamageMultiplier;

                                // 3. 총알 초기화 (변경된 PlayerBulletBehavior.Init 시그니처 호출)
                                bulletScript.Init(
                                    damageToPassToBullet,
                                    bulletSpeed.Random(),
                                    characterBehaviour.ClosestEnemyBehaviour, // 미니건은 보통 현재 가장 가까운 적을 계속 조준
                                    bulletDisableTime,
                                    true, // 미니건 총알은 보통 충돌 시 비활성화
                                    isGunCritical,
                                    this.characterBehaviour
                                );
                                
                                // 최종 발사 각도에 spread(퍼짐) 적용
                                float currentPelletSpread = Random.Range(spread * -0.5f, spread * 0.5f);
                                bulletScript.transform.Rotate(Vector3.up * currentPelletSpread, Space.Self); // 자신의 Y축 기준 회전

                                // 플로팅 텍스트 생성은 PlayerBulletBehavior.OnTriggerEnter에서 처리하므로 여기서는 호출하지 않음.
                            }
                        }

                        characterBehaviour.OnGunShooted();
                        AudioController.PlaySound(AudioController.AudioClips.shotMinigun); // 미니건 전용 사운드
                        // 미니건은 연사력이 매우 높아 카메라 셰이크를 약하게 하거나, N발당 한 번 등으로 조절 가능
                        // if (Time.frameCount % 3 == 0) // 예: 3프레임당 한 번 셰이크
                        // {
                        //    VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
                        //    if(gameCameraCase != null) gameCameraCase.Shake(0.01f, 0.01f, 0.05f, 0.3f); // 매우 약한 셰이크
                        // }
                    }
                    // else: 시야각 벗어남
                }
                else // 장애물 명중
                {
                    characterBehaviour.SetTargetUnreachable();
                }
            }
            else // 레이캐스트 미명중
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
            if (barrelTransform != null) // 총신 회전 정지 (선택적)
            {
                // barrelTransform.DOKill(); // DOTween으로 회전 시켰다면
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            if (characterGraphics == null || characterGraphics.MinigunHolderTransform == null)
            {
                Debug.LogError($"[MinigunBehavior] PlaceGun: characterGraphics 또는 MinigunHolderTransform이 null입니다!");
                return;
            }
            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            // 미니건은 전통적인 재장전 개념이 없을 수 있음. 이 메서드가 호출될 때 특별한 동작이 필요 없다면 비워둘 수 있음.
            // 또는 모든 총알을 풀로 반환하는 등의 정리 작업만 수행.
            bulletPool?.ReturnToPoolEverything();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || characterBehaviour == null || shootPoint == null) return;

            if (characterBehaviour.ClosestEnemyBehaviour != null)
            {
                Color defCol = Gizmos.color;
                Gizmos.color = Color.yellow; // 미니건 기즈모 색상
                Vector3 targetPos = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y);
                Vector3 dir = targetPos - shootPoint.position;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Gizmos.DrawRay(shootPoint.position, dir.normalized * 7f);
                }

                // Spread 시각화 (옵션)
                if (spread > 0)
                {
                    Quaternion leftRayRotation = Quaternion.AngleAxis(-spread / 2, Vector3.up);
                    Quaternion rightRayRotation = Quaternion.AngleAxis(spread / 2, Vector3.up);
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // 주황색 반투명
                    Gizmos.DrawRay(shootPoint.position, leftRayRotation * characterBehaviour.transform.forward.SetY(0) * 5f);
                    Gizmos.DrawRay(shootPoint.position, rightRayRotation * characterBehaviour.transform.forward.SetY(0) * 5f);
                }
                Gizmos.color = defCol;
            }
        }
#endif
    }
}