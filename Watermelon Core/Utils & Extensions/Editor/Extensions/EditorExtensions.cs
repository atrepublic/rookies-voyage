// 스크립트 설명: Unity 에디터 작업을 돕기 위한 확장 메서드들을 모아 놓은 정적 클래스입니다.
// 주로 SerializedProperty와 관련된 배열 조작, 객체 정보 가져오기, 프로퍼티 초기화 등의 기능을 제공합니다.
using System; // Type, StringSplitOptions 사용을 위한 네임스페이스
using System.Collections.Generic; // IEnumerable, List 사용을 위한 네임스페이스
using System.IO; // Path, File 사용을 위한 네임스페이스
using System.Linq; // LINQ 확장 메서드 사용을 위한 네임스페이스
using System.Reflection; // FieldInfo 사용을 위한 네임스페이스
using System.Text.RegularExpressions; // 정규식 사용을 위한 네임스페이스 (현재 코드에서는 사용되지 않음)
using UnityEditor; // Unity 에디터 기능 사용을 위한 네임스페이스
using UnityEngine; // Debug, GameObject, Object, Color, Vector2, Vector3, Vector4, Quaternion, Rect, Bounds, AnimationCurve, Gradient, Hash128, Vector2Int, Vector3Int, RectInt, BoundsInt 사용을 위한 네임스페이스
using Object = UnityEngine.Object; // UnityEngine.Object 사용 명시

namespace Watermelon
{
    // Unity 에디터 작업을 위한 확장 메서드를 제공하는 정적 클래스
    public static class EditorExtensions
    {
        /// <summary>
        /// 코드를 통해 ScriptableObject 타입의 애셋을 생성하거나 이미 존재하는 애셋을 가져옵니다.
        /// </summary>
        /// <typeparam name="T">생성하거나 가져올 ScriptableObject의 타입.</typeparam>
        /// <param name="type">생성할 ScriptableObject의 실제 Type 객체.</param>
        /// <param name="fullPath">애셋이 생성될 프로젝트 내의 전체 경로 (확장자 제외).</param>
        /// <returns>생성되거나 가져온 ScriptableObject 애셋.</returns>
        // Create SciptableObject from code - 원본 주석 번역
        public static T CreateItem<T>(Type type, string fullPath) where T : ScriptableObject
        {
            // 지정된 Type의 ScriptableObject 인스턴스 생성
            T item = (T)ScriptableObject.CreateInstance(type);

            // 애셋 저장 경로 설정 (경로 + ".asset" 확장자)
            string objectPath = fullPath + ".asset";

            // 이미 해당 경로에 같은 타입의 애셋이 존재하는지 확인
            if (AssetDatabase.LoadAssetAtPath<T>(objectPath) != null)
            {
                // 존재하면 해당 애셋을 로드하여 반환
                return AssetDatabase.LoadAssetAtPath<T>(objectPath);
            }

            // 존재하지 않으면 지정된 경로에 애셋 생성
            AssetDatabase.CreateAsset(item, objectPath);

            // 애셋 데이터 저장 및 에디터 새로고침
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 새로 생성된 애셋 반환
            return item;
        }

        /// <summary>
        /// SerializedProperty가 나타내는 배열에 Unity Object 타입의 요소를 추가합니다.
        /// </summary>
        /// <typeparam name="T">추가할 Unity Object 타입 (ScriptableObject 또는 MonoBehaviour 등).</typeparam>
        /// <param name="arrayProperty">요소를 추가할 SerializedProperty (배열이어야 함).</param>
        /// <param name="elementToAdd">배열에 추가할 Unity Object 요소.</param>
        // Add element to SerializedProperty array - 원본 주석 번역
        public static void AddToObjectArray<T>(this SerializedProperty arrayProperty, T elementToAdd) where T : Object
        {
            // 이 메서드가 배열이 아닌 SerializedProperty에 대해 호출되면 예외 발생
            // If the SerializedProperty this is being called from is not an array, throw an exception. - 원본 주석 번역
            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 는 배열이 아닙니다."); // 한글 예외 메시지

            // serializedObject의 최신 상태를 가져옵니다.
            // Pull all the information from the target of the serializedObject. - 원본 주석 번역
            arrayProperty.serializedObject.Update();

            // 배열의 끝에 null 요소를 삽입하고, 그 위치에 추가할 오브젝트를 할당합니다.
            // Add a null array element to the end of the array then populate it with the object parameter. - 원본 주석 번역
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).objectReferenceValue = elementToAdd;

