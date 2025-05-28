// SavePresetsWindow.cs
// 이 스크립트는 Unity 에디터에서 게임 저장 프리셋을 관리하는 커스텀 에디터 창입니다.
// 저장된 게임 상태를 프리셋으로 저장하고, 불러오거나 삭제하며, 다른 프리셋으로 업데이트하는 기능을 제공합니다.
// 탭, 정렬, 검색, 가져오기/내보내기 등 다양한 관리 기능을 포함합니다.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal; // ReorderableList를 사용하기 위해 필요
using System.Linq; // LINQ 확장을 사용하기 위해 필요

namespace Watermelon
{
    // 게임 저장 프리셋을 관리하는 Unity 에디터 창 클래스입니다.
    public class SavePresetsWindow : EditorWindow
    {
        // 마지막으로 선택된 탭 인덱스를 저장하는 EditorPrefs 키입니다.
        private const string LAST_SELECTED_TAB = "save_presets_window_last_selected_tab";
        // 각 탭의 정렬 방식을 저장하는 EditorPrefs 키 접두사입니다.
        private const string SORT_TYPE = "save_presets_window_sort_type__";

        // UI 요소의 고정 너비 값들입니다.
        private const int UPDATE_BUTTON_WIDTH = 80; // 업데이트 버튼 너비
        private const int ACTIVATE_BUTTON_WIDTH = 80; // 활성화 버튼 너비
        private const int SHARE_BUTTON_WIDTH = 18; // 공유 버튼 너비 (폴더 열기)
        private const int DATE_LABEL_WIDTH = 50; // 날짜 라벨 너비
        private const int DEFAULT_SPACE = 8; // 기본 간격

        // 에디터 창의 최소 크기입니다.
        private static readonly Vector2 WINDOW_SIZE = new Vector2(490, 495);
        // 에디터 창의 제목입니다.
        private static readonly string WINDOW_TITLE = "Save Presets";

        // 현재 선택된 탭 인덱스 (GUI 처리용 임시 변수)
        private int tempTabIndex;
        // 스크롤 뷰의 현재 스크롤 위치입니다.
        private Vector2 scrollView;

        // 로드된 모든 저장 프리셋 목록입니다.
        private List<SavePreset> allSavePresets;
        // 새로 생성할 프리셋의 임시 이름입니다.
        private string tempPresetName;
        // 현재 선택된 탭에 해당하는 저장 프리셋 목록입니다.
        private List<SavePreset> selectedSavePresets;
        // 선택된 저장 프리셋 목록을 표시하고 순서 변경 및 삭제를 지원하는 ReorderableList 객체입니다.
        private ReorderableList savePresetsList;
        // UI 요소 그리기에 사용되는 임시 Rect 객체입니다.
        private Rect workRect;
        // 탭 헤더에 표시될 이름 배열입니다. (폴더 이름)
        private string[] tabNames;
        // 각 탭(폴더)별 정렬 방식을 저장하는 Dictionary입니다.
        private Dictionary<string,TabSortType> sortTypes;
        // 현재 선택된 탭의 인덱스입니다.
        private int selectedTabIndex;
        // 현재 선택된 탭의 정렬 방식입니다.
        private TabSortType currentSortType;

        // 공유(폴더 열기) 버튼에 사용될 GUIContent (아이콘) 입니다.
        private GUIContent shareButtonContent;
        // 가져오기 버튼 옆에 표시될 화살표 아이콘 GUIContent입니다.
        private GUIContent arrowDownContent;
        // 가져오기 버튼의 Rect 영역입니다.
        private Rect importRect;
        // UI 요소에 간격을 추가하기 위한 GUIStyle입니다.
        private GUIStyle spacedContentStyle;
        // 수정된 박스 스타일 (추가 저장 필드에 사용) 입니다.
        private GUIStyle modifiedBoxStyle;
        // 임시로 저장하는 기본 라벨 너비입니다. (GUI 레이아웃 조정에 사용)
        private float backupLabelWidth;

