// 스크립트 기능 요약:
// 이 스크립트는 Unity 에디터 확장 기능으로, PoolManager 컴포넌트의 인스펙터 창을 사용자 정의하여 표시합니다.
// PoolManager에 등록된 풀 목록을 시각적으로 보여주고, 각 풀의 설정을 편집하며,
// 새로운 풀을 추가하거나 기존 풀을 삭제하는 등의 기능을 GUI로 제공합니다.
// 드래그 앤 드롭으로 풀을 추가하거나, 풀 이름 유효성을 검사하는 기능도 포함합니다.


#pragma warning disable 0414

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace Watermelon
{
    // CustomEditor 속성을 통해 PoolManager 컴포넌트에 대한 사용자 정의 인스펙터임을 명시합니다.
    [CustomEditor(typeof(PoolManager))]
    // sealed internal 클래스로 정의하여 상속을 막고 내부에서만 사용되도록 합니다.
    sealed internal class PoolManagerEditor : Editor
    {
        // poolsListProperty: PoolManager의 poolsList 직렬화된 속성에 대한 참조입니다.
        // 인스펙터에서 풀 목록을 표시하고 편집하는 데 사용됩니다.
        [Tooltip("PoolManager의 풀 목록 직렬화 속성")]
        private SerializedProperty poolsListProperty;
        // isAllPrefabsAssignedAtPoolMethodInfo: PoolManager 클래스의 IsAllPrefabsAssignedAtPool 메서드에 대한 리플렉션 정보입니다.
        // 풀에 모든 프리팹이 할당되었는지 확인하는 데 사용됩니다.
        [Tooltip("풀에 모든 프리팹이 할당되었는지 확인하는 메서드 정보")]
        private MethodInfo isAllPrefabsAssignedAtPoolMethodInfo;
        // recalculateWeightsAtPoolMethodInfo: PoolManager 클래스의 RecalculateWeightsAtPool 메서드에 대한 리플렉션 정보입니다.
        // 다중 풀의 가중치를 재계산하는 데 사용됩니다.
        [Tooltip("다중 풀의 가중치를 재계산하는 메서드 정보")]
        private MethodInfo recalculateWeightsAtPoolMethodInfo;

        // newPoolBuilder: 새로운 풀 생성 시 임시로 정보를 저장하는 객체입니다. (현재 코드에서는 주석 처리됨)
        //private PoolSettings newPoolBuilder;
        // inspectorRect: 인스펙터 창 전체의 GUI 영역을 나타내는 Rect입니다.
        [Tooltip("인스펙터 창 전체 GUI 영역")]
        private Rect inspectorRect = new Rect();
        // dragAndDropRect: 드래그 앤 드롭 이벤트를 감지할 GUI 영역을 나타내는 Rect입니다. (현재 코드에서는 주석 처리됨)
        [Tooltip("드래그 앤 드롭 감지 GUI 영역")]
        private Rect dragAndDropRect = new Rect();

        // isNameAllowed: 현재 입력된 풀 이름이 유효한지 여부를 나타내는 플래그입니다.
        [Tooltip("입력된 풀 이름의 유효성 여부")]
        private bool isNameAllowed = true;
        // isNameAlreadyExisting: 현재 입력된 풀 이름이 이미 존재하는지 여부를 나타내는 플래그입니다.
        [Tooltip("입력된 풀 이름의 중복 여부")]
        private bool isNameAlreadyExisting = false;
        // isSettingsExpanded: 인스펙터에서 설정 섹션이 확장되었는지 여부를 나타내는 플래그입니다. (현재 코드에서는 사용되지 않음)
        [Tooltip("설정 섹션 확장 여부")]
        private bool isSettingsExpanded = false;
        // dragAndDropActive: 드래그 앤 드롭 작업이 활성화되었는지 여부를 나타내는 플래그입니다. (현재 코드에서는 주석 처리됨)
        [Tooltip("드래그 앤 드롭 활성화 여부")]
        private bool dragAndDropActive = false;
        // skipEmptyNameWarning: 빈 이름 경고를 건너뛸지 여부를 나타내는 플래그입니다. (현재 코드에서는 사용되지 않음)
        [Tooltip("빈 이름 경고 건너뛰기 여부")]
        private bool skipEmptyNameWarning = false;

        // selectedPoolIndex: 현재 인스펙터에서 선택된 풀의 인덱스입니다.
        [Tooltip("현재 선택된 풀의 인덱스")]
        private int selectedPoolIndex;

        // POOLS_LIST_PROPERTY_NAME: poolsList 직렬화 속성의 이름 상수입니다.
        private const string POOLS_LIST_PROPERTY_NAME = "poolsList";
        // RENAMING_EMPTY_STRING: 이름 변경 중 빈 상태를 나타내는 상수 문자열입니다.
        private const string RENAMING_EMPTY_STRING = "[PoolManager: empty]";
        // EMPTY_POOL_BUILDER_NAME: 새로운 풀 빌더의 빈 상태를 나타내는 상수 문자열입니다. (현재 코드에서는 주석 처리됨)
        private const string EMPTY_POOL_BUILDER_NAME = "[PoolBuilder: empty]";

        // searchText: 풀 목록 검색에 사용되는 입력 문자열입니다.
        [Tooltip("풀 목록 검색어")]
        private string searchText = string.Empty;
        // prevNewPoolName: 새로운 풀 생성 시 이전 이름 저장 변수입니다. (현재 코드에서는 사용되지 않음)
        [Tooltip("새 풀 생성 시 이전 이름")]
        private string prevNewPoolName = string.Empty;
        // prevSelectedPoolName: 선택된 풀의 이전 이름 저장 변수입니다. (현재 코드에서는 사용되지 않음)
        [Tooltip("선택된 풀의 이전 이름")]
        private string prevSelectedPoolName = string.Empty;
        // lastRenamingName: 마지막으로 이름 변경을 시도한 문자열입니다.
        [Tooltip("마지막 이름 변경 시도 문자열")]
        private string lastRenamingName = string.Empty;

        // defaultColor: GUI 기본 색상 저장 변수입니다.
        [Tooltip("GUI 기본 색상")]
        private Color defaultColor;

        // boldStyle: 굵은 글꼴 스타일입니다.
        [Tooltip("굵은 글꼴 GUI 스타일")]
        private GUIStyle boldStyle = new GUIStyle();
        // headerStyle: 헤더 텍스트 스타일입니다.
        [Tooltip("헤더 GUI 스타일")]
        private GUIStyle headerStyle = new GUIStyle();
        // bigHeaderStyle: 큰 헤더 텍스트 스타일입니다.
        [Tooltip("큰 헤더 GUI 스타일")]
        private GUIStyle bigHeaderStyle = new GUIStyle();
        // centeredTextStyle: 가운데 정렬된 텍스트 스타일입니다.
        [Tooltip("가운데 정렬 텍스트 GUI 스타일")]
        private GUIStyle centeredTextStyle = new GUIStyle();
        // multiListLablesStyle: 다중 풀 목록 라벨 스타일입니다.
        [Tooltip("다중 풀 목록 라벨 GUI 스타일")]
        private GUIStyle multiListLablesStyle = new GUIStyle();
        // dragAndDropBoxStyle: 드래그 앤 드롭 영역 박스 스타일입니다. (현재 코드에서는 주석 처리됨)
        [Tooltip("드래그 앤 드롭 박스 GUI 스타일")]
        private GUIStyle dragAndDropBoxStyle = new GUIStyle();

        // warningIconGUIContent: 경고 아이콘 GUI 콘텐츠입니다.
        [Tooltip("경고 아이콘 GUI 콘텐츠")]
        private GUIContent warningIconGUIContent;
        // lockedIconGUIContent: 잠금 아이콘 GUI 콘텐츠입니다.
        [Tooltip("잠금 아이콘 GUI 콘텐츠")]
        private GUIContent lockedIconGUIContent;
        // unlockedIconGUIContent: 잠금 해제 아이콘 GUI 콘텐츠입니다.
        [Tooltip("잠금 해제 아이콘 GUI 콘텐츠")]
        private GUIContent unlockedIconGUIContent;

        /// <summary>
        /// 인스펙터 창이 활성화될 때 호출됩니다.
        /// 직렬화된 속성을 찾고, 리플렉션을 통해 메서드 정보를 가져오며, 스타일 및 변수들을 초기화합니다.
        /// </summary>
        private void OnEnable()
        {
            // PoolManager의 poolsList 직렬화 속성을 찾습니다.
            poolsListProperty = serializedObject.FindProperty(POOLS_LIST_PROPERTY_NAME);
            // 리플렉션을 사용하여 PoolManager의 IsAllPrefabsAssignedAtPool 메서드 정보를 가져옵니다.
            isAllPrefabsAssignedAtPoolMethodInfo = serializedObject.targetObject.GetType().GetMethod("IsAllPrefabsAssignedAtPool", BindingFlags.NonPublic | BindingFlags.Instance);
            // 리플렉션을 사용하여 PoolManager의 RecalculateWeightsAtPool 메서드 정보를 가져옵니다.
            recalculateWeightsAtPoolMethodInfo = serializedObject.targetObject.GetType().GetMethod("RecalculateWeightsAtPool", BindingFlags.NonPublic | BindingFlags.Instance);

            // 이름 변경 관련 변수들을 초기화합니다.
            lastRenamingName = RENAMING_EMPTY_STRING;
            isNameAllowed = true;
            isNameAlreadyExisting = false;

            // 선택된 풀 인덱스를 초기화합니다.
            selectedPoolIndex = -1;
            // newPoolBuilder를 초기 상태로 초기화합니다. (현재 주석 처리됨)
            //newPoolBuilder = new PoolSettings().SetName(EMPTY_POOL_BUILDER_NAME);

            // GUI 스타일들을 준비합니다.
            PrepareStyles();
        }

        /// <summary>
        /// 인스펙터에서 사용할 다양한 GUI 스타일들을 설정합니다.
        /// 글꼴 스타일, 정렬, 색상, 아이콘 등을 로드하고 설정합니다.
        /// </summary>
        private void PrepareStyles()
        {
            // 에디터 스킨에 따라 라벨 색상을 설정합니다.
            Color labelColor = EditorGUIUtility.isProSkin ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.12f, 0.12f, 0.12f);

            // 굵은 글꼴 스타일을 설정합니다.
            boldStyle.fontStyle = FontStyle.Bold;

            // 헤더 스타일을 설정합니다.
            headerStyle = new GUIStyle(EditorCustomStyles.Skin.label);
            headerStyle.normal.textColor = labelColor;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 12;

            // 큰 헤더 스타일을 설정합니다.
            bigHeaderStyle = new GUIStyle(EditorCustomStyles.Skin.label);
            bigHeaderStyle.normal.textColor = labelColor;
            bigHeaderStyle.normal.textColor = labelColor; // 중복 설정
            bigHeaderStyle.alignment = TextAnchor.MiddleCenter;
            bigHeaderStyle.fontStyle = FontStyle.Bold;
            bigHeaderStyle.fontSize = 14;

            // 가운데 정렬 텍스트 스타일을 설정합니다.
            centeredTextStyle = new GUIStyle(EditorCustomStyles.Skin.label);
            centeredTextStyle.normal.textColor = labelColor;
            centeredTextStyle.alignment = TextAnchor.MiddleCenter;

            // 다중 풀 목록 라벨 스타일을 설정합니다.
            multiListLablesStyle.fontSize = 8;
            multiListLablesStyle.normal.textColor = labelColor;

            // 경고 아이콘 텍스처를 로드하고 GUIContent를 생성합니다.
            Texture warningIconTexture = EditorCustomStyles.GetIcon("icon_warning");
            warningIconGUIContent = new GUIContent(warningIconTexture);

            // 잠금 아이콘 텍스처를 로드하고 GUIContent를 생성합니다.
            Texture lockedTexture = EditorCustomStyles.GetIcon("icon_locked");
            lockedIconGUIContent = new GUIContent(lockedTexture);

            // 잠금 해제 아이콘 텍스처를 로드하고 GUIContent를 생성합니다.
            Texture unlockedTexture = EditorCustomStyles.GetIcon("icon_unlocked");
            unlockedIconGUIContent = new GUIContent(unlockedTexture);

            // GUI 기본 색상을 저장합니다.
            defaultColor = GUI.contentColor;
        }


        /// <summary>
        /// 인스펙터 창의 GUI를 그리는 함수입니다.
        /// PoolManager의 풀 목록을 표시하고 편집하는 인터페이스를 제공합니다.
        /// 드래그 앤 드롭 영역, 검색 필드, 풀 목록, 각 풀의 세부 정보 등을 그립니다.
        /// </summary>

        public override void OnInspectorGUI()
        {
            // 직렬화된 객체를 업데이트하여 최신 상태를 반영합니다.
            serializedObject.Update();

            // 드래그 앤 드롭 영역이 활성화된 경우 해당 영역을 그립니다. (현재 주석 처리된 기능)
            if (dragAndDropActive)
            {
                dragAndDropBoxStyle = GUI.skin.box;
                dragAndDropBoxStyle.alignment = TextAnchor.MiddleCenter;
                dragAndDropBoxStyle.fontStyle = FontStyle.Bold;
                dragAndDropBoxStyle.fontSize = 12;

                GUILayout.Box("Drag objects here", dragAndDropBoxStyle, GUILayout.Width(EditorGUIUtility.currentViewWidth - 21), GUILayout.Height(inspectorRect.size.y));
            }
            else // 일반 인스펙터 GUI를 그립니다.
            {
                // 인스펙터 GUI 영역의 시작을 기록합니다.
                inspectorRect = EditorGUILayout.BeginVertical();

                // 제어 바 영역 (설정 및 풀 추가 버튼 등) (현재 주석 처리된 기능 포함)
                EditorGUILayout.BeginVertical(GUI.skin.box);

                //// if we are not setuping a new pool now - than displaying settings interface
                //if (newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME))
                //{
                //    EditorGUI.indentLevel++;

                //    isSettingsExpanded = EditorGUILayout.Foldout(isSettingsExpanded, "Settings");

                //    if (isSettingsExpanded)
                //    {
                //        EditorGUI.BeginChangeCheck();

                //        // [CACHE IS CURRENTLY DISABLED]
                //        //poolManagerRef.useCache = EditorGUILayout.Toggle("Use cache :", poolManagerRef.useCache);

                //        if (EditorGUI.EndChangeCheck())
                //        {
                //            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                //        }

                //        EditorGUILayout.Space();
                //    }

                //    EditorGUI.indentLevel--;


                //    if (GUILayout.Button("Add pool", GUILayout.Height(30)))
                //    {
                //        skipEmptyNameWarning = true;
                //        //AddNewSinglePool();
                //    }
                //}

                //// Pool creation bar //////////////////////////////////////////////////////////////////////////
                //if (!newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME))
                //{
                //    //EditorGUILayout.BeginVertical(GUI.skin.box);

                //    GUILayout.Space(3f);
                //    EditorGUILayout.BeginHorizontal();
                //    EditorGUILayout.LabelField("Pool creation:", headerStyle, GUILayout.Width(100));
                //    GUILayout.FlexibleSpace();
                //    if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                //    {
                //        CancelNewPoolCreation();

                //        return;
                //    }
                //    EditorGUILayout.EndHorizontal();

                //    GUILayout.Space(4f);

                //    //newPoolBuilder = DrawPool(newPoolBuilder, null, 0);

                //    GUILayout.Space(5f);

                //    if (GUILayout.Button("Confirm", GUILayout.Height(25)))
                //    {
                //        GUI.FocusControl(null);
                //        ConfirmPoolCreation();

                //        return;
                //    }

                //    GUILayout.Space(5f);
                //    //EditorGUILayout.EndVertical();
                //}

                EditorGUILayout.EndVertical();


                // Pools displaying region /////////////////////////////////////////////////////////////////////

                EditorGUILayout.BeginVertical();

                 // "Pools list" 헤더를 그립니다.
                EditorGUILayout.LabelField("Pools list", headerStyle);

                GUILayout.BeginHorizontal();

                // 검색 필드를 그립니다.
                searchText = EditorGUILayout.TextField(searchText, GUI.skin.FindStyle("ToolbarSearchTextField"));

                // 검색어 입력 시 검색 취소 버튼을 그립니다.
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                    {
                        searchText = ""; // 검색어 초기화
                        GUI.FocusControl(null); // GUI 포커스 해제
                    }
                }
                else // 검색어가 없을 때 빈 검색 취소 버튼을 그립니다.
                {
                    GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSearchCancelButtonEmpty"));
                }

                GUILayout.EndHorizontal();

                // 풀 목록이 비어있는 경우 메시지를 표시합니다.
                if (poolsListProperty.arraySize == 0)
                {
                    if (string.IsNullOrEmpty(searchText))
                    {
                        EditorGUILayout.HelpBox("풀이 없습니다.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("풀 \"" + searchText + "\"를 찾을 수 없습니다.", MessageType.Info);
                    }
                }
                else // 풀 목록이 비어있지 않으면 각 풀을 그립니다.
                {
                    // poolsListProperty.arraySize만큼 반복하여 각 풀 항목을 그립니다.
                    for (int currentPoolIndex = 0; currentPoolIndex < poolsListProperty.arraySize; currentPoolIndex++)
                    {
                        // 현재 풀 항목의 SerializedProperty를 가져옵니다.
                        SerializedProperty poolProperty = poolsListProperty.GetArrayElementAtIndex(currentPoolIndex);

                        // 검색어가 비어있거나 (전체 표시) 검색어와 풀 이름이 일치하는 경우에만 그립니다.
                        if (searchText == string.Empty || (searchText != string.Empty && poolProperty.FindPropertyRelative("name").stringValue.Contains(searchText)))
                        {
                            // 각 풀 항목의 클릭 가능한 영역을 시작합니다.
                            Rect clickRect = EditorGUILayout.BeginVertical(GUI.skin.box);
                            EditorGUI.indentLevel++; // 인스펙트 레벨 증가

                            // 현재 풀이 선택되지 않았거나 다른 풀이 선택된 경우 요약 정보를 표시합니다.
                            if (selectedPoolIndex == -1 || currentPoolIndex != selectedPoolIndex)
                            {
                                // 다른 풀이 선택된 경우 새로운 풀 생성 상태를 취소합니다.
                                if (selectedPoolIndex != -1)
                                {
                                    CancelNewPoolCreation();
                                }

                                // 풀에 모든 프리팹이 할당되었는지 확인하고 상태에 따라 표시를 변경합니다.
                                if ((bool)isAllPrefabsAssignedAtPoolMethodInfo.Invoke(serializedObject.targetObject, new object[] { currentPoolIndex }))
                                {
                                    // 모든 프리팹이 할당되었으면 풀 이름과 런타임 생성 여부를 표시합니다.
                                    string runtimeCreatedNameAddition = poolProperty.FindPropertyRelative("isRuntimeCreated").boolValue ? "   [Runtime]" : "";
                                    EditorGUILayout.LabelField(GetPoolName(currentPoolIndex) + runtimeCreatedNameAddition, centeredTextStyle);
                                }
                                else
                                {
                                    // 프리팹 할당이 불완전하면 경고 아이콘과 풀 이름을 표시합니다.
                                    EditorGUILayout.BeginHorizontal();

                                    GUI.contentColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                                    EditorGUILayout.LabelField(warningIconGUIContent, GUILayout.Width(30));
                                    GUI.contentColor = defaultColor;

                                    GUILayout.Space(-35f);
                                    EditorGUILayout.LabelField(GetPoolName(currentPoolIndex), centeredTextStyle);

                                    EditorGUILayout.EndHorizontal();
                                }

                            }
                            else // 현재 풀이 선택된 경우 상세 정보를 표시합니다.
                            {
                                GUILayout.Space(5);

                                // 풀 상세 정보를 그립니다. (현재 PoolPropertyDrawer 또는 PoolMultiplePropertyDrawer에서 처리됨)
                                //DrawPool(newPoolBuilder, poolProperty, currentPoolIndex);

                                GUILayout.Space(5);

                                // 캐시 시스템 관련 영역 (현재 주석 처리됨)
                                // [CURRENTLY DISABLED]
                                //if (poolManagerRef.useCache && poolsCacheList[currentPoolIndex] != null)
                                //{
                                //    EditorGUI.BeginChangeCheck();
                                //    poolsCacheList[currentPoolIndex].ignoreCache = EditorGUILayout.Toggle("Ignore cache: ", poolsCacheList[currentPoolIndex].ignoreCache);

                                //    if (EditorGUI.EndChangeCheck())
                                //    {
                                //        UpdateIgnoreCacheStateOfPool(poolsCacheList[currentPoolIndex].poolName, poolsCacheList[currentPoolIndex].ignoreCache);
                                //        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                //    }
                                //}

                                //if (poolManagerRef.useCache && poolsCacheDeltaList[currentPoolIndex] != 0 && poolsCacheList[currentPoolIndex] != null)
                                //{
                                //    if (poolsCacheList[currentPoolIndex].ignoreCache)
                                //    {
                                //        GUI.enabled = false;
                                //        EditorGUILayout.LabelField("Cached value: " + poolsCacheList[currentPoolIndex].poolSize);
                                //        GUI.enabled = true;
                                //    }
                                //    else
                                //    {
                                //        if (GUILayout.Button("Apply cache: " + (pool.Size + poolsCacheDeltaList[currentPoolIndex])))
                                //        {
                                //            Undo.RecordObject(target, "Apply cache");

                                //            poolManagerRef.pools[currentPoolIndex].Size = poolsCacheList[currentPoolIndex].poolSize;

                                //            ClearObsoleteCache();
                                //            UpdateCacheStateList();
                                //        }
                                //    }
                                //}

                                // 삭제 버튼을 그립니다.
                                if (GUILayout.Button("Delete"))
                                {
                                    // 삭제 확인 대화 상자를 표시합니다.
                                    if (EditorUtility.DisplayDialog("이 풀이 제거됩니다!", "정말로 제거하시겠습니까?", "제거", "취소"))
                                    {
                                        DeletePool(currentPoolIndex); // 풀 삭제

                                        // 삭제 후 프로젝트 창에 포커스를 맞춥니다.
                                        EditorApplication.delayCall += delegate
                                        {
                                            EditorUtility.FocusProjectWindow();
                                        };
                                    }
                                }

                                GUILayout.Space(5);
                            }

                            EditorGUI.indentLevel--; // 인스펙트 레벨 감소
                            EditorGUILayout.EndVertical(); // 풀 항목 영역 끝

                            // 풀 항목 클릭 이벤트를 처리합니다.
                            if (GUI.Button(clickRect, GUIContent.none, GUIStyle.none))
                            {
                                GUI.FocusControl(null); // GUI 포커스 해제

                                // 클릭된 풀을 선택하거나 선택 해제합니다.
                                if (selectedPoolIndex == -1 || selectedPoolIndex != currentPoolIndex)
                                {
                                    selectedPoolIndex = currentPoolIndex; // 현재 풀 선택
                                    lastRenamingName = RENAMING_EMPTY_STRING; // 이름 변경 상태 초기화
                                    isNameAlreadyExisting = false;
                                    isNameAllowed = true;
                                    // newPoolBuilder를 초기 상태로 초기화합니다. (현재 주석 처리됨)
                                    //newPoolBuilder = newPoolBuilder.Reset().SetName(EMPTY_POOL_BUILDER_NAME);
                                }
                                else
                                {
                                    selectedPoolIndex = -1; // 풀 선택 해제
                                    lastRenamingName = RENAMING_EMPTY_STRING; // 이름 변경 상태 초기화
                                    isNameAlreadyExisting = false;
                                    isNameAllowed = true;
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndVertical(); // 풀 목록 표시 영역 끝

                // GUI 변경 사항이 있으면 현재 타겟 객체(PoolManager)를 수정됨으로 표시합니다.
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndVertical(); // 일반 인스펙터 GUI 영역 끝

            }

            // 직렬화된 객체에 적용된 변경 사항을 저장합니다.
            serializedObject.ApplyModifiedProperties();

            // 드래그 앤 드롭 관련 영역 (현재 주석 처리됨)///////////////////////////////////////////////
            //Event currentEvent = Event.current;

            //if (inspectorRect.Contains(currentEvent.mousePosition) && selectedPoolIndex == -1 && newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME) && !isSettingsExpanded)
            //{
            //    if (currentEvent.type == EventType.DragUpdated)
            //    {
            //        dragAndDropActive = true;
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            //        currentEvent.Use();
            //    }
            //    else if (currentEvent.type == EventType.DragPerform)
            //    {
            //        dragAndDropActive = false;
            //        List<Pool.MultiPoolPrefab> draggedObjects = new List<Pool.MultiPoolPrefab>();

            //        foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            //        {
            //            if (obj.GetType() == typeof(GameObject))
            //            {
            //                draggedObjects.Add(new Pool.MultiPoolPrefab(obj as GameObject, 0, false));
            //            }
            //        }

            //        if (draggedObjects.Count == 1)
            //        {
            //            AddNewSinglePool(draggedObjects[0].prefab);
            //        }
            //        else
            //        {
            //            AddNewMultiPool(draggedObjects);
            //        }

            //        currentEvent.Use();
            //    }
            //}
            //else
            //{
            //    if (currentEvent.type == EventType.Repaint)
            //    {
            //        dragAndDropActive = false;
            //    }
            //}

        }


        //private PoolSettings DrawPool(PoolSettings poolBuilder, SerializedProperty poolProperty, int poolIndex)
        //{
        //    EditorGUI.BeginChangeCheck();

        //    // name ///////////
        //    string poolName = poolProperty != null ? poolProperty.FindPropertyRelative("name").stringValue : poolBuilder.name;

        //    GUILayout.BeginHorizontal();

        //    string newName = EditorGUILayout.TextField("Name: ", lastRenamingName != RENAMING_EMPTY_STRING ? lastRenamingName : poolName);

        //    if (newName == poolName && (!newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME) ? newName.Equals(string.Empty) : true))
        //    {
        //        lastRenamingName = RENAMING_EMPTY_STRING;
        //        isNameAllowed = true;
        //        isNameAlreadyExisting = false;
        //    }

        //    if (!isNameAllowed || newName == string.Empty || newName != poolName || lastRenamingName != RENAMING_EMPTY_STRING)
        //    {
        //        lastRenamingName = newName;
        //        isNameAllowed = IsNameAllowed(newName);

        //        EditorGUI.BeginDisabledGroup(!isNameAllowed);

        //        // if name is emplty or it's pool creation - do not show rename button

        //        if (!(!isNameAllowed && !isNameAlreadyExisting || !newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME)))
        //        {
        //            if (GUILayout.Button("rename"))
        //            {
        //                //RenamePool(poolProperty, poolBuilder, newName);

        //                lastRenamingName = RENAMING_EMPTY_STRING;
        //            }
        //        }

        //        EditorGUI.EndDisabledGroup();
        //        GUILayout.EndHorizontal();

        //        if (isNameAllowed)
        //        {
        //            // [CACHE IS CURRENTLY DISABLED]
        //            //if (poolManagerRef.useCache)
        //            //{
        //            //    RenameCachedPool(poolName, newName);
        //            //}
        //        }
        //        else
        //        {
        //            if (isNameAlreadyExisting)
        //            {
        //                EditorGUILayout.HelpBox("Name already exists", MessageType.Warning);
        //            }
        //            else
        //            {
        //                if (!skipEmptyNameWarning)
        //                {
        //                    EditorGUILayout.HelpBox("Name can't be empty", MessageType.Warning);
        //                }
        //            }
        //        }

        //    }
        //    else
        //    {
        //        EditorGUILayout.EndHorizontal();
        //    }

        //    if (!poolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME))
        //    {
        //        poolBuilder = poolBuilder.SetName(newName);
        //    }



        //    // type ///////////
        //    //Pool.PoolType poolType = poolProperty != null ? (Pool.PoolType)poolProperty.FindPropertyRelative("type").enumValueIndex : poolBuilder.type;
        //    //Pool.PoolType currentPoolType = (Pool.PoolType)EditorGUILayout.EnumPopup("Pool type:", poolType);

        //    //if (currentPoolType != poolType)
        //    //{
        //    //    if (poolProperty != null)
        //    //    {
        //    //        poolProperty.FindPropertyRelative("type").enumValueIndex = (int)currentPoolType;
        //    //    }
        //    //    else
        //    //    {
        //    //        poolBuilder = poolBuilder.SetType(currentPoolType);
        //    //    }
        //    //}

        //    // prefabs field ///////////
        //    //if (currentPoolType == Pool.PoolType.Single)
        //    if(true)
        //    {
        //        // single prefab pool editor
        //        GameObject currentPrefab = poolProperty != null ? (GameObject)poolProperty.FindPropertyRelative("singlePoolPrefab").objectReferenceValue : poolBuilder.singlePoolPrefab;
        //        GameObject prefab = (GameObject)EditorGUILayout.ObjectField("Prefab: ", currentPrefab, typeof(GameObject), false);

        //        if (currentPrefab != prefab)
        //        {
        //            if (poolProperty != null)
        //            {
        //                poolProperty.FindPropertyRelative("singlePoolPrefab").objectReferenceValue = prefab;
        //            }
        //            else
        //            {
        //                poolBuilder = poolBuilder.SetSinglePrefab(prefab);
        //            }

        //            string currentName = poolProperty != null ? poolProperty.FindPropertyRelative("name").stringValue : poolBuilder.name;

        //            if (currentName == string.Empty)
        //            {
        //                //RenamePool(poolProperty, poolBuilder, prefab.name);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // multiple prefabs pool editor
        //        GUILayout.Space(5f);

        //        int currentPrefabsAmount = poolProperty != null ? poolProperty.FindPropertyRelative("multiPoolPrefabsList").arraySize : poolBuilder.multiPoolPrefabsList.Count;

        //        EditorGUILayout.BeginHorizontal();

        //        EditorGUI.BeginDisabledGroup(true);
        //        EditorGUILayout.IntField("Prefabs amount:", currentPrefabsAmount);
        //        EditorGUI.EndDisabledGroup();

        //        int newPrefabsAmount = currentPrefabsAmount;

        //        if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)) && newPrefabsAmount > 0)
        //        {
        //            GUI.FocusControl(null);
        //            newPrefabsAmount--;
        //        }

        //        if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
        //        {
        //            GUI.FocusControl(null);
        //            newPrefabsAmount++;
        //        }

        //        EditorGUILayout.EndHorizontal();

        //        if (newPrefabsAmount != currentPrefabsAmount)
        //        {
        //            if (poolProperty != null)
        //            {
        //                poolProperty.FindPropertyRelative("multiPoolPrefabsList").arraySize = newPrefabsAmount;
        //            }
        //            else
        //            {
        //                if (newPrefabsAmount == 0)
        //                {
        //                    poolBuilder.multiPoolPrefabsList.Clear();
        //                }
        //                else if (newPrefabsAmount < poolBuilder.multiPoolPrefabsList.Count)
        //                {
        //                    int itemsToRemove = poolBuilder.multiPoolPrefabsList.Count - newPrefabsAmount;
        //                    poolBuilder.multiPoolPrefabsList.RemoveRange(poolBuilder.multiPoolPrefabsList.Count - itemsToRemove - 1, itemsToRemove);
        //                }
        //                else
        //                {
        //                    int itemsToAdd = newPrefabsAmount - poolBuilder.multiPoolPrefabsList.Count;
        //                    for (int j = 0; j < itemsToAdd; j++)
        //                    {
        //                        //poolBuilder.multiPoolPrefabsList.Add(new Pool.MultiPoolPrefab());
        //                    }
        //                }
        //            }

        //            if (poolProperty != null)
        //            {
        //                if (newPrefabsAmount > currentPrefabsAmount)
        //                {
        //                    for (int i = 0; i < newPrefabsAmount - currentPrefabsAmount; i++)
        //                    {
        //                        poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(currentPrefabsAmount + i).FindPropertyRelative("prefab").objectReferenceValue = null;
        //                        poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(currentPrefabsAmount + i).FindPropertyRelative("weight").intValue = 0;
        //                        poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(currentPrefabsAmount + i).FindPropertyRelative("isWeightLocked").boolValue = false;
        //                    }
        //                }

        //                serializedObject.ApplyModifiedProperties();
        //                recalculateWeightsAtPoolMethodInfo.Invoke(serializedObject.targetObject, new object[] { poolIndex });
        //            }
        //            else
        //            {
        //                poolBuilder.RecalculateWeights();
        //            }

        //            currentPrefabsAmount = newPrefabsAmount;
        //        }

        //        // prefabs list
        //        GUILayout.Space(-2f);
        //        EditorGUILayout.BeginHorizontal();
        //        EditorGUILayout.LabelField("objects", multiListLablesStyle, GUILayout.MaxHeight(10f));
        //        GUILayout.Space(-25);
        //        EditorGUILayout.LabelField("weights", multiListLablesStyle, GUILayout.Width(75), GUILayout.MaxHeight(10f));
        //        EditorGUILayout.EndHorizontal();
        //        float weightsSum = 0f;

        //        for (int j = 0; j < currentPrefabsAmount; j++)
        //        {
        //            EditorGUILayout.BeginHorizontal();

        //            // object 
        //            GameObject currentPrefab = poolProperty != null ? (GameObject)poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(j).FindPropertyRelative("prefab").objectReferenceValue : poolBuilder.multiPoolPrefabsList[j].prefab;
        //            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField(currentPrefab, typeof(GameObject), true);

        //            if (newPrefab != currentPrefab)
        //            {
        //                if (poolProperty != null)
        //                {
        //                    poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(j).FindPropertyRelative("prefab").objectReferenceValue = newPrefab;
        //                }
        //                else
        //                {
        //                    //poolBuilder.multiPoolPrefabsList[j] = new Pool.MultiPoolPrefab(newPrefab, poolBuilder.multiPoolPrefabsList[j].weight, poolBuilder.multiPoolPrefabsList[j].isWeightLocked);
        //                }
        //            }

        //            // weight
        //            bool isWeightLocked = poolProperty != null ? poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(j).FindPropertyRelative("isWeightLocked").boolValue : poolBuilder.multiPoolPrefabsList[j].isWeightLocked;
        //            EditorGUI.BeginDisabledGroup(isWeightLocked);

        //            int currentWeight = poolProperty != null ? poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(j).FindPropertyRelative("weight").intValue : poolBuilder.multiPoolPrefabsList[j].weight;


        //            int newWeight = EditorGUILayout.DelayedIntField(Math.Abs(currentWeight), GUILayout.Width(75));
        //            if (newWeight != currentWeight)
        //            {
        //                if (poolProperty != null)
        //                {
        //                    poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(j).FindPropertyRelative("weight").intValue = newWeight;
        //                }
        //                else
        //                {
        //                    //poolBuilder.multiPoolPrefabsList[j] = new Pool.MultiPoolPrefab(newPrefab, newWeight, poolBuilder.multiPoolPrefabsList[j].isWeightLocked);
        //                }
        //            }

        //            EditorGUI.EndDisabledGroup();

        //            weightsSum += newWeight;

        //            // lock
        //            GUI.contentColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        //            if (GUILayout.Button(isWeightLocked ? lockedIconGUIContent : unlockedIconGUIContent, centeredTextStyle, GUILayout.Height(13f), GUILayout.Width(13f)))
        //            {
        //                GUI.FocusControl(null);

        //                if (poolProperty != null)
        //                {
        //                    poolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(j).FindPropertyRelative("isWeightLocked").boolValue = !isWeightLocked;
        //                }
        //                else
        //                {
        //                    //poolBuilder.multiPoolPrefabsList[j] = new Pool.MultiPoolPrefab(newPrefab, poolBuilder.multiPoolPrefabsList[j].weight, !poolBuilder.multiPoolPrefabsList[j].isWeightLocked);
        //                }
        //            }
        //            GUI.contentColor = defaultColor;

        //            EditorGUILayout.EndHorizontal();
        //        }

        //        GUILayout.Space(5f);

        //        if (currentPrefabsAmount != 0 && weightsSum != 100)
        //        {
        //            EditorGUILayout.BeginHorizontal();

        //            EditorGUILayout.HelpBox("Weights sum should be 100 (current " + weightsSum + ").", MessageType.Warning);

        //            if (GUILayout.Button("Recalculate", GUILayout.Height(40f), GUILayout.Width(76)))
        //            {
        //                GUI.FocusControl(null);

        //                recalculateWeightsAtPoolMethodInfo.Invoke(serializedObject.targetObject, new object[] { poolIndex });

        //                // pool.RecalculateWeights();
        //            }

        //            EditorGUILayout.EndHorizontal();
        //        }
        //    }

        //    if (!(bool)isAllPrefabsAssignedAtPoolMethodInfo.Invoke(serializedObject.targetObject, new object[] { poolIndex }) && newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME))
        //    {
        //        EditorGUILayout.HelpBox("Please assign all prefabs references.", MessageType.Warning);
        //    }


        //    // pool size ///////////
        //    int currentSize = poolProperty != null ? poolProperty.FindPropertyRelative("size").intValue : poolBuilder.size;

        //    //if (currentPoolType == Pool.PoolType.Single)
        //    //{
        //    //    int newSize = EditorGUILayout.IntField("Pool size: ", currentSize);

        //    //    if (poolProperty != null)
        //    //    {
        //    //        poolProperty.FindPropertyRelative("size").intValue = newSize;
        //    //    }
        //    //    else
        //    //    {
        //    //        poolBuilder.size = newSize;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    EditorGUILayout.BeginHorizontal();

        //    //    int newSize = EditorGUILayout.IntField("Pool size: ", currentSize);

        //    //    newSize = newSize >= 0 ? newSize : 0;

        //    //    if (poolProperty != null)
        //    //    {
        //    //        poolProperty.FindPropertyRelative("size").intValue = newSize;
        //    //    }
        //    //    else
        //    //    {
        //    //        poolBuilder.size = newSize;
        //    //    }

        //    //    GUILayout.FlexibleSpace();

        //    //    int multiPrefabsAmount = poolProperty != null ? poolProperty.FindPropertyRelative("multiPoolPrefabsList").arraySize : poolBuilder.multiPoolPrefabsList != null ? poolBuilder.multiPoolPrefabsList.Count : 0;
        //    //    string lableString = " x " + multiPrefabsAmount + " = " + (newSize * multiPrefabsAmount);
        //    //    GUILayout.Space(-18);
        //    //    EditorGUILayout.LabelField(lableString);

        //    //    EditorGUILayout.EndHorizontal();
        //    //}

        //    // [CACHE IS CURRENTLY DISABLED]
        //    //if (poolManagerRef.useCache && currentSize != poolBuilder.size)
        //    //{
        //    //    UpdateCacheStateList();
        //    //}

        //    // auto size increment toggle ///////////
        //    bool currentAutoSizeIncrementState = poolProperty != null ? poolProperty.FindPropertyRelative("autoSizeIncrement").boolValue : poolBuilder.autoSizeIncrement;
        //    bool newAutoSizeIncrementState = EditorGUILayout.Toggle("Will grow: ", currentAutoSizeIncrementState);

        //    if (poolProperty != null)
        //    {
        //        poolProperty.FindPropertyRelative("autoSizeIncrement").boolValue = newAutoSizeIncrementState;
        //    }
        //    else
        //    {
        //        poolBuilder.autoSizeIncrement = newAutoSizeIncrementState;
        //    }

        //    // objects parrent ///////////
        //    Transform currentContainer = poolProperty != null ? (Transform)poolProperty.FindPropertyRelative("objectsContainer").objectReferenceValue : poolBuilder.objectsContainer;
        //    Transform newContainer = (Transform)EditorGUILayout.ObjectField("Objects parrent", currentContainer, typeof(Transform), true);

        //    if (poolProperty != null)
        //    {
        //        poolProperty.FindPropertyRelative("objectsContainer").objectReferenceValue = newContainer;
        //    }
        //    else
        //    {
        //        poolBuilder.objectsContainer = newContainer;
        //    }


        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        //    }

        //    return poolBuilder;
        //}

        //private void RenamePool(SerializedProperty poolProperty, PoolSettings poolBuilder, string newName)
        //{
        //    if (poolProperty != null)
        //    {
        //        poolProperty.FindPropertyRelative("name").stringValue = newName;
        //        serializedObject.ApplyModifiedProperties();

        //        // sorting pools list

        //        int newIndex = -1;
        //        int oldIndex = -1;

        //        for (int i = 0; i < poolsListProperty.arraySize; i++)
        //        {
        //            int comparingResult = newName.CompareTo(poolsListProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);

        //            if (newIndex == -1 && comparingResult == -1)
        //            {
        //                newIndex = i;

        //                if (oldIndex != -1)
        //                    break;
        //            }

        //            if (comparingResult == 0)
        //            {
        //                oldIndex = i;

        //                if (newIndex != -1)
        //                    break;
        //            }
        //        }

        //        if (newIndex == -1)
        //        {
        //            newIndex = poolsListProperty.arraySize - 1;
        //        }

        //        selectedPoolIndex = newIndex;
        //        poolsListProperty.MoveArrayElement(oldIndex, newIndex);
        //        serializedObject.ApplyModifiedProperties();
        //    }
        //    else
        //    {
        //        poolBuilder = poolBuilder.SetName(newName);
        //    }
        //}

        /// <summary>
        /// 새로운 풀 생성 상태를 취소하고 관련 변수들을 초기 상태로 되돌립니다.
        /// </summary>
        private void CancelNewPoolCreation()
        {
            // newPoolBuilder의 이름이 EMPTY_POOL_BUILDER_NAME과 같으면 이미 취소된 상태이므로 반환합니다.
            //if (newPoolBuilder.name.Equals(EMPTY_POOL_BUILDER_NAME))
            //    return;

            // newPoolBuilder를 리셋하고 이름 관련 변수들을 초기화합니다. (현재 주석 처리됨)
            //newPoolBuilder = newPoolBuilder.Reset().SetName(EMPTY_POOL_BUILDER_NAME);
            lastRenamingName = RENAMING_EMPTY_STRING;
            isNameAllowed = true;
            isNameAlreadyExisting = false;
            skipEmptyNameWarning = false;
        }

        /// <summary>
        /// 지정된 인덱스의 풀 이름을 가져오고, 필요한 경우 캐시 상태 정보를 추가하여 반환합니다.
        /// </summary>
        /// <param name="poolIndex">이름을 가져올 풀의 인덱스</param>
        /// <returns>풀 이름과 선택적 캐시 상태 정보</returns>
        private string GetPoolName(int poolIndex)
        {
            // SerializedProperty를 사용하여 풀 이름을 가져옵니다.
            string poolName = poolsListProperty.GetArrayElementAtIndex(poolIndex).FindPropertyRelative("name").stringValue;
            //poolsList[poolIndex].Name;

            // [CACHE IS CURRENTLY DISABLED]
            //if (poolManagerRef.useCache)
            //{
            //    if (poolsCacheList.IsNullOrEmpty() || poolsCacheDeltaList.IsNullOrEmpty() || poolIndex > poolsCacheDeltaList.Count || poolIndex > poolsCacheList.Count)
            //    {
            //        UpdateCacheStateList();
            //    }

            //    // there is not cache for current scene returning
            //    if (poolsCacheList.IsNullOrEmpty())
            //    {
            //        return poolName;
            //    }

            //    int delta = poolsCacheDeltaList[poolIndex];

            //    if (poolsCacheList[poolIndex] != null && poolsCacheList[poolIndex].ignoreCache)
            //    {
            //        poolName += "   [cache ignored]";
            //    }
            //    else if (delta != 0)
            //    {
            //        poolName += "   " + CacheDeltaToState(delta);
            //    }
            //}

            return poolName; // 최종 풀 이름을 반환합니다.
        }

        //private void AddNewSinglePool(GameObject prefab = null)
        //{
        //    selectedPoolIndex = -1;

        //    newPoolBuilder = new PoolSettings(prefab != null ? prefab.name : string.Empty, prefab, 10, true);
        //    IsNameAllowed(newPoolBuilder.name);
        //}

        //private void AddNewMultiPool(List<Pool.MultiPoolPrefab> prefabs = null)
        //{
        //    selectedPoolIndex = -1;

        //    string name = (prefabs != null && prefabs.Count != 0) ? prefabs[0].prefab.name : string.Empty;
        //    newPoolBuilder = new PoolSettings(name, prefabs, 10, true);

        //    IsNameAllowed(newPoolBuilder.name);
        //}

        /// <summary>
        /// 새로운 풀 생성을 확정하고 PoolManager에 추가하는 함수입니다. (현재 UI에서 직접적인 호출은 주석 처리됨)
        /// newPoolBuilder에 저장된 정보를 사용하여 새로운 풀 SerializedProperty를 생성하고 poolsListProperty에 추가합니다.
        /// </summary>
        private void ConfirmPoolCreation()
        {
            skipEmptyNameWarning = false;

            // 새로운 풀 이름이 유효한지 확인합니다.
            //if (IsNameAllowed(newPoolBuilder.name))
            //{
            //    Undo.RecordObject(target, "New pool added");

            //    int poolsAmount = serializedObject.FindProperty("poolsList").arraySize;
            //    poolsAmount++;
            //    serializedObject.FindProperty("poolsList").arraySize = poolsAmount;

            //    SerializedProperty newPoolProperty = serializedObject.FindProperty("poolsList").GetArrayElementAtIndex(poolsAmount - 1);

            //    newPoolProperty.FindPropertyRelative("name").stringValue = newPoolBuilder.name;
            //    newPoolProperty.FindPropertyRelative("type").enumValueIndex = (int)newPoolBuilder.type;
            //    newPoolProperty.FindPropertyRelative("singlePoolPrefab").objectReferenceValue = newPoolBuilder.singlePoolPrefab;
            //    newPoolProperty.FindPropertyRelative("multiPoolPrefabsList").arraySize = newPoolBuilder.multiPoolPrefabsList.Count;

            //    for (int i = 0; i < newPoolBuilder.multiPoolPrefabsList.Count; i++)
            //    {
            //        newPoolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue = newPoolBuilder.multiPoolPrefabsList[i].prefab;
            //        newPoolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(i).FindPropertyRelative("weight").intValue = newPoolBuilder.multiPoolPrefabsList[i].weight;
            //        newPoolProperty.FindPropertyRelative("multiPoolPrefabsList").GetArrayElementAtIndex(i).FindPropertyRelative("isWeightLocked").boolValue = newPoolBuilder.multiPoolPrefabsList[i].isWeightLocked;
            //    }

            //    newPoolProperty.FindPropertyRelative("size").intValue = newPoolBuilder.size;
            //    newPoolProperty.FindPropertyRelative("autoSizeIncrement").boolValue = newPoolBuilder.autoSizeIncrement;
            //    newPoolProperty.FindPropertyRelative("objectsContainer").objectReferenceValue = newPoolBuilder.objectsContainer;

            //    serializedObject.ApplyModifiedProperties();

            //    recalculateWeightsAtPoolMethodInfo.Invoke(serializedObject.targetObject, new object[] { poolsAmount - 1 });

            //    for (int i = 0; i < poolsListProperty.arraySize; i++)
            //    {
            //        if (poolsListProperty.GetArrayElementAtIndex(poolsAmount - 1).FindPropertyRelative("name").stringValue.CompareTo(poolsListProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue) == -1)
            //        {
            //            poolsListProperty.MoveArrayElement(poolsAmount - 1, i);
            //            break;
            //        }
            //    }

            //    serializedObject.ApplyModifiedProperties();

            //    newPoolBuilder = newPoolBuilder.Reset().SetName(EMPTY_POOL_BUILDER_NAME);
            //    prevNewPoolName = string.Empty;

            //    lastRenamingName = RENAMING_EMPTY_STRING;
            //    isNameAllowed = true;
            //    isNameAlreadyExisting = false;

            //    searchText = "";
            //}
        }

        /// <summary>
        /// 지정된 인덱스의 풀을 PoolManager에서 삭제합니다.
        /// 직렬화된 속성에서 해당 항목을 제거합니다.
        /// </summary>
        /// <param name="indexOfPoolToRemove">삭제할 풀의 인덱스</param>
        private void DeletePool(int indexOfPoolToRemove)
        {
            // Undo 시스템에 변경 사항을 기록합니다.
            Undo.RecordObject(target, "Pool deleted");

            // poolsListProperty 배열에서 해당 인덱스의 항목을 제거합니다.
            serializedObject.FindProperty("poolsList").RemoveFromVariableArrayAt(indexOfPoolToRemove);

            // 선택된 풀 인덱스 및 이름 관련 변수들을 초기화합니다.
            selectedPoolIndex = -1;
            lastRenamingName = RENAMING_EMPTY_STRING;
            isNameAllowed = true;
            isNameAlreadyExisting = false;
        }

        /// <summary>
        /// 지정된 풀 이름이 유효한지 (비어있지 않고 중복되지 않는지) 확인합니다.
        /// isNameAllowed 및 isNameAlreadyExisting 플래그를 업데이트합니다.
        /// </summary>
        /// <param name="nameToCheck">확인할 풀 이름</param>
        /// <returns>이름이 유효하면 true, 그렇지 않으면 false</returns>
        private bool IsNameAllowed(string nameToCheck)
        {
            // 이름이 비어있는 경우 유효하지 않음으로 판단합니다.
            if (nameToCheck.Equals(string.Empty))
            {
                isNameAllowed = false;
                isNameAlreadyExisting = false;
                return false;
            }

            // 풀 목록이 비어있는 경우 어떤 이름이든 유효합니다.
            if (serializedObject.FindProperty("poolsList").arraySize == 0)
            {
                isNameAllowed = true;
                isNameAlreadyExisting = false;
                return true;
            }

            // 이름이 이미 존재하는지 확인하고 결과를 플래그에 반영합니다.
            if (IsNameAlreadyExisting(nameToCheck))
            {
                isNameAllowed = false;
                isNameAlreadyExisting = true;
                return false;
            }
            else // 이름이 중복되지 않으면 유효합니다.
            {
                isNameAllowed = true;
                isNameAlreadyExisting = false;
                return true;
            }
        }

        /// <summary>
        /// 지정된 풀 이름이 PoolManager에 이미 등록된 풀 이름과 중복되는지 확인합니다.
        /// </summary>
        /// <param name="nameToCheck">중복 확인을 할 풀 이름</param>
        /// <returns>이름이 중복되면 true, 그렇지 않으면 false</returns>
        private bool IsNameAlreadyExisting(string nameToCheck)
        {
            // poolsListProperty 배열을 순회하며 각 풀의 이름과 비교합니다.
            for (int i = 0; i < poolsListProperty.arraySize; i++)
            {
                // 현재 풀의 이름과 nameToCheck가 일치하면 중복됩니다.
                if (poolsListProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue.Equals(nameToCheck))
                {
                    return true;
                }
            }

            // 모든 풀 이름을 확인했는데 중복이 없으면 false를 반환합니다.
            return false;
        }
    }
}

// -----------------
// Pool Manager v 1.6.5
// -----------------