            // serializedObject의 변경 사항을 타겟(실제 오브젝트)에 적용합니다.
            // Push all the information on the serializedObject back to the target. - 원본 주석 번역
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// SerializedProperty가 나타내는 배열에서 지정된 인덱스의 요소를 제거합니다.
        /// 제거되는 요소가 ObjectReference인 경우, 참조된 에셋이 프로젝트에서 삭제됩니다.
        /// </summary>
        /// <param name="arrayProperty">요소를 제거할 SerializedProperty (배열이어야 함).</param>
        /// <param name="index">제거할 요소의 인덱스.</param>
        /// <returns>제거 성공 시 true, 실패 시 false (대화 상자에서 취소 등).</returns>
        // Remove element from SerializedProperty array - 원본 주석 번역 (함수 오버로드로 인해 제거 시 애셋 삭제 기능 설명 추가)
        public static bool RemoveObject(this SerializedProperty property, int index, string title = "이 오브젝트가 제거됩니다!", string content = "정말 삭제하시겠습니까?") // 한글 기본값 설정
        {
            // 사용자에게 삭제 확인 대화 상자 표시
            if (EditorUtility.DisplayDialog(title, content, "제거", "취소")) // 한글 버튼 텍스트
            {
                if (property.isArray) // SerializedProperty가 배열인지 확인
                {
                    // 제거될 오브젝트의 애셋 경로 가져오기
                    string assetPath = AssetDatabase.GetAssetPath(property.GetArrayElementAtIndex(index).objectReferenceValue);

                    // 배열에서 해당 인덱스의 요소를 제거 (아래 정의된 확장 메서드 사용)
                    property.RemoveFromObjectArrayAt(index);

                    // 애셋 경로가 유효하고 해당 파일이 존재하면 프로젝트에서 애셋 삭제
                    // EditorUtils.projectFolderPath는 Watermelon 네임스페이스의 다른 곳에 정의된 것으로 가정
                    if (!string.IsNullOrEmpty(assetPath) && File.Exists(EditorUtils.projectFolderPath + "/" + assetPath)) // 경로 유효성 검사 추가, 파일 존재 확인
                    {
                        AssetDatabase.DeleteAsset(assetPath); // 애셋 삭제
                    }

                    return true; // 제거 성공 반환
                }
            }

            return false; // 제거 실패 (대화 상자 취소 또는 배열이 아님) 반환
        }


