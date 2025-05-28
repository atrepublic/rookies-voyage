// 이 스크립트는 개별 무기의 저장 데이터를 정의하는 클래스입니다.
// 주로 무기 강화에 필요한 카드 수량과 현재 강화 레벨을 저장합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // System.Serializable 속성을 사용하여 이 클래스의 객체를 직렬화하여 저장하고 로드할 수 있도록 합니다.
    [System.Serializable]
    public class WeaponSave : ISaveObject // ISaveObject 인터페이스를 구현하여 저장 시스템과 연동됩니다.
    {
        [Tooltip("무기 강화를 위해 현재 보유하고 있는 카드 수량입니다.")]
        public int CardsAmount = 0;
        [Tooltip("무기의 현재 강화 레벨입니다.")]
        public int UpgradeLevel = 0;

        /// <summary>
        /// 저장 객체의 데이터를 플러시하는 함수입니다.
        /// 현재 구현에서는 별다른 동작을 수행하지 않습니다.
        /// </summary>
        public void Flush()
        {
            // 이 함수는 저장 시스템에서 데이터를 저장 매체에 실제로 쓰기 전에 호출될 수 있지만,
            // 현재 이 클래스 내에서는 플러시할 추가적인 임시 데이터가 없습니다.
        }
    }
}