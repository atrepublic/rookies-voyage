// 이 스크립트는 보스 스나이퍼가 발사하는 투사체의 동작을 정의합니다.
// 기본 적 투사체 동작을 상속받으며, 미리 정의된 여러 개의 명중 지점을 순차적으로 따라 이동하는 기능을 추가합니다.
using System.Collections.Generic; // List 사용을 위해 필요합니다.
using UnityEngine;
using Watermelon; // Watermelon 네임스페이스의 다른 기능(예: ParticlesController)을 사용하기 위해 필요합니다.

namespace Watermelon.SquadShooter
{
    // EnemyBulletBehavior를 상속받아 적 투사체의 기본 기능을 활용합니다.
    public class BossSniperBulletBehavior : EnemyBulletBehavior
    {
        // 벽 충돌 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_WAll_HIT_HASH = "Minigun Wall Hit".GetHashCode();

        [Tooltip("투사체와 충돌할 수 있는 레이어 마스크입니다.")]
        [SerializeField] LayerMask collisionLayer; // 현재 코드에서는 직접 사용되지 않지만, 에디터 설정용으로 남겨둡니다.

        // 투사체가 순차적으로 이동할 명중 지점(위치) 목록입니다.
        private List<Vector3> hitPoints;

        // hitPoints 리스트에서 다음에 이동할 명중 지점의 인덱스입니다.
        private int nextHitPointId = 0;
        // 다음에 이동할 명중 지점의 위치를 편리하게 가져오는 프로퍼티입니다.
        private Vector3 NextHitPoint => hitPoints[nextHitPointId];

        /// <summary>
        /// 보스 스나이퍼 투사체를 초기화합니다.
        /// 기본 투사체 정보와 함께 이동할 명중 지점 목록을 설정합니다.
        /// </summary>
        /// <param name="damage">투사체의 데미지 값</param>
        /// <param name="speed">투사체의 이동 속도</param>
        /// <param name="selfDestroyDistance">투사체가 자동으로 파괴될 거리 (이 스크립트에서는 주로 hitPoints로 제어되므로 -1 사용 가능)</param>
        /// <param name="hitPoints">투사체가 순차적으로 방문할 명중 지점 목록</param>
        public void InitBullet(float damage, float speed, float selfDestroyDistance, List<Vector3> hitPoints)
        {
            // 상위 클래스의 Init 함수를 호출하여 기본 투사체 속성을 설정합니다.
            Init(damage, speed, selfDestroyDistance);

            // 전달받은 명중 지점 목록을 복사하여 저장합니다.
            this.hitPoints = new List<Vector3>(hitPoints.ToArray());
            // 다음 명중 지점 인덱스를 0으로 초기화합니다.
            nextHitPointId = 0;
        }

        /// <summary>
        /// 물리 업데이트 동안 호출되며 투사체의 이동을 처리합니다.
        /// FixedUpdate를 사용하는 이유는 물리 계산 및 이동이 프레임 속도에 독립적으로 이루어지도록 하기 위함입니다.
        /// </summary>
        protected override void FixedUpdate()
        {
            // 이 FixedUpdate 프레임 동안 이동할 거리를 계산합니다.
            var distanceTraveledDuringThisFrame = speed * Time.fixedDeltaTime;
            // 현재 위치에서 다음 명중 지점까지의 거리를 계산합니다.
            var distanceToNextHitPoint = (NextHitPoint - transform.position).magnitude;

            // 이 프레임 동안 이동할 거리가 다음 명중 지점까지의 거리보다 크거나 같으면
            if (distanceTraveledDuringThisFrame > distanceToNextHitPoint)
            {
                // 투사체 위치를 다음 명중 지점으로 정확히 설정합니다.
                transform.position = NextHitPoint;

                // 다음 명중 지점 인덱스를 증가시킵니다.
                nextHitPointId++;

                // 모든 명중 지점을 방문했는지 확인합니다.
                if (nextHitPointId >= hitPoints.Count)
                {
                    // 마지막 명중 지점에 도달했으므로 벽 충돌 파티클을 재생하고 투사체를 파괴합니다.
                    ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
                    SelfDestroy();
                }
                else
                {
                    // 아직 방문할 명중 지점이 남아있으므로 벽 충돌 파티클을 재생하고 다음 명중 지점을 바라보도록 회전을 업데이트합니다.
                    ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
                    transform.forward = (NextHitPoint - transform.position).normalized;
                }
            }
            else
            {
                // 이 프레임 동안 이동할 거리가 다음 명중 지점까지의 거리보다 작으면,
                // 다음 명중 지점 방향으로 계산된 거리만큼 투사체를 이동시킵니다.
                var directionToNextHitPoint = (NextHitPoint - transform.position).normalized;
                transform.position += directionToNextHitPoint * distanceTraveledDuringThisFrame;
            }
        }

        /// <summary>
        /// 다른 콜라이더와 충돌했을 때 호출됩니다.
        /// 이 투사체는 플레이어와의 충돌만 처리합니다.
        /// </summary>
        /// <param name="other">충돌한 콜라이더</param>
        protected override void OnTriggerEnter(Collider other)
        {
            // 충돌한 오브젝트의 레이어가 플레이어 레이어인지 확인합니다.
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                // 충돌한 오브젝트에서 CharacterBehaviour 컴포넌트를 가져옵니다.
                var character = other.GetComponent<CharacterBehaviour>();
                // CharacterBehaviour 컴포넌트가 존재하는 경우
                if (character != null)
                {
                    // 캐릭터에게 투사체의 데미지만큼 피해를 입힙니다.
                    character.TakeDamage(damage);

                    // 투사체를 파괴합니다.
                    SelfDestroy();
                }
            }
        }
    }
}