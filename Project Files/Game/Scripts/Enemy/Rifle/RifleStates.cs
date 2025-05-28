// ==============================================
// 📌 RifleStates.cs
// ✅ 라이플 적 유닛의 상태 enum 정의 스크립트
// ==============================================

namespace Watermelon.Enemy.Rifle
{
    /// <summary>
    /// 라이플 적이 사용할 수 있는 상태
    /// </summary>
    public enum State
    {
        Patrolling,   // 순찰
        Following,    // 추적
        Attacking,    // 사격 공격
        Fleeing       // 도주
    }
}
