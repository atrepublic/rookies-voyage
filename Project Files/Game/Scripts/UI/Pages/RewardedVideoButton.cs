/*
📄 RewardedVideoButton.cs 요약
보상형 광고(Rewarded Video) 또는 인게임 재화를 사용하여 보상을 받을 수 있는 버튼 UI를 제어하는 컴포넌트야.

🧩 주요 기능
광고 시청 가능 여부에 따라 버튼 UI 전환
→ 광고 비활성화 상태면 인게임 재화로 대체 구매 옵션 제공.

AdsManager와 연동해서 보상형 광고 재생을 시도하거나,
CurrencyPrice를 통해 재화로 대체 결제도 가능.

버튼 클릭 시 콜백 함수로 결과 반환(true 또는 false)

Redraw() 메서드를 통해 현재 상태에 맞는 UI 구성 자동 업데이트

⚙️ 사용 용도
캐릭터 부활, 아이템 획득, 추가 보상 받기 등에서
광고 또는 재화로 행동을 선택할 수 있는 버튼에 사용.

광고가 꺼져있을 경우에도 유저 경험을 해치지 않고 재화로 대체 가능하게 해줌.
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class RewardedVideoButton : MonoBehaviour
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Sprite activeBackgroundSprite;
        [SerializeField] Sprite blockedBackgroundSprite;

        [Space]
        [SerializeField] GameObject adsContentObject;

        [Space]
        [SerializeField] GameObject currencyContentObject;
        [SerializeField] Image currencyIconImage;
        [SerializeField] TextMeshProUGUI currencyText;

        private Button button;
        public Button Button => button;

        private CurrencyPrice currencyPrice;
        private SimpleBoolCallback completeCallback;

        private bool isInitialised;
        private Currency currency;

        public void Init(SimpleBoolCallback completeCallback, CurrencyPrice currencyPrice)
        {
            this.completeCallback = completeCallback;
            this.currencyPrice = currencyPrice;

            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);

            currency = currencyPrice.Currency;
            currency.OnCurrencyChanged += OnCurrencyChanged;

            isInitialised = true;

            Redraw();
        }

        private void OnCurrencyChanged(Currency currency, int difference)
        {
            if (!isInitialised) return;
            if (AdsManager.Settings.RewardedVideoType != AdProvider.Disable) return;

            Redraw();
        }

        public void Redraw()
        {
            // Activate currency purchase option if RV is disabled
            if(AdsManager.Settings != null && AdsManager.Settings.RewardedVideoType == AdProvider.Disable)
            {
                adsContentObject.SetActive(false);

                currencyContentObject.SetActive(true);

                Currency currency = currencyPrice.Currency;
                currencyIconImage.sprite = currency.Icon;
                currencyText.text = currencyPrice.FormattedPrice;

                if(currencyPrice.EnoughMoneyOnBalance())
                {
                    backgroundImage.sprite = activeBackgroundSprite;
                }
                else
                {
                    backgroundImage.sprite = blockedBackgroundSprite;
                }
            }
            else
            {
                currencyContentObject.SetActive(false);

                adsContentObject.SetActive(true);

                backgroundImage.sprite = activeBackgroundSprite;
            }
        }

        private void OnButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (AdsManager.Settings.RewardedVideoType == AdProvider.Disable)
            {
                if (currencyPrice.EnoughMoneyOnBalance())
                {
                    currencyPrice.SubstractFromBalance();

                    completeCallback?.Invoke(true);
                }
                else
                {
                    completeCallback?.Invoke(false);
                }
            }
            else
            {
                AdsManager.ShowRewardBasedVideo((success) =>
                {
                    completeCallback?.Invoke(success);
                });
            }
        }

        public void Clear()
        {
            isInitialised = false;

            completeCallback = null;
            currencyPrice = null;

            if (currency != null)
            {
                currency.OnCurrencyChanged -= OnCurrencyChanged;

                currency = null;
            }

            button.onClick.RemoveAllListeners();

            gameObject.SetActive(false);
        }
    }
}
