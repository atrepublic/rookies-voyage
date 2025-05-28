/*
 * TweenExtension.cs
 * 이 정적 클래스는 TweenCase, TweenCaseCollection 및 ScrollRect에 대한 확장 메서드를 제공합니다.
 * - TweenCase의 활성 여부 확인, Kill/Complete 처리
 * - TweenCase 배열 및 컬렉션의 일괄 Kill/Complete 처리
 * - ScrollRect 콘텐츠를 특정 대상 위치로 스냅하는 트윈 생성 기능
 */
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public static class TweenExtensions
    {
        /// <summary>
        /// TweenCase가 null이 아니고 활성 상태인지 여부를 반환합니다.
        /// </summary>
        public static bool ExistsAndActive(this TweenCase tweenCase)
        {
            return tweenCase != null && tweenCase.IsActive;
        }

        /// <summary>
        /// TweenCase가 활성 상태일 경우 Kill()을 호출하여 종료 처리하고 true를 반환합니다.
        /// </summary>
        public static bool KillActive(this TweenCase tweenCase)
        {
            if (tweenCase != null && tweenCase.IsActive)
            {
                tweenCase.Kill();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 배열 형태의 TweenCase에 대해 각 요소를 Kill() 처리합니다.
        /// </summary>
        public static void KillActive(this TweenCase[] tweenCases)
        {
            if (tweenCases != null)
            {
                foreach (var tweenCase in tweenCases)
                {
                    if (tweenCase != null && tweenCase.IsActive)
                    {
                        tweenCase.Kill();
                    }
                }
            }
        }

        /// <summary>
        /// TweenCaseCollection이 null이 아니고 완료되지 않은 경우 Kill()을 호출하고 true를 반환합니다.
        /// </summary>
        public static bool KillActive(this TweenCaseCollection tweenCaseCollection)
        {
            if (tweenCaseCollection != null && !tweenCaseCollection.IsComplete())
            {
                tweenCaseCollection.Kill();
                return true;
            }
            return false;
        }

        /// <summary>
        /// TweenCase가 null이 아니고 완료되지 않은 경우 Complete()를 호출하여 즉시 완료 처리하고 true를 반환합니다.
        /// </summary>
        public static bool CompleteActive(this TweenCase tweenCase)
        {
            if (tweenCase != null && !tweenCase.IsCompleted)
            {
                tweenCase.Complete();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 배열 형태의 TweenCase에 대해 완료되지 않은 요소를 Complete() 처리합니다.
        /// </summary>
        public static void CompleteActive(this TweenCase[] tweenCases)
        {
            if (tweenCases != null)
            {
                foreach (var tweenCase in tweenCases)
                {
                    if (tweenCase != null && tweenCase.IsActive)
                    {
                        tweenCase.Complete();
                    }
                }
            }
        }

        /// <summary>
        /// ScrollRect의 콘텐츠를 target 하단이 뷰포트 하단에 위치하도록 Snap 트윈을 생성합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // 성능 최적화를 위해 인라인 처리
        public static TweenCase DoSnapTargetBottom(this ScrollRect scrollRect, RectTransform target, float duration, float offsetX = 0, float offsetY = 0)
        {
            var targetPosition = target.position + Vector3.up * (scrollRect.viewport.rect.height / 2 + target.sizeDelta.y);
            return scrollRect.SnapToTarget(targetPosition, duration, offsetX, offsetY);
        }

        /// <summary>
        /// ScrollRect의 콘텐츠를 target 상단이 뷰포트 상단에 위치하도록 Snap 트윈을 생성합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // 성능 최적화를 위해 인라인 처리
        public static TweenCase DoSnapTargetTop(this ScrollRect scrollRect, RectTransform target, float duration, float offsetX = 0, float offsetY = 0)
        {
            return scrollRect.SnapToTarget(target.position, duration, offsetX, offsetY);
        }

        /// <summary>
        /// ScrollRect 콘텐츠를 지정한 위치로 Snap 트윈을 생성합니다.
        /// </summary>
        public static TweenCase SnapToTarget(this ScrollRect scrollRect, Vector3 target, float duration, float offsetX = 0, float offsetY = 0)
        {
            Vector2 contentPosition = scrollRect.viewport.InverseTransformPoint(scrollRect.content.position);
            Vector2 newPosition = scrollRect.viewport.InverseTransformPoint(target);
            newPosition = new Vector2(newPosition.x + offsetX, newPosition.y + offsetY);

            if (!scrollRect.horizontal)
                newPosition.x = contentPosition.x;

            if (!scrollRect.vertical)
                newPosition.y = contentPosition.y;

            return scrollRect.content.DOAnchoredPosition(contentPosition - newPosition, duration);
        }
    }
}
