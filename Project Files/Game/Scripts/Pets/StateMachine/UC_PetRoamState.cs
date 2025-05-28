// Assets/Scripts/Pet/StateMachine/UC_PetRoamState.cs
// ────────────────────────────────────────────────────
// 📌 펫의 Roam(배회) 상태 처리
//    • 플레이어 주변 랜덤 지점으로 이동
//    • 사거리 내 적 발견 시 Attack 전환
//    • 배회 종료 시 Follow 전환

using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.SquadShooter
{
    public class UC_PetRoamState : UC_PetBaseState
    {
        private readonly int speedHash = Animator.StringToHash("Speed");
        private Vector3 roamPoint;
        private float roamEndTime;

        public UC_PetRoamState(PetController controller) : base(controller) { }

        /// <summary>
        /// 상태 진입 시: 랜덤 배회 지점 계산 및 이동 시작
        /// </summary>
        public override void Enter()
        {
            controller.ApplyMovementSettings(controller.FollowSettings);
            controller.Agent.updatePosition = true;
            controller.Agent.isStopped     = false;

            Vector3 origin = controller.PlayerTransform.position;
            Vector3 offset = Random.insideUnitSphere * controller.RoamRadius;
            offset.y = 0f;
            NavMesh.SamplePosition(origin + offset, out var hit, 1f, NavMesh.AllAreas);
            roamPoint   = hit.position;
            roamEndTime = Time.time + Random.Range(controller.RoamDurationMin, controller.RoamDurationMax);
            controller.Agent.SetDestination(roamPoint);
        }

        /// <summary>
        /// 매 프레임 호출:
        /// • 사거리 내 적 발견 시 Attack 전환
        /// • 이동 애니메이션 갱신
        /// • 배회 종료 또는 플레이어와 멀어지면 Follow 전환
        /// </summary>
        public override void Update()
        {
            var target = controller.CurrentTarget;
            if (target != null && !target.IsDead &&
                Vector3.Distance(controller.transform.position, target.transform.position) <= controller.AttackDistance)
            {
                controller.StateMachine.SetState(new UC_PetAttackState(controller));
                return;
            }

            // 이동 애니메이션
            float spd = controller.Agent.velocity.magnitude;
            float norm = spd / controller.FollowSettings.MoveSpeed;
            controller.Animator.SetFloat(speedHash, norm, 0.1f, Time.deltaTime);

            // 배회 종료 또는 멀어짐 체크
            float distToPlayer = Vector3.Distance(controller.transform.position, controller.PlayerTransform.position);
            bool timeUp  = Time.time >= roamEndTime;
            bool arrived = Vector3.Distance(controller.transform.position, roamPoint) <= controller.Agent.stoppingDistance;
            if (timeUp || arrived || distToPlayer > controller.RoamCancelDistance)
            {
                controller.StateMachine.SetState(new UC_PetFollowState(controller));
            }
        }

        public override void Exit()
        {
            controller.Animator.SetFloat(speedHash, 0f);
        }
    }
}
