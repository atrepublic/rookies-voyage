// ==============================================
// 📌 RangeStates.cs
// ✅ 원거리 적(Ranged Enemy)의 상태 enum 정의 스크립트
// ==============================================

namespace Watermelon.Enemy.Range
{
    /// <summary>
    /// 원거리 적이 사용할 수 있는 상태 목록
    /// </summary>
    public enum State
    {
        Patrolling,   // 순찰
        Following,    // 추적
        Attacking,    // 공격
        Fleeing       // 도주
    }
}
