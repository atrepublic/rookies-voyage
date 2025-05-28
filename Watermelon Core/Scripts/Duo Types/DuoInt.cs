// DuoInt.cs
// --------------------------------------------------------
// 최소값과 최대값을 하나의 구조로 다룰 수 있는 범위 정수 클래스
// 데미지, 확률, 수치 랜덤화 등 다양한 게임 시스템에서 활용됨

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class DuoInt
    {
        [Tooltip("범위의 최소값")]
        public int firstValue;

        [Tooltip("범위의 최대값")]
        public int secondValue;

        public DuoInt(int firstValue, int secondValue)
        {
            this.firstValue = firstValue;
            this.secondValue = secondValue;
        }

        public DuoInt(int value)
        {
            this.firstValue = value;
            this.secondValue = value;
        }

        [Tooltip("1~1 범위의 DuoInt 기본 인스턴스")]
        public static DuoInt One => new DuoInt(1);

        [Tooltip("0~0 범위의 DuoInt 기본 인스턴스")]
        public static DuoInt Zero => new DuoInt(0);

        /// <summary>
        /// DuoInt 범위 내 무작위 정수를 반환합니다.
        /// </summary>
        public int Random()
        {
            return UnityEngine.Random.Range(firstValue, secondValue + 1);
        }

        /// <summary>
        /// 전달된 값을 DuoInt 범위 내로 제한합니다.
        /// </summary>
        public int Clamp(int value)
        {
            return Mathf.Clamp(value, firstValue, secondValue);
        }

        /// <summary>
        /// 보간 인자(t)에 따라 DuoInt 범위 내 선형 보간 값을 정수로 반환합니다.
        /// </summary>
        public int Lerp(float t)
        {
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(firstValue, secondValue, t)), firstValue, secondValue);
        }

        public static implicit operator Vector2Int(DuoInt value) => new Vector2Int(value.firstValue, value.secondValue);
        public static explicit operator DuoInt(Vector2Int vec) => new DuoInt(vec.x, vec.y);

        public static DuoInt operator *(DuoInt a, DuoInt b) => new DuoInt(a.firstValue * b.firstValue, a.secondValue * b.secondValue);

        public static DuoInt operator /(DuoInt a, DuoInt b)
        {
            if ((b.firstValue == 0) || (b.secondValue == 0))
                throw new System.DivideByZeroException();

            return new DuoInt(a.firstValue / b.firstValue, a.secondValue / b.secondValue);
        }

        public static DuoInt operator *(DuoInt a, float b) => new DuoInt((int)(a.firstValue * b), (int)(a.secondValue * b));

        public static DuoInt operator /(DuoInt a, float b)
        {
            if (b == 0)
                throw new System.DivideByZeroException();

            return new DuoInt((int)(a.firstValue / b), (int)(a.secondValue / b));
        }

        /// <summary>
        /// DuoFloat 구조체로 변환합니다. (float 기반 범위)
        /// </summary>
        public DuoFloat ToDuoFloat()
        {
            return new DuoFloat(firstValue, secondValue);
        }

        public override string ToString()
        {
            return "(" + firstValue + ", " + secondValue + ")";
        }
    }
}
