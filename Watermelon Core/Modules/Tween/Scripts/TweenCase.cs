/*
 * TweenCase.cs
 * 이 추상 클래스는 Tween 트윈 데이터의 기본 속성과 동작을 관리합니다.
 * 딜레이, 진행 상태(state), 지속 시간(duration), 이징(easing) 설정,
 * 일시정지/재개, 완료/강제 종료 처리, 콜백 등록 등을 지원합니다.
 */
using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    public abstract class TweenCase
    {
        [Tooltip("트윈의 고유 활성 ID")]        
        public int ActiveID;

        [Tooltip("트윈이 현재 활성 상태인지 여부")]        
        public bool IsActive;

        [Tooltip("딜레이 시간 경과 후 누적된 시간")]        
        protected float currentDelay;
        public float CurrentDelay => currentDelay;

        [Tooltip("트윈 시작 전에 지연될 시간")]        
        protected float delay;
        public float Delay => delay;

        [Tooltip("트윈의 현재 진행 비율(0~1)")]        
        protected float state;
        public float State => state;

        [Tooltip("업데이트 메서드 인덱스 (UpdateMethod 열거형)")]
        protected int updateMethodIndex;
        public int UpdateMethodIndex => updateMethodIndex;
        public UpdateMethod UpdateMethod => (UpdateMethod)updateMethodIndex;

        [Tooltip("트윈이 완료되는데 걸리는 총 시간")]        
        protected float duration;
        public float Duration => duration;

        [Tooltip("시간 배율(Time.timeScale)을 무시할지 여부")]        
        protected bool isUnscaled;
        public bool IsUnscaled => isUnscaled;

        [Tooltip("트윈이 일시정지된 상태인지 여부")]        
        protected bool isPaused;
        public bool IsPaused => isPaused;

        [Tooltip("트윈이 완료된 상태인지 여부")]        
        protected bool isCompleted;
        public bool IsCompleted => isCompleted;

        [Tooltip("트윈이 강제 종료(제거) 처리되었는지 여부")]        
        protected bool isKilling;
        public bool IsKilling => isKilling;

        [Tooltip("적용할 이징 함수 객체")]        
        protected Ease.IEasingFunction easeFunction;

        [Tooltip("완료 시 호출할 콜백 이벤트")]        
        protected event SimpleCallback tweenCompleted;

        [Tooltip("특정 시점 도달 시 실행할 콜백 목록")]        
        private List<CallbackData> callbackData;

        [Tooltip("필요한 경우 참조할 부모 GameObject")]        
        protected GameObject parentObject;
        public GameObject ParentObject => parentObject;

        // 생성자: 기본 이징을 선형(Linear)으로 설정
        public TweenCase()
        {
            SetEasing(Ease.Type.Linear);
        }

        /// <summary>
        /// 트윈을 시작하고 Tween 매니저에 등록합니다.
        /// </summary>
        public virtual TweenCase StartTween()
        {
            Tween.AddTween(this);
            return this;
        }

        /// <summary>
        /// 트윈 설정이 유효한지 검증합니다.
        /// </summary>
        public abstract bool Validate();

        /// <summary>
        /// 트윈을 중지하고 제거 대상으로 표시합니다.
        /// </summary>
        public TweenCase Kill()
        {
            if (!isKilling)
            {
                IsActive = false;
                Tween.MarkForKilling(this);
                isKilling = true;
            }
            return this;
        }

        /// <summary>
        /// 트윈을 즉시 완료 상태로 만듭니다.
        /// </summary>
        public TweenCase Complete()
        {
            if (isPaused) isPaused = false;
            state = 1;
            isCompleted = true;
            return this;
        }

        /// <summary>
        /// 현재 트윈을 일시정지합니다.
        /// </summary>
        public TweenCase Pause()
        {
            isPaused = true;
            return this;
        }

        /// <summary>
        /// 일시정지된 트윈을 재개합니다.
        /// </summary>
        public TweenCase Resume()
        {
            isPaused = false;
            return this;
        }

        /// <summary>
        /// 진행 상태(state)를 초기화합니다.(0으로 설정)
        /// </summary>
        public void Reset()
        {
            state = 0;
        }

        /// <summary>
        /// AnimationCurve 기반 커스텀 이징 함수 설정
        /// </summary>
        public TweenCase SetCurveEasing(AnimationCurve easingCurve)
        {
            easeFunction = new AnimationCurveEasingFunction(easingCurve);
            return this;
        }

        /// <summary>
        /// 사용자 지정 이징 함수 설정
        /// </summary>
        public TweenCase SetCustomEasing(Ease.IEasingFunction easeFunction)
        {
            this.easeFunction = easeFunction;
            return this;
        }

        /// <summary>
        /// 현재 이징 함수로 보간값을 계산하여 반환합니다. (0~1)
        /// </summary>
        public float Interpolate(float p)
        {
            return easeFunction.Interpolate(p);
        }

        #region 설정 메서드

        /// <summary>
        /// 트윈 시작 전 지연 시간을 설정합니다.
        /// </summary>
        public TweenCase SetDelay(float delay)
        {
            this.delay = delay;
            currentDelay = 0;
            return this;
        }

        /// <summary>
        /// 트윈을 어느 시점에 업데이트할지 설정합니다. (Update, FixedUpdate 등)
        /// </summary>
        public TweenCase SetUpdateMethod(UpdateMethod updateMethod)
        {
            this.updateMethodIndex = (int)updateMethod;
            return this;
        }

        /// <summary>
        /// 언스케일 모드 설정 (Time.timeScale 무시 여부)
        /// </summary>
        public TweenCase SetUnscaledMode(bool isUnscaled)
        {
            this.isUnscaled = isUnscaled;
            return this;
        }

        /// <summary>
        /// 기본 이징 타입을 설정합니다. (Ease.Type 열거형 사용)
        /// </summary>
        public TweenCase SetEasing(Ease.Type ease)
        {
            easeFunction = Ease.GetFunction(ease);
            return this;
        }

        /// <summary>
        /// 트윈의 지속 시간을 설정합니다.
        /// </summary>
        public TweenCase SetDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        #endregion

        /// <summary>
        /// 시스템 업데이트: 상태(state) 값을 갱신하고 완료/콜백을 처리합니다.
        /// </summary>
        public void UpdateState(float deltaTime)
        {
            state += Mathf.Min(1.0f, deltaTime / duration);
            if (state >= 1)
            {
                isCompleted = true;
            }
            else if (!callbackData.IsNullOrEmpty())
            {
                for (int i = 0; i < callbackData.Count; i++)
                {
                    var data = callbackData[i];
                    if (state >= data.t)
                    {
                        data.callback?.Invoke();
                        callbackData.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 시스템 업데이트: 딜레이 경과 시간을 갱신합니다.
        /// </summary>
        public void UpdateDelay(float deltaTime)
        {
            currentDelay += deltaTime;
        }

        /// <summary>
        /// 완료 시 호출할 콜백을 등록합니다.
        /// </summary>
        public TweenCase OnComplete(SimpleCallback callback)
        {
            tweenCompleted += callback;
            return this;
        }

        /// <summary>
        /// 특정 시점 t(0~1)에 도달하면 호출할 콜백을 등록합니다.
        /// </summary>
        public TweenCase OnTimeReached(float t, SimpleCallback callback)
        {
            if (callbackData == null) callbackData = new List<CallbackData>();
            callbackData.Add(new CallbackData(t, callback));
            return this;
        }

        /// <summary>
        /// 내부적으로 완료 이벤트를 실행합니다.
        /// </summary>
        public void InvokeCompleteEvent()
        {
            tweenCompleted?.Invoke();
        }

        /// <summary>
        /// 시스템 호출: 실제 트윈 동작을 처리합니다.
        /// </summary>
        public abstract void Invoke(float deltaTime);

        /// <summary>
        /// 시스템 호출: 기본 완료 처리 로직을 수행합니다.
        /// </summary>
        public abstract void DefaultComplete();

        // 특정 시점 콜백 정보를 담는 내부 클래스
        private class CallbackData
        {
            public float t;
            public SimpleCallback callback;

            public CallbackData(float t, SimpleCallback callback)
            {
                this.t = t;
                this.callback = callback;
            }
        }
    }
}
