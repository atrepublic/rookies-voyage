// Unity 에디터에서 스크립팅 정의 심볼(Scripting Define Symbols)을 관리하는 정적 클래스입니다.
// 이 스크립트는 특정 정의 심볼의 존재 여부를 확인하거나, 정의 심볼을 추가 및 제거하는 기능을 제공합니다.
// 또한, 특정 조건에 따라 자동으로 정의 심볼을 업데이트하는 기능도 포함하고 있습니다.
// Unity 2023 버전 이상 권장 문법을 사용하여 작성되었습니다.
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

#if UNITY_6000
using UnityEditor.Build;
#endif

namespace Watermelon
{
    public static class DefineManager
    {
        /// <summary>
        /// 현재 빌드 타겟 그룹에 특정 정의 심볼이 설정되어 있는지 확인합니다.
        /// </summary>
        /// <param name="define">확인할 정의 심볼 문자열입니다.</param>
        /// <returns>정의 심볼이 설정되어 있으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool HasDefine(string define)
        {
#if UNITY_6000
            // Unity 2022.2 이상 버전에서 권장되는 API를 사용하여 정의 심볼을 가져옵니다.
            string definesLine = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else
            // 이전 Unity 버전과의 호환성을 위한 API입니다.
            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 정의 심볼 문자열을 ';' 기준으로 분할하여 배열로 만든 후, 특정 정의 심볼이 배열에 존재하는지 찾습니다.
            return Array.FindIndex(definesLine.Split(';'), x => x == define) != -1;
        }