        /// <summary>
        /// SerializedProperty가 나타내는 Object 배열에서 지정된 인덱스의 요소를 제거합니다.
        /// 이 메서드는 참조된 오브젝트를 프로젝트에서 삭제하지 않고 배열에서만 제거합니다.
        /// </summary>
        /// <param name="arrayProperty">요소를 제거할 SerializedProperty (배열이어야 함).</param>
        /// <param name="index">제거할 요소의 인덱스.</param>
        // Remove element from SerializedProperty array - 원본 주석 번역 (함수 오버로드로 인해 참조 에셋 삭제 안 함 명시)
        public static void RemoveFromObjectArrayAt(this SerializedProperty arrayProperty, int index)
        {
            // 인덱스가 유효하지 않거나 SerializedProperty가 배열이 아니면 예외 발생
            // If the index is not appropriate or the serializedProperty this is being called from is not an array, throw an exception. - 원본 주석 번역
            if (index < 0)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 에서 음수 인덱스 요소를 제거할 수 없습니다."); // 한글 예외 메시지

            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 는 배열이 아닙니다."); // 한글 예외 메시지

            // 인덱스가 배열 범위를 벗어나면 예외 발생
            if (index > arrayProperty.arraySize - 1)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 에는 " + arrayProperty.arraySize + " 개의 요소만 있으므로 인덱스 " + index + " 의 요소를 제거할 수 없습니다."); // 한글 예외 메시지

            // serializedObject의 최신 상태를 가져옵니다.
            // Pull all the information from the target of the serializedObject. - 원본 주석 번역
            arrayProperty.serializedObject.Update();

            // 해당 인덱스의 요소가 null이 아니면 null로 설정합니다.
            // If there is a non-null element at the index, null it. - 원본 주석 번역
            // DeleteArrayElementAtIndex는 null이 아닌 요소를 만나면 오류가 발생할 수 있으므로 이 단계가 필요합니다.
            if (arrayProperty.GetArrayElementAtIndex(index).objectReferenceValue != null)
                arrayProperty.DeleteArrayElementAtIndex(index);

            // 해당 인덱스의 null 요소를 배열에서 삭제합니다.
            // Delete the null element from the array at the index. - 원본 주석 번역
            arrayProperty.DeleteArrayElementAtIndex(index);

            // serializedObject의 변경 사항을 타겟(실제 오브젝트)에 적용합니다.
            // Push all the information on the serializedObject back to the target. - 원본 주석 번역
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// SerializedProperty가 나타내는 Object 배열에서 지정된 오브젝트 요소를 제거합니다.
        /// 이 메서드는 참조된 오브젝트를 프로젝트에서 삭제하지 않고 배열에서만 제거합니다.
        /// </summary>
        /// <typeparam name="T">제거할 오브젝트의 타입.</typeparam>
        /// <param name="arrayProperty">요소를 제거할 SerializedProperty (배열이어야 함).</param>
        /// <param name="elementToRemove">배열에서 제거할 Unity Object 요소.</param>
        // Use this to remove an object from an object array represented by a SerializedProperty. - 원본 주석 번역
        /// <typeparam name="T">Type of object to be removed. - 원본 주석 번역</typeparam>
        /// <param name="arrayProperty">Property that contains array. - 원본 주석 번역</param>
        /// <param name="elementToRemove">Element to be removed. - 원본 주석 번역</param>
        public static void RemoveFromObjectArray<T>(this SerializedProperty arrayProperty, T elementToRemove) where T : Object
        {
            // SerializedProperty가 배열이 아니거나 제거할 요소가 null이면 예외 발생
            // If either the serializedProperty doesn't represent an array or the element is null, throw an exception. - 원본 주석 번역
            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 는 배열이 아닙니다."); // 한글 예외 메시지

            if (!elementToRemove)
                throw new UnityException("null 요소 제거는 이 메서드에서 지원되지 않습니다."); // 한글 예외 메시지

            // serializedObject의 최신 상태를 가져옵니다.
            // Pull all the information from the target of the serializedObject. - 원본 주석 번역
            arrayProperty.serializedObject.Update();

            // SerializedProperty의 배열에 있는 모든 요소를 순회하며...
            // Go through all the elements in the serializedProperty's array... - 원본 주석 번역
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = arrayProperty.GetArrayElementAtIndex(i);

                // ...매개변수로 전달된 요소와 일치하는 요소를 찾을 때까지...
                // ... until the element matches the parameter... - 원본 주석 번역
                if (elementProperty.objectReferenceValue == elementToRemove)
                {
                    // ...그 요소를 제거합니다.
                    // ... then remove it. - 원본 주석 번역
                    arrayProperty.RemoveFromObjectArrayAt(i); // 해당 인덱스의 요소 제거 (위에서 정의된 확장 메서드 사용)
                    return; // 제거 후 함수 종료
                }
            }

            // 배열에서 제거하려는 요소를 찾지 못했을 경우 예외 발생
            throw new UnityException("요소 " + elementToRemove.name + " 은(는) 프로퍼티 " + arrayProperty.name + " 에서 찾을 수 없습니다."); // 한글 예외 메시지
        }

