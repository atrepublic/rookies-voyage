/*
 * ColorTweenBehavior.cs
 * ------------------------------------------------------------
 * Graphic(이미지·텍스트 등)의 Color 값을 트위닝하는 컴포넌트입니다.
 * • 시작 색(startValue) → 목표 색(endValue)으로 duration 시간 동안 보간
 * • TweenBehavior<TComponent, TValue> 추상 클래스를 상속하여 공통 로직 재사용
 * • 루프 타입‧딜레이‧이징 설정은 부모 클래스(TweenBehavior) 인스펙터에서 제어
 * ------------------------------------------------------------
 */
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Graphic))]
    public class ColorTweenBehavior : TweenBehavior<Graphic, Color>
    {
        // TargetValue 프로퍼티 -------------------------------------------------
        /// <summary>
        /// Graphic 컴포넌트의 현재 색상을 가져오거나 설정합니다.
        /// </summary>
        protected override Color TargetValue
        {
            get => TargetComponent.color;
            set => TargetComponent.color = value;
        }

        // StartLoop -----------------------------------------------------------
        /// <summary>
        /// Tween 루프를 시작합니다.
        /// </summary>
        /// <param name="delay">딜레이(초)</param>
        protected override void StartLoop(float delay)
        {
            // 시작 값으로 설정
            TargetValue = startValue;
            // Graphic.DOColor 확장 메서드로 트윈 생성
            tweenCase = TargetComponent.DOColor(endValue, duration);

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
