// 스크립트 설명: 사용자 정의 드롭 아이템의 정보를 정의하는 클래스입니다.
// 특정 드롭 타입과 그에 해당하는 프리팹을 연결하는 데 사용됩니다.
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable] // Unity 에디터에서 직렬화하여 표시 가능하도록 설정
    public class CustomDropItem : IDropItem // IDropItem 인터페이스 구현
    {
        [SerializeField]
        [Tooltip("사용자 정의 드롭 아이템의 타입")] // 주요 변수 한글 툴팁
        DropableItemType dropableItemType; // 드롭 아이템 타입
        // 드롭 아이템의 타입을 반환합니다.
        public DropableItemType DropItemType => dropableItemType;

        [SerializeField]
        [Tooltip("사용자 정의 드롭 아이템으로 사용될 게임 오브젝트 프리팹")] // 주요 변수 한글 툴팁
        GameObject prefab; // 드롭 아이템 프리팹
        // 드롭 아이템 프리팹을 반환합니다.
        public GameObject DropPrefab => prefab;

        /// <summary>
        /// 사용자 정의 드롭 아이템 클래스의 생성자입니다.
        /// </summary>
        /// <param name="dropableItemType">드롭 아이템 타입.</param>
        /// <param name="prefab">드롭 아이템으로 사용될 프리팹.</param>
        public CustomDropItem(DropableItemType dropableItemType, GameObject prefab)
        {
            this.dropableItemType = dropableItemType;
            this.prefab = prefab;
        }

        /// <summary>
        /// 드롭 아이템 타입 초기화 시 호출됩니다. (현재 기능 없음)
        /// </summary>
        public void Init()
        {
            // 초기화 로직 (필요하다면 여기에 추가)
        }

        /// <summary>
        /// 드롭 아이템 타입 언로드 시 호출됩니다. (현재 기능 없음)
        /// </summary>
        public void Unload()
        {
            // 언로드 로직 (필요하다면 여기에 추가)
        }

        /// <summary>
        /// 이 사용자 정의 드롭 아이템의 게임 오브젝트(프리팹)를 가져옵니다.
        /// </summary>
        /// <param name="dropData">드롭 아이템 데이터 (이 클래스에서는 사용되지 않음).</param>
        /// <returns>사용자 정의 드롭 아이템 프리팹 게임 오브젝트.</returns>
        public GameObject GetDropObject(DropData dropData)
        {
            return prefab; // 연결된 프리팹 반환
        }
    }
}