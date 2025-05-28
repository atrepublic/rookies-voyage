// CoreEditor.cs
// 이 스크립트는 Unity 에디터에서 Watermelon Core 시스템의 전반적인 기능을 관리하고 접근하는 정적 클래스입니다.
// 코어 시스템의 폴더 경로, 설정 값들을 제공하며, Core Settings ScriptableObject 로딩 및 적용, 경로 포맷팅 기능을 포함합니다.
// 에디터 시작 시 자동으로 초기화됩니다.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    // Unity 에디터가 로드될 때 이 클래스를 초기화하도록 지정합니다.
    [InitializeOnLoad]
    public static class CoreEditor
    {
        // 코어 시스템의 루트 폴더 경로입니다.
        public static string FOLDER_CORE { get; private set; }

        // 코어 모듈이 위치하는 폴더 경로입니다.
        public static string FOLDER_CORE_MODULES => Path.Combine(FOLDER_CORE, "Modules");

        // 게임 데이터가 저장되는 폴더 경로입니다. CoreSettings에서 설정됩니다.
        public static string FOLDER_DATA;
        // 게임 씬 파일이 저장되는 폴더 경로입니다. CoreSettings에서 설정됩니다.
        public static string FOLDER_SCENES;

        // 커스텀 인스펙터 사용 여부입니다. CoreSettings에서 설정됩니다.
        public static bool UseCustomInspector { get; private set; } = true;
        // Hierarchy 창에 아이콘 사용 여부입니다. CoreSettings에서 설정됩니다.
        public static bool UseHierarchyIcons { get; private set; } = true;

        // 초기화 씬 자동 로드 여부입니다. CoreSettings에서 설정됩니다.
        public static bool AutoLoadInitializer { get; private set; } = true;
        // 초기화 씬의 이름입니다. CoreSettings에서 설정됩니다.
        public static string InitSceneName { get; private set; } = "Init";

        // 광고 더미의 배경 색상입니다. CoreSettings에서 설정됩니다.
        public static Color AdsDummyBackgroundColor { get; private set; } = new Color(0.2f, 0.2f, 0.3f);
        // 광고 더미의 메인 색상입니다. CoreSettings에서 설정됩니다.
        public static Color AdsDummyMainColor { get; private set; } = new Color(0.2f, 0.3f, 0.7f);

        // Watermelon 프로모션 표시 여부입니다. CoreSettings에서 설정됩니다.
        public static bool ShowWatermelonPromotions { get; private set; } = true;

        /// <summary>
        /// CoreEditor 클래스의 정적 생성자입니다.
        /// 에디터 로드 시 자동으로 호출되며 초기화 함수를 실행합니다.
        /// </summary>
        static CoreEditor()
        {
            Init(); // 초기화 함수를 호출합니다.
        }

        /// <summary>
        /// CoreEditor를 초기화하고 Core Settings ScriptableObject를 로드하거나 생성하는 함수입니다.
        /// Core Settings가 없으면 경고를 출력하고 기본 경로에 새로 생성합니다.
        /// </summary>
        private static void Init()
        {
            // 프로젝트에서 CoreSettings 에셋을 찾습니다.
            CoreSettings coreSettings = EditorUtils.GetAsset<CoreSettings>();
            if (coreSettings == null)
            {
                // 에디터가 업데이트 또는 컴파일 중이면 완료될 때까지 기다렸다가 다시 Init을 호출합니다.
                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    EditorApplication.delayCall += Init;

                    return;
                }

                // Core Settings 에셋을 찾을 수 없다는 경고 메시지를 출력합니다.
                Debug.LogWarning("[Watermelon Core]: Core Settings asset cannot be found in the project. This asset is required for the proper functionality of the modules.");

                // 새로운 CoreSettings ScriptableObject 인스턴스를 생성합니다.
                coreSettings = ScriptableObject.CreateInstance<CoreSettings>();

                // 기본 코어 폴더 경로를 설정합니다.
                FOLDER_CORE = Path.Combine("Assets", "Watermelon Core");

                // 코어 폴더가 존재하지 않으면 새로 생성합니다.
                if (!AssetDatabase.IsValidFolder(FOLDER_CORE))
                {
                    AssetDatabase.CreateFolder("Assets/", "Watermelon Core");
                }

                // CoreSettings 에셋을 지정된 경로에 생성하고 저장합니다.
                AssetDatabase.CreateAsset(coreSettings, Path.Combine("Assets", "Watermelon Core", "Core Settings.asset"));
                // 변경사항을 AssetDatabase에 저장합니다.
                AssetDatabase.SaveAssets();
                // AssetDatabase를 새로고침하여 변경사항을 반영합니다.
                AssetDatabase.Refresh();
            }
            else // CoreSettings 에셋이 존재하는 경우
            {
                // 기존 CoreSettings 에셋의 경로를 가져와 코어 폴더 경로를 설정합니다.
                FOLDER_CORE = AssetDatabase.GetAssetPath(coreSettings).Replace(coreSettings.name + ".asset", "");
            }

            // 로드하거나 생성된 CoreSettings 에셋의 설정을 CoreEditor의 정적 변수에 적용합니다.
            ApplySettings(coreSettings);
        }

        /// <summary>
        /// 제공된 CoreSettings 객체의 설정 값들을 CoreEditor의 정적 변수에 적용하는 함수입니다.
        /// </summary>
        /// <param name="settings">적용할 설정 값을 포함하는 CoreSettings 객체</param>
        public static void ApplySettings(CoreSettings settings)
        {
            // 폴더 경로 설정 값을 적용합니다.
            FOLDER_DATA = settings.DataFolder;
            FOLDER_SCENES = settings.ScenesFolder;

            // 초기화 관련 설정 값을 적용합니다.
            InitSceneName = settings.InitSceneName;
            AutoLoadInitializer = settings.AutoLoadInitializer;

            // 에디터 관련 설정 값을 적용합니다.
            UseCustomInspector = settings.UseCustomInspector;
            UseHierarchyIcons = settings.UseHierarchyIcons;

            // 광고 관련 설정 값을 적용합니다.
            AdsDummyBackgroundColor = settings.AdsDummyBackgroundColor;
            AdsDummyMainColor = settings.AdsDummyMainColor;

            // 기타 설정 값을 적용합니다.
            ShowWatermelonPromotions = settings.ShowWatermelonPromotions;
        }

        /// <summary>
        /// 경로 문자열에 포함된 특정 키워드('{CORE_MODULES}', '{CORE_DATA}', '{CORE}')를 실제 폴더 경로로 대체하여 포맷팅하는 함수입니다.
        /// </summary>
        /// <param name="path">포맷팅할 경로 문자열</param>
        /// <returns>키워드가 실제 경로로 대체된 포맷팅된 문자열</returns>
        public static string FormatPath(string path)
        {
            // 문자열의 키워드를 실제 폴더 경로로 대체하여 반환합니다.
            return path.Replace("{CORE_MODULES}", FOLDER_CORE_MODULES)
                       .Replace("{CORE_DATA}", FOLDER_DATA)
                       .Replace("{CORE}", FOLDER_CORE);
        }
    }
}