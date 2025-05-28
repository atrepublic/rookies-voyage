// 스크립트 설명: 무기 타입 드롭 아이템의 생성 정보를 제공하는 클래스입니다.
// IDropItem 인터페이스를 구현하여 드롭 매니저에서 사용됩니다.
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class WeaponDropItem : IDropItem // IDropItem 인터페이스 구현
    {
        // 드롭 아이템의 타입을 반환합니다. 이 클래스는 무기 타입입니다.
        public DropableItemType DropItemType => DropableItemType.Weapon;

        /// <summary>
        /// 주어진 드롭 데이터를 기반으로 무기 드롭 아이템의 게임 오브젝트(프리팹)를 가져옵니다.
        /// </summary>
        /// <param name="dropData">드롭 아이템 데이터.</param>
        /// <returns>무기 드롭 아이템 프리팹 게임 오브젝트 또는 null.</returns>
        public GameObject GetDropObject(DropData dropData)
        {
            WeaponData weaponData = dropData.Weapon; // 드롭 데이터에서 무기 데이터 가져오기 (DropData에 정의된 것으로 가정)
            if (weaponData != null)
            {
                return weaponData.DropPrefab; // 무기 데이터에 연결된 드롭 프리팹 반환 (WeaponData에 DropPrefab이 정의된 것으로 가정)
            }

            return null; // 무기 데이터가 없으면 null 반환
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
    }
}