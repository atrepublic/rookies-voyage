// ==============================================
// 📌 CowboyStates.cs
// ✅ 카우보이 적 AI가 가질 수 있는 상태 목록 정의
// ✅ 상태머신에서 사용하는 enum
// ==============================================

namespace Watermelon.Enemy.Cowboy
{
    /// <summary>
    /// 카우보이 적의 상태 종류
    /// </summary>
    public enum State
    {
        Patrolling,   // 순찰 중
        Following,    // 플레이어 추적 중
        Attacking,    // 공격 중
        Fleeing       // 도주 중
    }
}
