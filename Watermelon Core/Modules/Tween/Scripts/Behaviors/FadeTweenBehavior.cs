/*
 * FadeTweenBehavior.cs
 * ------------------------------------------------------------
 * CanvasGroup의 alpha 값을 트위닝하여 UI 요소의 페이드 인·아웃 효과를 구현하는
 * 컴포넌트입니다.
 * • 시작 알파(startValue) → 목표 알파(endValue) 로 duration 시간 동안 보간
 * • TweenBehavior<TComponent, TValue> 추상 클래스를 상속하여 공통 로직 재사용
 * • 루프 타입·딜레이·이징 설정은 부모 클래스(TweenBehavior) 인스펙터에서 제어
 * ------------------------------------------------------------
 */
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeTweenBehavior : TweenBehavior<CanvasGroup, float>
    {
        // TargetValue 프로퍼티 -------------------------------------------------
        /// <summary>
        /// CanvasGroup의 현재 alpha(투명도) 값을 가져오거나 설정합니다.
        /// </summary>
        protected override float TargetValue
        {
            get => TargetComponent.alpha;
            set => TargetComponent.alpha = value;
        }

        // StartLoop -----------------------------------------------------------
        /// <summary>
        /// Tween 루프를 시작합니다.
        /// </summary>
        /// <param name="delay">딜레이(초)</param>
        protected override void StartLoop(float delay)
        {
            // 시작 값 지정
            TargetValue = startValue;
            // CanvasGroup.DOFade 확장 메서드로 트윈 생성
            tweenCase = TargetComponent.DOFade(endValue, duration);

            base.StartLoop(delay); // 부모 클래스 공통 처리 호출
        }

        // IncrementLoopChangeValues ------------------------------------------
        /// <summary>
        /// LoopType.Increment 모드에서 매 반복마다 start/end 값을 갱신합니다.
        /// </summary>
        protected override void IncrementLoopChangeValues()
        {
            var difference = endValue - startValue;
            startValue = endValue;
            endValue += difference;
        }
    }
}
