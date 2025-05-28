// ==============================================
// 📌 EnemyAnimationCallback.cs
// ✅ 적 애니메이션에 연결된 Animation Event를 처리하는 클래스
// ✅ 애니메이션에서 발생한 콜백을 BaseEnemyBehavior로 전달
// ✅ Ragdoll 조정 기능(크기 조절 및 초기화)도 에디터 기능으로 포함
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 적 애니메이션 이벤트를 처리하는 클래스
    /// </summary>
    public class EnemyAnimationCallback : MonoBehaviour
    {
        [Tooltip("연결된 적 행동 제어 클래스")]
        private BaseEnemyBehavior baseEnemyBehavior;

        [Tooltip("에디터에서 사용할 래그돌 크기 배수")]
        [SerializeField] float ragdollSizeMultiplier = 1;

        /// <summary>
        /// 📌 적 애니메이션 콜백 핸들러 초기화
        /// </summary>
        public void Init(BaseEnemyBehavior baseEnemyBehavior)
        {
            this.baseEnemyBehavior = baseEnemyBehavior;
        }

        /// <summary>
        /// 📌 콜백 타입에 따라 적 동작 실행
        /// </summary>
        public void OnCallbackInvoked(EnemyCallbackType enemyCallbackType)
        {
            baseEnemyBehavior.OnAnimatorCallback(enemyCallbackType);
        }

        // 📌 각 애니메이션 콜백 함수들
        public void OnHitCallback() => OnCallbackInvoked(EnemyCallbackType.Hit);
        public void OnLeftHitCallback() => OnCallbackInvoked(EnemyCallbackType.LeftHit);
        public void OnRightHitCallback() => OnCallbackInvoked(EnemyCallbackType.RightHit);
        public void OnHitFinishCallback() => OnCallbackInvoked(EnemyCallbackType.HitFinish);
        public void OnBossLeftStepCallback() => OnCallbackInvoked(EnemyCallbackType.BossLeftStep);
        public void OnBossRightStepCallback() => OnCallbackInvoked(EnemyCallbackType.BossRightStep);
        public void OnBossDeathFallCallback() => OnCallbackInvoked(EnemyCallbackType.BossDeathFall);
        public void OnBossEnterFallCallback() => OnCallbackInvoked(EnemyCallbackType.BossEnterFall);
        public void OnBossKickCallback() => OnCallbackInvoked(EnemyCallbackType.BossKick);
        public void OnBossEnterFallFinishedCallback() => OnCallbackInvoked(EnemyCallbackType.BossEnterFallFinished);
        public void OnReloadFinishedCallback() => OnCallbackInvoked(EnemyCallbackType.ReloadFinished);

#if UNITY_EDITOR
        [Button("Multiply Ragdoll Size")]
        private void MultiplyRagdollWidth()
        {
            if (Application.isPlaying) return;
            MultiplyRagdollWidthRecursively(transform);
        }

        /// <summary>
        /// 📌 Ragdoll에 있는 Collider의 크기를 조정
        /// </summary>
        private void MultiplyRagdollWidthRecursively(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                {
                    if (child.TryGetComponent(out SphereCollider sphere)) sphere.radius *= ragdollSizeMultiplier;
                    if (child.TryGetComponent(out CapsuleCollider capsule)) capsule.radius *= ragdollSizeMultiplier;
                    if (child.TryGetComponent(out BoxCollider box))
                    {
                        box.size = box.size.MultX(ragdollSizeMultiplier).MultZ(ragdollSizeMultiplier);
                    }
                }

                MultiplyRagdollWidthRecursively(child);
            }
        }

        [Button("Clear Ragdoll")]
        private void ClearRagdoll()
        {
            if (Application.isPlaying) return;
            ClearRagdollRecursively(transform);
        }

        /// <summary>
        /// 📌 Ragdoll에 붙은 Rigidbody, Joint, Collider 제거
        /// </summary>
        private void ClearRagdollRecursively(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                {
                    DestroyImmediate(child.GetComponent<CharacterJoint>());
                    DestroyImmediate(child.GetComponent<Rigidbody>());

                    var collider = child.GetComponent<Collider>();
                    while (collider != null)
                    {
                        DestroyImmediate(collider);
                        collider = child.GetComponent<Collider>();
                    }
                }

                ClearRagdollRecursively(child);
            }
        }
#endif
    }

    /// <summary>
    /// 적 애니메이션 이벤트의 콜백 타입 정의
    /// </summary>
    public enum EnemyCallbackType
    {
        Hit = 0,
        HitFinish = 1,
        BossLeftStep = 2,
        BossRightStep = 3,
        BossDeathFall = 4,
        BossEnterFall = 5,
        BossKick = 6,
        BossEnterFallFinished = 7,
        ReloadFinished = 8,
        LeftHit = 9,
        RightHit = 10,
    }
}
