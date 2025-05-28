// 스크립트 기능 요약:
// 이 스크립트는 Unity의 PlayerSettings에 설정된 스크립팅 정의 심볼(Scripting Define Symbols) 문자열을 관리하는 유틸리티 클래스입니다.
// 현재 빌드 타겟의 정의 심볼을 가져와서 리스트 형태로 관리하고, 특정 정의 심볼의 포함 여부를 확인하거나 추가/제거하는 기능을 제공합니다.
// 변경된 정의 심볼 목록을 다시 문자열로 변환하고 PlayerSettings에 적용하는 기능도 포함합니다.

using UnityEditor;
using System.Collections.Generic;
using System.Text;
using UnityEngine; // Tooltip 속성 사용을 위해 필요

namespace Watermelon
{
    // DefineString 클래스는 스크립팅 정의 심볼 문자열을 처리하는 유틸리티 클래스입니다.
    public class DefineString
    {
        // defineLine: 초기화 시 PlayerSettings에서 가져온 원본 정의 심볼 문자열입니다.
        // 변경 사항이 있는지 확인하는 데 사용됩니다.
        [Tooltip("초기 PlayerSettings에서 가져온 정의 심볼 원본 문자열")]
        private string defineLine;
        // defineList: defineLine 문자열을 ';' 문자로 분리하여 저장하는 정의 심볼 이름 리스트입니다.
        // 정의 심볼을 추가하거나 제거할 때 이 리스트를 수정합니다.
        [Tooltip("분리된 정의 심볼 이름 리스트")]
        private List<string> defineList;

        /// <summary>
        /// DefineString 클래스의 생성자입니다.
        /// 객체가 생성될 때 현재 빌드 타겟 그룹에 설정된 스크립팅 정의 심볼 문자열을 가져와서 초기화합니다.
        /// Unity 2023 이상 버전과 이전 버전에 따라 다른 PlayerSettings API를 사용합니다.
        /// </summary>
        public DefineString()
        {
            // 현재 활성화된 빌드 타겟 그룹의 정의 심볼 문자열을 가져옵니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
            defineLine = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
#else // 이전 Unity 버전에 대한 조건부 컴파일
            defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#endif

            // 가져온 정의 심볼 문자열을 ';' 문자로 분리하여 defineList를 초기화합니다.
            defineList = new List<string>(defineLine.Split(';'));
        }

        /// <summary>
        /// 지정된 정의 심볼이 현재 defineList에 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="define">확인할 정의 심볼 이름</param>
        /// <returns>정의 심볼이 리스트에 포함되어 있으면 true, 그렇지 않으면 false</returns>
        public bool HasDefine(string define)
        {
            // defineList에서 지정된 정의 심볼을 찾습니다.
            // FindIndex는 일치하는 항목이 없으면 -1을 반환합니다.
            return defineList.FindIndex(x => x == define) != -1;
        }

        /// <summary>
        /// 지정된 정의 심볼을 defineList에서 제거합니다.
        /// 리스트에 해당 정의 심볼이 없으면 아무 작업도 수행하지 않습니다.
        /// </summary>
        /// <param name="define">제거할 정의 심볼 이름</param>
        public void RemoveDefine(string define)
        {
            // defineList에서 지정된 정의 심볼의 인덱스를 찾습니다.
            int defineIndex = defineList.FindIndex(x => x == define);
            // 인덱스가 -1이면(찾지 못했으면) 함수를 종료합니다.
            if (defineIndex == -1)
                return;

            // 해당 인덱스의 항목을 리스트에서 제거합니다.
            defineList.RemoveAt(defineIndex);
        }

        /// <summary>
        /// 지정된 정의 심볼을 defineList에 추가합니다.
        /// 이미 리스트에 해당 정의 심볼이 있으면 아무 작업도 수행하지 않습니다.
        /// </summary>
        /// <param name="define">추가할 정의 심볼 이름</param>
        public void AddDefine(string define)
        {
            // defineList에서 지정된 정의 심볼의 인덱스를 찾습니다.
            int defineIndex = defineList.FindIndex(x => x == define);
            // 인덱스가 -1이 아니면(이미 존재하면) 함수를 종료합니다.
            if (defineIndex != -1)
                return;

            // defineList에 지정된 정의 심볼을 추가합니다.
            defineList.Add(define);
        }

        /// <summary>
        /// 현재 defineList에 있는 정의 심볼들을 ';' 문자로 연결하여 하나의 문자열로 반환합니다.
        /// 이 문자열은 PlayerSettings에 설정할 수 있는 형식입니다.
        /// </summary>
        /// <returns>defineList의 정의 심볼들을 ';'로 구분한 문자열</returns>
        public string GetDefineLine()
        {
            StringBuilder sb = new StringBuilder();
            // defineList의 각 정의 심볼을 순회합니다.
            foreach (string define in defineList)
            {
                // StringBuilder에 정의 심볼을 추가합니다.
                sb.Append(define);
                // 각 정의 심볼 뒤에 ';' 문자를 추가합니다.
                sb.Append(";");
            }

            // 완성된 문자열을 반환합니다.
            return sb.ToString();
        }

        /// <summary>
        /// 현재 defineList의 내용이 초기화 시 가져온 defineLine 문자열과 다른지 확인하여 변경 사항이 있는지 여부를 반환합니다.
        /// </summary>
        /// <returns>변경 사항이 있으면 true, 없으면 false</returns>
        public bool HasChanges()
        {
            // 현재 defineList로 생성한 문자열과 초기 defineLine 문자열을 비교합니다.
            return defineLine != GetDefineLine();
        }

        /// <summary>
        /// 현재 defineList의 변경 사항을 PlayerSettings에 적용합니다.
        /// 초기 defineLine과 현재 defineList로 생성한 문자열이 다른 경우에만 PlayerSettings를 업데이트합니다.
        /// Unity 2023 이상 버전과 이전 버전에 따라 다른 PlayerSettings API를 사용합니다.
        /// </summary>
        public void ApplyDefines()
        {
            // 현재 defineList로 새로운 정의 심볼 문자열을 생성합니다.
            string newDefineLine = GetDefineLine();

            // 초기 defineLine과 새로운 defineLine이 다른 경우에만 PlayerSettings를 업데이트합니다.
            if (defineLine != newDefineLine)
            {
                // 현재 활성화된 빌드 타겟 그룹에 새로운 정의 심볼 문자열을 설정합니다.
#if UNITY_2023_1_OR_NEWER // Unity 2023.1 이상 버전에 대한 조건부 컴파일
                PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)), newDefineLine);
#else // 이전 Unity 버전에 대한 조건부 컴파일
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), newDefineLine);
#endif

                // 변경 사항을 적용했으므로 defineLine을 업데이트된 문자열로 갱신합니다.
                defineLine = newDefineLine;
            }
        }
    }
}