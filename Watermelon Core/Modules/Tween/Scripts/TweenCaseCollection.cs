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
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 수정된 부분 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            if (tweenCase == null)
            {
                // null인 TweenCase는 추가하지 않거나, 경고 로그를 남길 수 있습니다.
                // Debug.LogWarning("[TweenCaseCollection] AddTween: 추가하려는 tweenCase가 null입니다.");
                return; // null이면 메서드 종료
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            tweenCase.OnComplete(OnTweenCaseComplete);
            tweenCases.Add(tweenCase);
        }

        /// <summary>
        /// 모든 TweenCase가 완료되었는지 확인합니다.
        /// </summary>
        public bool IsComplete()
        {
            // tweenCases 리스트가 null이 아니고, 모든 요소가 null이 아니며 완료되었는지 확인
            if (tweenCases == null) return true; // 리스트 자체가 없으면 완료된 것으로 간주 (상황에 따라 다를 수 있음)

            for (int i = 0; i < tweenCases.Count; i++)
            {
                var tc = tweenCases[i];
                // 리스트에 null인 tweenCase가 추가되었을 가능성을 방지 (위의 AddTween 수정으로 실제로는 발생하지 않을 것으로 예상)
                if (tc != null && !tc.IsCompleted)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 모든 TweenCase를 즉시 완료 상태로 전환합니다.
        /// </summary>
        public void Complete()
        {
            if (tweenCases == null) return;
            foreach (var tweenCase in tweenCases)
            {
                tweenCase?.Complete(); // null 체크 추가
            }
        }

        /// <summary>
        /// 모든 TweenCase를 강제 종료합니다.
        /// </summary>
        public void Kill()
        {
            if (tweenCases == null) return;
            foreach (var tweenCase in tweenCases)
            {
                tweenCase?.Kill(); // null 체크 추가
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
            if (tweenCases == null) // tweenCases가 null일 경우를 대비
            {
                tweensCompleted?.Invoke();
                return;
            }

            foreach (var tweenCase in tweenCases)
            {
                // 리스트에 null인 tweenCase가 추가되었을 가능성을 방지
                if (tweenCase != null && !tweenCase.IsCompleted)
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
            {
                // Debug.LogWarning("[TweenCaseCollection] operator+: caseCollection이 null이므로 새로 생성합니다.");
                caseCollection = new TweenCaseCollection();
            }

            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 수정된 부분 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // AddTween 내부에서 null 체크를 하므로, 여기서 tweenCase가 null이어도 AddTween이 처리합니다.
            // 만약 tweenCase가 null일 때 caseCollection 자체를 반환하지 않으려면 여기서도 체크 가능합니다.
            if (tweenCase == null)
            {
                // Debug.LogWarning("[TweenCaseCollection] operator+: 추가하려는 tweenCase가 null이므로 컬렉션에 추가하지 않습니다.");
                return caseCollection; // null tween은 추가하지 않고 기존 컬렉션 반환
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 수정 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
            
            caseCollection.AddTween(tweenCase);
            return caseCollection;
        }
    }
}