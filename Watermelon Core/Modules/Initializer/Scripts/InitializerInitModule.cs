// InitializerInitModule.cs
// 이 스크립트는 Initializer 시스템의 초기화 모듈을 정의합니다.
// Initializer 초기화 과정에서 로딩 화면의 수동 제어 모드를 설정하고,
// 시스템 메시지 표시를 위한 UI 프리팹을 생성하고 Initializer 하위에 연결합니다.

using UnityEngine;

namespace Watermelon
{
    // InitModule로 등록하며 모듈 이름, 활성화 여부, 실행 순서를 설정합니다.
    [RegisterModule("Initializer Settings", true, order: 999)]
    public class InitializerInitModule : InitModule
    {
        // 이 모듈의 이름을 반환합니다.
        public override string ModuleName => "Initializer Settings";

        [Tooltip("수동 모드가 활성화되면 GameLoading.MarkAsReadyToHide 메서드가 호출될 때까지 로딩 화면이 활성 상태를 유지합니다.")]
        [Header("Loading")] // 인스펙터에서 "Loading" 헤더를 표시합니다.
        [SerializeField] bool manualControlMode; // 로딩 화면 수동 제어 모드 활성화 여부입니다.

        [Space] // 인스펙터에 공백을 추가합니다.
        [Tooltip("시스템 메시지를 표시하는 데 사용될 UI 프리팹입니다. SystemMessage 컴포넌트를 포함해야 합니다.")]
        [SerializeField] GameObject systemMessagesPrefab; // 시스템 메시지 UI 프리팹에 대한 참조입니다.

        /// <summary>
        /// 이 초기화 모듈의 컴포넌트를 생성하고 설정하는 함수입니다.
        /// Initializer 초기화 과정 중에 호출됩니다.
        /// </summary>
        public override void CreateComponent()
        {
            // 수동 제어 모드가 활성화되어 있으면 GameLoading에서 수동 모드를 활성화합니다.
            if (manualControlMode)
                GameLoading.EnableManualControlMode();

            // 시스템 메시지 프리팹이 설정되어 있으면 처리합니다.
            if(systemMessagesPrefab != null)
            {
                // 프리팹이 SystemMessage 컴포넌트를 가지고 있는지 확인합니다.
                if(systemMessagesPrefab.GetComponent<SystemMessage>() != null)
                {
                    // 시스템 메시지 프리팹을 인스턴스화합니다.
                    GameObject messagesCanvasObject = Instantiate(systemMessagesPrefab);
                    // 인스턴스화된 GameObject의 이름을 원본 프리팹과 동일하게 설정합니다.
                    messagesCanvasObject.name = systemMessagesPrefab.name;
                    // Initializer GameObject의 자식으로 설정합니다.
                    messagesCanvasObject.transform.SetParent(Initializer.Transform);
                }
                else // SystemMessage 컴포넌트가 없으면 경고를 출력합니다.
                {
                    Debug.LogError("The Linked System Message prefab doesn't have the SystemMessage component attached to it.");
                }
            }
            else // 시스템 메시지 프리팹이 설정되어 있지 않으면 경고를 출력합니다.
            {
                Debug.LogWarning("The System Message prefab isn't linked. This may affect the user experience while playing your game.");
            }
        }
    }
}