// ==============================================
// 📌 IControlBehavior.cs
// ✅ 입력 컨트롤러(키보드, 패드 등)에서 구현해야 하는 인터페이스
// ✅ 입력값 처리, 이동 활성/비활성 제어 및 이벤트 연결 용도
// ==============================================

using UnityEngine;

namespace Watermelon
{
    public interface IControlBehavior
    {
        /// <summary>
        /// 📌 현재 이동 입력 벡터 (WASD 또는 조이스틱 방향)
        /// </summary>
        public Vector3 MovementInput { get; }

        /// <summary>
        /// 📌 현재 이동 입력이 존재하는지 여부
        /// </summary>
        public bool IsMovementInputNonZero { get; }

        /// <summary>
        /// 📌 이동 입력 허용
        /// </summary>
        public void EnableMovementControl();

        /// <summary>
        /// 📌 이동 입력 차단
        /// </summary>
        public void DisableMovementControl();

        /// <summary>
        /// 📌 입력 상태 초기화
        /// </summary>
        public void ResetControl();

        /// <summary>
        /// 📌 처음 입력이 들어왔을 때 발생하는 콜백 이벤트
        /// </summary>
        public event SimpleCallback OnMovementInputActivated;
    }
}
