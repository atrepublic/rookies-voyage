// 스크립트 기능 요약:
// 이 스크립트는 Unity 에디터 확장 기능으로, 프로젝트의 스크립팅 정의 심볼(Scripting Define Symbols)을 관리하는 창을 제공합니다.
// 정의 심볼들을 확인하고, 활성화/비활성화하며, 저장하는 기능을 수행합니다.
// 프로젝트 전반에 걸쳐 사용되는 다양한 정의 심볼(정적, 프로젝트, 써드파티, 자동 생성)을 시각적으로 관리할 수 있도록 돕습니다.

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Watermelon
{
    // DefineManagerWindow 클래스는 Unity 에디터 창으로 작동합니다.
    public class DefineManagerWindow : EditorWindow
    {
        // projectDefines: 프로젝트 내에서 사용되는 모든 정의 심볼 목록을 저장하는 변수입니다.
        // 각 Define 객체는 정의 심볼 자체와 활성화 상태, 타입을 포함합니다.
        [Tooltip("프로젝트 내에서 사용되는 모든 정의 심볼 목록")]
        private Define[] projectDefines;

        // isDefinesSame: 현재 에디터의 정의 심볼 설정이 창에 표시된 설정과 동일한지 여부를 나타내는 변수입니다.
        // 이 값이 true이면 "Apply Defines" 버튼이 비활성화됩니다.
        [Tooltip("현재 설정과 에디터의 정의 심볼 설정이 동일한지 여부")]
        private bool isDefinesSame;
        // isRequireInit: 창이 활성화되거나 정의 심볼 설정이 변경되었을 때 다시 초기화가 필요한지 여부를 나타내는 변수입니다.
        // true이면 변수들을 다시 캐싱하고 정의 심볼 일치 여부를 확인합니다.
        [Tooltip("창 초기화 또는 설정 변경 시 다시 초기화가 필요한지 여부")]
        private bool isRequireInit;

        /// <summary>
        /// Define Manager 창을 보여줍니다.
        /// Unity 에디터의 "Tools/Editor/Define Manager" 또는 "Window/Watermelon Core/Define Manager" 메뉴를 통해 접근할 수 있습니다.
        /// </summary>
        [MenuItem("Tools/Editor/Define Manager")]
        [MenuItem("Window/Watermelon Core/Define Manager", priority = -50)]
        public static void ShowWindow()
        {
            DefineManagerWindow window = GetWindow<DefineManagerWindow>(true);
            window.minSize = new Vector2(300, 200);
            window.titleContent = new GUIContent("Define Manager");
        }

        /// <summary>
        /// 창이 활성화될 때 호출됩니다.
        /// 초기화 필요 플래그를 설정하고 변수들을 캐싱합니다.
        /// </summary>
        protected void OnEnable()
        {
            isRequireInit = true;

            CacheVariables();
        }

        /// <summary>
        /// 현재 빌드 타겟 그룹에 설정된 정적 정의 심볼 목록을 가져옵니다.
        /// Unity 2023 이상 버전과 이전 버전에 따라 다른 PlayerSettings API를 사용합니다.
        /// </summary>
        /// <returns>활성화된 정적 정의 심볼 문자열 배열 또는 정의 심볼이 없을 경우 null</returns>
        private string[] GetActiveStaticDefines()
        {
            // 현재 활성화된 빌드 타겟 그룹의 정의 심볼 문자열을 가져옵니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
            string definesLine = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else // 이전 Unity 버전에 대한 조건부 컴파일
            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 정의 심볼 문자열이 비어있지 않으면 처리합니다.
            if (!string.IsNullOrEmpty(definesLine))
            {
                List<string> activeDefines = new List<string>();

                // 정의 심볼 문자열을 ';' 문자를 기준으로 분할하여 배열로 만듭니다.
                string[] defines = definesLine.Split(';');

                // 미리 정의된 정적 정의 심볼 목록을 순회하며 현재 설정에 포함되어 있는지 확인합니다.
                for (int i = 0; i < DefineSettings.STATIC_DEFINES.Length; i++)
                {
                    // 현재 정적 정의 심볼이 활성화된 정의 심볼 배열에 포함되어 있는지 찾습니다.
                    if (Array.FindIndex(defines, x => x.Equals(DefineSettings.STATIC_DEFINES[i])) != -1)
                    {
                        // 포함되어 있다면 activeDefines 목록에 추가합니다.
                        activeDefines.Add(DefineSettings.STATIC_DEFINES[i]);
                    }
                }

                // 활성화된 정적 정의 심볼 목록을 배열로 변환하여 반환합니다.
                return activeDefines.ToArray();
            }

            // 정의 심볼 문자열이 비어있으면 null을 반환합니다.
            return null;
        }

        /// <summary>
        /// 프로젝트에서 사용되는 정의 심볼들을 캐싱합니다.
        /// 정적 정의, 프로젝트 정의, 자동 생성 정의, 써드파티 정의를 포함합니다.
        /// </summary>
        private void CacheVariables()
        {
            // 프로젝트 정의 심볼 목록을 저장할 리스트를 초기화합니다.
            List<Define> defines = new List<Define>();

            // 활성화된 정적 정의 심볼 목록을 가져옵니다.
            string[] activeStaticDefines = GetActiveStaticDefines();
            // 활성화된 정적 정의 심볼이 있으면 defines 목록에 추가합니다.
            if (!activeStaticDefines.IsNullOrEmpty())
            {
                for (int i = 0; i < activeStaticDefines.Length; i++)
                {
                    defines.Add(new Define(activeStaticDefines[i], Define.Type.Static, true));
                }
            }

            // DefineAttribute가 적용된 타입을 찾기 위해 현재 AppDomain의 모든 어셈블리를 가져옵니다.
            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    try
                    {
                        // 어셈블리에서 모든 타입을 가져옵니다.
                        Type[] tempTypes = assembly.GetTypes();

                        // DefineAttribute가 정의된 타입만 필터링합니다.
                        tempTypes = tempTypes.Where(m => m.IsDefined(typeof(DefineAttribute), true)).ToArray();

                        // 필터링된 타입이 있으면 gameTypes 목록에 추가합니다.
                        if (!tempTypes.IsNullOrEmpty())
                            gameTypes.AddRange(tempTypes);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        // 리플렉션 타입 로딩 중 예외가 발생하면 로그를 출력합니다.
                        Debug.LogException(e);
                    }
                }
            }

            // DefineAttribute가 적용된 각 타입을 순회하며 프로젝트 정의 심볼을 추출합니다.
            foreach (Type type in gameTypes)
            {
                // 타입에 적용된 DefineAttribute를 가져옵니다.
                DefineAttribute[] defineAttributes = (DefineAttribute[])Attribute.GetCustomAttributes(type, typeof(DefineAttribute));

                // 각 DefineAttribute를 순회합니다.
                for (int i = 0; i < defineAttributes.Length; i++)
                {
                    // AssemblyType이 비어있는 경우 프로젝트 정의 심볼로 처리합니다.
                    if (string.IsNullOrEmpty(defineAttributes[i].AssemblyType))
                    {
                        // 이미 목록에 추가된 정의 심볼인지 확인합니다.
                        int methodId = defines.FindIndex(x => x.define == defineAttributes[i].Define);
                        // 목록에 없으면 새로 추가합니다.
                        if (methodId == -1)
                        {
                            defines.Add(new Define(defineAttributes[i].Define, Define.Type.Project));
                        }
                    }
                }
            }

            // DefineSettings에서 자동 생성 정의 목록을 가져옵니다.
            List<RegisteredDefine> registeredDefines = DefineSettings.GetDynamicDefines();

            // 현재 빌드 타겟 그룹에 설정된 정의 심볼 문자열을 가져옵니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else // 이전 Unity 버전에 대한 조건부 컴파일
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 현재 설정된 정의 심볼들을 배열로 만듭니다.
            string[] currentDefinesArray = defineLine.Split(';');
            // 현재 설정된 정의 심볼들을 순회하며 defines 목록에 추가합니다.
            for (int i = 0; i < currentDefinesArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(currentDefinesArray[i]))
                {
                    // 자동 생성 정의 목록에 포함되어 있는지 확인합니다.
                    if (registeredDefines.FindIndex(x => x.Define == currentDefinesArray[i]) != -1)
                    {
                        defines.Add(new Define(currentDefinesArray[i], Define.Type.Auto, true));
                    }
                    // 이미 defines 목록에 없는 경우 써드파티 정의 심볼로 처리합니다.
                    else if (defines.FindIndex(x => x.define == currentDefinesArray[i]) == -1)
                    {
                        defines.Add(new Define(currentDefinesArray[i], Define.Type.ThirdParty, true));
                    }
                }
            }

            // 최종적으로 캐싱된 정의 심볼 목록을 projectDefines 배열에 저장합니다.
            projectDefines = defines.ToArray();

            // 활성화된 정의 심볼 상태를 로드합니다.
            LoadActiveDefines();
        }

        /// <summary>
        /// 현재 빌드 타겟 그룹에 설정된 정의 심볼을 기반으로 projectDefines 배열의 활성화 상태를 업데이트합니다.
        /// </summary>
        private void LoadActiveDefines()
        {
            // 현재 활성화된 빌드 타겟 그룹의 정의 심볼 문자열을 가져옵니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else // 이전 Unity 버전에 대한 조건부 컴파일
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 현재 설정된 정의 심볼들을 배열로 만듭니다.
            string[] currentDefinesArray = defineLine.Split(';');

            // 현재 설정된 정의 심볼들이 비어있지 않으면 처리합니다.
            if (!currentDefinesArray.IsNullOrEmpty())
            {
                // 현재 설정된 각 정의 심볼을 순회합니다.
                for (int i = 0; i < currentDefinesArray.Length; i++)
                {
                    // projectDefines 배열에서 현재 정의 심볼과 일치하는 항목의 인덱스를 찾습니다.
                    int defineIndex = Array.FindIndex(projectDefines, x => x.define.Equals(currentDefinesArray[i]));

                    // 일치하는 항목이 있으면 해당 정의 심볼의 활성화 상태를 true로 설정합니다.
                    if (defineIndex != -1)
                    {
                        projectDefines[defineIndex].isEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// projectDefines 배열에 설정된 활성화 상태를 기반으로 정의 심볼 문자열을 생성합니다.
        /// 이 문자열은 PlayerSettings에 저장될 수 있습니다.
        /// </summary>
        /// <returns>활성화된 정의 심볼을 ';'로 구분한 문자열</returns>
        private string GetActiveDefinesLine()
        {
            string definesLine = "";

            // projectDefines 배열의 각 Define 객체를 순회합니다.
            for (int i = 0; i < projectDefines.Length; i++)
            {
                // 해당 Define 객체가 활성화 상태이면 정의 심볼을 definesLine에 추가합니다.
                if (projectDefines[i].isEnabled)
                {
                    definesLine += projectDefines[i].define + ";";
                }
            }

            // 생성된 정의 심볼 문자열을 반환합니다.
            return definesLine;
        }

        /// <summary>
        /// 지정된 정의 심볼 문자열을 현재 빌드 타겟 그룹의 스크립팅 정의 심볼로 저장합니다.
        /// Unity 2023 이상 버전과 이전 버전에 따라 다른 PlayerSettings API를 사용합니다.
        /// </summary>
        /// <param name="definesLine">저장할 정의 심볼 문자열</param>
        private void SaveDefines(string definesLine)
        {
            // 현재 활성화된 빌드 타겟 그룹에 정의 심볼 문자열을 설정합니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)), definesLine);
#else // 이전 Unity 버전에 대한 조건부 컴파일
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), definesLine);
#endif
        }

        /// <summary>
        /// projectDefines 배열의 활성화 상태와 현재 빌드 타겟 그룹의 정의 심볼이 일치하는지 비교합니다.
        /// </summary>
        /// <returns>정의 심볼 설정이 동일하면 true, 다르면 false</returns>
        private bool CompareDefines()
        {
            // 현재 활성화된 빌드 타겟 그룹의 정의 심볼 문자열을 가져옵니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else // 이전 Unity 버전에 대한 조건부 컴파일
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 현재 설정된 정의 심볼들을 배열로 만듭니다.
            string[] currentDefinesArray = defineLine.Split(';');

            // projectDefines 배열의 각 Define 객체를 순회하며 현재 설정과 비교합니다.
            for (int i = 0; i < projectDefines.Length; i++)
            {
                // projectDefines의 정의 심볼이 현재 설정에 있는지 찾습니다.
                int findIndex = Array.FindIndex(currentDefinesArray, x => x == projectDefines[i].define);

                // projectDefines의 해당 항목이 활성화되어 있는데 현재 설정에 없거나,
                // projectDefines의 해당 항목이 비활성화되어 있는데 현재 설정에 있으면 일치하지 않는 것으로 판단합니다.
                if (projectDefines[i].isEnabled)
                {
                    if (findIndex == -1)
                        return false; // 활성화되어야 하지만 현재 설정에 없음
                }
                else
                {
                    if (findIndex != -1)
                        return false; // 비활성화되어야 하지만 현재 설정에 있음
                }
            }

            // 모든 항목이 현재 설정과 일치하면 true를 반환합니다.
            return true;
        }

        /// <summary>
        /// Define Manager 창의 GUI를 그립니다.
        /// 정의 심볼 목록을 표시하고 활성화/비활성화를 토글하며, 변경 사항을 적용하는 버튼을 제공합니다.
        /// </summary>
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);

            // projectDefines가 비어있지 않으면 정의 심볼 목록을 표시합니다.
            if (!projectDefines.IsNullOrEmpty())
            {
                // GUI 변경 사항 감지를 시작합니다.
                EditorGUI.BeginChangeCheck();

                int customDefineIndex = 0;

                // projectDefines 배열의 각 Define 객체를 순회하며 GUI 항목을 그립니다.
                for (int i = 0; i < projectDefines.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Define 객체의 타입에 따라 다른 GUI를 그립니다.
                    switch (projectDefines[i].type)
                    {
                        case Define.Type.Auto:
                            // 자동 생성 정의: 토글 가능하며 "(Auto)" 표시가 붙습니다.
                            projectDefines[i].isEnabled = EditorGUILayout.Toggle(projectDefines[i].isEnabled, GUILayout.Width(20));
                            EditorGUILayout.LabelField(projectDefines[i].define + " (Auto)");
                            break;
                        case Define.Type.Static:
                            // 정적 정의: 항상 활성화되어 있으며 토글할 수 없습니다.
                            EditorGUI.BeginDisabledGroup(true); // 비활성화 그룹 시작
                            EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            EditorGUILayout.LabelField(projectDefines[i].define);
                            GUILayout.Space(22); // 간격 추가
                            EditorGUI.EndDisabledGroup(); // 비활성화 그룹 끝
                            break;
                        case Define.Type.Project:
                            // 프로젝트 정의: 토글 가능합니다.
                            projectDefines[i].isEnabled = EditorGUILayout.Toggle(projectDefines[i].isEnabled, GUILayout.Width(20));
                            EditorGUILayout.LabelField(projectDefines[i].define);
                            break;
                        case Define.Type.ThirdParty:
                            // 써드파티 정의: 항상 활성화되어 있으며 토글할 수 없습니다. 삭제 버튼이 제공됩니다.
                            EditorGUI.BeginDisabledGroup(true); // 비활성화 그룹 시작
                            EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            EditorGUI.EndDisabledGroup(); // 비활성화 그룹 끝
                            EditorGUILayout.LabelField(projectDefines[i].define + " (Thrid Party)");
                            GUILayout.FlexibleSpace(); // 유연한 공간 추가

                            // 삭제 버튼을 그립니다.
                            if (GUILayout.Button("X", EditorCustomStyles.buttonRed, GUILayout.Height(18), GUILayout.Width(18)))
                            {
                                // 삭제 확인 대화 상자를 표시합니다.
                                if (EditorUtility.DisplayDialog("Remove define", "Are you sure you want to remove define?", "Remove", "Cancel"))
                                {
                                    // 현재 빌드 타겟 그룹의 정의 심볼 문자열을 가져옵니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
                                    string defineLine = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else // 이전 Unity 버전에 대한 조건부 컴파일
                                    string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif
                                    // 현재 설정된 정의 심볼들을 배열로 만듭니다.
                                    string[] currentDefinesArray = defineLine.Split(';');

                                    // 삭제할 정의 심볼을 제외하고 다시 정의 심볼 문자열을 구성합니다.
                                    defineLine = "";
                                    for (int k = 0; k < currentDefinesArray.Length; k++)
                                    {
                                        if (currentDefinesArray[k] != projectDefines[i].define)
                                            defineLine += currentDefinesArray[k] + ";";
                                    }

                                    // 변경된 정의 심볼을 저장합니다.
                                    SaveDefines(defineLine);

                                    // 변수들을 다시 캐싱하여 UI를 업데이트합니다.
                                    CacheVariables();
                                }
                            }
                            customDefineIndex++;
                            break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                // GUI 변경 사항이 감지되면 초기화 필요 플래그를 설정합니다.
                if (EditorGUI.EndChangeCheck())
                {
                    isRequireInit = true;
                }
            }
            else
            {
                // 프로젝트에 정의 심볼이 없을 경우 메시지를 표시합니다.
                EditorGUILayout.LabelField("There are no defines in project.");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);

            // 초기화가 필요한 경우 정의 심볼 일치 여부를 확인하고 초기화 플래그를 해제합니다.
            if (isRequireInit)
            {
                isDefinesSame = CompareDefines();
                isRequireInit = false;
            }

            // 정의 심볼 설정이 현재와 동일하면 "Apply Defines" 버튼을 비활성화합니다.
            EditorGUI.BeginDisabledGroup(isDefinesSame);

            // "Apply Defines" 버튼을 그립니다. 클릭 시 변경된 정의 심볼을 저장합니다.
            if (GUILayout.Button("Apply Defines", EditorCustomStyles.button))
            {
                SaveDefines(GetActiveDefinesLine());

                // 저장 후 GUI 업데이트를 위해 리턴합니다.
                return;
            }

            EditorGUI.EndDisabledGroup(); // 비활성화 그룹 끝

            // "Check Auto Defines" 버튼을 그립니다. 클릭 시 DefineManager의 자동 정의 심볼 확인 기능을 호출합니다.
            if (GUILayout.Button("Check Auto Defines", EditorCustomStyles.button))
            {
                DefineManager.CheckAutoDefines();

                // 기능 호출 후 GUI 업데이트를 위해 리턴합니다.
                return;
            }

            EditorGUILayout.EndVertical();

            // 컴파일 진행 상태 창을 그립니다.
            EditorGUILayoutCustom.DrawCompileWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

        // Define 클래스는 개별 정의 심볼의 정보를 저장합니다.
        [System.Serializable]
        private class Define
        {
            // define: 정의 심볼의 문자열 값입니다. 예: "UNITY_EDITOR"
            [Tooltip("정의 심볼 이름")]
            public string define;
            // type: 정의 심볼의 타입을 나타냅니다. Static, Project, ThirdParty, Auto 중 하나입니다.
            [Tooltip("정의 심볼 타입")]
            public Type type;

            // isEnabled: 해당 정의 심볼이 현재 활성화되어 있는지 여부를 나타냅니다.
            [Tooltip("정의 심볼 활성화 상태")]
            public bool isEnabled;

            /// <summary>
            /// Define 클래스의 생성자입니다.
            /// </summary>
            /// <param name="define">정의 심볼 이름</param>
            /// <param name="type">정의 심볼 타입</param>
            /// <param name="isEnabled">초기 활성화 상태 (기본값: false)</param>
            public Define(string define, Type type, bool isEnabled = false)
            {
                this.define = define;
                this.type = type;
                this.isEnabled = isEnabled;
            }

            // Type 열거형은 정의 심볼의 타입을 정의합니다.
            public enum Type
            {
                Static = 0, // Unity 빌드 설정에 의해 정의되는 정적 심볼
                Project = 1, // 프로젝트 내에서 DefineAttribute를 통해 정의되는 심볼
                ThirdParty = 2, // 프로젝트 외부(예: 에셋 스토어 패키지)에서 정의되는 심볼
                Auto = 3 // 특정 조건에 따라 DefineManager에 의해 자동으로 활성화/비활성화되는 심볼
            }
        }
    }
}