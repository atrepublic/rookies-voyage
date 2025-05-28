/*
 * CharacterBehaviour.cs
 * ---------------------
 * 이 스크립트는 플레이어 캐릭터의 주요 행동 로직을 관리합니다.
 * 여기에는 이동, 조준, 공격, 체력 관리, 무기 교체, 상태 효과, 적 감지,
 * 애니메이션 및 그래픽 업데이트, NavMeshAgent 통합 등이 포함됩니다.
 * IEnemyDetector, IHealth, INavMeshAgent 인터페이스를 구현하여
 * 다른 시스템과의 상호작용을 가능하게 합니다.
 */

using UnityEngine;
using UnityEngine.AI;
using Watermelon; // Watermelon 프레임워크 네임스페이스
using Watermelon.LevelSystem; // Watermelon 레벨 시스템 네임스페이스

namespace Watermelon.SquadShooter
{
    // 플레이어 캐릭터의 행동을 정의하는 메인 클래스
    public class CharacterBehaviour : MonoBehaviour, IEnemyDetector, IHealth, INavMeshAgent
    {
        // 셰이더에서 피격 시 빛나는 효과의 색상 속성 ID (최적화를 위해 미리 가져옴)
        private static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");

        // 싱글톤 패턴을 위한 정적 인스턴스
        private static CharacterBehaviour characterBehaviour;

        [Tooltip("캐릭터의 이동 및 경로 탐색을 담당하는 NavMeshAgent 컴포넌트")]
        [SerializeField] NavMeshAgent agent;
        [Tooltip("주변의 적을 감지하는 컴포넌트")]
        [SerializeField] EnemyDetector enemyDetector;

        [Header("체력 관련 설정")]
        [Tooltip("캐릭터의 체력 바 UI를 관리하는 컴포넌트")]
        [SerializeField] HealthbarBehaviour healthbarBehaviour;
        // 외부에서 체력 바 컴포넌트에 접근하기 위한 프로퍼티
        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        [Tooltip("체력 회복 시 재생될 파티클 시스템")]
        [SerializeField] ParticleSystem healingParticle;
        [Tooltip("무적 상태(갓 모드)일 때 재생될 파티클 시스템")]
        [SerializeField] ParticleSystem godModeParticle;

        [Header("타겟 관련 설정")]
        [Tooltip("적 타겟팅 시 표시될 링 프리팹")]
        [SerializeField] GameObject targetRingPrefab;
        [Tooltip("타겟 링이 활성화되었을 때의 색상")]
        [SerializeField] Color targetRingActiveColor;
        [Tooltip("타겟 링이 비활성화되었을 때(공격 불가 등)의 색상")]
        [SerializeField] Color targetRingDisabledColor;
        [Tooltip("엘리트 몬스터 등 특별한 타겟에 대한 링 색상")]
        [SerializeField] Color targetRingSpecialColor;

        [Space(5)] // 인스펙터에서 시각적 간격을 둠
        [Tooltip("캐릭터의 조준 범위 및 상태를 시각적으로 표시하는 링 컴포넌트")]
        [SerializeField] AimRingBehavior aimRingBehavior;

        // 캐릭터 그래픽 관련
        [Tooltip("캐릭터의 외형 및 애니메이션을 담당하는 컴포넌트")]
        private BaseCharacterGraphics graphics;
        // 외부에서 그래픽 컴포넌트에 접근하기 위한 프로퍼티
        public BaseCharacterGraphics Graphics => graphics;

        [Tooltip("현재 사용 중인 캐릭터 그래픽 프리팹")]
        private GameObject graphicsPrefab;
        [Tooltip("캐릭터 모델의 SkinnedMeshRenderer 컴포넌트 (피격 효과 등에 사용)")]
        private SkinnedMeshRenderer characterMeshRenderer;

        // 피격 시 빛나는 효과를 위한 MaterialPropertyBlock (성능 최적화)
        private MaterialPropertyBlock hitShinePropertyBlock;
        // 피격 효과 트윈 애니메이션 케이스
        private TweenCase hitShineTweenCase;

        [Tooltip("캐릭터의 능력치(체력, 이동 속도 등) 정보")]
        private CharacterStats stats;
        // 외부에서 캐릭터 능력치에 접근하기 위한 프로퍼티
        public CharacterStats Stats => stats;

        // 무기 관련
        [Tooltip("캐릭터가 현재 장착하고 있는 무기 컴포넌트")]
        private BaseGunBehavior gunBehaviour;
        // 외부에서 무기 컴포넌트에 접근하기 위한 프로퍼티
        public BaseGunBehavior Weapon => gunBehaviour;

        [Tooltip("현재 사용 중인 무기 그래픽 프리팹")]
        private GameObject gunPrefabGraphics;

        // 체력 관련
        [Tooltip("캐릭터의 현재 체력")]
        private float currentHealth;

        // 외부에서 현재 체력에 접근하기 위한 프로퍼티
        public float CurrentHealth => currentHealth;
        // 외부에서 최대 체력에 접근하기 위한 프로퍼티 (Stats에서 가져옴)
        public float MaxHealth => stats.Health;
        // 현재 체력이 최대 체력인지 확인하는 프로퍼티
        public bool FullHealth => currentHealth == stats.Health;

