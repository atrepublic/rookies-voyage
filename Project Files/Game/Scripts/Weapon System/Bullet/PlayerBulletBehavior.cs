// 이 스크립트는 플레이어 캐릭터가 발사하는 모든 투사체의 기본 동작을 정의하는 추상 클래스입니다.
// 투사체의 초기화, 이동, 적 및 장애물과의 충돌 처리, 자동 비활성화 기능 등을 포함합니다.
// 추상 클래스이므로 직접 사용되지 않으며, 다른 구체적인 투사체 클래스가 이 클래스를 상속받아 특정 동작을 구현해야 합니다.
using UnityEngine;
// using DG.Tweening; 네임스페이스는 DOTween 플러그인 사용에 필요하지만, 현재 코드에 명시적으로 포함되어 있지 않습니다.
// Tweening 관련 기능(DelayedCall, KillActive 등)은 해당 플러그인이 프로젝트에 추가되어 있음을 가정합니다.

namespace Watermelon.SquadShooter
{
    // 플레이어 투사체를 위한 기본 클래스입니다.
    // 이 컴포넌트를 사용하는 게임 오브젝트에 Collider와 Rigidbody 컴포넌트가 필수적으로 부착되도록 합니다.
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]

    
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        // 투사체의 데미지 값입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected float damage;
        // 투사체의 이동 속도입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected float speed;
        // 충돌 시 투사체를 자동으로 비활성화할지 여부입니다.
        private bool autoDisableOnHit;

        // 자동 비활성화를 위한 TweenCase 객체입니다.
        private TweenCase disableTweenCase;

        // 이 투사체가 현재 목표로 하는 적 객체입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected BaseEnemyBehavior currentTarget;

        /// <summary>
        /// 플레이어 투사체를 초기화합니다.
        /// 데미지, 속도, 목표 적, 자동 비활성화 시간 및 충돌 시 비활성화 여부 등을 설정합니다.
        /// </summary>
        /// <param name="damage">투사체의 데미지 값</param>
        /// <param name="speed">투사체의 이동 속도</param>
        /// <param name="currentTarget">투사체의 목표 적 (선택 사항)</param>
        /// <param name="autoDisableTime">투사체가 자동으로 비활성화될 시간 (0 이하이면 자동 비활성화 없음)</param>
        /// <param name="autoDisableOnHit">충돌 시 투사체를 자동으로 비활성화할지 여부 (기본값 true)</param>
        public virtual void Init(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            // 전달받은 값으로 투사체의 속성을 설정합니다.
            this.damage = damage;
            this.speed = speed;
            this.autoDisableOnHit = autoDisableOnHit;

            // 목표 적을 설정합니다.
            this.currentTarget = currentTarget;

            // 자동 비활성화 시간이 0보다 큰 경우, 해당 시간 후 투사체를 비활성화하는 지연 호출(Tween)을 설정합니다.
            if (autoDisableTime > 0)
            {
                disableTweenCase = Tween.DelayedCall(autoDisableTime, delegate
                {
                    // 투사체 게임 오브젝트를 비활성화합니다.
                    gameObject.SetActive(false);
                });
            }
        }

        /// <summary>
        /// 물리 업데이트 동안 호출되며 투사체의 이동을 처리합니다.
        /// FixedUpdate를 사용하는 이유는 물리 계산 및 이동이 프레임 속도에 독립적으로 이루어지도록 하기 위함입니다.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            // 속도가 0이 아닌 경우에만 투사체의 현재 위치에서 앞 방향(transform.forward)으로 속도와 시간 간격만큼 이동합니다.
            if (speed != 0)
                transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }

        /// <summary>
        /// 다른 콜라이더와 충돌했을 때 호출됩니다.
        /// 적 또는 다른 오브젝트와의 충돌을 처리합니다.
        /// </summary>
        /// <param name="other">충돌한 콜라이더</param>
private void OnTriggerEnter(Collider other)
{
//    Debug.Log($"[PlayerBullet] OnTriggerEnter with: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}, Tag: {other.gameObject.tag}");

    if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
    {
        // ... (기존 적 처리 로직)
        BaseEnemyBehavior baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
        if (baseEnemyBehavior != null)
        {
            // IsDead 상태를 OnEnemyHitted 호출 직전에 한 번 더 명확히 로깅
      //      Debug.Log($"[PlayerBullet] Enemy identified: {baseEnemyBehavior.gameObject.name}, IsDead status before calling OnEnemyHitted: {baseEnemyBehavior.IsDead}");
            if (!baseEnemyBehavior.IsDead)
            {
                disableTweenCase.KillActive();

                if (autoDisableOnHit)
                    gameObject.SetActive(false);

                baseEnemyBehavior.TakeDamage(damage, transform.position, transform.forward);
                OnEnemyHitted(baseEnemyBehavior);
            }
            else
            {
      //          Debug.Log($"[PlayerBullet] Enemy {baseEnemyBehavior.gameObject.name} is already dead. No damage/OnHit call.");
                // 이미 죽은 적과 충돌 시, 원한다면 여기서도 투사체를 비활성화할 수 있습니다. (선택 사항)
                // if (autoDisableOnHit) gameObject.SetActive(false); 
                // 아니면 그냥 통과시키거나 다른 로직을 적용
            }
        }
    }
    else // 적 레이어가 아닌 다른 것과 충돌
    {
 //       Debug.LogWarning($"[PlayerBullet] Hit an obstacle: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}. Calling OnObstacleHitted.");
        OnObstacleHitted();
    }
}

        /// <summary>
        /// 이 오브젝트가 비활성화될 때 호출되는 함수입니다.
        /// </summary>
        private void OnDisable()
        {
            // 현재 실행 중인 자동 비활성화 트윈 애니메이션을 중지합니다.
            disableTweenCase.KillActive();
        }

        /// <summary>
        /// 이 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// </summary>
        private void OnDestroy()
        {
            // 현재 실행 중인 자동 비활성화 트윈 애니메이션을 중지합니다.
            disableTweenCase.KillActive();
        }

        /// <summary>
        /// 적에게 명중했을 때 호출되는 추상 함수입니다.
        /// 구체적인 투사체 동작 클래스에서 반드시 구현해야 합니다.
        /// </summary>
        /// <param name="baseEnemyBehavior">명중한 적 객체</param>
        protected abstract void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior);

        /// <summary>
        /// 장애물에 명중했을 때 호출되는 가상 함수입니다.
        /// 기본적으로 자동 비활성화 트윈을 중지하고 투사체를 비활성화합니다.
        /// 상속받는 클래스에서 필요에 따라 오버라이드할 수 있습니다.
        /// </summary>
        protected virtual void OnObstacleHitted()
        {
            // 현재 실행 중인 자동 비활성화 트윈 애니메이션을 중지합니다.
            disableTweenCase.KillActive();

            // 투사체 게임 오브젝트를 비활성화합니다.
            gameObject.SetActive(false);
        }
    }
}