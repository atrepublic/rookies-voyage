// SkinDataPropertyDrawer.cs
// 이 스크립트는 AbstractSkinData 기반 스킨 객체를 인스펙터에서 보기 좋게 시각화하는 커스텀 PropertyDrawer입니다.
// 스킨 ID, 잠금 상태, 미리보기 객체를 자동으로 표시하며, 모든 하위 필드를 확장형으로 출력합니다.

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(AbstractSkinData), true)]
    public class SkinDataPropertyDrawer : PropertyDrawer, System.IDisposable
    {
        private Dictionary<int, PropertyData> propertiesData = new Dictionary<int, PropertyData>();

        // 인스펙터에 해당 스킨 데이터 속성을 출력합니다.
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyData data = GetPropertyData(property);

            EditorGUI.BeginProperty(position, label, property);

            if (data.PreviewProperty != null)
            {
                position.width -= 60; // 미리보기 박스 공간 확보
            }

            Rect propertyPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // 등록된 모든 필드를 차례로 출력
            foreach (var subProperty in data.Properties)
            {
                EditorGUI.PropertyField(propertyPosition, subProperty, true);
                propertyPosition.y += EditorGUI.GetPropertyHeight(subProperty, GUIContent.none, true) + 2;
            }

            // 미리보기 박스 출력
            if (data.PreviewProperty != null)
            {
                Rect boxRect = new Rect(position.x + propertyPosition.width + 2, position.y, 58, 58);
                GUI.Box(boxRect, GUIContent.none);

                Object prefabObject = data.PreviewProperty.objectReferenceValue;
                if (prefabObject != null)
                {
                    Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabObject);
                    if (previewTexture != null)
                    {
                        GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), previewTexture);
                    }
                }
                else
                {
                    GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), EditorCustomStyles.GetMissingIcon());
                }
            }

            EditorGUI.EndProperty();
        }

        // 각 필드의 높이를 계산하여 전체 높이 반환
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropertyData data = GetPropertyData(property);

            float height = 0;
            int propertiesCount = data.Properties.Count;

            if (propertiesCount > 0)
            {
                foreach (SerializedProperty subProperty in data.Properties)
                {
                    height += EditorGUI.GetPropertyHeight(subProperty, GUIContent.none, true);
                }

                height += (propertiesCount - 1) * 2; // 각 필드 간 간격
            }

            return Mathf.Clamp(height, 58, float.MaxValue);
        }

        // 캐시된 PropertyData를 반환하거나 새로 생성합니다.
        private PropertyData GetPropertyData(SerializedProperty property)
        {
            int hash = property.propertyPath.GetHashCode();
            if (!propertiesData.TryGetValue(hash, out PropertyData propertyData))
            {
                propertyData = new PropertyData(property);
                propertiesData.Add(hash, propertyData);
            }
            return propertyData;
        }

        // 에디터 종료 시 캐시 해제
        public void Dispose()
        {
            propertiesData.Clear();
        }

        // 각 스킨 데이터의 필드와 미리보기 정보를 저장하는 클래스
        private class PropertyData
        {
            public List<SerializedProperty> Properties { get; private set; }
            public SerializedProperty PreviewProperty { get; private set; }

            public PropertyData(SerializedProperty property)
            {
                PreviewProperty = null;
                Properties = new List<SerializedProperty>
                {
                    property.FindPropertyRelative("id")
                };

                System.Type targetType = property.boxedValue.GetType();
                IEnumerable<FieldInfo> fieldInfos = targetType.GetFields(ReflectionUtils.FLAGS_INSTANCE);

                foreach (var field in fieldInfos)
                {
                    SerializedProperty subProperty = property.FindPropertyRelative(field.Name);
                    if (subProperty != null)
                    {
                        Properties.Add(subProperty);

                        if (field.GetCustomAttribute<SkinPreviewAttribute>() != null)
                        {
                            PreviewProperty = subProperty;
                        }
                    }
                }
            }
        }
    }
}
