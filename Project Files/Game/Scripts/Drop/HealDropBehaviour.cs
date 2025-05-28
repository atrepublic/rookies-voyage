// 스크립트 설명: 회복 아이템 드롭의 동작을 처리하는 클래스입니다.
// 캐릭터에게 체력 회복 보상을 적용하고 캐릭터의 체력 상태에 따라 획득 가능 여부를 판단합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// HealDropBehaviour는 캐릭터에게 체력 회복 보상을 적용하고
    /// 캐릭터의 체력 상태에 기반하여 드롭 아이템이 획득 가능한지 여부를 결정하는 역할을 합니다.
    /// </summary>
    public class HealDropBehaviour : BaseDropBehavior // BaseDropBehavior 상속 (이전 파일에서 정의된 것으로 가정)
    {
        [SerializeField]
        [Tooltip("회복 아이템 사용 시 회복되는 체력량")] // 주요 변수 한글 툴팁
        int amount; // 회복량

        /// <summary>
        /// 회복 아이템의 회복량을 설정합니다.
        /// </summary>
        /// <param name="amount">설정할 회복량.</param>
        public void SetData(int amount)
        {
            this.amount = amount;
        }

        /// <summary>
        /// 회복 아이템 획득에 대한 보상(체력 회복)을 적용합니다.
        /// </summary>
        /// <param name="autoReward">자동 보상 적용 여부 (이 스크립트에서는 사용되지 않음).</param>
        public override void ApplyReward(bool autoReward = false)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour(); // 현재 플레이어 캐릭터의 Behaviour 가져오기
            if (characterBehaviour != null)
            {
                characterBehaviour.Heal(amount); // 캐릭터의 체력 회복 (CharacterBehaviour에 정의된 것으로 가정)
            }
        }

        /// <summary>
        /// 지정된 캐릭터가 이 회복 아이템을 주울 수 있는지 여부를 판단합니다.
        /// 체력이 가득 차 있지 않은 경우에만 획득 가능합니다.
        /// </summary>
        /// <param name="characterBehaviour">캐릭터 동작 컴포넌트.</param>
        /// <returns>아이템을 주울 수 있으면 true, 그렇지 않으면 false.</returns>
        public override bool IsPickable(CharacterBehaviour characterBehaviour)
        {
            return !characterBehaviour.FullHealth; // 캐릭터의 체력이 가득 찼는지 확인 (CharacterBehaviour에 FullHealth 프로퍼티가 정의된 것으로 가정)
        }
    }
}