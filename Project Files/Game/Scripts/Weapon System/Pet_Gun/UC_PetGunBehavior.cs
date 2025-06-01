// UC_PetGunBehavior.cs (v1.13 - PlayerBulletBehavior.Init 변경사항 반영)
// 📌 펫 전용 무기 시스템 관리 스크립트
// • 펫의 업그레이드 정보에 따라 공격력을 자동 적용합니다
// • 풀링을 통해 UC_PetBulletBehavior 탄환을 효율적으로 관리합니다
// • [추가] 데미지 텍스트 색상을 인스펙터에서 지정 가능
// • PlayerBulletBehavior의 변경된 Init 시그니처를 반영하여 UC_PetBulletBehavior.Init 호출부를 수정합니다.

using UnityEngine;
using Watermelon;                  // PoolManager, PoolGeneric 등
using Watermelon.SquadShooter;     // PetController, UC_PetData, BaseEnemyBehavior, CharacterBehaviour 등

namespace Watermelon.SquadShooter
{
    public class UC_PetGunBehavior : MonoBehaviour
    {
        [Header("=== 사격 설정 ===")]
        [Tooltip("UC_PetBulletBehavior 컴포넌트가 붙어 있는 탄환 프리팹")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("풀에 미리 생성할 탄환 개수")]
        [SerializeField] private int initialPoolSize = 10;

        [Tooltip("탄환 이동 속도 (유닛/초)")]
        [SerializeField] private float bulletSpeed = 20f;

        [Tooltip("초당 발사 횟수 (Fire Rate)")]
        [SerializeField] private float fireRate = 3f;

        [Tooltip("공격 가능한 최대 사거리")]
        [SerializeField] private float attackRange = 15f;

        [Header("=== 무기 트랜스폼 ===")]
        [Tooltip("총신 회전을 담당할 루트 Transform")]
        [SerializeField] private Transform gunRootTransform;

        [Tooltip("탄환 생성(Muzzle) 위치 Transform")]
        [SerializeField] private Transform muzzleTransform;

        [Header("텍스트 색상 설정")]
        [Tooltip("펫이 적을 공격했을 때 출력할 데미지 텍스트 색상")]
        [SerializeField] private Color hitTextColor = new Color(0.3f, 1f, 1f); // 시안색 계열

        // 내부 상태
        private float calculatedDamage; // Init에서 계산된 최종 데미지
        private float nextFireTime;
        private PoolGeneric<UC_PetBulletBehavior> bulletPool;
        private string poolName;
        private PetController ownerPetController; // 네이밍 명확화: 이 총을 소유한 PetController

        /// <summary>
        /// 펫 컨트롤러와 연결하여 무기 시스템을 초기화합니다.
        /// 업그레이드 정보에서 총 데미지를 계산하여 반영하고, 총알 풀을 생성합니다.
        /// </summary>
        public void Init(PetController petCtrl)
        {
            this.ownerPetController = petCtrl;
            if (this.ownerPetController == null)
            {
                Debug.LogError($"[UC_PetGunBehavior] Init: PetController 참조가 null입니다! 무기를 초기화할 수 없습니다.", gameObject);
                this.enabled = false;
                return;
            }

            if (bulletPrefab == null)
            {
                Debug.LogError($"[UC_PetGunBehavior] Init: bulletPrefab이 할당되지 않았습니다! Pet: {ownerPetController.name}", gameObject);
                this.enabled = false;
                return;
            }
            if (bulletPrefab.GetComponent<UC_PetBulletBehavior>() == null)
            {
                 Debug.LogError($"[UC_PetGunBehavior] Init: bulletPrefab '{bulletPrefab.name}'에 UC_PetBulletBehavior 컴포넌트가 없습니다!", gameObject);
                this.enabled = false;
                return;
            }


            // 업그레이드 데미지 적용
            calculatedDamage = CalculateTotalAttackPower(ownerPetController.PetData, ownerPetController.UpgradeLevel);

            // 풀 이름 구성 (펫 인스턴스 ID를 포함하여 고유성 보장)
            poolName = $"{bulletPrefab.name}_PetGun_{ownerPetController.GetInstanceID()}";
            if (PoolManager.HasPool(poolName)) // 기존 풀이 있다면 파괴 (씬 전환 등으로 인해 남아있을 수 있음)
            {
                Debug.LogWarning($"[UC_PetGunBehavior] Init: 기존 풀 '{poolName}'을 제거하고 새로 생성합니다. Pet: {ownerPetController.name}");
                PoolManager.DestroyPool(PoolManager.GetPoolByName(poolName));
            }

            // 새 풀 생성
            bulletPool = new PoolGeneric<UC_PetBulletBehavior>(
                bulletPrefab,
                poolName,
                muzzleTransform // 총알 생성 시 부모 컨테이너로 총구 지정 (선택적)
            );
            bulletPool.CreatePoolObjects(initialPoolSize);

            nextFireTime = Time.time; // 즉시 발사 가능하도록
            Debug.Log($"[UC_PetGunBehavior] Init 완료. Pet: {ownerPetController.name}, Pool: {poolName}, Damage: {calculatedDamage}");
        }

