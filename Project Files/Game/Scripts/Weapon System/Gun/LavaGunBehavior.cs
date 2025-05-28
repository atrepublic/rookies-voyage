// 이 스크립트는 용암 총의 동작을 정의합니다.
// 기본 총기 동작을 상속받으며, 용암 투사체 발사 로직, 목표 추적, 파티클 및 사운드 효과를 구현합니다.
using UnityEngine;
using Watermelon; // Watermelon 네임스페이스의 다른 기능(예: Pool, PoolManager, Tween, ParticlesController, AudioController)을 사용하기 위해 필요합니다.
using Watermelon.LevelSystem; // ActiveRoom 사용을 위해 필요합니다.
// using DG.Tweening; 네임스페이스는 DOTween 플러그인 사용에 필요하지만, 현재 코드에 명시적으로 포함되어 있지 않습니다.
// Tweening 관련 기능(DOLocalMoveZ, OnComplete, SetEasing 등)은 해당 플러그인이 프로젝트에 추가되어 있음을 가정합니다.

namespace Watermelon.SquadShooter
{
    // BaseGunBehavior를 상속받아 기본 총기 기능을 활용합니다.
    public class LavaGunBehavior : BaseGunBehavior
    {
        [LineSpacer] // 인스펙터에 라인 구분자를 추가하는 사용자 정의 속성일 수 있습니다.
        [Tooltip("총기의 그래픽스 부분을 나타내는 트랜스폼입니다 (예: 발사 애니메이션에 사용).")]
        [SerializeField] Transform graphicsTransform;
        [Tooltip("총기 발사 시 재생될 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem shootParticleSystem;

        [Tooltip("투사체가 명중할 수 있는 대상 레이어 마스크입니다 (예: 적, 장애물).")]
        [SerializeField] LayerMask targetLayers;
        [Tooltip("용암 투사체 폭발 시 피해를 줄 반경입니다.")]
        [SerializeField] float explosionRadius;
        [Tooltip("용암 투사체의 곡선 이동 높이 범위 (최소/최대 값)입니다.")]
        [SerializeField] DuoFloat bulletHeight; // DuoFloat는 두 개의 float 값을 저장하는 사용자 정의 구조체일 수 있습니다.

        // 총기 발사 사이의 지연 시간 (공격 속도에 반비례)입니다.
        private float attackDelay;
        // 용암 투사체의 이동 속도 범위 (최소/최대 값)입니다.
        private DuoFloat bulletSpeed; // DuoFloat는 두 개의 float 값을 저장하는 사용자 정의 구조체일 수 있습니다.

        // 다음 발사가 가능한 시간입니다.
        private float nextShootTime;
        // 마지막 발사 시간입니다. (재장전 UI 등에 사용될 수 있습니다)
        private float lastShootTime;

        // 용암 투사체 객체 풀입니다.
        private Pool bulletPool;

        // 총기 그래픽스 이동 애니메이션을 제어하는 TweenCase 객체입니다.
        private TweenCase shootTweenCase;

        // 캐릭터의 적 감지 반경 (발사 가능 사거리로 사용될 수 있습니다).
        private float shootingRadius;


        // 추가: 치명타 텍스트 색상
        private static readonly Color critColor = new Color(1f, 0.4f, 0f);

        /// <summary>
        /// 용암 총의 동작을 초기화합니다.
        /// 기본 총기 정보 설정 후 투사체 풀을 생성하고 발사 사거리를 설정하며 데미지를 계산합니다.
        /// </summary>
        /// <param name="characterBehaviour">총기를 장착할 캐릭터 행동 컴포넌트</param>
        /// <param name="weapon">이 총기의 무기 데이터</param>
        public override void Init(CharacterBehaviour characterBehaviour, WeaponData weapon)
        {
            // 상위 클래스의 Init 함수를 호출하여 기본 총기 속성을 설정합니다.
            base.Init(characterBehaviour, weapon);

            // 현재 무기 데이터의 강화 상태를 가져옵니다.
            WeaponUpgrade currentUpgrade = weapon.GetCurrentUpgrade();
            // 현재 강화 단계의 투사체 프리팹을 가져옵니다.
            GameObject bulletObj = currentUpgrade.BulletPrefab;

            // 투사체 프리팹을 사용하여 객체 풀을 생성합니다.
            bulletPool = new Pool(bulletObj, bulletObj.name);

            // 캐릭터의 적 감지 반경을 발사 사거리로 설정합니다.
            shootingRadius = characterBehaviour.EnemyDetector.DetectorRadius;

            // 데미지 및 관련 스탯을 다시 계산합니다.
            RecalculateDamage();
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// 투사체 객체 풀을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 투사체 객체 풀이 존재하면 파괴합니다.
            if (bulletPool != null)
                PoolManager.DestroyPool(bulletPool);
        }

        /// <summary>
        /// 레벨이 로드될 때 호출될 수 있는 함수입니다.
        /// 데미지 및 관련 스탯을 다시 계산합니다.
        /// </summary>
        public override void OnLevelLoaded()
        {
            // 현재 무기 강화 상태에 따라 데미지 및 관련 스탯을 다시 계산합니다.
            RecalculateDamage();
        }

        /// <summary>
        /// 현재 무기 강화 상태에 따라 총기의 데미지 및 관련 스탯(공격 속도, 투사체 속도)을 다시 계산합니다.
        /// </summary>
        public override void RecalculateDamage()
        {
            // 현재 무기 데이터의 강화 상태를 가져옵니다.
            WeaponUpgrade upgrade = weapon.GetCurrentUpgrade();

            // 강화 단계 데이터에서 데미지, 공격 속도, 투사체 속도를 가져와 설정합니다.
            damage = upgrade.Damage;
            attackDelay = 1f / upgrade.FireRate;
            bulletSpeed = upgrade.BulletSpeed;
        }

        /// <summary>
        /// 매 프레임 용암 총의 동작을 업데이트합니다.
        /// 재장전 UI 업데이트, 적 감지 확인, 발사 로직 등을 처리합니다.
        /// </summary>
        public override void GunUpdate()
        {
            // 공격 버튼의 재장전 UI를 업데이트합니다.
            AttackButtonBehavior.SetReloadFill(1 - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime));

            // 캐릭터 주변에 가까운 적이 없으면 발사 로직을 실행하지 않습니다.
            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            // 다음 발사 시간이 아직 되지 않았거나 캐릭터의 공격이 허용되지 않으면 발사하지 않습니다.
            if (nextShootTime >= Time.timeSinceLevelLoad || !characterBehaviour.IsAttackingAllowed)
                return;

            // 재장전 UI를 0으로 초기화합니다.
            AttackButtonBehavior.SetReloadFill(0);

            // 가장 가까운 적의 위치를 향하는 발사 방향 벡터를 계산합니다 (Y축은 발사 지점의 Y축 사용).
            var shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position; // SetY()는 사용자 정의 확장 함수일 수 있습니다.
            // 레이캐스트 발사 원점을 발사 지점보다 약간 뒤쪽으로 설정합니다.
            var origin = shootPoint.position - shootDirection.normalized * 1.5f;

            // 발사 원점에서 발사 방향으로 레이캐스트를 발사하여 적 또는 장애물에 명중하는지 확인합니다.
            if (Physics.Raycast(origin, shootDirection, out var hitInfo, 300f, targetLayers) && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                // 발사 방향이 총기의 앞 방향과 크게 다르지 않으면 (40도 이내)
                if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                {
                    // 캐릭터의 목표를 유효한 상태로 설정합니다.
                    characterBehaviour.SetTargetActive();

                    // 현재 실행 중인 총기 그래픽스 이동 트윈 애니메이션을 중지합니다.
                    shootTweenCase.KillActive();

                    // 총기 그래픽스를 짧은 시간 동안 앞으로 이동시키고, 완료되면 원래 위치로 돌아오는 애니메이션을 실행합니다.
                    shootTweenCase = graphicsTransform.DOLocalMoveZ(-0.15f, attackDelay * 0.1f).OnComplete(delegate
                    {
                        shootTweenCase = graphicsTransform.DOLocalMoveZ(0, attackDelay * 0.15f);
                    });

                    // 발사 파티클 시스템을 재생합니다.
                    shootParticleSystem.Play();
                    // 다음 발사 가능 시간을 업데이트합니다.
                    nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                    // 마지막 발사 시간을 업데이트합니다.
                    lastShootTime = Time.timeSinceLevelLoad;

                    // 현재 강화 단계에서 발사될 투사체 수를 무작위로 가져옵니다.
                    int bulletsNumber = weapon.GetCurrentUpgrade().BulletsPerShot.Random(); // Random()은 DuoInt에 대한 사용자 정의 확장 함수일 수 있습니다.

                    // 발사될 투사체 수만큼 투사체를 생성하고 초기화합니다.
                    for (int i = 0; i < bulletsNumber; i++)
                    {
                        // 투사체 풀에서 객체를 가져와 위치와 회전을 설정하고 LavaBulletBehavior 컴포넌트를 가져옵니다.
                        LavaBulletBehavior bullet = bulletPool.GetPooledObject().SetPosition(shootPoint.position).SetEulerAngles(shootPoint.eulerAngles).GetComponent<LavaBulletBehavior>(); // SetPosition(), SetEulerAngles()는 사용자 정의 확장 함수일 수 있습니다.

                    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 치명타 및 플로팅 텍스트 로직 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                    // [치명타 데미지 및 여부 계산]
                    // damageToDisplayForText: 치명타/일반 공격에 따른 데미지 (텍스트 표시용)
                    // isCritical: 치명타 발생 여부
                    var (damageToDisplayForText, isCritical) = CalculateFinalDamageWithCrit();

                    // [최종 데미지 계산] - 캐릭터 스탯의 BulletDamageMultiplier 적용
                    float finalDamageForText = damageToDisplayForText * characterBehaviour.Stats.BulletDamageMultiplier;

                    // [투사체 초기화]
                    // LavaBulletBehavior의 Init은 기존처럼 weapon.GetCurrentUpgrade().Damage (DuoInt)를 사용할 수 있습니다.
                    // 또는, Init 메서드를 수정하여 finalDamageForText (float)나 isCritical (bool)을 받아
                    // 투사체 자체의 폭발 데미지 등에 치명타를 반영할 수도 있습니다.
                    // 여기서는 플로팅 텍스트만 치명타를 반영하고, 투사체 Init은 기존 damage 필드(DuoInt)를 사용하도록 유지합니다.
                    // 만약 투사체의 실제 폭발 데미지 등에도 치명타를 반영하려면 LavaBulletBehavior.Init 및 관련 로직 수정이 필요합니다.
                    bullet.Init(this.damage, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, -1f, false, shootingRadius, characterBehaviour, bulletHeight, explosionRadius);

                    // [플로팅 텍스트 출력]
                    Color textColor = isCritical ? critColor : Color.white;
                    FloatingTextController.SpawnFloatingText(
                        "Hit", // 다른 총기와 일관되게 "Hit" 사용 (또는 프로젝트에 맞게 통일된 이름)
                        finalDamageForText.ToString(), // 실제 표시될 데미지 (치명타 반영)
                        characterBehaviour.ClosestEnemyBehaviour.transform.position, // 텍스트 생성 위치
                        Quaternion.identity,
                        1.0f, // scaleMultiplier
                        textColor,
                        isCritical
                    );

                        // 용암 투사체를 초기화합니다.
                        //bullet.Init(damage, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, -1f, false, shootingRadius, characterBehaviour, bulletHeight, explosionRadius); // Random()은 DuoFloat에 대한 사용자 정의 확장 함수일 수 있습니다.
                    }

                    // 캐릭터에게 총을 발사했음을 알립니다.
                    characterBehaviour.OnGunShooted();

                    // 게임 카메라를 가져와서 약한 흔들림 효과를 적용합니다.
                    VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game); // CameraController는 사용자 정의 클래스일 수 있습니다.
                    gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                    // 용암 총 발사 사운드를 재생합니다.
                    AudioController.PlaySound(AudioController.AudioClips.shotLavagun, 0.8f); // AudioController.AudioClips는 사용자 정의 열거형일 수 있습니다.
                }
            }
            else
            {
                // 레이캐스트에 적이 명중하지 않으면 캐릭터의 목표를 도달 불가능 상태로 설정합니다.
                characterBehaviour.SetTargetUnreachable();
            }
        }

