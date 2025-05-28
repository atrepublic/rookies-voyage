// SaveController.cs
// 이 스크립트는 게임 저장/로드 시스템의 핵심 컨트롤러입니다.
// GlobalSave 객체를 관리하고 파일에 저장하거나 파일로부터 로드하는 기능을 제공합니다.
// 자동 저장, 저장 필요 상태 추적, 프리셋 저장/로드, 저장 파일 삭제 등의 기능을 포함합니다.
// 게임 전반에서 접근 가능한 정적 클래스입니다.

using System;
using System.Collections;
using UnityEngine;
using System.Threading; // 스레드 사용을 위해 필요

namespace Watermelon
{
    // Unity가 어셈블리 언로드 시 이 정적 클래스의 UnloadStatic() 메서드를 호출하도록 지정합니다.
    [StaticUnload]
    public static class SaveController
    {
        // 게임 저장 파일의 기본 이름입니다.
        private const string SAVE_FILE_NAME = "save";

        // 게임의 전역 저장 데이터를 담고 있는 GlobalSave 객체입니다.
        private static GlobalSave globalSave;

        // 저장 데이터가 성공적으로 로드되었는지 여부를 나타냅니다.
        private static bool isSaveLoaded;
        // 저장 로드 상태를 가져옵니다.
        public static bool IsSaveLoaded => isSaveLoaded;

        // 현재 저장해야 할 변경 사항이 있는지 여부를 나타냅니다.
        private static bool isSaveRequired;

        // 현재 게임 플레이 시간입니다. GlobalSave에서 계산된 값을 가져옵니다.
        public static float GameTime => globalSave.GameTime;

        // 게임이 마지막으로 종료된 날짜 및 시간입니다. GlobalSave에서 가져옵니다.
        public static DateTime LastExitTime => globalSave.LastExitTime;

        // 저장 데이터 로드가 완료되었을 때 발생하는 이벤트입니다.
        public static event SimpleCallback OnSaveLoaded;

        /// <summary>
        /// SaveController를 초기화하고 저장 데이터를 로드하거나 새 저장 데이터를 생성하는 함수입니다.
        /// 자동 저장 루틴을 시작할 수 있습니다.
        /// </summary>
        /// <param name="autoSaveDelay">자동 저장 간격 (초). 0보다 크면 자동 저장 활성화.</param>
        /// <param name="clearSave">기존 저장 데이터를 무시하고 새 저장 데이터를 생성할지 여부 (기본값: false)</param>
        /// <param name="overrideTime">초기 게임 시간을 재정의할 값 (기본값: -1f, 현재 Time.time 사용)</param>
        public static void Init(float autoSaveDelay, bool clearSave = false, float overrideTime = -1f)
        {
            // 데이터 직렬화/역직렬화를 위한 Serializer를 초기화합니다.
            Serializer.Init();

            // 저장 콜백을 받기 위한 GameObject를 생성합니다. Hierarchy에서 숨겨집니다.
            GameObject saveCallbackReciever = new GameObject("[SAVE CALLBACK RECIEVER]");
            saveCallbackReciever.hideFlags = HideFlags.HideInHierarchy;

            // 씬 전환 시 파괴되지 않도록 설정합니다.
            GameObject.DontDestroyOnLoad(saveCallbackReciever);

            // Unity MonoBehaviour 콜백(OnDestroy, OnApplicationFocus)을 받기 위한 컴포넌트를 추가합니다.
            UnityCallbackReciever unityCallbackReciever = saveCallbackReciever.AddComponent<UnityCallbackReciever>();

            // clearSave 플래그에 따라 새 저장 데이터를 생성하거나 기존 저장 데이터를 로드합니다.
            if (clearSave)
            {
                InitClear(overrideTime != -1f ? overrideTime : Time.time); // 새 저장 데이터 생성
            }
            else
            {
                Load(overrideTime != -1f ? overrideTime : Time.time); // 기존 저장 데이터 로드
            }

            // 자동 저장 간격이 0보다 크면 자동 저장 코루틴을 시작합니다.
            if (autoSaveDelay > 0)
            {
                // Unity 콜백 리시버를 통해 자동 저장 코루틴을 시작합니다.
                unityCallbackReciever.StartCoroutine(AutoSaveCoroutine(autoSaveDelay));
            }
        }

