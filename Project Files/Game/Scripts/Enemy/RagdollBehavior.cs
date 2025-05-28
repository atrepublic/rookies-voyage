// ==============================================
// 📌 RagdollBehavior.cs
// ✅ 적 또는 캐릭터의 래그돌 물리 처리를 제어하는 클래스
// ✅ Rigidbody 상태 전환, 폭발 반응, 초기 위치 복원 등 기능 포함
// ✅ Rigidbody가 있는 하위 오브젝트를 자동 인식하여 관리
// ==============================================

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 래그돌 물리 처리를 제어하는 클래스
    /// </summary>
    public class RagdollBehavior
    {
        [Tooltip("래그돌용 리지드바디 정보 리스트")]
        private List<RigidbodyCase> rbCases;

        /// <summary>
        /// 📌 래그돌 오브젝트에서 Rigidbody들을 수집하고 비활성화
        /// </summary>
        public void Init(Transform ragdollParentTransform)
        {
            List<Rigidbody> rigidbodies = new List<Rigidbody>();
            ragdollParentTransform.GetComponentsInChildren(rigidbodies);

            rbCases = new List<RigidbodyCase>();

            for (int i = 0; i < rigidbodies.Count; i++)
            {
                var rigidbody = rigidbodies[i];

                if (rigidbody.gameObject.layer != 14) // Ragdoll 레이어만 처리
                    continue;

                var rbCase = new RigidbodyCase(rigidbody);
                rbCase.Disable();

                rbCases.Add(rbCase);
            }
        }

        /// <summary>
        /// 📌 래그돌 활성화 (중력 작동, 충돌 활성화)
        /// </summary>
        public void Activate()
        {
            for (int i = 0; i < rbCases.Count; i++)
            {
                rbCases[i].Activate();
            }
        }

        /// <summary>
        /// 📌 특정 지점에서 폭발력을 가하며 래그돌 활성화
        /// </summary>
        public void ActivateWithForce(Vector3 point, float force, float radius)
        {
            for (int i = 0; i < rbCases.Count; i++)
            {
                rbCases[i].Activate();
                rbCases[i].AddForce(point, force, radius);
            }
        }

        /// <summary>
        /// 📌 모든 리지드바디 비활성화 (중력/충돌 OFF)
        /// </summary>
        public void Disable()
        {
            if (rbCases.IsNullOrEmpty())
                return;

            for (int i = 0; i < rbCases.Count; i++)
            {
                if (rbCases[i] != null && rbCases[i].rigidbody != null)
                    rbCases[i].Disable();
            }
        }

        /// <summary>
        /// 📌 리지드바디들의 위치, 회전, 크기를 초기 상태로 복원
        /// </summary>
        public void Reset()
        {
            if (rbCases.IsNullOrEmpty())
                return;

            for (int i = 0; i < rbCases.Count; i++)
            {
                rbCases[i].Reset();
            }
        }

        /// <summary>
        /// 리지드바디와 콜라이더 상태를 저장하고 관리하는 구조체
        /// </summary>
        private class RigidbodyCase
        {
            public Rigidbody rigidbody;
            public Collider collider;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;

            public RigidbodyCase(Rigidbody rigidbody)
            {
                this.rigidbody = rigidbody;

                collider = rigidbody.GetComponent<Collider>();

                localPosition = rigidbody.transform.localPosition;
                localRotation = rigidbody.transform.localRotation;
                localScale = rigidbody.transform.localScale;
            }

            /// <summary>
            /// 📌 리지드바디 비활성화 (중력/충돌 OFF)
            /// </summary>
            public void Disable()
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.Sleep();

                if (collider != null)
                    collider.enabled = false;
            }

            /// <summary>
            /// 📌 리지드바디 활성화 (중력/충돌 ON)
            /// </summary>
            public void Activate()
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                rigidbody.WakeUp();

                if (collider != null)
                    collider.enabled = true;
            }

            /// <summary>
            /// 📌 폭발력 적용
            /// </summary>
            public void AddForce(Vector3 point, float force, float radius)
            {
                rigidbody.AddExplosionForce(force, point, radius);
            }

            /// <summary>
            /// 📌 초기 상태로 위치, 회전, 스케일 복원
            /// </summary>
            public void Reset()
            {
                rigidbody.transform.localPosition = localPosition;
                rigidbody.transform.localRotation = localRotation;
                rigidbody.transform.localScale = localScale;
            }
        }
    }
}
