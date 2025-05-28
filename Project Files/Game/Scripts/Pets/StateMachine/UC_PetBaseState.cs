// Assets/Scripts/Pet/StateMachine/UC_PetBaseState.cs
// ────────────────────────────────────────────────────
// 📌 펫 상태 머신의 기본 추상 클래스
//    상태 진입(Enter), 매 프레임(Update), 상태 종료(Exit) 로직 분리용
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public abstract class UC_PetBaseState
    {
        /// <summary>상태 소유한 PetController 참조</summary>
        protected PetController controller;

        /// <summary>생성자: PetController 인스턴스를 전달받습니다.</summary>
        public UC_PetBaseState(PetController controller)
        {
            this.controller = controller;
        }

        /// <summary>상태 진입 시 초기화 로직</summary>
        public virtual void Enter() { }

        /// <summary>매 프레임 호출되는 업데이트 로직</summary>
        public virtual void Update() { }

        /// <summary>상태 종료 시 정리 로직</summary>
        public virtual void Exit() { }
    }
}
