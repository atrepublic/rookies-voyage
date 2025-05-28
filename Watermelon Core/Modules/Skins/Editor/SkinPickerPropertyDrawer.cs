// SkinPickerPropertyDrawer.cs
// SkinPickerAttribute가 붙은 필드에 대해 사용자 지정 UI를 제공하는 PropertyDrawer입니다.
// 인스펙터 상에서 선택된 스킨을 시각적으로 미리보이고, 선택 버튼을 제공하여 SkinPickerWindow를 호출합니다.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(SkinPickerAttribute))]
    public class SkinPickerPropertyDrawer : PropertyDrawer, System.IDisposable
    {
        private Dictionary<int, PropertyData> propertiesData = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "문자열(string) 타입만 지원됩니다!", MessageType.Error);
                EditorGUI.EndProperty();
                return;
            }

            PropertyData data = GetPropertyData(property);
            position.width -= 60;
            EditorGUI.LabelField(position, label);

            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;

            Rect propertyPosition = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (string.IsNullOrEmpty(property.stringValue) || data.SelectedSkinData == null)
            {
                DrawBlock(propertyPosition, "스킨이 선택되지 않았습니다!", null, property);
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.BeginChangeCheck();
            DrawBlock(propertyPosition, property.stringValue, data.PreviewObject, property);
            if (EditorGUI.EndChangeCheck())
            {
                data.UpdateValues();
            }

            EditorGUI.EndProperty();
        }

        private void DrawBlock(Rect propertyPosition, string idText, Object previewObject, SerializedProperty property)
        {
            float defaultY = propertyPosition.y;
            EditorGUI.LabelField(propertyPosition, "선택된 스킨:");

            using (new EditorGUI.DisabledScope(true))
            {
                propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.LabelField(propertyPosition, idText);
                propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;
            }

            Rect boxRect = new(propertyPosition.x + propertyPosition.width + 2, defaultY, 58, 58);
            GUI.Box(boxRect, GUIContent.none);

            Texture2D preview = AssetPreview.GetAssetPreview(previewObject);
            GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), preview ?? EditorCustomStyles.GetMissingIcon());

            if (GUI.Button(new Rect(propertyPosition.x + propertyPosition.width + 5, defaultY + 40, 53, 16), new GUIContent("선택")))
            {
                if (attribute is SkinPickerAttribute skinPickerAttribute)
                {
                    SkinPickerWindow.PickSkin(property, skinPickerAttribute.DatabaseType);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Clamp(EditorGUIUtility.singleLineHeight * 3 + 2, 58, float.MaxValue);
        }

        private PropertyData GetPropertyData(SerializedProperty property)
        {
            int hash = property.stringValue.GetHashCode();
            if (!propertiesData.TryGetValue(hash, out var propertyData))
            {
                propertyData = new PropertyData(property);
                propertiesData.Add(hash, propertyData);
            }
            return propertyData;
        }

        public void Dispose() => propertiesData.Clear();

        // 선택된 스킨 데이터와 미리보기 오브젝트를 저장하는 내부 클래스
        private class PropertyData
        {
            public string ID { get; private set; }
            public ISkinData SelectedSkinData { get; private set; }
            public Object PreviewObject { get; private set; }

            public PropertyData(SerializedProperty property)
            {
                ID = property.stringValue;
                UpdateValues();
            }

            public void UpdateValues()
            {
                SelectedSkinData = null;
                foreach (var provider in EditorSkinsProvider.SkinsDatabases)
                {
                    SelectedSkinData = provider.GetSkinData(ID);
                    if (SelectedSkinData != null) break;
                }

                if (SelectedSkinData != null)
                {
                    FieldInfo previewField = SelectedSkinData.GetType()
                        .GetFields(ReflectionUtils.FLAGS_INSTANCE)
                        .FirstOrDefault(x => x.GetCustomAttribute<SkinPreviewAttribute>() != null);

                    PreviewObject = previewField?.GetValue(SelectedSkinData) as Object;
                }
            }
        }
    }
}
