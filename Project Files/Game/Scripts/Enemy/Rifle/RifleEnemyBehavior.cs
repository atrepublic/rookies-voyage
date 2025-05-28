// ==============================================
// 📌 RifleEnemyBehavior.cs
// ✅ 라이플 적 유닛의 공격 행동 처리용 스크립트
// ✅ 현재 상태에서는 기능 미정 상태로 FixedUpdate만 구현됨
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class RifleEnemyBehavior : BaseEnemyBehavior
    {
        /// <summary>
        /// 📌 체력바 위치 실시간 업데이트
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            healthbarBehaviour.FollowUpdate();
        }

        /// <summary>
        /// 📌 공격 트리거 (구현 예정)
        /// </summary>
        public override void Attack()
        {
            // 추후 구현 예정
        }

        /// <summary>
        /// 📌 애니메이션 이벤트 콜백 처리 (구현 예정)
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            // 추후 구현 예정
        }
    }
}
