// ==============================================
// 📌 IDistanceToggle.cs
// ✅ DistanceToggle 시스템에 사용되는 인터페이스 정의
// ✅ 특정 거리 안팎으로 진입/이탈 시 이벤트를 발생시키기 위한 인터페이스
// ==============================================

using UnityEngine;

namespace Watermelon
{
    public interface IDistanceToggle
    {
        /// <summary>
        /// 현재 활성화(갱신 대상 포함) 상태인지
        /// </summary>
        public bool IsShowing { get; }

        /// <summary>
        /// 현재 화면에 표시되고 있는 상태인지
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// 토글 활성화/비활성화에 기준이 되는 거리
        /// </summary>
        public float ShowingDistance { get; }

        /// <summary>
        /// 거리 비교 기준이 되는 위치
        /// </summary>
        public Vector3 DistancePointPosition { get; }

        /// <summary>
        /// 플레이어가 토글 범위 안으로 들어왔을 때 호출
        /// </summary>
        public void PlayerEnteredZone();

        /// <summary>
        /// 플레이어가 토글 범위를 벗어났을 때 호출
        /// </summary>
        public void PlayerLeavedZone();
    }
}
