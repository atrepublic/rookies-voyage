// ==============================================
// 📌 GrenaderEnemyBehavior.cs
// ✅ 수류탄 던지는 적 유닛의 공격 행동 정의
// ✅ 애니메이션 이벤트 기반으로 수류탄 투척 실행
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class GrenaderEnemyBehavior : BaseEnemyBehavior
    {
        [Header("Grenade")]
        [Tooltip("일반 적용 수류탄 프리팹")]
        [SerializeField] private GameObject grenadePrefab;

        [Tooltip("엘리트 적용 수류탄 프리팹")]
        [SerializeField] private GameObject eliteGrenadePrefab;

        [Space]
        [Tooltip("수류탄 시작 위치")]
        [SerializeField] private Transform grenadeStartPosition;

        [Space]
        [Tooltip("수류탄 시각화용 3D 오브젝트")]
        [SerializeField] private GameObject grenadeObject;

        /// <summary>
        /// 📌 초기 설정: 이동 가능 활성화
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            CanMove = true;
        }

        /// <summary>
        /// 📌 체력 UI 위치 갱신
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        /// <summary>
        /// 📌 공격 시작 트리거 (애니메이션에 의해 조절됨)
        /// </summary>
        public override void Attack()
        {
            animatorRef.SetBool("Is Shooting", true);
        }

        /// <summary>
        /// 📌 애니메이션 이벤트 기반 공격 처리
        /// </summary>
        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    var grenade = Instantiate(
                        Tier == EnemyTier.Elite ? eliteGrenadePrefab : grenadePrefab
                    ).GetComponent<GrenadeBehavior>();

                    grenade.Throw(grenadeStartPosition.position, TargetPosition, GetCurrentDamage());

                    grenadeObject.SetActive(false);
                    break;

                case EnemyCallbackType.HitFinish:
                    animatorRef.SetBool("Is Shooting", false);
                    InvokeOnAttackFinished();
                    grenadeObject.SetActive(true);
                    break;
            }
        }
    }
}
