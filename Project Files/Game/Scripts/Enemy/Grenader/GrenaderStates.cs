// ==============================================
// 📌 GrenaderStates.cs
// ✅ 수류탄 던지는 적 유닛(Grenader)의 상태 enum 정의
// ==============================================

namespace Watermelon.Enemy.Grenader
{
    /// <summary>
    /// 그레네이더 적이 가질 수 있는 상태 열거형
    /// </summary>
    public enum State
    {
        Patrolling,  // 순찰 중
        Following,   // 추적 중
        Attacking,   // 수류탄 공격
        Fleeing      // 도주 중
    }
}
