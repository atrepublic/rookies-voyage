/*
 * TweenCaseFunction.cs
 * 제네릭 기반 TweenCase 추상 클래스입니다.
 * tweenObject 대상에 대한 startValue에서 resultValue까지 보간값을 적용하는 기본 구조를 제공합니다.
 */
using UnityEngine;

namespace Watermelon
{
    public abstract class TweenCaseFunction<TBaseObject, TValue> : TweenCase
    {
        [Tooltip("트윈 대상 객체")]
        public TBaseObject tweenObject;

        [Tooltip("보간 시작 값")]
        public TValue startValue;

        [Tooltip("보간 최종 값")]
        public TValue resultValue;

        /// <summary>
        /// tweenObject와 resultValue를 초기화하는 생성자입니다.
        /// </summary>
        public TweenCaseFunction(TBaseObject tweenObject, TValue resultValue)
        {
            this.tweenObject = tweenObject;
            this.resultValue = resultValue;
        }
    }
}
