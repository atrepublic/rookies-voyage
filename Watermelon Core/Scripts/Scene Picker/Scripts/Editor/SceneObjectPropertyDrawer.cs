// 스크립트 설명: SceneObject 클래스를 Unity 에디터 인스펙터에서 사용자 정의하여 표시하는 PropertyDrawer입니다.
// 씬 애셋 링크 상태, 빌드 설정 포함 여부 등을 검사하고 관련 문제를 해결할 수 있는 버튼을 제공합니다.
using System.Collections; // 컬렉션 사용을 위한 네임스페이스 (List)
using System.Collections.Generic; // 제네릭 컬렉션 사용을 위한 네임스페이스 (List)
using UnityEngine; // GameObject, Object, Rect, MessageType, Vector2, GUIStyle 사용을 위한 네임스페이스
using UnityEditor; // EditorGUI, EditorGUIUtility, PropertyDrawer, CustomPropertyDrawer, SerializedProperty, EditorBuildSettings, AssetDatabase, EditorCustomStyles, GUIContent, EditorUtility, Selection 사용을 위한 네임스페이스
using Object = UnityEngine.Object; // UnityEngine.Object 사용 명시

namespace Watermelon
{
    // SceneObject 타입을 위한 커스텀 프로퍼티 드로어로 지정
    [CustomPropertyDrawer(typeof(SceneObject))]
    public class SceneObjectPropertyDrawer : UnityEditor.PropertyDrawer
    {
        // 씬 유효성 검사 결과 오류 코드 상수 정의
        private const int ERROR_NULL = 0; // 씬 파일이 링크되지 않음
        private const int ERROR_UNACTIVE = 1; // 씬이 빌드 설정에 추가되었지만 비활성화됨
        private const int ERROR_BUILD_MISSING = 2; // 씬이 빌드 설정에 추가되지 않음

        // 오류 코드에 해당하는 한글 메시지 배열
        private readonly string[] ERROR_MESSAGES = new string[]
        {
            "씬 파일이 연결되어야 합니다!", // 오류 코드 0
            "씬이 빌드 설정에 추가되었지만 비활성화 상태입니다!", // 오류 코드 1
            "씬이 빌드 설정에 추가되지 않았습니다!" // 오류 코드 2
        };

        /// <summary>
        /// 인스펙터에서 해당 프로퍼티를 그리는 데 사용됩니다.
        /// 씬 링크 상태를 검사하고, 문제가 있을 경우 경고 메시지와 해결 버튼을 표시합니다.
        /// </summary>
        /// <param name="position">프로퍼티가 그려질 위치와 크기.</param>
        /// <param name="property">그릴 SerializedProperty (SceneObject 타입).</param>
        /// <param name="label">프로퍼티 레이블.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // GUI 변경 시작 (Undo/Redo 지원)
            EditorGUI.BeginProperty(position, label, property);

            // SceneObject 내의 "scene" 프로퍼티를 찾음
            SerializedProperty sceneObjectProperty = property.FindPropertyRelative("scene");

            // 프로퍼티를 그릴 영역 설정 (기본 한 줄 높이)
            Rect propertyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // 씬 유효성 검사 수행
            int errorID = ValidateScene(sceneObjectProperty.objectReferenceValue);
            if (errorID != -1) // 오류가 있을 경우 (-1이 아니면)
            {
                // 경고 메시지를 표시할 영역 설정
                Rect helpBoxRect = new Rect(position.x + 14, position.y + 2, position.width - 14, 24);

                // 경고 메시지 표시 (해당 오류 ID의 메시지 사용)
                EditorGUI.HelpBox(helpBoxRect, ERROR_MESSAGES[errorID], MessageType.Warning);

                // 오류 타입에 따라 해결 버튼 표시
                if(errorID ==  ERROR_UNACTIVE) // 씬이 비활성화된 경우
                {
                    // "활성화" 버튼 표시 및 클릭 시 ActivateScene 호출
                    DrawHelpButton(helpBoxRect, "활성화", () =>
                    {
                        ActivateScene(sceneObjectProperty.objectReferenceValue);
                    });
                }
                else if (errorID == ERROR_BUILD_MISSING) // 씬이 빌드 설정에 없는 경우
                {
                    // "빌드에 추가" 버튼 표시 및 클릭 시 AddNewScene 호출
                    DrawHelpButton(helpBoxRect, "빌드에 추가", () =>
                    {
                        AddNewScene(sceneObjectProperty.objectReferenceValue);
                    });
                }

                // 다음 그릴 요소의 Y 위치 조정 (경고 메시지 높이만큼 아래로 이동)
                propertyRect.y += helpBoxRect.height + 4;
            }

