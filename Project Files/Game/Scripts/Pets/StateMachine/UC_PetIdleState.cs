// Assets/Scripts/Pet/StateMachine/UC_PetIdleState.cs
// ────────────────────────────────────────────────────
// 📌 펫의 Idle(대기) 상태 처리
//    • 대기 시간 경과 후 Roam 또는 Follow 전환
//    • 사거리 내 적 발견 시 Attack 전환

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class UC_PetIdleState : UC_PetBaseState
    {
        private readonly int speedHash = Animator.StringToHash("Speed");
        private float idleTimer;

        public UC_PetIdleState(PetController controller) : base(controller) { }

        /// <summary>
        /// 상태 진입 시: 이동 중지, Speed=0으로 Idle 애니메이션 유지
        /// </summary>
        public override void Enter()
        {
            controller.Agent.isStopped = true;
            controller.Animator.SetFloat(speedHash, 0f);
            idleTimer = 0f;
        }

        /// <summary>
        /// 매 프레임 호출:
        /// • Idle 유지
        /// • 사거리 내 적 발견 시 Attack 전환
        /// • idleToRoamTime 지난 뒤 Roam/Follow 전환
        /// </summary>
        public override void Update()
        {
            // 1) 적 발견 시 전환
            var target = controller.CurrentTarget;
            if (target != null && !target.IsDead &&
                Vector3.Distance(controller.transform.position, target.transform.position) <= controller.AttackDistance)
            {
                controller.StateMachine.SetState(new UC_PetAttackState(controller));
                return;
            }

            // 2) Idle 시간 증가
            idleTimer += Time.deltaTime;
            if (idleTimer >= controller.IdleToRoamTime)
            {
                if (Random.value < controller.RoamProbability)
                    controller.StateMachine.SetState(new UC_PetRoamState(controller));
                else
                    controller.StateMachine.SetState(new UC_PetFollowState(controller));
                return;
            }

            // 3) 플레이어에서 너무 멀어지면 Follow 전환
            if (Vector3.Distance(controller.transform.position, controller.PlayerTransform.position) > controller.FollowDistance)
            {
                controller.StateMachine.SetState(new UC_PetFollowState(controller));
            }
        }

        public override void Exit() { }
    }
}
