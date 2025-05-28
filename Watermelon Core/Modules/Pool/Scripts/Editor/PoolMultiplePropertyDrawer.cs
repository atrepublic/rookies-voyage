// 스크립트 기능 요약:
// 이 스크립트는 Unity 에디터 확장 기능으로, PoolMultiple 클래스의 인스펙터 표시 방식을 사용자 정의합니다.
// PoolMultiple 객체가 Inspector에 표시될 때 해당 속성들을 깔끔하게 배치하고,
// 다중 프리팹 목록, 각 프리팹의 가중치 및 선택 확률, 최대 크기 제한 등을 시각적으로 편집할 수 있는 GUI를 제공합니다.
// PropertyDrawer를 상속받아 특정 타입의 속성(PoolMultiple)에 대한 그리기를 담당합니다.

using System.Globalization; // 소수점 형식 지정을 위해 필요
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    // CustomPropertyDrawer 속성을 통해 PoolMultiple 타입의 속성을 그릴 때 이 PropertyDrawer를 사용하도록 지정합니다.
    [CustomPropertyDrawer(typeof(PoolMultiple))]
    // UnityEditor.PropertyDrawer를 상속받아 사용자 정의 속성 그리기를 구현합니다.
    public class PoolMultiplePropertyDrawer : UnityEditor.PropertyDrawer
    {
        // multiListLablesStyle: 다중 풀 목록에서 사용될 라벨들의 GUI 스타일입니다.
        [Tooltip("다중 풀 목록 라벨 GUI 스타일")]
        private GUIStyle multiListLablesStyle;

        /// <summary>
        /// 인스펙터 GUI를 그릴 때 사용할 스타일들을 초기화합니다.
        /// </summary>
        private void InitStyles()
        {
            // 에디터 스킨에 따라 라벨 색상을 설정합니다.
            Color labelColor = EditorGUIUtility.isProSkin ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.12f, 0.12f, 0.12f);

            // multiListLablesStyle을 초기화하고 글꼴 크기 및 색상을 설정합니다.
            multiListLablesStyle = new GUIStyle();
            multiListLablesStyle.fontSize = 8;
            multiListLablesStyle.normal.textColor = labelColor;
        }

        /// <summary>
        /// PoolMultiple 속성의 인스펙터 GUI를 그리는 함수입니다.
        /// 속성의 각 필드(이름, 프리팹 목록, 컨테이너, 크기 제한 등)를 레이아웃에 맞게 그립니다.
        /// </summary>
        /// <param name="position">속성이 그려질 GUI 영역</param>
        /// <param name="property">그릴 PoolMultiple 속성의 SerializedProperty</param>
        /// <param name="label">속성의 라벨</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 스타일을 초기화합니다.
            InitStyles();

            // GUI 그리기를 시작함을 알립니다.
            EditorGUI.BeginProperty(position, label, property);

            // 속성 그리기 영역의 시작 위치 및 기본 높이를 설정합니다.
            float x = position.x;
            float y = position.y;
            float width = (position.width - EditorGUIUtility.labelWidth);
            float height = EditorGUIUtility.singleLineHeight;

            // PoolMultiple 속성의 자식 속성들(name, multiPoolPrefabsList 등)에 대한 SerializedProperty를 찾습니다.
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty poolsListProperty = property.FindPropertyRelative("multiPoolPrefabsList");

            // 속성의 확장/축소 폴드아웃을 그립니다.
            // 속성이 확장되지 않은 경우 라벨에 풀 이름을 포함하여 표시합니다.
            property.isExpanded = EditorGUI.Foldout(new Rect(x, y, position.width, height), property.isExpanded, property.isExpanded ? property.displayName : GetLabel(property.displayName, nameProperty.stringValue), true);

            // 속성이 확장된 경우 상세 정보를 그립니다.
            if (property.isExpanded)
            {
                // 인스펙트 레벨에 따른 들여쓰기를 적용합니다.
                x += 18;
                y += EditorGUIUtility.singleLineHeight + 2;
                width = position.width - 18;

                // 'Name' 필드를 그립니다.
                EditorGUI.PropertyField(new Rect(x, y, width, height), nameProperty);

                // 'Pools list' (다중 프리팹 목록) 섹션을 그립니다.
                int arraySize = poolsListProperty.arraySize; // 프리팹 목록의 현재 크기

                y += EditorGUIUtility.singleLineHeight + 2;

                // 'Prefabs amount' 필드를 비활성화된 상태로 표시합니다.
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.IntField(new Rect(x, y, width - 40, height), new GUIContent("Prefabs amount:"), arraySize);
                }

                // 프리팹 목록 항목을 줄이는 '-' 버튼을 그립니다.
                if (GUI.Button(new Rect(x + width - 38, y, 18, 18), "-"))
                {
                    // 버튼 클릭 시 로직 (현재 비어있음, PropertyDrawer에서는 GUI 그리기만 담당하고 로직은 외부에서 처리해야 함)
                }

                // 프리팹 목록 항목을 늘리는 '+' 버튼을 그립니다.
                if (GUI.Button(new Rect(x + width - 18, y, 18, 18), "+"))
                {
                    // 버튼 클릭 시 로직 (현재 비어있음, PropertyDrawer에서는 GUI 그리기만 담당하고 로직은 외부에서 처리해야 함)
                }

                // 다중 프리팹 목록의 헤더 라벨 ('objects', 'weights' 등)을 그립니다.
                y += EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(new Rect(x + 2, y + 5, 60, 10), "objects", multiListLablesStyle);
                EditorGUI.LabelField(new Rect(x + width - 75, y + 5, 75, 10), "weights", multiListLablesStyle);

                // 다중 프리팹 목록의 각 항목을 그립니다.
                if (arraySize > 0)
                {
                    for(int i = 0; i < arraySize; i++)
                    {
                        y += EditorGUIUtility.singleLineHeight + 2;

                        // 현재 항목 (MultiPoolPrefab)의 SerializedProperty를 가져옵니다.
                        SerializedProperty arrayProperty = poolsListProperty.GetArrayElementAtIndex(i);

                        // MultiPoolPrefab 구조체의 자식 속성들(Prefab, Weight, isWeightLocked)에 대한 SerializedProperty를 찾습니다.
                        SerializedProperty arrayPrefabProperty = arrayProperty.FindPropertyRelative("Prefab");
                        SerializedProperty arrayWeightProperty = arrayProperty.FindPropertyRelative("Weight");
                        SerializedProperty arrayIsLockedProperty = arrayProperty.FindPropertyRelative("isWeightLocked");

                        // 프리팹 필드 및 가중치 필드의 너비를 계산합니다.
                        float prefabWidth = width * 0.6f;
                        float weightWidth = width * 0.1f;

                        // 'Prefab' 필드를 그립니다. (라벨 없이)
                        EditorGUI.PropertyField(new Rect(x, y, prefabWidth, height), arrayPrefabProperty, GUIContent.none);
                        // 'Weight' 필드를 그립니다. (라벨 없이)
                        EditorGUI.PropertyField(new Rect(x + prefabWidth + 4 + weightWidth, y, weightWidth, height), arrayWeightProperty, GUIContent.none);
                        // 가중치에 따른 선택 확률을 계산하여 표시합니다. (소수점 첫째 자리까지 퍼센트 형식으로)
                        EditorGUI.LabelField(new Rect(x + prefabWidth + 4 + weightWidth + 4 + weightWidth, y, 50, height), new GUIContent(GetChance(poolsListProperty, arrayWeightProperty.intValue).ToString("P01", CultureInfo.InvariantCulture)));

                        // isWeightLocked 필드는 PropertyDrawer에서 직접 그리지 않고, PoolManagerEditor에서 아이콘 버튼 등으로 처리될 수 있습니다.
                        // 현재 이 PropertyDrawer에서는 isWeightLocked를 직접 편집하는 GUI를 그리지 않습니다.
                    }
                }
                else
                {
                    // 목록이 비어있는 경우 추가적인 표시는 하지 않습니다.
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
                if (capSizeProperty.boolValue)
                {
                    SerializedProperty maxSizeProperty = property.FindPropertyRelative("maxSize");
                    y += EditorGUIUtility.singleLineHeight + 2;
                    EditorGUI.PropertyField(new Rect(x, y, width, height), maxSizeProperty);
                }
            }

            // GUI 그리기를 마쳤음을 알립니다.
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 다중 풀에서 특정 프리팹의 가중치에 따른 선택 확률을 계산하는 함수입니다.
        /// 모든 프리팹의 가중치 합계에 대한 해당 프리팹 가중치의 비율을 반환합니다.
        /// </summary>
        /// <param name="arrayProperty">MultiPoolPrefab 목록을 나타내는 SerializedProperty (poolsListProperty)</param>
        /// <param name="weight">확률을 계산할 프리팹의 가중치</param>
        /// <returns>선택 확률 (0.0 ~ 1.0 사이의 값)</returns>
        private float GetChance(SerializedProperty arrayProperty, int weight)
        {
            int totalWeight = 0; // 전체 가중치 합계

            // MultiPoolPrefab 목록을 순회하며 모든 프리팹의 가중치를 합산합니다.
            int arraySize = arrayProperty.arraySize;
            for (int i = 0; i < arraySize; i++)
            {
                totalWeight += arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Weight").intValue;
            }

            // 전체 가중치 합계가 0보다 크면 가중치에 대한 전체 가중치의 비율을 반환합니다.
            // 0이면 0을 반환하여 나누기 오류를 방지합니다.
            return totalWeight > 0 ? (float)weight / totalWeight : 0f;
        }

        /// <summary>
        /// 속성 라벨에 풀 이름을 추가하여 더 상세한 라벨 텍스트를 생성하는 함수입니다.
        /// 속성이 확장되지 않은 상태에서 인스펙터에 표시될 때 사용됩니다.
        /// </summary>
        /// <param name="propertyName">기본 속성 이름 (예: "Pools")</param>
        /// <param name="poolName">PoolMultiple 객체의 이름</param>
        /// <returns>풀 이름이 추가된 라벨 텍스트</returns>
        private string GetLabel(string propertyName, string poolName)
        {
            // 풀 이름이 비어있지 않으면 기본 속성 이름 뒤에 괄호와 함께 풀 이름을 추가합니다.
            if (!string.IsNullOrEmpty(poolName))
            {
                propertyName += " (" + poolName + ")";
            }

            return propertyName; // 최종 라벨 텍스트를 반환합니다.
        }

        /// <summary>
        /// PoolMultiple 속성이 인스펙터에 표시될 때 필요한 높이를 계산하는 함수입니다.
        /// 속성의 확장 여부, 최대 크기 제한 활성화 여부, 다중 프리팹 목록의 크기 등을 고려하여 높이를 반환합니다.
        /// </summary>
        /// <param name="property">높이를 계산할 PoolMultiple 속성의 SerializedProperty</param>
        /// <param name="label">속성의 라벨</param>
        /// <returns>속성을 그리는 데 필요한 높이</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 속성이 확장된 경우 상세 정보를 그리는 데 필요한 높이를 계산합니다.
            if (property.isExpanded)
            {
                // 기본 필드 수 (이름, 프리팹 목록 헤더, 컨테이너, Cap size, Max size)를 고려한 높이입니다.
                // (주의: 'Prefabs amount' 필드와 헤더 라벨들도 높이에 포함되어야 함)
                int variablesCount = 5; // 이름, 프리팹 목록 헤더, 컨테이너, Cap size, Max size (capSize 활성화 시)

                // 기본 필드 높이와 필드 간 간격을 더한 높이입니다.
                float height = EditorGUIUtility.singleLineHeight * variablesCount + 2 * (variablesCount - 1);

                // 다중 프리팹 목록 헤더 라벨 및 공간에 대한 높이를 추가합니다.
                height += EditorGUIUtility.singleLineHeight * 2 + 4;

                // 다중 프리팹 목록의 각 항목에 대한 높이를 추가합니다.
                SerializedProperty poolsListProperty = property.FindPropertyRelative("multiPoolPrefabsList");
                height += poolsListProperty.arraySize * EditorGUIUtility.singleLineHeight;

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