        /// <summary>
        /// Unity 에디터 메뉴에 "Tools/Save Presets" 및 "Window/Save Presets" 항목을 추가하고,
        /// 메뉴 선택 시 Save Presets 에디터 창을 엽니다.
        /// </summary>
        // Unity 에디터 메뉴 항목을 정의합니다.
        [MenuItem("Tools/Save Presets")]
        [MenuItem("Window/Save Presets")]
        static void ShowWindow()
        {
            // SavePresetsWindow 타입의 에디터 창을 가져오거나 생성합니다.
            SavePresetsWindow tempWindow = (SavePresetsWindow)GetWindow(typeof(SavePresetsWindow), false, WINDOW_TITLE);
            // 창의 최소 크기를 설정합니다.
            tempWindow.minSize = WINDOW_SIZE;
            // 창 제목과 아이콘을 설정합니다.
            tempWindow.titleContent = new GUIContent(WINDOW_TITLE, EditorCustomStyles.GetIcon("icon_title"));
        }

        /// <summary>
        /// 에디터 창이 활성화될 때 호출됩니다.
        /// 저장 프리셋 데이터 로드, 폴더 구조 확인 및 생성, 탭 초기화, 정렬 설정 로드, ReorderableList 설정 등을 수행합니다.
        /// </summary>
        protected void OnEnable()
        {
            // 저장 데이터가 수정되었음을 나타내는 플래그를 초기화합니다.
            SavePresets.saveDataMofied = false;
            // 모든 저장 프리셋 목록을 초기화합니다.
            allSavePresets = new List<SavePreset>();
            // 저장 프리셋이 저장될 기본 디렉토리 경로를 가져옵니다.
            string directoryPath = SavePresets.GetDirectoryPath();

            // 저장 디렉토리가 존재하지 않으면 생성합니다.
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 저장 디렉토리 내의 모든 서브 디렉토리(탭) 목록을 가져옵니다.
            string[] directoryEntries = Directory.GetDirectories(directoryPath);
            // 각 탭별 정렬 방식을 저장할 Dictionary를 초기화합니다.
            sortTypes = new Dictionary<string, TabSortType>();
            // 현재 처리 중인 디렉토리 이름 (탭 이름)
            string directory = SavePresets.DEFAULT_DIRECTORY;
            // 현재 탭의 정렬 방식
            TabSortType sortType;
            // 현재 디렉토리 내의 파일 목록
            string[] fileEntries;
            // 파일 생성 시간
            DateTime creationTime = DateTime.Now;
            // 탭 이름 목록 (임시 저장용)
            List<string> tempTabNames = new List<string>();
            // 기본 디렉토리 탭을 목록의 첫 번째로 추가합니다.
            tempTabNames.Add(SavePresets.DEFAULT_DIRECTORY);

            // 각 서브 디렉토리(탭)를 순회합니다.
            for (int i = 0; i < directoryEntries.Length; i++)
            {
                // 디렉토리 경로에서 디렉토리 이름(탭 이름)을 가져옵니다.
                directory = SavePresets.GetFileName(directoryEntries[i]);
                // 현재 디렉토리(탭)의 저장된 정렬 방식을 가져옵니다.
                sortType = GetSortType(directory);
                // 현재 디렉토리 내의 모든 파일 목록을 가져옵니다.
                fileEntries = Directory.GetFiles(directoryEntries[i]);

                // 파일이 존재하는 디렉토리만 탭 목록에 추가합니다.
                if(fileEntries.Length > 0)
                {
                    // 이미 탭 목록에 추가되지 않았으면 추가합니다.
                    if (!tempTabNames.Contains(directory))
                    {
                        tempTabNames.Add(directory);
                    }

                    // 현재 디렉토리(탭)의 정렬 방식을 Dictionary에 저장합니다.
                    sortTypes.Add(directory, sortType);
                }

                // 현재 디렉토리 내의 파일을 순회하며 저장 프리셋으로 추가합니다.
                for (int j = 0; j < fileEntries.Length; j++)
                {
                    // ".meta" 파일은 건너뜁니다.
                    if (!fileEntries[j].EndsWith(SavePresets.META_SUFFIX))
                    {
                        // 파일 생성 시간을 가져옵니다.
                        creationTime = File.GetCreationTimeUtc(fileEntries[j]);
                        // SavePreset 객체를 생성하여 모든 저장 프리셋 목록에 추가합니다.
                        allSavePresets.Add(new SavePreset(SavePresets.GetFileName(fileEntries[j]), creationTime, fileEntries[j], directory));
                    }
                }
            }

            // 기본 디렉토리의 정렬 방식이 아직 추가되지 않았으면 추가합니다.
            if (!sortTypes.ContainsKey(SavePresets.DEFAULT_DIRECTORY))
            {
                sortTypes.Add(SavePresets.DEFAULT_DIRECTORY, GetSortType(SavePresets.DEFAULT_DIRECTORY));
            }

            // 임시 탭 이름 목록을 배열로 변환합니다.
            tabNames = tempTabNames.ToArray();
            // 현재 선택된 탭에 표시될 저장 프리셋 목록을 초기화합니다.
            selectedSavePresets = new List<SavePreset>();
            // EditorPrefs에 저장된 마지막으로 선택된 탭 인덱스를 로드하고 유효 범위 내로 조정합니다.
            selectedTabIndex = Mathf.Clamp(EditorPrefs.GetInt(LAST_SELECTED_TAB, selectedTabIndex), 0, tabNames.Length - 1);
            // 로드된 탭 인덱스에 해당하는 탭을 선택합니다.
            SelectTab(selectedTabIndex);

            // 선택된 저장 프리셋 목록을 사용하여 ReorderableList를 생성합니다.
            savePresetsList = new ReorderableList(selectedSavePresets, typeof(SavePreset), false, false, false, true);
            // 리스트 항목의 높이를 설정합니다.
            savePresetsList.elementHeight = 26;
            // 각 리스트 항목을 그리는 콜백 함수를 설정합니다.
            savePresetsList.drawElementCallback = DrawElement;
            // 리스트 항목이 삭제될 때 호출되는 콜백 함수를 설정합니다.
            savePresetsList.onRemoveCallback = RemoveCallback;
            // 리스트가 비어 있을 때 표시될 내용을 그리는 콜백 함수를 설정합니다.
            savePresetsList.drawNoneElementCallback = DrawNoneElementCallback;

            // UI 그리기에 사용될 임시 Rect 객체를 초기화합니다.
            workRect = new Rect();
            // 공유 버튼 아이콘을 로드하여 GUIContent를 생성합니다.
            shareButtonContent = new GUIContent(EditorCustomStyles.GetIcon("icon_share"));
            // 화살표 아래 아이콘을 로드하여 GUIContent를 생성합니다.
            arrowDownContent = new GUIContent(EditorCustomStyles.GetIcon("icon_arrow_down"));
            // 간격 스타일을 설정합니다.
            spacedContentStyle = new GUIStyle();
            spacedContentStyle.padding = new RectOffset(4, 4, 4, 4);
            // 수정된 박스 스타일을 설정합니다.
            modifiedBoxStyle = new GUIStyle(EditorCustomStyles.box);
            modifiedBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            modifiedBoxStyle.padding = new RectOffset(8, 8, 8, 8);
            modifiedBoxStyle.overflow = new RectOffset(0, 0, 0, 0);
        }

