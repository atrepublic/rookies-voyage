// PlayerBulletBehavior.cs
// 이 스크립트는 플레이어 캐릭터가 발사하는 모든 투사체의 기본 동작을 정의하는 추상 클래스입니다.
// 투사체의 초기화, 이동, 적 및 장애물과의 충돌 처리, 자동 비활성화 기능 등을 포함하며,
// 이제 명중 시 플로팅 데미지 텍스트 생성 책임을 가집니다.
using UnityEngine;
using Watermelon; // FloatingTextController, Tween 등 Watermelon 프레임워크 네임스페이스 사용

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 플레이어가 발사하는 모든 투사체의 기본 행동을 정의하는 추상 클래스입니다.
    /// Collider와 Rigidbody 컴포넌트가 반드시 부착되어야 합니다.
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        [Header("투사체 기본 속성")]
        [Tooltip("투사체의 기본 이동 속도입니다.")]
        [SerializeField] protected float speed; // 이제 Init에서 설정되므로, 인스펙터 노출은 선택사항

        [Tooltip("충돌 시 투사체를 자동으로 비활성화할지 여부입니다.")]
        [SerializeField] private bool autoDisableOnHit = true; // 기본값을 true로 명시

        // 이 투사체의 현재 데미지 값입니다. 총기에서 계산되어 Init으로 전달되며,
        // 테슬라 총알의 연쇄 공격 등에서 내부적으로 변경될 수 있습니다.
        protected float currentDamage;

        // 투사체의 자동 비활성화를 처리하는 TweenCase 객체입니다.
        private TweenCase autoDisableTweenCase;

        // 투사체의 초기 목표 대상입니다. (일부 총알 타입에서 사용될 수 있음)
        protected BaseEnemyBehavior initialTarget;

        /// <summary>
        /// 이 투사체가 총기에서 발사될 때 치명타였는지 여부입니다.
        /// 이 정보는 TakeDamage에 전달되어 적의 특수 반응 유도에 사용될 수 있지만,
        /// 플로팅 텍스트의 치명타 표시는 각 타격 시점에 새로 독립적으로 계산됩니다.
        /// </summary>
        protected bool wasInitialShotCritical;

        /// <summary>
        /// 이 투사체를 발사한 캐릭터의 CharacterBehaviour 참조입니다.
        /// 치명타 계산 등 캐릭터 스탯 접근에 사용됩니다.
        /// </summary>
        protected CharacterBehaviour ownerCharacterBehaviour;

        /// <summary>
        /// 치명타 발생 시 플로팅 텍스트에 사용될 기본 색상입니다.
        /// </summary>
        protected static readonly Color CRITICAL_HIT_TEXT_COLOR = new Color(1f, 0.4f, 0f); // (예: 주황색)

        /// <summary>
        /// 플레이어 투사체를 초기화합니다.
        /// </summary>
        /// <param name="baseDamageFromGun">총기에서 계산된 기본 데미지 값 (이미 치명타 배율이 적용되었을 수도 있고, 아닐 수도 있음. 이 값은 총알의 기본 공격력으로 사용됨)</param>
        /// <param name="bulletSpeed">투사체의 이동 속도</param>
        /// <param name="target">투사체의 초기 목표 적 (선택 사항)</param>
        /// <param name="autoDisableDuration">투사체가 자동으로 비활성화될 때까지의 시간 (0 이하면 자동 비활성화 없음)</param>
        /// <param name="disableOnHit">충돌 시 투사체를 자동으로 비활성화할지 여부</param>
        /// <param name="isCritFromGun">총구 발사 시점의 치명타 여부</param>
        /// <param name="owner">이 투사체를 발사한 캐릭터의 CharacterBehaviour</param>
        public virtual void Init(float baseDamageFromGun, float bulletSpeed, BaseEnemyBehavior target, float autoDisableDuration, bool disableOnHit, bool isCritFromGun, CharacterBehaviour owner)
        {
            this.currentDamage = baseDamageFromGun;
            this.speed = bulletSpeed;
            this.initialTarget = target; // 일부 총알 타입은 이 초기 타겟을 사용
            this.autoDisableOnHit = disableOnHit;
            this.wasInitialShotCritical = isCritFromGun; // 총구 발사 시점의 치명타 상태 저장
            this.ownerCharacterBehaviour = owner;       // 발사자 정보 저장

            // 기존 자동 비활성화 트윈이 있다면 확실히 중지
            if (autoDisableTweenCase != null && autoDisableTweenCase.IsActive)
            {
                autoDisableTweenCase.KillActive();
            }

            // 자동 비활성화 시간이 설정된 경우, 지연 호출 설정
            if (autoDisableDuration > 0)
            {
                autoDisableTweenCase = Tween.DelayedCall(autoDisableDuration, () =>
                {
                    if (gameObject.activeSelf) // 비활성화 전 항상 현재 활성 상태인지 확인
                    {
                        gameObject.SetActive(false);
                    }
                });
            }
        }

        /// <summary>
        /// 물리 업데이트마다 호출되어 투사체의 직선 이동을 처리합니다.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (speed != 0) // 속도가 0이 아니면 이동
            {
                transform.position += transform.forward * speed * Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// 다른 콜라이더와 충돌(Trigger)했을 때 호출됩니다.
        /// 적 또는 장애물과의 충돌을 처리하고, 데미지 적용 및 플로팅 텍스트 생성을 담당합니다.
        /// </summary>
        /// <param name="other">충돌한 다른 콜라이더</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!gameObject.activeSelf) return;
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 라바탄 직접 충돌 시 즉시 폭발 처리 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        if (this is LavaBulletBehavior lavaBullet)
        {
            // 라바탄은 적이든 장애물이든 직접 충돌하면 바로 폭발 로직을 수행하도록 함
            // (OnEnemyHitted(null)이 폭발을 의미하므로, 충돌 대상을 전달할 필요 없음)
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY || other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                // Debug.Log($"[PlayerBulletBehavior] LavaBullet 직접 충돌 감지: {other.gameObject.name}. 즉시 OnEnemyHitted(null) 호출하여 폭발.");
                autoDisableTweenCase.KillActive(); // 진행 중이던 자동 비활성화 및 이동 트윈 중지 (LavaBullet의 movementTween도 여기서 중지해야 함)
                lavaBullet.ForceExplosion(); // LavaBulletBehavior에 즉시 폭발을 위한 public 메서드 추가 고려
                return; // 이 OnTriggerEnter의 나머지 로직(일반 데미지 처리, 플로팅 텍스트) 실행 안 함
            }
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 라바탄 예외 처리 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                BaseEnemyBehavior enemyHit = other.GetComponent<BaseEnemyBehavior>();
                if (enemyHit != null && !enemyHit.IsDead)
                {
                    float damageToApply = this.currentDamage;

                    bool currentImpactIsCrit = false;
                    float damageForFloatingText = damageToApply;

                    if (ownerCharacterBehaviour != null && ownerCharacterBehaviour.Stats != null)
                    {
                        currentImpactIsCrit = Random.value < ownerCharacterBehaviour.Stats.CritChance;
                        if (currentImpactIsCrit)
                        {
                            damageForFloatingText = Mathf.RoundToInt(damageToApply * ownerCharacterBehaviour.Stats.CritMultiplier);
                        }
                    }

                    enemyHit.TakeDamage(damageToApply, transform.position, transform.forward);

                    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ textColorToUse 선언 위치 및 값 할당 수정 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                    Color textColorToUse; // if 블록 외부에서 선언
                    float textScaleMultiplier = currentImpactIsCrit ? 1.2f : 1.0f;

                    if (this is UC_PetBulletBehavior petBullet)
                    {
                        // 펫 총알인 경우, 펫 전용 색상 사용. 펫은 치명타가 없다고 가정하므로 currentImpactIsCrit는 무시될 수 있음
                        // (또는 펫도 치명타가 있다면 currentImpactIsCrit에 따라 분기)
                        textColorToUse = petBullet.PetHitFloatingTextColor; // UC_PetBulletBehavior에 이 프로퍼티가 public으로 있어야 함
                        // 펫 공격은 보통 치명타 표시를 다르게 하지 않으므로, currentImpactIsCrit는 false로 간주하거나
                        // 펫 전용 치명타 로직이 있다면 그것을 따름. 여기서는 PlayerBulletBehavior의 currentImpactIsCrit를 그대로 사용.
                    }
                    else
                    {
                        // 플레이어 총알인 경우, 치명타 여부에 따라 색상 결정
                        textColorToUse = currentImpactIsCrit ? CRITICAL_HIT_TEXT_COLOR : Color.white;
                    }
                    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

                    FloatingTextController.SpawnFloatingText(
                        "Hit",
                        damageForFloatingText.ToString("F0"),
                        enemyHit.transform.position + Vector3.up * 1.5f,
                        Quaternion.identity,
                        textScaleMultiplier,
                        textColorToUse, // 수정된 변수 사용
                        currentImpactIsCrit, // 이 타격의 치명타 여부
                        enemyHit.gameObject
                    );

                    autoDisableTweenCase.KillActive();
                    if (autoDisableOnHit)
                    {
                        gameObject.SetActive(false);
                    }

                    OnEnemyHitted(enemyHit);
                }
            }
            else
            {
                OnObstacleHitted();
            }
        }

        /// <summary>
        /// 이 게임 오브젝트가 비활성화될 때 호출됩니다.
        /// 진행 중인 자동 비활성화 트윈을 중지합니다.
        /// </summary>
        private void OnDisable()
        {
            autoDisableTweenCase.KillActive();
        }

        /// <summary>
        /// 이 게임 오브젝트가 파괴될 때 호출됩니다. (풀링 시스템에서는 잘 호출되지 않을 수 있음)
        /// 만약을 위해 자동 비활성화 트윈을 중지합니다.
        /// </summary>
        private void OnDestroy()
        {
            autoDisableTweenCase.KillActive();
        }

        /// <summary>
        /// 적에게 명중했을 때 호출되는 추상 메서드입니다.
        /// 각 구체적인 총알 타입(예: 테슬라, 라바)은 이 메서드를 오버라이드하여
        /// 연쇄 공격, 범위 피해 등 고유한 2차 효과를 구현하고, 필요시 해당 효과에 대한 플로팅 텍스트도 생성해야 합니다.
        /// </summary>
        /// <param name="enemyHit">명중한 적의 BaseEnemyBehavior</param>
        protected abstract void OnEnemyHitted(BaseEnemyBehavior enemyHit);

        /// <summary>
        /// 장애물에 명중했을 때 호출되는 가상 메서드입니다.
        /// 기본적으로 자동 비활성화 트윈을 중지하고 투사체를 비활성화합니다.
        /// </summary>
        protected virtual void OnObstacleHitted()
        {
            autoDisableTweenCase.KillActive();
            if (gameObject.activeSelf) // 비활성화 전 항상 현재 활성 상태인지 확인
            {
                gameObject.SetActive(false);
            }
        }
    }
}