        /// <summary>
        /// SerializedProperty가 나타내는 일반(Object가 아닌) 변수 배열에서 지정된 인덱스의 요소를 제거합니다.
        /// </summary>
        /// <param name="arrayProperty">요소를 제거할 SerializedProperty (배열이어야 함).</param>
        /// <param name="index">제거할 요소의 인덱스.</param>
        // Use this to remove the object at an index from an object array represented by a SerializedProperty. - 원본 주석 번역 (Object 배열이 아닌 일반 변수 배열에 적용됨)
        public static void RemoveFromVariableArrayAt(this SerializedProperty arrayProperty, int index)
        {
            // 인덱스가 유효하지 않거나 SerializedProperty가 배열이 아니면 예외 발생
            // If the index is not appropriate or the serializedProperty this is being called from is not an array, throw an exception. - 원본 주석 번역
            if (index < 0)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 에서 음수 인덱스 요소를 제거할 수 없습니다."); // 한글 예외 메시지

            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 는 배열이 아닙니다."); // 한글 예외 메시지

            // 인덱스가 배열 범위를 벗어나면 예외 발생
            if (index > arrayProperty.arraySize - 1)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " 에는 " + arrayProperty.arraySize + " 개의 요소만 있으므로 인덱스 " + index + " 의 요소를 제거할 수 없습니다."); // 한글 예외 메시지

            // serializedObject의 최신 상태를 가져옵니다.
            // Pull all the information from the target of the serializedObject. - 원본 주석 번역
            arrayProperty.serializedObject.Update();

            // 해당 인덱스의 요소를 배열에서 삭제합니다.
            // Delete the null element from the array at the index. - 원본 주석 번역 (Object가 아닌 일반 변수 요소에 대해 삭제)
            arrayProperty.DeleteArrayElementAtIndex(index);

            // serializedObject의 변경 사항을 타겟(실제 오브젝트)에 적용합니다.
            // Push all the information on the serializedObject back to the target. - 원본 주석 번역
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// SerializedProperty에서 해당 프로퍼티가 참조하는 실제 객체를 가져옵니다.
        /// (주의: 이 메서드는 복잡한 구조나 특정 타입의 프로퍼티에서는 예상대로 작동하지 않을 수 있습니다.)
        /// </summary>
        /// <param name="property">객체를 가져올 SerializedProperty.</param>
        /// <returns>프로퍼티가 참조하는 실제 객체.</returns>
        // Get object from serializedProperty - 원본 주석 번역
        public static object GetPropertyObject(SerializedProperty property)
        {
            // 프로퍼티 경로를 '.' 기준으로 분할
            string[] path = property.propertyPath.Split('.');
            // SerializedObject의 타겟 오브젝트 (가장 상위 객체) 가져오기
            object baseObject = property.serializedObject.targetObject;
            // 타겟 오브젝트의 타입 가져오기
            Type baseType = baseObject.GetType();