        /// <summary>
        /// 총기가 해제될 때 호출됩니다.
        /// 투사체 객체 풀을 파괴합니다.
        /// </summary>
        public override void OnGunUnloaded()
        {
            // 투사체 객체 풀이 존재하면 파괴하고 null로 설정합니다.
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool); // PoolManager는 사용자 정의 클래스일 수 있습니다.

                bulletPool = null;
            }
        }

        /// <summary>
        /// 캐릭터 그래픽스의 로켓 홀더 트랜스폼에 총기를 장착시킵니다.
        /// </summary>
        /// <param name="characterGraphics">총기를 장착할 캐릭터 그래픽스 컴포넌트</param>
        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            // 총기의 부모를 캐릭터 그래픽스의 로켓 홀더 트랜스폼으로 설정하고 로컬 위치/회전/스케일을 초기화합니다.
            transform.SetParent(characterGraphics.RocketHolderTransform);
            transform.ResetLocal(); // ResetLocal()은 사용자 정의 확장 함수일 수 있습니다.
        }

        /// <summary>
        /// 총기를 재장전합니다.
        /// 모든 투사체를 객체 풀로 반환합니다.
        /// </summary>
        public override void Reload()
        {
            // 투사체 풀의 모든 활성화된 객체를 풀로 반환합니다.
            bulletPool.ReturnToPoolEverything();
        }
    }
}