            // 실제 씬 애셋을 선택하고 드래그앤드롭할 수 있는 ObjectField 그리기
            sceneObjectProperty.objectReferenceValue = EditorGUI.ObjectField(propertyRect, "씬", sceneObjectProperty.objectReferenceValue, typeof(SceneAsset), false) as SceneAsset; // 한글 레이블 사용

            // GUI 변경 종료
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 경고/정보 메시지 옆에 해결 버튼을 그립니다.
        /// </summary>
        /// <param name="rect">버튼이 그려질 영역 (일반적으로 HelpBox 영역).</param>
        /// <param name="name">버튼에 표시될 텍스트.</param>
        /// <param name="clickCallback">버튼 클릭 시 실행될 콜백 함수.</param>
        private void DrawHelpButton(Rect rect, string name, SimpleCallback clickCallback) // SimpleCallback은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
        {
            // 버튼 스타일 설정 (기본 에디터 버튼 스타일 사용 및 폰트 크기/패딩 조정)
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton); // EditorCustomStyles.button 대신 EditorStyles.miniButton 사용
            buttonStyle.fontSize = 10;
            buttonStyle.padding = new RectOffset(4, 4, 3, 3);

            // 버튼에 표시될 레이블 콘텐츠 생성
            GUIContent label = new GUIContent(name);

            // 버튼의 계산된 크기 가져오기
            Vector2 buttonSize = buttonStyle.CalcSize(label);

            // 버튼을 HelpBox 세로 중앙에 배치하기 위한 Y 오프셋 계산
            float yOffset = (rect.height - buttonSize.y) / 2;

            // 버튼이 그려질 최종 위치와 크기 설정
            Rect buttonRect = new Rect(rect.x + rect.width - buttonSize.x - 8, rect.y + yOffset, buttonSize.x, buttonSize.y);

            // 버튼 그리기 및 클릭 감지
            if(GUI.Button(buttonRect, label, buttonStyle))
            {
                clickCallback?.Invoke(); // 콜백 함수 실행 (null 조건부 연산자 사용)
            }
        }

        /// <summary>
        /// 인스펙터에서 해당 프로퍼티가 차지할 세로 높이를 계산하여 반환합니다.
        /// 씬 유효성 검사 경고가 표시될 경우 추가 높이를 계산합니다.
        /// </summary>
        /// <param name="property">높이를 계산할 SerializedProperty (SceneObject 타입).</param>
        /// <param name="label">프로퍼티 레이블.</param>
        /// <returns>프로퍼티가 차지할 총 세로 높이.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 기본 높이는 한 줄 높이 + 간격
            float height = EditorGUIUtility.singleLineHeight + 2;

            // SceneObject 내의 "scene" 프로퍼티를 찾음
            SerializedProperty sceneObjectProperty = property.FindPropertyRelative("scene");

            // 씬 유효성 검사를 수행하고 오류가 있을 경우
            if (ValidateScene(sceneObjectProperty.objectReferenceValue) != -1)
            {
                // 기본 높이에 경고 메시지 높이(24)와 간격(4)을 추가
                height += 24 + 4;
            }