            // 프로퍼티 경로의 각 부분을 따라가며 실제 객체에 접근
            for (int i = 0; i < path.Length; i++)
            {
                // 현재 타입에서 필드 정보 가져오기 (public 또는 non-public, 인스턴스 필드 검색)
                FieldInfo fieldInfo = baseType.GetField(path[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                // 다음 단계의 타입은 현재 필드의 타입
                baseType = fieldInfo.FieldType;

                // 현재 객체에서 필드의 실제 값 가져오기 (다음 단계의 객체)
                baseObject = fieldInfo.GetValue(baseObject);
            }

            return baseObject; // 최종적으로 얻은 실제 객체 반환
        }

        /// <summary>
        /// SerializedProperty의 전체 경로 중 배열 인덱스 부분을 제외한 기본 경로를 가져옵니다.
        /// </summary>
        /// <param name="property">경로를 가져올 SerializedProperty.</param>
        /// <returns>배열 인덱스 부분이 제거된 프로퍼티 기본 경로.</returns>
        // Get full path to serializedProperty - 원본 주석 번역
        public static string GetPropertyPath(this SerializedProperty property)
        {
            // 프로퍼티 경로를 ".Array" 기준으로 분할하고 첫 번째 부분을 반환
            return property.propertyPath.Split(new string[] { ".Array" }, StringSplitOptions.None)[0]; // StringSplitOptions.None 사용 명시
        }

        /// <summary>
        /// SerializedProperty가 배열 요소인 경우, 해당 요소의 인덱스를 가져옵니다.
        /// </summary>
        /// <param name="property">인덱스를 가져올 SerializedProperty.</param>
        /// <returns>배열 요소의 인덱스.</returns>
        // Get serializedProperty id from path - 원본 주석 번역
        public static int GetPropertyArrayIndex(this SerializedProperty property)
        {
            // 프로퍼티 경로에서 마지막 '[' 문자의 인덱스 찾기
            int index1 = property.propertyPath.LastIndexOf('[');
            // 프로퍼티 경로에서 마지막 ']' 문자의 인덱스 찾기
            int index2 = property.propertyPath.LastIndexOf(']');
            // '['와 ']' 사이의 문자열(인덱스 숫자)을 파싱하여 정수로 변환하여 반환
            return int.Parse(property.propertyPath.Substring(index1 + 1, index2 - index1 - 1));
        }

        /// <summary>
        /// SerializedProperty가 나타내는 배열에 ScriptableObject 타입의 요소를 추가합니다.
        /// AddToObjectArray와 유사하지만, ScriptableObject에 특화되어 있습니다.
        /// </summary>
        /// <typeparam name="T">추가할 ScriptableObject의 타입.</typeparam>
        /// <param name="property">요소를 추가할 SerializedProperty (배열이어야 함).</param>
        /// <param name="addedObject">배열에 추가할 ScriptableObject 요소.</param>
        /// <returns>추가 성공 시 true, 실패 시 false.</returns>
        public static bool AddObject<T>(this SerializedProperty property, T addedObject) where T : ScriptableObject
        {
            if (property.isArray) // SerializedProperty가 배열인지 확인
            {
                if (addedObject != null) // 추가할 오브젝트가 null이 아닌지 확인
                {
                    property.serializedObject.Update(); // serializedObject의 최신 상태를 가져옴

                    int index = property.arraySize; // 현재 배열의 마지막 인덱스 (새 요소가 추가될 위치)

                    property.arraySize++; // 배열 크기 1 증가
                    property.GetArrayElementAtIndex(index).objectReferenceValue = addedObject; // 새 인덱스에 오브젝트 할당

                    property.serializedObject.ApplyModifiedProperties(); // 변경된 프로퍼티를 적용

                    return true; // 추가 성공 반환
                }
            }

            return false; // 추가 실패 반환 (배열이 아니거나 추가할 오브젝트가 null)
        }

        /// <summary>
        /// SerializedProperty가 참조하는 Unity Object 애셋을 선택하고 프로젝트 창에서 하이라이트합니다.
        /// </summary>
        /// <param name="property">선택할 오브젝트를 참조하는 SerializedProperty.</param>
        public static void SelectSourceObject(this SerializedProperty property)
        {
            // 프로퍼티가 유효한 오브젝트를 참조하고 있다면
            if (property.objectReferenceValue != null)
            {
                EditorUtility.FocusProjectWindow(); // Unity 프로젝트 창에 포커스
                EditorGUIUtility.PingObject(property.objectReferenceValue); // 참조된 오브젝트를 프로젝트 창에서 하이라이트
            }
        }

        /// <summary>
        /// SerializedProperty 및 그 자식 프로퍼티들의 값을 기본값으로 초기화합니다.
        /// 각 프로퍼티 타입에 맞는 기본값을 할당합니다.
        /// </summary>
        /// <param name="property">값을 초기화할 SerializedProperty.</param>
        public static void ClearProperty(this SerializedProperty property)
        {
            // 프로퍼티를 복사하여 반복에 사용
            SerializedProperty iterator = property.Copy();

            // 현재 프로퍼티 및 그 자식 프로퍼티들을 순회
            // NextVisible(true): 자식까지 포함하여 다음 보이는 프로퍼티로 이동
            // iterator.propertyPath.Contains(property.propertyPath): 순회 중인 프로퍼티가 시작 프로퍼티의 자식인지 확인
            while (iterator.NextVisible(true) && iterator.propertyPath.Contains(property.propertyPath))
            {
                ClearValueProperty(iterator); // 각 프로퍼티의 값을 초기화
            }
        }

        /// <summary>
        /// 주어진 SerializedProperty의 값을 해당 타입의 기본값으로 초기화합니다.
        /// </summary>
        /// <param name="property">값을 초기화할 SerializedProperty.</param>
        private static void ClearValueProperty(SerializedProperty property)
        {
            // 프로퍼티 타입에 따라 적절한 기본값 할당
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    // 일반 타입은 여기에서 직접 초기화하기 어려우므로 건너뜦니다.
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = 0; // 정수형은 0으로 초기화
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = false; // 불리언은 false로 초기화
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = 0; // 부동 소수점형은 0으로 초기화
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = string.Empty; // 문자열은 빈 문자열로 초기화
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = Color.white; // Color는 흰색으로 초기화
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = null; // 오브젝트 참조는 null로 초기화
                    break;
                case SerializedPropertyType.LayerMask:
                    // LayerMask는 특별한 초기화 로직이 필요할 수 있습니다. (여기서는 기본값 유지)
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = 0; // 열거형은 첫 번째 값(인덱스 0)으로 초기화
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2.zero; // Vector2는 Vector2.zero로 초기화
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3.zero; // Vector3는 Vector3.zero로 초기화
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = Vector4.zero; // Vector4는 Vector4.zero로 초기화
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = Rect.zero; // Rect는 Rect.zero로 초기화
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = 0; // 배열 크기는 0으로 초기화
                    break;
                case SerializedPropertyType.Character:
                    // Character 타입은 일반적으로 사용되지 않거나 초기화 로직이 다를 수 있습니다.
                    break;
                case SerializedPropertyType.AnimationCurve:
                    // AnimationCurve는 기본 커브로 초기화 (0부터 0까지 값 0 유지)
                    property.animationCurveValue = AnimationCurve.Constant(0, 0, 0);
                    break;
                case SerializedPropertyType.Bounds:
                    // Bounds는 중심 (0,0,0) 및 크기 (0,0,0)로 초기화
                    property.boundsValue = new Bounds(Vector3.zero, Vector3.zero);
                    break;
                case SerializedPropertyType.Gradient:
                    // Gradient는 새로운 빈 Gradient로 초기화
                    property.gradientValue = new Gradient();
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = Quaternion.identity; // Quaternion은 Quaternion.identity로 초기화
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = null; // ExposedReference는 null로 초기화
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    // FixedBufferSize는 여기에서 직접 초기화하기 어렵습니다.
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = Vector2Int.zero; // Vector2Int는 Vector2Int.zero로 초기화
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = Vector3Int.zero; // Vector3Int는 Vector3Int.zero로 초기화
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = new RectInt(); // RectInt는 새로운 빈 RectInt로 초기화
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = new BoundsInt(); // BoundsInt는 새로운 빈 BoundsInt로 초기화
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = null; // ManagedReference는 null로 초기화
                    break;
                case SerializedPropertyType.Hash128:
                    property.hash128Value = new Hash128(); // Hash128은 새로운 빈 Hash128로 초기화
                    break;
            }
        }

