// UC_PetBulletBehavior.cs
// 📌 펫 전용 투사체 스크립트
// • PlayerBulletBehavior를 상속해 펫용 Init 오버로드만 노출
// • 자동 비활성화 시간(autoDisableTime)과 자동 비활성화 여부(autoDisableOnHit)를 내부에서 고정
// • 펫 전용 추가 기능(이펙트, 사운드 등)을 넣을 수 있는 기반 클래스
// • PlayerBulletBehavior의 변경된 Init 시그니처 및 필드명을 따르도록 수정되었습니다.

using UnityEngine;
using Watermelon; // FloatingTextController 등 Watermelon 프레임워크 사용

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 펫이 발사하는 투사체의 특정 동작을 정의합니다.
    /// PlayerBulletBehavior를 상속받아 기본적인 투사체 기능을 활용하며,
    /// 펫 전용 초기화 메서드 및 명중 시 효과를 제공합니다.
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class UC_PetBulletBehavior : PlayerBulletBehavior
    {
        [Tooltip("펫 투사체가 적에게 명중했을 때 표시될 플로팅 텍스트의 기본 색상입니다.")]
        [SerializeField] private Color floatingTextColor = Color.cyan; // 기본 색상을 인스펙터에서 설정 가능하도록 변경

        // 👇 이 프로퍼티를 추가하거나 확인합니다.
        /// <summary>
        /// 이 펫 총알이 명중했을 때 사용할 플로팅 텍스트 색상입니다.
        /// </summary>
        public Color PetHitFloatingTextColor => floatingTextColor;

        // 펫은 플레이어와 다른 치명타 로직을 가질 수 있거나, 치명타가 없을 수 있습니다.
        // 여기서는 펫 총알은 치명타가 없다고 가정하고 isCritFromGun을 false로 전달합니다.
        // 펫의 'owner'는 PetController가 될 수 있으나, PlayerBulletBehavior의 owner는 CharacterBehaviour 타입입니다.
        // 펫의 공격 주체가 CharacterBehaviour가 아니라면, null을 전달하거나 PlayerBulletBehavior를 수정해야 합니다.
        // 현재는 펫의 공격 주체(owner) CharacterBehaviour가 없다고 가정하고 null을 전달합니다.
        // (만약 펫이 플레이어의 스탯(예: 치명타 확률)을 일부 공유한다면 projectileOwner를 전달해야 함)

        /// <summary>
        /// 펫 투사체를 초기화하는 간소화된 오버로드 메서드입니다. (색상 포함)
        /// 자동 비활성화 시간은 0(무한), 충돌 시 자동 비활성화는 true로 고정됩니다.
        /// 펫 공격은 치명타가 없다고 가정하고, 발사 주체(owner)는 null로 설정합니다.
        /// </summary>
        /// <param name="damageAmount">투사체의 데미지 값</param>
        /// <param name="bulletSpeed">투사체의 이동 속도</param>
        /// <param name="targetEnemy">투사체의 초기 목표 적</param>
        /// <param name="textColor">플로팅 데미지 텍스트의 색상</param>
        public void Init(float damageAmount, float bulletSpeed, BaseEnemyBehavior targetEnemy, Color textColor)
        {
            this.floatingTextColor = textColor; // 전달받은 색상으로 설정

            // PlayerBulletBehavior.Init 호출:
            // autoDisableTime: 0f (펫 총알은 보통 즉시 사라지거나 다른 조건으로 사라짐)
            // autoDisableOnHit: true (명중 시 비활성화)
            // isCritFromGun: false (펫은 치명타가 없다고 가정)
            // owner: null (펫은 CharacterBehaviour 주체가 아님. 만약 펫도 CharacterBehaviour를 가진다면 해당 참조 전달)
            base.Init(damageAmount, bulletSpeed, targetEnemy, 0f, true, false, null);
        }

        /// <summary>
        /// 펫 투사체를 초기화하는 기본 오버로드 메서드입니다. (색상 기본값 사용)
        /// </summary>
        /// <param name="damageAmount">투사체의 데미지 값</param>
        /// <param name="bulletSpeed">투사체의 이동 속도</param>
        /// <param name="targetEnemy">투사체의 초기 목표 적</param>
        public void Init(float damageAmount, float bulletSpeed, BaseEnemyBehavior targetEnemy)
        {
            // 기본 색상(인스펙터 또는 클래스 기본값)을 사용하여 다른 Init 오버로드 호출
            Init(damageAmount, bulletSpeed, targetEnemy, this.floatingTextColor);
        }

        /// <summary>
        /// 적에게 명중했을 때 호출되는 콜백입니다. (PlayerBulletBehavior.OnTriggerEnter 내부에서 호출됨)
        /// 펫 전용 플로팅 텍스트를 생성합니다.
        /// </summary>
        /// <param name="enemyHit">이번에 명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior enemyHit)
        {
            // PlayerBulletBehavior.OnTriggerEnter에서 이미 데미지 적용 및 기본 플로팅 텍스트 생성을
            // 시도했을 수 있습니다. 펫 전용 텍스트를 여기서 별도로 띄우려면,
            // PlayerBulletBehavior.OnTriggerEnter의 플로팅 텍스트 생성 로직을 조건부로 만들거나,
            // 여기서 생성하는 텍스트가 중복되지 않도록 주의해야 합니다.

            // 현재 PlayerBulletBehavior.OnTriggerEnter는 모든 타격에 대해 "Hit" 유형의 플로팅 텍스트를
            // 생성하려고 시도합니다. 펫 총알의 경우, 그 텍스트의 색상이나 내용을 여기서 다르게 하고 싶을 수 있습니다.
            // 만약 PlayerBulletBehavior에서 생성되는 텍스트를 사용하지 않고 여기서 완전히 새로 생성하려면,
            // PlayerBulletBehavior.OnTriggerEnter의 텍스트 생성 부분에 "ownerCharacterBehaviour == null" (즉, 펫 총알인 경우)이면
            // 텍스트를 생성하지 않는 조건을 추가하는 방법이 있습니다.

            // 여기서는 PlayerBulletBehavior에서 생성되는 텍스트와 별개로, 또는 그것을 대체하여
            // 펫 전용 텍스트를 띄운다고 가정합니다.
            // 하지만 PlayerBulletBehavior.OnTriggerEnter에서 이미 텍스트를 띄우므로, 이 부분은 중복될 수 있습니다.
            // 가장 좋은 방법은 PlayerBulletBehavior.OnTriggerEnter가 펫 총알을 구분하여 텍스트를 생성하지 않도록 하는 것입니다.
            // (그 수정이 없다면 아래 코드는 중복 텍스트를 유발합니다.)

            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ CS0103 오류 수정 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // PlayerBulletBehavior의 damage 필드명이 currentDamage로 변경되었으므로 이를 사용합니다.
            // 또한, 펫의 데미지 표시는 보통 양수로 하므로 "-" 부호는 제거하거나 게임 디자인에 맞게 조정합니다.
            // 이 데미지는 Init을 통해 전달받은 값입니다.
            string damageText = this.currentDamage.ToString("F0");
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // FloatingTextController의 빈도 조절 기능이 있는 SpawnFloatingText 호출
            // subjectOfText로 피격된 적(enemyHit.gameObject)을 전달
            // 펫 공격은 치명타가 없다고 가정하고 isCritical을 false로 전달
            FloatingTextController.SpawnFloatingText(
                "Hit", // 플로팅 텍스트 종류 이름 (또는 펫 전용 "PetHit" 등 정의 가능)
                damageText,
                enemyHit.transform.position + Vector3.up * 1.3f, // 텍스트 위치 조정
                Quaternion.identity,
                1.0f, // 기본 스케일
                this.floatingTextColor, // Init에서 설정된 색상 사용
                false, // 펫 공격은 치명타가 없다고 가정
                enemyHit.gameObject // 빈도 조절 대상
            );
        }

        // OnObstacleHitted는 PlayerBulletBehavior의 기본 동작을 따르거나, 필요시 오버라이드하여
        // 펫 총알 전용 장애물 충돌 효과를 추가할 수 있습니다.
        // protected override void OnObstacleHitted()
        // {
        //     base.OnObstacleHitted();
        //     // 펫 총알 전용 장애물 충돌 효과 (예: 다른 파티클)
        // }
    }
}