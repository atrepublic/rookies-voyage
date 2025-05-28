// 스크립트 기능 요약:
// 이 스크립트는 Unity 에디터 확장 기능으로, Pool 클래스의 인스펙터 표시 방식을 사용자 정의합니다.
// Pool 객체가 Inspector에 표시될 때 해당 속성들을 깔끔하게 배치하고,
// 프리팹, 이름, 컨테이너, 최대 크기 제한 등을 시각적으로 편집할 수 있는 GUI를 제공합니다.
// PropertyDrawer를 상속받아 특정 타입의 속성(Pool)에 대한 그리기를 담당합니다.

using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    // CustomPropertyDrawer 속성을 통해 Pool 타입의 속성을 그릴 때 이 PropertyDrawer를 사용하도록 지정합니다.
    [CustomPropertyDrawer(typeof(Pool))]
    // UnityEditor.PropertyDrawer를 상속받아 사용자 정의 속성 그리기를 구현합니다.
    public class PoolPropertyDrawer : UnityEditor.PropertyDrawer
    {
        /// <summary>
        /// Pool 속성의 인스펙터 GUI를 그리는 함수입니다.
        /// 속성의 각 필드(이름, 프리팹, 컨테이너, 크기 제한 등)를 레이아웃에 맞게 그립니다.
        /// </summary>
        /// <param name="position">속성이 그려질 GUI 영역</param>
        /// <param name="property">그릴 Pool 속성의 SerializedProperty</param>
        /// <param name="label">속성의 라벨</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // GUI 그리기를 시작함을 알립니다.
            EditorGUI.BeginProperty(position, label, property);

            // 속성 그리기 영역의 시작 위치 및 기본 높이를 설정합니다.
            float x = position.x;
            float y = position.y;
            float width = (position.width - EditorGUIUtility.labelWidth);
            float height = EditorGUIUtility.singleLineHeight;

            // Pool 속성의 자식 속성들(name, prefab 등)에 대한 SerializedProperty를 찾습니다.
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty prefabProperty = property.FindPropertyRelative("prefab");

            // 속성의 라벨 텍스트를 결정합니다. 속성이 확장되지 않은 경우 풀 이름을 포함합니다.
            string labelText = property.isExpanded ? property.displayName : GetLabel(property.displayName, nameProperty.stringValue);

            // 속성의 확장/축소 폴드아웃을 그립니다.
            property.isExpanded = EditorGUI.Foldout(new Rect(x, y, position.width, height), property.isExpanded, GUIContent.none, true);
            // 속성의 라벨을 그립니다. Tooltip에 전체 라벨 텍스트를 설정합니다.
            EditorGUI.PrefixLabel(new Rect(x, y, EditorGUIUtility.labelWidth - 15, height), new GUIContent(labelText, labelText));

            // 속성이 확장된 경우 상세 정보를 그립니다.
            if (property.isExpanded)
            {
                // 인스펙트 레벨에 따른 들여쓰기를 적용합니다.
                x += 18;
                y += EditorGUIUtility.singleLineHeight + 2;
                width = position.width - 18;

                // 'Name' 필드를 그립니다.
                EditorGUI.PropertyField(new Rect(x, y, width, height), nameProperty);

                // 'Prefab' 필드를 그립니다.
                y += EditorGUIUtility.singleLineHeight + 2;

                // Prefab 필드 변경 감지를 시작합니다.
                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(new Rect(x, y, width, height), prefabProperty);

                // Prefab 필드 변경이 감지된 경우 처리합니다.
                if(EditorGUI.EndChangeCheck())
                {
                    // 새로운 Prefab이 할당되었고 풀 이름이 비어있으면 Prefab 이름으로 풀 이름을 자동 설정합니다.
                    if(prefabProperty.objectReferenceValue != null && string.IsNullOrEmpty(nameProperty.stringValue))
                    {
                        nameProperty.stringValue = prefabProperty.objectReferenceValue.name;
                    }
                }

                // 'Container' 필드를 그립니다.
                SerializedProperty containerProperty = property.FindPropertyRelative("objectsContainer");
                y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(new Rect(x, y, width, height), containerProperty);

                // 'Cap size' 필드를 그립니다.
                SerializedProperty capSizeProperty = property.FindPropertyRelative("capSize");
                y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(new Rect(x, y, width, height), capSizeProperty);

                // 'Cap size'가 true이면 'Max size' 필드를 추가로 그립니다.
                if(capSizeProperty.boolValue)
                {
                    SerializedProperty maxSizeProperty = property.FindPropertyRelative("maxSize");
                    y += EditorGUIUtility.singleLineHeight + 2;
                    EditorGUI.PropertyField(new Rect(x, y, width, height), maxSizeProperty);
                }
            }
            else // 속성이 축소된 경우 요약 정보를 그립니다.
            {
                // 'Prefab' 필드를 비활성화된 상태로 요약 정보 옆에 그립니다.
                using(new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.PropertyField(new Rect(x + EditorGUIUtility.labelWidth, y, width, height), prefabProperty, GUIContent.none);
                }
            }

            // GUI 그리기를 마쳤음을 알립니다.
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 속성 라벨에 풀 이름을 추가하여 더 상세한 라벨 텍스트를 생성하는 함수입니다.
        /// 속성이 확장되지 않은 상태에서 인스펙터에 표시될 때 사용됩니다.
        /// </summary>
        /// <param name="propertyName">기본 속성 이름 (예: "Pool")</param>
        /// <param name="poolName">Pool 객체의 이름</param>
        /// <returns>풀 이름이 추가된 라벨 텍스트</returns>
        private string GetLabel(string propertyName, string poolName)
        {
            // 풀 이름이 비어있지 않으면 기본 속성 이름 뒤에 괄호와 함께 풀 이름을 추가합니다.
            if(!string.IsNullOrEmpty(poolName))
            {
                propertyName += " (" + poolName + ")";
            }

            return propertyName; // 최종 라벨 텍스트를 반환합니다.
        }

        /// <summary>
        /// Pool 속성이 인스펙터에 표시될 때 필요한 높이를 계산하는 함수입니다.
        /// 속성의 확장 여부, 최대 크기 제한 활성화 여부 등을 고려하여 높이를 반환합니다.
        /// </summary>
        /// <param name="property">높이를 계산할 Pool 속성의 SerializedProperty</param>
        /// <param name="label">속성의 라벨</param>
        /// <returns>속성을 그리는 데 필요한 높이</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 속성이 확장된 경우 상세 정보를 그리는 데 필요한 높이를 계산합니다.
            if(property.isExpanded)
            {
                // 기본 필드 수 (이름, 프리팹, 컨테이너, Cap size, Max size)를 고려한 높이입니다.
                int variablesCount = 5;

                // 기본 필드 높이와 필드 간 간격을 더한 높이입니다.
                float height = EditorGUIUtility.singleLineHeight * variablesCount + 2 * (variablesCount - 1);

                // 'Cap size'가 true이면 'Max size' 필드에 대한 높이를 추가합니다.
                if (property.FindPropertyRelative("capSize").boolValue)
                    height += (EditorGUIUtility.singleLineHeight + 2);

                return height; // 최종 높이를 반환합니다.
            }

            // 속성이 축소된 경우 기본 한 줄 높이를 반환합니다.
            return EditorGUIUtility.singleLineHeight;
        }
    }
}