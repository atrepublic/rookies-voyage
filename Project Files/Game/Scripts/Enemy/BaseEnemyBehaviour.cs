/*
 * BaseEnemyBehaviour.cs
 * ---------------------
 * 이 추상 클래스는 게임 내 모든 적 캐릭터의 기본 행동 및 속성을 정의하는 기반 클래스입니다.
 * 체력 관리(IHealth), NavMeshAgent를 이용한 이동(INavMeshAgent), 상태 머신, 애니메이션,
 * 피격 효과, 죽음 처리, 아이템 드랍, 무기 장착 등 적 캐릭터의 공통 기능을 포함합니다.
 * 구체적인 적 유형(근접, 원거리 등)은 이 클래스를 상속받아 Attack() 등의 추상 메서드를 구현합니다.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent 사용
using Watermelon.LevelSystem; // LevelController 등 사용
using Random = UnityEngine.Random; // UnityEngine.Random 명시적 사용
using Watermelon; // Watermelon 프레임워크 네임스페이스

#if UNITY_EDITOR // 에디터 전용 코드 컴파일 지시문 시작
using UnityEditor;
using UnityEditor.SceneManagement;
#endif // 에디터 전용 코드 컴파일 지시문 끝

namespace Watermelon.SquadShooter
{
    // 적 캐릭터의 기본 행동을 정의하는 추상 클래스, IHealth 및 INavMeshAgent 인터페이스 구현
    public abstract class BaseEnemyBehavior : MonoBehaviour, IHealth, INavMeshAgent
    {
        // 상수 정의
        [Tooltip("피격 시 잠시 적용될 오버레이 색상")]
        private static readonly Color HIT_OVERLAY_COLOR = new Color(0.6f, 0.6f, 0.6f, 1.0f);

        // 애니메이터 파라미터 해시값 (성능 최적화)
        [Tooltip("애니메이터 'Running' 파라미터 해시")]
        protected readonly int ANIMATOR_RUN_HASH = Animator.StringToHash("Running"); // 달리기 상태
        [Tooltip("애니메이터 'Movement Speed' 파라미터 해시")]
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed"); // 이동 속도

        [Tooltip("애니메이터 'Hit' 트리거 파라미터 해시")]
        public static readonly int ANIMATOR_HIT_HASH = Animator.StringToHash("Hit"); // 피격 트리거
        [Tooltip("애니메이터 'Hit Index' 정수 파라미터 해시")]
        public static readonly int ANIMATOR_HIT_INDEX_HASH = Animator.StringToHash("Hit Index"); // 피격 애니메이션 인덱스
        [Tooltip("피격 애니메이션 간 최소 간격 (쿨다운)")]
        public static readonly float ANIMATOR_HIT_COOLDOWN = 0.08f; // 피격 애니메이션 쿨다운

        // 셰이더 파라미터 해시값 (성능 최적화)
        [Tooltip("셰이더 '_EmissionColor' 파라미터 해시 (피격 시 반짝임 효과)")]
        private static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");
        [Tooltip("셰이더 '_MaskPercent' 파라미터 해시 (스폰/디졸브 효과 등에 사용될 수 있음)")]
        private static readonly int MASK_PERCENT_HASH = Shader.PropertyToID("_MaskPercent");

        [Tooltip("적의 유형 (예: 근접, 원거리)")]
        [SerializeField] EnemyType type;
        // 외부에서 적 유형에 접근하기 위한 프로퍼티
        public EnemyType EnemyType => type;

        [Tooltip("적의 등급 (일반, 엘리트, 보스)")]
        [SerializeField] EnemyTier tier = EnemyTier.Regular; // 기본값은 일반
        // 외부에서 적 등급에 접근하기 위한 프로퍼티 (set은 내부에서만 가능)
        public EnemyTier Tier { get => tier; private set => tier = value; }

        [Tooltip("적의 기본 능력치 (데이터베이스 또는 EnemyData에서 설정됨)")]
        protected EnemyStats stats;
        // 외부에서 적 능력치에 접근하기 위한 프로퍼티
        public EnemyStats Stats => stats;

        [Tooltip("적의 애니메이터 컴포넌트 참조")]
        [SerializeField]
        protected Animator animatorRef;
        // 외부에서 애니메이터에 접근하기 위한 프로퍼티
        public Animator Animator => animatorRef;

        [Tooltip("적의 체력 바 UI를 관리하는 컴포넌트")]
        [SerializeField]
        protected HealthbarBehaviour healthbarBehaviour;
        [Tooltip("애니메이션 이벤트 콜백을 처리하는 컴포넌트")]
        [SerializeField] EnemyAnimationCallback enemyAnimationCallback;

        [Space] // 인스펙터 공백
        [Tooltip("적의 주 메쉬 렌더러 (표시 여부 확인, 피격 효과 등에 사용)")]
        [SerializeField]
        protected SkinnedMeshRenderer meshRenderer;
        // 현재 카메라 시야 내에 렌더링되고 있는지 여부
        public bool IsVisible => meshRenderer != null && meshRenderer.isVisible; // Null 체크 추가

        [Tooltip("적의 기본 피격 콜라이더 (활성/비활성 제어용)")]
        [SerializeField]
        protected Collider baseHitCollider;

        [Tooltip("적의 상태 머신 인터페이스 (상태 관리)")]
        protected IStateMachine StateMachine { get; private set; }

        [Space] // 인스펙터 공백
        [Tooltip("엘리트 등급일 때 추가 효과를 관리하는 컴포넌트")]
        [SerializeField] EliteCase eliteCase; // EliteCase는 엘리트 관련 시각/기능 효과 관리 클래스 추정

        [Header("무기 관련 설정")]
        [Tooltip("적이 장착할 수 있는 무기 목록 (WeaponRigBehavior 컴포넌트)")]
        [SerializeField] List<WeaponRigBehavior> weapons;

        [Tooltip("오른손 뼈대 Transform (무기 부착 등)")]
        [SerializeField] Transform rightHandBone;
        [Tooltip("왼손 뼈대 Transform (무기 부착 등)")]
        [SerializeField] Transform leftHandBone;

        // 외부에서 오른손/왼손 뼈대에 접근하기 위한 프로퍼티
        public Transform RightHandBone => rightHandBone;
        public Transform LeftHandBone => leftHandBone;

        [Tooltip("죽음 시 래그돌에 가해지는 폭발 힘")]
        public float deathExplosionForce = 7000;
        [Tooltip("죽음 시 래그돌 폭발 힘의 적용 반경")]
        public float deathExplosionRadius = 100;

        // 상태 플래그
        [Tooltip("적이 목표를 추적할 수 있는지 여부")]
        public bool CanPursue { get; set; }
        [Tooltip("적이 이동할 수 있는지 여부")]
        public bool CanMove { get; set; }
        [Tooltip("적이 현재 공격 중인지 여부")]
        public bool IsAttacking { get; set; }

        // 체력 관련 (IHealth 인터페이스 구현)
        [Tooltip("적의 현재 체력")]
        protected float currentHealth;
        // 외부에서 현재 체력에 접근하기 위한 프로퍼티
        public float CurrentHealth => currentHealth;
        // 외부에서 최대 체력에 접근하기 위한 프로퍼티 (엘리트인 경우 배율 적용)
        public float MaxHealth => stats != null ? stats.Hp * (tier == EnemyTier.Elite ? stats.EliteHealthMult : 1f) : 0f; // stats Null 체크 추가

        [Tooltip("적이 사망했는지 여부")]
        protected bool isDead;
        // 외부에서 사망 여부에 접근하기 위한 프로퍼티
        public bool IsDead => isDead;

        [Tooltip("추격 모드 활성화 여부 (첫 피격 후 활성화되어 타겟을 계속 추적)")]
        protected bool chaseMode;

        // 이동 관련 (INavMeshAgent 인터페이스 구현)
        [Tooltip("NavMeshAgent 컴포넌트 참조")]
        protected NavMeshAgent navMeshAgent;
        // 외부에서 NavMeshAgent에 접근하기 위한 프로퍼티
        public NavMeshAgent NavMeshAgent => navMeshAgent;

        [Tooltip("달리기 속도 (NavMeshAgent의 기본 속도)")]
        public float RunningSpeed { get; protected set; }
        [Tooltip("걷기 속도 (달리기 속도의 절반)")]
        public float WalkingSpeed { get; protected set; }
        [Tooltip("현재 걷기 상태인지 여부")]
        public bool IsWalking { get; set; }

        // 위치 및 회전 관련 프로퍼티
        public Vector3 Position => transform.position;
        public Quaternion Rotation { get => transform.rotation; set => transform.rotation = value; }

        // 타겟 관련 프로퍼티
        public Vector3 TargetPosition => Target != null ? Target.position : transform.position; // Target Null 체크 추가
        public float VisionRange => visionRange; // 적의 시야 범위

        // 타겟과의 거리 기반 상태 확인 프로퍼티
        public bool IsTargetInVisionRange => Target != null && Vector3.Distance(transform.position, Target.position) <= visionRange; // 시야 범위 내 (Null 체크 추가)
        public bool IsTargetInAttackRange => Target != null && stats != null && Vector3.Distance(transform.position, Target.position) <= stats.AttackDistance; // 공격 범위 내 (Null 체크 추가)
        public bool IsTargetInFleeRange => Target != null && stats != null && Vector3.Distance(transform.position, Target.position) <= stats.FleeDistance; // 도주 범위 내 (일부 적만 사용, Null 체크 추가)

        [Tooltip("적이 피해를 입은 적이 있는지 여부")]
        public bool HasTakenDamage { get; private set; }

        [Tooltip("적의 현재 타겟 (주로 플레이어 캐릭터)")]
        protected Transform target;
        // 외부에서 타겟 Transform에 접근하기 위한 프로퍼티
        public Transform Target => target;

        [Tooltip("플레이어 캐릭터 비헤이비어 참조")]
        protected CharacterBehaviour characterBehaviour;

        // 주요 컴포넌트 참조
        [Tooltip("적의 메인 콜라이더 (캡슐 형태)")]
        protected CapsuleCollider enemyCollider;
        [Tooltip("적의 리지드바디 컴포넌트")]
        protected Rigidbody enemyRigidbody;

        // 이벤트 델리게이트 정의
        public static OnEnemyDiedDelegate OnDiedEvent; // 적 사망 시 호출될 정적 이벤트

        // 피격 효과 관련
        [Tooltip("피격 시 반짝임 효과를 위한 MaterialPropertyBlock")]
        private MaterialPropertyBlock hitShinePropertyBlock;
        [Tooltip("피격 시 반짝임 효과 트윈 애니메이션 케이스")]
        private TweenCase hitShineTweenCase;

        // 타이머 변수
        [Tooltip("마지막 플로팅 텍스트 표시 시간")]
        private float lastFlotingTextTime;
        [Tooltip("마지막으로 피해를 입은 시간 (넉백 효과 복구용)")]
        private float lastDamagedTime;
        [Tooltip("마지막 피격 반짝임 효과 시간")]
        private float lastHitShineTime;

        [Tooltip("피격 애니메이션 재생 종료 시간")]
        protected float hitAnimationTime;

        // 외부 접근용 프로퍼티
        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        // 순찰 관련
        [Tooltip("적이 순찰할 지점 목록")]
        private Vector3[] patrollingPoints;
        // 외부에서 순찰 지점 목록에 접근하기 위한 프로퍼티
        public Vector3[] PatrollingPoints => patrollingPoints;

        // 드랍 관련
        [Tooltip("이 적이 드랍할 아이템 데이터 목록")]
        private List<DropData> dropData;

        [Tooltip("이 적의 원본 데이터 (EnemyData ScriptableObject)")]
        protected EnemyData enemyData;

        [Tooltip("적의 실제 시야 범위 (초기값은 EnemyData에서 가져옴)")]
        protected float visionRange;
        [Tooltip("래그돌 비활성화 트윈 케이스")]
        protected TweenCase ragdollCase;

        [Tooltip("피격 시 넉백 거리 배율 (연속 피격 시 감소)")]
        private float hitOffsetMult;

        // 래그돌 관련
        [Tooltip("래그돌 동작을 관리하는 컴포넌트")]
        protected RagdollBehavior ragdoll;
        // 외부에서 래그돌 컴포넌트에 접근하기 위한 프로퍼티
        public RagdollBehavior Ragdoll => ragdoll;

        // 이벤트 정의
        public event SimpleCallback OnTakenDamage; // 피해를 입었을 때 발생하는 이벤트
        public event SimpleCallback OnReloadFinished; // 재장전 완료 시 발생하는 이벤트 (주로 원거리 적)
        protected Vector3 lastProjectilePosition; // 마지막으로 맞은 투사체의 위치 (래그돌 방향 계산용)
        public event SimpleCallback OnAttackFinished; // 공격 애니메이션/동작 완료 시 발생하는 이벤트
        public delegate void OnEnemyDiedDelegate(BaseEnemyBehavior enemy); // 적 사망 이벤트 델리게이트


        /// <summary>
        /// MonoBehaviour: 객체가 처음 활성화될 때 Awake가 호출된 후 호출됩니다.
        /// 주로 컴포넌트 참조 및 초기 설정을 수행합니다.
        /// </summary>
        protected virtual void Awake()
        {
            isDead = true; // 초기 상태는 죽음으로 설정 (Init에서 변경)
            navMeshAgent = GetComponent<NavMeshAgent>(); // NavMeshAgent 컴포넌트 가져오기
            enemyCollider = GetComponent<CapsuleCollider>(); // CapsuleCollider 컴포넌트 가져오기
            enemyRigidbody = GetComponent<Rigidbody>(); // Rigidbody 컴포넌트 가져오기

            // 래그돌 초기화
            ragdoll = new RagdollBehavior();
            ragdoll.Init(transform);

            // 피격 효과용 MaterialPropertyBlock 생성
            hitShinePropertyBlock = new MaterialPropertyBlock();

            // 애니메이션 콜백 컴포넌트 초기화 (Null 체크 추가)
            if (enemyAnimationCallback != null)
                enemyAnimationCallback.Init(this);

            navMeshAgent.enabled = false; // NavMeshAgent는 Init에서 활성화

            // 이동 속도 설정 (navMeshAgent Null 체크 추가)
            if (navMeshAgent != null)
            {
                RunningSpeed = navMeshAgent.speed; // 달리기 속도는 NavMeshAgent의 기본 속도 사용
                WalkingSpeed = RunningSpeed / 2f; // 걷기 속도는 달리기 속도의 절반
            }

            // 기본 피격 콜라이더 활성화 (존재할 경우)
            if (baseHitCollider != null)
                baseHitCollider.enabled = true;

            // 상태 머신 컴포넌트 가져오기
            StateMachine = GetComponent<IStateMachine>();

            // 무기 초기화 (Null 체크 추가)
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] != null)
                        weapons[i].Init(this);
                }
            }
        }

        /// <summary>
        /// 적의 데이터(스탯, 등급 등)를 설정합니다. 스폰 시 호출됩니다.
        /// </summary>
        /// <param name="enemyData">적의 기본 데이터</param>
        /// <param name="isElite">엘리트 등급 여부</param>
        public void SetEnemyData(EnemyData enemyData, bool isElite)
        {
            // eliteCase Null 체크 추가
            if (eliteCase != null)
            {
                eliteCase.Validate(); // 엘리트 케이스 유효성 검사 (내부 로직)

                // 엘리트 여부에 따라 등급 및 엘리트 효과 설정
                if (isElite)
                {
                    Tier = EnemyTier.Elite;
                    eliteCase.SetElite();
                }
                // 보스가 아닌 경우에만 일반 등급 설정 가능 (보스는 초기 설정 유지)
                else if (Tier != EnemyTier.Boss)
                {
                    Tier = EnemyTier.Regular;
                    eliteCase.SetRegular();
                }
            } else if (isElite) // eliteCase가 없어도 등급은 설정
            {
                 Tier = EnemyTier.Elite;
            }
            else if (Tier != EnemyTier.Boss)
            {
                 Tier = EnemyTier.Regular;
            }


            // 데이터 및 스탯 설정
            this.enemyData = enemyData;
            stats = enemyData.Stats;

            // 시야 범위 설정 (stats Null 체크 추가)
            if (stats != null)
                visionRange = enemyData.Stats.VisionRange;
        }

        /// <summary>
        /// 적을 초기화하고 활성화합니다. 스폰 또는 재활용 시 호출됩니다.
        /// </summary>
        public virtual void Init()
        {
            // 타겟 설정 (플레이어)
            characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null) // characterBehaviour Null 체크 추가
                target = characterBehaviour.transform;
            hitOffsetMult = 1f; // 피격 넉백 배율 초기화

            transform.localScale = Vector3.one; // 스케일 초기화

            // 리지드바디 비활성화 (NavMeshAgent 사용, Null 체크 추가)
            if (enemyRigidbody != null)
            {
                enemyRigidbody.isKinematic = true;
                enemyRigidbody.useGravity = false;
            }

            // 콜라이더를 트리거로 설정 (물리적 충돌 대신 감지용, Null 체크 추가)
            if (enemyCollider != null)
                enemyCollider.isTrigger = true;

            // 애니메이터 활성화 (Null 체크 추가)
            if (animatorRef != null)
            {
                animatorRef.gameObject.SetActive(true);
                animatorRef.enabled = true; // 애니메이터 활성화 추가 (OnDeath에서 비활성화되므로)
            }

            // 체력 초기화
            currentHealth = MaxHealth;

            // 체력 바 초기화 및 설정 (Null 체크 추가)
            if (healthbarBehaviour != null && LevelController.CurrentLevelData != null)
                healthbarBehaviour.Init(transform, this, true, healthbarBehaviour.HealthbarOffset, LevelController.CurrentLevelData.EnemiesLevel, Tier == EnemyTier.Elite);

            // 상태 초기화
            isDead = false;
            chaseMode = false;

            // NavMeshAgent 활성화 요청
            NavMeshController.InvokeOrSubscribe(this);

            // 플레이어 사망 또는 레벨 클리어 이벤트 구독 (LevelController Null 체크 고려 필요할 수 있음)
            LevelController.OnPlayerDiedEvent += OnRoomDone;
            LevelController.OnPlayerExitLevelEvent += OnRoomDone;

            // 기본 피격 콜라이더 활성화 (Null 체크 추가)
            if (baseHitCollider != null)
                baseHitCollider.enabled = true;

            HasTakenDamage = false; // 피해 입은 상태 초기화

            // 상태 머신 시작 (Null 체크 추가)
            if (StateMachine != null)
                StateMachine.StartMachine();

            // 무기 활성화 (Null 체크 추가)
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] != null) weapons[i].enabled = true;
                }
            }
        }

        /// <summary>
        /// 현재 방(Room)이 완료되었을 때(플레이어 사망 또는 레벨 클리어) 호출됩니다.
        /// 이벤트 구독 해제 및 래그돌 관련 처리를 합니다.
        /// </summary>
        public virtual void OnRoomDone()
        {
            // 이벤트 구독 해제 (LevelController Null 체크 고려 필요할 수 있음)
            LevelController.OnPlayerDiedEvent -= OnRoomDone;
            LevelController.OnPlayerExitLevelEvent -= OnRoomDone;

            ragdollCase.KillActive(); // 래그돌 비활성화 트윈 중지
        }

        /// <summary>
        /// NavMesh가 업데이트되었을 때 호출됩니다 (INavMeshAgent 인터페이스).
        /// NavMeshAgent를 활성화하고 관련 속성을 설정합니다.
        /// </summary>
        public void OnNavMeshUpdated()
        {
            // navMeshAgent 및 stats Null 체크 추가
            if (navMeshAgent != null && stats != null)
            {
                navMeshAgent.enabled = true; // NavMeshAgent 활성화
                navMeshAgent.stoppingDistance = stats.PreferedDistanceToPlayer; // 선호 거리 설정
                navMeshAgent.speed = stats.MoveSpeed; // 이동 속도 설정
                navMeshAgent.angularSpeed = stats.AngularSpeed; // 회전 속도 설정
            }
        }

        /// <summary>
        /// (추상 메서드) 애니메이션 이벤트 콜백 발생 시 호출됩니다.
        /// 자식 클래스에서 구체적인 애니메이션 이벤트 처리 로직을 구현해야 합니다.
        /// </summary>
        /// <param name="enemyCallbackType">발생한 콜백 유형</param>
        public abstract void OnAnimatorCallback(EnemyCallbackType enemyCallbackType);

        /// <summary>
        /// MonoBehaviour: 객체가 파괴될 때 호출됩니다.
        /// 체력 바 오브젝트를 파괴합니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 체력 바 오브젝트가 존재하면 파괴
            if (healthbarBehaviour != null && healthbarBehaviour.HealthBarTransform != null)
            {
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);
            }
            // 이벤트 구독 해제 (안전하게 여기서도 처리)
            LevelController.OnPlayerDiedEvent -= OnRoomDone;
            LevelController.OnPlayerExitLevelEvent -= OnRoomDone;
        }

        /// <summary>
        /// 적 객체를 비활성화(언로드)할 때 호출됩니다.
        /// 체력 바를 파괴하고 NavMeshAgent를 정지시킵니다.
        /// </summary>
        public virtual void Unload()
        {
            // healthbarBehaviour Null 체크 추가
            if (healthbarBehaviour != null)
                healthbarBehaviour.Destroy(); // 체력 바 파괴

            // NavMeshAgent가 활성화 상태면 정지 (Null 체크 추가)
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;
        }

        /// <summary>
        /// 현재 위치에서 타겟까지 장애물 없이 직접 보이는지 확인합니다.
        /// </summary>
        /// <returns>직선 시야가 확보되면 true</returns>
        public bool IsTargetInSight()
        {
            // Target 또는 stats가 Null이면 시야 확보 불가
            if (Target == null || stats == null) return false;

            var origin = transform.position.SetY(1); // 발사 원점 (Y=1)
            var direction = (Target.position.SetY(1) - origin).normalized; // 타겟 방향 (Y=1)
            // Raycast 실행 (공격 거리 내, LayerMask 값 328 사용 - 원본 코드 값 유지)
            // LayerMask 328 (이진수 101001000): 보통 Layer 3(Ignore Raycast), 6, 8(Player 추정) 포함
            if (Physics.Raycast(new Ray(origin, direction), out var hit, Stats.AttackDistance, 328))
            {
                // 충돌한 객체가 타겟(플레이어)이면 시야 확보
                if (hit.collider.gameObject == Target.gameObject)
                    return true;
            }
            // 그 외의 경우 (다른 물체에 막히거나, 아무것도 맞지 않음) 시야 미확보
            return false;
        }


        /// <summary>
        /// 유니티 에디터에서 값이 변경될 때 호출됩니다.
        /// 무기 컴포넌트를 초기화합니다.
        /// </summary>
        private void OnValidate()
        {
            // 무기 목록이 null이면 종료 (Null 체크 추가)
            if (weapons == null) return;

            // 모든 무기에 대해 초기화 함수 호출 (Null 체크 추가)
            for (int i = 0; i < weapons.Count; i++)
            {
                var weapon = weapons[i];
                if (weapon == null) continue; // 무기가 null이면 건너뜀

                weapon.Init(this); // 무기 초기화
            }
        }

        #region 전투 관련 (Combat)

        /// <summary>
        /// 현재 적의 공격력을 계산하여 반환합니다. (등급 및 스탯 기반)
        /// </summary>
        /// <returns>계산된 현재 공격력</returns>
        public int GetCurrentDamage()
        {
            // stats 또는 stats.Damage가 Null이면 0 반환
            if (stats == null || stats.Damage == null) return 0;

            // 기본 데미지 범위 내에서 랜덤 값 * 엘리트 데미지 배율(엘리트인 경우)
            return (int)(stats.Damage.Random() * (tier == EnemyTier.Elite ? stats.EliteDamageMult : 1f));
        }

        /// <summary>
        /// 적의 추격 모드를 활성화합니다. (시야 범위를 무한대로 설정)
        /// </summary>
        public void StartChasing()
        {
            visionRange = 9999; // 시야 범위를 매우 큰 값으로 설정
            chaseMode = true; // 추격 모드 플래그 설정
        }

        /// <summary>
        /// 재장전 완료 이벤트를 발생시킵니다. (주로 자식 클래스에서 호출)
        /// </summary>
        protected void InvokeOnReloadFinished()
        {
            OnReloadFinished?.Invoke(); // 이벤트 호출
        }

        /// <summary>
        /// 적이 피해를 입었을 때 처리 로직입니다. (IHealth 인터페이스 구현)
        /// </summary>
        /// <param name="damage">받은 피해량</param>
        /// <param name="projectilePosition">피해를 입힌 투사체의 현재 위치</param>
        /// <param name="projectileDirection">피해를 입힌 투사체의 방향</param>
        public virtual void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead) return; // 이미 죽었으면 무시
            if (damage <= 0) return; // 피해량이 0 이하면 무시

            // 체력 감소 (0 이하 또는 최대 체력 이상으로 벗어나지 않도록 Clamp)
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);

            // 체력 바 업데이트 (Null 체크 추가)
            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();

            // 마지막 투사체 위치 저장 (래그돌 방향 계산용)
            lastProjectilePosition = projectilePosition - projectileDirection; // 발사 원점 추정

            // 체력이 0 이하가 되면 사망 처리
            if (currentHealth <= 0)
            {
                OnDeath();
                return; // 사망 처리 후 함수 종료
            }

            // 피격 시 약간의 넉백 효과 (타겟 반대 방향으로, Target Null 체크 추가)
            if (Target != null)
            {
                transform.position += (transform.position - Target.position).normalized * 0.15f * hitOffsetMult;
                hitOffsetMult *= 0.8f; // 넉백 배율 감소
            }


            HitEffect(); // 피격 시각 효과 재생

            // 플로팅 데미지 텍스트 표시 (짧은 시간 내 중복 표시 방지, stats Null 체크 추가)
            if (stats != null && lastFlotingTextTime + 0.18f <= Time.realtimeSinceStartup)
            {
                lastFlotingTextTime = Time.realtimeSinceStartup;
                // 데미지 텍스트 스폰
               // FloatingTextController.SpawnFloatingText("Hit", "-" + damage.ToString("F0"), transform.position + transform.forward * stats.HitTextOffsetForward + new Vector3(Random.Range(-0.3f, 0.3f), stats.HitTextOffsetY, Random.Range(-0.1f, 0.1f)), Quaternion.identity, 1.0f, Color.white);
            }

            lastDamagedTime = Time.realtimeSinceStartup; // 마지막 피격 시간 기록 (넉백 복구용)

            OnTakenDamage?.Invoke(); // 피격 이벤트 호출

            HasTakenDamage = true; // 피해 입음 플래그 설정

            // 첫 피격 시 추격 모드 활성화 (시야 무한)
            if (!chaseMode)
            {
                StartChasing();
            }
        }

        /// <summary>
        /// MonoBehaviour: 고정된 시간 간격으로 호출됩니다.
        /// 피격 넉백 효과 복구를 처리합니다.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            // 넉백 배율이 1보다 작고, 마지막 피격 후 일정 시간이 지났으면
            if (hitOffsetMult < 1 && lastDamagedTime + 0.5f < Time.realtimeSinceStartup)
            {
                // 넉백 배율을 점차 1로 복구
                hitOffsetMult += Time.fixedDeltaTime;
                if (hitOffsetMult > 1) hitOffsetMult = 1f; // 1을 넘지 않도록
            }
        }

        /// <summary>
        /// 적 사망 시 처리 로직입니다.
        /// </summary>
        protected virtual void OnDeath()
        {
            if (isDead) return; // 중복 실행 방지

            isDead = true; // 사망 상태 설정

            if (navMeshAgent != null && navMeshAgent.enabled) navMeshAgent.enabled = false; // NavMeshAgent 비활성화

            // 체력 바 비활성화 (Null 체크 추가)
            if (healthbarBehaviour != null)
                healthbarBehaviour.DisableBar();

            // 콜라이더 설정 변경 (물리 충돌 가능하게, Null 체크 추가)
            if (enemyCollider != null)
                enemyCollider.isTrigger = false;
            // 애니메이터 비활성화 (래그돌 사용, Null 체크 추가)
            if (animatorRef != null)
                animatorRef.enabled = false;

            // ShowDeathFallAnimation(); // 원본 코드 주석 처리됨 - 래그돌로 대체된 것으로 보임

            // 기본 피격 콜라이더 비활성화 (존재할 경우, Null 체크 추가)
            if (baseHitCollider != null)
                baseHitCollider.enabled = false;

            // 래그돌 활성화
            ActivateRagdollOnDeath();

            // 자원 드랍
            DropResources();

            // 사망 사운드 재생 (랜덤)
            AudioController.PlaySound(AudioController.AudioClips.enemyScreems.GetRandomItem());

            // 레벨 컨트롤러에 적 처치 알림
            LevelController.OnEnemyKilled(this);

            // 사망 이벤트 호출
            OnDiedEvent?.Invoke(this);

            HasTakenDamage = false; // 피해 입은 상태 초기화

            // 상태 머신 중지 (Null 체크 추가)
            if (StateMachine != null)
                StateMachine.StopMachine();
        }

        /// <summary>
        /// 사망 시 래그돌을 활성화합니다.
        /// </summary>
        protected void ActivateRagdollOnDeath()
        {
            // 무기 비활성화 (Null 체크 추가)
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] != null) weapons[i].enabled = false;
                }
            }

            // 래그돌 활성화 (힘과 함께)
            EnableRagdoll(deathExplosionForce, lastProjectilePosition);

            // 일정 시간 후 래그돌 비활성화 예약
            ragdollCase = Tween.DelayedCall(2.0f, () =>
            {
                ragdoll?.Disable(); // 래그돌 비활성화
            });
        }

        /// <summary>
        /// 지정된 지점에서 힘을 가하여 래그돌을 활성화합니다.
        /// </summary>
        /// <param name="force">가할 힘의 크기</param>
        /// <param name="point">힘을 가할 원점</param>
        private void EnableRagdoll(float force, Vector3 point)
        {
            // 래그돌 컴포넌트의 활성화 함수 호출
            ragdoll?.ActivateWithForce(point, force, deathExplosionRadius);
        }

        /// <summary>
        /// (사용되지 않음 - 래그돌로 대체된 것으로 추정) 죽음 시 단순 낙하 애니메이션을 보여줍니다.
        /// </summary>
        private void ShowDeathFallAnimation()
        {
            // 리지드바디 Null 체크 추가
            if (enemyRigidbody == null) return;

            // 리지드바디 활성화
            enemyRigidbody.isKinematic = false; // 물리 효과 받도록
            enemyRigidbody.useGravity = true; // 중력 사용

            // 폭발 힘 추가 (물리적 날아감 효과)
            enemyRigidbody.AddExplosionForce(deathExplosionForce, transform.position + (transform.forward * 0.5f).SetY(15f) + transform.right * Random.Range(-0.5f, 0.5f), deathExplosionRadius);
        }

        /// <summary>
        /// 피격 시 시각 효과 (메쉬 반짝임)를 재생합니다.
        /// </summary>
        protected void HitEffect()
        {
            // meshRenderer 또는 hitShinePropertyBlock이 Null이면 실행 중지
            if (meshRenderer == null || hitShinePropertyBlock == null) return;

            // 짧은 시간 내 중복 재생 방지
            if (lastHitShineTime + 0.11f > Time.realtimeSinceStartup)
                return;

            lastHitShineTime = Time.realtimeSinceStartup; // 마지막 재생 시간 기록

            hitShineTweenCase.KillActive(); // 이전 효과 중지

            // MaterialPropertyBlock을 사용하여 셰이더의 EmissionColor 속성 변경
            meshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, HIT_OVERLAY_COLOR); // 지정된 오버레이 색상으로 설정
            meshRenderer.SetPropertyBlock(hitShinePropertyBlock);

            // 일정 시간 동안 다시 원래 색상(검정색, Emission 없음)으로 되돌리는 트윈 애니메이션 실행
            hitShineTweenCase = meshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH, hitShinePropertyBlock, Color.black, 0.32f);
        }

        /// <summary>
        /// 메쉬의 마스크 비율을 설정합니다. (스폰/디졸브 효과 등에 사용)
        /// </summary>
        /// <param name="percent">마스크 비율 (0.0 ~ 1.0)</param>
        protected void SetMaskPercent(float percent)
        {
            // meshRenderer 또는 hitShinePropertyBlock이 Null이면 실행 중지
            if (meshRenderer == null || hitShinePropertyBlock == null) return;

            // MaterialPropertyBlock을 사용하여 셰이더의 _MaskPercent 속성 변경
            meshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetFloat(MASK_PERCENT_HASH, percent);
            meshRenderer.SetPropertyBlock(hitShinePropertyBlock);
        }

        /// <summary>
        /// 지정된 인덱스의 피격 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="animationIndex">재생할 피격 애니메이션 인덱스</param>
        protected void HitAnimation(int animationIndex)
        {
            // animatorRef Null 체크 추가
            if (animatorRef == null) return;

            // 애니메이터 트리거 및 파라미터 설정
            animatorRef.SetTrigger(ANIMATOR_HIT_HASH);
            animatorRef.SetInteger(ANIMATOR_HIT_INDEX_HASH, animationIndex);

            // 피격 애니메이션 쿨다운 시간 설정
            hitAnimationTime = Time.time + ANIMATOR_HIT_COOLDOWN;
        }

        /// <summary>
        /// (추상 메서드) 적의 공격 로직을 구현해야 합니다.
        /// 자식 클래스에서 구체적인 공격 방식을 정의합니다. (근접, 원거리 발사 등)
        /// </summary>
        public abstract void Attack();

        /// <summary>
        /// 공격 완료 이벤트를 발생시킵니다. (주로 자식 클래스에서 호출)
        /// </summary>
        protected void InvokeOnAttackFinished()
        {
            OnAttackFinished?.Invoke(); // 이벤트 호출
        }

        #endregion

        #region 드랍 관련 (Drop)

        /// <summary>
        /// 드랍 아이템 목록을 초기화(리셋)합니다.
        /// </summary>
        public void ResetDrop()
        {
            // 기존 목록이 있으면 비우고, 없으면 새로 생성
            if (dropData == null)
                dropData = new List<DropData>();
            else
                dropData.Clear();
        }

        /// <summary>
        /// 드랍할 아이템 데이터를 목록에 추가합니다.
        /// </summary>
        /// <param name="drop">추가할 드랍 데이터</param>
        public void AddDrop(DropData drop)
        {
            // 목록이 초기화되지 않았으면 초기화
            if (dropData == null)
                ResetDrop();

            dropData.Add(drop); // 목록에 추가
        }

/// <summary>
        /// 저장된 드랍 목록과 확률에 따라 자원을 드랍합니다.
        /// </summary>
        protected void DropResources()
        {
            // 게임 플레이 상태가 아니거나 ActiveRoom.LevelData가 Null이면 드랍하지 않음
            // Drop.Instance == null 체크 제거
            if (!LevelController.IsGameplayActive || ActiveRoom.LevelData == null)
                return;

            // 추가된 드랍 데이터가 있으면 드랍 실행
            if (dropData != null && !dropData.IsNullOrEmpty()) // dropData Null 체크 추가
            {
                for (int i = 0; i < dropData.Count; i++)
                {
                    // **오류 수정:** Quaternion 대신 Vector3(오일러 각) 전달
                    Drop.SpawnDropItem(dropData[i], transform.position, new Vector3(0, Random.Range(0f, 360f), 0), false, (drop, fallingStyle) =>
                    {
                        Drop.ThrowItem(drop, fallingStyle); // 아이템 던지는 효과 적용
                    });
                }
            }

            // 확률적으로 힐 아이템 드랍 (stats 및 stats.HpForPlayer Null 체크 추가)
            if (stats != null && stats.HpForPlayer != null && Random.Range(0.0f, 1.0f) <= ActiveRoom.LevelData.HealSpawnPercent)
            {
                // 플레이어에게 줄 체력 회복량 계산 (적 스탯 기반)
                int health = Mathf.RoundToInt(stats.HpForPlayer.Random());

                // 힐 아이템 드랍 데이터 생성 및 스폰
                // **오류 수정:** Quaternion 대신 Vector3(오일러 각) 전달
                Drop.SpawnDropItem(new DropData() { DropType = DropableItemType.Heal, Amount = health }, transform.position, new Vector3(0, Random.Range(0f, 360f), 0), false, (drop, fallingStyle) =>
                {
                    Drop.ThrowItem(drop, fallingStyle); // 아이템 던지는 효과 적용
                });
            }
        }

        #endregion

        #region 이동 관련 (Movement)

        /// <summary>
        /// 지정된 위치로 이동 명령을 내립니다.
        /// </summary>
        /// <param name="pos">목표 위치</param>
        public void MoveToPoint(Vector3 pos)
        {
            // NavMeshAgent가 유효하면 목적지 설정 및 이동 시작 (Null 체크 추가)
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(pos);
            }
        }

        /// <summary>
        /// 현재 이동을 멈춥니다.
        /// </summary>
        public void StopMoving()
        {
            // NavMeshAgent가 유효하고 활성화 상태면 이동 중지 (Null 체크 추가)
            if (gameObject.activeInHierarchy && navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;
        }

        /// <summary>
        /// 현재 속도에 기반하여 이동 애니메이션 속도 배율을 설정합니다.
        /// </summary>
        /// <param name="speedMultiplier">추가 속도 배율 (기본값 1)</param>
        public void SetAnimMovementMultiplier(float speedMultiplier = 1f) // 기본값 1f 설정
        {
            // animatorRef 또는 navMeshAgent가 Null이면 실행 중지
            if (animatorRef == null || navMeshAgent == null) return;

            // 현재 속도 / 최대 속도 * 추가 배율 값으로 애니메이터 파라미터 설정
            if (RunningSpeed > 0.001f) // 0에 매우 가까운 값으로 나누기 방지
            {
                animatorRef.SetFloat(ANIMATOR_SPEED_HASH, navMeshAgent.velocity.magnitude / RunningSpeed * speedMultiplier);
            }
            else
            {
                animatorRef.SetFloat(ANIMATOR_SPEED_HASH, 0f); // 속도가 0이면 애니메이션 속도도 0
            }
        }

        /// <summary>
        /// 순찰 지점 목록을 설정합니다.
        /// </summary>
        /// <param name="points">순찰할 지점들의 배열</param>
        public void SetPatrollingPoints(Vector3[] points)
        {
            patrollingPoints = points;
        }

        #endregion


#if UNITY_EDITOR // 에디터 전용 디버깅/테스트 기능 시작

        /// <summary>
        /// [에디터 버튼] 애니메이션 모드를 토글합니다.
        /// </summary>
        [Button("Toggle Animation Mode")] // NaughtyAttributes 또는 유사 에셋의 기능으로 추정
        private void ToggleAnimationMode()
        {
            if (AnimationMode.InAnimationMode()) // 현재 애니메이션 모드인지 확인
            {
                AnimationMode.StopAnimationMode(); // 애니메이션 모드 중지
                PrefabStage.prefabStageClosing -= OnPrefabClosing; // 프리팹 스테이지 닫힘 이벤트 구독 해제
            }
            else
            {
                AnimationMode.StartAnimationMode(); // 애니메이션 모드 시작
                PrefabStage.prefabStageClosing += OnPrefabClosing; // 프리팹 스테이지 닫힘 이벤트 구독
            }
        }

        /// <summary>
        /// [에디터 콜백] 프리팹 스테이지가 닫힐 때 호출됩니다.
        /// </summary>
        /// <param name="obj">닫히는 프리팹 스테이지 객체</param>
        private void OnPrefabClosing(PrefabStage obj)
        {
            AnimationMode.StopAnimationMode(); // 애니메이션 모드 중지
            PrefabStage.prefabStageClosing -= OnPrefabClosing; // 이벤트 구독 해제
        }

        /// <summary>
        /// [에디터 버튼] 애니메이션 모드일 때, 랜덤 애니메이션 클립을 샘플링하여 보여줍니다.
        /// </summary>
        [Button("Sample Random Animation", "IsAnimationMode", ButtonVisibility.ShowIf)] // 애니메이션 모드일 때만 버튼 표시
        private void SampleRandomAnimation()
        {
            // animatorRef Null 체크 추가
            if (animatorRef == null || animatorRef.runtimeAnimatorController == null)
            {
                 Debug.LogError("Animator or RuntimeAnimatorController is missing.");
                 return;
            }

            if (AnimationMode.InAnimationMode())
            {
                var clips = animatorRef.runtimeAnimatorController.animationClips;
                if (clips == null || clips.Length == 0)
                {
                    Debug.LogError("No animation clips found in the Animator Controller.");
                    return;
                }

                // 현재 애니메이터 컨트롤러의 랜덤 애니메이션 클립 가져오기
                var randomClip = clips.GetRandomItem();
                if (randomClip == null)
                {
                    Debug.LogError("Failed to get a random animation clip.");
                    return;
                }

                // 샘플링 시작
                AnimationMode.BeginSampling();
                // 랜덤 클립의 랜덤한 시간 지점을 샘플링하여 적용
                AnimationMode.SampleAnimationClip(animatorRef.gameObject, randomClip, Random.value);
                // 샘플링 종료
                AnimationMode.EndSampling();
            }
            else
            {
                Debug.LogError("Animation Mode is turned off"); // 애니메이션 모드가 아니면 오류 로그 출력
            }
        }

        /// <summary>
        /// MonoBehaviour: 씬 뷰에서 기즈모를 그릴 때 호출됩니다.
        /// 애니메이션 모드일 때 텍스트를 표시합니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (AnimationMode.InAnimationMode())
            {
                // 씬 뷰에 "ANIMATION MODE" 텍스트 표시
                GUIStyle style = new GUIStyle();
                style.fontSize = 30;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.red;
                // Handles가 Editor 네임스페이스에 있으므로 확인
#if UNITY_EDITOR
                Handles.Label(transform.position, "ANIMATION MODE", style);
#endif
            }
        }

        /// <summary>
        /// [에디터 버튼 조건용] 현재 애니메이션 모드인지 여부를 반환합니다.
        /// </summary>
        /// <returns>애니메이션 모드이면 true</returns>
        protected bool IsAnimationMode()
        {
            return AnimationMode.InAnimationMode();
        }
#endif // 에디터 전용 디버깅/테스트 기능 끝
    }
}