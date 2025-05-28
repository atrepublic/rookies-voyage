// LoadingGraphics.cs
// 이 스크립트는 게임 로딩 화면의 UI 요소(텍스트, 배경 이미지) 및 애니메이션을 관리하는 MonoBehaviour입니다.
// GameLoading 시스템의 이벤트에 반응하여 로딩 메시지를 업데이트하고 로딩 완료 시 UI를 페이드 아웃합니다.
// 씬 전환 시 파괴되지 않고 로딩 화면을 유지합니다.

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Watermelon
{
    public class LoadingGraphics : MonoBehaviour
    {
        [Tooltip("로딩 메시지를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI loadingText; // 로딩 메시지 텍스트 UI입니다.
        [Tooltip("로딩 화면의 배경 이미지를 표시할 Image 컴포넌트입니다.")]
        [SerializeField] Image backgroundImage; // 로딩 화면 배경 이미지 UI입니다.
        [Tooltip("UI 스케일링을 관리하는 CanvasScaler 컴포넌트입니다.")]
        [SerializeField] CanvasScaler canvasScaler; // CanvasScaler 컴포넌트입니다.
        [Tooltip("로딩 화면을 렌더링하는 데 사용되는 카메라입니다.")]
        [SerializeField] Camera loadingCamera; // 로딩 화면 카메라입니다.

        /// <summary>
        /// MonoBehaviour 인스턴스가 로드될 때 호출됩니다.
        /// 씬 전환 시 파괴되지 않도록 설정하고, CanvasScaler를 초기화하며, 초기 로딩 메시지를 설정합니다.
        /// </summary>
        private void Awake()
        {
            // 씬이 전환되어도 이 GameObject가 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);

            // CanvasScaler의 MatchSize 설정을 적용합니다.
            canvasScaler.MatchSize();

            // 초기 로딩 메시지를 설정하고 로딩 이벤트 핸들러를 호출합니다.
            OnLoading(0.0f, "Loading..");
        }

        /// <summary>
        /// MonoBehaviour 인스턴스가 활성화될 때 호출됩니다.
        /// GameLoading 시스템의 로딩 이벤트에 로딩 UI 업데이트 함수들을 연결합니다.
        /// </summary>
        private void OnEnable()
        {
            // GameLoading의 OnLoading 이벤트에 OnLoading 함수를 구독합니다. (로딩 상태 업데이트 시 호출)
            GameLoading.OnLoading += OnLoading;
            // GameLoading의 OnLoadingFinished 이벤트에 OnLoadingFinished 함수를 구독합니다. (로딩 완료 시 호출)
            GameLoading.OnLoadingFinished += OnLoadingFinished;
        }

        /// <summary>
        /// MonoBehaviour 인스턴스가 비활성화될 때 호출됩니다.
        /// GameLoading 시스템의 로딩 이벤트에서 로딩 UI 업데이트 함수들의 구독을 해지합니다.
        /// </summary>
        private void OnDisable()
        {
            // GameLoading의 OnLoading 이벤트 구독을 해지합니다.
            GameLoading.OnLoading -= OnLoading;
            // GameLoading의 OnLoadingFinished 이벤트 구독을 해지합니다.
            GameLoading.OnLoadingFinished -= OnLoadingFinished;
        }

        /// <summary>
        /// GameLoading 시스템의 OnLoading 이벤트에 의해 호출됩니다.
        /// 로딩 텍스트 UI에 현재 로딩 메시지를 표시합니다.
        /// </summary>
        /// <param name="state">로딩 진행 상태 (0.0f ~ 1.0f), 현재는 사용되지 않습니다.</param>
        /// <param name="message">표시할 로딩 메시지</param>
        private void OnLoading(float state, string message)
        {
            // 로딩 텍스트 UI의 내용을 업데이트합니다.
            loadingText.text = message;
        }

        /// <summary>
        /// GameLoading 시스템의 OnLoadingFinished 이벤트에 의해 호출됩니다.
        /// 로딩 완료 시 로딩 텍스트와 배경 이미지를 페이드 아웃시키고 GameObject를 파괴합니다.
        /// </summary>
        private void OnLoadingFinished()
        {
            // 로딩 텍스트를 0.6초 동안 투명하게 페이드 아웃시킵니다. 시간 스케일에 영향을 받지 않습니다.
            loadingText.DOFade(0.0f, 0.6f, unscaledTime: true);
            // 배경 이미지를 0.6초 동안 투명하게 페이드 아웃시키고 완료 시 GameObject를 파괴하는 콜백을 실행합니다.
            backgroundImage.DOFade(0.0f, 0.6f, unscaledTime: true).OnComplete(delegate
            {
                // 로딩 화면 GameObject를 파괴합니다.
                Destroy(gameObject);
            });
        }
    }
}