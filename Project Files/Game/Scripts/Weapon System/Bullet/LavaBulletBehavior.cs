// 이 스크립트는 플레이어가 발사하는 용암 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 곡선 이동 및 폭발 범위 피해 기능을 추가합니다.
using UnityEngine;
// using DG.Tweening; 네임스페이스는 DOTween 플러그인 사용에 필요하지만, 현재 코드에 명시적으로 포함되어 있지 않습니다.
// Tweening 관련 기능(DOBezierMove, KillActive 등)은 해당 플러그인이 프로젝트에 추가되어 있음을 가정합니다.

namespace Watermelon.SquadShooter
{
    // PlayerBulletBehavior를 상속받아 플레이어 투사체의 기본 기능을 활용합니다.
    public class LavaBulletBehavior : PlayerBulletBehavior
    {
        // 용암이 명중했을 때 재생할 파티클 시스템 해시 값입니다.
        private readonly static int SPLASH_PARTICLE_HASH = "Lava Hit".GetHashCode();
        // 용암이 벽에 명중했을 때 재생할 파티클 시스템 해시 값입니다.
        private readonly static int WALL_SPLASH_PARTICLE_HASH = "Lava Wall Hit".GetHashCode();

        [Tooltip("투사체 이동 시 재생되는 트레일 파티클 시스템입니다.")]
        [SerializeField] ParticleSystem trailParticleSystem;

        // 폭발 시 피해를 줄 반경입니다.
        private float explosionRadius;
        // 폭발 시 적용될 데미지 값입니다 (최소/최대 값).
        private DuoInt damageValue; // DuoInt는 두 개의 정수 값을 저장하는 사용자 정의 구조체일 수 있습니다.
        // 이 투사체를 발사한 플레이어 캐릭터 행동 컴포넌트입니다.
        private CharacterBehaviour characterBehaviour;

        // 투사체의 곡선 이동(Tween)을 제어하는 TweenCase 객체입니다.
        private TweenCase movementTween;

        // 현재 프레임에서의 투사체 위치입니다.
        private Vector3 position;
        // 이전 프레임에서의 투사체 위치입니다. 폭발 시 데미지 방향 계산에 사용될 수 있습니다.
        private Vector3 prevPosition;

