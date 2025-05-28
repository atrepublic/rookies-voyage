// UC_PetGunBehavior.cs (v1.12)
// 📌 펫 전용 무기 시스템 관리 스크립트
// • 펫의 업그레이드 정보에 따라 공격력을 자동 적용합니다
// • 풀링을 통해 UC_PetBulletBehavior 탄환을 효율적으로 관리합니다
// • [추가] 데미지 텍스트 색상을 인스펙터에서 지정 가능

using UnityEngine;
using Watermelon;                  // PoolManager

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
        [SerializeField] private Color hitTextColor = new Color(0.3f, 1f, 1f);

        // 내부 상태
        private float damage;
        private float nextFireTime;
        private PoolGeneric<UC_PetBulletBehavior> bulletPool;
        private string poolName;
        private PetController controller;

        /// <summary>
        /// 펫 컨트롤러와 연결하여 무기 시스템을 초기화합니다.
        /// 업그레이드 정보에서 총 데미지를 계산하여 반영합니다.
        /// </summary>
        public void Init(PetController controller)
        {
            this.controller = controller;

            // 업그레이드 데미지 적용
            damage = CalculateTotalAttackPower(controller.PetData, controller.UpgradeLevel);

            // 풀 이름 구성 및 기존 풀 제거
            poolName = $"{bulletPrefab.name}_PetGun_{controller.GetInstanceID()}";
            if (PoolManager.HasPool(poolName))
                PoolManager.DestroyPool(PoolManager.GetPoolByName(poolName));

            // 새 풀 생성
            bulletPool = new PoolGeneric<UC_PetBulletBehavior>(
                bulletPrefab,
                poolName,
                muzzleTransform
            );
            bulletPool.CreatePoolObjects(initialPoolSize);

            // 발사 쿨다운 초기화
            nextFireTime = Time.time;
        }

        /// <summary>
        /// 업그레이드 레벨을 기반으로 총 공격력을 누적 계산합니다.
        /// </summary>
        private float CalculateTotalAttackPower(UC_PetData data, int level)
        {
            float total = data.baseAttackPower;
            if (data.upgrades == null) return total;
            for (int i = 0; i < level && i < data.upgrades.Count; i++)
            {
                total += data.upgrades[i].attackPowerIncrease;
            }
            return total;
        }

        private void OnDestroy()
        {
            if (bulletPool != null)
                PoolManager.DestroyPool(bulletPool);
        }

        /// <summary>
        /// 펫의 타겟이 유효하고 쿨타임이 지났다면 공격을 실행합니다.
        /// </summary>
        public void TryFire()
        {
            if (controller == null || bulletPool == null) return;
            if (Time.time < nextFireTime) return;

            var target = controller.CurrentTarget;
            if (target == null || target.IsDead) return;

            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist > attackRange) return;

            nextFireTime = Time.time + 1f / fireRate;

            UC_PetBulletBehavior bullet = bulletPool.GetPooledComponent();
            if (bullet == null) return;

            bullet.transform.SetPositionAndRotation(
                muzzleTransform.position,
                muzzleTransform.rotation
            );

            // [수정] 데미지 텍스트 색상도 함께 전달
            bullet.Init(damage, bulletSpeed, target, hitTextColor);
        }

        public float FireRate => fireRate;
        public float GetAttackRange() => attackRange;
        public Transform GunRoot => gunRootTransform;
    }
}