        /// <summary>
        /// GlobalSave 객체의 현재 게임 시간을 업데이트하는 함수입니다.
        /// 프레임 업데이트마다 호출되어야 합니다.
        /// </summary>
        /// <param name="time">현재 게임 시간 (예: Time.time)</param>
        public static void UpdateTime(float time)
        {
            // GlobalSave 객체의 시간을 업데이트합니다.
            globalSave.Time = time;
        }

        /// <summary>
        /// 지정된 해시(hash)를 가진 저장 객체(ISaveObject 구현체)를 GlobalSave에서 찾아 반환하는 제네릭 함수입니다.
        /// 저장 데이터가 아직 로드되지 않았으면 오류를 로그하고 기본값을 반환합니다.
        /// </summary>
        /// <typeparam name="T">찾고자 하는 저장 객체의 타입 (ISaveObject 인터페이스 구현 및 기본 생성자 필수)</typeparam>
        /// <param name="hash">찾고자 하는 저장 객체의 해시 값</param>
        /// <returns>찾거나 생성된 저장 객체 인스턴스</returns>
        public static T GetSaveObject<T>(int hash) where T : ISaveObject, new()
        {
            // 저장 데이터가 로드되지 않았으면 오류를 로그하고 기본값을 반환합니다.
            if (!isSaveLoaded)
            {
                Debug.LogError("Save controller has not been initialized");
                return default;
            }

            // GlobalSave 객체에서 해시를 사용하여 저장 객체를 찾아 반환합니다.
            return globalSave.GetSaveObject<T>(hash);
        }

        /// <summary>
        /// 지정된 고유 이름(uniqueName)의 해시 값을 사용하여 저장 객체(ISaveObject 구현체)를 찾아 반환하는 제네릭 함수입니다.
        /// </summary>
        /// <typeparam name="T">찾고자 하는 저장 객체의 타입 (ISaveObject 인터페이스 구현 및 기본 생성자 필수)</typeparam>
        /// <param name="uniqueName">찾고자 하는 저장 객체의 고유 이름</param>
        /// <returns>찾거나 생성된 저장 객체 인스턴스</returns>
        public static T GetSaveObject<T>(string uniqueName) where T : ISaveObject, new()
        {
            // 고유 이름의 해시 값을 계산하여 GetSaveObject(int hash) 함수를 호출하고 결과를 반환합니다.
            return GetSaveObject<T>(uniqueName.GetHashCode());
        }

        /// <summary>
        /// 새 GlobalSave 객체를 생성하고 초기화하여 저장 데이터로 설정하는 함수입니다.
        /// 기존 저장 데이터는 무시됩니다.
        /// </summary>
        /// <param name="time">초기 게임 시간</param>
        private static void InitClear(float time)
        {
            // 새 GlobalSave 객체를 생성합니다.
            globalSave = new GlobalSave();
            // 새 GlobalSave 객체를 초기화합니다.
            globalSave.Init(time);

            // 콘솔에 새 저장 데이터가 생성되었음을 로그합니다.
            Debug.Log("[Save Controller]: Created clear save!");

            // 저장 데이터가 로드되었음을 표시합니다.
            isSaveLoaded = true;
        }

        /// <summary>
        /// 파일로부터 기존 GlobalSave 객체를 로드하고 초기화하는 함수입니다.
        /// 저장 데이터 로드가 이미 완료되었으면 아무런 동작도 하지 않습니다.
        /// </summary>
        /// <param name="time">초기 게임 시간</param>
        private static void Load(float time)
        {
            // 저장 데이터가 이미 로드되었으면 함수를 종료합니다.
            if (isSaveLoaded)
                return;

            // 설정된 SaveWrapper를 사용하여 저장 파일을 읽고 GlobalSave 객체로 역직렬화합니다.
            // 파일이 없으면 새 GlobalSave 객체를 생성합니다.
            globalSave = BaseSaveWrapper.ActiveWrapper.Load(SAVE_FILE_NAME);

            // 로드되거나 새로 생성된 GlobalSave 객체를 초기화합니다.
            globalSave.Init(time);

            // 콘솔에 저장 데이터가 로드되었음을 로그합니다.
            Debug.Log("[Save Controller]: Save is loaded!");

            // 저장 데이터가 로드되었음을 표시합니다.
            isSaveLoaded = true;

            // 저장 로드 완료 이벤트를 발생시킵니다.
            OnSaveLoaded?.Invoke();
        }

