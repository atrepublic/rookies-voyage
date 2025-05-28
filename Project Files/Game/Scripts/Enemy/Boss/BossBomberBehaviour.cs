// ==============================================
// 📌 BossBomberBehaviour.cs
// ✅ 보스 폭탄형 적 유닛의 전투, 이동, 폭탄 발사, 애니메이션 제어 스크립트
// ✅ 상태머신과 연동되어 공격 / 추적 / 도주 / 입장 / 사망 등을 처리
// ==============================================

using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class BossBomberBehaviour : BaseEnemyBehavior
    {
        // 이펙트 해시값
        protected static readonly int PARTICLE_BOSS_EAT_HASH = "Boss Eat".GetHashCode();
        private readonly int PARTICLE_STEP_HASH = "Boss Step".GetHashCode();
        private readonly int PARTICLE_DEATH_FALL_HASH = "Boss Death Fall".GetHashCode();
        private readonly int PARTICLE_ENTER_FALL_HASH = "Boss Enter Fall".GetHashCode();
        private readonly int PARTICLE_KICK_HASH = "Boss Kick".GetHashCode();

        // 애니메이터 해시값
        private readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");
        private readonly int ANIMATOR_DIE_HASH = Animator.StringToHash("Death");
        private readonly int ANIMATOR_ENTER_HASH = Animator.StringToHash("Enter");
        private readonly int ANIMATOR_SHOOTING_HASH = Animator.StringToHash("Shooting");
        private readonly int ANIMATOR_KICK_HASH = Animator.StringToHash("Kick");

        [SerializeField] private GameObject graphicsObject;

        [Header("Fighting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private int shotsPerAttempt;
        [SerializeField] private float attackDistanceMin;
        [SerializeField] private float attackDistanceMax;
        [SerializeField] private float kickDistance;
        [SerializeField] private DuoInt damage;
        [SerializeField] private float hitCooldown;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private AnimationCurve yBombMovementCurve;

        [Tooltip("발차기 시 적용되는 거리")]
        public float KickDistance => kickDistance;

        [Tooltip("사격이 가능한 최소 거리")]
        public float AttackDistanceMin => attackDistanceMin;

        [Tooltip("사격이 가능한 최대 거리")]
        public float AttackDistanceMax => attackDistanceMax;

        [Tooltip("타격 쿨다운")]
        public float HitCooldown => hitCooldown;

        [Header("Bomb Settings")]
        [SerializeField] private float bombDamageMin;
        [SerializeField] private float bombDamageMax;
        [SerializeField] private float bombExplosionDuration;
        [SerializeField] private float bombExplosionRadius;
        [SerializeField] private float bombShakeDurationMin;
        [SerializeField] private float bombShakeDurationMax;
        [SerializeField] private float bombShakeDistance;

        [Header("Weapon")]
        [SerializeField] private Transform weaponHolderTransform;
        [SerializeField] private Transform weaponTransform;
        [SerializeField] private Transform shootPointTransform;
        [SerializeField] private ParticleSystem shootParticleSystem;

        [Header("Boss Body")]
        [SerializeField] private Transform leftFootTransform;
        [SerializeField] private Transform rightFootTransform;
        [SerializeField] private Transform backTransform;

        [Space]
        [SerializeField] private Collider bossCollider;
        [SerializeField] private WeaponRigBehavior weaponRigBehavior;

        private float sqrSpeed;
        private Vector3 bombPoint;
        private int shotsAmount;
        private VirtualCameraCase gameCameraCase;

        public event SimpleCallback OnEntered;

        protected override void Awake()
        {
            base.Awake();
            bossCollider.enabled = false;
        }

        public override void Init()
        {
            base.Init();

            sqrSpeed = stats.MoveSpeed * stats.MoveSpeed;
            gameCameraCase = CameraController.GetCamera(CameraType.Game);

            // 무기 초기화
            weaponTransform.SetParent(weaponHolderTransform);
            weaponTransform.ResetLocal();
            weaponTransform.gameObject.SetActive(true);

            // 그래픽 비활성화
            isDead = true;
            graphicsObject.SetActive(false);
            healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);

            weaponRigBehavior.enabled = true;
            weaponTransform.gameObject.SetActive(false);
        }

        public void PerformHit()
        {
            navMeshAgent.SetDestination(transform.position);
            navMeshAgent.isStopped = true;
            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);

            transform.LookAt(target);

            shotsAmount = shotsPerAttempt;
            animatorRef.SetBool(ANIMATOR_SHOOTING_HASH, true);
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);

            bombPoint = target.position;
        }

        public void PerformKick()
        {
            transform.LookAt(target);
            navMeshAgent.isStopped = true;

            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
            animatorRef.SetBool(ANIMATOR_SHOOTING_HASH, false);
            animatorRef.SetTrigger(ANIMATOR_KICK_HASH);

            Tween.DelayedCall(0.3f, () =>
            {
                AudioController.PlaySound(AudioController.AudioClips.punch1);
            });
        }

        public void ChasePlayer()
        {
            navMeshAgent.SetDestination(target.position);
            navMeshAgent.isStopped = false;
            animatorRef.SetBool(ANIMATOR_RUN_HASH, true);
        }

        public void Idle()
        {
            navMeshAgent.SetDestination(transform.position);
            navMeshAgent.isStopped = true;
            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive || isDead)
                return;

            if (navMeshAgent.isStopped)
            {
                var targetRotation = Quaternion.LookRotation(target.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 50f);
            }

            healthbarBehaviour.FollowUpdate();
            animatorRef.SetFloat(ANIMATOR_SPEED_HASH, navMeshAgent.velocity.sqrMagnitude / sqrSpeed);
        }

        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead) return;

            base.TakeDamage(damage, projectilePosition, projectileDirection);

            if (hitAnimationTime < Time.time)
                HitAnimation(Random.Range(0, 2));
        }

        protected override void OnDeath()
        {
            if (isDead) return;

            isDead = true;

            StateMachine.StopMachine();
            navMeshAgent.enabled = false;

            healthbarBehaviour.DisableBar();
            animatorRef.Play(ANIMATOR_DIE_HASH, -1, 0);

            AudioController.PlaySound(AudioController.AudioClips.bossScream, 0.6f);
            DropResources();
            LevelController.OnEnemyKilled(this);
            OnDiedEvent?.Invoke(this);

            weaponRigBehavior.enabled = false;
            ActivateRagdollOnDeath();
        }

        public void OnBombExploded(BossBombBehaviour bossBomb, bool playerHitted)
        {
            if (playerHitted)
            {
                gameCameraCase.Shake(0.04f, 0.04f, bombShakeDurationMax, 1.4f);
                return;
            }

            float distance = Vector3.Distance(characterBehaviour.transform.position, bossBomb.transform.position);
            float distanceMultiplier = 1.0f - Mathf.InverseLerp(0, bombShakeDistance, distance);

            if (distanceMultiplier > 0)
            {
                float shakeDuration = Mathf.Lerp(bombShakeDurationMin, bombShakeDurationMax, distanceMultiplier);
                gameCameraCase.Shake(0.04f, 0.04f, shakeDuration, 1.4f);
            }
        }

        public override void OnAnimatorCallback(EnemyCallbackType type)
        {
            switch (type)
            {
                case EnemyCallbackType.Hit:
                    OnBossHit();
                    break;

                case EnemyCallbackType.BossKick:
                    if (Vector3.Distance(transform.position, target.position) <= kickDistance)
                    {
                        ParticlesController.PlayParticle(PARTICLE_KICK_HASH).SetPosition(leftFootTransform.position);
                        characterBehaviour.TakeDamage(characterBehaviour.MaxHealth * 0.5f);
                    }
                    break;

                case EnemyCallbackType.BossLeftStep:
                    ParticlesController.PlayParticle(PARTICLE_STEP_HASH).SetPosition(leftFootTransform.position);
                    break;

                case EnemyCallbackType.BossRightStep:
                    ParticlesController.PlayParticle(PARTICLE_STEP_HASH).SetPosition(rightFootTransform.position);
                    break;

                case EnemyCallbackType.BossEnterFall:
                    OnBossEnterFall();
                    break;

                case EnemyCallbackType.BossEnterFallFinished:
                    OnEntered?.Invoke();
                    CharacterBehaviour.GetBehaviour().TryAddClosestEnemy(this);
                    break;

                case EnemyCallbackType.BossDeathFall:
                    ParticlesController.PlayParticle(PARTICLE_DEATH_FALL_HASH).SetPosition(backTransform.position);
                    gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);
                    break;
            }
        }

        private void OnBossHit()
        {
            shootParticleSystem.Play();

            GameObject bombObject = Instantiate(bulletPrefab);
            bombObject.transform.position = shootPointTransform.position;
            bombObject.transform.LookAt(bombPoint);

            var bomb = bombObject.GetComponent<BossBombBehaviour>();
            bomb.Init(this, bombExplosionDuration, Random.Range(bombDamageMin, bombDamageMax), bombExplosionRadius);

            bomb.transform.DOMoveXZ(bombPoint.x, bombPoint.z, 1.0f);
            bomb.transform.DOMoveY(bombPoint.y, 1.0f).SetCurveEasing(yBombMovementCurve).OnComplete(() =>
            {
                bomb.OnPlaced();
            });

            shotsAmount--;

            bombPoint = target.position + new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));

            if (shotsAmount <= 0)
            {
                navMeshAgent.SetDestination(target.position);
                navMeshAgent.isStopped = false;
                animatorRef.SetBool(ANIMATOR_SHOOTING_HASH, false);
                animatorRef.SetBool(ANIMATOR_RUN_HASH, true);
            }

            AudioController.PlaySound(AudioController.AudioClips.shoot2, 0.3f);
        }

        private void OnBossEnterFall()
        {
            isDead = false;

            healthbarBehaviour.HealthBarTransform.gameObject.SetActive(true);
            ParticlesController.PlayParticle(PARTICLE_ENTER_FALL_HASH).SetPosition(transform.position);
            gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            AudioController.PlaySound(AudioController.AudioClips.jumpLanding);
            CharacterBehaviour.GetBehaviour().TryAddClosestEnemy(this);

            weaponTransform.gameObject.SetActive(true);

            Tween.DelayedCall(0.4f, () =>
            {
                AudioController.PlaySound(AudioController.AudioClips.bossScream, 0.6f);
            });
        }

        public void Enter()
        {
            graphicsObject.SetActive(true);
            healthbarBehaviour.HealthBarTransform.gameObject.SetActive(true);

            animatorRef.Play(ANIMATOR_ENTER_HASH, -1, 0);
        }

        public override void Attack() { }
    }
}
