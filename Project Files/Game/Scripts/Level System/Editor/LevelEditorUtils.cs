// 이 스크립트는 Unity 에디터 확장에서 사용되는 유틸리티 함수를 제공합니다.
// 주로 SerializedObject 및 SerializedProperty를 사용하여 오브젝트의 속성을 탐색하고 접근하는 기능을 포함합니다.
// LevelEditorSetting 커스텀 어트리뷰트를 가진 속성 또는 가지지 않은 속성을 필터링하여 가져올 수 있습니다.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class LevelEditorUtils
    {
        // LevelEditorSetting 어트리뷰트가 적용된 속성들을 가져옵니다.
        // SerializedObject를 기반으로 하여 최상위 레벨의 속성들을 찾습니다.
        // <param name="serializedObject">속성을 가져올 SerializedObject입니다.</param>
        // <returns>LevelEditorSetting 어트리뷰트가 적용된 SerializedProperty들의 열거형입니다.</returns>
        public static IEnumerable<SerializedProperty> GetLevelEditorProperies(SerializedObject serializedObject)
        {
            Type targetType = serializedObject.targetObject.GetType();

            // Reflection을 사용하여 LevelEditorSetting 어트리뷰트가 있는 필드를 찾습니다.
            IEnumerable<FieldInfo> fieldInfos = targetType.GetFields(ReflectionUtils.FLAGS_INSTANCE).Where(x => x.GetCustomAttribute<LevelEditorSetting>() != null);

            foreach (var field in fieldInfos)
            {
                // 필드 이름으로 SerializedProperty를 찾습니다.
                SerializedProperty serializedProperty = serializedObject.FindProperty(field.Name);
                if (serializedProperty != null)
                    yield return serializedProperty;
            }
        }

        // LevelEditorSetting 어트리뷰트가 적용된 자식 속성들을 가져옵니다.
        // SerializedProperty를 기반으로 하여 해당 속성의 자식 속성들을 찾습니다.
        // 제네릭 타입 (예: 클래스 또는 구조체)의 속성에 대해 작동합니다.
        // <param name="serializedProperty">자식 속성을 가져올 SerializedProperty입니다.</param>
        // <returns>LevelEditorSetting 어트리뷰트가 적용된 자식 SerializedProperty들의 열거형입니다.</returns>
        public static IEnumerable<SerializedProperty> GetLevelEditorProperies(SerializedProperty serializedProperty)
        {
            // 속성이 제네릭 타입인지 확인합니다.
            if (serializedProperty.propertyType == SerializedPropertyType.Generic)
            {
                Type targetType = serializedProperty.boxedValue.GetType();
                 // Reflection을 사용하여 LevelEditorSetting 어트리뷰트가 있는 자식 필드를 찾습니다.
                IEnumerable<FieldInfo> fieldInfos = targetType.GetFields(ReflectionUtils.FLAGS_INSTANCE).Where(x => x.GetCustomAttribute<LevelEditorSetting>() != null);
                foreach (var field in fieldInfos)
                {
                    // 자식 속성 이름으로 SerializedProperty를 찾습니다.
                    SerializedProperty subProperty = serializedProperty.FindPropertyRelative(field.Name);
                    if (subProperty != null)
                        yield return subProperty;
                }
            }
        }

        // LevelEditorSetting 어트리뷰트가 적용되지 않은 속성들을 가져옵니다.
        // SerializedObject를 기반으로 하여 최상위 레벨의 속성들을 찾습니다.
        // <param name="serializedObject">속성을 가져올 SerializedObject입니다.</param>
        // <returns>LevelEditorSetting 어트리뷰트가 적용되지 않은 SerializedProperty들의 열거형입니다.</returns>
        public static IEnumerable<SerializedProperty> GetUnmarkedProperties(SerializedObject serializedObject)
        {
            Type targetType = serializedObject.targetObject.GetType();

            // Reflection을 사용하여 LevelEditorSetting 어트리뷰트가 없는 필드를 찾습니다.
            IEnumerable<FieldInfo> fieldInfos = targetType.GetFields(ReflectionUtils.FLAGS_INSTANCE).Where(x => x.GetCustomAttribute<LevelEditorSetting>() == null);

            foreach (var field in fieldInfos)
            {
                // 필드 이름으로 SerializedProperty를 찾습니다.
                SerializedProperty serializedProperty = serializedObject.FindProperty(field.Name);
                if (serializedProperty != null)
                    yield return serializedProperty;
            }
        }

        // LevelEditorSetting 어트리뷰트가 적용되지 않은 자식 속성들을 가져옵니다.
        // SerializedProperty를 기반으로 하여 해당 속성의 자식 속성들을 찾습니다.
        // 제네릭 타입 (예: 클래스 또는 구조체)의 속성에 대해 작동합니다.
        // <param name="serializedProperty">자식 속성을 가져올 SerializedProperty입니다.</param>
        // <returns>LevelEditorSetting 어트리뷰트가 적용되지 않은 자식 SerializedProperty들의 열거형입니다.</returns>
        public static IEnumerable<SerializedProperty> GetUnmarkedProperties(SerializedProperty serializedProperty)
        {
            // 속성이 제네릭 타입인지 확인합니다.
            if (serializedProperty.propertyType == SerializedPropertyType.Generic)
            {
                Type targetType = serializedProperty.boxedValue.GetType();
                // Reflection을 사용하여 LevelEditorSetting 어트리뷰트가 없는 자식 필드를 찾습니다.
                IEnumerable<FieldInfo> fieldInfos = targetType.GetFields(ReflectionUtils.FLAGS_INSTANCE).Where(x => x.GetCustomAttribute<LevelEditorSetting>() == null);
                foreach (var field in fieldInfos)
                {
                     // 자식 속성 이름으로 SerializedProperty를 찾습니다.
                    SerializedProperty subProperty = serializedProperty.FindPropertyRelative(field.Name);
                    if (subProperty != null)
                        yield return subProperty;
                }
            }
        }

    }
}