// DuoColor.cs
// 이 스크립트는 두 가지 색상 사이의 보간 및 랜덤 색상 선택 기능을 제공하는 유틸리티 클래스입니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class DuoColor
    {
        [Tooltip("보간 또는 랜덤 추출 시 사용될 첫 번째 색상 값(시작 값)")]
        public Color32 firstValue;

        [Tooltip("보간 또는 랜덤 추출 시 사용될 두 번째 색상 값(끝 값)")]
        public Color32 secondValue;

        /// <summary>
        /// 생성자: 첫 번째와 두 번째 색상을 지정하여 DuoColor를 초기화합니다.
        /// </summary>
        /// <param name="first">시작 색상</param>
        /// <param name="second">끝 색상</param>
        public DuoColor(Color32 first, Color32 second)
        {
            firstValue = first;
            secondValue = second;
        }

        /// <summary>
        /// 생성자: 단일 색상을 지정하여 firstValue와 secondValue를 동일하게 설정합니다.
        /// </summary>
        /// <param name="color">적용할 색상</param>
        public DuoColor(Color32 color)
        {
            firstValue = color;
            secondValue = color;
        }

        /// <summary>
        /// 주어진 상태(state)에 따라 firstValue와 secondValue 사이를 선형 보간하여 반환합니다.
        /// </summary>
        /// <param name="state">0.0(첫 번째 색상) ~ 1.0(두 번째 색상) 사이의 보간 값</param>
        /// <returns>보간된 Color32 값</returns>
        public Color32 Lerp(float state)
        {
            return Color32.Lerp(firstValue, secondValue, state);
        }

        /// <summary>
        /// firstValue와 secondValue 사이에서 무작위 값을 랜덤으로 반환합니다.
        /// </summary>
        /// <returns>두 색상 사이의 임의 색상</returns>
        public Color32 RandomBetween()
        {
            return Color32.Lerp(firstValue, secondValue, Random.value);
        }
    }
}
