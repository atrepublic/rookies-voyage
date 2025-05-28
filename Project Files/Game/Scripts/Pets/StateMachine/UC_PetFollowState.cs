// UC_PetFollowState.cs (v1.06)
// ────────────────────────────────────────────────────
// 📌 펫의 Follow 상태: 플레이어를 일정 거리로 추적하며,
//    사거리(attackDistance) 내 적이 있으면 무기를 회전하여 계속 공격합니다.
//    씬 언로드 시 Null 참조를 방지하도록 PlayerTransform 체크 포함.

using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.SquadShooter
{
    public class UC_PetFollowState : UC_PetBaseState
    {
        [Tooltip("Animator 내 'Speed' 파라미터 해시")] 
        private readonly int speedHash = Animator.StringToHash("Speed");

        public UC_PetFollowState(PetController controller) : base(controller) { }

        /// <summary>
        /// 상태 진입 시 호출됩니다.
        /// • 이동 세팅(FollowSettings) 적용
        /// • NavMeshAgent 활성화
        /// • 플레이어 뒤쪽 FollowDistance만큼 떨어진 지점으로 즉시 목적지 설정
        /// </summary>
        public override void Enter()
        {
            controller.ApplyMovementSettings(controller.FollowSettings);
            controller.Agent.updatePosition = true;
            controller.Agent.isStopped     = false;

            Vector3 dest = controller.PlayerTransform.position
                         - controller.PlayerTransform.forward * controller.FollowDistance;
            controller.Agent.SetDestination(dest);
        }

        /// <summary>
        /// 매 프레임 호출됩니다.
        /// 1) PlayerTransform이 유효한지 체크 후 진행  
        /// 2) 사거리 내 적 발견 시  
        ///    - 무기 루트(y축)만을 부드럽게 적 방향으로 회전  
        ///    - PetShoot 트리거, TryFire() 호출  
        /// 3) 플레이어 뒤쪽으로 이동 목적지 업데이트  
        /// 4) 이동 속도에 따라 Animator Speed 파라미터 갱신  
        /// </summary>
        public override void Update()
        {
            // ▶▶ 씬 언로드 시 PlayerTransform null 참조 방지
            if (controller.PlayerTransform == null) return;

            var target = controller.CurrentTarget;
            if (target != null && !target.IsDead)
            {
                float dist = Vector3.Distance(controller.transform.position, target.transform.position);
                if (dist <= controller.AttackDistance)
                {
                    // • 무기 루트(y) 회전만 계산
                    Vector3 toTarget = target.transform.position - controller.GunBehavior.GunRoot.position;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude > 0.0001f)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(toTarget);
                        controller.GunBehavior.GunRoot.rotation =
                            Quaternion.Slerp(
                                controller.GunBehavior.GunRoot.rotation,
                                lookRot,
                                controller.GunRotationSpeed * Time.deltaTime
                            );
                    }

                    // • 사격 애니메이션 & 실제 발사
                    controller.Animator.SetTrigger("PetShoot");
                    controller.GunBehavior.TryFire();
                }
            }

            // ▶▶ 플레이어 뒤로 지속 추적
            Vector3 followDest = controller.PlayerTransform.position
                               - controller.PlayerTransform.forward * controller.FollowDistance;
            controller.Agent.SetDestination(followDest);

            // ▶▶ 속도 기반 애니메이션 반영
            float spd = controller.Agent.velocity.magnitude;
            float norm = spd / controller.FollowSettings.MoveSpeed;
            controller.Animator.SetFloat(speedHash, norm, 0.1f, Time.deltaTime);
            
            if (!controller.Agent.isOnNavMesh) return;
                controller.Agent.SetDestination(followDest);
        }

        /// <summary>
        /// 상태 종료 시 호출됩니다.
        /// • Animator의 Speed 파라미터를 0으로 리셋하여 애니메이션 정지
        /// </summary>
        public override void Exit()
        {
            controller.Animator.SetFloat(speedHash, 0f);
        }
    }
}