        /// <summary>
        /// 지정된 인덱스의 탭을 선택하고, 해당 탭의 저장 프리셋 목록을 필터링 및 정렬하는 함수입니다.
        /// </summary>
        /// <param name="index">선택할 탭의 인덱스</param>
        private void SelectTab(int index)
        {
            // 모든 저장 프리셋 목록을 탭 이름 및 현재 탭의 정렬 방식에 따라 정렬합니다.
            allSavePresets.Sort(TypeAndCustomSort);
            // 선택된 탭 인덱스를 업데이트합니다.
            selectedTabIndex = index;
            // 현재 선택된 탭의 정렬 방식을 가져옵니다.
            currentSortType = sortTypes[tabNames[selectedTabIndex]];
            // 마지막으로 선택된 탭 인덱스를 EditorPrefs에 저장합니다.
            EditorPrefs.SetInt(LAST_SELECTED_TAB, selectedTabIndex);
            // 선택된 저장 프리셋 목록을 비웁니다.
            selectedSavePresets.Clear();

            // 모든 저장 프리셋 목록을 순회하며 현재 선택된 탭에 해당하는 프리셋만 선택된 목록에 추가합니다.
            for (int i = 0; i < allSavePresets.Count; i++)
            {
                if (allSavePresets[i].folderName.Equals(tabNames[selectedTabIndex]))
                {
                    selectedSavePresets.Add(allSavePresets[i]);
                }
            }
        }

