// CharacterBehaviour.cs
// 이 스크립트는 플레이어 캐릭터의 주요 행동 로직을 관리합니다.
// ... (기존 주석 및 네임스페이스 선언 유지) ...
using UnityEngine;
using UnityEngine.AI;
using Watermelon; // Watermelon 프레임워크 네임스페이스
using Watermelon.LevelSystem; // Watermelon 레벨 시스템 네임스페이스

namespace Watermelon.SquadShooter
{
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
        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        [Tooltip("체력 회복 시 재생될 파티클 시스템")]
        [SerializeField] ParticleSystem healingParticle;
        [Tooltip("무적 상태(갓 모드)일 때 재생될 파티클 시스템")]
        [SerializeField] ParticleSystem godModeParticle;

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 플레이어 피격 데미지 텍스트 색상 설정 필드 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        [Header("피격 효과 설정")]
        [Tooltip("플레이어가 피격당했을 때 표시될 데미지 텍스트의 색상입니다.")]
        [SerializeField] private Color playerHitDamageTextColor = Color.red; // 기본값 빨간색, 인스펙터에서 변경 가능
                                                                             // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
         
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 수정된 필드: Vector3로 변경 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        /// <summary>
        /// 캐릭터의 체력 바 오프셋 값입니다. (CharacterStageData로부터 설정됨)
        /// </summary>
        private Vector3 characterHealthBarOffset = new Vector3(0, 2.0f, 0); // 기본값 Vector3로 설정
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        [Header("타겟 관련 설정")]
        [Tooltip("적 타겟팅 시 표시될 링 프리팹")]
        [SerializeField] GameObject targetRingPrefab;
        [Tooltip("타겟 링이 활성화되었을 때의 색상")]
        [SerializeField] Color targetRingActiveColor;
        [Tooltip("타겟 링이 비활성화되었을 때(공격 불가 등)의 색상")]
        [SerializeField] Color targetRingDisabledColor;
        [Tooltip("엘리트 몬스터 등 특별한 타겟에 대한 링 색상")]
        [SerializeField] Color targetRingSpecialColor;

        [Space(5)]
        [Tooltip("캐릭터의 조준 범위 및 상태를 시각적으로 표시하는 링 컴포넌트")]
        [SerializeField] AimRingBehavior aimRingBehavior;

        private BaseCharacterGraphics graphics;
        public BaseCharacterGraphics Graphics => graphics;

        private GameObject graphicsPrefab;
        private SkinnedMeshRenderer characterMeshRenderer;

        private MaterialPropertyBlock hitShinePropertyBlock;
        private TweenCase hitShineTweenCase;

        private CharacterStats stats;
        public CharacterStats Stats => stats;

        private BaseGunBehavior gunBehaviour;
        public BaseGunBehavior Weapon => gunBehaviour;

        private GameObject gunPrefabGraphics;

        private float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Health;
        public bool FullHealth => currentHealth == stats.Health;

        public bool IsInvulnerable { get; private set; }

        public bool IsActive => isActive;
        private bool isActive;

        public static Transform Transform => characterBehaviour.transform;

        private MovementSettings movementSettings;
        private MovementSettings movementAimingSettings;
        private MovementSettings activeMovementSettings;
        public MovementSettings MovementSettings => activeMovementSettings;

        private bool isMoving;
        private float speed = 0;
        private Vector3 movementVelocity;
        public Vector3 MovementVelocity => movementVelocity;

        public EnemyDetector EnemyDetector => enemyDetector;

        public bool IsCloseEnemyFound => closestEnemyBehaviour != null;
        public bool IsAttackingAllowed { get; private set; } = true;

        private BaseEnemyBehavior closestEnemyBehaviour;
        public BaseEnemyBehavior ClosestEnemyBehaviour => closestEnemyBehaviour;

        private Transform playerTarget;
        private GameObject targetRing;
        private Renderer targetRingRenderer;
        private TweenCase ringTweenCase;

