// SavedDataContainer.cs
// 이 스크립트는 게임 저장 시스템에서 개별 저장 객체(ISaveObject)의 데이터를
// 직렬화 가능한 형태로 감싸는(컨테이너) 클래스입니다.
// 저장 객체의 고유 해시 값과 직렬화된 JSON 문자열을 포함하여,
// 데이터 로드 시 객체를 복원하고 저장 시 데이터를 동기화하는 역할을 합니다.

using UnityEngine;

namespace Watermelon
{
    // Unity의 직렬화 시스템으로 저장하고 로드할 수 있도록 지정합니다.
    [System.Serializable]
    public class SavedDataContainer
    {
        // 이 저장 데이터의 고유 해시 값입니다.
        [Tooltip("이 저장 데이터의 고유 식별 해시 값입니다.")]
        [SerializeField] int hash;
        // 저장 데이터의 해시 값을 가져옵니다.
        public int Hash => hash;

        // 저장 객체가 JSON 형식으로 직렬화된 문자열입니다.
        [Tooltip("저장 객체의 직렬화된 JSON 문자열 데이터입니다.")]
        [SerializeField] string json;

        // 저장 객체가 파일로부터 로드된 후 메모리에 복원되었는지 여부를 나타냅니다.
        // 이 플래그는 런타임 중에만 사용되며, 직렬화 시 포함되지 않습니다.
        // [System.NonSerialized] // CS0592 에러 해결: NonSerialized 특성은 필드에만 적용 가능합니다.
        public bool Restored { get; set; }

        // 복원된 실제 저장 객체 인스턴스에 대한 참조입니다.
        // 이 필드는 직렬화되지 않습니다.
        [System.NonSerialized]
        ISaveObject saveObject;
        // 실제 저장 객체 인스턴스를 가져옵니다.
        public ISaveObject SaveObject => saveObject;

        /// <summary>
        /// SavedDataContainer 클래스의 생성자입니다.
        /// 새로운 저장 객체와 그 해시 값을 사용하여 컨테이너를 초기화합니다.
        /// </summary>
        /// <param name="hash">저장 객체의 고유 해시 값</param>
        /// <param name="saveObject">저장할 실제 객체 인스턴스 (ISaveObject 구현체)</param>
        public SavedDataContainer(int hash, ISaveObject saveObject)
        {
            this.hash = hash; // 해시 값을 설정합니다.
            this.saveObject = saveObject; // 실제 저장 객체 참조를 저장합니다.
            Restored = true; // 새로 생성되었으므로 즉시 복원된 상태로 간주합니다.
        }

        /// <summary>
        /// 저장 객체의 현재 상태를 JSON 문자열로 동기화하는 함수입니다.
        /// GlobalSave가 파일 저장을 준비할 때 호출됩니다.
        /// </summary>
        public void Flush()
        {
            // 실제 저장 객체가 null이 아니면 해당 객체의 Flush() 함수를 먼저 호출하여 내부 데이터를 동기화합니다.
            if (saveObject != null) saveObject.Flush();

            // 컨테이너가 복원된 상태이면 (실제 객체가 메모리에 로드되어 있으면)
            if (Restored)
                // 실제 저장 객체를 JSON 문자열로 직렬화하여 'json' 필드에 저장합니다.
                json = JsonUtility.ToJson(saveObject);
        }

        /// <summary>
        /// 저장된 JSON 문자열 데이터를 역직렬화하여 실제 저장 객체 인스턴스로 복원하는 제네릭 함수입니다.
        /// GlobalSave 로드 시 호출됩니다.
        /// </summary>
        /// <typeparam name="T">복원할 저장 객체의 타입 (ISaveObject 구현체)</typeparam>
        public void Restore<T>() where T : ISaveObject
        {
            // JSON 문자열을 지정된 타입 T의 객체로 역직렬화하여 'saveObject' 필드에 저장합니다.
            saveObject = JsonUtility.FromJson<T>(json);
            // 복원이 완료되었음을 나타내는 플래그를 true로 설정합니다.
            Restored = true;
        }
    }
}