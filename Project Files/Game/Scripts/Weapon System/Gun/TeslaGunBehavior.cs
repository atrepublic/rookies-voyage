// 이 스크립트는 테슬라 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 충전, 연쇄 공격, 파티클 및 사운드 효과,
// 그리고 치명타 판정 및 플로팅 텍스트 생성을 구현합니다.
using UnityEngine;
using Watermelon; // Watermelon 프레임워크 네임스페이스
using System.Collections.Generic; // Dictionary 사용

// DOTween 사용 시 주석 해제 또는 프로젝트에 맞게 설정
// using DG.Tweening; 

namespace Watermelon.SquadShooter
{
    public class TeslaGunBehavior : BaseGunBehavior
    {
        [Header("테슬라 총 전용 설정")]
        [Tooltip("총기 발사 시 재생될 충전/발사 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem shootParticleSystem;
        
        [Tooltip("완전히 충전되었을 때 활성화되는 루프 파티클 게임 오브젝트입니다.")]
        [SerializeField] GameObject lightningLoopParticle;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다.")]
        [SerializeField] LayerMask targetLayers;
        
        [Tooltip("총기 충전 완료까지 걸리는 시간입니다.")]
        [SerializeField] float chargeDuration;
        
        [Tooltip("테슬라 투사체가 연쇄 공격할 적 대상 수 범위 (최소/최대 값)입니다.")]
        [SerializeField] DuoInt targetsHitGoal;

        private DuoFloat bulletSpeed; // 투사체 속도 (RecalculateDamage에서 설정됨)
        private Pool bulletPool;      // 테슬라 투사체 오브젝트 풀

        // 애니메이션 및 발사 로직용 내부 변수
        private TweenCase shootTweenCase;
        private Vector3 shootDirection;
        private bool isCharging;
        private bool isCharged;
        private bool isChargeParticleActivated;
        private float fullChargeTime;
        private float startChargeTime; // startChartgeTime 오타 수정

        // 플로팅 텍스트 빈도 조절용
        private Dictionary<BaseEnemyBehavior, float> lastFloatingTextTimePerEnemy = new Dictionary<BaseEnemyBehavior, float>();
        private const float FLOATING_TEXT_COOLDOWN = 0.2f; // 플로팅 텍스트 표시 최소 간격 (초)

        // 치명타 텍스트 색상 (다른 총기와 유사하게 정의)
        private static readonly Color critTextColor = new Color(1f, 0.4f, 0f); // 예시: 주황색

        /// <summary>
        /// 테슬라 총의 동작을 초기화합니다.
        /// </summary>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon);

            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletObj = currentUpgrade.BulletPrefab;

            // 이름 중복을 피하기 위해 풀 이름에 총기 ID 또는 고유한 접두사 추가 고려 가능
            bulletPool = new Pool(bulletObj, $"TeslaBullet_{weapon.ID}_{bulletObj.name}"); 
            
            RecalculateDamage();
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출됩니다.
        /// </summary>
        private void OnDestroy()
        {
            if(bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null; // 참조 해제
            }
        }

        /// <summary>
        /// 레벨이 로드될 때 호출됩니다.
        /// </summary>
        public override void OnLevelLoaded()
        {
            RecalculateDamage();
            // 레벨이 새로 로드될 때 마지막 플로팅 텍스트 시간 기록 초기화
            lastFloatingTextTimePerEnemy.Clear();
        }

        /// <summary>
        /// 현재 무기 강화 상태에 따라 총기의 데미지 및 관련 스탯을 다시 계산합니다.
        /// </summary>
        public override void RecalculateDamage()
        {
            if (weapon == null) // weapon이 null일 경우 방어
            {
                Debug.LogError("[TeslaGunBehavior] RecalculateDamage: WeaponData가 null입니다!");
                return;
            }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            if (currentUpgrade == null) // currentUpgrade가 null일 경우 방어
            {
                Debug.LogError($"[TeslaGunBehavior] RecalculateDamage: WeaponData '{weapon.WeaponName}'의 현재 강화 단계 정보를 가져올 수 없습니다!");
                return;
            }

            damage = currentUpgrade.Damage; // BaseGunBehavior의 damage 필드 (DuoInt)
            bulletSpeed = currentUpgrade.BulletSpeed;
        }

        /// <summary>
        /// 매 프레임 테슬라 총의 동작을 업데이트합니다.
        /// </summary>
        public override void GunUpdate()
        {
            // 캐릭터 및 무기 데이터 유효성 검사
            if (characterBehaviour == null || weapon == null) return;

            // 재장전 UI 업데이트 (충전 중이 아닐 때만 최대치로 표시)
            if(!isCharging && !isCharged)
            {
                AttackButtonBehavior.SetReloadFill(1); 
            }

            // 주변에 적이 없으면 충전 중이거나 완료된 상태를 취소
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
                Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 충전 시작.");
            }

            // 충전 진행 로직
            if (fullChargeTime >= Time.timeSinceLevelLoad)
            {
                float chargeProgress = 1 - (Time.timeSinceLevelLoad - startChargeTime) / chargeDuration;
                AttackButtonBehavior.SetReloadFill(Mathf.Clamp01(chargeProgress));

                if (!isChargeParticleActivated && fullChargeTime - Time.timeSinceLevelLoad <= 0.5f)
                {
                    isChargeParticleActivated = true;
                    if (shootParticleSystem != null) shootParticleSystem.Play();
                    Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 충전 파티클 활성화.");
                }

                // 시야 내 적 유무에 따른 타겟 상태 변경
                if (IsEnemyVisible()) characterBehaviour.SetTargetActive();
                else characterBehaviour.SetTargetUnreachable();
                
                return; // 충전 중에는 발사 로직으로 넘어가지 않음
            }
            // 충전 완료 처리
            else if (!isCharged)
            {
                AttackButtonBehavior.SetReloadFill(0); 
                isCharged = true;
                if (lightningLoopParticle != null) lightningLoopParticle.SetActive(true);
                Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 충전 완료.");
            }

            // 발사 로직 (충전 완료, 시야 내 적 존재, 공격 허용 상태일 때)
            if (isCharged && IsEnemyVisible() && characterBehaviour.IsAttackingAllowed)
            {
                Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 발사 조건 충족, 발사 시도.");
                characterBehaviour.SetTargetActive();

                shootTweenCase.KillActive(); 
                shootTweenCase = transform.DOLocalMoveZ(-0.15f, chargeDuration * 0.3f).OnComplete(delegate
                {
                    shootTweenCase = transform.DOLocalMoveZ(0, chargeDuration * 0.6f);
                });

                WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
                if (currentUpgrade == null) { // 방어 코드
                    Debug.LogError($"[TeslaGunBehavior] GunUpdate: WeaponData '{weapon.WeaponName}'의 현재 강화 단계 정보를 가져올 수 없습니다!");
                    CancelCharge(); // 발사 취소
                    return;
                }
                int bulletsNumber = currentUpgrade.BulletsPerShot.Random();

                for (int k = 0; k < bulletsNumber; k++)
                {
                    if (bulletPool == null) { // 방어 코드
                        Debug.LogError($"[TeslaGunBehavior] GunUpdate: bulletPool이 null입니다! Init이 제대로 호출되었는지 확인하세요.");
                        break; 
                    }
                    GameObject bulletGO = bulletPool.GetPooledObject();
                    if (bulletGO == null) { // 방어 코드
                         Debug.LogError($"[TeslaGunBehavior] GunUpdate: bulletPool에서 가져온 게임 오브젝트가 null입니다!");
                         continue;
                    }

                    bulletGO.transform.position = shootPoint.position;
                    bulletGO.transform.eulerAngles = characterBehaviour.transform.eulerAngles;
                    
                    TeslaBulletBehavior bullet = bulletGO.GetComponent<TeslaBulletBehavior>();
                    if (bullet == null) { // 방어 코드
                        Debug.LogError($"[TeslaGunBehavior] GunUpdate: 가져온 투사체 프리팹에 TeslaBulletBehavior 컴포넌트가 없습니다!");
                        bulletGO.SetActive(false); // 사용 불가한 객체는 풀로 반환되도록 비활성화
                        continue;
                    }

                    // 1. 데미지 및 치명타 계산 (BaseGunBehavior의 메서드 사용)
                    var (calculatedDamageValue, isCritical) = CalculateFinalDamageWithCrit(); 
                    float finalDamageForBullet = calculatedDamageValue * characterBehaviour.Stats.BulletDamageMultiplier;

                    // 2. 투사체 초기화 (계산된 최종 데미지 적용)
                    bullet.Init(finalDamageForBullet, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, 5f, false); 
                    bullet.SetTargetsHitGoal(targetsHitGoal.Random());
                    Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 투사체 ID: {bullet.gameObject.GetInstanceID()} 발사. 데미지: {finalDamageForBullet}, 치명타: {isCritical}");

                    // 3. 플로팅 텍스트 생성 (빈도 조절 포함) - 최초 발사 대상에 대해서만
                    // 중요: 이 로직은 최초 발사되는 투사체에 대한 텍스트입니다. 
                    //       TeslaBulletBehavior.OnEnemyHitted에서 연쇄 타격에 대한 텍스트 생성을 별도로 관리해야 합니다.
                    BaseEnemyBehavior initialTarget = characterBehaviour.ClosestEnemyBehaviour;
                    if (initialTarget != null) 
                    {
                        bool showText = true;
                        if (lastFloatingTextTimePerEnemy.TryGetValue(initialTarget, out float lastTime))
                        {
                            if (Time.timeSinceLevelLoad < lastTime + FLOATING_TEXT_COOLDOWN)
                            {
                                showText = false;
                            }
                        }

                        if (showText)
                        {
                            lastFloatingTextTimePerEnemy[initialTarget] = Time.timeSinceLevelLoad;

                            Color textColor = isCritical ? critTextColor : Color.white; 
                            Vector3 textPosition = initialTarget.transform.position; // 기본 위치
                            
                            // 적 스탯을 이용한 오프셋 적용 (EnemyStats.cs에 HitTextOffsetY, HitTextOffsetForward 정의 필요)
                            if(initialTarget.Stats != null) { 
                                 textPosition += initialTarget.transform.forward * initialTarget.Stats.HitTextOffsetForward + 
                                                 initialTarget.transform.up * initialTarget.Stats.HitTextOffsetY + // 위쪽 오프셋을 위해 up 벡터 사용
                                                 Random.insideUnitSphere * 0.1f; // 약간의 랜덤 위치 가미
                            } else { 
                                 textPosition += Vector3.up * 1.7f + Random.insideUnitSphere * 0.1f; // 기본 Y 오프셋 및 랜덤
                            }

                            FloatingTextController.SpawnFloatingText(
                                "Hit", 
                                finalDamageForBullet.ToString("F0"), // 데미지 값을 정수로 표시
                                textPosition,
                                Quaternion.identity, // 기본 회전
                                isCritical ? 1.2f : 1.0f, // 치명타 시 텍스트 크기 약간 확대 (선택 사항)
                                textColor,
                                isCritical 
                            );
                            Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 플로팅 텍스트 생성. 대상: {initialTarget.name}, 데미지: {finalDamageForBullet}, 치명타: {isCritical}");
                        } else {
                            Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 플로팅 텍스트 생성 쿨다운. 대상: {initialTarget.name}");
                        }
                    }
                }

                characterBehaviour.OnGunShooted(); 

                VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game); 
                if(gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                CancelCharge(); // 발사 후 충전 상태 해제

                AudioController.PlaySound(AudioController.AudioClips.shotTesla, volumePercentage: 0.8f); 
            }
            else if (isCharged) // 충전은 완료되었으나 발사 조건 미충족 (예: 적이 시야 벗어남)
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        /// <summary>
        /// 적이 현재 총기의 시야 범위 내에 있고 레이캐스트로 확인 가능한지 여부를 판단합니다.
        /// </summary>
        public bool IsEnemyVisible()
        {
            if (characterBehaviour == null || !characterBehaviour.IsCloseEnemyFound || characterBehaviour.ClosestEnemyBehaviour == null) // 방어 코드 강화
                return false;

            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            // 레이캐스트 발사 원점을 약간 뒤로 이동시켜 캐릭터 자신에게 맞는 경우를 방지할 수 있음
            Vector3 rayOrigin = shootPoint.position - shootDirection.normalized * 0.1f; 

            RaycastHit hitInfo;
            // LayerMask.GetMask("Enemy", "Obstacle") 와 같이 사용하는 것이 더 명확할 수 있습니다. targetLayers가 올바르게 설정되었는지 확인 필요.
            if (Physics.Raycast(rayOrigin, shootDirection, out hitInfo, 300f, targetLayers)) 
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY) 
                {
                    // 발사각 체크: 총구 방향과 실제 적 방향이 너무 크게 차이나지 않는지 확인
                    if (Vector3.Angle(shootDirection.normalized, transform.forward.normalized) < 45f) // 각도를 조금 더 여유있게
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
            if (shootParticleSystem != null) shootParticleSystem.Stop();
            Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 충전 취소됨.");
        }
        
        /// <summary>
        /// 캐릭터 그래픽스의 테슬라 홀더 트랜스폼에 총기를 장착시킵니다.
        /// </summary>
        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            if (characterGraphics == null || characterGraphics.TeslaHolderTransform == null) // 방어 코드
            {
                Debug.LogError($"[TeslaGunBehavior] PlaceGun: characterGraphics 또는 TeslaHolderTransform이 null입니다!");
                return;
            }
            transform.SetParent(characterGraphics.TeslaHolderTransform); 
            transform.ResetLocal(); 
        }

        /// <summary>
        /// 총기가 해제될 때 호출됩니다. (풀 반환은 OnDestroy에서 처리)
        /// </summary>
        public override void OnGunUnloaded()
        {
            // 현재는 OnDestroy에서 풀을 파괴하므로, 개별 Unload 시 특별한 처리는 없음.
            // 만약 풀을 파괴하지 않고 재사용한다면 여기서 관련 로직 추가 가능.
            CancelCharge(); // 혹시 충전 중이었다면 취소
            Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - 총기 해제됨 (OnGunUnloaded).");
        }

        /// <summary>
        /// 총기를 재장전합니다. 테슬라 건은 별도의 재장전 로직이 없을 수 있습니다.
        /// </summary>
        public override void Reload()
        {
            // 테슬라 건은 전통적인 의미의 재장전이 없을 수 있으므로, 필요시 로직 추가.
            // 현재는 풀의 모든 객체를 반환하는 정도로만 구현 (Squad Shooter 원본과 유사).
            bulletPool?.ReturnToPoolEverything(); 
            Debug.Log($"[테슬라 건] ID: {this.gameObject.GetInstanceID()} - Reload 호출됨 (모든 투사체 풀로 반환 시도).");
        }

        // OnDrawGizmos는 에디터 디버깅용이므로 원본 유지 또는 필요시 수정
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterBehaviour == null || shootPoint == null) return;
            if (characterBehaviour.ClosestEnemyBehaviour == null) return;

            Color defCol = Gizmos.color;
            Gizmos.color = Color.red;
            Vector3 targetEnemyPos = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y);
            Vector3 currentShootDirection = targetEnemyPos - shootPoint.position;
            Gizmos.DrawLine(shootPoint.position, shootPoint.position + currentShootDirection.normalized * 5f);
            Gizmos.color = defCol;
        }
#endif
    }
}