        private bool isMovementActive = false;
        public bool IsMovementActive => isMovementActive;

        public static bool IsDead { get; private set; } = false;

        public static SimpleCallback OnDied;

        private void Awake()
        {
            agent.enabled = false;
            // characterBehaviour 싱글톤 인스턴스 설정 (중복 방지)
            if (characterBehaviour == null)
            {
                characterBehaviour = this;
            }
            else if (characterBehaviour != this)
            {
                Debug.LogWarning("[CharacterBehaviour] 중복된 인스턴스가 감지되어 현재 오브젝트를 파괴합니다.");
                Destroy(gameObject);
                return;
            }
        }

        public void Init()
        {
            characterBehaviour = this;
            hitShinePropertyBlock = new MaterialPropertyBlock();
            isActive = false;
            enabled = false;

            GameObject tempTarget = new GameObject("[PLAYER_AIM_TARGET]");
            tempTarget.transform.position = transform.position;
            playerTarget = tempTarget.transform;
            // DontDestroyOnLoad(tempTarget); // 필요에 따라

            enemyDetector.Init(this);

            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ HealthBarOffset 설정 로직 수정 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            CharacterData selectedCharacterData = CharactersController.SelectedCharacter;
            if (selectedCharacterData != null)
            {
                CharacterStageData currentStage = selectedCharacterData.GetCurrentStage();
                if (currentStage != null)
                {
                    // CharacterStageData의 HealthBarOffset이 Vector3라고 가정하고 직접 할당
                    this.characterHealthBarOffset = currentStage.HealthBarOffset; 
                    if (healthbarBehaviour != null)
                    {
                        // healthbarBehaviour.Init의 네 번째 인자가 Vector3를 받는다고 가정
                        healthbarBehaviour.Init(transform, this, true, this.characterHealthBarOffset); 
                    }
                }
                else
                {
                    Debug.LogWarning("[CharacterBehaviour] Init: CharacterStageData를 가져올 수 없습니다. 기본 체력 바 오프셋을 사용합니다.");
                    // 기본값 characterHealthBarOffset (Vector3(0, 2.0f, 0)) 사용
                    if (healthbarBehaviour != null) healthbarBehaviour.Init(transform, this, true, this.characterHealthBarOffset);
                }
            }
            else
            {
                 Debug.LogError("[CharacterBehaviour] Init: SelectedCharacterData가 null입니다. 체력 바 오프셋을 설정할 수 없습니다.");
                 if (healthbarBehaviour != null) healthbarBehaviour.Init(transform, this, true, this.characterHealthBarOffset); // 기본값으로 초기화
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            aimRingBehavior.Init(transform);

            if (targetRingPrefab != null)
            {
                targetRing = Instantiate(targetRingPrefab);
                targetRing.transform.position = new Vector3(0f, 0f, -999f);
                targetRingRenderer = targetRing.GetComponent<Renderer>();
                targetRing.SetActive(false);
            }
            else
            {
                Debug.LogError("[CharacterBehaviour] Init: targetRingPrefab이 할당되지 않았습니다!");
            }

            IsDead = false;

            GameSettings settings = GameSettings.GetSettings();
            if (settings != null)
            {
                IsAttackingAllowed = !settings.UseAttackButton;
                if (settings.UseAttackButton)
                {
                    AttackButtonBehavior.onStatusChanged -= OnAttackButtonStatusChanged;
                    AttackButtonBehavior.onStatusChanged += OnAttackButtonStatusChanged;
                }
            }
            else
            {
                Debug.LogError("[CharacterBehaviour] Init: GameSettings를 가져올 수 없습니다!");
                IsAttackingAllowed = true;
            }
        }

        private void OnAttackButtonStatusChanged(bool isPressed)
        {
            IsAttackingAllowed = isPressed;
        }

        public void Reload(bool resetHealth = true)
        {
            if (stats == null) // Stats가 설정되기 전에 Reload가 호출될 수 있음
            {
                Debug.LogWarning("[CharacterBehaviour] Reload: Stats가 아직 설정되지 않았습니다. 체력 초기화가 정확하지 않을 수 있습니다.");
            }
            else if (resetHealth)
            {
                currentHealth = MaxHealth;
            }

            IsDead = false;
            if (healthbarBehaviour != null) // null 체크
            {
                healthbarBehaviour.EnableBar();
                healthbarBehaviour.RedrawHealth();
            }
            if (enemyDetector != null) // null 체크
            {
                enemyDetector.Reload();
                enemyDetector.gameObject.SetActive(false);
            }
            if (graphics != null) // null 체크
            {
                graphics.DisableRagdoll();
                graphics.Reload();
            }
            if (gunBehaviour != null) // null 체크
            {
                gunBehaviour.Reload();
            }
            gameObject.SetActive(true);
        }

        public void ResetDetector()
        {
            if (enemyDetector == null) return; // null 체크
            var radius = enemyDetector.DetectorRadius;
            enemyDetector.SetRadius(0);
            Tween.NextFrame(() => { if (enemyDetector != null) enemyDetector.SetRadius(radius); }, framesOffset: 2, updateMethod: UpdateMethod.FixedUpdate);
        }

        public void Unload()
        {
            if (graphics != null) graphics.Unload();
            if (playerTarget != null) Destroy(playerTarget.gameObject);
            if (aimRingBehavior != null) Destroy(aimRingBehavior.gameObject); // aimRingBehavior도 파괴
            if (healthbarBehaviour != null) healthbarBehaviour.Destroy();
            if (targetRing != null) Destroy(targetRing); // targetRing도 파괴
        }

        public void OnLevelLoaded()
        {
            gunBehaviour?.OnLevelLoaded(); // null 조건부 연산자 사용
        }

        public void OnNavMeshUpdated()
        {
            if (agent != null && agent.isOnNavMesh) // agent null 체크 추가
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }

        public void ActivateAgent()
        {
            if (agent != null) // agent null 체크 추가
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }

        public static void DisableNavmeshAgent()
        {
            if (characterBehaviour != null && characterBehaviour.agent != null) // null 체크 추가
            {
                characterBehaviour.agent.enabled = false;
            }
        }

        public void MakeInvulnerable(float duration)
        {
            IsInvulnerable = true;
            if (godModeParticle != null) godModeParticle.Play(); // null 체크 추가

            Tween.DelayedCall(duration, () => {
                IsInvulnerable = false;
                if (godModeParticle != null) godModeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            });
        }

        public virtual bool TakeDamage(float damageAmount)
        {
            if (currentHealth <= 0 || IsInvulnerable)
                return false;

            // 체력 감소 로직은 여기에 집중
            float previousHealth = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0, MaxHealth);
            float actualDamageTaken = previousHealth - currentHealth; // 실제 감소된 체력량

            if (healthbarBehaviour != null) healthbarBehaviour.OnHealthChanged();

            VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
            if (gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            if (currentHealth <= 0)
            {
                if (healthbarBehaviour != null) healthbarBehaviour.DisableBar();
                OnCloseEnemyChanged(null);
                isActive = false;
                enabled = false;
                if (enemyDetector != null) enemyDetector.gameObject.SetActive(false);
                if (aimRingBehavior != null) aimRingBehavior.Hide();
                OnDeath();
                if (graphics != null) graphics.EnableRagdoll();
                OnDied?.Invoke();
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
            }

            HitEffect();
            AudioController.PlaySound(AudioController.AudioClips.characterHit.GetRandomItem());
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 플레이어 피격 플로팅 텍스트 생성 수정 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            if (actualDamageTaken > 0) // 실제 데미지를 입었을 때만 텍스트 표시
            {
                string damageTextToShow = actualDamageTaken.ToString("F0");
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 수정된 부분: 저장된 Vector3 오프셋 사용 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                // 캐릭터 위치에 Vector3 오프셋을 직접 더하고, 추가적인 랜덤 오프셋 적용
                Vector3 textBasePosition = transform.position + this.characterHealthBarOffset;
                Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.1f, 0.3f), Random.Range(-0.1f, 0.1f)); // Y에도 약간의 랜덤 추가 가능
                Vector3 textSpawnPosition = textBasePosition + randomOffset;
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                
                // 플레이어 피격은 치명타 개념이 없다고 가정하고 isCritical을 false로 전달
                // GameSettings에 "PlayerHit"으로 정의된 FloatingTextCase 사용
                FloatingTextController.SpawnFloatingText(
                    "PlayerHit",                  // GameSettings에 정의된 이름
                    damageTextToShow,
                    textSpawnPosition,
                    Quaternion.identity,
                    1.0f,                         // 기본 스케일 배율
                    playerHitDamageTextColor,     // ★ 인스펙터에서 설정한 색상 사용 ★
                    false,                        // 플레이어 피격은 치명타 없음 (또는 게임 디자인에 따라 결정)
                    this.gameObject               // 빈도 조절 대상
                );
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            return true;
        }

