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
        private static readonly Color critColor = new Color(1f, 0.4f, 0f); // 치명타 시 텍스트 색상

        /// <summary>
        /// 샷건 총의 동작을 초기화합니다.
        /// 기본 총기 정보 설정 후 투사체 풀을 생성하고 데미지를 계산합니다.
        /// </summary>
        /// <param name="characterBehaviour">총기를 장착할 캐릭터 행동 컴포넌트</param>
        /// <param name="weapon">이 총기의 무기 데이터</param>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            base.Init(characterBehaviour, weapon); // 부모 클래스 초기화 호출
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            GameObject bulletObj = currentUpgrade.BulletPrefab; // 사용할 총알 프리팹 가져오기

            // 총알 이름으로 오브젝트 풀 생성
            bulletPool = new Pool(bulletObj, bulletObj.name + "_ShotgunPool"); // 풀 이름에 접미사 추가하여 다른 풀과 구분 용이하게 함
            RecalculateDamage(); // 데미지 및 관련 스탯 계산
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// 투사체 객체 풀을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 풀이 존재하면 PoolManager를 통해 안전하게 파괴
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null; // 참조 해제
            }
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
            damage = currentUpgrade.Damage; // 기본 데미지 설정
            bulletSpreadAngle = currentUpgrade.Spread; // 산탄 각도 설정
            attackDelay = 1f / currentUpgrade.FireRate; // 공격 딜레이(연사 간격) 계산
            bulletSpeed = currentUpgrade.BulletSpeed; // 총알 속도 설정
        }

        /// <summary>
        /// 매 프레임 샷건의 동작을 업데이트합니다.
        /// 재장전 UI 업데이트, 적 감지 확인, 발사 로직 등을 처리합니다.
        /// </summary>
        public override void GunUpdate()
        {
            // 재장전 UI 진행 상태 업데이트
            float reloadProgress = (nextShootTime > lastShootTime) ? (1 - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime)) : 1f;
            AttackButtonBehavior.SetReloadFill(Mathf.Clamp01(reloadProgress));

            // 주변에 적이 없으면 발사 로직을 진행하지 않음
            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            // 다음 발사 가능 시간이 되지 않았거나, 캐릭터가 공격 불가능 상태이면 발사 로직을 진행하지 않음
            if (nextShootTime >= Time.timeSinceLevelLoad) return;
            if (!characterBehaviour.IsAttackingAllowed) return;

            AttackButtonBehavior.SetReloadFill(0); // 발사 직전 재장전 UI 초기화

            // 가장 가까운 적을 향하는 발사 방향 계산 (Y축은 총구 높이로 고정)
            // shootDirection 계산 전에 ClosestEnemyBehaviour null 체크 추가
            if (characterBehaviour.ClosestEnemyBehaviour == null)
            {
                characterBehaviour.SetTargetUnreachable(); // 타겟을 찾을 수 없음으로 처리
                return; // 발사 로직 중단
            }
            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            // 레이캐스트를 사용하여 발사 경로에 적이 있는지, 시야각 내에 있는지 확인
            if (Physics.Raycast(shootPoint.position, shootDirection.normalized, out var hitInfo, 300f, targetLayers)) // 발사 지점에서 발사
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY) // 적 레이어와 충돌했는지 확인
                {
                    if (Vector3.Angle(shootDirection.normalized, transform.forward.normalized) < 40f) // 총구 방향과 적 방향 사이의 각도 확인
                    {
                        characterBehaviour.SetTargetActive(); // 타겟 유효 상태로 설정

                        // 총기 발사 애니메이션 (반동 효과)
                        shootTweenCase.KillActive(); // 이전 애니메이션 중지
                        shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate
                        {
                            shootTweenCase = transform.DOLocalMoveZ(0, 0.15f);
                        });

                        if(shootParticleSystem != null) shootParticleSystem.Play(); // 발사 파티클 재생
                        
                        // 다음 발사 시간 및 마지막 발사 시간 업데이트
                        nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                        lastShootTime = Time.timeSinceLevelLoad;

                        // 발사할 총알 수 결정 (WeaponData 설정에 따름)
                        int bulletsNumber = weapon.GetCurrentUpgrade().BulletsPerShot.Random();
                        Debug.Log($"[ShotgunBehavior] 발사될 총알 수 (bulletsNumber): {bulletsNumber}");

                        // 결정된 총알 수만큼 반복하여 발사
                        for (int i = 0; i < bulletsNumber; i++)
                        {
                            Debug.Log($"[ShotgunBehavior] 루프 {i}: 총알 생성 시도");
                            
                            // 풀에서 총알 오브젝트 가져오기 및 초기 위치/회전 설정
                            GameObject bulletGO = bulletPool.GetPooledObject();
                            if (bulletGO == null)
                            {
                                Debug.LogError($"[ShotgunBehavior] 루프 {i}: bulletPool.GetPooledObject()가 null을 반환했습니다!");
                                continue; // 다음 총알로 넘어감
                            }
                            bulletGO.transform.SetPositionAndRotation(shootPoint.position, Quaternion.LookRotation(shootDirection.normalized)); // 방향을 정확히 설정
                            
                            PlayerBulletBehavior bullet = bulletGO.GetComponent<PlayerBulletBehavior>();
                            if (bullet == null)
                            {
                                Debug.LogError($"[ShotgunBehavior] 루프 {i}: 총알 프리팹에 PlayerBulletBehavior 컴포넌트가 없습니다! 오브젝트명: {bulletGO.name}");
                                bulletGO.SetActive(false); // 사용 불가 오브젝트는 풀로 반환되도록 비활성화
                                continue; 
                            }
                            Debug.Log($"[ShotgunBehavior] 루프 {i}: bullet.gameObject.name = {bullet.gameObject.name}, 활성 상태: {bullet.gameObject.activeSelf}");

                            // 데미지 및 치명타 계산
                            var (damageValue, isCritical) = CalculateFinalDamageWithCrit();
                            float finalDamage = damageValue * characterBehaviour.Stats.BulletDamageMultiplier;
                            
                            // 총알 초기화 (데미지, 속도, 타겟, 비활성화 시간 등)
                            bullet.Init(finalDamage, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                            Debug.Log($"[ShotgunBehavior] 루프 {i}: 총알 초기화 완료, 초기 위치: {bullet.transform.position}");

                            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 플로팅 텍스트 로직 (Null 체크 추가) ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                            if (characterBehaviour.ClosestEnemyBehaviour != null) // ⭐ 조치 방안: ClosestEnemyBehaviour null 체크
                            {
                                Color textColor = isCritical ? critColor : Color.white;
                                FloatingTextController.SpawnFloatingText(
                                    "Hit", 
                                    finalDamage.ToString("F0"), // 데미지를 정수로 표시
                                    characterBehaviour.ClosestEnemyBehaviour.transform.position + Vector3.up * 1.5f, // 텍스트 위치 약간 위로 조정
                                    Quaternion.identity,
                                    isCritical ? 1.2f : 1.0f, // 치명타 시 텍스트 크기 약간 크게
                                    textColor,
                                    isCritical
                                );
                            }
                            else
                            {
                                Debug.LogWarning($"[ShotgunBehavior] 루프 {i}: ClosestEnemyBehaviour is null. 플로팅 텍스트를 생성하지 않습니다.");
                            }
                            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로직 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                            
                            // 총알에 산탄 각도 적용
                            bullet.transform.Rotate(new Vector3(0f, (i == 0 && bulletsNumber == 1) ? 0f : Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f), 0f)); // 단발일 경우 가운데로
                            Debug.Log($"[ShotgunBehavior] 루프 {i}: 총알 회전 적용 후 forward: {bullet.transform.forward}");
                        }

                        characterBehaviour.OnGunShooted(); // 캐릭터 발사 이벤트 호출
                        
                        // 카메라 흔들림 효과
                        VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
                        if(gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
                        
                        // 발사 사운드 재생
                        AudioController.PlaySound(AudioController.AudioClips.shotShotgun);
                    }
                    // else : 시야각 벗어남 (별도 처리 없음)
                }
                else // 적이 아닌 다른 장애물과 충돌
                {
                    characterBehaviour.SetTargetUnreachable(); // 타겟 도달 불가 상태로 설정
                }
            }
            else // 레이캐스트에 아무것도 맞지 않음
            {
                characterBehaviour.SetTargetUnreachable(); // 타겟 도달 불가 상태로 설정
            }
        }

        /// <summary>
        /// 에디터에서 기즈모를 그릴 때 호출됩니다.
        /// 발사 방향을 시각적으로 표시합니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            // 에디터에서만, 필요한 객체가 모두 할당되어 있을 때만 기즈모 표시
            if (Application.isEditor && characterBehaviour != null && characterBehaviour.ClosestEnemyBehaviour != null && shootPoint != null)
            {
                Color defCol = Gizmos.color;
                Gizmos.color = Color.red;
                // shootDirection을 사용하거나, 현재 ClosestEnemyBehaviour 기준으로 다시 계산하여 표시
                Vector3 currentShootDirectionGizmo = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
                Gizmos.DrawLine(shootPoint.position, shootPoint.position + currentShootDirectionGizmo.normalized * 10f); // 10 유닛 길이로 방향 표시
                Gizmos.color = defCol;
            }
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
                bulletPool = null; // 참조 해제
            }
        }

        /// <summary>
        /// 캐릭터 그래픽스의 샷건 홀더 트랜스폼에 총기를 장착시킵니다.
        /// </summary>
        /// <param name="characterGraphics">총기를 장착할 캐릭터 그래픽스 컴포넌트</param>
        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            if (characterGraphics.ShootGunHolderTransform != null) // 홀더 트랜스폼 유효성 검사
            {
                transform.SetParent(characterGraphics.ShootGunHolderTransform);
                transform.ResetLocal(); // 로컬 위치, 회전, 스케일 초기화
            }
            else
            {
                Debug.LogError($"[ShotgunBehavior] PlaceGun: CharacterGraphics에 ShootGunHolderTransform이 할당되지 않았습니다!");
            }
        }

        /// <summary>
        /// 총기를 재장전합니다.
        /// 모든 투사체를 객체 풀로 반환합니다.
        /// </summary>
        public override void Reload()
        {
            // 현재 활성화된 모든 총알을 풀로 반환 (비활성화)
            bulletPool?.ReturnToPoolEverything();
        }
    }
}