        /// <summary>
        /// ReorderableList에서 항목이 삭제될 때 호출되는 콜백 함수입니다.
        /// 삭제 확인 대화상자를 표시하고, 확인 시 해당 저장 프리셋 파일을 삭제합니다.
        /// </summary>
        /// <param name="list">삭제 이벤트가 발생한 ReorderableList 객체</param>
        private void RemoveCallback(ReorderableList list)
        {
            // 삭제 확인 대화상자를 표시합니다.
            if (EditorUtility.DisplayDialog("This preset will be removed!", "Are you sure?", "Remove", "Cancel"))
            {
                // 유효한 인덱스 범위 내에서 선택된 항목이 있으면
                if (list.index >= 0 && list.index < selectedSavePresets.Count)
                {
                    // SavePresets 유틸리티를 사용하여 해당 저장 프리셋 파일을 삭제합니다.
                    SavePresets.RemoveSave(selectedSavePresets[list.index].fileName, selectedSavePresets[list.index].folderName);
                }

                // ReorderableList의 선택을 해제합니다.
                savePresetsList.ClearSelection();
            }
        }

        /// <summary>
        /// ReorderableList의 각 항목을 그릴 때 호출되는 콜백 함수입니다.
        /// 각 저장 프리셋의 이름, 날짜, 활성화/업데이트/공유 버튼을 그립니다.
        /// </summary>
        /// <param name="rect">항목이 그려질 영역의 Rect</param>
        /// <param name="index">현재 항목의 인덱스</param>
        /// <param name="isActive">현재 항목이 활성화 상태인지 여부</param>
        /// <param name="isFocused">현재 항목이 포커스 상태인지 여부</param>
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // UI 요소 배치를 위한 작업 Rect를 설정합니다.
            workRect.Set(rect.x + rect.width, rect.y + 4, 0, 18);

            // 공유(폴더 열기) 버튼을 그립니다.
            workRect.x -= SHARE_BUTTON_WIDTH + DEFAULT_SPACE;
            workRect.width = SHARE_BUTTON_WIDTH;
            if(GUI.Button(workRect, shareButtonContent))
            {
                // 버튼 클릭 시 해당 프리셋 파일이 저장된 폴더를 파일 탐색기(Finder)에서 엽니다.
                EditorUtility.RevealInFinder(selectedSavePresets[index].path);
            }

            // 현재 탭이 기본 디렉토리 탭(인덱스 0)인 경우에만 업데이트 버튼을 그립니다.
            if (selectedTabIndex == 0)
            {
                // 업데이트 버튼을 그립니다.
                workRect.x -= UPDATE_BUTTON_WIDTH + DEFAULT_SPACE;
                workRect.width = UPDATE_BUTTON_WIDTH;
                if (GUI.Button(workRect, "Update"))
                {
                    // 업데이트 확인 대화상자를 표시합니다.
                    if (EditorUtility.DisplayDialog("This preset will rewrited!", "Are you sure?", "Rewrite", "Cancel"))
                    {
                        // 확인 시 SavePresets 유틸리티를 사용하여 현재 게임 상태로 해당 프리셋 파일을 덮어씁니다.
                        SavePresets.CreateSave(selectedSavePresets[index].fileName, selectedSavePresets[index].folderName);
                    }
                }
            }

            // 활성화 버튼을 그립니다.
            workRect.x -= ACTIVATE_BUTTON_WIDTH + DEFAULT_SPACE;
            workRect.width = ACTIVATE_BUTTON_WIDTH;

            // 에디터가 플레이 모드일 때는 활성화 버튼을 비활성화합니다.
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            if (GUI.Button(workRect, "Activate", EditorCustomStyles.buttonGreen))
            {
                // 버튼 클릭 시 SavePresets 유틸리티를 사용하여 해당 저장 프리셋을 게임에 로드합니다.
                SavePresets.LoadSave(selectedSavePresets[index].fileName, tabNames[selectedTabIndex]);
            }
            EditorGUI.EndDisabledGroup();

