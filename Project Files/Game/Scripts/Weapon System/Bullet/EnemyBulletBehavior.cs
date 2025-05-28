// 이 스크립트는 적 캐릭터가 발사하는 일반 투사체의 기본 동작을 정의합니다.
// 이동, 자동 파괴 거리 제한, 플레이어 및 장애물과의 충돌 처리 등을 담당합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // MonoBehaviour를 상속받아 Unity 게임 오브젝트에 부착될 수 있는 컴포넌트가 됩니다.
    public class EnemyBulletBehavior : MonoBehaviour
    {
        // 플레이어 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_HIT_HASH = "Shotgun Hit".GetHashCode();
        // 벽 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_WALL_HIT_HASH = "Shotgun Wall Hit".GetHashCode();

        // 투사체의 데미지 값입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected float damage;
        // 투사체의 이동 속도입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected float speed;

        // 투사체가 자동으로 파괴될 최대 이동 거리입니다 (-1은 거리 제한 없음을 의미).
        protected float selfDestroyDistance;
        // 투사체가 현재까지 이동한 총 거리입니다.
        protected float distanceTraveled = 0;

        // 투사체 비활성화를 위한 TweenCase 객체 (현재 코드에서 사용되지 않음).
        protected TweenCase disableTweenCase;

        /// <summary>
        /// 적 투사체를 초기화합니다.
        /// 데미지, 속도, 자동 파괴 거리 등을 설정하고 게임 오브젝트를 활성화합니다.
        /// </summary>
        /// <param name="damage">투사체의 데미지 값</param>
        /// <param name="speed">투사체의 이동 속도</param>
        /// <param name="selfDestroyDistance">투사체가 자동으로 파괴될 거리 (-1은 제한 없음)</param>
        public virtual void Init(float damage, float speed, float selfDestroyDistance)
        {
            // 전달받은 값으로 투사체의 속성을 설정합니다.
            this.damage = damage;
            this.speed = speed;

            // 자동 파괴 거리와 이동 거리를 초기화합니다.
            this.selfDestroyDistance = selfDestroyDistance;
            distanceTraveled = 0;

            // 투사체 게임 오브젝트를 활성화합니다.
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 물리 업데이트 동안 호출되며 투사체의 이동을 처리합니다.
        /// FixedUpdate를 사용하는 이유는 물리 계산 및 이동이 프레임 속도에 독립적으로 이루어지도록 하기 위함입니다.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            // 투사체의 현재 위치에서 앞 방향(transform.forward)으로 속도와 시간 간격만큼 이동합니다.
            transform.position += transform.forward * speed * Time.fixedDeltaTime;

            // 자동 파괴 거리 제한이 설정되어 있는 경우 (-1이 아닌 경우)
            if (selfDestroyDistance != -1)
            {
                // 이동 거리를 누적합니다.
                distanceTraveled += speed * Time.fixedDeltaTime;

                // 이동 거리가 자동 파괴 거리를 넘어서면
                if (distanceTraveled >= selfDestroyDistance)
                {
                    // 투사체를 파괴합니다.
                    SelfDestroy();
                }
            }
        }

        /// <summary>
        /// 다른 콜라이더와 충돌했을 때 호출됩니다.
        /// 플레이어 또는 장애물과의 충돌을 처리합니다.
        /// </summary>
        /// <param name="other">충돌한 콜라이더</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            // 충돌한 오브젝트의 레이어가 플레이어 레이어인지 확인합니다.
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                // 충돌한 오브젝트에서 CharacterBehaviour 컴포넌트를 가져옵니다.
                CharacterBehaviour characterBehaviour = other.GetComponent<CharacterBehaviour>();
                // CharacterBehaviour 컴포넌트가 존재하는 경우
                if (characterBehaviour != null)
                {
                    // 플레이어에게 투사체의 데미지만큼 피해를 입힙니다.
                    // TakeDamage 함수가 피해를 입혔는지 여부를 반환하는 경우 (true)
                    if (characterBehaviour.TakeDamage(damage))
                    {
                        // 플레이어 명중 파티클을 재생합니다.
                        ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
                    }

                    // 투사체를 파괴합니다.
                    SelfDestroy();
                }
            }
            // 충돌한 오브젝트의 레이어가 장애물 레이어인지 확인합니다.
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                // 투사체를 파괴합니다.
                SelfDestroy();

                // 벽 명중 파티클을 재생합니다.
                ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            }
        }

        /// <summary>
        /// 투사체 게임 오브젝트를 파괴합니다.
        /// </summary>
        public void SelfDestroy()
        {
            // 이 스크립트가 부착된 게임 오브젝트를 파괴합니다.
            Destroy(gameObject);
        }
    }
}