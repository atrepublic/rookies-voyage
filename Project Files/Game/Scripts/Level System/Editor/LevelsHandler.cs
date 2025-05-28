// 이 스크립트는 Unity 에디터에서 레벨 데이터를 관리하는 핸들러입니다.
// 레벨 에셋 생성, 삭제, 이름 변경, 순서 변경 등의 기능을 제공하며, ReorderableList를 사용하여 레벨 목록을 시각적으로 표시하고 조작합니다.
// LevelEditorBase 클래스와 연동하여 특정 레벨 타입에 맞게 동작합니다.

#pragma warning disable 649

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Text;
using System;
using Watermelon.List; // Watermelon 네임스페이스의 CustomList를 사용하기 위해 추가

namespace Watermelon
{
    public class LevelsHandler
    {

        // 상수 문자열들 정의
        private const string LEVEL_PREFIX = "Level_";
        private const string ASSET_SUFFIX = ".asset";
        private const string OLD_PREFIX = "old_";
        private const string REMOVE_LEVEL = "정말로 레벨을 제거하시겠습니까: "; // 레벨 삭제 확인 메시지
        private const string BRACKET = "\"";
        private const string QUESTION_MARK = "?";
        private const string REMOVING_LEVEL_TITLE = "레벨 제거"; // 레벨 삭제 확인 창 제목
        private const string YES = "예"; // 확인 버튼 텍스트
        private const string CANCEL = "취소"; // 취소 버튼 텍스트
        private const string FORMAT_TYPE = "000"; // 레벨 번호 형식 (예: 001, 010, 100)
        private const string PATH_SEPARATOR = "/"; // 경로 구분자
        private const string DEFAULT_LEVEL_LIST_HEADER = "레벨 개수: "; // 레벨 목록 헤더 텍스트
        private const string REMOVE_SELECTION = "선택 해제"; // 선택 해제 버튼 텍스트
        private const string RENAME_LEVELS_LABEL = "레벨 이름 변경"; // 레벨 이름 변경 버튼 텍스트
        private const string GLOBAL_VALIDATION_LABEL = "전역 유효성 검사"; // 전역 유효성 검사 버튼 텍스트
        private const string REMOVE_ELEMENT_CALLBACK = "요소 제거"; // 요소 제거 콜백 텍스트
        private const string ON_ENABLE_OVERRIDEN_ERROR = "LevelEditorBase.Instance == null. OnEnable() 오버라이드 시 base.OnEnable() 호출 누락."; // OnEnable 오류 메시지
        private const string SET_POSITION_LABEL = "위치 설정"; // 위치 설정 메뉴 텍스트
        private const string INDEX_CHANGE_WINDOW = "인덱스 변경 창"; // 인덱스 변경 창 제목
        private readonly Vector2 INDEX_CHANGE_WINDOW_SIZE = new Vector2(300, 64); // 인덱스 변경 창 크기

        #region delegates

        // 레벨 추가 콜백 델리게이트
        public delegate void AddElementCallbackDelegate();
        // 레벨 제거 콜백 델리게이트
        public delegate void RemoveElementCallbackDelegate();
        // 컨텍스트 메뉴 표시 콜백 델리게이트
        public delegate void DisplayContextMenuCallbackDelegate(GenericMenu genericMenu);
        // 선택 해제 콜백 델리게이트
        public delegate void OnClearSelectionCallbackDelegate();
        // 전체 이름 변경 콜백 델리게이트
        public delegate void OnRenameAllCallbackDelegate();

        [Tooltip("레벨 추가 시 호출될 콜백 함수")]
        public AddElementCallbackDelegate addElementCallback;
        [Tooltip("레벨 제거 시 호출될 콜백 함수")]
        public RemoveElementCallbackDelegate removeElementCallback;
        [Tooltip("컨텍스트 메뉴 표시 시 호출될 콜백 함수")]
        public DisplayContextMenuCallbackDelegate displayContextMenuCallback;
        [Tooltip("선택 해제 시 호출될 콜백 함수")]
        public OnClearSelectionCallbackDelegate onClearSelectionCallback;
        [Tooltip("전체 레벨 이름 변경 시 호출될 콜백 함수")]
        public OnRenameAllCallbackDelegate onRenameAllCallback;
        #endregion

