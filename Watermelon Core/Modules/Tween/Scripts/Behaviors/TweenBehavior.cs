/*
 * TweenBehavior.cs
 * ------------------------------------------------------------
 * 특정 컴포넌트 C(예: Transform, CanvasGroup 등)와 값 T(예: Vector3, float 등)를
 * 대상으로 트윈 애니메이션을 제어하는 추상 베이스 클래스입니다.
 *  - 지속 시간·딜레이·이징·반복 루프 등을 인스펙터에서 설정
 *  - OnEnable 시 지정된 start/end 값을 계산해 TweenCase 실행
 *  - 루프 타입(Repeat, Yoyo, Increment)에 따라 자동 반복 처리
 *  - Enable/Disable 시 TweenCase 생명주기 관리(KillActive)
 *  기능은 그대로 두고, 가독성을 높이기 위해 한글 요약·툴팁·함수 설명
 *  주석을 추가했습니다.
 * ------------------------------------------------------------
 */
using UnityEngine;

namespace Watermelon
{
    public abstract class TweenBehavior<C, T> : MonoBehaviour where C : Component
    {
        //===========================
        // 인스펙터 노출 변수들
        //===========================

        [SerializeField, Tooltip("트윈이 지속될 시간(초)")]
        protected float duration;

        [SerializeField, Tooltip("트윈 시작 전 딜레이(초)")]
        protected float delay;

        [Header("Easing")]
        [SerializeField, Tooltip("적용할 기본 이징 타입")]
        protected Ease.Type easing;

        [Space]
        [SerializeField, Tooltip("커스텀 커브 사용 여부")]
        protected bool hasCustomCurve;

        [SerializeField, Tooltip("사용자 정의 커스텀 커브")]
        protected AnimationCurve customCurve;

        [Header("Loop")]
        [SerializeField, Tooltip("루프 반복 횟수 (1 = 한 번, 0 또는 음수 = 무제한)")]
        protected int loopAmount = 1;

        [SerializeField, Tooltip("반복 방식(Repeat / Yoyo / Increment)")]
        protected LoopType loopType;

        [Header("Target")]
        [Space]
        [SerializeField, Tooltip("목표값 설정 방식(From: 시작값 → 초기값, To: 초기값 → 목표값)")]
        protected TweenTargetType type;

        [SerializeField, Tooltip("트윈이 도달할 목표값")]
        protected T target;

        //===========================
        // 런타임 변수들
        //===========================

        private C targetComponent;
        public C TargetComponent
        {
            get
            {
                if (targetComponent != null) return targetComponent;

                targetComponent = GetComponent<C>();
                return targetComponent;
            }
        }

        protected int loopId;
        protected TweenCase tweenCase;

        protected T initialValue; // 컴포넌트 활성 시점의 원본 값
        protected T startValue;   // 루프 시작 값
        protected T endValue;     // 루프 목표 값

        /// <summary>
        /// 파생 클래스에서 트윈이 적용될 실제 프로퍼티를 정의합니다.
        /// </summary>
        protected abstract T TargetValue { get; set; }

        //====================================================================//
        // MonoBehaviour 이벤트
        //====================================================================//

        /// <summary>
        /// 초기화: 컴포넌트의 현재 값을 저장해둡니다.
        /// </summary>
        protected virtual void Awake()
        {
            initialValue = TargetValue;
        }

        /// <summary>
        /// Enable 시값 세팅 및 첫 루프 시작.
        /// </summary>
        protected virtual void OnEnable()
        {
            // from/to 유형에 따라 start/end 값 계산
            if (type == TweenTargetType.From)
            {
                startValue = target;
                endValue = initialValue;
            }
            else
            {
                endValue = target;
                startValue = initialValue;
            }

            loopId = 0;

            StartLoop(delay);
        }

        /// <summary>
        /// 트윈 루프를 시작하고 TweenCase 설정을 적용합니다.
        /// </summary>
        protected virtual void StartLoop(float delay)
        {
            loopId++;

            tweenCase.SetDelay(delay);

            if (hasCustomCurve)
            {
                tweenCase.SetCurveEasing(customCurve);
            }
            else
            {
                tweenCase.SetEasing(easing);
            }

            // 마지막 루프가 아닐 경우 완료 콜백 등록
            if (loopId != loopAmount)
            {
                tweenCase.OnComplete(OnComplete);
            }
        }

        /// <summary>
        /// 한 루프가 완료되었을 때 호출됩니다. 루프 타입에 따라 값을 교체하거나 증가.
        /// </summary>
        protected virtual void OnComplete()
        {
            switch (loopType)
            {
                case LoopType.Yoyo:
                    var clone = startValue;
                    startValue = endValue;
                    endValue = clone;
                    break;

                case LoopType.Increment:
                    IncrementLoopChangeValues();
                    break;
            }

            // 다음 루프 즉시 시작
            StartLoop(0);
        }

        /// <summary>
        /// LoopType.Increment일 때 호출되어 start/end 값을 증가시키는 로직을 파생 클래스에서 구현합니다.
        /// </summary>
        protected abstract void IncrementLoopChangeValues();

        /// <summary>
        /// Disable 시 진행 중이던 TweenCase를 안전하게 종료합니다.
        /// </summary>
        protected virtual void OnDisable()
        {
            tweenCase.KillActive();
        }

        //====================================================================//
        // 열거형 정의
        //====================================================================//

        public enum LoopType
        {
            Repeat = 0,   // 단순 반복
            Yoyo = 1,     // 끝 ↔ 시작을 교차 반복
            Increment = 2 // 반복마다 값이 누적 증가
        }

        public enum TweenTargetType
        {
            From = 0, // 지정 값 → 초기 값
            To = 1    // 초기 값 → 지정 값
        }
    }
}
