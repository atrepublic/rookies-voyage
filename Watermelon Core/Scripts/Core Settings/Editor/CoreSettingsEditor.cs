// CoreSettingsEditor.cs
// 이 스크립트는 Unity 에디터에서 CoreSettings ScriptableObject의 커스텀 인스펙터 창을 제공합니다.
// Core Settings의 값이 변경될 때 CoreEditor에 해당 변경사항을 즉시 적용하는 기능을 수행합니다.
// 또한 에디터 메뉴에 Core Settings를 선택하는 메뉴 항목을 추가합니다.

using UnityEditor;

namespace Watermelon
{
    // CoreSettings 타입에 대한 커스텀 에디터임을 지정합니다.
    [CustomEditor(typeof(CoreSettings))]
    public class CoreSettingsEditor : Editor
    {
        // 현재 편집 중인 CoreSettings 객체 참조입니다.
        private CoreSettings coreSettings;

        /// <summary>
        /// 이 에디터 창이 활성화될 때 호출됩니다.
        /// 대상 객체를 CoreSettings로 형변환하여 참조합니다.
        /// </summary>
        private void OnEnable()
        {
            // 편집 대상 객체를 CoreSettings 타입으로 가져옵니다.
            coreSettings = (CoreSettings)target;
        }

        /// <summary>
        /// 인스펙터 GUI를 그릴 때 호출됩니다.
        /// 기본 인스펙터 GUI를 그리고, 값 변경 시 CoreEditor에 설정을 적용합니다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // GUI 변경 감지를 시작합니다.
            EditorGUI.BeginChangeCheck();

            // CoreSettings의 기본 인스펙터 GUI를 그립니다. (SerializedObject를 사용한 필드 그리기)
            base.OnInspectorGUI();

            // GUI 변경이 감지되었는지 확인합니다.
            if (EditorGUI.EndChangeCheck())
            {
                // CoreSettings 객체가 유효하면 CoreEditor에 변경된 설정을 적용합니다.
                if (coreSettings != null)
                {
                    CoreEditor.ApplySettings(coreSettings);
                }
            }
        }

        /// <summary>
        /// Unity 에디터 메뉴에 "Window/Watermelon Core/Core Settings" 항목을 추가하는 함수입니다.
        /// 메뉴 클릭 시 Core Settings 에셋을 프로젝트 창에서 선택하고 강조 표시합니다.
        /// </summary>
        // 메뉴 항목을 추가하고 우선순위를 설정합니다.
        [MenuItem("Window/Watermelon Core/Core Settings", priority = 50)]
        private static void SelectSettings()
        {
            // 프로젝트에서 CoreSettings 에셋을 찾습니다.
            CoreSettings coreSettings = EditorUtils.GetAsset<CoreSettings>();
            // CoreSettings 에셋을 찾았으면
            if(coreSettings != null)
            {
                // 찾은 에셋을 현재 선택된 오브젝트로 설정합니다. (프로젝트 창에서 선택됨)
                Selection.activeObject = coreSettings;

                // 찾은 에셋을 프로젝트 창에서 깜빡이게 하여 강조 표시합니다.
                EditorGUIUtility.PingObject(coreSettings);
            }
        }
    }
}