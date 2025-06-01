// ShotgunBehavior.cs
// 이 스크립트는 샷건 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 산탄 형태로 투사체 발사, 파티클, 사운드 및 에디터 디버그 기능을 구현합니다.
// 플로팅 텍스트 생성 책임은 PlayerBulletBehavior로 이전되었습니다.
using UnityEngine;
using Watermelon; // Pool, PoolManager, Tween, ParticlesController, AudioController 등

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 샷건의 발사 로직 및 관련 효과를 처리하는 클래스입니다.
    /// BaseGunBehavior를 상속받습니다.
    /// </summary>
    public class ShotgunBehavior : BaseGunBehavior
    {
        [Header("샷건 전용 설정")]
        [LineSpacer]
        [Tooltip("총기 발사 시 재생될 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem shootParticleSystem;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다 (예: 적, 장애물).")]
        [SerializeField] LayerMask targetLayers;
        [Tooltip("투사체가 자동으로 비활성화될 때까지의 시간입니다.")]
        [SerializeField] float bulletDisableTime = 1.2f;

        // 총기 발사 사이의 지연 시간 (공격 속도에 반비례)입니다.
        private float attackDelay;
        // 투사체의 이동 속도 범위 (최소/최대 값)입니다.
        private DuoFloat bulletSpeed;
        // 투사체 산탄(퍼짐)의 최대 각도입니다.
        private float bulletSpreadAngle;

        // 다음 발사가 가능한 시간입니다.
        private float nextShootTime;
        // 마지막 발사 시간입니다. (재장전 UI 등에 사용될 수 있습니다)
        private float lastShootTime;

        // 샷건 투사체 객체 풀입니다.
        private Pool bulletPool;

        // 총기 그래픽스 이동 애니메이션(반동)을 제어하는 TweenCase 객체입니다.
        private TweenCase shootTweenCase;
        // 현재 적을 향하는 발사 방향 벡터입니다.
        private Vector3 currentShootDirection;
        
        /// <summary>
        /// 샷건 총의 동작을 초기화합니다.
        /// 기본 총기 정보 설정 후 투사체 풀을 생성하고 관련 스탯을 계산합니다.
        /// </summary>
        /// <param name="characterBehaviour">총기를 장착한 캐릭터의 CharacterBehaviour 컴포넌트입니다.</param>
        /// <param name="weapon">이 총기의 WeaponData입니다.</param>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon); // 부모 클래스의 Init 호출

            // 필수 데이터 유효성 검사
            if (weapon == null || weapon.GetCurrentUpgrade() == null || weapon.GetCurrentUpgrade().BulletPrefab == null)
            {
                Debug.LogError($"[ShotgunBehavior] Init: WeaponData 또는 BulletPrefab이 유효하지 않습니다! 무기명: {(weapon != null ? weapon.WeaponName : "N/A")}");
                this.enabled = false; // 핵심 데이터 없으면 이 컴포넌트 비활성화
                return;
            }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletPrefab = currentUpgrade.BulletPrefab;

            if (bulletPrefab.GetComponent<PlayerBulletBehavior>() == null) // ShotgunBulletBehavior도 PlayerBulletBehavior를 상속
            {
                Debug.LogError($"[ShotgunBehavior] Init: BulletPrefab '{bulletPrefab.name}'에 PlayerBulletBehavior 또는 이를 상속하는 컴포넌트가 없습니다!");
                this.enabled = false;
                return;
            }

            // 총알 이름과 이 스크립트 인스턴스 ID를 조합하여 풀 이름의 고유성 확보
            bulletPool = new Pool(bulletPrefab, $"{bulletPrefab.name}_ShotgunPool_{this.GetInstanceID()}");
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
                bulletPool = null; // 참조 해제
            }
            shootTweenCase.KillActive(); // 활성화된 트윈 애니메이션 중지
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
        /// 현재 무기 강화 상태에 따라 총기의 데미지 및 관련 스탯(산탄 각도, 공격 속도, 투사체 속도)을 다시 계산합니다.
        /// </summary>
        public override void RecalculateDamage()
        {
            if (weapon == null) { Debug.LogError("[ShotgunBehavior] RecalculateDamage: weapon 데이터가 없습니다."); return; }
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            if (currentUpgrade == null) { Debug.LogError($"[ShotgunBehavior] RecalculateDamage: {weapon.name}의 현재 강화 정보를 가져올 수 없습니다."); return; }

            damage = currentUpgrade.Damage; // BaseGunBehavior의 DuoInt damage (기본 공격력 범위)
            bulletSpreadAngle = currentUpgrade.Spread;
            attackDelay = 1f / currentUpgrade.FireRate; // 연사 간격
            bulletSpeed = currentUpgrade.BulletSpeed;
        }

        /// <summary>
        /// 매 프레임 샷건의 발사 로직 및 관련 업데이트를 처리합니다.
        /// </summary>
        public override void GunUpdate()
        {
            // 재장전 UI 진행 상태 업데이트 (연사 간격이 극히 짧지 않을 때만 의미 있음)
            float reloadProgress = (nextShootTime > lastShootTime && (nextShootTime - lastShootTime) > 0.01f) 
                                 ? (1f - Mathf.Clamp01((Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime))) 
                                 : 1f;
            AttackButtonBehavior.SetReloadFill(reloadProgress);

            // 유효한 캐릭터 및 감지된 적 유무 확인
            if (characterBehaviour == null || !characterBehaviour.IsCloseEnemyFound)
                return;

            // 발사 가능 시간 및 캐릭터 공격 가능 상태 확인
            if (nextShootTime > Time.timeSinceLevelLoad || !characterBehaviour.IsAttackingAllowed)
                return;
            
            // 가장 가까운 적 유효성 재확인
            if (characterBehaviour.ClosestEnemyBehaviour == null)
            {
                characterBehaviour.SetTargetUnreachable(); // 타겟 유실 처리
                return;
            }

            AttackButtonBehavior.SetReloadFill(0); // 발사 직전 재장전 UI 초기화

            // 발사 방향 계산
            currentShootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            // 발사 지점에서 시야 확인 및 발사각 제한 (장애물 및 각도 확인)
            // LayerMask targetLayers는 적과 장애물을 모두 포함해야 함
            if (Physics.Raycast(shootPoint.position, currentShootDirection.normalized, out RaycastHit hitInfo, 300f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY) // 레이캐스트 결과가 적 레이어인지 확인
                {
                    if (Vector3.Angle(currentShootDirection.normalized, transform.forward.normalized) < 45f) // 총구 방향 기준 발사각 제한 (45도 예시)
                    {
                        characterBehaviour.SetTargetActive(); // 유효 타겟 설정

                        // 총기 반동 애니메이션
                        shootTweenCase.KillActive(); // 이전 애니메이션 중지
                        shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate
                        {
                            shootTweenCase = transform.DOLocalMoveZ(0, 0.15f);
                        });

                        if(shootParticleSystem != null) shootParticleSystem.Play(); // 발사 파티클 재생
                        
                        // 다음 발사 시간 및 마지막 발사 시간 업데이트
                        nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                        lastShootTime = Time.timeSinceLevelLoad;

                        WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
                        int bulletsToShoot = currentUpgrade.BulletsPerShot.Random(); // 이번에 발사할 총알(펠릿) 수
                        Debug.Log($"[ShotgunBehavior] 발사 결정. 총알 수: {bulletsToShoot}");

                        for (int i = 0; i < bulletsToShoot; i++)
                        {
                            // Debug.Log($"[ShotgunBehavior] 루프 {i}: 총알 생성 시도"); // 필요시 개별 총알 생성 로그 활성화
                            
                            if (bulletPool == null) { Debug.LogError($"[ShotgunBehavior] 루프 {i}: bulletPool이 null입니다!"); continue; }
                            GameObject bulletGO = bulletPool.GetPooledObject();
                            if (bulletGO == null) { Debug.LogError($"[ShotgunBehavior] 루프 {i}: bulletPool에서 null 오브젝트를 반환받았습니다."); continue; }

                            // 총알 초기 위치는 shootPoint, 초기 방향은 캐릭터의 현재 정면 방향으로 설정
                            bulletGO.transform.SetPositionAndRotation(shootPoint.position, Quaternion.LookRotation(characterBehaviour.transform.forward.SetY(0)));
                            
                            PlayerBulletBehavior bulletScript = bulletGO.GetComponent<PlayerBulletBehavior>(); // PlayerBulletBehavior로 캐스팅
                            if (bulletScript == null)
                            {
                                Debug.LogError($"[ShotgunBehavior] 루프 {i}: 총알 프리팹 '{bulletGO.name}'에 PlayerBulletBehavior 또는 상속 스크립트가 없습니다!");
                                bulletGO.SetActive(false); // 풀로 반환될 수 있도록 비활성화
                                continue; 
                            }
                            // Debug.Log($"[ShotgunBehavior] 루프 {i}: bullet.gameObject.name = {bulletScript.gameObject.name}, 활성 상태: {bulletScript.gameObject.activeSelf}");

                            // 1. 총구 발사 시점의 데미지 및 치명타 계산
                            var (gunCalculatedDamage, isGunCritical) = CalculateFinalDamageWithCrit();
                            // 2. 캐릭터의 최종 데미지 배율 등 적용 (BaseGunBehavior의 damage는 기본 범위, CalculateFinalDamageWithCrit가 최종 값 반환)
                            float damageToPassToBullet = (float)gunCalculatedDamage; // CalculateFinalDamageWithCrit 결과가 최종 데미지라 가정

                            // 3. 총알 초기화 (변경된 PlayerBulletBehavior.Init 시그니처 호출)
                            bulletScript.Init(
                                damageToPassToBullet,                   // float baseDamageFromGun
                                bulletSpeed.Random(),                   // float bulletSpeed
                                characterBehaviour.ClosestEnemyBehaviour, // BaseEnemyBehavior target (샷건은 각 펠릿이 알아서 날아감)
                                bulletDisableTime,                      // float autoDisableDuration
                                true,                                   // bool disableOnHit (샷건 펠릿은 보통 true)
                                isGunCritical,                          // bool isCritFromGun
                                this.characterBehaviour                 // CharacterBehaviour owner
                            );
                            // Debug.Log($"[ShotgunBehavior] 루프 {i}: 총알 초기화 완료. 전달 데미지: {damageToPassToBullet}, 초기치명타: {isGunCritical}");

                            // 플로팅 텍스트 생성은 PlayerBulletBehavior.OnTriggerEnter에서 처리하므로 여기서는 호출하지 않음.
                            
                            // 총알에 산탄 각도 적용
                            float currentPelletSpread = (bulletsToShoot == 1) ? 0f : Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f);
                            bulletScript.transform.Rotate(Vector3.up * currentPelletSpread, Space.Self); // 자신의 Y축 기준 회전
                            // Debug.Log($"[ShotgunBehavior] 루프 {i}: 총알 회전 적용 ({currentPelletSpread}도)");
                        }

                        characterBehaviour.OnGunShooted(); // 캐릭터 발사 이벤트 호출
                        
                        VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
                        if(gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f); // 카메라 흔들림
                        
                        AudioController.PlaySound(AudioController.AudioClips.shotShotgun); // 발사음 재생
                    }
                    // else : 시야각 벗어남
                }
                else // 적 레이어가 아닌 다른 장애물과 충돌
                {
                    characterBehaviour.SetTargetUnreachable();
                }
            }
            else // 레이캐스트에 아무것도 맞지 않음 (또는 사거리 밖)
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        /// <summary>
        /// 총기가 캐릭터로부터 해제될 때 호출됩니다.
        /// </summary>
        public override void OnGunUnloaded()
        {
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool); // 풀 파괴
                bulletPool = null;
            }
        }

        /// <summary>
        /// 캐릭터의 그래픽스 컴포넌트에 정의된 샷건 홀더에 이 총기를 배치합니다.
        /// </summary>
        /// <param name="characterGraphics">총기를 장착할 캐릭터의 그래픽스 컴포넌트입니다.</param>
        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            if (characterGraphics == null || characterGraphics.ShootGunHolderTransform == null) // 샷건 홀더 이름 확인 필요
            {
                Debug.LogError($"[ShotgunBehavior] PlaceGun: characterGraphics 또는 해당 캐릭터의 샷건용 총기 홀더(ShootGunHolderTransform)가 null입니다!");
                return;
            }
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal(); // 로컬 위치, 회전, 스케일 초기화
        }

        /// <summary>
        /// 총기를 재장전합니다. 샷건은 일반적으로 탄창 전체를 재장전하는 개념이므로,
        /// 현재는 모든 활성화된 투사체를 풀로 반환하는 역할만 합니다.
        /// </summary>
        public override void Reload()
        {
            bulletPool?.ReturnToPoolEverything(); // 풀에 모든 총알 반환 (null 안전 호출)
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 선택되었을 때 기즈모를 그립니다. 발사 방향 및 감지된 적을 시각화합니다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || characterBehaviour == null || shootPoint == null) return;

            // 현재 조준 방향 (캐릭터 정면 기준)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(shootPoint.position, characterBehaviour.transform.forward.SetY(0) * 5f);


            if (characterBehaviour.ClosestEnemyBehaviour != null)
            {
                // 가장 가까운 적을 향하는 계산된 발사 방향
                Vector3 calculatedShootDir = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
                if (calculatedShootDir.sqrMagnitude > 0.01f)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(shootPoint.position, calculatedShootDir.normalized * 7f); // 감지된 적 방향
                }
            }
        }
#endif
    }
}