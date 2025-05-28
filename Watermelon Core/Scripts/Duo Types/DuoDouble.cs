// DuoDouble.cs
// 이 스크립트는 두 개의 double 값 범위를 관리하는 유틸리티 클래스로,
// 최소값과 최대값을 설정하고, 난수 생성, 값을 범위 내로 클램핑(clamp)하는 기능을 제공합니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class DuoDouble
    {
        [Tooltip("범위의 최소값(첫 번째 값)")]
        public double firstValue;

        [Tooltip("범위의 최대값(두 번째 값)")]
        public double secondValue;

        // 난수 생성에 사용할 Random 인스턴스 (static)
        private static System.Random random;

        /// <summary>
        /// 생성자: 주어진 두 값으로 범위를 초기화합니다.
        /// </summary>
        /// <param name="firstValue">범위의 최소값</param>
        /// <param name="secondValue">범위의 최대값</param>
        public DuoDouble(double firstValue, double secondValue)
        {
            this.firstValue = firstValue;
            this.secondValue = secondValue;
        }

        /// <summary>
        /// 생성자: 단일 값으로 최소/최대값을 동일하게 설정합니다.
        /// </summary>
        /// <param name="value">최소값 및 최대값으로 사용할 값</param>
        public DuoDouble(double value)
        {
            this.firstValue = value;
            this.secondValue = value;
        }

        /// <summary>
        /// 정적 프로퍼티: (1,1) 범위를 나타내는 DuoDouble 인스턴스를 반환합니다.
        /// </summary>
        public static DuoDouble One => new DuoDouble(1);

        /// <summary>
        /// 정적 프로퍼티: (0,0) 범위를 나타내는 DuoDouble 인스턴스를 반환합니다.
        /// </summary>
        public static DuoDouble Zero => new DuoDouble(0);

        /// <summary>
        /// 범위 내에서 난수를 생성하여 반환합니다.
        /// </summary>
        /// <returns>firstValue와 secondValue 사이의 임의의 double 값</returns>
        public double Random()
        {
            if (random == null)
            {
                random = new System.Random();
            }

            return random.NextDouble() * (this.secondValue - this.firstValue) + this.firstValue;
        }

        /// <summary>
        /// 주어진 값을 범위 내로 클램핑하여 반환합니다.
        /// </summary>
        /// <param name="value">클램핑할 값</param>
        /// <returns>firstValue와 secondValue 사이로 제한된 값</returns>
        public double Clamp(double value)
        {
            if (value < firstValue)
            {
                return firstValue;
            }
            else if (value > secondValue)
            {
                return secondValue;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// 두 DuoDouble 인스턴스를 곱합니다.
        /// </summary>
        public static DuoDouble operator *(DuoDouble a, DuoDouble b)
            => new DuoDouble(a.firstValue * b.firstValue, a.secondValue * b.secondValue);

        /// <summary>
        /// 두 DuoDouble 인스턴스를 나눕니다. 0으로 나누면 예외가 발생합니다.
        /// </summary>
        public static DuoDouble operator /(DuoDouble a, DuoDouble b)
        {
            if (b.firstValue == 0 || b.secondValue == 0)
            {
                throw new System.DivideByZeroException();
            }

            return new DuoDouble(a.firstValue / b.firstValue, a.secondValue / b.secondValue);
        }

        /// <summary>
        /// 범위 값을 "(min, max)" 형식의 문자열로 반환합니다. 소수점 한 자리까지 표시합니다.
        /// </summary>
        public override string ToString()
        {
            return "(" + FormatValue(firstValue) + ", " + FormatValue(secondValue) + ")";
        }

        /// <summary>
        /// 내부: double 값을 소수점 한 자리 문자열로 포맷합니다.
        /// </summary>
        private string FormatValue(double value)
        {
            return value.ToString("0.0").Replace(',', '.');
        }
    }
}