        [Tooltip("캐릭터가 현재 무적 상태인지 여부")]
        public bool IsInvulnerable { get; private set; }

        [Tooltip("캐릭터가 활성화되어 있는지 (게임 로직 처리 중인지) 여부")]
        public bool IsActive => isActive;
        private bool isActive; // 내부 활성화 상태 플래그

        // 캐릭터의 Transform에 쉽게 접근하기 위한 정적 프로퍼티
        public static Transform Transform => characterBehaviour.transform;

        // 이동 관련
        [Tooltip("기본 이동 설정 (속도, 가속도 등)")]
        private MovementSettings movementSettings;
        [Tooltip("조준 중 이동 설정 (속도, 가속도 등)")]
        private MovementSettings movementAimingSettings;

        [Tooltip("현재 활성화된 이동 설정 (기본 또는 조준 중)")]
        private MovementSettings activeMovementSettings;
        // 외부에서 현재 이동 설정에 접근하기 위한 프로퍼티
        public MovementSettings MovementSettings => activeMovementSettings;

        [Tooltip("캐릭터가 현재 이동 중인지 여부")]
        private bool isMoving;
        [Tooltip("캐릭터의 현재 속도")]
        private float speed = 0;

        [Tooltip("캐릭터의 현재 이동 속도 벡터")]
        private Vector3 movementVelocity;
        // 외부에서 이동 속도 벡터에 접근하기 위한 프로퍼티
        public Vector3 MovementVelocity => movementVelocity;

        // 외부에서 적 감지 컴포넌트에 접근하기 위한 프로퍼티 (IEnemyDetector 구현)
        public EnemyDetector EnemyDetector => enemyDetector;

        [Tooltip("가까운 적이 감지되었는지 여부")]
        public bool IsCloseEnemyFound => closestEnemyBehaviour != null;
        [Tooltip("공격이 허용된 상태인지 여부 (예: 공격 버튼이 눌렸는지)")]
        public bool IsAttackingAllowed { get; private set; } = true;

        [Tooltip("현재 감지된 가장 가까운 적 캐릭터")]
        private BaseEnemyBehavior closestEnemyBehaviour;
        // 외부에서 가장 가까운 적에 접근하기 위한 프로퍼티
        public BaseEnemyBehavior ClosestEnemyBehaviour => closestEnemyBehaviour;

        [Tooltip("캐릭터가 바라볼 가상의 타겟 Transform (적 추적 등에 사용)")]
        private Transform playerTarget;
        [Tooltip("적 타겟팅 시 생성되는 링 오브젝트 인스턴스")]
        private GameObject targetRing;
        [Tooltip("타겟 링의 Renderer 컴포넌트 (색상 변경 등에 사용)")]
        private Renderer targetRingRenderer;
        // 타겟 링 스케일 애니메이션 트윈 케이스
        private TweenCase ringTweenCase;

        [Tooltip("캐릭터 이동 로직이 활성화되었는지 여부")]
        private bool isMovementActive = false;
        // 외부에서 이동 활성화 상태에 접근하기 위한 프로퍼티
        public bool IsMovementActive => isMovementActive;

        [Tooltip("캐릭터가 사망했는지 여부")]
        public static bool IsDead { get; private set; } = false;

        // 캐릭터 사망 시 호출될 콜백 이벤트
        public static SimpleCallback OnDied;

        /// <summary>
        /// MonoBehaviour: 객체가 활성화되기 전 호출됩니다.
        /// NavMeshAgent를 비활성화 상태로 시작합니다.
        /// </summary>
        private void Awake()
        {
            agent.enabled = false; // 시작 시 NavMeshAgent 비활성화 (초기화 후 활성화)
        }

        /// <summary>
        /// 캐릭터 초기화 함수입니다.
        /// 게임 시작 또는 캐릭터 생성 시 호출되어 필요한 설정과 컴포넌트를 준비합니다.
        /// </summary>
        public void Init()
        {
            characterBehaviour = this; // 정적 인스턴스에 자기 자신 할당

            hitShinePropertyBlock = new MaterialPropertyBlock(); // 피격 효과용 MaterialPropertyBlock 생성

            isActive = false; // 초기 상태는 비활성
            enabled = false; // 스크립트 자체도 비활성화 (Activate 함수로 활성화)

            // 가상 타겟 생성 및 초기화
            GameObject tempTarget = new GameObject("[TARGET]");
            tempTarget.transform.position = transform.position;
            tempTarget.SetActive(true);
            playerTarget = tempTarget.transform;

            // 적 감지기 초기화
            enemyDetector.Init(this);

            // 체력 설정 (Stats가 설정되기 전이므로, SetStats에서 최종 설정됨)
            // currentHealth = MaxHealth; // SetStats에서 처리

            // 체력 바 초기화
            healthbarBehaviour.Init(transform, this, true, CharactersController.SelectedCharacter.GetCurrentStage().HealthBarOffset);

            // 조준 링 초기화
            aimRingBehavior.Init(transform);

            // 타겟 링 프리팹 인스턴스화 및 초기 설정
            targetRing = Instantiate(targetRingPrefab, new Vector3(0f, 0f, -999f), Quaternion.identity); // 화면 밖에 생성
            targetRingRenderer = targetRing.GetComponent<Renderer>();

            aimRingBehavior.Hide(); // 조준 링 숨기기

            IsDead = false; // 사망 상태 초기화

            // 게임 설정에 따라 공격 가능 여부 초기화
            GameSettings settings = GameSettings.GetSettings();
            IsAttackingAllowed = !settings.UseAttackButton;
            if (settings.UseAttackButton)
            {
                // 공격 버튼 사용 시, 버튼 상태 변경 이벤트 구독
                AttackButtonBehavior.onStatusChanged += OnAttackButtonStatusChanged;
            }
        }

