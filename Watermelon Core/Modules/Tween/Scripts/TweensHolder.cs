/*
 * TweensHolder.cs
 * ------------------------------------------------------------
 *  이 클래스는 특정 UpdateMethod(Update, FixedUpdate, LateUpdate)에 속하는
 *  TweenCase 인스턴스들을 배열로 보관‧관리하는 매니저 역할을 합니다.
 *  • 트윈 추가·제거 및 배열 자동 확장
 *  • 비활성 슬롯을 압축하는 재정렬(Reorganize) 로직
 *  • 일괄 Pause / Resume / Kill
 *  • Time.timeScale 무시 여부 및 딜레이·보간 처리
 *  원본 기능은 그대로 유지하며, 가독성을 위해 한글 툴팁과 설명 주석을 추가했습니다.
 * ------------------------------------------------------------
 */
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Watermelon
{
    public class TweensHolder
    {
        #region Fields

        [Tooltip("실행 중인 TweenCase들을 보관하는 배열")]
        protected TweenCase[] tweens;
        public TweenCase[] Tweens => tweens;

        [Tooltip("배열에 실제로 등록된 TweenCase 개수")]
        protected int tweensCount;

        [Tooltip("현재 활성 트윈이 하나라도 있는지 여부")]
        protected bool hasActiveTweens = false;

        [Tooltip("배열에 null 슬롯이 생겨 재정렬이 필요한지 여부")]
        protected bool requiresActiveReorganization = false;

        [Tooltip("재정렬을 시작할 인덱스 (최초 null 슬롯)")]
        protected int reorganizeFromID = -1;

        [Tooltip("가장 큰 활성 인덱스 (압축 시 갱신)")]
        protected int maxActiveLookupID = -1;

        [Tooltip("Update 루프가 끝난 후 Kill 처리할 트윈 목록")]
        protected List<TweenCase> killingTweens = new List<TweenCase>();

#if UNITY_EDITOR
        [Tooltip("에디터에서 디버그용으로 기록하는 최대 동시 트윈 수")]
        protected int maxTweensAmount = 0;
#endif

        [Tooltip("이 TweensHolder가 담당하는 UpdateMethod 종류")]
        protected UpdateMethod updateMethod;

        #endregion

        #region Constructor

        /// <summary>
        /// 지정된 UpdateMethod용 TweensHolder를 생성합니다.
        /// </summary>
        /// <param name="updateMethod">Update, FixedUpdate, LateUpdate 중 하나</param>
        /// <param name="defaultAmount">초기 배열 크기</param>
        public TweensHolder(UpdateMethod updateMethod, int defaultAmount)
        {
            this.updateMethod = updateMethod;
            tweens = new TweenCase[defaultAmount];
        }

        #endregion

        #region Public API

        /// <summary>
        /// 새 TweenCase를 등록합니다. 배열이 가득 차면 자동으로 50개 확장합니다.
        /// </summary>
        public void AddTween(TweenCase tween)
        {
            if (tweensCount >= tweens.Length)
            {
                Array.Resize(ref tweens, tweens.Length + 50);
                Debug.LogWarning("[Tween]: 트윈(Update) 배열 크기를 자동 확장했습니다. 현재 크기 - " + tweens.Length + " (퍼포먼스 누수를 방지하려면 기본 크기를 늘려주세요)");
            }

            if (requiresActiveReorganization)
                ReorganizeUpdateActiveTweens();

            tween.IsActive = true;
            tween.ActiveID = (maxActiveLookupID = tweensCount);

            tweens[tweensCount] = tween;
            tweensCount++;
            hasActiveTweens = true;

#if UNITY_EDITOR
            if (maxTweensAmount < tweensCount)
                maxTweensAmount = tweensCount;
#endif
        }

        /// <summary>
        /// 모든 TweenCase를 일시정지합니다.
        /// </summary>
        public void Pause()
        {
            for (int i = 0; i < tweensCount; i++)
            {
                TweenCase tween = tweens[i];
                if (tween != null)
                    tween.Pause();
            }
        }

        /// <summary>
        /// 일시정지된 모든 TweenCase를 재개합니다.
        /// </summary>
        public void Resume()
        {
            for (int i = 0; i < tweensCount; i++)
            {
                TweenCase tween = tweens[i];
                if (tween != null)
                    tween.Resume();
            }
        }

        /// <summary>
        /// 모든 TweenCase를 즉시 Kill() 처리합니다.
        /// </summary>
        public void Kill()
        {
            for (int i = 0; i < tweensCount; i++)
            {
                TweenCase tween = tweens[i];
                if (tween != null)
                    tween.Kill();
            }
        }

        /// <summary>
        /// Update 루프 중 Kill 대기열에 등록합니다.
        /// </summary>
        public void MarkForKilling(TweenCase tween)
        {
            killingTweens.Add(tween);
        }

        /// <summary>
        /// 배열과 상태 값을 초기화합니다. (씬 언로드 시 호출)
        /// </summary>
        public void Unload()
        {
            if (!tweens.IsNullOrEmpty())
            {
                for (int i = 0; i < tweens.Length; i++)
                    tweens[i] = null;
            }

            killingTweens.Clear();
            tweensCount = 0;
            hasActiveTweens = false;
            requiresActiveReorganization = false;
            reorganizeFromID = -1;
            maxActiveLookupID = -1;
        }

        #endregion

        #region Update Loop

        /// <summary>
        /// TweensHolder 전용 업데이트 루틴. Watermelon.Tween에서 호출됩니다.
        /// </summary>
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (!hasActiveTweens)
                return;

            if (requiresActiveReorganization)
                ReorganizeUpdateActiveTweens();

            for (int i = 0; i < tweensCount; i++)
            {
                TweenCase tween = tweens[i];
                if (tween != null)
                {
                    if (!tween.Validate())
                    {
                        tween.Kill();
                    }
                    else if (tween.IsActive && !tween.IsPaused)
                    {
                        // 딜레이 & 스케일 처리는 Unscaled 여부에 따라 분기
                        float dt = tween.IsUnscaled ? unscaledDeltaTime : deltaTime;

                        if (tween.Delay > 0 && tween.Delay > tween.CurrentDelay)
                        {
                            tween.UpdateDelay(dt);
                        }
                        else
                        {
                            tween.UpdateState(dt);
                            tween.Invoke(dt);
                        }

                        if (tween.IsCompleted)
                        {
                            tween.DefaultComplete();
                            tween.InvokeCompleteEvent();
                            tween.Kill();
                        }
                    }
                }
            }

            // 한 프레임 동안 누적된 Kill 대상을 실제로 배열에서 제거
            for (int i = killingTweens.Count - 1; i > -1; i--)
            {
                RemoveActiveTween(killingTweens[i]);
            }
            killingTweens.Clear();
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        /// 비어 있는 슬롯을 앞으로 당겨 배열을 압축합니다.
        /// </summary>
        private void ReorganizeUpdateActiveTweens()
        {
            if (tweensCount <= 0)
            {
                maxActiveLookupID = -1;
                reorganizeFromID = -1;
                requiresActiveReorganization = false;
                return;
            }

            if (reorganizeFromID == maxActiveLookupID)
            {
                maxActiveLookupID--;
                reorganizeFromID = -1;
                requiresActiveReorganization = false;
                return;
            }

            int defaultOffset = 1;
            int tweensTempCount = maxActiveLookupID + 1;
            maxActiveLookupID = reorganizeFromID - 1;

            for (int i = reorganizeFromID + 1; i < tweensTempCount; i++)
            {
                TweenCase tween = tweens[i];
                if (tween != null)
                {
                    tween.ActiveID = (maxActiveLookupID = i - defaultOffset);
                    tweens[i - defaultOffset] = tween;
                    tweens[i] = null;
                }
                else
                {
                    defaultOffset++;
                }
            }

            requiresActiveReorganization = false;
            reorganizeFromID = -1;
        }

        /// <summary>
        /// 배열에서 트윈을 제거하고 재정렬 플래그를 설정합니다.
        /// </summary>
        private void RemoveActiveTween(TweenCase tween)
        {
            int activeId = tween.ActiveID;
            tween.ActiveID = -1;

            requiresActiveReorganization = true;
            if (reorganizeFromID == -1 || reorganizeFromID > activeId)
                reorganizeFromID = activeId;

            tweens[activeId] = null;
            tweensCount--;
            hasActiveTweens = (tweensCount > 0);
        }

        #endregion
    }
}
