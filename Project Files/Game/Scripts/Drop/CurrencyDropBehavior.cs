// 스크립트 설명: 게임 내 화폐 드롭 아이템의 동작을 처리하는 클래스입니다.
// 획득 시 보상 적용 및 획득 사운드 재생 기능을 포함합니다.
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 게임 내 화폐 드롭의 동작을 처리합니다. 보상 적용 및 획득 사운드 재생 기능이 포함됩니다.
    /// </summary>
    public class CurrencyDropBehavior : BaseDropBehavior // BaseDropBehavior 상속 (이전 파일에서 정의된 것으로 가정)
    {
        [SerializeField]
        [Tooltip("이 드롭 아이템의 화폐 타입")] // 주요 변수 한글 툴팁
        CurrencyType currencyType; // 화폐 타입

        [SerializeField]
        [Tooltip("이 드롭 아이템의 화폐 수량")] // 주요 변수 한글 툴팁
        int amount; // 화폐 수량

        /// <summary>
        /// 화폐 드롭 아이템의 화폐 타입과 수량을 설정합니다.
        /// </summary>
        /// <param name="currencyType">설정할 화폐 타입.</param>
        /// <param name="amount">설정할 화폐 수량.</param>
        public void SetCurrencyData(CurrencyType currencyType, int amount)
        {
            this.currencyType = currencyType;
            this.amount = amount;
        }

        /// <summary>
        /// 화폐 드롭 아이템 획득에 대한 보상을 적용합니다.
        /// </summary>
        /// <param name="autoReward">자동 보상 적용 여부.</param>
        public override void ApplyReward(bool autoReward = false)
        {
            // 화폐 타입에 따라 보상 적용 로직 분기
            if (currencyType == CurrencyType.Coins)
            {
                if (IsRewarded)
                {
                    // 보상으로 코인을 획득했을 때 호출 (LevelController에 정의된 것으로 가정)
                    LevelController.OnRewardedCoinPicked(amount);
                }
                else
                {
                    // 일반 코인 획득 시 호출 (LevelController에 정의된 것으로 가정)
                    LevelController.OnCoinPicked(amount);
                }
            }
            else
            {
                // 다른 화폐 타입일 경우 CurrencyController를 통해 추가 (CurrencyController에 정의된 것으로 가정)
                CurrencyController.Add(currencyType, amount);
            }

            // 자동 보상이 아닐 경우 획득 사운드 재생
            if (!autoReward)
            {
                Currency currency = CurrencyController.GetCurrency(currencyType); // 화폐 데이터 가져오기 (CurrencyController에 정의된 것으로 가정)
                if (currency != null)
                {
                    AudioClip pickUpSound = currency.Data.DropPickupSound; // 획득 사운드 클립 가져오기
                    if (pickUpSound != null)
                    {
                        AudioController.PlaySound(pickUpSound); // 사운드 재생 (AudioController에 정의된 것으로 가정)
                    }
                }
            }
        }
    }
}