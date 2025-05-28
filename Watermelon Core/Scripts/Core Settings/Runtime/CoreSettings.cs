// CoreSettings.cs
// 이 스크립트는 게임 코어 시스템의 전반적인 설정을 관리하는 ScriptableObject입니다.
// 프로젝트 경로, 초기화 설정, 에디터 설정, 광고 더미 설정 등 다양한 핵심 설정을 포함합니다.
// 이를 통해 게임의 기본적인 동작 방식을 중앙에서 관리할 수 있습니다.

using System.IO;
using UnityEngine;

namespace Watermelon
{
    // Unity 에디터의 Assets > Create 메뉴에 "Data/Core/Core Settings" 경로로 생성 메뉴 항목을 추가합니다.
    // 파일 이름은 "Core Settings"로 기본 설정됩니다.
    [CreateAssetMenu(fileName = "Core Settings", menuName = "Data/Core/Core Settings")]
    public class CoreSettings : ScriptableObject
    {
        [Header("Path")] // 인스펙터에서 "Path" 헤더를 표시합니다.

        [Tooltip("프로젝트 내 데이터 파일이 저장될 기본 폴더 경로입니다.")]
        [SerializeField] string dataFolder = Path.Combine("Assets", "Project Files", "Data");
        // 데이터 파일이 저장되는 폴더의 경로를 가져옵니다.
        public string DataFolder => dataFolder;

        [Tooltip("게임 씬 파일이 저장될 기본 폴더 경로입니다.")]
        [SerializeField] string scenesFolder = Path.Combine("Assets", "Project Files", "Game", "Scenes");
        // 게임 씬 파일이 저장되는 폴더의 경로를 가져옵니다.
        public string ScenesFolder => scenesFolder;

        [Header("Init")] // 인스펙터에서 "Init" 헤더를 표시합니다.

        [Tooltip("게임을 초기화하는 데 사용될 씬의 이름입니다. 이 씬은 게임 시작 시 로드될 수 있습니다.")]
        [SerializeField] string initSceneName = "Init";
        // 초기화 씬의 이름을 가져옵니다.
        public string InitSceneName => initSceneName;

        [Tooltip("게임 시작 시 초기화 씬 로딩 모듈을 자동으로 로드할지 여부를 설정합니다.")]
        [SerializeField] bool autoLoadInitializer = true;
        // 초기화 모듈을 자동으로 로드할지 여부를 가져옵니다.
        public bool AutoLoadInitializer => autoLoadInitializer;

        [Header("Editor")] // 인스펙터에서 "Editor" 헤더를 표시합니다.

        [Tooltip("커스텀 인스펙터 기능을 사용할지 여부를 설정합니다. 활성화 시 더 보기 좋은 인스펙터 UI가 제공될 수 있습니다.")]
        [SerializeField] bool useCustomInspector = true;
        // 커스텀 인스펙터 사용 여부를 가져옵니다.
        public bool UseCustomInspector => useCustomInspector;

        [Tooltip("Hierarchy 창에서 오브젝트 옆에 아이콘을 표시하여 시각적으로 구분할지 여부를 설정합니다.")]
        [SerializeField] bool useHierarchyIcons = true;
        // Hierarchy 아이콘 사용 여부를 가져옵니다.
        public bool UseHierarchyIcons => useHierarchyIcons;

        [Header("Ads")] // 인스펙터에서 "Ads" 헤더를 표시합니다.

        [Tooltip("광고 더미(테스트용 광고)의 배경 색상입니다.")]
        [SerializeField] Color adsDummyBackgroundColor = new Color(0.1f, 0.2f, 0.35f, 1.0f);
        // 광고 더미의 배경 색상을 가져옵니다.
        public Color AdsDummyBackgroundColor => adsDummyBackgroundColor;

        [Tooltip("광고 더미의 메인 요소에 사용될 색상입니다.")]
        [SerializeField] Color adsDummyMainColor = new Color(0.15f, 0.37f, 0.6f, 1.0f);
        // 광고 더미의 메인 색상을 가져옵니다.
        public Color AdsDummyMainColor => adsDummyMainColor;

        [Header("Other")] // 인스펙터에서 "Other" 헤더를 표시합니다.

        [Tooltip("Watermelon 관련 프로모션 정보를 표시할지 여부를 설정합니다.")]
        [SerializeField] bool showWatermelonPromotions = true;
        // Watermelon 프로모션 표시 여부를 가져옵니다.
        public bool ShowWatermelonPromotions => showWatermelonPromotions;
    }
}