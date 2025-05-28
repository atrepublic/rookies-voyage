// SystemMessage.cs
// 이 스크립트는 게임 내에서 시스템 메시지 또는 로딩 화면을 표시하는 UI를 관리하는 MonoBehaviour입니다.
// 싱글톤 패턴으로 구현되어 메시지 표시 및 로딩 패널 제어 기능을 제공합니다.
// 필요한 Canvas와 CanvasScaler 컴포넌트가 GameObject에 추가되도록 요구합니다.

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic; // Add missing using directive for List

namespace Watermelon
{
    // 이 스크립트가 추가된 GameObject에 Canvas와 CanvasScaler 컴포넌트가 필수적으로 있도록 지정합니다.
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class SystemMessage : MonoBehaviour
    {
        // SystemMessage 인스턴스에 대한 정적 참조입니다. 싱글톤 패턴을 적용합니다.
        private static SystemMessage floatingMessage;

        [Header("Messages")] // 인스펙터에서 "Messages" 헤더를 표시합니다.
        [Tooltip("시스템 메시지 패널의 RectTransform입니다.")]
        [SerializeField] RectTransform messagePanelRectTransform; // 메시지 패널의 RectTransform
        [Tooltip("시스템 메시지 텍스트를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI messageText; // 메시지 텍스트 UI

        [Header("Loading")] // 인스펙터에서 "Loading" 헤더를 표시합니다.
        [Tooltip("로딩 중 표시될 로딩 패널 GameObject입니다.")]
        [SerializeField] GameObject loadingPanelObject; // 로딩 패널 GameObject
        [Tooltip("로딩 상태 메시지를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI loadingStatusText; // 로딩 상태 텍스트 UI
        [Tooltip("로딩 아이콘의 RectTransform입니다. 로딩 중에 회전됩니다.")]
        [SerializeField] RectTransform loadingIconRectTransform; // 로딩 아이콘의 RectTransform

        // 현재 실행 중인 애니메이션(페이드 인/아웃)에 대한 TweenCase 참조입니다.
        private TweenCase animationTweenCase;

        // 메시지 패널의 투명도를 제어하기 위한 CanvasGroup 컴포넌트입니다.
        private CanvasGroup messagePanelCanvasGroup;

        // 로딩 패널이 현재 활성화 상태인지 나타내는 플래그입니다.
        private bool isLoadingActive;

        /// <summary>
        /// MonoBehaviour 인스턴스가 로드될 때 호출됩니다.
        /// SystemMessage 인스턴스가 하나만 존재하도록 보장하고 필요한 컴포넌트 및 초기 상태를 설정합니다.
        /// </summary>
        private void Start()
        {
            // 이미 SystemMessage 인스턴스가 존재하면 현재 인스턴스를 파괴하여 중복을 방지합니다.
            if (floatingMessage != null)
            {
                Destroy(gameObject); // 현재 GameObject를 파괴합니다.
                return; // 함수 실행을 종료합니다.
            }

            // 현재 인스턴스를 정적 참조에 할당합니다.
            floatingMessage = this;

            // CanvasScaler 컴포넌트를 가져와 MatchSize 설정을 적용합니다.
            CanvasScaler canvasScaler = gameObject.GetComponent<CanvasScaler>();
            canvasScaler.MatchSize();

            // CanvasGroup 컴포넌트를 GameObject에 추가하고 참조를 저장합니다.
            messagePanelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            // 메시지 텍스트에 포인터 클릭 이벤트 리스너를 추가합니다. 클릭 시 OnPanelClick 함수가 호출됩니다.
            messageText.AddEvent(EventTriggerType.PointerClick, (data) => OnPanelClick());

            // 로딩 패널과 메시지 패널 GameObject를 초기에는 비활성화합니다.
            loadingPanelObject.SetActive(false);
            messagePanelRectTransform.gameObject.SetActive(false);
        }

        /// <summary>
        /// 매 프레임마다 호출됩니다.
        /// 로딩 패널이 활성화되어 있으면 로딩 아이콘을 회전시킵니다.
        /// </summary>
        private void Update()
        {
            // 로딩 패널이 활성화되어 있으면
            if (isLoadingActive)
            {
                // 로딩 아이콘을 매 프레임마다 회전시킵니다. (Z축 기준 -50도/초)
                loadingIconRectTransform.Rotate(0, 0, -50 * Time.deltaTime);
            }
        }

        /// <summary>
        /// 메시지 패널이 클릭되었을 때 호출되는 함수입니다.
        /// 현재 메시지 페이드 아웃 애니메이션을 중지하고 즉시 새 페이드 아웃 애니메이션을 시작합니다.
        /// </summary>
        private void OnPanelClick()
        {
            // 현재 애니메이션 트윈이 활성화되어 있고 완료되지 않았으면 중지합니다.
            if (floatingMessage.animationTweenCase != null && !floatingMessage.animationTweenCase.IsCompleted)
                floatingMessage.animationTweenCase.Kill();

            // 메시지 패널을 0.3초 동안 투명하게 페이드 아웃시키는 애니메이션을 시작합니다. 시간 스케일에 영향을 받지 않습니다.
            floatingMessage.animationTweenCase = floatingMessage.messagePanelCanvasGroup.DOFade(0, 0.3f, unscaledTime: true).SetEasing(Ease.Type.CircOut).OnComplete(delegate
            {
                // 페이드 아웃 완료 시 메시지 패널 GameObject를 비활성화합니다.
                floatingMessage.messagePanelRectTransform.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// 시스템 메시지를 화면에 표시하는 정적 함수입니다.
        /// 메시지 패널을 활성화하고 텍스트를 설정한 후, 지정된 시간 동안 표시했다가 페이드 아웃시킵니다.
        /// </summary>
        /// <param name="message">표시할 시스템 메시지 텍스트</param>
        /// <param name="duration">메시지가 표시될 시간 (초) (기본값: 2.5f)</param>
        public static void ShowMessage(string message, float duration = 2.5f)
        {
            // SystemMessage 인스턴스가 유효하면 메시지를 표시합니다.
            if(floatingMessage != null)
            {
                // 로딩 패널이 활성화되어 있으면 메시지를 표시하지 않고 함수를 종료합니다.
                if (floatingMessage.isLoadingActive) return;

                // 현재 애니메이션 트윈이 활성화되어 있고 완료되지 않았으면 중지합니다.
                if (floatingMessage.animationTweenCase != null && !floatingMessage.animationTweenCase.IsCompleted)
                    floatingMessage.animationTweenCase.Kill();

                // 메시지 텍스트를 설정합니다.
                floatingMessage.messageText.text = message;

                // 메시지 패널 GameObject를 활성화합니다.
                floatingMessage.messagePanelRectTransform.gameObject.SetActive(true);

                // 메시지 패널을 즉시 완전히 보이도록 설정하고
                floatingMessage.messagePanelCanvasGroup.alpha = 1.0f;
                // 지정된 시간(duration) 후에 메시지 패널을 페이드 아웃시키는 애니메이션을 시작합니다. 시간 스케일에 영향을 받지 않습니다.
                floatingMessage.animationTweenCase = Tween.DelayedCall(duration, delegate
                {
                    floatingMessage.animationTweenCase = floatingMessage.messagePanelCanvasGroup.DOFade(0, 0.5f, unscaledTime: true).SetEasing(Ease.Type.CircOut).OnComplete(delegate
                    {
                        floatingMessage.messagePanelRectTransform.gameObject.SetActive(false);
                    });
                }, unscaledTime: true);
            }
            else // SystemMessage 인스턴스가 없으면 오류 메시지를 출력합니다.
            {
                // 콘솔에 메시지를 로그하고, 모듈이 초기화되지 않았다는 오류 메시지를 출력합니다.
                Debug.Log("[System Message]: " + message);
                Debug.LogError("[System Message]: ShowMessage() method has called, but module isn't initialized!");
            }
        }

        /// <summary>
        /// 로딩 패널을 화면에 표시하는 정적 함수입니다.
        /// 메시지 패널이 활성화되어 있으면 비활성화하고, 로딩 패널을 활성화합니다.
        /// </summary>
        public static void ShowLoadingPanel()
        {
            // SystemMessage 인스턴스가 없거나 로딩 패널이 이미 활성화되어 있으면 함수를 종료합니다.
            if (floatingMessage == null) return;
            if (floatingMessage.isLoadingActive) return;

            // 메시지 패널이 활성 상태이면 현재 애니메이션을 중지하고 비활성화합니다.
            floatingMessage.animationTweenCase.KillActive();
            floatingMessage.messagePanelRectTransform.gameObject.SetActive(false);

            // 로딩 상태를 활성화하고 로딩 패널 GameObject를 활성화합니다.
            floatingMessage.isLoadingActive = true;
            floatingMessage.loadingPanelObject.SetActive(true);
        }

        /// <summary>
        /// 로딩 패널에 표시될 메시지를 변경하는 정적 함수입니다.
        /// </summary>
        /// <param name="message">표시할 로딩 상태 메시지 텍스트</param>
        public static void ChangeLoadingMessage(string message)
        {
            // SystemMessage 인스턴스가 없으면 함수를 종료합니다.
            if (floatingMessage == null) return;

            // 로딩 상태 텍스트를 업데이트합니다.
            floatingMessage.loadingStatusText.text = message;
        }

        /// <summary>
        /// 로딩 패널을 화면에서 숨기는 정적 함수입니다.
        /// 로딩 상태를 비활성화하고 로딩 패널 GameObject를 비활성화합니다.
        /// </summary>
        public static void HideLoadingPanel()
        {
            // SystemMessage 인스턴스가 없으면 함수를 종료합니다.
            if (floatingMessage == null) return;

            // 로딩 상태를 비활성화하고 로딩 패널 GameObject를 비활성화합니다.
            floatingMessage.isLoadingActive = false;
            floatingMessage.loadingPanelObject.SetActive(false);
        }
    }
}