        // 레벨 이름 목록
        private List<string> levelLabels;
        // 레벨 데이터베이스 SerializedObject
        private SerializedObject levelsDatabaseSerializedObject;
        // 레벨 목록 SerializedProperty
        private SerializedProperty levelsSerializedProperty;
        // 커스텀 ReorderableList
        private CustomList customList;

        // 현재 선택된 레벨의 인덱스 (읽기 전용)
        [Tooltip("ReorderableList에서 현재 선택된 레벨의 인덱스입니다.")]
        public int SelectedLevelIndex => customList.SelectedIndex;
        // 현재 선택된 레벨의 SerializedProperty
        [Tooltip("ReorderableList에서 현재 선택된 레벨에 해당하는 SerializedProperty입니다.")]
        public SerializedProperty SelectedLevelProperty { get => levelsSerializedProperty.GetArrayElementAtIndex(SelectedLevelIndex); set => levelsSerializedProperty.GetArrayElementAtIndex(SelectedLevelIndex).objectReferenceValue = value.objectReferenceValue; }
        // 드래그 이벤트 무시 여부
        [Tooltip("ReorderableList의 드래그 이벤트를 무시할지 여부입니다.")]
        public bool IgnoreDragEvents { get => customList.IgnoreDragEvents; set => customList.IgnoreDragEvents = value; }
        // 커스텀 ReorderableList 인스턴스
        [Tooltip("레벨 목록을 관리하는 커스텀 ReorderableList 인스턴스입니다.")]
        public CustomList CustomList => customList;

        // LevelsHandler 클래스의 생성자
        // 레벨 데이터베이스와 레벨 목록 SerializedProperty를 받아 초기화합니다.
        // <param name="levelsDatabaseSerializedObject">레벨 데이터베이스의 SerializedObject입니다.</param>
        // <param name="levelsSerializedProperty">레벨 목록의 SerializedProperty입니다.</param>
        public LevelsHandler(SerializedObject levelsDatabaseSerializedObject, SerializedProperty levelsSerializedProperty)
        {
            this.levelsDatabaseSerializedObject = levelsDatabaseSerializedObject;
            this.levelsSerializedProperty = levelsSerializedProperty;
            this.levelLabels = new List<string>();

            SetLevelLabels();
            SetCustomList();
        }

        #region Reordable list

        // 커스텀 ReorderableList를 설정합니다.
        // ReorderableList에 필요한 콜백 함수들을 연결합니다.
        private void SetCustomList()
        {
            // customList = new CustomList(levelsDatabaseSerializedObject, levelsSerializedProperty);
            // GetLabel 콜백을 추가하여 레벨 이름을 표시하도록 변경
            customList = new CustomList(levelsDatabaseSerializedObject, levelsSerializedProperty,  GetLabel);

            // ReorderableList 이벤트에 콜백 함수 연결
            customList.getHeaderLabelCallback += GetHeaderCallback; // 헤더 라벨 가져오기 콜백
            customList.selectionChangedCallback += SelectionChangedCallback; // 선택 변경 콜백
            customList.listReorderedCallback += ListReorderedCallback; // 목록 순서 변경 콜백
            customList.addElementCallback += AddElementCallback; // 요소 추가 콜백
            customList.removeElementCallback += RemoveElementCallback; // 요소 제거 콜백
            customList.displayContextMenuCallback += DisplayContextMenuCallback; // 컨텍스트 메뉴 표시 콜백
        }

        // ReorderableList의 각 요소에 표시될 라벨을 가져옵니다.
        // <param name="elementProperty">현재 요소의 SerializedProperty입니다.</param>
        // <param name="elementIndex">현재 요소의 인덱스입니다.</param>
        // <returns>요소에 표시될 문자열 라벨입니다.</returns>
        private string GetLabel(SerializedProperty elementProperty, int elementIndex)
        {
            return levelLabels[elementIndex];
        }

