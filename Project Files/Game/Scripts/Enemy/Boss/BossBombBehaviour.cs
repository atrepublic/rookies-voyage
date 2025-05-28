// ==============================================
// 📌 BossBombBehaviour.cs
// ✅ 보스가 설치하는 폭탄 오브젝트의 동작을 제어하는 스크립트
// ✅ 설치 후 지정된 시간 뒤 폭발, 범위 내 플레이어에게 피해를 줌
// ✅ 폭발 시 이펙트, 사운드, 카메라 흔들림, 보스 콜백 호출 포함
// ==============================================

using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class BossBombBehaviour : MonoBehaviour
    {
        [Tooltip("폭발 시 출력할 피격 이펙트")]
        private readonly int PARTICLE_HIT_HASH = "Boss Bomb Hit".GetHashCode();

        [Tooltip("폭발 중심 이펙트")]
        private readonly int PARTICLE_EXPLOSION_HASH = "Boss Bomb Explosion".GetHashCode();

        [Tooltip("폭발 범위 시각화 이펙트")]
        private readonly int PARTICLE_EXPLOSION_RADIUS_HASH = "Boss Bomb Radius".GetHashCode();

        [Tooltip("폭탄이 설치 완료되었는지 여부")]
        private bool isPlaced;

        [Tooltip("폭발까지 대기 시간")]
        private float duration;

        [Tooltip("폭발 시 피해량")]
        private float damage;

        [Tooltip("폭발 범위 반경")]
        private float radius;

        [Tooltip("이 폭탄을 생성한 보스 AI")]
        private BossBomberBehaviour bossEnemyBehaviour;

        /// <summary>
        /// 📌 폭탄 초기화 (설치 전 상태)
        /// </summary>
        public void Init(BossBomberBehaviour bossEnemyBehaviour, float duration, float damage, float radius)
        {
            this.bossEnemyBehaviour = bossEnemyBehaviour;
            this.duration = duration;
            this.damage = damage;
            this.radius = radius;

            isPlaced = false;

            transform.localScale = Vector3.one;
            transform.rotation = Random.rotation;
        }

        /// <summary>
        /// 📌 폭탄이 설치되기 전 회전 애니메이션
        /// </summary>
        private void Update()
        {
            if (!isPlaced)
            {
                transform.Rotate(transform.right * Time.deltaTime * 50f, Space.Self);
            }
        }

        /// <summary>
        /// 📌 폭탄 설치 완료 시 호출 (폭발 타이머 시작)
        /// </summary>
        public void OnPlaced()
        {
            isPlaced = true;

            // 폭탄 설치 이펙트
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
            ParticlesController.PlayParticle(PARTICLE_EXPLOSION_RADIUS_HASH)
                               .SetPosition(transform.position)
                               .SetDuration(duration);

            // 일정 시간 후 폭발 애니메이션
            transform.DOScale(2.0f, duration).SetEasing(Ease.Type.CubicIn).OnComplete(() =>
            {
                bool playerHitted = false;

                // 폭발 범위 내 플레이어 탐지
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
                foreach (var collider in hitColliders)
                {
                    if (collider.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
                    {
                        CharacterBehaviour character = collider.GetComponent<CharacterBehaviour>();
                        if (character != null)
                        {
                            // 플레이어에게 피해 적용
                            character.TakeDamage(damage);

                            // 카메라 흔들림 효과
                            var cam = CameraController.GetCamera(CameraType.Game);
                            cam.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                            playerHitted = true;
                        }
                    }
                }

                // 폭발 이펙트 및 사운드
                ParticlesController.PlayParticle(PARTICLE_EXPLOSION_HASH).SetPosition(transform.position);
                AudioController.PlaySound(AudioController.AudioClips.explode);

                // 보스 콜백 호출
                bossEnemyBehaviour?.OnBombExploded(this, playerHitted);

                // 오브젝트 제거
                Destroy(gameObject);
            });
        }
    }
}