            // 생성 날짜 라벨을 그립니다.
            workRect.x -= DATE_LABEL_WIDTH + DEFAULT_SPACE;
            workRect.width = DATE_LABEL_WIDTH;
            GUI.Label(workRect, selectedSavePresets[index].creationDate.ToString("dd.MM"));

            // 파일 이름 라벨을 그립니다.
            workRect.x -= DEFAULT_SPACE;
            workRect.width = workRect.x - rect.x;
            workRect.x = rect.x;
            GUI.Label(workRect, selectedSavePresets[index].fileName);
        }

        /// <summary>
        /// ReorderableList가 비어 있을 때 호출되는 콜백 함수입니다.
        /// 리스트가 비어 있음을 알리는 메시지를 표시합니다.
        /// </summary>
        /// <param name="rect">메시지가 그려질 영역의 Rect</param>
        private void DrawNoneElementCallback(Rect rect)
        {
            // 리스트가 비어 있음을 알리는 라벨을 그립니다.
            GUI.Label(rect, "There are no saves yet");
        }

        /// <summary>
        /// 에디터 창의 전체 GUI를 그릴 때 호출됩니다.
        /// 탭 바, 옵션 라인, 저장 프리셋 목록 (ReorderableList), 새 저장 추가 라인을 그립니다.
        /// 저장 데이터 수정 감지 시 창을 새로고침합니다.
        /// </summary>
        private void OnGUI()
        {
            // 창의 전체 GUI 레이아웃을 시작합니다. (수직 정렬)
            EditorGUILayout.BeginVertical(spacedContentStyle);

            // 탭이 여러 개인 경우에만 탭 바를 그립니다.
            if (tabNames.Length > 1)
            {
                // GUILayout.Toolbar를 사용하여 탭 바를 그리고 선택된 탭 인덱스를 가져옵니다.
                tempTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames, EditorCustomStyles.tab);
                // 탭 바 아래에 간격을 추가합니다.
                GUILayout.Space(5);

                // 선택된 탭이 변경되었으면 해당 탭을 선택하는 함수를 호출합니다.
                if (tempTabIndex != selectedTabIndex)
                {
                    SelectTab(tempTabIndex);
                }
            }

            // 옵션 라인 (가져오기, 정렬, 모두 제거 버튼)을 그립니다.
            DisplayOptionsLine();

            // 저장 프리셋 목록을 스크롤 가능한 영역 안에 그립니다.
            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            savePresetsList.DoLayoutList(); // ReorderableList의 GUI를 그립니다.
            EditorGUILayout.EndScrollView();

            // 새 저장 추가 라인 (입력 필드 및 확인 버튼)을 그립니다.
            DisplayAddSaveLine();

            // 창의 전체 GUI 레이아웃을 종료합니다.
            EditorGUILayout.EndVertical();