        public void OnDeath()
        {
            if (graphics != null) graphics.OnDeath();
            IsDead = true;
            Tween.DelayedCall(0.5f, LevelController.OnPlayerDied);
        }

        public void SetPosition(Vector3 position)
        {
            if (playerTarget != null) playerTarget.position = position.AddToZ(10f);
            transform.position = position;
            transform.rotation = Quaternion.identity;
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
            }
        }

        protected void HitEffect()
        {
            if (characterMeshRenderer == null || hitShinePropertyBlock == null) return; // null 체크 추가

            hitShineTweenCase.KillActive();
            characterMeshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, Color.white);
            characterMeshRenderer.SetPropertyBlock(hitShinePropertyBlock);
            hitShineTweenCase = characterMeshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH, hitShinePropertyBlock, Color.black, 0.32f);
            if (graphics != null) graphics.PlayHitAnimation();
        }

        #region Gun
        public void SetGun(WeaponData weaponData, WeaponUpgrade weaponUpgradeData, bool playBounceAnimation = false, bool playAnimation = false, bool playParticle = false)
        {
            if (gunBehaviour != null)
                gunBehaviour.OnGunUnloaded();

            if (gunPrefabGraphics != weaponUpgradeData.WeaponPrefab)
            {
                gunPrefabGraphics = weaponUpgradeData.WeaponPrefab;
                if (gunBehaviour != null)
                {
                    Destroy(gunBehaviour.gameObject);
                }

                if (gunPrefabGraphics != null)
                {
                    GameObject gunObject = Instantiate(gunPrefabGraphics);
                    gunObject.SetActive(true);
                    gunBehaviour = gunObject.GetComponent<BaseGunBehavior>();
                    if (graphics != null)
                    {
                        gunBehaviour.InitCharacter(graphics);
                        gunBehaviour.PlaceGun(graphics);
                        graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());
                        gunBehaviour.UpdateHandRig();
                    }
                }
            }

            if (gunBehaviour != null)
            {
                gunBehaviour.Init(this, weaponData);
                Vector3 defaultScale = gunBehaviour.transform.localScale;
                if (playAnimation)
                {
                    gunBehaviour.transform.localScale = defaultScale * 0.8f;
                    gunBehaviour.transform.DOScale(defaultScale, 0.15f).SetEasing(Ease.Type.BackOut);
                }
                if (playBounceAnimation)
                    gunBehaviour.PlayBounceAnimation();
                if (playParticle)
                    gunBehaviour.PlayUpgradeParticle();
            }

            if (enemyDetector != null) enemyDetector.SetRadius(weaponUpgradeData.RangeRadius); // null 체크
            if (aimRingBehavior != null) aimRingBehavior.SetRadius(weaponUpgradeData.RangeRadius); // null 체크
        }

        public void OnGunShooted()
        {
            if (graphics != null) graphics.OnShoot(); // null 체크
        }
        #endregion

        #region Graphics & Stats
        public void SetStats(CharacterStats newStats)
        {
            this.stats = newStats;
            if (this.stats != null) // null 체크
            {
                currentHealth = this.stats.Health;
            }
            if (healthbarBehaviour != null) healthbarBehaviour.OnHealthChanged();
        }

        public void SetGraphics(GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            if (graphicsPrefab != newGraphicsPrefab)
            {
                graphicsPrefab = newGraphicsPrefab;
                AnimatorParameters animatorParameters = null;
                if (graphics != null)
                {
                    animatorParameters = new AnimatorParameters(graphics.CharacterAnimator);
                    if (gunBehaviour != null)
                        gunBehaviour.transform.SetParent(null);
                    graphics.Unload();
                    Destroy(graphics.gameObject);
                }

                if (newGraphicsPrefab != null) // null 체크
                {
                    GameObject graphicObject = Instantiate(newGraphicsPrefab);
                    graphicObject.transform.SetParent(transform);
                    graphicObject.transform.ResetLocal();
                    graphicObject.SetActive(true);
                    graphics = graphicObject.GetComponent<BaseCharacterGraphics>();
                    if (graphics != null) // null 체크
                    {
                        graphics.Init(this);
                        movementSettings = graphics.MovementSettings;
                        movementAimingSettings = graphics.MovementAimingSettings;
                        activeMovementSettings = movementSettings;
                        characterMeshRenderer = graphics.MeshRenderer;
                    }
                    else
                    {
                        Debug.LogError($"[CharacterBehaviour] SetGraphics: 새로 생성된 그래픽 오브젝트 '{newGraphicsPrefab.name}'에 BaseCharacterGraphics 컴포넌트가 없습니다!");
                    }
                }


                if (gunBehaviour != null && graphics != null)
                {
                    gunBehaviour.InitCharacter(graphics);
                    gunBehaviour.PlaceGun(graphics);
                    graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());
                    gunBehaviour.UpdateHandRig();
                }
                if (playParticle && graphics != null)
                    graphics.PlayUpgradeParticle();
                if (playAnimation && graphics != null)
                    graphics.PlayBounceAnimation();
                if (animatorParameters != null && graphics != null && graphics.CharacterAnimator != null)
                    animatorParameters.ApplyTo(graphics.CharacterAnimator);
            }
        }
        #endregion

        public void Activate(bool check = true)
        {
            if (check && isActive)
                return;
            isActive = true;
            enabled = true;
            if (enemyDetector != null) enemyDetector.gameObject.SetActive(true);
            if (aimRingBehavior != null) aimRingBehavior.Show();
            if (graphics != null) graphics.Activate();
            NavMeshController.InvokeOrSubscribe(this);
        }

        public void Disable()
        {
            if (!isActive)
                return;
            isActive = false;
            enabled = false;
            if (agent != null) agent.enabled = false; // null 체크
            if (aimRingBehavior != null) aimRingBehavior.Hide();
            if (targetRing != null) // null 체크
            {
                targetRing.SetActive(false);
                targetRing.transform.SetParent(null);
            }
            if (graphics != null) graphics.Disable();
            closestEnemyBehaviour = null;
            if (isMoving)
            {
                isMoving = false;
                movementVelocity = Vector3.zero;
                if (graphics != null) graphics.OnMovingStoped(); // null 체크
                speed = 0;
            }
        }

        public void MoveForwardAndDisable(float duration)
        {
            if (agent != null) agent.enabled = false; // null 체크
            if (activeMovementSettings != null) // null 체크
            {
                transform.DOMove(transform.position + Vector3.forward * activeMovementSettings.MoveSpeed * duration, duration)
                    .OnComplete(() => { Disable(); });
            } else {
                Debug.LogWarning("[CharacterBehaviour] MoveForwardAndDisable: activeMovementSettings가 null입니다. 이동을 수행할 수 없습니다.");
                Disable(); // 이동 없이 비활성화
            }
        }

        public void DisableAgent()
        {
            if (agent != null) agent.enabled = false; // null 체크
        }

        public void ActivateMovement()
        {
            isMovementActive = true;
            if (aimRingBehavior != null) aimRingBehavior.Show(); // null 체크
        }

        private void Update()
        {
            if (gunBehaviour != null)
                gunBehaviour.UpdateHandRig();

            if (!isActive)
                return;

            var joystick = Control.CurrentControl;

            if (isMovementActive && joystick != null && joystick.IsMovementInputNonZero && joystick.MovementInput.sqrMagnitude > 0.1f) // joystick null 체크
            {
                if (!isMoving)
                {
                    isMoving = true;
                    speed = 0;
                    if (graphics != null) graphics.OnMovingStarted();
                }

                if (activeMovementSettings != null) // null 체크
                {
                    float maxAllowedSpeed = Mathf.Clamp01(joystick.MovementInput.magnitude) * activeMovementSettings.MoveSpeed;
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
                    movementVelocity = transform.forward * speed;
                    transform.position += joystick.MovementInput * Time.deltaTime * speed;
                }


                if (graphics != null && activeMovementSettings != null) // null 체크
                {
                     graphics.OnMoving(Mathf.InverseLerp(0, activeMovementSettings.MoveSpeed, speed), joystick.MovementInput, IsCloseEnemyFound);
                }


                if (!IsCloseEnemyFound)
                {
                    if(joystick.MovementInput.sqrMagnitude > 0.01f) // 0벡터가 아닐 때만 회전
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(joystick.MovementInput.normalized), Time.deltaTime * (activeMovementSettings != null ? activeMovementSettings.RotationSpeed : 10f) ); // activeMovementSettings null 체크
                    }
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    movementVelocity = Vector3.zero;
                    if (graphics != null) graphics.OnMovingStoped();
                    speed = 0;
                }
            }

            if (IsCloseEnemyFound && closestEnemyBehaviour != null && playerTarget != null) // null 체크
            {
                playerTarget.position = Vector3.Lerp(playerTarget.position, new Vector3(closestEnemyBehaviour.transform.position.x, transform.position.y, closestEnemyBehaviour.transform.position.z), Time.deltaTime * (activeMovementSettings != null ? activeMovementSettings.RotationSpeed : 10f)); // activeMovementSettings null 체크
                if((playerTarget.position - transform.position).sqrMagnitude > 0.001f) // LookAt 방향이 0벡터가 아닐 때만
                {
                    transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
                }
            }

            if (targetRing != null) targetRing.transform.rotation = Quaternion.identity; // null 체크
            if (healthbarBehaviour != null) healthbarBehaviour.FollowUpdate(); // null 체크
            if (aimRingBehavior != null) aimRingBehavior.UpdatePosition(); // null 체크
        }

        private void FixedUpdate()
        {
            if (graphics != null) graphics.CustomFixedUpdate(); // null 체크
            if (gunBehaviour != null)
                gunBehaviour.GunUpdate();
        }

        public void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior)
        {
            if (!isActive) return;

            if (enemyBehavior != null)
            {
                if (closestEnemyBehaviour == null && playerTarget != null) // null 체크
                {
                    playerTarget.position = transform.position + transform.forward * 5;
                }

                if (graphics != null) activeMovementSettings = movementAimingSettings != null ? movementAimingSettings : graphics.MovementAimingSettings; // null 체크 및 fallback
                else activeMovementSettings = null;


                closestEnemyBehaviour = enemyBehavior;

                if (targetRing != null && enemyBehavior.Stats != null) // null 체크
                {
                    targetRing.SetActive(true);
                    targetRing.transform.rotation = Quaternion.identity;
                    ringTweenCase.KillActive();
                    targetRing.transform.SetParent(enemyBehavior.transform);
                    targetRing.transform.localScale = Vector3.one * enemyBehavior.Stats.TargetRingSize * 1.4f;
                    targetRing.transform.localPosition = Vector3.zero;
                    ringTweenCase = targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f).SetEasing(Ease.Type.BackIn);
                }

                CameraController.SetEnemyTarget(enemyBehavior);
                SetTargetActive();
                return;
            }

            if (graphics != null) activeMovementSettings = movementSettings != null ? movementSettings : graphics.MovementSettings; // null 체크 및 fallback
            else activeMovementSettings = null;

            closestEnemyBehaviour = null;
            if (targetRing != null) // null 체크
            {
                targetRing.SetActive(false);
                targetRing.transform.SetParent(null);
            }
            CameraController.SetEnemyTarget(null);
        }

        public static BaseEnemyBehavior GetClosestEnemy()
        {
            return characterBehaviour?.enemyDetector?.ClosestEnemy; // null 조건부 연산자
        }

        public static CharacterBehaviour GetBehaviour()
        {
            return characterBehaviour;
        }

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector?.TryAddClosestEnemy(enemy); // null 조건부 연산자
        }

        public void SetTargetActive()
        {
            if (targetRingRenderer == null) return; // null 체크

            if (closestEnemyBehaviour != null && closestEnemyBehaviour.Tier == EnemyTier.Elite)
            {
                targetRingRenderer.material.color = targetRingSpecialColor;
            }
            else
            {
                targetRingRenderer.material.color = targetRingActiveColor;
            }
        }

        public void SetTargetUnreachable()
        {
            if (targetRingRenderer != null) // null 체크
            {
                targetRingRenderer.material.color = targetRingDisabledColor;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                BaseDropBehavior item = other.GetComponent<BaseDropBehavior>();
                if (item != null && item.IsPickable(this) && !item.IsPicked) // null 체크
                {
                    item.Pick();
                }
            }
            else if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                AbstractChestBehavior chest = other.GetComponent<AbstractChestBehavior>();
                if (chest != null) chest.ChestApproached(); // null 체크
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                BaseDropBehavior item = other.GetComponent<BaseDropBehavior>();
                if (item != null && item.IsPickable(this) && !item.IsPicked) // null 체크
                {
                    item.Pick();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                AbstractChestBehavior chest = other.GetComponent<AbstractChestBehavior>();
                if (chest != null) chest.ChestLeft(); // null 체크
            }
        }

        public void Heal(int healAmount)
        {
            if (stats == null) return; // null 체크

            currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, MaxHealth);
            if (healthbarBehaviour != null) healthbarBehaviour.OnHealthChanged();
            if (healingParticle != null) healingParticle.Play();
        }

        public void Jump()
        {
            if (graphics != null) graphics.Jump();
            if (gunBehaviour != null) // null 체크
            {
                gunBehaviour.transform.localScale = Vector3.zero;
                gunBehaviour.gameObject.SetActive(false);
            }
        }

        public void SpawnWeapon()
        {
            if (graphics != null) graphics.EnableRig();
            if (gunBehaviour != null) // null 체크
            {
                gunBehaviour.gameObject.SetActive(true);
                gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
            }
        }

        private void OnDestroy()
        {
            if (healthbarBehaviour != null && healthbarBehaviour.HealthBarTransform != null) // null 체크
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);

            if (aimRingBehavior != null) aimRingBehavior.OnPlayerDestroyed(); // null 체크
            if (targetRing != null) Destroy(targetRing); // targetRing도 파괴

            AttackButtonBehavior.onStatusChanged -= OnAttackButtonStatusChanged; // 이벤트 구독 해제

            // 싱글톤 인스턴스 해제 (현재 오브젝트가 싱글톤 인스턴스일 경우)
            if (characterBehaviour == this)
            {
                characterBehaviour = null;
            }
            // 가상 타겟 오브젝트 파괴
            if (playerTarget != null)
            {
                Destroy(playerTarget.gameObject);
                playerTarget = null;
            }
        }
    }
}