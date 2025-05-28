/*
 * IEnemyDetector.cs
 * ---------------------
 * 이 인터페이스는 EnemyDetector 컴포넌트와 상호작용하는 클래스가 구현해야 할 메서드를 정의합니다.
 * 주로 가장 가까운 적이 변경되었을 때 EnemyDetector가 해당 정보를 전달하기 위해 사용됩니다.
 */

namespace Watermelon.SquadShooter
{
    // 적 감지 이벤트 수신을 위한 인터페이스 정의
    public interface IEnemyDetector
    {
        /// <summary>
        /// 감지된 가장 가까운 적이 변경되었을 때 호출됩니다.
        /// </summary>
        /// <param name="enemyBehavior">새롭게 가장 가까워진 적 (없으면 null)</param>
        void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior);
    }
}