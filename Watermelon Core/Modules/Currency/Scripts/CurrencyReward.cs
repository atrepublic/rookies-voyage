// CurrencyReward.cs
// 이 스크립트는 보상으로 주어지는 통화(화폐)를 관리합니다.
// 통화 보상의 종류, 수량, 표시 방식 등을 설정하고, 실제로 보상을 적용하는 기능을 포함합니다.
// 또한 통화 획득 시 통화 구름 효과를 발생시키는 기능도 지원합니다.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class CurrencyReward : Reward
    {
        [Tooltip("이 보상에 포함된 다양한 통화 정보 목록입니다.")]
        [SerializeField] CurrencyData[] currencies;

        [Tooltip("통화 보상을 획득할 때 통화 구름 효과를 발생시킬지 여부를 설정합니다.")]
        [SerializeField] bool spawnCurrencyCloud;

        [ShowIf("spawnCurrencyCloud")]
        [Tooltip("통화 구름 효과에 사용될 통화의 종류를 설정합니다.")]
        [SerializeField] CurrencyType currencyCloudType;

        [ShowIf("spawnCurrencyCloud")]
        [Tooltip("통화 구름 효과 시 생성될 통화 요소의 개수를 설정합니다.")]
        [SerializeField] int cloudElementsAmount = 10;

        [ShowIf("spawnCurrencyCloud")]
        [Tooltip("통화 구름 효과가 시작될 위치를 설정합니다.")]
        [SerializeField] RectTransform currencyCloudSpawnPoint;

        [ShowIf("spawnCurrencyCloud")]
        [Tooltip("통화 구름 효과가 이동하여 최종적으로 도달할 위치를 설정합니다.")]
        [SerializeField] RectTransform currencyCloudTargetPoint;

        /// <summary>
        /// 보상을 초기화하는 함수입니다.
        /// 통화 데이터에 따라 UI 요소(이미지, 텍스트)를 설정합니다.
        /// </summary>
        public override void Init()
        {
            foreach (CurrencyData currencyData in currencies)
            {
                // CurrencyType에 해당하는 통화 정보를 가져옵니다.
                Currency currency = CurrencyController.GetCurrency(currencyData.CurrencyType);

                // 통화 이미지가 설정되어 있으면 해당 통화의 아이콘으로 설정합니다.
                if (currencyData.CurrencyImage != null)
                    currencyData.CurrencyImage.sprite = currency.Icon;

                // 통화 수량 텍스트가 설정되어 있으면 수량을 포맷팅하여 표시합니다.
                if (currencyData.AmountText != null)
                {
                    // 설정에 따라 숫자를 포맷하거나 그대로 문자열로 변환합니다.
                    string numberText = currencyData.FormatTheNumber ? CurrencyHelper.Format(currencyData.Amount) : currencyData.Amount.ToString();
                    // 설정된 텍스트 포맷에 맞춰 최종 텍스트를 설정합니다. (예: "x{0}")
                    currencyData.AmountText.text = string.Format(string.IsNullOrEmpty(currencyData.TextFormating) ? "{0}" : currencyData.TextFormating, numberText);
                }
            }
        }

        /// <summary>
        /// 이 보상을 플레이어에게 적용하는 함수입니다.
        /// 통화를 지급하고 설정에 따라 통화 구름 효과를 발생시킵니다.
        /// </summary>
        public override void ApplyReward()
        {
            // 통화를 실제로 지급하는 로컬 함수입니다.
            void ApplyCurrency()
            {
                foreach (CurrencyData currencyData in currencies)
                {
                    // 각 통화 데이터에 설정된 통화 타입과 수량만큼 통화를 추가합니다.
                    CurrencyController.Add(currencyData.CurrencyType, currencyData.Amount);
                }
            }

            // 통화 구름 효과를 발생시키도록 설정되어 있으면
            if(spawnCurrencyCloud)
            {
                // 통화 구름 효과를 생성합니다.
                // FloatingCloud.SpawnCurrency(통화 타입 이름, 시작 위치, 목표 위치, 요소 개수, 추가 데이터(사용 안 함), 통화 적용 콜백)
                FloatingCloud.SpawnCurrency(currencyCloudType.ToString(), currencyCloudSpawnPoint, currencyCloudTargetPoint, cloudElementsAmount, "", ApplyCurrency);
            }
            else // 통화 구름 효과를 사용하지 않으면
            {
                // 통화를 즉시 적용합니다.
                ApplyCurrency();
            }
        }

        [System.Serializable]
        public class CurrencyData
        {
            [Tooltip("통화의 종류를 설정합니다.")]
            [SerializeField] CurrencyType currencyType;
            // 이 통화 데이터가 나타내는 통화의 종류입니다.
            public CurrencyType CurrencyType => currencyType;

            [Tooltip("이 보상으로 획득할 통화의 수량을 설정합니다.")]
            [SerializeField] int amount;
            // 이 통화 보상의 수량입니다.
            public int Amount => amount;

            [Space]
            [Tooltip("통화 아이콘을 표시할 UI Image 컴포넌트입니다.")]
            [SerializeField] Image currencyImage;
            // 통화 아이콘을 표시하는 Image UI 요소입니다.
            public Image CurrencyImage => currencyImage;

            [Tooltip("통화 수량을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
            [SerializeField] TextMeshProUGUI amountText;
            // 통화 수량을 표시하는 TextMeshProUGUI UI 요소입니다.
            public TextMeshProUGUI AmountText => amountText;

            [Tooltip("통화 수량을 표시할 텍스트 포맷입니다. {0}에 수량이 들어갑니다. (예: \"x{0}\")")]
            [SerializeField] string textFormating = "x{0}";
            // 통화 수량을 표시할 때 사용할 문자열 포맷입니다.
            public string TextFormating => textFormating;

            [Tooltip("통화 수량을 짧은 형식(예: 1.2K, 3.4M)으로 포맷할지 여부를 설정합니다.")]
            [SerializeField] bool formatTheNumber;
            // 통화 수량을 포맷팅할지 여부를 나타냅니다.
            public bool FormatTheNumber => formatTheNumber;
        }
    }
}