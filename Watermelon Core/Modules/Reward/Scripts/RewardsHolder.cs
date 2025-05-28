// RewardsHolder.cs
// 이 추상 클래스는 여러 보상(Reward) 컴포넌트를 관리하며, 보상 초기화 및 적용 로직과 보상 수령 이벤트를 제공합니다.

using UnityEngine;
using UnityEngine.Events;

namespace Watermelon
{
    public abstract class RewardsHolder : MonoBehaviour
    {
        [Header("이벤트")]
        [SerializeField]
        [Tooltip("보상이 적용된 후 호출되는 이벤트")]
        private UnityEvent rewardReceived;
        
        /// <summary>
        /// 보상 적용 후 외부에서 구독할 수 있는 이벤트 프로퍼티입니다.
        /// </summary>
        public UnityEvent RewardReceived => rewardReceived;

        // 보상 컴포넌트 배열 캐시
        protected Reward[] rewards;

        /// <summary>
        /// Awake 또는 Start 시 컴포넌트를 초기화하고 보상 리스트를 구성합니다.
        /// </summary>
        protected void InitializeComponents()
        {
            // 게임 오브젝트에 연결된 모든 Reward 컴포넌트를 가져옵니다.
            rewards = GetComponents<Reward>();

            // 각 보상을 초기화합니다.
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].Init();
            }
        }

        /// <summary>
        /// 보상을 순차적으로 적용하고, 보상 수령 이벤트를 호출합니다.
        /// </summary>
        protected void ApplyRewards()
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].ApplyReward();
            }

            // 보상 수령 이벤트 호출
            rewardReceived?.Invoke();
        }
    }
}
