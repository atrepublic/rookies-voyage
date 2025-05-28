// 스크립트 설명: 드롭 가능한 모든 아이템 타입이 구현해야 하는 인터페이스입니다.
// 드롭 타입, 초기화, 언로드, 드롭 오브젝트 가져오기 기능을 정의합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 드롭 가능한 모든 아이템 타입이 구현해야 하는 인터페이스
    public interface IDropItem
    {
        // 드롭 아이템의 타입을 가져오는 프로퍼티
        public DropableItemType DropItemType { get; }

        /// <summary>
        /// 드롭 아이템 타입을 초기화합니다.
        /// </summary>
        public void Init();

        /// <summary>
        /// 드롭 아이템 타입을 언로드합니다.
        /// </summary>
        public void Unload();

        /// <summary>
        /// 주어진 드롭 데이터를 기반으로 드롭 아이템의 게임 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="dropData">드롭 아이템 데이터.</param>
        /// <returns>드롭 아이템 게임 오브젝트 또는 null.</returns>
        public GameObject GetDropObject(DropData dropData);
    }
}