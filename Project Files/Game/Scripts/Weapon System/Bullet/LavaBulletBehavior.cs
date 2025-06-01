// 이 스크립트는 플레이어가 발사하는 용암 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 곡선 이동 및 착탄 시 범위 폭발 피해 기능을 추가합니다.
// 이제 범위 폭발로 피해를 입은 각 적에게도 데미지 플로팅 텍스트를 표시합니다.
using UnityEngine;
using Watermelon; // Pool, PoolManager, Tween, ParticlesController, AudioController, FloatingTextController 등
using Watermelon.LevelSystem; // ActiveRoom 사용을 위해 필요합니다.

namespace Watermelon.SquadShooter
{
    public class LavaBulletBehavior : PlayerBulletBehavior
    {
        private readonly static int SPLASH_PARTICLE_HASH = "Lava Hit".GetHashCode();
        private readonly static int WALL_SPLASH_PARTICLE_HASH = "Lava Wall Hit".GetHashCode();

        [Tooltip("투사체 이동 시 재생되는 트레일 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem trailParticleSystem;

        // 폭발 시 피해를 줄 반경입니다.
        private float currentExplosionRadius;
        // 폭발 시 적용될 데미지 값의 범위입니다 (최소/최대 값).
        private DuoInt explosionDamageRange; // Init에서 설정됨

        // 투사체의 곡선 이동(Tween)을 제어하는 TweenCase 객체입니다.
        private TweenCase movementTween;

        // 현재 프레임에서의 투사체 위치입니다.
        private Vector3 currentFramePosition;
        // 이전 프레임에서의 투사체 위치입니다. 폭발 시 데미지 방향 계산에 사용될 수 있습니다.
        private Vector3 previousFramePosition;

        /// <summary>
        /// 용암 투사체를 초기화하고 목표 지점으로 곡선 이동을 시작합니다.
        /// </summary>
        /// <param name="explosionDamage">폭발 시 적용될 데미지 범위 (DuoInt)</param>
        /// <param name="travelSpeed">투사체의 이동 속도 (곡선 이동 시간 계산에 사용)</param>
        /// <param name="initialTargetForProjectile">투사체의 초기 목표 적 (곡선 이동의 최종 목적지 설정에 사용)</param>
        /// <param name="projectileAutoDisableTime">투사체가 자동으로 비활성화될 시간</param>
        /// <param name="projectileDisableOnHit">투사체가 (직접) 충돌 시 비활성화될지 여부 (용암탄은 보통 false, 폭발로 처리)</param>
        /// <param name="gunShotWasCritical">총구 발사 시점의 치명타 여부 (용암탄의 직접 타격이 없으므로 큰 의미는 없으나, 일관성을 위해 전달받음)</param>
        /// <param name="projectileOwner">이 투사체를 발사한 캐릭터의 CharacterBehaviour</param>
        /// <param name="lavaShootingRadius">캐릭터의 발사 사거리 (곡선 높이 계산에 사용)</param>
        /// <param name="lavaBulletHeight">투사체 곡선의 최소/최대 높이</param>
        /// <param name="actualExplosionRadius">실제 폭발 반경</param>
    
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 즉시 폭발 메서드 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        /// <summary>
        /// 외부 또는 내부 조건에 의해 즉시 폭발을 시작합니다.
        /// 진행 중인 이동 트윈을 중지하고 OnEnemyHitted(null)을 호출하여 폭발 로직을 실행합니다.
        /// </summary>
        public void ForceExplosion()
        {
            if (!gameObject.activeSelf) return; // 이미 비활성화 되었다면 중복 실행 방지

            // Debug.Log($"[LavaBulletBehavior] ({gameObject.name}) ForceExplosion 호출됨.");
            movementTween.KillActive(); // 진행 중인 이동 트윈 즉시 중지
            OnEnemyHitted(null); // 폭발 로직 실행 (내부에서 SetActive(false) 처리)
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        public void Init(
            DuoInt explosionDamage,
            float travelSpeed,
            BaseEnemyBehavior initialTargetForProjectile,
            float projectileAutoDisableTime,
            bool projectileDisableOnHit,
            bool gunShotWasCritical,
            CharacterBehaviour projectileOwner,
            float lavaShootingRadius,
            DuoFloat lavaBulletHeight,
            float actualExplosionRadius)
        {
            // PlayerBulletBehavior.Init 호출:
            // 용암탄은 직접 타격 데미지가 없거나 무시할 수 있으므로 baseDamageFromGun을 0f로 전달합니다.
            // gunShotWasCritical은 초기 발사 시점의 치명타 여부입니다.
            base.Init(0f, travelSpeed, initialTargetForProjectile, projectileAutoDisableTime, projectileDisableOnHit, gunShotWasCritical, projectileOwner);

            // 용암 투사체 고유 속성 설정
            this.explosionDamageRange = explosionDamage;
            this.currentExplosionRadius = actualExplosionRadius;
            // this.ownerCharacterBehaviour는 base.Init에서 이미 설정됨

            // 목표 적 주변에 랜덤한 목표 위치 설정 (착탄 지점)
            Vector3 targetPosition = initialTargetForProjectile.transform.position + new Vector3(Random.Range(-0.6f, 0.6f), 0, Random.Range(-0.6f, 0.6f));

            // 캐릭터와 목표 위치 사이의 거리에 비례하여 곡선 높이 승수 계산
            float distanceMultiplier = Mathf.InverseLerp(0, lavaShootingRadius, Vector3.Distance(this.ownerCharacterBehaviour.transform.position, targetPosition));
            // 이동 속도를 기반으로 투사체의 예상 비행 시간 계산
            float bulletFlyTime = 1 / travelSpeed; // travelSpeed는 base.speed로도 접근 가능

            // 트레일 파티클 시스템을 멈추고 재생 (재사용 시 초기화)
            if(trailParticleSystem != null)
            {
                trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                trailParticleSystem.Play();
            }

            // 이동 트윈이 이미 활성화되어 있다면 중지
            movementTween.KillActive();
            // DOTween을 사용하여 베지어 곡선 이동 애니메이션 설정
            movementTween = transform.DOBezierMove(targetPosition, Mathf.Lerp(lavaBulletHeight.firstValue, lavaBulletHeight.secondValue, distanceMultiplier), 0, 0, bulletFlyTime).OnComplete(delegate
            {
                // 애니메이션 완료 시 (목표 지점 도달 시) 적 명중(폭발) 처리 함수 호출
                OnEnemyHitted(null); // baseEnemyBehavior는 null로 전달, 폭발은 특정 대상이 아님
            });

            // 비행 시간의 80% 지점에서 사운드를 재생하는 지연 호출 설정 (기존 로직 유지)
            Tween.DelayedCall(bulletFlyTime * 0.8f, () =>
            {
                AudioController.PlaySound(AudioController.AudioClips.shotLavagun, 0.6f); // 볼륨 조절 가능
            });
        }

        /// <summary>
        /// 매 프레임 업데이트 동안 호출됩니다. 이전 위치와 현재 위치를 기록합니다.
        /// </summary>
        private void Update()
        {
            previousFramePosition = currentFramePosition;
            currentFramePosition = transform.position;
        }

        /// <summary>
        /// 물리 업데이트. 용암 투사체는 DOTween으로 이동하므로 이 함수는 비워둡니다.
        /// </summary>
        protected override void FixedUpdate()
        {
            // PlayerBulletBehavior의 FixedUpdate는 직선 이동을 처리하지만,
            // 용암탄은 DOBezierMove로 이동하므로 이 메서드를 오버라이드하여 비워둡니다.
        }

        /// <summary>
        /// 목표 지점에 도달하여 폭발할 때 호출됩니다.
        /// 폭발 범위 내의 모든 적에게 피해를 입히고 플로팅 텍스트를 생성하며, 파티클 및 사운드를 재생합니다.
        /// </summary>
        /// <param name="enemyThatWasHit">이 메서드는 폭발로 호출되므로 특정 명중 대상은 null입니다.</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior enemyThatWasHit) // 파라미터는 PlayerBulletBehavior와 맞추지만 사용 안함
        {
            movementTween.KillActive(); // 이동 애니메이션 중지

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentExplosionRadius);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    BaseEnemyBehavior enemyInAoE = hitColliders[i].GetComponent<BaseEnemyBehavior>();
                    if (enemyInAoE != null && !enemyInAoE.IsDead)
                    {
                        // 1. 이 폭발 타격에 대한 기본 데미지 계산 (거리 감쇠 등)
                        float explosionDamageMultiplier = 1.0f - Mathf.InverseLerp(0, currentExplosionRadius, Vector3.Distance(transform.position, hitColliders[i].transform.position));
                        float baseAoeDamage = this.explosionDamageRange.Random() * explosionDamageMultiplier; // explosionDamageRange는 Init에서 설정된 DuoInt

                        // 2. 이 폭발 타격에 대한 치명타 여부 및 최종 표시 데미지 계산
                        bool isAoeCrit = false;
                        float finalDamageForText = baseAoeDamage; // 기본적으로 치명타 아닌 데미지

                        if (ownerCharacterBehaviour != null && ownerCharacterBehaviour.Stats != null)
                        {
                            isAoeCrit = Random.value < ownerCharacterBehaviour.Stats.CritChance;
                            if (isAoeCrit)
                            {
                                finalDamageForText = Mathf.RoundToInt(baseAoeDamage * ownerCharacterBehaviour.Stats.CritMultiplier);
                            }
                        }
                        
                        // 3. 적에게 실제 데미지 적용 (TakeDamage는 현재 isCritical 인자를 받지 않음)
                        //    baseAoeDamage를 전달하여 적의 방어력 등을 거치게 함
                        enemyInAoE.TakeDamage(baseAoeDamage, transform.position, (transform.position - previousFramePosition).normalized);

                        // 4. 플로팅 데미지 텍스트 생성
                        Color textColor = isAoeCrit ? CRITICAL_HIT_TEXT_COLOR : Color.white;
                        float textScaleMultiplier = isAoeCrit ? 1.2f : 1.0f;

                        FloatingTextController.SpawnFloatingText(
                            "Hit",
                            finalDamageForText.ToString("F0"),
                            enemyInAoE.transform.position + Vector3.up * 1.5f,
                            Quaternion.identity,
                            textScaleMultiplier,
                            textColor,
                            isAoeCrit,
                            enemyInAoE.gameObject // 빈도 조절 대상
                        );
                    }
                }
            }

            // 폭발 사운드 및 파티클 (루프 외부에서 한 번)
            AudioController.PlaySound(AudioController.AudioClips.explode);
            if(trailParticleSystem != null) trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            ParticlesController.PlayParticle(SPLASH_PARTICLE_HASH).SetPosition(transform.position);

            // 카메라 흔들림 (루프 외부에서 한 번)
            VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
            if(gameCameraCase != null) gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f); // 셰이크 강도 및 시간 조절 가능

            // 투사체 게임 오브젝트 비활성화
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다. (용암탄은 주로 OnEnemyHitted에서 폭발로 처리되므로, 직접 장애물 충돌은 드물 수 있음)
        /// 기본 장애물 충돌 처리와 함께 벽 스플래시 파티클을 재생합니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            // PlayerBulletBehavior의 OnObstacleHitted가 먼저 호출되어 비활성화 및 트윈 중지 처리
            base.OnObstacleHitted();

            // 벽 스플래시 파티클 재생
            ParticlesController.PlayParticle(WALL_SPLASH_PARTICLE_HASH).SetPosition(transform.position);
            
            // 트레일 파티클도 확실히 정리
            if(trailParticleSystem != null) trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            movementTween.KillActive(); // 이동 트윈도 확실히 중지

            // 이미 base.OnObstacleHitted()에서 gameObject.SetActive(false)가 호출될 수 있음
            if (gameObject.activeSelf) 
            {
                gameObject.SetActive(false);
            }
        }
    }
}