            return height; // 계산된 총 높이 반환
        }

        /// <summary>
        /// 지정된 씬 애셋을 Unity 빌드 설정의 씬 목록에서 활성화 상태로 변경합니다.
        /// </summary>
        /// <param name="sceneObject">활성화할 씬 애셋.</param>
        private void ActivateScene(Object sceneObject)
        {
            if (sceneObject == null) return; // 씬 오브젝트가 null이면 처리 중지

            // 씬 애셋의 프로젝트 내 경로 가져오기
            string scenePath = AssetDatabase.GetAssetPath(sceneObject);

            // 현재 빌드 설정에 있는 씬 목록을 가져와 리스트로 변환
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var scene in editorBuildSettingsScenes)
            {
                // 씬 경로가 동일한 씬이 목록에 있는지 확인
                if (scene.path == scenePath)
                {
                    scene.enabled = true; // 해당 씬을 활성화 상태로 변경

                    // 변경된 씬 목록을 빌드 설정에 다시 적용
                    EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

                    break; // 처리 완료 후 반복 중단
                }
            }
        }

        /// <summary>
        /// 지정된 씬 애셋을 Unity 빌드 설정의 씬 목록에 새로 추가하고 활성화합니다.
        /// 이미 목록에 있는 경우 중복 추가하지 않습니다.
        /// </summary>
        /// <param name="sceneObject">추가할 씬 애셋.</param>
        private void AddNewScene(Object sceneObject)
        {
            if (sceneObject == null) return; // 씬 오브젝트가 null이면 처리 중지

            // 씬 애셋의 프로젝트 내 경로 가져오기
            string scenePath = AssetDatabase.GetAssetPath(sceneObject);

            // 현재 빌드 설정에 있는 씬 목록을 가져와 리스트로 변환
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(var scene in editorBuildSettingsScenes)
            {
                // 이미 목록에 추가되어 있는지 확인
                if (scene.path == scenePath) return; // 이미 있으면 중복 추가하지 않고 함수 종료
            }

            // 빌드 설정 씬 목록에 새로운 EditorBuildSettingsScene 객체를 생성하여 추가 (경로, 활성화 상태 true)
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));

            // 변경된 씬 목록을 빌드 설정에 다시 적용
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }

        /// <summary>
        /// 지정된 씬 애셋의 유효성을 검사합니다.
        /// 씬이 null인지, 빌드 설정에 포함되었는지, 활성화되었는지 등을 확인합니다.
        /// </summary>
        /// <param name="sceneObject">유효성을 검사할 씬 애셋.</param>
        /// <returns>
        /// 오류 코드 (ERROR_NULL, ERROR_UNACTIVE, ERROR_BUILD_MISSING)를 반환합니다.
        /// 유효성 검사를 통과했으면 -1을 반환합니다.
        /// </returns>
        private int ValidateScene(Object sceneObject)
        {
            if (sceneObject == null) // 씬 애셋 참조가 null이면
            {
                return ERROR_NULL; // null 오류 코드 반환
            }

            bool isSceneAddedToBuildList = false; // 씬이 빌드 설정에 추가되었는지 여부

            // 씬 애셋의 프로젝트 내 경로 가져오기
            string scenePath = AssetDatabase.GetAssetPath(sceneObject);
            // 빌드 설정에 있는 모든 씬 목록을 순회하며
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == scenePath) // 씬 경로가 동일한 씬을 찾으면
                {
                    isSceneAddedToBuildList = true; // 빌드 설정에 추가된 것으로 표시

                    if (!scene.enabled) // 씬이 비활성화 상태이면
                    {
                        return ERROR_UNACTIVE; // 비활성 오류 코드 반환
                    }
                }
            }

            if (!isSceneAddedToBuildList) // 순회 후에도 빌드 설정에 추가되지 않았다면
            {
                return ERROR_BUILD_MISSING; // 빌드 설정 누락 오류 코드 반환
            }

            return -1; // 모든 검사를 통과했으면 -1 반환 (오류 없음)
        }
    }
}