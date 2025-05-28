// CorePreferences.cs
// 이 스크립트는 Unity 에디터의 Preferences(환경 설정) 창에 Watermelon Core 설정 메뉴를 추가합니다.
// Core Settings ScriptableObject를 Preferences 창에서 바로 편집할 수 있도록 인스펙터 UI를 제공합니다.

using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class CorePreferences
    {
        /// <summary>
        /// Unity 에디터의 Preferences 창에 새로운 SettingsProvider를 생성하고 등록하는 함수입니다.
        /// 이 함수는 [SettingsProvider] 속성에 의해 자동으로 호출됩니다.
        /// </summary>
        /// <returns>생성된 SettingsProvider 객체</returns>
        // SettingsProvider를 생성하고 Preferences 창에 등록합니다.
        [SettingsProvider]
        public static SettingsProvider CustomPreferencesMenu()
        {
            // Core Settings ScriptableObject 에셋을 찾습니다.
            CoreSettings coreSettings = EditorUtils.GetAsset<CoreSettings>();

            // Core Settings 에셋의 인스펙터 UI를 그리기 위한 Editor 객체를 생성합니다.
            Editor editor = null;
            if(coreSettings != null)
            {
                // CoreSettings 객체의 커스텀 인스펙터를 위한 Editor를 생성합니다.
                editor = Editor.CreateEditor(coreSettings);
            }

            // 새로운 SettingsProvider를 생성합니다.
            // 경로는 "Preferences/Watermelon Core"로 설정되고, 사용자 범위 설정을 가집니다.
            SettingsProvider provider = new SettingsProvider("Preferences/Watermelon Core", SettingsScope.User)
            {
                // Preferences 페이지에 표시될 라벨입니다.
                label = "Watermelon Core",

                // 이 메서드는 Preferences 페이지의 GUI를 그릴 때 호출됩니다.
                guiHandler = (searchContext) =>
                {
                    // GUI 레이아웃을 시작합니다. (수평 정렬)
                    EditorGUILayout.BeginHorizontal();

                    // 왼쪽에 10픽셀 간격을 둡니다.
                    GUILayout.Space(10);
                    // GUI 레이아웃을 시작합니다. (수직 정렬)
                    EditorGUILayout.BeginVertical();

                    // Core Settings Editor 객체가 유효하면 해당 인스펙터 GUI를 그립니다.
                    if (editor != null)
                    {
                        editor.OnInspectorGUI();
                    }
                    else // Core Settings 에셋을 찾을 수 없으면 메시지를 표시합니다.
                    {
                        EditorGUILayout.LabelField("Core Settings file can't be found!");
                    }

                    // 수직 및 수평 레이아웃을 종료합니다.
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                },

                // Preferences 창의 검색 바에서 이 설정을 찾기 위한 키워드를 정의합니다.
                keywords = new string[] { "Custom", "Preferences", "Watermelon Core", "Core" }
            };

            // 생성된 SettingsProvider를 반환합니다.
            return provider;
        }
    }
}