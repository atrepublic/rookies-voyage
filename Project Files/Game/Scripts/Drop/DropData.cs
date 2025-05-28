// 스크립트 설명: 드롭될 아이템의 데이터를 담는 클래스입니다.
// 드롭 타입, 화폐 타입, 무기/캐릭터 데이터, 수량, 레벨 등의 정보를 포함합니다.
using UnityEngine.Serialization; // FormerlySerializedAs 사용을 위한 네임스페이스
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // Unity 에디터에서 인스펙터 창에 표시될 수 있도록 직렬화 가능하게 설정
    [System.Serializable]
    public class DropData
    {
        // 이전 필드 이름(dropType)과의 호환성을 위한 속성
        [FormerlySerializedAs("dropType")]
        [Tooltip("드롭될 아이템의 타입")] // 주요 변수 한글 툴팁
        public DropableItemType DropType; // 드롭 아이템 타입

        // 이전 필드 이름(currencyType)과의 호환성을 위한 속성
        [FormerlySerializedAs("currencyType")]
        [Tooltip("드롭될 아이템이 화폐일 경우의 화폐 타입")] // 주요 변수 한글 툴팁
        public CurrencyType CurrencyType; // 화폐 타입

        // 이전 필드 이름(weapon)과의 호환성을 위한 속성
        [FormerlySerializedAs("weapon")]
        [Tooltip("드롭될 아이템이 무기일 경우의 무기 데이터")] // 주요 변수 한글 툴팁
        public WeaponData Weapon; // 무기 데이터

        // 이전 필드 이름(amount)과의 호환성을 위한 속성
        [FormerlySerializedAs("amount")]
        [Tooltip("드롭될 아이템의 수량")] // 주요 변수 한글 툴팁
        public int Amount; // 드롭 수량

        [Tooltip("드롭될 아이템이 캐릭터일 경우의 캐릭터 데이터")] // 주요 변수 한글 툴팁
        public CharacterData Character; // 캐릭터 데이터

        [Tooltip("드롭될 아이템의 레벨 정보")] // 주요 변수 한글 툴팁
        public int Level; // 아이템 레벨

        // 기본 생성자
        public DropData() { }

        /// <summary>
        /// 현재 DropData 객체를 복제하여 새로운 객체를 생성합니다.
        /// </summary>
        /// <returns>복제된 DropData 객체.</returns>
        public DropData Clone()
        {
            // 새로운 DropData 객체 생성
            DropData data = new DropData();

            // 현재 객체의 데이터를 새로운 객체에 복사
            data.DropType = DropType;
            data.CurrencyType = CurrencyType;
            data.Weapon = Weapon;
            data.Amount = Amount;
            data.Character = Character;
            data.Level = Level;

            return data; // 복제된 객체 반환
        }
    }
}