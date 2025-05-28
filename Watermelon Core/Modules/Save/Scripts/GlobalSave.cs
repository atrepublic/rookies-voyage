// GlobalSave.cs
// 이 스크립트는 게임의 전역 저장 데이터를 포함하는 컨테이너 클래스입니다.
// 다양한 저장 객체들(ISaveObject 인터페이스를 구현하는 클래스 인스턴스)의 목록과
// 게임 시간, 마지막 종료 시간 등의 전역 정보를 관리합니다.
// 직렬화 가능하여 파일로 저장/로드될 수 있습니다.

using System;
using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    // Unity의 직렬화 시스템으로 저장하고 로드할 수 있도록 지정합니다.
    [Serializable]
    public class GlobalSave
    {
        // 게임에 포함된 저장 객체들의 컨테이너 배열입니다. (직렬화용)
        [SerializeField] SavedDataContainer[] saveObjects;
        // 게임에 포함된 저장 객체들의 컨테이너 목록입니다. (런타임 사용)
        private List<SavedDataContainer> saveObjectsList;

        // 저장된 게임 총 플레이 시간입니다.
        [Tooltip("게임이 저장된 시점까지의 총 플레이 시간 (초) 입니다.")]
        [SerializeField] float gameTime;
        // 마지막으로 저장된 시점 이후 현재까지의 플레이 시간을 더한 실제 게임 시간입니다.
        public float GameTime => gameTime + (Time - lastFlushTime);

        // 게임이 마지막으로 종료된 날짜 및 시간입니다.
        [Tooltip("게임이 마지막으로 저장되거나 종료된 날짜 및 시간입니다.")]
        [SerializeField] DateTime lastExitTime;
        // 마지막 종료 날짜 및 시간을 가져옵니다.
        public DateTime LastExitTime => lastExitTime;

        // 마지막으로 Flush(데이터 동기화)가 수행된 게임 시간입니다.
        private float lastFlushTime = 0;

        // 현재 게임 시간입니다. SaveController에 의해 설정됩니다.
        public float Time { get; set; }

        /// <summary>
        /// GlobalSave 객체를 초기화하는 함수입니다.
        /// 로드된 저장 객체 목록을 준비하고 마지막 플러시 시간을 설정합니다.
        /// </summary>
        /// <param name="time">현재 게임 시간</param>
        public void Init(float time)
        {
            // 로드된 saveObjects 배열이 null이면 새로운 목록을 생성합니다.
            if (saveObjects == null)
            {
                saveObjectsList = new List<SavedDataContainer>();
            }
            else // null이 아니면 배열에서 목록을 생성합니다.
            {
                saveObjectsList = new List<SavedDataContainer>(saveObjects);
            }

            // 저장 객체 컨테이너의 복원 상태 플래그를 초기화합니다.
            for (int i = 0; i < saveObjectsList.Count; i++)
            {
                saveObjectsList[i].Restored = false;
            }

            Time = time; // 현재 게임 시간을 설정합니다.
            lastFlushTime = Time; // 마지막 플러시 시간을 현재 게임 시간으로 설정합니다.
        }

        /// <summary>
        /// 런타임 중 변경된 저장 객체들의 데이터를 직렬화 가능한 상태로 동기화하고,
        /// 게임 시간을 업데이트하며, 마지막 종료 시간을 기록하는 함수입니다.
        /// </summary>
        /// <param name="updateLastExitTime">마지막 종료 시간을 현재 시간으로 업데이트할지 여부</param>
        public void Flush(bool updateLastExitTime)
        {
            // 런타임 목록(saveObjectsList)을 배열(saveObjects)로 변환하여 직렬화 준비를 합니다.
            saveObjects = saveObjectsList.ToArray();

            // 각 저장 객체 컨테이너에 대해 Flush 작업을 수행하여 내부 데이터를 직렬화 가능한 상태로 만듭니다.
            for (int i = 0; i < saveObjectsList.Count; i++)
            {
                SavedDataContainer saveObject = saveObjectsList[i];

                saveObject.Flush();
            }

            // 게임 총 플레이 시간을 업데이트합니다. (마지막 플러시 이후 경과 시간 추가)
            gameTime += Time - lastFlushTime;

            // 마지막 플러시 시간을 현재 게임 시간으로 업데이트합니다.
            lastFlushTime = Time;

            // 마지막 종료 시간을 업데이트하도록 설정되어 있으면 현재 시간으로 설정합니다.
            if(updateLastExitTime) lastExitTime = DateTime.Now;
        }

        /// <summary>
        /// 지정된 해시(hash)를 가진 저장 객체를 찾거나 새로 생성하여 반환하는 제네릭 함수입니다.
        /// 찾은 객체가 아직 복원되지 않았으면 데이터를 복원합니다.
        /// </summary>
        /// <typeparam name="T">찾거나 생성할 저장 객체의 타입 (ISaveObject 인터페이스 구현 및 기본 생성자 필수)</typeparam>
        /// <param name="hash">찾고자 하는 저장 객체의 해시 값</param>
        /// <returns>찾거나 생성된 저장 객체 인스턴스</returns>
        public T GetSaveObject<T>(int hash) where T : ISaveObject, new()
        {
            // 해시 값을 사용하여 저장 객체 컨테이너 목록에서 해당 컨테이너를 찾습니다.
            SavedDataContainer container = saveObjectsList.Find((container) => container.Hash == hash);

            // 컨테이너를 찾지 못했으면
            if (container == null)
            {
                // 지정된 타입 T의 새로운 저장 객체를 생성하고 새 컨테이너를 만듭니다.
                container = new SavedDataContainer(hash, new T());

                // 새로운 컨테이너를 목록에 추가합니다.
                saveObjectsList.Add(container);

            }
            else // 컨테이너를 찾았으면
            {
                // 컨테이너가 아직 복원되지 않았으면 데이터를 복원합니다.
                if (!container.Restored) container.Restore<T>();
            }
            // 컨테이너에서 실제 저장 객체를 가져와 지정된 타입 T로 형변환하여 반환합니다.
            return (T)container.SaveObject;
        }

        /// <summary>
        /// 지정된 고유 이름(uniqueName)의 해시 값을 사용하여 저장 객체를 찾거나 새로 생성하여 반환하는 제네릭 함수입니다.
        /// 내부적으로 GetSaveObject(int hash) 함수를 호출합니다.
        /// </summary>
        /// <typeparam name="T">찾거나 생성할 저장 객체의 타입 (ISaveObject 인터페이스 구현 및 기본 생성자 필수)</typeparam>
        /// <param name="uniqueName">찾고자 하는 저장 객체의 고유 이름</param>
        /// <returns>찾거나 생성된 저장 객체 인스턴스</returns>
        public T GetSaveObject<T>(string uniqueName) where T : ISaveObject, new()
        {
            // 고유 이름의 해시 값을 계산하여 GetSaveObject(int hash) 함수를 호출하고 결과를 반환합니다.
            return GetSaveObject<T>(uniqueName.GetHashCode());
        }

        /// <summary>
        /// 현재 GlobalSave에 포함된 모든 저장 객체의 정보(해시 및 객체 타입)를 콘솔에 출력하는 함수입니다.
        /// 디버깅 용도로 사용될 수 있습니다.
        /// </summary>
        public void Info()
        {
            // 저장 객체 컨테이너 목록을 순회하며 각 컨테이너의 정보를 로그합니다.
            foreach (var container in saveObjectsList)
            {
                Debug.Log("Hash: " + container.Hash); // 해시 값 출력
                Debug.Log("Save Object: " + container.SaveObject); // 저장 객체 인스턴스 정보 출력
            }
        }
    }
}