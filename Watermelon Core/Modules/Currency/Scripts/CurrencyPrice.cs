// 스크립트 기능 요약:
// 이 스크립트는 게임 내 특정 화폐 타입과 금액의 조합, 즉 '화폐 가격'을 나타내는 데이터 구조입니다.
// 어떤 종류의 화폐로 얼마의 가격인지 정보를 저장하며,
// 현재 보유량으로 이 가격을 지불할 수 있는지 확인하거나, 보유량에서 이 가격만큼 차감하는 기능을 제공합니다.
// 또한, 화폐 아이콘을 포함한 형식화된 가격 문자열을 생성하는 기능도 포함합니다.
// [System.Serializable] 속성을 통해 Unity 에디터에서 직렬화되어 인스펙터 창 등에 표시될 수 있습니다.

using UnityEngine; // SerializeField 속성 사용을 위해 필요

namespace Watermelon
{
    // CurrencyPrice 클래스는 게임 내 화폐 가격을 나타내는 직렬화 가능한 데이터 클래스입니다.
    [System.Serializable]
    public class CurrencyPrice
    {
        // TEXT_FORMAT: 화폐 아이콘과 금액을 포함한 문자열을 형식화하는 데 사용되는 상수입니다.
        // <sprite name={0}> 부분은 TextMeshPro 등에서 {0}에 해당하는 이름의 스프라이트를 표시합니다.
        [Tooltip("화폐 아이콘과 금액을 포함한 문자열 형식")]
        private const string TEXT_FORMAT = "<sprite name={0}>{1}";

        // currencyType: 이 가격에 사용되는 화폐의 종류입니다.
        [SerializeField]
        [Tooltip("이 가격에 사용되는 화폐의 종류")]
        CurrencyType currencyType;
        // CurrencyType 속성: currencyType 변수의 값을 읽기 전용으로 제공합니다.
        public CurrencyType CurrencyType => currencyType;

        // price: 이 가격의 금액입니다.
        [SerializeField]
        [Tooltip("이 가격의 금액")]
        int price;
        // Price 속성: price 변수의 값을 읽기 전용으로 제공합니다.
        public int Price => price;

        // Currency 속성: CurrencyController를 통해 이 가격에 해당하는 Currency 객체를 가져옵니다.
        [Tooltip("이 가격에 해당하는 Currency 객체")]
        public Currency Currency => CurrencyController.GetCurrency(currencyType);
        // FormattedPrice 속성: CurrencyHelper를 사용하여 이 가격의 금액을 형식화된 문자열로 가져옵니다.
        [Tooltip("형식화된 가격 금액 문자열")]
        public string FormattedPrice => CurrencyHelper.Format(price);

        /// <summary>
        /// CurrencyPrice 클래스의 기본 생성자입니다.
        /// 필드들은 기본값으로 초기화됩니다.
        /// </summary>
        public CurrencyPrice()
        {
            // 필드들은 기본값으로 초기화
        }

        /// <summary>
        /// CurrencyPrice 클래스의 생성자입니다.
        /// 화폐 타입과 가격 금액을 지정하여 새로운 CurrencyPrice 객체를 생성합니다.
        /// </summary>
        /// <param name="currencyType">이 가격에 사용할 화폐 타입</param>
        /// <param name="price">이 가격의 금액</param>
        public CurrencyPrice(CurrencyType currencyType, int price)
        {
            this.currencyType = currencyType; // 화폐 타입 설정
            this.price = price; // 가격 금액 설정
        }

        /// <summary>
        /// 현재 보유량으로 이 가격을 지불할 수 있는지 확인하는 함수입니다.
        /// CurrencyController의 HasAmount 함수를 사용하여 확인합니다.
        /// </summary>
        /// <returns>보유량이 충분하면 true, 그렇지 않으면 false</returns>
        public bool EnoughMoneyOnBalance()
        {
            // CurrencyController를 통해 지정된 화폐 타입의 보유량이 가격 이상인지 확인합니다.
            return CurrencyController.HasAmount(currencyType, price);
        }

        /// <summary>
        /// 현재 보유량에서 이 가격만큼 차감하는 함수입니다.
        /// CurrencyController의 Substract 함수를 사용하여 보유량을 감소시킵니다.
        /// </summary>
        public void SubstractFromBalance()
        {
            // CurrencyController를 통해 지정된 화폐 타입의 보유량에서 가격만큼 차감합니다.
            CurrencyController.Substract(currencyType, price);
        }

        /// <summary>
        /// 화폐 아이콘과 금액을 포함하는 형식화된 문자열을 반환하는 함수입니다.
        /// TextMeshPro 등에서 아이콘을 표시하는 데 사용될 수 있습니다.
        /// </summary>
        /// <returns>화폐 아이콘과 금액이 포함된 형식화된 문자열</returns>
        public string GetTextWithIcon()
        {
            // TEXT_FORMAT을 사용하여 화폐 타입 이름과 가격 금액을 삽입하여 문자열을 생성합니다.
            return string.Format(TEXT_FORMAT, currencyType, price);
        }
    }
}