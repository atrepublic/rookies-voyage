// 스크립트 설명: 무기 드롭 아이템의 동작을 처리하는 클래스입니다.
// 무기 데이터 설정 및 획득 시 캐릭터에게 무기를 장착시키는 기능을 구현합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class WeaponDropBehavior : BaseDropBehavior // BaseDropBehavior 상속 (이전 파일에서 정의된 것으로 가정)
    {
        [SerializeField]
        [Tooltip("이 드롭 아이템에 해당하는 무기 데이터")] // 주요 변수 한글 툴팁
        WeaponData weapon; // 무기 데이터

        [SerializeField]
        [Tooltip("이 드롭 아이템에 해당하는 무기 레벨")] // 주요 변수 한글 툴팁
        int weaponLevel; // 무기 레벨

        /// <summary>
        /// 무기 드롭 아이템의 무기 데이터와 레벨을 설정합니다.
        /// </summary>
        /// <param name="weapon">설정할 무기 데이터.</param>
        /// <param name="weaponLevel">설정할 무기 레벨.</param>
        public void SetWeaponData(WeaponData weapon, int weaponLevel)
        {
            if (weapon == null)
            {
                Debug.LogError("무기 데이터가 Null입니다!"); // 한글 로그 메시지

                return;
            }

            this.weapon = weapon;
            this.weaponLevel = weaponLevel;
        }

        /// <summary>
        /// 무기 드롭 아이템 획득에 대한 보상을 적용합니다.
        /// 캐릭터에게 해당 무기를 장착시킵니다.
        /// </summary>
        /// <param name="autoReward">자동 보상 적용 여부 (이 스크립트에서는 사용되지 않음).</param>
        public override void ApplyReward(bool autoReward = false)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour(); // 현재 플레이어 캐릭터의 Behaviour 가져오기 (CharacterBehaviour에 정의된 것으로 가정)
            if(characterBehaviour != null)
            {
                // 캐릭터에게 무기를 장착시키고 업그레이드 정보를 적용합니다.
                // SetGun 함수와 GetUpgrade 함수는 CharacterBehaviour 및 WeaponData에 정의된 것으로 가정
                characterBehaviour.SetGun(weapon, weapon.GetUpgrade(weaponLevel), true, true, false);
            }
        }
    }
}