        /// <summary>
        /// 펫 데이터와 현재 업그레이드 레벨을 기반으로 총 공격력을 계산합니다.
        /// </summary>
        private float CalculateTotalAttackPower(UC_PetData data, int level)
        {
            if (data == null)
            {
                Debug.LogError("[UC_PetGunBehavior] CalculateTotalAttackPower: UC_PetData가 null입니다!");
                return 0f;
            }
            float total = data.baseAttackPower;
            if (data.upgrades != null) // null 체크 추가
            {
                for (int i = 0; i < level && i < data.upgrades.Count; i++) // level이 Count를 넘지 않도록 방어
                {
                    total += data.upgrades[i].attackPowerIncrease;
                }
            }
            return total;
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출됩니다. 생성된 총알 풀을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (bulletPool != null)
            {
                PoolManager.DestroyPool(bulletPool);
                bulletPool = null;
            }
        }

        /// <summary>
        /// 펫의 타겟이 유효하고 발사 쿨다운이 지났다면 공격(총알 발사)을 실행합니다.
        /// </summary>
        public void TryFire()
        {
            if (ownerPetController == null || bulletPool == null)
            {
                // Debug.LogWarning("[UC_PetGunBehavior] TryFire: ownerPetController 또는 bulletPool이 null입니다.");
                return;
            }
            if (Time.time < nextFireTime) return;

            BaseEnemyBehavior targetEnemy = ownerPetController.CurrentTarget;
            if (targetEnemy == null || targetEnemy.IsDead) return;

            float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);
            if (distanceToTarget > attackRange) return;

            nextFireTime = Time.time + 1f / fireRate;

            UC_PetBulletBehavior bullet = bulletPool.GetPooledComponent();
            if (bullet == null)
            {
                Debug.LogWarning($"[UC_PetGunBehavior] TryFire: bulletPool '{poolName}'에서 UC_PetBulletBehavior를 가져올 수 없습니다.");
                return;
            }

            if (muzzleTransform == null)
            {
                Debug.LogError("[UC_PetGunBehavior] TryFire: muzzleTransform이 할당되지 않았습니다!", gameObject);
                return;
            }
            bullet.transform.SetPositionAndRotation(
                muzzleTransform.position,
                muzzleTransform.rotation
            );

            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ UC_PetBulletBehavior.Init 호출 수정 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // UC_PetBulletBehavior의 Init(float, float, BaseEnemyBehavior, Color) 오버로드를 사용합니다.
            // 이 오버로드 내부에서 PlayerBulletBehavior.Init에 필요한 나머지 인자(isCritFromGun, owner)를 채웁니다.
            bullet.Init(calculatedDamage, bulletSpeed, targetEnemy, hitTextColor);
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }

        // 인스펙터에서 참조 또는 디버깅 목적으로 사용될 수 있는 프로퍼티
        public float FireRate => fireRate;
        public float GetAttackRange() => attackRange; // 메서드 형태 유지
        public Transform GunRoot => gunRootTransform;
    }
}