// PetController.cs (통합 리팩토링 v1.10)
// ────────────────────────────────────────────────────
// 📌 펫 전체 동작을 관리하는 컨트롤러
//    • 컴포넌트 자동 할당 (NavMeshAgent, EnemyDetector, Gun, Animator)
//    • 상태 머신 초기화 및 업데이트
//    • IEnemyDetector 콜백으로 타겟 갱신
//    • 디버그용 Gizmo 시각화
//    • [추가] UC_PetData 및 업그레이드 레벨 연동 → 무기 Init 반영

using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.SquadShooter
{
    public enum AttackMoveMode
    {
        ChaseTarget,    // 적 추격
        FollowPlayer    // 플레이어 뒤 따라다니기
    }

    [RequireComponent(typeof(NavMeshAgent), typeof(EnemyDetector))]
    public class PetController : MonoBehaviour, IEnemyDetector
    {
        //─────────────────────────────────────────────────
        [Header("컴포넌트 참조")]
        [SerializeField, Tooltip("NavMeshAgent 컴포넌트")]      
        private NavMeshAgent agent;
        [SerializeField, Tooltip("적 감지용 EnemyDetector 컴포넌트")]      
        private EnemyDetector enemyDetector;
        [SerializeField, Tooltip("사격 로직 컴포넌트")]      
        private UC_PetGunBehavior gunBehavior;
        [SerializeField, Tooltip("애니메이터 컴포넌트 (자식 검색)")]      
        private Animator animator;
        [SerializeField, Tooltip("플레이어 Transform (Init 시 자동 할당)")]      
        private Transform playerTransform;

        //─────────────────────────────────────────────────
        [Header("이동 설정")]
        [SerializeField, Tooltip("플레이어 뒤 유지 거리")]      
        private float followDistance = 2f;
        [SerializeField, Tooltip("추적 모드 이동 세팅")]      
        private MovementSettings followSettings;
        [SerializeField, Tooltip("공격 모드 이동 세팅")]      
        private MovementSettings attackSettings;

        [Header("공격 중 복귀 설정")]
        [SerializeField, Tooltip("공격 중 플레이어와 이 거리 이상 멀어지면 FollowState로 복귀")]
        private float attackCancelDistance = 15f;
        public float AttackCancelDistance => attackCancelDistance;

        //─────────────────────────────────────────────────
        [Header("배회(Roam) 설정")]
        [SerializeField, Tooltip("배회 반경")]      
        private float roamRadius = 5f;
        [SerializeField, Tooltip("배회 최소 지속 시간")]      
        private float roamDurationMin = 2f;
        [SerializeField, Tooltip("배회 최대 지속 시간")]      
        private float roamDurationMax = 4f;
        [SerializeField, Tooltip("배회 취소 거리")]      
        private float roamCancelDistance = 10f;

        //─────────────────────────────────────────────────
        [Header("Idle → 이동 전환 설정")]
        [SerializeField, Tooltip("Idle 후 Roam 또는 Follow 전환까지 대기 시간")]      
        private float idleToRoamTime = 3f;
        [SerializeField, Tooltip("Idle 후 Roam으로 전환할 확률(0~1)")] [Range(0f,1f)]
        private float roamProbability = 0.5f;

        //─────────────────────────────────────────────────
        [Header("공격 설정")]
        [SerializeField, Tooltip("공격 모드 중 이동 방식")]      
        private AttackMoveMode attackMoveMode = AttackMoveMode.ChaseTarget;
        [SerializeField, Tooltip("적을 공격하기 위한 적정 거리")]      
        private float attackDistance = 3f;
        [SerializeField, Tooltip("무기 회전 속도")]      
        private float gunRotationSpeed = 10f;

        //─────────────────────────────────────────────────
        // 공개 프로퍼티
        public Transform PlayerTransform        => playerTransform;
        public MovementSettings FollowSettings  => followSettings;
        public MovementSettings AttackSettings  => attackSettings;
        public float FollowDistance             => followDistance;
        public float RoamRadius                 => roamRadius;
        public float RoamDurationMin            => roamDurationMin;
        public float RoamDurationMax            => roamDurationMax;
        public float RoamCancelDistance         => roamCancelDistance;
        public float IdleToRoamTime             => idleToRoamTime;
        public float RoamProbability            => roamProbability;
        public AttackMoveMode AttackMoveMode    => attackMoveMode;
        public float AttackDistance             => attackDistance;
        public float GunRotationSpeed           => gunRotationSpeed;
        public NavMeshAgent Agent               => agent;
        public Animator Animator                => animator;
        public UC_PetGunBehavior GunBehavior    => gunBehavior;
        public BaseEnemyBehavior CurrentTarget  => targetEnemy;

        //─────────────────────────────────────────────────
        // 내부 상태
        private BaseEnemyBehavior targetEnemy;

        private UC_PetStateMachine stateMachine;

        /// <summary>상태 전환 및 업데이트를 관리하는 상태 머신</summary>
        public UC_PetStateMachine StateMachine => stateMachine;

        // [추가] 펫 업그레이드 관련 상태
        private UC_PetData petData;
        private int upgradeLevel;

        /// <summary>
        /// PetManager에서 펫 생성 시 데이터 및 업그레이드 레벨을 전달받아 저장
        /// </summary>
        public void SetData(UC_PetData data, int level)
        {
            petData = data;
            upgradeLevel = level;
        }

        /// <summary>
        /// 펫 초기화: 플레이어 Transform 전달 및 하위 컴포넌트 초기화
        /// [수정] gunBehavior.Init(this) 시 업그레이드 적용 가능
        /// </summary>
    public void Init(Transform player)
    {
        playerTransform = player;

        Vector3 desiredPos = transform.position;
        NavMeshHit hit;
        const float sampleRange = 555f;
        if (NavMesh.SamplePosition(desiredPos, out hit, sampleRange, NavMesh.AllAreas))
        {
            desiredPos = hit.position;
        }

        agent.Warp(desiredPos);
        transform.position = desiredPos;

        enemyDetector.Init(this);
        gunBehavior.Init(this);

        agent.enabled = true;
        enabled       = true;

        // [추가] 상태 머신이 null 상태로 접근되지 않도록 생성
        stateMachine = new UC_PetStateMachine(this);
        stateMachine.SetState(new UC_PetIdleState(this));
    }

        private void Update()
        {
            stateMachine.Update();
        }

        public void ApplyMovementSettings(MovementSettings s)
        {
            if (s == null) return;
            agent.speed        = s.MoveSpeed;
            agent.acceleration = s.Acceleration;
            agent.angularSpeed = s.RotationSpeed;
        }

        public void OnCloseEnemyChanged(BaseEnemyBehavior enemy)
        {
            targetEnemy = enemy;
        }

        public UC_PetData PetData => petData;
        public int UpgradeLevel => upgradeLevel;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, followDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDistance);
            if (playerTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(playerTransform.position, roamRadius);
            }
        }
#endif
    }
}
