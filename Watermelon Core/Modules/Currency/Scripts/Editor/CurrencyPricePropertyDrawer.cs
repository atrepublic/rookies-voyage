// CurrencyPricePropertyDrawer.cs
// 이 스크립트는 CurrencyPrice 구조체를 Unity 에디터의 인스펙터 창에 사용자 정의하여 표시하는 PropertyDrawer입니다.
// 통화 가격과 통화 타입을 나란히 표시하여 편집하기 쉽게 만듭니다.

using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(CurrencyPrice))]
    public class CurrencyPricePropertyDrawer : UnityEditor.PropertyDrawer
    {
        // 가격과 통화 타입을 표시할 컬럼 수입니다.
        private const int ColumnCount = 2;
        // 컬럼 사이의 간격 크기입니다.
        private const int GapSize = 6;
        // 컬럼 사이의 간격 개수입니다.
        private const int GapCount = ColumnCount - 1;

        /// <summary>
        /// 인스펙터에서 CurrencyPrice 속성을 그릴 때 호출되는 함수입니다.
        /// 가격과 통화 타입 필드를 나란히 정렬하여 표시합니다.
        /// </summary>
        /// <param name="position">GUI 요소가 그려질 영역의 Rect</param>
        /// <param name="property">그려질 CurrencyPrice 속성</param>
        /// <param name="label">속성에 대한 GUIContent 라벨</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 속성 그리기를 시작함을 에디터에 알립니다.
            EditorGUI.BeginProperty(position, label, property);

            // 현재 그리기 영역의 X, Y 좌표와 전체 너비를 가져옵니다.
            float x = position.x;
            float y = position.y;
            // 각 컬럼(통화 타입, 가격)의 너비를 계산합니다. (전체 너비 - 라벨 너비 - 간격) / 컬럼 수
            float width = (position.width - EditorGUIUtility.labelWidth - GapCount * GapSize) / ColumnCount;
            // 한 줄의 높이를 가져옵니다.
            float height = EditorGUIUtility.singleLineHeight;
            // 다음 컬럼으로 이동하기 위한 오프셋 값을 계산합니다. (컬럼 너비 + 간격)
            float offset = width + GapSize;

            // 'price' 속성을 찾습니다.
            SerializedProperty priceProperty = property.FindPropertyRelative("price");

            // 속성의 라벨을 표시합니다.
            EditorGUI.PrefixLabel(new Rect(x, y, position.width, position.height), new GUIContent(property.displayName));
            // 통화 타입 속성을 표시합니다. (라벨 없음)
            EditorGUI.PropertyField(new Rect(x + EditorGUIUtility.labelWidth + 2, y, width, height), property.FindPropertyRelative("currencyType"), GUIContent.none);
            // 가격 속성을 표시합니다. (라벨 없음)
            EditorGUI.PropertyField(new Rect(x + EditorGUIUtility.labelWidth + offset, y, width, height), priceProperty, GUIContent.none);

            // 가격 값이 0보다 작으면 0으로 설정하여 음수 값을 방지합니다.
            if (priceProperty.intValue < 0)
                priceProperty.intValue = 0;

            // 속성 그리기가 끝났음을 에디터에 알립니다.
            EditorGUI.EndProperty();
        }
    }
}