        /// <summary>
        /// 공격 버튼 상태 변경 시 호출되는 콜백 함수입니다.
        /// </summary>
        /// <param name="isPressed">버튼이 눌렸는지 여부</param>
        private void OnAttackButtonStatusChanged(bool isPressed)
        {
            IsAttackingAllowed = isPressed; // 공격 허용 상태 업데이트
        }

        /// <summary>
        /// 캐릭터 상태를 재설정합니다. 레벨 재시작 등에 사용됩니다.
        /// </summary>
        /// <param name="resetHealth">체력을 최대로 재설정할지 여부</param>
        public void Reload(bool resetHealth = true)
        {
            // 체력 재설정
            if (resetHealth)
            {
                currentHealth = MaxHealth;
            }

            IsDead = false; // 사망 상태 초기화

            // 체력 바 활성화 및 갱신
            healthbarBehaviour.EnableBar();
            healthbarBehaviour.RedrawHealth();

            // 적 감지기 재설정
            enemyDetector.Reload();
            enemyDetector.gameObject.SetActive(false); // 초기에는 비활성화

            // 그래픽 상태 재설정 (래그돌 비활성화 등)
            graphics.DisableRagdoll();
            graphics.Reload();

            // 무기 상태 재설정
            gunBehaviour.Reload();

            gameObject.SetActive(true); // 캐릭터 게임 오브젝트 활성화
        }

        /// <summary>
        /// 적 감지기의 반경을 잠시 초기화했다가 다시 설정합니다.
        /// 주변 적 목록을 강제로 갱신할 때 사용될 수 있습니다.
        /// </summary>
        public void ResetDetector()
        {
            var radius = enemyDetector.DetectorRadius;
            enemyDetector.SetRadius(0);
            // 다음 FixedUpdate 프레임에서 2 프레임 후에 반경을 원래대로 복구
            Tween.NextFrame(() => enemyDetector.SetRadius(radius), framesOffset: 2, updateMethod: UpdateMethod.FixedUpdate);
        }

        /// <summary>
        /// 캐릭터 관련 리소스를 해제합니다. 게임 종료 또는 씬 전환 시 호출될 수 있습니다.
        /// </summary>
        public void Unload()
        {
            if (graphics != null)
                graphics.Unload(); // 그래픽 리소스 해제

            if (playerTarget != null)
                Destroy(playerTarget.gameObject); // 가상 타겟 오브젝트 파괴

            if (aimRingBehavior != null)
                Destroy(aimRingBehavior.gameObject); // 조준 링 오브젝트 파괴

            if (healthbarBehaviour != null)
                healthbarBehaviour.Destroy(); // 체력 바 파괴
        }

        /// <summary>
        /// 레벨이 로드되었을 때 호출됩니다.
        /// </summary>
        public void OnLevelLoaded()
        {
            if (gunBehaviour != null)
                gunBehaviour.OnLevelLoaded(); // 무기 관련 레벨 로드 처리
        }

        /// <summary>
        /// NavMesh가 업데이트되었을 때 호출됩니다 (INavMeshAgent 인터페이스).
        /// NavMeshAgent를 활성화하고 이동을 시작합니다.
        /// </summary>
        public void OnNavMeshUpdated()
        {
            if (agent.isOnNavMesh) // NavMesh 위에 있는지 확인
            {
                agent.enabled = true; // NavMeshAgent 활성화
                agent.isStopped = false; // 이동 시작
            }
        }

        /// <summary>
        /// NavMeshAgent를 강제로 활성화합니다.
        /// </summary>
        public void ActivateAgent()
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        /// <summary>
        /// NavMeshAgent를 비활성화합니다 (정적 함수).
        /// </summary>
        public static void DisableNavmeshAgent()
        {
            characterBehaviour.agent.enabled = false;
        }

        /// <summary>
        /// 지정된 시간 동안 캐릭터를 무적 상태로 만듭니다.
        /// </summary>
        /// <param name="duration">무적 상태 지속 시간 (초)</param>
        public void MakeInvulnerable(float duration)
        {
            IsInvulnerable = true; // 무적 상태 활성화
            godModeParticle.Play(); // 무적 파티클 재생

            // 지정된 시간 후에 무적 상태 해제
            Tween.DelayedCall(duration, () => {
                IsInvulnerable = false;
                godModeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting); // 파티클 정지 (이미 방출된 파티클은 유지)
            });
        }

        /// <summary>
        /// 캐릭터가 피해를 받습니다 (IHealth 인터페이스).
        /// </summary>
        /// <param name="damage">받는 피해량</param>
        /// <returns>피해를 입었으면 true, 아니면 false (사망, 무적 상태 등)</returns>
        public virtual bool TakeDamage(float damage)
        {
            // 이미 죽었거나 무적 상태면 피해를 받지 않음
            if (currentHealth <= 0 || IsInvulnerable)
                return false;

            // 체력 감소 (0 이하 또는 최대 체력 이상으로 벗어나지 않도록 Clamp)
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);

