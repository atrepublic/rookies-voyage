// Assets/Scripts/Pet/StateMachine/UC_PetStateMachine.cs
// ────────────────────────────────────────────────────
// 📌 펫 상태 전환 및 업데이트를 관리하는 매니저 클래스
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class UC_PetStateMachine
    {
        private UC_PetBaseState currentState;
        private readonly PetController controller;

        /// <summary>생성자: PetController 인스턴스를 전달받아 초기화</summary>
        public UC_PetStateMachine(PetController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// 상태 전환: 이전 상태 Exit() → 새 상태 Enter() 호출
        /// </summary>
        public void SetState(UC_PetBaseState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        /// <summary>매 프레임 현재 상태의 Update() 호출</summary>
        public void Update()
        {
            currentState?.Update();
        }
    }
}