        // ReorderableList의 컨텍스트 메뉴를 표시합니다.
        // 메뉴 항목을 추가하고 사용자 정의 콜백을 호출합니다.
        private void DisplayContextMenuCallback()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent(SET_POSITION_LABEL), false, OpenSetIndexModalWindow); // 위치 설정 메뉴 항목 추가
            genericMenu.AddItem(new GUIContent(REMOVE_SELECTION), false, ClearSelection); // 선택 해제 메뉴 항목 추가
            genericMenu.AddItem(new GUIContent(REMOVE_ELEMENT_CALLBACK), false, RemoveElementCallback); // 요소 제거 메뉴 항목 추가
            displayContextMenuCallback?.Invoke(genericMenu); // 사용자 정의 컨텍스트 메뉴 콜백 호출
            genericMenu.ShowAsContext(); // 컨텍스트 메뉴 표시
        }

        // 요소 제거 콜백 함수입니다.
        // 선택된 레벨을 삭제합니다.
        private void RemoveElementCallback()
        {
            DeleteLevel(SelectedLevelIndex); // 선택된 레벨 삭제
            removeElementCallback?.Invoke(); // 사용자 정의 제거 콜백 호출
        }

        // 요소 추가 콜백 함수입니다.
        // 새로운 레벨을 추가합니다.
        private void AddElementCallback()
        {
            AddLevel(); // 새로운 레벨 추가
            addElementCallback?.Invoke(); // 사용자 정의 추가 콜백 호출
        }

        // 목록 순서 변경 콜백 함수입니다.
        // 레벨 라벨 목록을 다시 설정합니다.
        private void ListReorderedCallback()
        {
            SetLevelLabels(); // 레벨 라벨 재설정
        }

        // 선택 변경 콜백 함수입니다.
        // 선택된 레벨을 엽니다.
        private void SelectionChangedCallback()
        {
            OpenLevel(SelectedLevelIndex); // 선택된 레벨 열기
        }

        // ReorderableList의 헤더 라벨을 가져옵니다.
        // 현재 레벨의 총 개수를 표시합니다.
        // <returns>헤더에 표시될 문자열 라벨입니다.</returns>
        private string GetHeaderCallback()
        {
            return DEFAULT_LEVEL_LIST_HEADER + levelsSerializedProperty.arraySize; // "레벨 개수: [개수]" 형식으로 반환
        }

        // 현재 선택된 레벨을 해제합니다.
        public void ClearSelection()
        {
            customList.SelectedIndex = -1; // 선택된 인덱스를 -1로 설정
            onClearSelectionCallback?.Invoke(); // 사용자 정의 선택 해제 콜백 호출
        }

        // 현재 선택된 레벨의 라벨을 업데이트합니다.
        // <param name="label">업데이트할 새로운 라벨 문자열입니다.</param>
        public void UpdateCurrentLevelLabel(string label)
        {
            if (SelectedLevelIndex != -1)
            {
                levelLabels[SelectedLevelIndex] = label; // 선택된 레벨의 라벨 업데이트
            }
        }

        // ReorderableList를 화면에 표시합니다.
        public void DisplayReordableList()
        {
            customList.Display(); // ReorderableList 표시
        }

        #endregion

        // 지정된 인덱스의 레벨을 엽니다.
        // LevelEditorBase 인스턴스를 통해 레벨을 엽니다.
        // <param name="index">열 레벨의 인덱스입니다.</param>
        public void OpenLevel(int index)
        {
            if (LevelEditorBase.Instance == null)
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR); // LevelEditorBase 인스턴스 누락 오류 로그
            }
            else
            {
                // LevelEditorBase를 통해 레벨 열기
                LevelEditorBase.Instance.OpenLevel(levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue, index);
            }
        }

        // 현재 선택된 레벨을 다시 엽니다.
        public void ReopenLevel()
        {
            OpenLevel(SelectedLevelIndex); // 선택된 레벨 열기
        }

        // 새로운 레벨을 추가합니다.
        // ScriptableObject로 새로운 레벨 에셋을 생성하고 목록에 추가합니다.
        public void AddLevel()
        {
            if (LevelEditorBase.Instance == null)
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR); // LevelEditorBase 인스턴스 누락 오류 로그
                return;
            }

            levelsSerializedProperty.arraySize++; // 레벨 배열 크기 증가
            int newLevelIndex = levelsSerializedProperty.arraySize - 1; // 새로운 레벨 인덱스 설정
            // LevelEditorBase에서 레벨 타입을 가져와 ScriptableObject 인스턴스 생성
            UnityEngine.Object level = ScriptableObject.CreateInstance(LevelEditorBase.Instance.GetLevelType());

            // 새로운 레벨 에셋 생성
            AssetDatabase.CreateAsset(level, GetRelativeLevelAssetPathByNumber(GetLevelNumber(levelsSerializedProperty.arraySize)));
            LevelEditorBase.Instance.ClearLevel(level); // 새로운 레벨 초기화
            // LevelEditorBase에서 레벨 라벨 가져와 목록에 추가
            levelLabels.Add(LevelEditorBase.Instance.GetLevelLabel(level, newLevelIndex));
            // SerializedProperty에 새로운 레벨 오브젝트 할당
            levelsSerializedProperty.GetArrayElementAtIndex(newLevelIndex).objectReferenceValue = level;
            AssetDatabase.SaveAssets(); // 에셋 저장

            customList.SelectedIndex = newLevelIndex; // 새로운 레벨을 선택 상태로 설정

            OpenLevel(newLevelIndex); // 새로운 레벨 열기
        }

        // 다음 사용 가능한 레벨 번호를 가져옵니다.
        // 파일 시스템을 확인하여 중복되지 않는 번호를 찾습니다.
        // <param name="arraySize">현재 레벨 배열의 크기입니다.</param>
        // <returns>형식화된 레벨 번호 문자열입니다.</returns>
        private string GetLevelNumber(int arraySize)
        {
            int levelNumber = arraySize - 1;

            // 파일이 존재하지 않는 번호를 찾을 때까지 증가
            do
            {
                levelNumber++;
            }
            while (File.Exists(LevelEditorBase.GetProjectPath() + GetRelativeLevelAssetPathByNumber(FormatNumber(levelNumber))));

            return FormatNumber(levelNumber); // 형식화된 번호 반환
        }

        // 레벨 번호에 해당하는 상대적인 에셋 경로를 가져옵니다.
        // <param name="levelNumber">레벨 번호 문자열입니다.</param>
        // <returns>레벨 에셋의 상대 경로 문자열입니다.</returns>
        private static string GetRelativeLevelAssetPathByNumber(string levelNumber)
        {
            return LevelEditorBase.Instance.LEVELS_FOLDER_PATH + PATH_SEPARATOR + LEVEL_PREFIX + levelNumber + ASSET_SUFFIX;
        }

        // 숫자를 지정된 형식으로 변환합니다.
        // <param name="maxIndex">형식화할 숫자입니다.</param>
        // <returns>형식화된 숫자 문자열입니다.</returns>
        private static string FormatNumber(int maxIndex)
        {
            return maxIndex.ToString(FORMAT_TYPE); // "000" 형식으로 변환
        }

        // 지정된 인덱스의 레벨을 삭제합니다.
        // 사용자에게 확인 메시지를 표시한 후 삭제를 처리합니다.
        // <param name="levelIndex">삭제할 레벨의 인덱스입니다.</param>
        public void DeleteLevel(int levelIndex)
        {
            StringBuilder stringBuilder = LevelEditorBase.Instance.stringBuilder;
            stringBuilder.Clear();
            stringBuilder.Append(REMOVE_LEVEL); // 삭제 확인 메시지 구성
            stringBuilder.Append(BRACKET);
            stringBuilder.Append(levelLabels[levelIndex]);
            stringBuilder.Append(BRACKET);
            stringBuilder.Append(QUESTION_MARK);

            // 삭제 확인 대화 상자 표시
            if (EditorUtility.DisplayDialog(REMOVING_LEVEL_TITLE, stringBuilder.ToString(), YES, CANCEL))
            {
                HandleDeleteLevel(levelIndex); // 삭제 처리 함수 호출
            }
        }

        // 레벨 삭제를 실제 처리하는 함수입니다.
        // 에셋 파일을 삭제하고 목록에서 해당 레벨을 제거합니다.
        // <param name="levelIndex">삭제할 레벨의 인덱스입니다.</param>
        private void HandleDeleteLevel(int levelIndex)
        {
            UnityEngine.Object tempObject = levelsSerializedProperty.GetArrayElementAtIndex(levelIndex).objectReferenceValue;

            if (tempObject != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tempObject)); // 에셋 파일 삭제
                AssetDatabase.Refresh(); // 에셋 데이터베이스 새로고침
            }

            if (customList != null)
            {
                customList.SelectedIndex = -1; // 선택 해제
            }

            levelLabels.RemoveAt(levelIndex); // 라벨 목록에서 제거
            levelsSerializedProperty.GetArrayElementAtIndex(levelIndex).objectReferenceValue = null; // SerializedProperty 값 초기화
            levelsSerializedProperty.DeleteArrayElementAtIndex(levelIndex); // SerializedProperty에서 요소 삭제
        }

        // 레벨 라벨 목록을 다시 설정합니다.
        // 현재 레벨 목록을 기반으로 라벨 목록을 업데이트합니다.
        public void SetLevelLabels()
        {
            levelLabels.Clear(); // 기존 라벨 목록 초기화

            if (LevelEditorBase.Instance == null)
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR); // LevelEditorBase 인스턴스 누락 오류 로그
                return;
            }

            // 각 레벨에 대해 LevelEditorBase에서 라벨을 가져와 목록에 추가
            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                levelLabels.Add(LevelEditorBase.Instance.GetLevelLabel(levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue, i));
            }
        }

        // 모든 레벨의 이름을 올바른 형식으로 변경합니다.
        // "Level_[번호]" 형식으로 이름을 맞춥니다.
        public void RenameLevels()
        {
            List<int> indexesOfIncorrectLevels = new List<int>();

            // 이름이 올바르지 않은 레벨의 인덱스를 찾습니다.
            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                if (!levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue.name.Equals(LEVEL_PREFIX + FormatNumber(i + 1)))
                {
                    indexesOfIncorrectLevels.Add(i);
                }
            }

            string name;

            // 올바르지 않은 이름의 레벨을 임시 이름으로 변경합니다.
            foreach (int index in indexesOfIncorrectLevels)
            {
                if (levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue != null)
                {
                    name = levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue), OLD_PREFIX + name);
                }
            }

            // 임시 이름으로 변경된 레벨의 이름을 올바른 형식으로 다시 변경합니다.
            foreach (int index in indexesOfIncorrectLevels)
            {
                if (levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue != null)
                {
                    name = levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue), LEVEL_PREFIX + FormatNumber(index + 1));
                }
            }

            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // 에셋 데이터베이스 새로고침
            SetLevelLabels(); // 레벨 라벨 재설정
            onRenameAllCallback?.Invoke(); // 전체 이름 변경 콜백 호출
        }

        #region draw buttons

        // 레벨 이름 변경 버튼을 그립니다.
        public void DrawRenameLevelsButton()
        {
            if (GUILayout.Button(RENAME_LEVELS_LABEL, EditorCustomStyles.button))
            {
                RenameLevels(); // 버튼 클릭 시 레벨 이름 변경 함수 호출
            }
        }

        // 선택 해제 버튼을 그립니다.
        public void DrawClearSelectionButton()
        {
            if (GUILayout.Button(REMOVE_SELECTION, EditorCustomStyles.button))
            {
                ClearSelection(); // 버튼 클릭 시 선택 해제 함수 호출
            }
        }

        // 전역 유효성 검사 버튼을 그립니다.
        public void DrawGlobalValidationButton()
        {
            if (GUILayout.Button(GLOBAL_VALIDATION_LABEL, EditorCustomStyles.button))
            {
                Debug.Log("전역 유효성 검사 시작"); // 유효성 검사 시작 로그

                // 각 레벨에 대해 LevelEditorBase를 통해 유효성 검사 수행
                for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
                {
                    LevelEditorBase.Instance.LogErrorsForGlobalValidation(levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue, i);
                }

                Debug.Log("전역 유효성 검사 종료"); // 유효성 검사 종료 로그

                SetLevelLabels(); // 레벨 라벨 재설정 (유효성 검사 결과 반영 등)
            }
        }

        #endregion

        #region Set index modal window

        // 레벨 인덱스 변경 모달 창을 엽니다.
        private void OpenSetIndexModalWindow()
        {
            // SetIndexModalWindow 인스턴스 생성 및 데이터 설정
            SetIndexModalWindow window = ScriptableObject.CreateInstance<SetIndexModalWindow>();
            window.SetData(SelectedLevelIndex, levelsSerializedProperty.arraySize, this);
            window.minSize = INDEX_CHANGE_WINDOW_SIZE; // 창 최소 크기 설정
            window.maxSize = INDEX_CHANGE_WINDOW_SIZE; // 창 최대 크기 설정
            window.titleContent = new GUIContent(INDEX_CHANGE_WINDOW); // 창 제목 설정

            window.ShowModal(); // 모달 창 표시
        }

        // 모달 창에서 인덱스 변경 요청을 처리합니다.
        // 레벨의 순서를 변경하고 SerializedObject를 업데이트합니다.
        // <param name="originalIndex">원래 레벨의 인덱스입니다.</param>
        // <param name="newIndex">변경할 새로운 인덱스입니다.</param>
        private void ModalWindowProcessChange(int originalIndex, int newIndex)
        {
            levelsSerializedProperty.MoveArrayElement(originalIndex, newIndex); // 배열 요소 순서 변경
            levelsDatabaseSerializedObject.ApplyModifiedProperties(); // SerializedObject 변경 사항 적용
            customList.SelectedIndex = newIndex; // 선택된 인덱스 업데이트
            ListReorderedCallback(); // 목록 순서 변경 콜백 호출 (라벨 재설정 등)
        }

        // 레벨 인덱스 변경을 위한 모달 창 클래스
        private class SetIndexModalWindow : EditorWindow
        {
            private const string INT_FIELD_LABEL = "대상 요소 새 번호"; // 입력 필드 라벨
            private const string CANCEL_BUTTON_LABEL = "취소"; // 취소 버튼 텍스트
            private const string CHANGE_BUTTON_LABEL = "변경"; // 변경 버튼 텍스트
            private const string DEFAULT_LABEL = "대상 요소 번호: "; // 기본 라벨 텍스트
            [Tooltip("원래 레벨의 인덱스입니다.")]
            public int elementOrinialIndex;
            [Tooltip("총 레벨의 개수입니다.")]
            public int arraySize;
            [Tooltip("이 모달 창을 호출한 LevelsHandler 인스턴스입니다.")]
            public LevelsHandler levelsHandler;
            private string label;
            private int newPositionNumber;

            // 모달 창 데이터 설정 함수
            // <param name="elementOrinialIndex">원래 레벨의 인덱스입니다.</param>
            // <param name="arraySize">총 레벨의 개수입니다.</param>
            // <param name="levelsHandler">LevelsHandler 인스턴스입니다.</param>
            public void SetData(int elementOrinialIndex,int arraySize, LevelsHandler levelsHandler)
            {
                this.elementOrinialIndex = elementOrinialIndex;
                this.levelsHandler = levelsHandler;
                this.arraySize = arraySize;
                label = DEFAULT_LABEL + (elementOrinialIndex + 1); // 라벨 설정
                newPositionNumber = elementOrinialIndex + 1; // 기본 새 위치 번호 설정
            }

            // GUI를 그리는 함수
            void OnGUI()
            {
                EditorGUILayout.BeginVertical(); // 수직 레이아웃 시작
                EditorGUILayout.LabelField(label); // 현재 요소 번호 라벨 표시
                newPositionNumber = EditorGUILayout.IntField(INT_FIELD_LABEL, newPositionNumber); // 새 위치 번호 입력 필드
                newPositionNumber = Mathf.Clamp(newPositionNumber, 1, arraySize); // 입력 값 유효성 검사 (1부터 총 개수까지)

                EditorGUILayout.BeginHorizontal(); // 수평 레이아웃 시작

                // 취소 버튼
                if (GUILayout.Button(CANCEL_BUTTON_LABEL))
                {
                    this.Close(); // 창 닫기
                }

                // 변경 버튼
                if (GUILayout.Button(CHANGE_BUTTON_LABEL))
                {
                    // LevelsHandler의 인덱스 변경 처리 함수 호출
                    levelsHandler.ModalWindowProcessChange(elementOrinialIndex, newPositionNumber - 1);
                    this.Close(); // 창 닫기
                }

                EditorGUILayout.EndHorizontal(); // 수평 레이아웃 종료

                EditorGUILayout.EndVertical(); // 수직 레이아웃 종료
            }
        }

        #endregion
    }
}