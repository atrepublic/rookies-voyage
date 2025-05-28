// Initializer.cs
// 이 스크립트는 게임 초기화 프로세스의 진입점 역할을 하는 MonoBehaviour입니다.
// 게임 시작 시 필요한 시스템 컴포넌트(ProjectInitSettings, EventSystem 등)를 설정하고,
// 초기화 모듈을 실행하며, 게임 씬 로딩을 시작합니다. 씬 전환 시 파괴되지 않고 유지됩니다.

#pragma warning disable 0649 // 사용되지 않는 필드에 대한 경고를 비활성화합니다. (SerializeField 필드에 사용)

using UnityEngine;
using UnityEngine.EventSystems;

#if MODULE_INPUT_SYSTEM // 새로운 입력 시스템 모듈이 활성화된 경우
using UnityEngine.InputSystem.UI; // InputSystemUIInputModule 네임스페이스를 가져옵니다.
#endif

namespace Watermelon
{
    // 이 스크립트의 실행 순서를 다른 기본 스크립트보다 훨씬 먼저(-999) 설정합니다.
    [DefaultExecutionOrder(-999)]
    public class Initializer : MonoBehaviour
    {
        // Initializer 인스턴스에 대한 정적 참조입니다. 싱글톤 패턴을 적용합니다.
        private static Initializer initializer;

        [Tooltip("프로젝트 초기화 설정을 담고 있는 ScriptableObject입니다.")]
        [SerializeField] ProjectInitSettings initSettings; // 프로젝트 초기화 설정 객체입니다.
        [Tooltip("씬에 존재하는 EventSystem에 대한 참조입니다.")]
        [SerializeField] EventSystem eventSystem; // 씬의 EventSystem 객체입니다.

        // 이 Initializer GameObject에 대한 정적 참조입니다.
        public static GameObject GameObject { get; private set; }
        // 이 Initializer Transform에 대한 정적 참조입니다.
        public static Transform Transform { get; private set; }

        // 프로젝트 초기화 설정 객체에 대한 정적 참조입니다.
        public static ProjectInitSettings InitSettings { get; private set; }

        // 수동 활성화 모드가 활성화되었는지 나타내는 플래그입니다.
        private bool manualActivation;

        /// <summary>
        /// MonoBehaviour 인스턴스가 로드될 때 호출됩니다.
        /// Initializer 인스턴스가 하나만 존재하도록 보장하고 필요한 컴포넌트 및 설정을 초기화합니다.
        /// </summary>
        public void Awake()
        {
            // 이미 Initializer 인스턴스가 존재하면 현재 인스턴스를 파괴하여 중복을 방지합니다.
            if (initializer != null)
            {
                //Destroy(gameObject); // 현재 GameObject를 파괴합니다.
                return; // 함수 실행을 종료합니다.
            }

            // 현재 인스턴스를 정적 참조에 할당합니다.
            initializer = this;

            // 수동 활성화 플래그를 기본값(false)으로 설정합니다.
            manualActivation = false;

            // 프로젝트 초기화 설정을 정적 참조에 할당합니다.
            InitSettings = initSettings;

            // 현재 GameObject와 Transform을 정적 참조에 할당합니다.
            GameObject = gameObject;
            Transform = transform;

#if MODULE_INPUT_SYSTEM // 새로운 입력 시스템 모듈이 활성화된 경우
            // EventSystem GameObject에 InputSystemUIInputModule 컴포넌트가 있는지 확인하고 없으면 추가합니다.
            eventSystem.gameObject.GetOrSetComponent<InputSystemUIInputModule>(); // 네임스페이스 제거
#else // 기본(레거시) 입력 시스템이 활성화된 경우
            // EventSystem GameObject에 StandaloneInputModule 컴포넌트가 있는지 확인하고 없으면 추가합니다.
            eventSystem.gameObject.GetOrSetComponent<StandaloneInputModule>();
#endif

            // 씬이 전환될 때 이 GameObject가 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);

            // 프로젝트 초기화 설정을 초기화하고 이 Initializer 인스턴스를 전달합니다.
            initSettings.Init(this);

                // ★ 펫설정 추가 ★
            //GameSettings.GetSettings().PetDatabase.Init();
        }

        /// <summary>
        /// MonoBehaviour 인스턴스가 활성화되고 첫 번째 프레임 업데이트 전에 호출됩니다.
        /// 수동 활성화 모드가 아니면 게임 로딩을 시작합니다.
        /// </summary>
        public void Start()
        {
            // 수동 활성화 모드가 아니면 게임 로딩을 시작합니다. (로딩 씬 사용)
            if (!manualActivation)
                LoadGame(true);
        }

        /// <summary>
        /// 게임 로딩 프로세스를 시작하는 함수입니다.
        /// 로딩 씬을 사용할지 여부에 따라 GameLoading의 적절한 로딩 함수를 호출합니다.
        /// </summary>
        /// <param name="loadingScene">로딩 씬을 사용할지 여부 (true: 사용, false: 사용 안 함)</param>
        public void LoadGame(bool loadingScene)
        {
            // 로딩 씬 사용 여부에 따라 다른 로딩 함수를 호출합니다.
            if (loadingScene)
            {
                GameLoading.LoadGameScene(); // 로딩 씬을 사용하는 게임 씬 로딩 시작
            }
            else
            {
                GameLoading.SimpleLoad(); // 씬 로딩 없이 로딩 작업만 수행하는 간단 로딩 시작
            }
        }

        /// <summary>
        /// Initializer의 자동 활성화를 비활성화하고 수동 활성화 모드를 활성화하는 함수입니다.
        /// 이 함수가 호출되면 Start()에서 자동으로 LoadGame()이 호출되지 않습니다.
        /// </summary>
        public void EnableManualActivation()
        {
            manualActivation = true; // 수동 활성화 플래그를 true로 설정합니다.
        }
    }
}