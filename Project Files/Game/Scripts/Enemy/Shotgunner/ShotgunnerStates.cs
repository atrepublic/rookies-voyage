// ==============================================
// 📌 ShotgunnerStates.cs
// ✅ 샷건 적 유닛이 사용할 수 있는 상태 enum 정의
// ==============================================

namespace Watermelon.Enemy.Shotgunner
{
    /// <summary>
    /// 샷건 적이 사용할 수 있는 상태
    /// </summary>
    public enum State
    {
        Patrolling,   // 순찰
        Following,    // 추적
        Attacking     // 사격 공격
    }
}
