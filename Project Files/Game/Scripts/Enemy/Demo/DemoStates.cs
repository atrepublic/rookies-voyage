// ==============================================
// 📌 DemoStates.cs
// ✅ 데모 적 유닛의 상태(enum)를 정의하는 열거형 스크립트
// ==============================================

namespace Watermelon.Enemy.Demo
{
    /// <summary>
    /// 데모 적 AI가 가질 수 있는 상태
    /// </summary>
    public enum State
    {
        Patrolling,  // 순찰 중
        Following,   // 추적 중
        Attacking    // 공격 준비/진행 중
    }
}
