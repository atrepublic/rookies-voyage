// UC_PetBulletBehavior.cs
// 📌 펫 전용 투사체 스크립트
// • PlayerBulletBehavior를 상속해 펫용 Init 오버로드만 노출
// • 자동 비활성화 시간(autoDisableTime)과 자동 비활성화 여부(autoDisableOnHit)를 내부에서 고정
// • 펫 전용 추가 기능(이펙트, 사운드 등)을 넣을 수 있는 기반 클래스

using UnityEngine;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class UC_PetBulletBehavior : PlayerBulletBehavior
    {
        private Color floatingTextColor = Color.cyan; // [추가] 펫 데미지 텍스트 색상

        /// <summary>
        /// 펫 투사체 초기화 간소화 오버로드 (색상 포함)
        /// </summary>
        public void Init(float damage, float speed, BaseEnemyBehavior target, Color textColor)
        {
            floatingTextColor = textColor;
            base.Init(damage, speed, target, 0f, true);
        }

        /// <summary>
        /// 펫 투사체 초기화 기본 오버로드 (색상 기본값 사용)
        /// </summary>
        public void Init(float damage, float speed, BaseEnemyBehavior target)
        {
            Init(damage, speed, target, Color.cyan);
        }

        /// <summary>
        /// 적에게 명중했을 때 호출되는 콜백
        /// </summary>
        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            FloatingTextController.SpawnFloatingText(
                "Hit",
                "-" + damage.ToString("F0"),
                baseEnemyBehavior.transform.position + Vector3.up * 1.3f,
                Quaternion.identity,
                1.0f,
                floatingTextColor
            );
        }
    }
}