        /// <summary>
        /// 현재 GlobalSave 객체의 데이터를 파일에 저장하는 함수입니다.
        /// 저장 필요 상태이거나 강제 저장인 경우에만 저장합니다.
        /// 스레드를 사용하여 비동기적으로 저장할 수 있습니다.
        /// </summary>
        /// <param name="forceSave">저장 필요 상태와 관계없이 강제로 저장할지 여부 (기본값: false)</param>
        /// <param name="useThreads">저장을 별도 스레드에서 비동기적으로 수행할지 여부 (기본값: true). SaveWrapper가 스레드를 지원해야 합니다.</param>
        public static void Save(bool forceSave = false, bool useThreads = true)
        {
            // 강제 저장이 아니고 저장 필요 상태도 아니면 함수를 종료합니다.
            if (!forceSave && !isSaveRequired) return;
            // GlobalSave 객체가 null이면 함수를 종료합니다.
            if (globalSave == null) return;

            // GlobalSave 객체의 Flush 작업을 수행하여 최신 상태를 반영합니다. 마지막 종료 시간은 업데이트합니다.
            globalSave.Flush(true);

            // 현재 활성화된 SaveWrapper를 가져옵니다.
            BaseSaveWrapper saveWrapper = BaseSaveWrapper.ActiveWrapper;
            // 스레드 사용이 설정되어 있고 SaveWrapper가 스레드를 지원하면
            if(useThreads && saveWrapper.UseThreads())
            {
                // 새 스레드를 생성하여 SaveWrapper의 저장 함수를 비동기적으로 실행합니다.
                Thread saveThread = new Thread(() => BaseSaveWrapper.ActiveWrapper.Save(globalSave, SAVE_FILE_NAME));
                saveThread.Start(); // 스레드를 시작합니다.
            }
            else // 스레드를 사용하지 않거나 SaveWrapper가 지원하지 않으면
            {
                // 메인 스레드에서 동기적으로 저장 함수를 실행합니다.
                BaseSaveWrapper.ActiveWrapper.Save(globalSave, SAVE_FILE_NAME);
            }

            // 콘솔에 게임이 저장되었음을 로그합니다.
            Debug.Log("[Save Controller]: Game is saved!");

            // 저장 필요 상태 플래그를 false로 재설정합니다.
            isSaveRequired = false;
        }

        /// <summary>
        /// 제공된 GlobalSave 객체의 데이터를 현재 게임 저장 파일에 저장하는 함수입니다.
        /// 프리셋 로드 후 현재 상태를 해당 파일에 다시 저장할 때 유용합니다.
        /// </param>
        /// <param name="globalSave">저장할 데이터가 포함된 GlobalSave 객체</param>
        public static void SaveCustom(GlobalSave globalSave)
        {
            // 제공된 GlobalSave 객체가 null이 아니면
            if(globalSave != null)
            {
                // GlobalSave 객체의 Flush 작업을 수행합니다. 마지막 종료 시간은 업데이트하지 않습니다.
                globalSave.Flush(false);

                // SaveWrapper를 사용하여 제공된 GlobalSave 객체를 기본 저장 파일 이름으로 저장합니다.
                BaseSaveWrapper.ActiveWrapper.Save(globalSave, SAVE_FILE_NAME);
            }
        }

        /// <summary>
        /// 게임 저장 데이터에 변경 사항이 발생하여 저장이 필요함을 표시하는 함수입니다.
        /// 자동 저장 또는 수동 저장 시 이 플래그를 확인하여 실제 저장 여부를 결정합니다.
        /// </summary>
        public static void MarkAsSaveIsRequired()
        {
            // 저장 필요 상태 플래그를 true로 설정합니다.
            isSaveRequired = true;
        }

        /// <summary>
        /// 지정된 지연 시간마다 자동으로 게임을 저장하는 코루틴입니다.
        /// Init 함수에서 autoSaveDelay가 0보다 클 때 시작됩니다.
        /// </summary>
        /// <param name="saveDelay">자동 저장 간격 (초)</param>
        /// <returns>코루틴 실행을 위한 IEnumerator</returns>
        private static IEnumerator AutoSaveCoroutine(float saveDelay)
        {
            // 지정된 지연 시간 동안 대기하기 위한 WaitForSeconds 객체입니다.
            WaitForSeconds waitForSeconds = new WaitForSeconds(saveDelay);

            // 무한 루프를 돌며 자동 저장을 반복합니다.
            while (true)
            {
                yield return waitForSeconds; // 지정된 시간만큼 대기합니다.

                Save(); // 게임 저장 함수를 호출합니다.
            }
        }

