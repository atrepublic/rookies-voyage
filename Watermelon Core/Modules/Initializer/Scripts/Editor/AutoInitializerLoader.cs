// AutoInitializerLoader.cs
// 이 스크립트는 게임 시작 시 Initializer 프리팹을 자동으로 로드하고 초기화하는 정적 클래스입니다.
// Core Settings에서 자동 로드 설정이 활성화되어 있고 현재 씬이 초기화 씬이 아닌 경우에 작동합니다.
// 이를 통해 Initializer가 필요한 모든 씬에서 게임 코어 시스템이 올바르게 시작되도록 보장합니다.

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    // Unity 에디터가 로드될 때 이 클래스를 초기화하도록 지정합니다.
    [InitializeOnLoad]
    public static class AutoInitializerLoader
    {
        /// <summary>
        /// 런타임 시작 시 (첫 씬 로드 전에) 자동으로 호출되는 함수입니다.
        /// Core Settings에서 자동 초기화 로드가 활성화된 경우 Initializer 프리팹을 찾아 인스턴스화하고 초기화합니다.
        /// </summary>
        // 런타임 시작 시 (첫 씬 로드 전에) 자동으로 호출되도록 지정합니다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadMain()
        {
            // Core Settings에서 Initializer 자동 로드 설정이 비활성화되어 있으면 함수를 종료합니다.
            if (!CoreEditor.AutoLoadInitializer) return;

            // 현재 활성화된 씬을 가져옵니다.
            Scene currentScene = SceneManager.GetActiveScene();
            // 현재 씬이 유효하면
            if (currentScene != null)
            {
                // 현재 씬의 이름이 Core Settings에 설정된 초기화 씬 이름과 다르면
                if (currentScene.name != CoreEditor.InitSceneName)
                {
                    // 씬에서 Initializer 컴포넌트를 찾습니다. Unity 버전별로 다른 API를 사용합니다.
#if UNITY_6000 // Unity 2022 LTS 이후 버전에 해당하는 UNITY_6000 (또는 그에 준하는 심볼) 정의 시
                    Initializer initializer = Object.FindFirstObjectByType<Initializer>(); // 새로운 API 사용
#else // 그 외 Unity 버전 (레거시 API 사용)
                    Initializer initializer = Object.FindObjectOfType<Initializer>(); // 기존 API 사용
#endif

                    // Initializer 인스턴스를 찾지 못했으면 새로 생성합니다.
                    if (initializer == null)
                    {
                        // "Initializer" 이름의 GameObject 프리팹을 에셋 데이터베이스에서 찾습니다.
                        GameObject initializerPrefab = EditorUtils.GetAsset<GameObject>("Initializer");
                        // Initializer 프리팹을 찾았으면
                        if (initializerPrefab != null)
                        {
                            // 프리팹을 인스턴스화하여 씬에 추가합니다.
                            GameObject InitializerObject = Object.Instantiate(initializerPrefab);

                            // 인스턴스화된 GameObject에서 Initializer 컴포넌트를 가져옵니다.
                            initializer = InitializerObject.GetComponent<Initializer>();
                            // Initializer의 Awake 함수를 수동으로 호출하여 초기 설정을 완료합니다. (Instantiate 시 Awake는 바로 호출되지 않을 수 있음)
                            initializer.Awake();
                            // Initializer가 Start()에서 자동으로 게임 로딩을 시작하지 않도록 수동 활성화 모드를 활성화합니다.
                            initializer.EnableManualActivation();
                            // 로딩 씬 없이 간단 로딩(로딩 작업만 수행)을 시작합니다.
                            initializer.LoadGame(false);
                        }
                        else // Initializer 프리팹을 찾지 못했으면 오류 메시지를 출력합니다.
                        {
                            Debug.LogError("[Game]: Initializer prefab is missing!");
                        }
                    }
                }
            }
        }
    }
}