            // 체력 바 업데이트
            healthbarBehaviour.OnHealthChanged();

            // 카메라 흔들기 효과
            VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
            gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            // 체력이 0 이하가 되면 사망 처리
            if (currentHealth <= 0)
            {
                healthbarBehaviour.DisableBar(); // 체력 바 비활성화
                OnCloseEnemyChanged(null); // 타겟 해제

                isActive = false; // 캐릭터 비활성화
                enabled = false; // 스크립트 비활성화

                enemyDetector.gameObject.SetActive(false); // 적 감지기 비활성화
                aimRingBehavior.Hide(); // 조준 링 숨기기

                OnDeath(); // 사망 로직 호출

                graphics.EnableRagdoll(); // 래그돌 활성화

                OnDied?.Invoke(); // 사망 이벤트 호출

#if MODULE_HAPTIC // Haptic 모듈이 정의된 경우
                Haptic.Play(Haptic.HAPTIC_MEDIUM); // 중간 세기 햅틱 재생
#endif
            }

            HitEffect(); // 피격 시각 효과 재생

            // 피격 사운드 재생 (랜덤 선택)
            AudioController.PlaySound(AudioController.AudioClips.characterHit.GetRandomItem());

#if MODULE_HAPTIC // Haptic 모듈이 정의된 경우
            Haptic.Play(Haptic.HAPTIC_LIGHT); // 약한 세기 햅틱 재생
#endif

