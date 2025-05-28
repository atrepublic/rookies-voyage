// 스크립트 기능 요약:
// 이 스크립트는 게임 내 화폐 금액을 읽기 쉬운 형식(예: 1000 -> 1k, 1500000 -> 1.5M)으로 변환하는 유틸리티 함수를 제공하는 정적 클래스입니다.
// 큰 숫자를 표현할 때 사용되는 약어(K, M, B 등)와 소수점 처리를 관리하여 사용자 친화적인 금액 표시를 돕습니다.

using UnityEngine; // Mathf 사용을 위해 필요

namespace Watermelon
{
    // CurrencyHelper 클래스는 화폐 금액 형식화와 관련된 유틸리티 함수를 제공하는 정적 클래스입니다.
    public static class CurrencyHelper
    {
        // DIGITS: 큰 숫자를 표현할 때 사용되는 약어 배열입니다.
        // 인덱스 0: 10^0 ~ 10^2 (빈 문자열)
        // 인덱스 1: 10^3 ~ 10^5 (K: Thousand)
        // 인덱스 2: 10^6 ~ 10^8 (M: Million)
        // 인덱스 3: 10^9 ~ 10^11 (B: Billion)
        // 인덱스 4: 10^12 ~ 10^14 (T: Trillion)
        // 인덱스 5: 10^15 ~ 10^17 (Qa: Quadrillion)
        [Tooltip("큰 숫자를 표현할 때 사용되는 약어 배열")]
        private static readonly string[] DIGITS = new string[] { "", "K", "M", "B", "T", "Qa" };

        /// <summary>
        /// 정수형 화폐 금액을 읽기 쉬운 형식의 문자열로 변환합니다.
        /// (예: 1234 -> 1.2K, 1500000 -> 1.5M)
        /// </summary>
        /// <param name="value">형식화할 화폐 금액 (정수)</param>
        /// <returns>형식화된 화폐 금액 문자열</returns>
        public static string Format(int value)
        {
            float moneyRepresentation = value; // 금액을 float으로 변환하여 나누기 연산에 사용
            int counter = 0; // 1000으로 나눈 횟수 (약어 인덱스)

            // 금액이 1000 이상인 동안 1000으로 나누고 카운터를 증가시킵니다.
            while (moneyRepresentation >= 1000)
            {
                moneyRepresentation /= 1000;
                counter++;
            }

            // moneyRepresentation 값과 카운터(약어 인덱스)에 따라 형식화 방식을 결정합니다.
            if (moneyRepresentation >= 100)
            {
                // 100 이상인 경우, 소수점 없이 반올림하여 정수 부분만 표시합니다.
                moneyRepresentation = Mathf.Floor(moneyRepresentation);
                // 카운터가 0이 아니면 약어를 붙입니다.
                if (counter != 0)
                    return moneyRepresentation.ToString("F0") + GetDigits(counter); // 소수점 0자리로 형식화
            }
            else if (moneyRepresentation >= 10)
            {
                // 10 이상 100 미만인 경우, 소수점 첫째 자리까지 표시합니다.
                string result = moneyRepresentation.ToString("F1");

                // 소수점 첫째 자리가 0이면 소수점 이하를 제거합니다. (예: 12.0 -> 12)
                if (result[result.Length - 1] == '0')
                    result = result.Remove(result.Length - 2); // 소수점과 0 모두 제거

                // 카운터가 0이 아니면 약어를 붙입니다.
                if (counter != 0)
                    return result + GetDigits(counter);
            }
            else
            {
                // 10 미만인 경우, 소수점 둘째 자리까지 표시합니다.
                string result = moneyRepresentation.ToString("F2");

                // 소수점 둘째 자리가 0이면 제거합니다.
                if (result[result.Length - 1] == '0')
                {
                    result = result.Remove(result.Length - 1); // 소수점 둘째 자리 0 제거

                    // 소수점 첫째 자리도 0이면 제거합니다. (예: 1.00 -> 1, 1.20 -> 1.2)
                    if (result[result.Length - 1] == '0')
                        result = result.Remove(result.Length - 2); // 소수점과 0 모두 제거
                }

                // 카운터가 0이 아니면 약어를 붙입니다.
                if (counter != 0)
                    return result + GetDigits(counter);
            }

            // 카운터가 0인 경우 (1000 미만) 소수점 없이 반올림하여 정수형으로 반환합니다.
            return Mathf.RoundToInt(moneyRepresentation).ToString();
        }

        /// <summary>
        /// 지정된 인덱스에 해당하는 약어 문자열을 반환하는 함수입니다.
        /// DIGITS 배열의 범위를 벗어나는 인덱스는 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="index">가져올 약어의 인덱스 (1000으로 나눈 횟수)</param>
        /// <returns>해당 인덱스의 약어 문자열 또는 빈 문자열</returns>
        private static string GetDigits(int index)
        {
            // 인덱스가 DIGITS 배열의 유효 범위를 벗어나면 빈 문자열을 반환합니다.
            if (index < 0 || index >= DIGITS.Length)
                return "";

            return DIGITS[index]; // 해당 인덱스의 약어 문자열 반환
        }
    }
}