        /// <summary>
        /// 현재 GlobalSave 객체의 데이터를 지정된 전체 파일 이름으로 저장하는 함수입니다.
        /// 저장 프리셋 기능을 구현할 때 사용될 수 있습니다.
        /// </summary>
        /// <param name="fullFileName">저장할 파일의 전체 경로 및 이름</param>
        public static void PresetsSave(string fullFileName)
        {
            // GlobalSave 객체의 Flush 작업을 수행합니다. 마지막 종료 시간은 업데이트하지 않습니다.
            globalSave.Flush(false);

            // SaveWrapper를 사용하여 GlobalSave 객체를 지정된 파일 이름으로 저장합니다.
            BaseSaveWrapper.ActiveWrapper.Save(globalSave, fullFileName);
        }

        /// <summary>
        /// 현재 GlobalSave 객체에 포함된 저장 객체들의 정보를 콘솔에 출력하는 함수입니다.
        /// 디버깅 용도로 사용될 수 있습니다.
        /// </summary>
        public static void Info()
        {
            // GlobalSave 객체의 Info 함수를 호출하여 정보를 출력합니다.
            globalSave.Info();
        }

        /// <summary>
        /// 게임 저장 파일을 삭제하는 함수입니다.
        /// PlayerPrefs는 삭제하지 않습니다.
        /// </summary>
        public static void DeleteSaveFile()
        {
            // SaveWrapper를 사용하여 기본 저장 파일을 삭제합니다.
            BaseSaveWrapper.ActiveWrapper.Delete(SAVE_FILE_NAME);
        }

        /// <summary>
        /// 저장 파일로부터 GlobalSave 객체를 로드하여 반환하는 함수입니다.
        /// 현재 게임의 GlobalSave 객체를 변경하지 않고 별도로 로드할 때 사용됩니다.
        /// </summary>
        /// <returns>로드된 GlobalSave 객체 인스턴스</returns>
        public static GlobalSave GetGlobalSave()
        {
            // SaveWrapper를 사용하여 저장 파일을 로드하고 GlobalSave 객체로 역직렬화합니다.
            GlobalSave tempGlobalSave = BaseSaveWrapper.ActiveWrapper.Load(SAVE_FILE_NAME);

            // 로드된 GlobalSave 객체를 현재 게임 시간으로 초기화합니다.
            tempGlobalSave.Init(Time.time);

            // 로드된 GlobalSave 객체를 반환합니다.
            return tempGlobalSave;
        }

        /// <summary>
        /// Unity가 어셈블리를 언로드할 때 호출되어 정적 변수들을 초기 상태로 되돌리는 함수입니다.
        /// </summary>
        private static void UnloadStatic()
        {
            // 정적 변수들을 null 또는 기본값으로 재설정합니다.
            globalSave = null;

            isSaveLoaded = false;
            isSaveRequired = false;

            OnSaveLoaded = null; // 이벤트 핸들러를 해지합니다.
        }

        // MonoBehaviour 콜백을 받기 위한 내부 도우미 클래스입니다.
        // 이 클래스의 인스턴스가 SaveController::Init에서 생성되어 GameObject에 추가됩니다.
        private class UnityCallbackReciever : MonoBehaviour
        {
            /// <summary>
            /// 이 MonoBehaviour 인스턴스가 파괴될 때 호출됩니다.
            /// 에디터 플레이 모드 종료 시 저장 데이터를 강제로 저장합니다.
            /// </summary>
            private void OnDestroy()
            {
#if UNITY_EDITOR // 에디터 환경에서만 실행
                // 에디터 플레이 모드 종료 시 저장 데이터를 강제로 저장합니다.
                SaveController.Save(true);
#endif
            }

            /// <summary>
            /// 애플리케이션 포커스 상태가 변경될 때 호출됩니다.
            /// 에디터가 아닌 빌드에서 애플리케이션이 포커스를 잃을 때 저장합니다.
            /// </summary>
            /// <param name="focus">애플리케이션이 포커스를 얻었으면 true, 잃었으면 false</param>
            private void OnApplicationFocus(bool focus)
            {
#if !UNITY_EDITOR // 빌드된 애플리케이션 환경에서만 실행
                // 애플리케이션이 포커스를 잃었을 때 저장합니다.
                if(!focus) SaveController.Save();
#endif
            }
        }
    }
}