            // 피해량 텍스트 표시
            FloatingTextController.SpawnFloatingText("PlayerHit", "-" + damage.ToString("F0"), transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 3.75f, Random.Range(-0.1f, 0.1f)), Quaternion.identity, 1.0f, Color.white);

            return true; // 피해를 입었음을 반환
        }

        /// <summary>
        /// 캐릭터 사망 시 처리 로직입니다.
        /// </summary>
        public void OnDeath()
        {
            graphics.OnDeath(); // 그래픽 관련 사망 처리

            IsDead = true; // 사망 상태 플래그 설정

            // 0.5초 후 레벨 컨트롤러에 플레이어 사망 알림
            Tween.DelayedCall(0.5f, LevelController.OnPlayerDied);
        }

        /// <summary>
        /// 캐릭터의 위치를 지정된 좌표로 즉시 이동시킵니다.
        /// </summary>
        /// <param name="position">새로운 위치</param>
        public void SetPosition(Vector3 position)
        {
            // 가상 타겟 위치도 함께 이동 (카메라 추적 등에 영향)
            playerTarget.position = position.AddToZ(10f); // Z축으로 약간 뒤로
            transform.position = position; // 캐릭터 위치 설정
            transform.rotation = Quaternion.identity; // 회전 초기화

            // NavMeshAgent가 활성화되어 있고 NavMesh 위에 있다면 Warp 기능으로 즉시 이동
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
            }
        }

        /// <summary>
        /// 피격 시 시각 효과 (캐릭터 메쉬 반짝임)를 재생합니다.
        /// </summary>
        protected void HitEffect()
        {
            hitShineTweenCase.KillActive(); // 이전 효과가 진행 중이면 중지

            // MaterialPropertyBlock을 사용하여 셰이더의 EmissionColor 속성 변경
            characterMeshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, Color.white); // 흰색으로 설정
            characterMeshRenderer.SetPropertyBlock(hitShinePropertyBlock);

            // 일정 시간 동안 다시 원래 색상(검정색)으로 되돌리는 트윈 애니메이션 실행
            hitShineTweenCase = characterMeshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH, hitShinePropertyBlock, Color.black, 0.32f);

            graphics.PlayHitAnimation(); // 그래픽 컴포넌트의 피격 애니메이션 재생
        }

        #region 무기 관련 함수 (Gun)

        /// <summary>
        /// 캐릭터의 무기를 설정(교체)합니다.
        /// </summary>
        /// <param name="weapon">설정할 무기의 데이터</param>
        /// <param name="weaponUpgrade">무기의 업그레이드 정보 (프리팹, 사거리 등 포함)</param>
        /// <param name="playBounceAnimation">무기 교체 시 바운스 애니메이션 재생 여부</param>
        /// <param name="playAnimation">무기 교체 시 스케일 애니메이션 재생 여부</param>
        /// <param name="playParticle">무기 교체 시 파티클 효과 재생 여부</param>
        public void SetGun(WeaponData weapon, WeaponUpgrade weaponUpgrade, bool playBounceAnimation = false, bool playAnimation = false, bool playParticle = false)
        {
            // 기존 무기가 있으면 언로드 처리
            if (gunBehaviour != null)
                gunBehaviour.OnGunUnloaded();

            // 교체할 무기의 그래픽 프리팹이 현재와 다르면
            if (gunPrefabGraphics != weaponUpgrade.WeaponPrefab)
            {
                // 새로운 프리팹 정보 저장
                gunPrefabGraphics = weaponUpgrade.WeaponPrefab;

                // 기존 무기 오브젝트 파괴
                if (gunBehaviour != null)
                {
                    Destroy(gunBehaviour.gameObject);
                }

                // 새로운 무기 프리팹이 있으면 인스턴스화
                if (gunPrefabGraphics != null)
                {
                    GameObject gunObject = Instantiate(gunPrefabGraphics);
                    gunObject.SetActive(true);

                    gunBehaviour = gunObject.GetComponent<BaseGunBehavior>(); // 무기 컴포넌트 가져오기

                    // 캐릭터 그래픽이 설정되어 있으면 무기 초기화 및 배치
                    if (graphics != null)
                    {
                        gunBehaviour.InitCharacter(graphics);
                        gunBehaviour.PlaceGun(graphics); // 캐릭터의 손 위치 등에 맞게 배치

                        // 무기에 맞는 발사 애니메이션 설정
                        graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());

                        gunBehaviour.UpdateHandRig(); // 손 위치 IK 업데이트
                    }
                }
            }

            // 무기 컴포넌트 초기화 (데이터 설정 등)
            if (gunBehaviour != null)
            {
                gunBehaviour.Init(this, weapon);

                Vector3 defaultScale = gunBehaviour.transform.localScale; // 기본 스케일 저장

                // 교체 애니메이션 재생 (옵션)
                if (playAnimation)
                {
                    gunBehaviour.transform.localScale = defaultScale * 0.8f; // 작게 시작해서
                    gunBehaviour.transform.DOScale(defaultScale, 0.15f).SetEasing(Ease.Type.BackOut); // 커지는 애니메이션
                }

                // 바운스 애니메이션 재생 (옵션)
                if (playBounceAnimation)
                    gunBehaviour.PlayBounceAnimation();

                // 업그레이드 파티클 재생 (옵션)
                if (playParticle)
                    gunBehaviour.PlayUpgradeParticle();
            }

            // 적 감지기 및 조준 링 반경을 무기의 사거리에 맞게 설정
            enemyDetector.SetRadius(weaponUpgrade.RangeRadius);
            aimRingBehavior.SetRadius(weaponUpgrade.RangeRadius);
        }

        /// <summary>
        /// 무기가 발사되었을 때 호출됩니다. (주로 무기 스크립트에서 호출)
        /// </summary>
        public void OnGunShooted()
        {
            graphics.OnShoot(); // 캐릭터 그래픽의 발사 관련 처리 호출
        }
        #endregion

        #region 그래픽 및 스탯 관련 함수 (Graphics & Stats)

        /// <summary>
        /// 캐릭터의 능력치(스탯)를 설정합니다.
        /// </summary>
        /// <param name="stats">설정할 캐릭터 능력치 데이터</param>
        public void SetStats(CharacterStats stats)
        {
            this.stats = stats; // 능력치 데이터 저장

            currentHealth = stats.Health; // 현재 체력을 최대 체력으로 설정

            // 체력 바가 있으면 체력 변경 사항 반영
            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();

            // [추가] 치명타 능력치는 stats 내부에 포함되어 있으므로 별도 처리 없이 자동 반영됨
            // 사용: stats.CritChance, stats.CritMultiplier → 무기나 공격 판정에서 활용
        }

        /// <summary>
        /// 캐릭터의 외형(그래픽)을 설정(교체)합니다.
        /// </summary>
        /// <param name="newGraphicsPrefab">새로운 캐릭터 그래픽 프리팹</param>
        /// <param name="playParticle">교체 시 파티클 효과 재생 여부</param>
        /// <param name="playAnimation">교체 시 바운스 애니메이션 재생 여부</param>
        public void SetGraphics(GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            // 새로운 그래픽 프리팹이 현재와 다르면
            if (graphicsPrefab != newGraphicsPrefab)
            {
                // 새로운 프리팹 정보 저장
                graphicsPrefab = newGraphicsPrefab;

                AnimatorParameters animatorParameters = null; // 이전 애니메이터 상태 저장용

                // 기존 그래픽이 있으면
                if (graphics != null)
                {
                    // 현재 애니메이터 상태 저장
                    animatorParameters = new AnimatorParameters(graphics.CharacterAnimator);

                    // 무기가 그래픽의 자식으로 있다면 임시로 부모 해제
                    if (gunBehaviour != null)
                        gunBehaviour.transform.SetParent(null);

                    // 기존 그래픽 언로드 및 파괴
                    graphics.Unload();
                    Destroy(graphics.gameObject);
                }

                // 새로운 그래픽 프리팹 인스턴스화 및 설정
                GameObject graphicObject = Instantiate(newGraphicsPrefab);
                graphicObject.transform.SetParent(transform); // 현재 캐릭터의 자식으로 설정
                graphicObject.transform.ResetLocal(); // 로컬 위치/회전/스케일 초기화
                graphicObject.SetActive(true);

                graphics = graphicObject.GetComponent<BaseCharacterGraphics>(); // 그래픽 컴포넌트 가져오기
                graphics.Init(this); // 그래픽 컴포넌트 초기화

                // 이동 설정 가져오기
                movementSettings = graphics.MovementSettings;
                movementAimingSettings = graphics.MovementAimingSettings;
                activeMovementSettings = movementSettings; // 기본 이동 설정으로 시작

                characterMeshRenderer = graphics.MeshRenderer; // 메쉬 렌더러 참조 저장

                // 무기가 있다면 새로운 그래픽에 맞게 재설정
                if (gunBehaviour != null)
                {
                    gunBehaviour.InitCharacter(graphics);
                    gunBehaviour.PlaceGun(graphics);
                    graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());
                    gunBehaviour.UpdateHandRig();
                }

                // 교체 파티클 재생 (옵션)
                if (playParticle)
                    graphics.PlayUpgradeParticle();

                // 바운스 애니메이션 재생 (옵션)
                if (playAnimation)
                    graphics.PlayBounceAnimation();

                // 저장해둔 애니메이터 상태가 있으면 새로운 애니메이터에 적용
                if (animatorParameters != null)
                    animatorParameters.ApplyTo(graphics.CharacterAnimator);
            }
        }
        #endregion

        /// <summary>
        /// 캐릭터를 활성화합니다. 게임 로직 처리를 시작합니다.
        /// </summary>
        /// <param name="check">이미 활성화 상태인지 확인할지 여부</param>
        public void Activate(bool check = true)
        {
            // 이미 활성화 상태이면 중복 실행 방지 (check가 true일 때)
            if (check && isActive)
                return;

            isActive = true; // 활성 상태 플래그 설정
            enabled = true; // 스크립트 활성화 (Update 등 호출 시작)

            enemyDetector.gameObject.SetActive(true); // 적 감지기 활성화

            aimRingBehavior.Show(); // 조준 링 표시

            graphics.Activate(); // 그래픽 관련 활성화 처리

            // NavMesh가 준비되었는지 확인하고 Agent 활성화 요청
            NavMeshController.InvokeOrSubscribe(this);
        }

        /// <summary>
        /// 캐릭터를 비활성화합니다. 게임 로직 처리를 중지합니다.
        /// </summary>
        public void Disable()
        {
            // 이미 비활성 상태이면 중복 실행 방지
            if (!isActive)
                return;

            isActive = false; // 활성 상태 플래그 해제
            enabled = false; // 스크립트 비활성화

            agent.enabled = false; // NavMeshAgent 비활성화

            aimRingBehavior.Hide(); // 조준 링 숨기기

            // 타겟 링 숨기기 및 부모 해제
            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);

            graphics.Disable(); // 그래픽 관련 비활성화 처리

            closestEnemyBehaviour = null; // 가장 가까운 적 정보 초기화

            // 이동 중이었다면 정지 처리
            if (isMoving)
            {
                isMoving = false;
                speed = 0;
            }
        }

        /// <summary>
        /// 지정된 시간 동안 앞으로 이동한 후 캐릭터를 비활성화합니다. (주로 스테이지 클리어 연출 등에 사용)
        /// </summary>
        /// <param name="duration">이동 시간</param>
        public void MoveForwardAndDisable(float duration)
        {
            agent.enabled = false; // NavMeshAgent 비활성화 (직접 이동 제어)

            // 지정된 시간 동안 현재 이동 속도로 전진하는 트윈 실행
            transform.DOMove(transform.position + Vector3.forward * activeMovementSettings.MoveSpeed * duration, duration)
                .OnComplete(() => // 이동 완료 시
                {
                    Disable(); // 캐릭터 비활성화
                });
        }

        /// <summary>
        /// NavMeshAgent만 비활성화합니다.
        /// </summary>
        public void DisableAgent()
        {
            agent.enabled = false;
        }

        /// <summary>
        /// 캐릭터 이동 로직을 활성화합니다.
        /// </summary>
        public void ActivateMovement()
        {
            isMovementActive = true; // 이동 활성화 플래그 설정
            aimRingBehavior.Show(); // 조준 링 표시
        }

        /// <summary>
        /// MonoBehaviour: 매 프레임 호출됩니다.
        /// 캐릭터 이동, 회전, 조준, 애니메이션 업데이트 등을 처리합니다.
        /// </summary>
        private void Update()
        {
            // 무기가 있고 손 IK 업데이트가 필요하면 실행
            if (gunBehaviour != null)
                gunBehaviour.UpdateHandRig();

            // 캐릭터가 비활성 상태면 로직 처리 중단
            if (!isActive)
                return;

            var joystick = Control.CurrentControl; // 현재 조이스틱 입력 가져오기

            // 조이스틱 입력이 있고, 입력 크기가 일정 값 이상이면 (데드존 처리)
            if (isMovementActive && joystick.IsMovementInputNonZero && joystick.MovementInput.sqrMagnitude > 0.1f)
            {
                // 이동 시작 처리
                if (!isMoving)
                {
                    isMoving = true;
                    speed = 0; // 속도 초기화
                    graphics.OnMovingStarted(); // 그래픽 이동 시작 처리
                }

                // 조이스틱 입력 크기에 따른 최대 허용 속도 계산
                float maxAllowedSpeed = Mathf.Clamp01(joystick.MovementInput.magnitude) * activeMovementSettings.MoveSpeed;

                // 현재 속도가 최대 속도보다 크면 감속, 작으면 가속
                if (speed > maxAllowedSpeed)
                {
                    speed -= activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed < maxAllowedSpeed) speed = maxAllowedSpeed;
                }
                else
                {
                    speed += activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed > maxAllowedSpeed) speed = maxAllowedSpeed;
                }

                movementVelocity = transform.forward * speed; // 이동 속도 벡터 계산

                // 조이스틱 입력 방향과 속도에 따라 캐릭터 위치 이동
                // NavMeshAgent를 사용하지 않고 직접 이동 처리 (특정 게임 디자인에 따라 선택)
                // 참고: NavMeshAgent를 사용하려면 agent.velocity 또는 agent.Move 사용
                transform.position += joystick.MovementInput * Time.deltaTime * speed;

                // 그래픽 이동 처리 (애니메이션 파라미터 등 업데이트)
                // 속도를 0~1 사이 값으로 정규화하여 전달
                graphics.OnMoving(Mathf.InverseLerp(0, activeMovementSettings.MoveSpeed, speed), joystick.MovementInput, IsCloseEnemyFound);

                // 가까운 적이 없으면 조이스틱 방향으로 캐릭터 회전
                if (!IsCloseEnemyFound)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(joystick.MovementInput.normalized), Time.deltaTime * activeMovementSettings.RotationSpeed);
                }
            }
            else // 조이스틱 입력이 없거나 작으면
            {
                // 이동 정지 처리
                if (isMoving)
                {
                    isMoving = false;
                    movementVelocity = Vector3.zero; // 속도 벡터 초기화
                    graphics.OnMovingStoped(); // 그래픽 이동 정지 처리
                    speed = 0; // 속도 초기화
                }
            }

            // 가까운 적이 있으면
            if (IsCloseEnemyFound)
            {
                // 적의 위치를 향해 부드럽게 가상 타겟 위치 이동 (Y축은 캐릭터 높이 유지)
                playerTarget.position = Vector3.Lerp(playerTarget.position, new Vector3(closestEnemyBehaviour.transform.position.x, transform.position.y, closestEnemyBehaviour.transform.position.z), Time.deltaTime * activeMovementSettings.RotationSpeed);

                // 가상 타겟 방향으로 캐릭터 회전 (Y축만 사용)
                transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
            }

            // 타겟 링은 항상 정면을 보도록 회전 고정
            targetRing.transform.rotation = Quaternion.identity;

            // 체력 바 위치 업데이트
            if (healthbarBehaviour != null)
                healthbarBehaviour.FollowUpdate();

            // 조준 링 위치 업데이트
            aimRingBehavior.UpdatePosition();
        }

        /// <summary>
        /// MonoBehaviour: 고정된 시간 간격으로 호출됩니다.
        /// 물리 기반 업데이트나 일정한 간격으로 실행되어야 하는 로직에 사용됩니다.
        /// </summary>
        private void FixedUpdate()
        {
            graphics.CustomFixedUpdate(); // 그래픽 관련 FixedUpdate 처리

            // 무기가 있으면 무기 관련 FixedUpdate 처리 (발사 등)
            if (gunBehaviour != null)
                gunBehaviour.GunUpdate();
        }

        /// <summary>
        /// 가장 가까운 적이 변경되었을 때 호출됩니다 (EnemyDetector에서 호출).
        /// </summary>
        /// <param name="enemyBehavior">새롭게 가장 가까워진 적 (없으면 null)</param>
        public void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior)
        {
            if (!isActive) return; // 캐릭터가 비활성 상태면 처리 중단

            // 새로운 적이 감지되면
            if (enemyBehavior != null)
            {
                // 이전에 타겟이 없었다면, 가상 타겟 위치 초기화
                if (closestEnemyBehaviour == null)
                {
                    playerTarget.position = transform.position + transform.forward * 5; // 현재 방향 약간 앞으로
                }

                activeMovementSettings = movementAimingSettings; // 조준 중 이동 설정으로 변경

                closestEnemyBehaviour = enemyBehavior; // 가장 가까운 적 정보 업데이트

                // 타겟 링 활성화 및 설정
                targetRing.SetActive(true);
                targetRing.transform.rotation = Quaternion.identity; // 회전 고정

                ringTweenCase.KillActive(); // 이전 링 애니메이션 중지

                // 타겟 링을 적의 자식으로 설정하고 위치/크기 조정
                targetRing.transform.SetParent(enemyBehavior.transform);
                targetRing.transform.localScale = Vector3.one * enemyBehavior.Stats.TargetRingSize * 1.4f; // 약간 크게 시작해서
                targetRing.transform.localPosition = Vector3.zero; // 로컬 위치는 중앙

                // 원래 크기로 줄어드는 애니메이션
                ringTweenCase = targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f).SetEasing(Ease.Type.BackIn);

                CameraController.SetEnemyTarget(enemyBehavior); // 카메라가 적을 추적하도록 설정

                SetTargetActive(); // 타겟 링 색상 설정 (활성 상태)

                return; // 처리 완료
            }

            // 감지된 적이 없으면 (enemyBehavior == null)
            activeMovementSettings = movementSettings; // 기본 이동 설정으로 변경

            closestEnemyBehaviour = null; // 가장 가까운 적 정보 초기화
            // 타겟 링 비활성화 및 부모 해제
            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);

            CameraController.SetEnemyTarget(null); // 카메라 타겟 해제
        }

        /// <summary>
        /// 가장 가까운 적 객체를 반환합니다 (정적 함수).
        /// </summary>
        /// <returns>가장 가까운 적 BaseEnemyBehavior (없으면 null)</returns>
        public static BaseEnemyBehavior GetClosestEnemy()
        {
            return characterBehaviour.enemyDetector.ClosestEnemy;
        }

        /// <summary>
        /// CharacterBehaviour의 싱글톤 인스턴스를 반환합니다 (정적 함수).
        /// </summary>
        /// <returns>CharacterBehaviour 인스턴스</returns>
        public static CharacterBehaviour GetBehaviour()
        {
            return characterBehaviour;
        }

        /// <summary>
        /// 특정 적을 가장 가까운 적으로 등록 시도합니다 (IEnemyDetector 인터페이스).
        /// </summary>
        /// <param name="enemy">등록 시도할 적</param>
        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector.TryAddClosestEnemy(enemy);
        }

        /// <summary>
        /// 타겟 링의 색상을 활성 상태(공격 가능)로 설정합니다.
        /// 엘리트 몬스터인 경우 특별 색상을 사용합니다.
        /// </summary>
        public void SetTargetActive()
        {
            if (closestEnemyBehaviour != null && closestEnemyBehaviour.Tier == EnemyTier.Elite)
            {
                targetRingRenderer.material.color = targetRingSpecialColor; // 엘리트 색상
            }
            else
            {
                targetRingRenderer.material.color = targetRingActiveColor; // 일반 활성 색상
            }
        }

        /// <summary>
        /// 타겟 링의 색상을 비활성 상태(공격 불가 등)로 설정합니다.
        /// </summary>
        public void SetTargetUnreachable()
        {
            targetRingRenderer.material.color = targetRingDisabledColor; // 비활성 색상
        }

        /// <summary>
        /// MonoBehaviour: 다른 Collider가 트리거 영역에 들어왔을 때 호출됩니다.
        /// 아이템 획득, 상자 상호작용 시작 등에 사용됩니다.
        /// </summary>
        /// <param name="other">충돌한 Collider</param>
        private void OnTriggerEnter(Collider other)
        {
            // 아이템 태그 확인
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                BaseDropBehavior item = other.GetComponent<BaseDropBehavior>();
                // 획득 가능하고 아직 획득되지 않은 아이템이면 획득 처리
                if (item.IsPickable(this) && !item.IsPicked)
                {
                    item.Pick();
                }
            }
            // 상자 태그 확인
            else if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                // 상자에 접근했음을 알림
                other.GetComponent<AbstractChestBehavior>().ChestApproached();
            }
        }

        /// <summary>
        /// MonoBehaviour: 다른 Collider가 트리거 영역 안에 머무는 동안 매 프레임 호출됩니다.
        /// 혹시 Enter에서 놓친 아이템이 있다면 처리합니다.
        /// </summary>
        /// <param name="other">충돌 중인 Collider</param>
        private void OnTriggerStay(Collider other)
        {
            // 아이템 태그 확인
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                BaseDropBehavior item = other.GetComponent<BaseDropBehavior>();
                // 획득 가능하고 아직 획득되지 않은 아이템이면 획득 처리
                if (item.IsPickable(this) && !item.IsPicked)
                {
                    item.Pick();
                }
            }
        }

        /// <summary>
        /// MonoBehaviour: 다른 Collider가 트리거 영역에서 나갔을 때 호출됩니다.
        /// 상자 상호작용 종료 등에 사용됩니다.
        /// </summary>
        /// <param name="other">충돌이 끝난 Collider</param>
        private void OnTriggerExit(Collider other)
        {
            // 상자 태그 확인
            if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                // 상자에서 멀어졌음을 알림
                other.GetComponent<AbstractChestBehavior>().ChestLeft();
            }
        }

        /// <summary>
        /// 캐릭터의 체력을 회복시킵니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        public void Heal(int healAmount)
        {
            // 체력 증가 (최대 체력 초과하지 않도록 Clamp)
            currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, MaxHealth);
            healthbarBehaviour.OnHealthChanged(); // 체력 바 업데이트
            healingParticle.Play(); // 회복 파티클 재생
        }

        /// <summary>
        /// 캐릭터 점프 액션 (특수 연출 등에 사용될 수 있음)
        /// </summary>
        public void Jump()
        {
            graphics.Jump(); // 그래픽 점프 처리
            // 점프 중에는 무기 숨기기
            gunBehaviour.transform.localScale = Vector3.zero;
            gunBehaviour.gameObject.SetActive(false);
        }

        /// <summary>
        /// 숨겨진 무기를 다시 나타나게 합니다. (점프 후 착지 등)
        /// </summary>
        public void SpawnWeapon()
        {
            graphics.EnableRig(); // 무기 IK 리그 활성화
            gunBehaviour.gameObject.SetActive(true); // 무기 오브젝트 활성화
            // 무기가 나타나는 스케일 애니메이션
            gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
        }

        /// <summary>
        /// MonoBehaviour: 객체가 파괴될 때 호출됩니다.
        /// 이벤트 구독 해제 등 리소스 정리 작업을 수행합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 체력 바 오브젝트가 존재하면 파괴
            if (healthbarBehaviour.HealthBarTransform != null)
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);

            // 조준 링 관련 정리 작업 호출
            if (aimRingBehavior != null)
                aimRingBehavior.OnPlayerDestroyed();

            // 공격 버튼 이벤트 구독 해제
            AttackButtonBehavior.onStatusChanged -= OnAttackButtonStatusChanged;
        }
    }
}