        /// <summary>
        /// 현재 빌드 타겟 그룹에 특정 정의 심볼을 추가합니다. 이미 존재하는 경우 아무 작업도 수행하지 않습니다.
        /// </summary>
        /// <param name="define">추가할 정의 심볼 문자열입니다.</param>
        public static void EnableDefine(string define)
        {
#if UNITY_6000
            // Unity 2022.2 이상 버전에서 권장되는 API를 사용하여 정의 심볼을 가져옵니다.
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else
            // 이전 Unity 버전과의 호환성을 위한 API입니다.
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 이미 정의 심볼이 존재하는지 확인하고, 존재하면 함수를 종료합니다.
            if (Array.FindIndex(defineLine.Split(';'), x => x == define) != -1)
            {
                return;
            }

            // 새로운 정의 심볼을 기존 정의 심볼 문자열의 맨 앞에 추가합니다.
            defineLine = defineLine.Insert(0, define + ";");

#if UNITY_6000
            // Unity 2022.2 이상 버전에서 권장되는 API를 사용하여 정의 심볼을 설정합니다.
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)), defineLine);
#else
            // 이전 Unity 버전과의 호환성을 위한 API입니다.
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), defineLine);
#endif
        }

        /// <summary>
        /// 현재 빌드 타겟 그룹에서 특정 정의 심볼을 제거합니다. 존재하지 않는 경우 아무 작업도 수행하지 않습니다.
        /// </summary>
        /// <param name="define">제거할 정의 심볼 문자열입니다.</param>
        public static void DisableDefine(string define)
        {
#if UNITY_6000
            // Unity 2022.2 이상 버전에서 권장되는 API를 사용하여 정의 심볼을 가져옵니다.
            string defineLine = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else
            // 이전 Unity 버전과의 호환성을 위한 API입니다.
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 정의 심볼 문자열을 ';' 기준으로 분할합니다.
            string[] splitedDefines = defineLine.Split(';');

            // 제거할 정의 심볼의 인덱스를 찾습니다.
            int tempDefineIndex = Array.FindIndex(splitedDefines, x => x == define);
            string tempDefineLine = ""; // 제거 후의 정의 심볼 문자열을 저장할 변수입니다.

            // 정의 심볼이 존재하는 경우
            if (tempDefineIndex != -1)
            {
                // 제거할 정의 심볼을 제외하고 다시 문자열을 만듭니다.
                for (int i = 0; i < splitedDefines.Length; i++)
                {
                    if (i != tempDefineIndex)
                    {
                        tempDefineLine += splitedDefines[i] + ";"; // 세미콜론을 다시 붙여줍니다.
                    }
                }
                // 마지막에 붙은 세미콜론을 제거합니다.
                if (tempDefineLine.EndsWith(";"))
                {
                    tempDefineLine = tempDefineLine.Substring(0, tempDefineLine.Length - 1);
                }
            } else
            {
                 // 제거할 정의 심볼이 없으면 원래 문자열을 그대로 사용합니다.
                 tempDefineLine = defineLine;
            }


            // 변경된 정의 심볼 문자열이 기존과 다른 경우에만 설정을 업데이트합니다.
            // 문자열 비교 전에 공백과 세미콜론을 정리하여 정확한 비교를 합니다.
            string cleanedDefineLine = string.Join(";", defineLine.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            string cleanedTempDefineLine = string.Join(";", tempDefineLine.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries));


            if (cleanedDefineLine != cleanedTempDefineLine)
            {
#if UNITY_6000
                // Unity 2022.2 이상 버전에서 권장되는 API를 사용하여 정의 심볼을 설정합니다.
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)), tempDefineLine);
#else
                // 이전 Unity 버전과의 호환성을 위한 API입니다.
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), tempDefineLine);
#endif
            }
        }

        /// <summary>
        /// 자동으로 관리되는 정의 심볼들을 확인하고 필요한 경우 업데이트를 예약합니다.
        /// 컴파일 중이거나 에디터 업데이트 중인 경우 완료될 때까지 기다립니다.
        /// </summary>
        public static void CheckAutoDefines()
        {
            // 에디터가 컴파일 중이거나 업데이트 중이거나 코어 폴더 경로가 설정되지 않은 경우, 딜레이 호출을 통해 함수를 다시 실행합니다.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || string.IsNullOrEmpty(CoreEditor.FOLDER_CORE))
            {
                EditorApplication.delayCall += CheckAutoDefines;

                return;
            }

            // 현재 로드된 모든 어셈블리를 가져옵니다.
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 자동으로 관리될 정의 심볼들의 상태를 저장할 리스트를 생성합니다.
            List<DefineState> markedDefines = new List<DefineState>();
            // 설정 파일에서 동적으로 관리되는 정의 심볼 목록을 가져옵니다.
            List<RegisteredDefine> registeredDefines = DefineSettings.GetDynamicDefines();

            // 등록된 각 정의 심볼에 대해 해당 어셈블리가 로드되었는지 확인합니다.
            foreach (RegisteredDefine registeredDefine in registeredDefines)
            {
                bool defineFound = false; // 정의 심볼과 연결된 타입이 어셈블리에서 발견되었는지 나타냅니다.

                // 각 어셈블리를 순회하며 등록된 정의 심볼과 연결된 타입을 찾습니다.
                foreach(Assembly assembly in assemblies)
                {
                    // 지정된 타입 이름으로 어셈블리에서 타입을 검색합니다 (예외 발생 시에도 처리를 계속합니다).
                    Type targetType = assembly.GetType(registeredDefine.AssemblyType, false);
                    if (targetType != null)
                    {
                        // 타입이 발견되면 해당 정의 심볼을 활성화 상태로 표시합니다.
                        markedDefines.Add(new DefineState(registeredDefine.Define, true));

                        defineFound = true; // 정의 심볼을 찾았음을 표시합니다.

                        break; // 현재 어셈블리에서 타입을 찾았으므로 다음 등록된 정의 심볼로 넘어갑니다.
                    }
                }

                // 모든 어셈블리를 순회한 후에도 타입을 찾지 못한 경우 해당 정의 심볼을 비활성화 상태로 표시합니다.
                if(!defineFound)
                {
                    markedDefines.Add(new DefineState(registeredDefine.Define, false));
                }
            }

            // 자동으로 관리되는 정의 심볼들의 상태를 변경합니다.
            ChangeAutoDefinesState(markedDefines);
        }

        /// <summary>
        /// 자동으로 관리되는 정의 심볼들의 상태를 실제 스크립팅 정의 심볼에 적용합니다.
        /// 컴파일 중인 경우 작업을 건너뜁니다.
        /// </summary>
        /// <param name="defineStates">상태를 적용할 정의 심볼과 해당 상태(활성화/비활성화) 목록입니다.</param>
        public static void ChangeAutoDefinesState(List<DefineState> defineStates)
        {
            // 에디터가 컴파일 중이면 작업을 수행하지 않습니다.
            if (EditorApplication.isCompiling)
                return;

            // 적용할 정의 심볼 목록이 비어있으면 작업을 수행하지 않습니다.
            if (defineStates.IsNullOrEmpty())
                return;

            bool definesUpdated = false; // 정의 심볼이 변경되었는지 나타냅니다.

            StringBuilder sb = new StringBuilder(); // 로그 메시지를 생성하기 위한 StringBuilder입니다.
            sb.Append("[Define Manager]: Dependencies change is detected. Updating Scripting Define Symbols..");
            sb.AppendLine();

            DefineString definesString = new DefineString(); // 현재 설정된 정의 심볼을 관리하는 헬퍼 클래스입니다.

            // 각 정의 심볼 상태에 따라 스크립팅 정의 심볼을 업데이트합니다.
            foreach (DefineState defineState in defineStates)
            {
                // 정의 심볼을 활성화해야 하는 경우
                if (defineState.State)
                {
                    // 현재 설정에 해당 정의 심볼이 없으면 추가합니다.
                    if (!definesString.HasDefine(defineState.Define))
                    {
                        definesUpdated = true; // 변경이 발생했음을 표시합니다.

                        definesString.AddDefine(defineState.Define); // 정의 심볼을 추가합니다.

                        sb.AppendLine();
                        sb.Append(defineState.Define);
                        sb.Append(" - added"); // 로그 메시지에 추가되었음을 기록합니다.
                    }
                }
                // 정의 심볼을 비활성화해야 하는 경우
                else
                {
                    // 현재 설정에 해당 정의 심볼이 있으면 제거합니다.
                    if (definesString.HasDefine(defineState.Define))
                    {
                        definesUpdated = true; // 변경이 발생했음을 표시합니다.

                        definesString.RemoveDefine(defineState.Define); // 정의 심볼을 제거합니다.

                        sb.AppendLine();
                        sb.Append(defineState.Define);
                        sb.Append(" - removed"); // 로그 메시지에 제거되었음을 기록합니다.
                    }
                }
            }
            sb.AppendLine();

            // 정의 심볼 변경이 발생한 경우 로그 메시지를 출력합니다.
            if (definesUpdated)
                Debug.Log(sb.ToString());

            // 변경된 정의 심볼 설정을 실제로 적용합니다.
            definesString.ApplyDefines();
        }
    }
}

// -----------------
// Define Manager v0.3.1
// -----------------

// 변경사항 (Changelog)
// v 0.3.1
// • Define 어트리뷰트를 클래스에 추가하여 자동 정의를 로드하는 기능 추가
// v 0.3
// • 특정 정의에 대한 자동 토글 기능 추가
// • UI를 ScriptableObject 에디터에서 에디터 창으로 이동
// v 0.2.1
// • 문서 링크 추가
// • Define 활성화 기능 수정
// v 0.1
// • 기본 버전 추가