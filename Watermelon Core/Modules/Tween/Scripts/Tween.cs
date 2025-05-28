/*
 * Tween.cs
 * 이 스크립트는 트위닝(Tween) 기능을 제공하는 싱글톤 매니저 클래스입니다.
 * Update, FixedUpdate, LateUpdate 타이밍별로 등록된 트윈을 관리하고,
 * 다양한 커스텀 트윈 메서드를 통해 값 보간 및 지연 호출 등을 지원합니다.
 */
using UnityEngine;
using System.Collections;

namespace Watermelon
{
    [StaticUnload]
    public class Tween : MonoBehaviour
    {
        [Tooltip("Tween 시스템의 싱글톤 인스턴스")]        
        private static Tween instance;

        [Tooltip("Update 타이밍에 실행되는 트윈의 인덱스")]        
        private readonly int TWEEN_UPDATE = (int)UpdateMethod.Update;

        [Tooltip("FixedUpdate 타이밍에 실행되는 트윈의 인덱스")]        
        private readonly int TWEEN_FIXED_UPDATE = (int)UpdateMethod.FixedUpdate;

        [Tooltip("LateUpdate 타이밍에 실행되는 트윈의 인덱스")]        
        private readonly int TWEEN_LATE_UPDATE = (int)UpdateMethod.FixedUpdate;

        [Tooltip("타이밍별 트윈들을 관리하는 TweensHolder 배열")]        
        private static TweensHolder[] tweens;

        /// <summary>외부에서 접근 가능한 TweensHolder 배열</summary>
        public static TweensHolder[] Tweens => tweens;

        [Tooltip("현재 활성화된 TweenCase 콜렉션")]        
        private static TweenCaseCollection activeTweenCaseCollection;

        [Tooltip("TweenCase 콜렉션 활성화 여부")]        
        private static bool isActiveTweenCaseCollectionEnabled;

        /// <summary>
        /// Tween 시스템 초기화 함수.
        /// 지정한 카운트만큼 각 UpdateMethod에 대한 TweensHolder를 생성하고 초기 설정을 합니다.
        /// </summary>
        public void Init(int tweensUpdateCount, int tweensFixedUpdateCount, int tweensLateUpdateCount, bool systemLogs)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            tweens = new TweensHolder[]
            {
                new TweensHolder(UpdateMethod.Update, tweensUpdateCount),
                new TweensHolder(UpdateMethod.FixedUpdate, tweensFixedUpdateCount),
                new TweensHolder(UpdateMethod.LateUpdate, tweensLateUpdateCount)
            };
        }

        /// <summary>
        /// 새로운 트윈을 추가합니다.
        /// </summary>
        public static void AddTween(TweenCase tween)
        {
            tweens[tween.UpdateMethodIndex].AddTween(tween);

            if (isActiveTweenCaseCollectionEnabled)
                activeTweenCaseCollection.AddTween(tween);
        }

        /// <summary>
        /// 특정 UpdateMethod의 트윈을 일시 정지합니다.
        /// </summary>
        public static void Pause(UpdateMethod tweenType)
        {
            tweens[(int)tweenType].Pause();
        }

        /// <summary>
        /// 모든 트윈을 일시 정지합니다.
        /// </summary>
        public static void PauseAll()
        {
            foreach (TweensHolder tween in tweens)
            {
                tween.Pause();
            }
        }

        /// <summary>
        /// 특정 UpdateMethod의 트윈을 재개합니다.
        /// </summary>
        public static void Resume(UpdateMethod tweenType)
        {
            tweens[(int)tweenType].Resume();
        }

        /// <summary>
        /// 모든 트윈을 재개합니다.
        /// </summary>
        public static void ResumeAll()
        {
            foreach (TweensHolder tween in tweens)
            {
                tween.Resume();
            }
        }

        /// <summary>
        /// 특정 UpdateMethod의 모든 트윈을 제거합니다.
        /// </summary>
        public static void Remove(UpdateMethod tweenType)
        {
            tweens[(int)tweenType].Kill();
        }

        /// <summary>
        /// 모든 트윈을 제거합니다.
        /// </summary>
        public static void RemoveAll()
        {
            foreach (TweensHolder tween in tweens)
            {
                tween.Kill();
            }
        }