        /// <summary>
        /// 용암 투사체를 초기화하고 목표 지점으로 곡선 이동을 시작합니다.
        /// </summary>
        /// <param name="damage">투사체의 데미지 (여기서는 DuoInt 사용)</param>
        /// <param name="speed">투사체의 이동 속도 (이동 시간 계산에 사용)</param>
        /// <param name="currentTarget">투사체의 목표 적</param>
        /// <param name="autoDisableTime">자동 비활성화 시간 (기본 클래스에서 사용)</param>
        /// <param name="autoDisableOnHit">충돌 시 자동 비활성화 여부 (기본 클래스에서 사용)</param>
        /// <param name="shootingRadius">플레이어의 총 발사 사거리 (곡선 높이 계산에 사용)</param>
        /// <param name="characterBehaviour">투사체를 발사한 캐릭터 행동</param>
        /// <param name="bulletHeight">투사체 곡선의 최소/최대 높이</param>
        /// <param name="explosionRadius">폭발 반경</param>
        public void Init(DuoInt damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit, float shootingRadius, CharacterBehaviour characterBehaviour, DuoFloat bulletHeight, float explosionRadius)
        {
            // 상위 클래스의 Init 함수를 호출하여 기본 투사체 속성을 설정합니다. (기본 데미지는 0f으로 설정)
            base.Init(0f, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            // 용암 투사체 고유 속성을 설정합니다.
            this.explosionRadius = explosionRadius;
            this.characterBehaviour = characterBehaviour;

            // 목표 적 주변에 랜덤한 목표 위치를 설정합니다.
            Vector3 targetPosition = currentTarget.transform.position + new Vector3(Random.Range(-0.6f, 0.6f), 0, Random.Range(-0.6f, 0.6f));

            // 캐릭터와 목표 위치 사이의 거리에 비례하여 곡선 높이 승수를 계산합니다.
            float distanceMultiplier = Mathf.InverseLerp(0, shootingRadius, Vector3.Distance(characterBehaviour.transform.position, targetPosition));
            // 이동 속도를 기반으로 투사체의 예상 비행 시간을 계산합니다.
            float bulletFlyTime = 1 / speed;

            // 데미지 값을 설정합니다.
            damageValue = damage;

            // 트레일 파티클 시스템을 멈추고 재생합니다.
            trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            trailParticleSystem.Play();

            // DOTween을 사용하여 베지어 곡선 이동 애니메이션을 설정합니다.
            // 시작점, 제어점 높이 (거리에 따라 Lerp), 끝점, 비행 시간을 설정하고, 애니메이션 완료 시 OnEnemyHitted 함수를 호출하도록 합니다.
            movementTween = transform.DOBezierMove(targetPosition, Mathf.Lerp(bulletHeight.firstValue, bulletHeight.secondValue, distanceMultiplier), 0, 0, bulletFlyTime).OnComplete(delegate
            {
                // 애니메이션 완료 시 (목표 지점 도달 시) 적 명중 처리 함수를 호출합니다.
                OnEnemyHitted(null); // 목표 적을 특정하지 않고 호출합니다.
            });

            // 비행 시간의 80% 지점에서 사운드를 재생하는 지연 호출을 설정합니다.
            Tween.DelayedCall(bulletFlyTime * 0.8f, () =>
            {
                AudioController.PlaySound(AudioController.AudioClips.shotLavagun, 0.6f);
            });
        }

        // 매 프레임 업데이트 동안 호출됩니다.
        private void Update()
        {
            // 이전 위치를 현재 위치로 업데이트하고 현재 위치를 저장합니다.
            prevPosition = position;
            position = transform.position;
        }

        /// <summary>
        /// 물리 업데이트 동안 호출됩니다.
        /// 이 클래스에서는 곡선 이동을 DOTween으로 처리하므로 이 함수는 비어 있습니다.
        /// </summary>
        protected override void FixedUpdate()
        {
            // 기본 FixedUpdate는 이동을 처리하지만, 용암 투사체는 DOTween으로 이동하므로 여기서는 아무것도 하지 않습니다.
        }

        /// <summary>
        /// 적에게 명중했거나 목표 지점에 도달했을 때 호출됩니다.
        /// 폭발 범위 내의 모든 적에게 피해를 입히고 파티클 및 사운드를 재생합니다.
        /// </summary>
        /// <param name="baseEnemyBehavior">명중한 적 (곡선 이동 완료 시에는 null일 수 있음)</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            // 현재 실행 중인 이동 트윈 애니메이션을 중지합니다.
            movementTween.KillActive();

            // 투사체 위치를 중심으로 폭발 반경 내의 모든 콜라이더를 가져옵니다.
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

            // 충돌한 콜라이더들을 순회합니다.
            for (int i = 0; i < hitColliders.Length; i++)
            {
                // 충돌한 오브젝트의 레이어가 적 레이어인지 확인합니다.
                if (hitColliders[i].gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    // 충돌한 오브젝트에서 BaseEnemyBehavior 컴포넌트를 가져옵니다.
                    BaseEnemyBehavior enemy = hitColliders[i].GetComponent<BaseEnemyBehavior>();
                    // 적 컴포넌트가 존재하고 적이 죽지 않은 상태이면
                    if (enemy != null && !enemy.IsDead)
                    {
                        // 폭발 중심으로부터의 거리에 따라 적용될 데미지 승수를 계산합니다 (중심에서 멀어질수록 승수 감소).
                        float explosionDamageMultiplier = 1.0f - Mathf.InverseLerp(0, explosionRadius, Vector3.Distance(transform.position, hitColliders[i].transform.position));

                        // 적에게 계산된 데미지를 입힙니다. (DuoInt.Lerp를 사용하여 데미지 범위 내에서 승수 적용)
                        // 피해 위치와 데미지 방향도 함께 전달합니다.
                        enemy.TakeDamage(damageValue.Lerp(explosionDamageMultiplier), transform.position, (transform.position - prevPosition).normalized);

                        // 게임 카메라를 가져와서 약한 흔들림 효과를 적용합니다.
                        VirtualCameraCase gameCameraCase = CameraController.GetCamera(CameraType.Game);
                        gameCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
                    }
                }
            }

            // 폭발 사운드를 재생합니다.
            AudioController.PlaySound(AudioController.AudioClips.explode);

            // 트레일 파티클 시스템을 멈추고 초기화합니다.
            trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // 투사체 게임 오브젝트를 비활성화하여 재사용 풀에 반환될 수 있도록 합니다.
            gameObject.SetActive(false);

            // 폭발 스플래시 파티클을 재생합니다.
            ParticlesController.PlayParticle(SPLASH_PARTICLE_HASH).SetPosition(transform.position);
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다.
        /// 기본 장애물 충돌 처리와 함께 벽 스플래시 파티클을 재생합니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            // 상위 클래스의 장애물 충돌 처리 함수를 호출합니다.
            base.OnObstacleHitted();

            // 벽 스플래시 파티클을 재생합니다.
            ParticlesController.PlayParticle(WALL_SPLASH_PARTICLE_HASH).SetPosition(transform.position);
        }
    }
}