        /// <summary>
        /// SerializedProperty의 모든 자식 프로퍼티들을 열거(enumerate)합니다.
        /// </summary>
        /// <param name="property">자식 프로퍼티를 가져올 SerializedProperty.</param>
        /// <returns>자식 프로퍼티들을 포함하는 IEnumerable.</returns>
        // Get children from serializedProperty - 원본 주석 번역
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            // 시작 프로퍼티를 복사하여 반복에 사용
            property = property.Copy();
            // 다음 형제 요소를 찾기 위한 복사본
            var nextElement = property.Copy();
            // 다음 형제 요소가 있는지 확인 (자식은 포함하지 않음)
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null; // 다음 형제가 없으면 null로 설정
            }

            // 시작 프로퍼티의 첫 번째 자식으로 이동
            property.NextVisible(true);
            while (true)
            {
                // 현재 프로퍼티가 다음 형제 요소와 같다면 (즉, 자식 순회가 끝났다면) 순회 중지
                if ((SerializedProperty.EqualContents(property, nextElement)))
                {
                    yield break; // 순회 종료
                }

                yield return property; // 현재 자식 프로퍼티 반환

                // 다음 보이는 프로퍼티로 이동 (자식은 포함하지 않음, 동일 레벨)
                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break; // 더 이상 보이는 프로퍼티가 없으면 순회 중지
                }
            }
        }
    }
}