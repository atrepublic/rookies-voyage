/*
 * TweenCaseCollection.cs
 * 이 클래스는 여러 TweenCase를 그룹화하여 일괄 처리할 수 있는 컬렉션 기능을 제공합니다.
 * 트윈 케이스 추가, 완료 검사, 일괄 완료/강제 종료, 완료 콜백 등록 및 내부 처리 등을 지원합니다.
 */
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class TweenCaseCollection
    {
        [Tooltip("관리 중인 TweenCase 목록")]
        private List<TweenCase> tweenCases = new List<TweenCase>();
        public List<TweenCase> TweenCases => tweenCases;

        [Tooltip("모든 TweenCase 완료 시 호출될 콜백 이벤트")]
        private event SimpleCallback tweensCompleted;

        /// <summary>
        /// TweenCase를 컬렉션에 추가하고 완료 시 내부 콜백을 등록합니다.
        /// </summary>
        public void AddTween(TweenCase tweenCase)
        {
            tweenCase.OnComplete(OnTweenCaseComplete);
            tweenCases.Add(tweenCase);
        }

        /// <summary>
        /// 모든 TweenCase가 완료되었는지 확인합니다.
        /// </summary>
        public bool IsComplete()
        {
            foreach (var tweenCase in tweenCases)
            {
                if (!tweenCase.IsCompleted)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 모든 TweenCase를 즉시 완료 상태로 전환합니다.
        /// </summary>
        public void Complete()
        {
            foreach (var tweenCase in tweenCases)
            {
                tweenCase.Complete();
            }
        }

        /// <summary>
        /// 모든 TweenCase를 강제 종료합니다.
        /// </summary>
        public void Kill()
        {
            foreach (var tweenCase in tweenCases)
            {
                tweenCase.Kill();
            }
        }

        /// <summary>
        /// 컬렉션 전체가 완료되었을 때 호출될 콜백을 등록합니다.
        /// </summary>
        public void OnComplete(SimpleCallback callback)
        {
            tweensCompleted += callback;
        }

        /// <summary>
        /// 내부 완료 콜백: 모든 TweenCase가 완료되면 등록된 콜백을 실행합니다.
        /// </summary>
        private void OnTweenCaseComplete()
        {
            foreach (var tweenCase in tweenCases)
            {
                if (!tweenCase.IsCompleted)
                    return;
            }
            tweensCompleted?.Invoke();
        }

        /// <summary>
        /// '+' 연산자 오버로드: 기존 컬렉션에 TweenCase를 추가하여 반환합니다.
        /// </summary>
        public static TweenCaseCollection operator +(TweenCaseCollection caseCollection, TweenCase tweenCase)
        {
            if (caseCollection == null)
                caseCollection = new TweenCaseCollection();
            caseCollection.AddTween(tweenCase);
            return caseCollection;
        }
    }
}