            // SavePresets에서 저장 데이터가 수정되었음을 감지하면 창을 새로고침합니다.
            if (SavePresets.saveDataMofied)
            {
                OnEnable(); // OnEnable을 다시 호출하여 데이터를 다시 로드하고 UI를 갱신합니다.
            }
        }

        /// <summary>
        /// 에디터 창의 옵션 라인 (가져오기, 정렬, 모두 제거 버튼)을 그리는 함수입니다.
        /// </summary>
        private void DisplayOptionsLine()
        {
            // 옵션 라인의 수평 레이아웃을 시작합니다.
            EditorGUILayout.BeginHorizontal();

            // "Import" 버튼을 그립니다.
            importRect = EditorGUILayout.BeginHorizontal(GUI.skin.button, GUILayout.Width(50));
            EditorGUILayout.LabelField("Import", GUILayout.Width(40));
            EditorGUILayout.LabelField(arrowDownContent, GUILayout.Width(14));
            EditorGUILayout.EndHorizontal();
            // 버튼 영역에 투명 버튼을 오버레이하여 클릭 이벤트를 처리합니다.
            if (GUI.Button(importRect, GUIContent.none, GUIStyle.none))
            {
                ImportFile(); // 버튼 클릭 시 파일 가져오기 함수를 호출합니다.
            }

            // 선택된 저장 프리셋이 없으면 나머지 옵션(정렬, 모두 제거)은 그리지 않고 함수를 종료합니다.
            if (selectedSavePresets.Count == 0)
            {
                EditorGUILayout.EndHorizontal();
                return;
            }

            // 정렬 방식 선택 UI를 그립니다.
            backupLabelWidth = EditorGUIUtility.labelWidth; // 기본 라벨 너비를 임시 저장합니다.
            EditorGUIUtility.labelWidth = 50; // 라벨 너비를 조정합니다.
            EditorGUILayout.PrefixLabel("Sort by"); // "Sort by" 라벨을 그립니다.
            EditorGUIUtility.labelWidth = backupLabelWidth; // 라벨 너비를 원래대로 복원합니다.

            // 정렬 방식 EnumPopup을 그립니다. 값 변경 감지를 시작합니다.
            EditorGUI.BeginChangeCheck();
            currentSortType = (TabSortType)EditorGUILayout.EnumPopup(currentSortType);
            // 정렬 방식이 변경되었으면
            if (EditorGUI.EndChangeCheck())
            {
                // 현재 탭의 정렬 방식과 다르면 저장하고 창을 새로고침합니다.
                if (sortTypes[tabNames[selectedTabIndex]] != currentSortType)
                {
                    SetSortType(currentSortType, tabNames[selectedTabIndex]); // 변경된 정렬 방식을 EditorPrefs에 저장
                    sortTypes[tabNames[selectedTabIndex]] = currentSortType; // Dictionary 업데이트
                    OnEnable(); // 창 새로고침 (데이터 다시 로드 및 UI 갱신)
                }
            }

            // 남은 공간을 유연하게 사용하도록 합니다.
            GUILayout.FlexibleSpace();

            // "Remove all" 버튼을 그립니다.
            if (GUILayout.Button("Remove all"))
            {
                // 모두 제거 확인 대화상자를 표시합니다.
                if (EditorUtility.DisplayDialog("All presets will be removed!", "Are you sure?", "Remove", "Cancel"))
                {
                    // 확인 시 ReorderableList의 선택을 해제합니다.
                    savePresetsList.ClearSelection();

                    // 현재 탭의 모든 저장 프리셋 파일을 역순으로 순회하며 삭제합니다.
                    for (int i = selectedSavePresets.Count - 1; i >= 0; i--)
                    {
                        SavePresets.RemoveSave(selectedSavePresets[i].fileName, selectedSavePresets[i].folderName);
                    }

                    OnEnable(); // 창 새로고침 (데이터 다시 로드 및 UI 갱신)
                }
            }

            // 옵션 라인의 수평 레이아웃을 종료합니다.
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 에디터 창의 새 저장 추가 라인 (입력 필드 및 확인 버튼)을 그리는 함수입니다.
        /// 기본 디렉토리 탭(인덱스 0)일 때만 표시됩니다.
        /// </summary>
        private void DisplayAddSaveLine()
        {
            // 현재 탭이 기본 디렉토리 탭(인덱스 0)일 때만 새 저장 추가 라인을 그립니다.
            if (selectedTabIndex == 0)
            {
                // 남은 공간을 유연하게 사용하도록 합니다.
                GUILayout.FlexibleSpace();

                // 새 저장 추가 섹션의 수직 레이아웃을 시작합니다.
                EditorGUILayout.BeginVertical(modifiedBoxStyle);
                // "Add new save" 라벨을 그립니다.
                EditorGUILayout.LabelField("Add new save");
                // 입력 필드와 확인 버튼의 수평 레이아웃을 시작합니다.
                EditorGUILayout.BeginHorizontal();

                // 입력 필드에 포커스된 상태에서 Enter 키를 눌렀는지 확인합니다.
                if (GUI.GetNameOfFocusedControl().Equals("tempPresetName"))
                {
                    if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return))
                    {
                        ConfirmButtonPressed(); // Enter 키 입력 시 확인 버튼 동작 수행
                        Repaint(); // 창을 다시 그리도록 요청합니다.
                    }
                }

                // 임시 프리셋 이름 입력 필드를 그립니다. 이름을 설정하여 GUI 포커스 추적에 사용합니다.
                GUI.SetNextControlName("tempPresetName");
                tempPresetName = EditorGUILayout.TextField(tempPresetName);

                // "Confirm" 버튼을 그립니다.
                if (GUILayout.Button("Confirm"))
                {
                    ConfirmButtonPressed(); // 버튼 클릭 시 확인 버튼 동작 수행
                }

                // 입력 필드와 확인 버튼의 수평 레이아웃을 종료합니다.
                EditorGUILayout.EndHorizontal();
                // 새 저장 추가 섹션의 수직 레이아웃을 종료합니다.
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 새 저장 프리셋 추가 시 확인 버튼을 눌렀을 때 실행되는 함수입니다.
        /// 입력된 이름으로 저장 프리셋을 생성하고 입력 필드를 초기화합니다.
        /// </summary>
        private void ConfirmButtonPressed()
        {
            // 입력된 이름으로 저장 프리셋을 생성합니다. (기본 디렉토리에 생성)
            SavePresets.CreateSave(tempPresetName);
            // 현재 GUI 포커스를 해제합니다.
            GUI.FocusControl(null);
            // 임시 프리셋 이름 입력 필드를 비웁니다.
            tempPresetName = string.Empty;
        }

        /// <summary>
        /// 외부 저장 프리셋 파일을 선택하여 현재 관리 중인 기본 디렉토리로 가져오는 함수입니다.
        /// </summary>
        private void ImportFile()
        {
            // 파일 선택 대화상자를 열어 가져올 저장 파일을 선택하도록 합니다.
            string originalFilePath =  EditorUtility.OpenFilePanel("Select save to import", string.Empty, string.Empty);

            // 파일 선택이 취소되었으면 함수를 종료합니다.
            if(originalFilePath.Length == 0)
            {
                return;
            }

            // 선택된 파일 경로에서 파일 이름만 가져옵니다.
            string name = originalFilePath.Substring(originalFilePath.LastIndexOf('/') + 1);

            // 파일 이름에 확장자(.)가 포함되어 있으면 유효하지 않은 프리셋으로 간주하고 오류를 출력합니다.
            if (name.Contains('.'))
            {
                Debug.LogError("Selected invalid save preset.");
                return;
            }

            // 저장 프리셋의 기본 디렉토리 경로를 가져옵니다.
            string defaultDirectoryPath = SavePresets.GetDirectoryPath(SavePresets.DEFAULT_DIRECTORY);

            // 기본 디렉토리가 존재하지 않으면 생성합니다.
            if (!Directory.Exists(defaultDirectoryPath)) //Creating SavePresets folder
            {
                Directory.CreateDirectory(defaultDirectoryPath);
            }

            // 가져올 파일의 새로운 경로를 기본 디렉토리 내에 생성합니다.
            string newFilePath = Path.Combine(defaultDirectoryPath, name);
            // 원본 파일을 새 경로로 복사합니다. 이미 존재하면 덮어씁니다.
            File.Copy(originalFilePath, newFilePath, true);
            // 가져온 저장 파일의 ID를 설정합니다. (이름과 동일하게 설정)
            SavePresets.SetId(name, SavePresets.DEFAULT_DIRECTORY, name);
            // 저장 데이터가 수정되었음을 알리는 플래그를 true로 설정하여 창을 새로고침하도록 합니다.
            SavePresets.saveDataMofied = true;
            // 기본 디렉토리 탭(인덱스 0)을 선택하여 가져온 프리셋이 보이도록 합니다.
            SelectTab(0);
        }

        /// <summary>
        /// 저장 프리셋 목록을 정렬하기 위한 비교 함수입니다.
        /// 먼저 폴더 이름으로 정렬하고, 같은 폴더 내에서는 현재 탭의 정렬 방식(이름 또는 생성 날짜)에 따라 정렬합니다.
        /// </summary>
        /// <param name="x">비교할 첫 번째 SavePreset 객체</param>
        /// <param name="y">비교할 두 번째 SavePreset 객체</param>
        /// <returns>정렬 순서에 따른 정수 값 (음수: x가 y보다 먼저, 0: 순서 동일, 양수: x가 y보다 나중)</returns>
        private int TypeAndCustomSort(SavePreset x, SavePreset y)
        {
            // 먼저 폴더 이름으로 비교하여 정렬합니다.
            int value = x.folderName.CompareTo(y.folderName);

            // 폴더 이름이 같으면 현재 폴더(탭)의 정렬 방식에 따라 추가 정렬을 수행합니다.
            if (value == 0)
            {
                // 정렬 방식이 이름이면 파일 이름으로 비교합니다.
                if (sortTypes[x.folderName] == TabSortType.Name)
                {
                    return x.fileName.CompareTo(y.fileName);
                }
                // 정렬 방식이 생성 날짜이면 생성 날짜로 비교합니다.
                else if (sortTypes[x.folderName] == TabSortType.CreationDate)
                {
                    return x.creationDate.CompareTo(y.creationDate);
                }
                else // 그 외의 경우 (정의되지 않은 정렬 방식 등)
                {
                    return 0; // 순서 변경 없음
                }
            }
            else // 폴더 이름이 다르면 폴더 이름 비교 결과(value)를 반환합니다.
            {
                return value;
            }
        }

        /// <summary>
        /// 지정된 폴더(탭) 이름에 해당하는 저장된 정렬 방식을 EditorPrefs에서 가져오는 함수입니다.
        /// 저장된 값이 없으면 기본 디렉토리의 경우 CreationDate, 그 외의 경우 Name을 기본값으로 반환합니다.
        /// </summary>
        /// <param name="folderName">정렬 방식을 가져올 폴더(탭) 이름 (기본값: SavePresets.DEFAULT_DIRECTORY)</param>
        /// <returns>저장된 또는 기본 정렬 방식</returns>
        private TabSortType GetSortType(string folderName = SavePresets.DEFAULT_DIRECTORY)
        {
            // EditorPrefs에서 지정된 키(SORT_TYPE + folderName)로 저장된 정렬 방식을 int 값으로 가져옵니다.
            // 저장된 값이 없으면 기본값을 사용합니다.
            if(folderName.Equals(SavePresets.DEFAULT_DIRECTORY))
            {
                return (TabSortType)EditorPrefs.GetInt(SORT_TYPE + folderName, (int)TabSortType.CreationDate);
            }
            else
            {
                return (TabSortType)EditorPrefs.GetInt(SORT_TYPE + folderName, (int)TabSortType.Name);
            }
        }

        /// <summary>
        /// 지정된 정렬 방식을 EditorPrefs에 저장하는 함수입니다.
        /// </summary>
        /// <param name="tabSortType">저장할 정렬 방식</param>
        /// <param name="folderName">정렬 방식을 저장할 폴더(탭) 이름 (기본값: SavePresets.DEFAULT_DIRECTORY)</param>
        private void SetSortType(TabSortType tabSortType, string folderName = SavePresets.DEFAULT_DIRECTORY)
        {
            // EditorPrefs에 지정된 키(SORT_TYPE + folderName)로 정렬 방식을 int 값으로 저장합니다.
            EditorPrefs.SetInt(SORT_TYPE + folderName, (int)tabSortType);
        }

        // 저장 프리셋 데이터를 표현하는 내부 클래스입니다.
        private class SavePreset
        {
            // 저장 파일 이름
            public string fileName;
            // 저장 파일 생성 날짜 및 시간 (UTC 기준)
            public DateTime creationDate;
            // 저장 파일의 전체 경로
            public string path;
            // 저장 파일이 속한 폴더(탭) 이름
            public string folderName;

            /// <summary>
            /// SavePreset 클래스의 생성자입니다.
            /// </summary>
            /// <param name="fileName">저장 파일 이름</param>
            /// <param name="creationDate">생성 날짜</param>
            /// <param name="path">파일 경로</param>
            /// <param name="folderName">폴더 이름</param>
            public SavePreset(string fileName, DateTime creationDate, string path, string folderName)
            {
                this.fileName = fileName;
                this.creationDate = creationDate;
                this.path = path;
                this.folderName = folderName;
            }
        }

        // 탭(폴더)별 저장 프리셋 정렬 방식을 정의하는 열거형입니다.
        [System.Serializable]
        private enum TabSortType
        {
            CreationDate = 0, // 생성 날짜 기준 정렬
            Name = 1,         // 이름 기준 정렬
        }
    }
}