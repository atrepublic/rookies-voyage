/*
 * AnchoredPositionTweenBehavior.cs
 * ------------------------------------------------------------
 * RectTransform의 anchoredPosition3D 값을 트위닝하여 UI 요소의 위치를
 * 애니메이션합니다.
 * • 시작 위치(startValue) → 목표 위치(endValue)로 duration 시간 동안 보간
 * • TweenBehavior<TComponent, TValue> 추상 클래스를 상속하여 공통 로직 재사용
 * • 루프 타입‧딜레이‧이징 설정은 부모 클래스(TweenBehavior) 인스펙터에서 제어
 * ------------------------------------------------------------
 */
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(RectTransform))]
    public class AnchoredPositionTweenBehavior : TweenBehavior<RectTransform, Vector3>
    {
        // TargetValue 프로퍼티 -------------------------------------------------
        /// <summary>
        /// RectTransform의 현재 anchoredPosition3D 값을 가져오거나 설정합니다.
        /// </summary>
        protected override Vector3 TargetValue
        {
            get => TargetComponent.anchoredPosition3D;
            set => TargetComponent.anchoredPosition3D = value;
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
            // RectTransform.DOAnchoredPosition 확장 메서드로 트윈 생성
            tweenCase = TargetComponent.DOAnchoredPosition(endValue, duration);

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
