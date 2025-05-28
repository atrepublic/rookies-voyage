// SimpleIntSave.cs
// 이 스크립트는 하나의 int 값을 저장하기 위한 간단한 저장 객체 클래스입니다.
// ISaveObject 인터페이스를 구현하여 게임 저장 시스템(SaveController)에서 관리될 수 있습니다.
// Unity의 직렬화 시스템으로 저장/로드됩니다.

using UnityEngine;

namespace Watermelon
{
    // Unity의 직렬화 시스템으로 저장하고 로드할 수 있도록 지정합니다.
    [System.Serializable]
    public class SimpleIntSave : ISaveObject
    {
        // 저장될 실제 int 값입니다.
        [Tooltip("저장될 int 값입니다.")]
        [SerializeField] int value;

        /// <summary>
        /// 저장된 int 값을 가져오거나 설정합니다.
        /// 이 속성을 통해 실제 저장된 값에 접근하고 변경할 수 있습니다.
        /// </summary>
        public virtual int Value
        {
            get => value; // 현재 저장된 값을 반환합니다.
            set
            {
                this.value = value; // 저장될 값을 설정합니다.
            }
        }

        /// <summary>
        /// 저장 객체의 현재 상태를 직렬화 가능한 데이터 구조로 동기화하는 함수입니다.
        /// SimpleIntSave의 경우 별도의 동기화 로직이 필요 없으므로 비어 있습니다.
        /// ISaveObject 인터페이스 요구 사항으로 존재합니다.
        /// </summary>
        public virtual void Flush() { }
    }
}