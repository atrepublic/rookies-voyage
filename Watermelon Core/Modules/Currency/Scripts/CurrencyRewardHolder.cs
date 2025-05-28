// CurrencyRewardHolder.cs
// 이 스크립트는 통화 보상을 담고 있는 홀더 역할을 합니다.
// 특정 통화 가격으로 보상을 구매하거나 획득할 수 있도록 관리하며,
// 보상 획득 후 비활성화되는 기능을 제공합니다.

using UnityEngine;

namespace Watermelon
{
    public sealed class CurrencyRewardsHolder : RewardsHolder
    {
        [Group("Settings"), UniqueID]
        [Tooltip("이 보상 홀더를 식별하는 고유 ID입니다. 저장 데이터에 사용됩니다.")]
        [SerializeField] string rewardID;

        [Group("Settings")]
        [Tooltip("보상을 구매하기 위한 UI 통화 버튼 컴포넌트입니다.")]
        [SerializeField] UICurrencyButton currencyButton;

        [Group("Settings")]
        [Tooltip("이 보상을 획득하기 위해 필요한 통화 가격 정보입니다.")]
        [SerializeField] CurrencyPrice price;

        [Group("Settings"), Space]
        [Tooltip("이 보상을 구매한 후 이 홀더 GameObject를 비활성화할지 여부를 설정합니다.")]
        [SerializeField] bool disableAfterPurchase;

        // 보상 구매 상태를 저장하기 위한 간단한 bool 타입 저장 객체입니다.
        private SimpleBoolSave save;

        // 이 스크립트 인스턴스가 로드될 때 호출됩니다. 초기화 및 저장 데이터 로드를 수행합니다.
        private void Awake()
        {
            InitializeComponents(); // 컴포넌트를 초기화합니다. (RewardsHolder에서 상속받은 기능)

            // rewardID를 사용하여 해당 보상의 구매 상태를 저장하는 객체를 불러오거나 생성합니다.
            save = SaveController.GetSaveObject<SimpleBoolSave>($"CurrencyProduct_{rewardID}");

            // 구매 후 비활성화 설정이 되어 있고 이미 구매한 상태이면
            if(disableAfterPurchase && save.Value)
            {
                // 홀더 GameObject를 비활성화합니다.
                gameObject.SetActive(false);

                // 더 이상 진행할 필요 없으므로 함수를 종료합니다.
                return;
            }

            // 각 보상의 비활성화 조건을 확인합니다.
            for (int i = 0; i < rewards.Length; i++)
            {
                // 보상이 비활성화 상태여야 한다면
                if (rewards[i].CheckDisableState())
                {
                    // 홀더 GameObject를 비활성화합니다.
                    gameObject.SetActive(false);

                    // 더 이상 진행할 필요 없으므로 함수를 종료합니다.
                    return;
                }
            }

            // 통화 버튼을 가격과 통화 타입으로 초기화합니다.
            currencyButton.Init(price.Price, price.CurrencyType);
            // 통화 버튼 구매 이벤트에 OnPurchased 함수를 연결합니다.
            currencyButton.Purchased += OnPurchased;
        }

        // 통화 버튼 구매 완료 시 호출되는 함수입니다.
        private void OnPurchased()
        {
            // 이 홀더에 연결된 보상들을 적용합니다.
            ApplyRewards();

            // 구매 상태를 true로 설정하여 저장합니다.
            save.Value = true;

            // 구매 후 비활성화 설정이 되어 있으면
            if(disableAfterPurchase)
            {
                // 홀더 GameObject를 비활성화합니다.
                gameObject.SetActive(false);
            }

            // 저장해야 함을 SaveController에 알립니다.
            SaveController.MarkAsSaveIsRequired();
        }
    }
}