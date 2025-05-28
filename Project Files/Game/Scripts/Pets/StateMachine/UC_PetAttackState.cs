// Assets/Scripts/Pet/StateMachine/UC_PetAttackState.cs
// v1.01: 무기 회전 및 발사 방향 개선
// • 무기 루트를 적 방향으로 회전하여 총알이 항상 적을 향하도록 수정
// • Korean tooltips 및 주석 추가
// ─────────────────────────────────────────────────────────────────────
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.SquadShooter
{
    public class UC_PetAttackState : UC_PetBaseState
    {
        [Tooltip("Animator의 Speed 파라미터 해시")] private readonly int speedHash = Animator.StringToHash("Speed");
        private float nextFireTime;

        public UC_PetAttackState(PetController controller) : base(controller) { }

        /// <summary>
        /// 상태 진입: 이동 세팅 적용, stoppingDistance 설정, 애니메이터 초기화, 즉시 사격 준비
        /// </summary>
        public override void Enter()
        {
            controller.ApplyMovementSettings(controller.AttackSettings);
            controller.Agent.stoppingDistance = controller.AttackDistance;
            controller.Agent.updatePosition     = true;
            controller.Agent.isStopped          = false;
            controller.Animator.SetFloat(speedHash, 0f);
            nextFireTime = Time.time;
        }

        /// <summary>
        /// 매 프레임: 플레이어 거리 확인, 타겟 유효성 검사, 이동 처리, 애니메이션, 무기 회전, 사격
        /// </summary>
        public override void Update()
        {
            // 1) 플레이어 거리가 공격 취소 거리 초과 시 Follow 상태로 전환
            float distToPlayer = Vector3.Distance(controller.transform.position, controller.PlayerTransform.position);
            if (distToPlayer > controller.AttackCancelDistance)
            {
                controller.StateMachine.SetState(new UC_PetFollowState(controller));
                return;
            }

            // 2) 타겟 유효성 검사: 없거나 사망 시 Idle 상태로 전환
            var target = controller.CurrentTarget;
            if (target == null || target.IsDead)
            {
                controller.StateMachine.SetState(new UC_PetIdleState(controller));
                return;
            }

            // 3) 이동: ChaseTarget vs FollowPlayer
            Vector3 destination = controller.AttackMoveMode == AttackMoveMode.ChaseTarget
                ? target.transform.position
                : controller.PlayerTransform.position - controller.PlayerTransform.forward * controller.FollowDistance;
            controller.Agent.SetDestination(destination);

            // 4) 애니메이션: 이동 속도 기반 Speed 파라미터 갱신
            float speed = controller.Agent.velocity.magnitude;
            float normalized = speed / controller.AttackSettings.MoveSpeed;
            controller.Animator.SetFloat(speedHash, normalized, 0.1f, Time.deltaTime);

            // [추가] 5) 무기 루트를 타겟 방향으로 부드럽게 회전
            Vector3 toTarget = target.transform.position - controller.GunBehavior.GunRoot.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(toTarget);
                controller.GunBehavior.GunRoot.rotation =
                    Quaternion.Slerp(
                        controller.GunBehavior.GunRoot.rotation,
                        lookRot,
                        controller.GunRotationSpeed * Time.deltaTime
                    );
            }

            // 6) 사격 로직: 사거리 내 적 & 쿨다운 경과 시 발사
            float distToEnemy = Vector3.Distance(controller.transform.position, target.transform.position);
            if (distToEnemy <= controller.AttackDistance && Time.time >= nextFireTime)
            {
                controller.Animator.SetTrigger("PetShoot");
                controller.GunBehavior.TryFire();
                nextFireTime = Time.time + (1f / controller.GunBehavior.FireRate);
            }
        }

        public override void Exit()
        {
            controller.Agent.stoppingDistance = 0f;
            controller.Animator.SetFloat(speedHash, 0f);
        }
    }
}
