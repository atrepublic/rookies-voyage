// ============================================================
// ParticleCase.cs
// ------------------------------------------------------------
// 🔹 스크립트 용도
//   파티클 풀링 시스템에서 **개별 파티클 인스턴스**의 라이프사이클을
//   관리합니다.
//   - 재생/정지
//   - 위치·회전·스케일 설정
//   - 강제 종료 및 자동 종료 타이머
//   - ParticleBehaviour(부가 동작) 트리거
//   메서드 체이닝을 지원해 간결한 플레이 코드 작성을 돕습니다.
// ============================================================

using System;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// ▶ **ParticleCase**
    /// 
    /// 풀(pool)에서 꺼낸 <see cref="ParticleSystem"/> 오브젝트를 담당하여
    /// 활성화·비활성화 시퀀스와 Transform 제어, 강제 정지 로직을 관리한다.
    /// </summary>
    public sealed class ParticleCase
    {
        #region 🔑 필드 --------------------------------------------------

        [Tooltip("파티클이 비활성화될 예정 시각 (-1이면 예약 없음)")]
        private float disableTime = -1f;

        [Tooltip("시각 효과를 담당하는 ParticleSystem 컴포넌트")]
        public readonly ParticleSystem ParticleSystem;

        [Tooltip("파티클에 부착된 추가 동작 스크립트 (옵션)")]
        public readonly ParticleBehaviour SpecialBehavior;

        [Tooltip("비활성화 시 부모 트랜스폼을 Detach 할지 여부")]
        public readonly bool ResetParent;

        /// <summary>
        /// 파티클이 비활성화될 때 호출되는 콜백.
        /// </summary>
        public event SimpleCallback Disabled;

        /// <summary>
        /// 강제 정지 상태 여부.
        /// </summary>
        public bool IsForceStopped { get; private set; }

        #endregion

        #region 🏗️ 초기화 ---------------------------------------------

        /// <summary>
        /// 파티클 케이스 생성자 (풀에서 꺼내는 시점에 호출).
        /// </summary>
        /// <param name="particleSystem">관리할 ParticleSystem</param>
        /// <param name="isDelayed">딜레이 재생 여부</param>
        /// <param name="resetParent">비활성화 시 부모 해제 여부</param>
        public ParticleCase(ParticleSystem particleSystem, bool isDelayed, bool resetParent)
        {
            if (particleSystem == null)
            {
                Debug.LogError("[ParticleCase] ParticleSystem이 null입니다.");
                return;
            }

            ParticleSystem = particleSystem;
            ResetParent    = resetParent;
            IsForceStopped = false;

            // ✦ 즉시 재생 또는 딜레이 모드 설정
            if (isDelayed)
                ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            else
                ParticleSystem.Play();

            // ✦ 부가 행동 스크립트 캐싱 및 활성화 알림
            SpecialBehavior = ParticleSystem.GetComponent<ParticleBehaviour>();
            SpecialBehavior?.OnParticleActivated();
        }

        #endregion

        #region 🔄 상태 갱신 및 종료 ------------------------------------

        /// <summary>
        /// 파티클을 비활성화할 때 호출 (풀 관리자가 Update 루프에서 호출).
        /// </summary>
        public void OnDisable()
        {
            if (ParticleSystem != null)
            {
                if (ResetParent)
                    ParticleSystem.transform.SetParent(null);

                ParticleSystem.Stop();
                ParticleSystem.gameObject.SetActive(false);
            }

            SpecialBehavior?.OnParticleDisabled();
            Disabled?.Invoke();
        }

        /// <summary>
        /// 파티클을 즉시 정지하고 <see cref="Disabled"/> 이벤트를 실행.
        /// </summary>
        /// <param name="stopBehavior">정지 방식</param>
        public void ForceDisable(ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
        {
            IsForceStopped = true;

            if (ParticleSystem != null)
            {
                if (ResetParent)
                    ParticleSystem.transform.SetParent(null);

                ParticleSystem.Stop(true, stopBehavior);
            }

            Disabled?.Invoke();
        }

        /// <summary>
        /// 파티클을 강제 종료해야 하는지 검사.
        /// (IsForceStopped <b>또는</b> 지정된 지속 시간 초과)
        /// </summary>
        public bool IsForceDisabledRequired()
        {
            if (IsForceStopped)
                return true;

            if (disableTime != -1f && Time.time > disableTime)
                return true;

            return false;
        }

        #endregion

        #region 🔧 체이닝 유틸 -----------------------------------------

        /// <summary>
        /// Disabled 시 호출될 콜백을 지정.
        /// </summary>
        public ParticleCase SetOnDisabled(SimpleCallback onDisabled)
        {
            Disabled = onDisabled;
            return this;
        }

        /// <summary>
        /// 월드 위치를 설정.
        /// </summary>
        public ParticleCase SetPosition(Vector3 position)
        {
            ParticleSystem.transform.position = position;
            return this;
        }

        /// <summary>
        /// 로컬 스케일을 설정.
        /// </summary>
        public ParticleCase SetScale(Vector3 scale)
        {
            ParticleSystem.transform.localScale = scale;
            return this;
        }

        /// <summary>
        /// 로컬 회전을 설정.
        /// </summary>
        public ParticleCase SetRotation(Quaternion rotation)
        {
            ParticleSystem.transform.localRotation = rotation;
            return this;
        }

        /// <summary>
        /// 지정한 시간(초) 후 자동 비활성화 타이머 설정.
        /// </summary>
        public ParticleCase SetDuration(float duration)
        {
            disableTime = Time.time + duration;
            return this;
        }

        /// <summary>
        /// 특정 트랜스폼을 따라가도록 설정 (부모 지정 + 로컬 위치 고정).
        /// </summary>
        public ParticleCase SetTarget(Transform followTarget, Vector3 localPosition)
        {
            ParticleSystem.transform.SetParent(followTarget);
            ParticleSystem.transform.localPosition = localPosition;
            return this;
        }

        /// <summary>
        /// 메인 및 자식 파티클 시스템에 커스텀 액션을 적용.
        /// </summary>
        public void ApplyToParticles(Action<ParticleSystem> action)
        {
            if (action == null) return;

            action.Invoke(ParticleSystem);

            foreach (ParticleSystem ps in ParticleSystem.GetComponentsInChildren<ParticleSystem>())
            {
                if (ps != ParticleSystem)
                    action.Invoke(ps);
            }
        }

        #endregion
    }
}
