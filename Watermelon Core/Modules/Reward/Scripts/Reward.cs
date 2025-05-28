// Reward.cs
// 이 추상 클래스는 게임 내 보상(Reward) 시스템의 기본 구조를 정의합니다.
// Init: 보상 초기화, ApplyReward: 보상을 적용, CheckDisableState: 보상 비활성화 조건 확인

using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 보상 시스템의 기본 추상 클래스입니다.
    /// 다양한 보상(코인, 아이템, 경험치 등)이 이 클래스를 상속하여 구현할 수 있습니다.
    /// </summary>
    public abstract class Reward : MonoBehaviour
    {
        /// <summary>
        /// 보상 초기화 함수입니다. 보상 객체가 활성화될 때 필요한 초기 설정을 구현할 수 있습니다.
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 실제 보상을 적용하는 추상 메서드입니다.
        /// 상속 클래스에서 보상의 구체적인 동작(예: 코인 추가, 아이템 지급)을 구현해야 합니다.
        /// </summary>
        public abstract void ApplyReward();

        /// <summary>
        /// 이 보상을 비활성화할지 여부를 판단합니다.
        /// 예: 이미 광고 제거 상품을 구매한 경우, 다른 광고 제거 오퍼를 비활성화하기 위해 true를 반환할 수 있습니다.
        /// </summary>
        /// <returns>보상을 비활성화해야 한다면 true를 반환합니다.</returns>
        public virtual bool CheckDisableState() { return false; }
    }
}
