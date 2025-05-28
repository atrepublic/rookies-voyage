// GameLoading.cs
// 이 스크립트는 게임 로딩 및 씬 전환을 관리하는 정적 클래스입니다.
// 로딩 작업(LoadingTask)을 처리하고, 비동기 씬 로딩을 수행하며, 로딩 상태 및 메시지를 UI에 전달하는 기능을 제공합니다.
// 최소 로딩 시간 설정 및 수동 제어 모드도 지원합니다.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace Watermelon
{
    public static class GameLoading
    {
        // 최소 로딩 시간을 설정합니다. 이 시간 동안은 로딩 화면이 유지됩니다.
        private const float MINIMUM_LOADING_TIME = 2.0f;

        // 현재 진행 중인 비동기 씬 로드 작업에 대한 참조입니다.
        private static AsyncOperation loadingOperation;

        // 로딩 화면을 숨길 준비가 되었는지 나타내는 플래그입니다. 수동 제어 모드에서 사용됩니다.
        private static bool isReadyToHide;
        // 로딩 진행을 외부에서 MarkAsReadyToHide() 호출로 제어할지 나타내는 플래그입니다.
        private static bool manualControlMode;

        // 현재 로딩 화면에 표시될 메시지입니다.
        private static string loadingMessage;
        // 로딩 중 실행될 작업 목록입니다. (예: 데이터 로드, 리소스 초기화 등)
        private static List<LoadingTask> loadingTasks = new List<LoadingTask>();

        // 로딩 상태(진행률, 메시지)가 업데이트될 때 발생하는 이벤트입니다.
        public static event LoadingCallback OnLoading;
        // 로딩 과정이 완전히 종료되었을 때 발생하는 이벤트입니다.
        public static event Action OnLoadingFinished;

        /// <summary>
        /// 로딩 화면에 표시될 메시지를 설정하고 로딩 이벤트 핸들러를 호출합니다.
        /// </summary>
        /// <param name="message">표시할 로딩 메시지</param>
        public static void SetLoadingMessage(string message)
        {
            loadingMessage = message; // 메시지를 저장합니다.

            // 현재 로딩 진행 상태를 가져옵니다. 로드 작업이 없으면 0입니다.
            float progress = 0.0f;
            if (loadingOperation != null)
                progress = loadingOperation.progress;

            // 로딩 이벤트 핸들러를 호출하여 진행률과 메시지를 전달합니다.
            OnLoading?.Invoke(progress, message);
        }

        /// <summary>
        /// 로딩 과정에 새로운 로딩 작업(LoadingTask)을 추가합니다.
        /// </summary>
        /// <param name="loadingTask">추가할 LoadingTask 객체</param>
        public static void AddTask(LoadingTask loadingTask)
        {
            loadingTasks.Add(loadingTask); // 로딩 작업 목록에 추가합니다.
        }

        /// <summary>
        /// 다음 게임 씬을 비동기적으로 로드하고 로딩 작업을 처리하는 코루틴입니다.
        /// 최소 로딩 시간 및 수동 제어 모드를 지원합니다.
        /// </summary>
        /// <param name="onSceneLoaded">씬 로드 완료 후 호출될 콜백 함수 (선택 사항)</param>
        /// <returns>코루틴 실행을 위한 IEnumerator</returns>
        private static IEnumerator LoadSceneCoroutine(SimpleCallback onSceneLoaded = null)
        {
            isReadyToHide = false; // 로딩 화면 숨김 준비 플래그를 초기화합니다.

            float realtimeSinceStartup = Time.realtimeSinceStartup; // 로딩 시작 시간을 기록합니다.

            // 로딩 작업 목록을 순회하며 각 작업을 활성화하고 완료될 때까지 기다립니다.
            int taskIndex = 0;
            while(taskIndex < loadingTasks.Count)
            {
                // 작업이 아직 활성화되지 않았으면 활성화합니다.
                if(!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                // 작업이 완료되었으면 다음 작업으로 넘어갑니다.
                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null; // 다음 프레임까지 대기합니다.
            }

            // 현재 씬의 빌드 인덱스 다음 씬을 로드 대상으로 설정합니다.
            int sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            // 다음 씬이 빌드 설정에 없으면 오류 메시지를 출력합니다.
            if (SceneManager.sceneCount <= sceneIndex) // 수정: 비교 연산자 변경
                Debug.LogError("[Loading]: First scene is missing!");

            // 로딩 화면이 최소한 표시되어야 할 종료 시간을 계산합니다.
            float minimumFinishTime = realtimeSinceStartup + MINIMUM_LOADING_TIME;

            // 다음 씬을 비동기적으로 로드하는 작업을 시작합니다.
            loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);
            // 씬 로딩이 완료되어도 자동으로 전환되지 않도록 설정합니다.
            loadingOperation.allowSceneActivation = false;

            // 씬 로딩이 완료되거나 최소 로딩 시간이 경과할 때까지 기다립니다.
            while (!loadingOperation.isDone || Time.realtimeSinceStartup < minimumFinishTime) // 수정: realtimeSinceStartup 업데이트를 while 조건에 포함
            {
                yield return null; // 다음 프레임까지 대기합니다.

                // realtimeSinceStartup = Time.realtimeSinceStartup; // 이미 while 조건에서 업데이트됨

                // 로딩 이벤트 핸들러를 호출하여 진행률과 메시지를 전달합니다. (씬 로딩 중에는 1.0f로 표시)
                OnLoading?.Invoke(1.0f, loadingMessage);

                // 씬 로딩 진행률이 0.9 이상이면 씬 전환을 허용합니다. (실제 전환은 allowSceneActivation = true 이후에 발생)
                if (loadingOperation.progress >= 0.9f)
                {
                    loadingOperation.allowSceneActivation = true;
                }
            }

            // 수동 제어 모드가 활성화된 경우
            if(manualControlMode)
            {
                // MarkAsReadyToHide 메소드 호출 누락에 대한 디버그 경고를 지연 호출로 추가합니다.
                Tween.DelayedCall(10, () =>
                {
                    if (!isReadyToHide)
                        Debug.LogError("[Loading]: Seems like you forget to call MarkAsReadyToHide method to finish the loading process.");
                });

                // isReadyToHide 플래그가 true가 될 때까지 대기합니다.
                while (!isReadyToHide)
                {
                    yield return null; // 다음 프레임까지 대기합니다.
                }
            }

            // 로딩이 완료되었음을 나타내는 메시지와 함께 로딩 이벤트 핸들러를 호출합니다.
            OnLoading?.Invoke(1.0f, "Done");

            yield return null; // 한 프레임 더 대기합니다.

            // 씬 로드 완료 콜백 함수가 있으면 호출합니다.
            if (onSceneLoaded != null)
                onSceneLoaded.Invoke();

            // 로딩 종료 이벤트 핸들러를 호출합니다.
            OnLoadingFinished?.Invoke();
        }

        /// <summary>
        /// 씬 로딩 없이 로딩 작업만 처리하는 간단한 로딩 코루틴입니다.
        /// </summary>
        /// <param name="onSceneLoaded">로딩 작업 완료 후 호출될 콜백 함수 (선택 사항)</param>
        /// <returns>코루틴 실행을 위한 IEnumerator</returns>
        private static IEnumerator SimpleLoadCoroutine(SimpleCallback onSceneLoaded = null)
        {
            // float realtimeSinceStartup = Time.realtimeSinceStartup; // 사용되지 않음

            // 로딩 작업 목록을 순회하며 각 작업을 활성화하고 완료될 때까지 기다립니다.
            int taskIndex = 0;
            while (taskIndex < loadingTasks.Count)
            {
                // 작업이 아직 활성화되지 않았으면 활성화합니다.
                if (!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                // 작업이 완료되었으면 다음 작업으로 넘어갑니다.
                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null; // 다음 프레임까지 대기합니다.
            }

            // 로딩 작업 완료 후 콜백 함수가 있으면 호출합니다.
            if (onSceneLoaded != null)
                onSceneLoaded.Invoke();

            // 로딩 종료 이벤트 핸들러를 호출합니다.
            OnLoadingFinished?.Invoke();
        }

        /// <summary>
        /// 수동 제어 모드에서 로딩 화면을 숨길 준비가 되었음을 표시하는 함수입니다.
        /// 이 함수가 호출되어야 로딩 코루틴이 완료됩니다.
        /// </summary>
        public static void MarkAsReadyToHide()
        {
            isReadyToHide = true; // 로딩 화면 숨김 준비 플래그를 true로 설정합니다.
        }

        /// <summary>
        /// 로딩 화면의 진행을 외부에서 제어하는 수동 제어 모드를 활성화하는 함수입니다.
        /// </summary>
        public static void EnableManualControlMode()
        {
            manualControlMode = true; // 수동 제어 모드 플래그를 true로 설정합니다.
        }

        /// <summary>
        /// 로딩 작업을 처리하고 다음 게임 씬을 비동기적으로 로드하는 과정을 시작하는 함수입니다.
        /// 로딩 메시지를 설정하고 로드 코루틴을 실행합니다.
        /// </summary>
        /// <param name="onSceneLoaded">씬 로드 완료 후 호출될 콜백 함수 (선택 사항)</param>
        public static void LoadGameScene(SimpleCallback onSceneLoaded = null)
        {
            SetLoadingMessage("Loading.."); // 기본 로딩 메시지를 설정합니다.

            // 씬 로딩 코루틴을 실행합니다.
            Tween.InvokeCoroutine(LoadSceneCoroutine(onSceneLoaded));
        }

        /// <summary>
        /// 씬 로딩 없이 로딩 작업만 처리하는 간단한 로딩 과정을 시작하는 함수입니다.
        /// 간단 로딩 코루틴을 실행합니다.
        /// </summary>
        /// <param name="onSceneLoaded">로딩 작업 완료 후 호출될 콜백 함수 (선택 사항)</param>
        public static void SimpleLoad(SimpleCallback onSceneLoaded = null)
        {
            // 간단 로딩 코루틴을 실행합니다.
            Tween.InvokeCoroutine(SimpleLoadCoroutine(onSceneLoaded));
        }

        // 로딩 상태 및 메시지 업데이트 이벤트를 위한 델리게이트 정의입니다.
        public delegate void LoadingCallback(float state, string message);
    }
}