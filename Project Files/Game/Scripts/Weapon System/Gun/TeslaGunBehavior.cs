// TeslaGunBehavior.cs
// 이 스크립트는 테슬라 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 충전, 연쇄 공격, 파티클 및 사운드 효과를 구현합니다.
// 플로팅 텍스트 생성 책임은 PlayerBulletBehavior로 이전되었습니다.
using UnityEngine;
using Watermelon; // Pool, PoolManager, Tween, ParticlesController, AudioController 등
// DOTween 사용 시 필요에 따라 using DG.Tweening; 추가

namespace Watermelon.SquadShooter
{
    public class TeslaGunBehavior : BaseGunBehavior
    {
        [Header("테슬라 총 전용 설정")]
        [Tooltip("총기 발사 시 재생될 충전/발사 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem shootParticleSystem;
        
        [Tooltip("완전히 충전되었을 때 활성화되는 루프 파티클 게임 오브젝트입니다.")]
        [SerializeField] GameObject lightningLoopParticle;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다. (현재 IsEnemyVisible에서 활용)")]
        [SerializeField] LayerMask targetLayers; 
        
        [Tooltip("총기 충전 완료까지 걸리는 시간입니다.")]
        [SerializeField] float chargeDuration = 1.0f;
        
        [Tooltip("테슬라 투사체가 연쇄 공격할 적 대상 수 범위 (최소/최대 값)입니다.")]
        [SerializeField] DuoInt targetsHitGoal = new DuoInt(3, 5);

        // 투사체 속도 (RecalculateDamage에서 WeaponUpgrade로부터 설정됨)
        private DuoFloat bulletSpeed;
        // 테슬라 투사체 오브젝트 풀
        private Pool bulletPool;

        // 애니메이션 및 발사 로직용 내부 변수
        private TweenCase shootTweenCase;
        private Vector3 currentShootDirection; // IsEnemyVisible에서 계산 및 사용
        private bool isCharging;
        private bool isCharged;
        private bool isChargeParticleActivated;
        private float fullChargeTime;
        private float startChargeTime;

        /// <summary>
        /// 테슬라 총의 동작을 초기화합니다.
        /// </summary>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon);

            if (weapon == null || weapon.GetCurrentUpgrade() == null || weapon.GetCurrentUpgrade().BulletPrefab == null)
            {
                Debug.LogError($"[TeslaGunBehavior] Init: WeaponData 또는 BulletPrefab이 유효하지 않습니다! 무기명: {(weapon != null ? weapon.WeaponName : "N/A")}");
                this.enabled = false; // 핵심 데이터 없으면 비활성화
                return;
            }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletObj = currentUpgrade.BulletPrefab;

            if (bulletObj.GetComponent<TeslaBulletBehavior>() == null)
            {
                Debug.LogError($"[TeslaGunBehavior] Init: BulletPrefab '{bulletObj.name}'에 TeslaBulletBehavior 컴포넌트가 없습니다!");
                this.enabled = false;
                return;
            }
            
            // 풀 이름에 총기 ID와 인스턴스 ID를 포함하여 고유성 확보
            bulletPool = new Pool(bulletObj, $"TeslaBullet_{weapon.ID}_{bulletObj.name}_{this.GetInstanceID()}"); 
            
            RecalculateDamage();
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출됩니다. 풀 및 트윈을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            if(bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null;
            }
            shootTweenCase.KillActive();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
            // 플로팅 텍스트 빈도 조절 관련 로직은 FloatingTextController로 이전되었으므로,
            // lastFloatingTextTimePerEnemy.Clear()는 여기서 필요 없습니다.
        }

        public override void RecalculateDamage()
        {
            if (weapon == null) { Debug.LogError("[TeslaGunBehavior] RecalculateDamage: weapon이 null입니다."); return; }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            if (currentUpgrade == null) { Debug.LogError($"[TeslaGunBehavior] RecalculateDamage: {weapon.name}의 currentUpgrade가 null입니다."); return; }

            damage = currentUpgrade.Damage; // BaseGunBehavior의 DuoInt damage (치명타 계산 시 사용)
            bulletSpeed = currentUpgrade.BulletSpeed;
            // chargeDuration, targetsHitGoal 등은 인스펙터에서 설정하거나, 필요시 여기서 WeaponUpgrade로부터 값을 받을 수 있습니다.
        }

        public override void GunUpdate()
        {
            if (characterBehaviour == null || weapon == null) return;

            // 충전 중이 아닐 때만 재장전 UI를 최대로 표시
            if(!isCharging && !isCharged)
            {
                AttackButtonBehavior.SetReloadFill(1); 
            }

            // 주변에 적이 없으면 진행 중인 충전/완료 상태 취소
            if (!characterBehaviour.IsCloseEnemyFound) 
            {
                if (isCharging || isCharged)
                {
                    CancelCharge();
                }
                return;
            }

            // 충전 시작 로직
            if (!isCharging && !isCharged)
            {
                isCharging = true;
                isChargeParticleActivated = false;
                fullChargeTime = Time.timeSinceLevelLoad + chargeDuration;
                startChargeTime = Time.timeSinceLevelLoad;
            }

            // 충전 진행 중 로직
            if (isCharging && fullChargeTime >= Time.timeSinceLevelLoad)
            {
                float chargeProgress = 1f - Mathf.Clamp01((Time.timeSinceLevelLoad - startChargeTime) / chargeDuration);
                AttackButtonBehavior.SetReloadFill(chargeProgress);

                // 충전 완료 직전에 충전 파티클 활성화 (0.5초 전)
                if (!isChargeParticleActivated && (fullChargeTime - Time.timeSinceLevelLoad <= 0.5f))
                {
                    isChargeParticleActivated = true;
                    if (shootParticleSystem != null) shootParticleSystem.Play();
                }

                // 충전 중에도 적 시야 확인 및 타겟 상태 업데이트
                if (IsEnemyVisible()) characterBehaviour.SetTargetActive();
                else characterBehaviour.SetTargetUnreachable();
                
                return; // 아직 충전 중이므로 발사 로직으로 넘어가지 않음
            }
            
            // 충전 완료된 순간의 처리
            if (isCharging && fullChargeTime < Time.timeSinceLevelLoad && !isCharged) 
            {
                isCharging = false;
                isCharged = true;
                AttackButtonBehavior.SetReloadFill(0); // 충전 완료 시 재장전 UI 0으로
                if (lightningLoopParticle != null) lightningLoopParticle.SetActive(true);
            }

            // 발사 로직: 충전 완료, 적 시야 확보, 공격 가능 상태일 때
            if (isCharged && IsEnemyVisible() && characterBehaviour.IsAttackingAllowed)
            {
                characterBehaviour.SetTargetActive();

                shootTweenCase.KillActive(); 
                shootTweenCase = transform.DOLocalMoveZ(-0.15f, chargeDuration * 0.3f).OnComplete(delegate // 반동 애니메이션
                {
                    shootTweenCase = transform.DOLocalMoveZ(0, chargeDuration * 0.6f);
                });

                WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
                if (currentUpgrade == null) {
                    Debug.LogError($"[TeslaGunBehavior] GunUpdate: WeaponData '{weapon.WeaponName}'의 현재 강화 단계 정보를 가져올 수 없습니다!");
                    CancelCharge(); 
                    return;
                }
                // 테슬라건은 보통 한 번에 한 줄기의 번개를 발사 (BulletsPerShot = 1 가정)
                int bulletsToShootOnce = currentUpgrade.BulletsPerShot.Random(); 

                for (int k = 0; k < bulletsToShootOnce; k++)
                {
                    if (bulletPool == null) { Debug.LogError($"[TeslaGunBehavior] GunUpdate 루프 {k}: bulletPool이 null입니다!"); break;  }
                    GameObject bulletGO = bulletPool.GetPooledObject();
                    if (bulletGO == null) { Debug.LogError($"[TeslaGunBehavior] GunUpdate 루프 {k}: bulletPool에서 null 오브젝트를 반환받았습니다!"); continue; }

                    // 총알 초기 위치 및 방향 설정 (캐릭터 정면 기준)
                    bulletGO.transform.SetPositionAndRotation(shootPoint.position, Quaternion.LookRotation(characterBehaviour.transform.forward.SetY(0)));
                    
                    TeslaBulletBehavior bullet = bulletGO.GetComponent<TeslaBulletBehavior>();
                    if (bullet == null) {
                        Debug.LogError($"[TeslaGunBehavior] GunUpdate 루프 {k}: 총알 프리팹 '{bulletGO.name}'에 TeslaBulletBehavior 컴포넌트가 없습니다!");
                        bulletGO.SetActive(false); 
                        continue;
                    }

                    // 1. 총구 발사 시점의 데미지 및 치명타 계산
                    var (gunCalculatedDamage, isGunCritical) = CalculateFinalDamageWithCrit();
                    // 2. 캐릭터의 최종 데미지 배율 적용 (BaseGunBehavior의 damage가 이미 최종값이라면 이 단계는 선택적)
                    float damageToPassToBullet = (float)gunCalculatedDamage * characterBehaviour.Stats.BulletDamageMultiplier;

                    // 3. 총알 초기화 (변경된 PlayerBulletBehavior.Init 시그니처 호출)
                    bullet.Init(
                        damageToPassToBullet,                 // float baseDamageFromGun
                        bulletSpeed.Random(),                 // float bulletSpeed
                        characterBehaviour.ClosestEnemyBehaviour, // BaseEnemyBehavior target (테슬라탄의 첫 타겟)
                        5f,                                   // float autoDisableDuration (테슬라탄 전용 값)
                        false,                                // bool disableOnHit (테슬라탄은 연쇄 후 자체 판단)
                        isGunCritical,                        // bool isCritFromGun
                        this.characterBehaviour               // CharacterBehaviour owner
                    );
                    bullet.SetTargetsHitGoal(targetsHitGoal.Random()); // 테슬라탄 고유의 연쇄 목표 수 설정
                    
                    // 플로팅 텍스트 생성 로직은 PlayerBulletBehavior.OnTriggerEnter에서 처리합니다.
                }

                characterBehaviour.OnGunShooted(); 

                VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game); 
                if(gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                AudioController.PlaySound(AudioController.AudioClips.shotTesla, volumePercentage: 0.8f); 
                CancelCharge(); // 발사 후 즉시 충전 상태 해제 및 다음 충전 준비
            }
            else if (isCharged && !IsEnemyVisible()) // 충전은 완료되었으나 발사 조건 미충족 (예: 적이 시야 벗어남)
            {
                characterBehaviour.SetTargetUnreachable();
                // 이 경우 충전을 계속 유지할지, 아니면 취소할지는 게임 디자인에 따릅니다.
                // CancelCharge(); // 만약 시야를 벗어나면 즉시 충전을 해제하고 싶다면 활성화
            }
        }

        /// <summary>
        /// 현재 가장 가까운 적이 시야 내에 있고 공격 가능한지 확인합니다.
        /// </summary>
        /// <returns>적이 보이면 true, 아니면 false</returns>
        public bool IsEnemyVisible()
        {
            if (characterBehaviour == null || !characterBehaviour.IsCloseEnemyFound || characterBehaviour.ClosestEnemyBehaviour == null)
                return false;

            // 가장 가까운 적을 향하는 방향 계산
            currentShootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
            if (currentShootDirection.sqrMagnitude < 0.001f) return true; // 매우 가까우면 방향 계산 없이 true

            // 레이캐스트 발사하여 장애물 확인
            // targetLayers에 "Obstacle"과 "Enemy"가 모두 포함되어 있어야 함
            if (Physics.Raycast(shootPoint.position, currentShootDirection.normalized, out RaycastHit hitInfo, 300f, targetLayers)) 
            {
                // 레이캐스트가 적에게 직접 도달했는지 확인
                if (hitInfo.collider.gameObject == characterBehaviour.ClosestEnemyBehaviour.gameObject) 
                {
                    // 총구 방향과 실제 적 방향 사이의 각도가 너무 크지 않은지 확인
                    if (Vector3.Angle(currentShootDirection.normalized, transform.forward.normalized) < 45f) // 발사각 허용 범위
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 총기 충전 상태를 취소하고 관련 파티클 및 플래그를 초기화합니다.
        /// </summary>
        private void CancelCharge()
        {
            isCharging = false;
            isCharged = false;
            isChargeParticleActivated = false;
            if (lightningLoopParticle != null) lightningLoopParticle.SetActive(false);
            if (shootParticleSystem != null && shootParticleSystem.isPlaying)
            {
                shootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            //Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 충전 취소됨.");
        }
        
        /// <summary>
        /// 캐릭터 그래픽스의 테슬라 총 홀더에 총기를 배치합니다.
        /// </summary>
        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            if (characterGraphics == null || characterGraphics.TeslaHolderTransform == null)
            {
                Debug.LogError($"[TeslaGunBehavior] PlaceGun: characterGraphics 또는 TeslaHolderTransform이 null입니다!");
                return;
            }
            transform.SetParent(characterGraphics.TeslaHolderTransform); 
            transform.ResetLocal(); 
        }

        /// <summary>
        /// 총기가 캐릭터로부터 해제될 때 호출됩니다.
        /// </summary>
        public override void OnGunUnloaded()
        {
            CancelCharge(); // 충전 상태가 있다면 취소
            // 풀 파괴는 OnDestroy에서 처리합니다.
        }

        /// <summary>
        /// 총기를 재장전합니다. 테슬라 건은 충전 방식이므로 이 메서드는 주로 상태 초기화에 사용될 수 있습니다.
        /// </summary>
        public override void Reload()
        {
            CancelCharge(); // 현재 충전 상태를 취소
            bulletPool?.ReturnToPoolEverything(); // 혹시 활성화된 총알이 있다면 풀로 반환
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || characterBehaviour == null || shootPoint == null) return;
            
            if (characterBehaviour.ClosestEnemyBehaviour != null)
            {
                Color defCol = Gizmos.color;
                Gizmos.color = Color.cyan; // 테슬라 컨셉 색상
                Vector3 targetEnemyPos = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y);
                // currentShootDirection은 GunUpdate에서 계산되지만, Gizmos는 매번 호출되므로 여기서도 필요시 계산
                Vector3 gizmoShootDirection = targetEnemyPos - shootPoint.position;
                if(gizmoShootDirection.sqrMagnitude > 0.01f)
                {
                    Gizmos.DrawRay(shootPoint.position, gizmoShootDirection.normalized * 7f); // 사거리 시각화 (예시 길이)
                }
                Gizmos.color = defCol;
            }
        }
#endif
    }
}