        /// <summary>
        /// Unity의 Update 이벤트에서 해당 타이밍 트윈을 업데이트합니다.
        /// </summary>
        private void Update()
        {
            tweens[TWEEN_UPDATE].Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Unity의 FixedUpdate 이벤트에서 해당 타이밍 트윈을 업데이트합니다.
        /// </summary>
        private void FixedUpdate()
        {
            tweens[TWEEN_FIXED_UPDATE].Update(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
        }

        /// <summary>
        /// Unity의 LateUpdate 이벤트에서 해당 타이밍 트윈을 업데이트합니다.
        /// </summary>
        private void LateUpdate()
        {
            tweens[TWEEN_LATE_UPDATE].Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// 트윈을 마킹하여 이후 제거 대상으로 설정합니다.
        /// </summary>
        public static void MarkForKilling(TweenCase tween)
        {
            tweens[tween.UpdateMethodIndex].MarkForKilling(tween);
        }

        /// <summary>
        /// 새로운 TweenCaseCollection을 시작하고 활성화합니다.
        /// </summary>
        public static TweenCaseCollection BeginTweenCaseCollection()
        {
            isActiveTweenCaseCollectionEnabled = true;
            activeTweenCaseCollection = new TweenCaseCollection();
            return activeTweenCaseCollection;
        }

        /// <summary>
        /// 현재 활성화된 TweenCaseCollection을 종료합니다.
        /// </summary>
        public static void EndTweenCaseCollection()
        {
            isActiveTweenCaseCollectionEnabled = false;
            activeTweenCaseCollection = null;
        }

        #region Custom Tweens
        /// <summary>
        /// 지정된 시간(delay) 후에 콜백(callback)을 실행합니다.
        /// </summary>
        /// <param name="callback">호출할 콜백 메서드.</param>
        /// <param name="delay">지연 시간(초).</param>
        public static TweenCase DelayedCall(float delay, SimpleCallback callback, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            if (delay <= 0)
            {
                callback?.Invoke();
                return null;
            }
            else
            {
                return new SystemTweenCases.Default()
                    .SetDuration(delay)
                    .SetUnscaledMode(unscaledTime)
                    .OnComplete(callback)
                    .SetUpdateMethod(tweenType)
                    .StartTween();
            }
        }

        /// <summary>
        /// 색상 값을 보간합니다.
        /// </summary>
        /// <param name="startValue">시작 색상.</param>
        /// <param name="resultValue">목표 색상.</param>
        /// <param name="time">지속 시간(초).</param>
        /// <param name="callback">보간 결과를 처리할 콜백.</param>
        public static TweenCase DoColor(Color startValue, Color resultValue, float time, SystemTweenCases.ColorCase.TweenColorCallback callback, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new SystemTweenCases.ColorCase(startValue, resultValue, callback)
                .SetDuration(time)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(tweenType)
                .StartTween();
        }

        /// <summary>
        /// 부동 소수점 값을 보간합니다.
        /// </summary>
        /// <param name="startValue">시작 값.</param>
        /// <param name="resultValue">목표 값.</param>
        /// <param name="time">지속 시간(초).</param>
        /// <param name="delay">시작 전 지연 시간(초).</param>
        /// <param name="unscaledTime">비시간 스케일 모드 여부.</param>
        /// <param name="tweenType">UpdateMethod 타입.</param>
        public static TweenCase DoFloat(float startValue, float resultValue, float time, SystemTweenCases.Float.TweenFloatCallback callback, float delay = 0.0f, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new SystemTweenCases.Float(startValue, resultValue, callback)
                .SetDelay(delay)
                .SetDuration(time)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(tweenType)
                .StartTween();
        }

        /// <summary>
        /// 지정된 조건을 만족할 때까지 대기합니다.
        /// </summary>
        /// <param name="callback">조건 검사 콜백.</param>
        public static TweenCase DoWaitForCondition(SystemTweenCases.Condition.TweenConditionCallback callback, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new SystemTweenCases.Condition(callback)
                .SetDuration(float.MaxValue)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(tweenType)
                .StartTween();
        }

        /// <summary>
        /// 다음 프레임에 콜백을 실행합니다.
        /// </summary>
        /// <param name="callback">실행할 콜백.</param>
        /// <param name="framesOffset">지연 프레임 수.</param>
        public static TweenCase NextFrame(SimpleCallback callback, int framesOffset = 1, bool unscaledTime = true, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new SystemTweenCases.NextFrame(callback, framesOffset)
                .SetDuration(float.MaxValue)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod)
                .StartTween();
        }

        /// <summary>
        /// MonoBehaviour가 아닌 스크립트에서 코루틴을 시작합니다.
        /// </summary>
        public static Coroutine InvokeCoroutine(IEnumerator enumerator)
        {
            if (instance == null) return null;
            return instance.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 커스텀 코루틴을 중지합니다.
        /// </summary>
        public static void StopCustomCoroutine(Coroutine coroutine)
        {
            if (instance == null) return;
            instance.StopCoroutine(coroutine);
        }
        #endregion

        /// <summary>
        /// 모든 코루틴을 중지하고 등록된 트윈을 언로드하여 메모리를 해제합니다.
        /// </summary>
        public static void DestroyObject()
        {
            if (instance != null)
                instance.StopAllCoroutines();

            foreach (TweensHolder tween in tweens)
            {
                tween.Unload();
            }
        }

        /// <summary>
        /// 정적 변수와 인스턴스를 정리합니다.
        /// </summary>
        private static void UnloadStatic()
        {
            instance = null;

            if (!tweens.IsNullOrEmpty())
            {
                foreach (TweensHolder tween in tweens)
                {
                    tween.Unload();
                }
            }

            activeTweenCaseCollection = null;
            isActiveTweenCaseCollectionEnabled = false;
        }
    }

    /// <summary>
    /// 트윈 업데이트 타이밍을 정의하는 열거형입니다.
    /// </summary>
    public enum UpdateMethod
    {
        Update = 0,
        FixedUpdate = 1,
        LateUpdate = 2
    }
}
