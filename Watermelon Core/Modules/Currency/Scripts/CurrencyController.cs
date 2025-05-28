// 스크립트 기능 요약:
// 이 스크립트는 게임 내 모든 화폐(통화)의 중앙 관리자 역할을 하는 정적 클래스입니다.
// CurrencyDatabase로부터 화폐 목록을 로드하고 초기화하며,
// 특정 화폐의 현재 보유량을 조회하거나, 추가, 감소, 설정하는 기능을 제공합니다.
// 화폐 변경 시 이벤트를 관리하고, 전역 화폐 변경 콜백을 구독/해지하는 기능도 포함합니다.
// 저장 시스템(SaveController)과 연동하여 화폐 보유량을 저장하고 로드합니다.

using System.Collections.Generic;
using UnityEngine; // Debug.LogError 등 사용을 위해 필요
#if UNITY_EDITOR // 에디터 전용 코드 포함을 위해 필요
using UnityEditor; // EditorApplication, AssetDatabase 등 에디터 기능 사용을 위해 필요 (원본 코드에 없지만 에디터 코드에 사용될 수 있음)
#endif

namespace Watermelon
{
    // StaticUnload 속성을 통해 애플리케이션 종료 시 정적 데이터를 정리할 수 있도록 표시합니다.
    [StaticUnload]
    // CurrencyController 클래스는 게임 내 모든 화폐를 관리하는 정적 유틸리티 클래스입니다.
    public static class CurrencyController
    {
        // currencies: CurrencyDatabase에서 로드된 모든 화폐 객체 배열입니다.
        [Tooltip("모든 화폐 객체 배열")]
        private static Currency[] currencies;
        // Currencies 속성: currencies 배열을 읽기 전용으로 제공합니다.
        public static Currency[] Currencies => currencies;

        // currenciesLink: CurrencyType 열거형 값을 키로 사용하여 해당 화폐의 currencies 배열 내 인덱스를 저장하는 딕셔너리입니다.
        // CurrencyType으로 화폐 객체를 빠르게 찾을 때 사용됩니다.
        [Tooltip("CurrencyType으로 화폐 배열 인덱스를 찾는 딕셔너리")]
        private static Dictionary<CurrencyType, int> currenciesLink;

        // isInitialized: CurrencyController가 초기화되었는지 여부를 나타내는 플래그입니다.
        [Tooltip("컨트롤러 초기화 완료 여부")]
        private static bool isInitialized;

        /// <summary>
        /// CurrencyController를 초기화하는 함수입니다.
        /// CurrencyDatabase에서 화폐 목록을 로드하고, 각 화폐 객체를 초기화하며,
        /// CurrencyType과 화폐 객체의 인덱스를 연결하는 딕셔너리를 구성합니다.
        /// 저장 시스템과 연동하여 화폐 보유량을 로드합니다.
        /// </summary>
        /// <param name="currenciesDatabase">화폐 목록이 포함된 CurrencyDatabase 객체</param>
        public static void Init(CurrencyDatabase currenciesDatabase)
        {
            // 이미 초기화된 경우 함수를 종료합니다.
            if (isInitialized) return;

            // CurrencyDatabase에서 화폐 배열을 가져와 저장합니다.
            currencies = currenciesDatabase.Currencies;

            // 각 화폐 객체를 초기화합니다. (Currency.Init() 호출)
            foreach (Currency currency in currencies)
            {
                currency.Init();
            }

            // CurrencyType과 화폐 배열 인덱스를 연결하는 딕셔너리를 구성합니다.
            currenciesLink = new Dictionary<CurrencyType, int>();
            for (int i = 0; i < currencies.Length; i++)
            {
                // 딕셔너리에 이미 동일한 CurrencyType이 존재하는지 확인합니다. (중복 방지)
                if (!currenciesLink.ContainsKey(currencies[i].CurrencyType))
                {
                    // 중복되지 않으면 딕셔너리에 추가합니다.
                    currenciesLink.Add(currencies[i].CurrencyType, i);
                }
                else
                {
                    // 중복된 CurrencyType이 발견되면 오류 메시지를 출력합니다.
                    Debug.LogError(string.Format("[Currency Syste]: {0} 타입의 화폐가 데이터베이스에 두 번 추가되었습니다!", currencies[i].CurrencyType));
                }

                // 저장 시스템에서 해당 화폐의 저장 데이터를 로드하거나 기본값을 설정합니다.
                // "currency:화폐타입정수값" 형식의 키를 사용합니다.
                Currency.Save save = SaveController.GetSaveObject<Currency.Save>("currency" + ":" + (int)currencies[i].CurrencyType);
                // 로드된 저장 데이터의 보유량이 -1이면 (초기값 또는 저장되지 않음) 기본 보유량으로 설정합니다.
                if(save.Amount == -1)
                    save.Amount = currencies[i].DefaultAmount;

                // 로드되거나 설정된 저장 객체를 해당 화폐 객체에 설정합니다.
                currencies[i].SetSave(save);
            }

            // 초기화 완료 플래그를 true로 설정합니다.
            isInitialized = true;
        }

        /// <summary>
        /// 지정된 타입의 화폐를 지정된 금액 이상 보유하고 있는지 확인하는 함수입니다.
        /// </summary>
        /// <param name="currencyType">확인할 화폐의 타입</param>
        /// <param name="amount">확인할 금액</param>
        /// <returns>지정된 금액 이상 보유하고 있으면 true, 그렇지 않으면 false</returns>
        public static bool HasAmount(CurrencyType currencyType, int amount)
        {
            // CurrencyType을 사용하여 해당 화폐 객체를 찾고 보유량과 비교합니다.
            return currencies[currenciesLink[currencyType]].Amount >= amount;
        }

        /// <summary>
        /// 지정된 타입의 화폐 현재 보유량을 가져오는 함수입니다.
        /// </summary>
        /// <param name="currencyType">보유량을 가져올 화폐의 타입</param>
        /// <returns>해당 화폐의 현재 보유량</returns>
        public static int Get(CurrencyType currencyType)
        {
            // CurrencyType을 사용하여 해당 화폐 객체를 찾고 보유량을 반환합니다.
            return currencies[currenciesLink[currencyType]].Amount;
        }

        /// <summary>
        /// 지정된 타입의 Currency 객체 자체를 가져오는 함수입니다.
        /// 에디터 모드와 플레이 모드에서 다르게 작동합니다.
        /// </summary>
        /// <param name="currencyType">가져올 Currency 객체의 타입</param>
        /// <returns>해당 Currency 객체 또는 찾을 수 없으면 null</returns>
        public static Currency GetCurrency(CurrencyType currencyType)
        {
#if UNITY_EDITOR // Unity 에디터에서만 실행되는 코드 블록
            // 에디터 플레이 모드가 아닌 경우 (Inspector 등에서 접근 시)
            if(!Application.isPlaying)
            {
                // ProjectInitSettings에서 CurrencyInitModule을 통해 CurrencyDatabase를 찾습니다.
                ProjectInitSettings projectInitSettings = RuntimeEditorUtils.GetAsset<ProjectInitSettings>();
                if (projectInitSettings != null)
                {
                    CurrencyInitModule currencyInitModule = projectInitSettings.GetModule<CurrencyInitModule>();
                    if(currencyInitModule != null)
                    {
                        CurrencyDatabase currencyDatabase = currencyInitModule.Database;
                        if (currencyDatabase != null)
                        {
                            // CurrencyDatabase에서 해당 CurrencyType을 가진 화폐를 찾아 반환합니다.
                            return currencyDatabase.Currencies.Find(x => x.CurrencyType.Equals(currencyType));
                        }
                    }
                }

                return null; // 찾을 수 없으면 null 반환
            }
#endif // UNITY_EDITOR 끝

            // 플레이 모드에서는 초기화된 currencies 배열에서 CurrencyType을 사용하여 해당 화폐 객체를 찾아 반환합니다.
            return currencies[currenciesLink[currencyType]];
        }

        /// <summary>
        /// 지정된 타입의 화폐 보유량을 특정 값으로 설정하는 함수입니다.
        /// </summary>
        /// <param name="currencyType">보유량을 설정할 화폐의 타입</param>
        /// <param name="amount">설정할 새로운 보유량</param>
        public static void Set(CurrencyType currencyType, int amount)
        {
            // CurrencyType을 사용하여 해당 화폐 객체를 찾습니다.
            Currency currency = currencies[currenciesLink[currencyType]];

            // 화폐의 보유량을 설정합니다.
            currency.Amount = amount;

            // 변경 사항 저장이 필요함을 저장 시스템에 알립니다.
            SaveController.MarkAsSaveIsRequired();

            // 화폐 변경 이벤트를 발생시킵니다. (변화량은 0으로 전달)
            currency.InvokeChangeEvent(0);
        }

        /// <summary>
        /// 지정된 타입의 화폐 보유량을 증가시키는 함수입니다.
        /// </summary>
        /// <param name="currencyType">보유량을 증가시킬 화폐의 타입</param>
        /// <param name="amount">증가시킬 금액</param>
        public static void Add(CurrencyType currencyType, int amount)
        {
            // CurrencyType을 사용하여 해당 화폐 객체를 찾습니다.
            Currency currency = currencies[currenciesLink[currencyType]];

            // 화폐의 보유량을 증가시킵니다.
            currency.Amount += amount;

            // 변경 사항 저장이 필요함을 저장 시스템에 알립니다.
            SaveController.MarkAsSaveIsRequired();

            // 화폐 변경 이벤트를 발생시킵니다. (변화량은 증가된 금액으로 전달)
            currency.InvokeChangeEvent(amount);
        }

        /// <summary>
        /// 지정된 타입의 화폐 보유량을 감소시키는 함수입니다.
        /// </summary>
        /// <param name="currencyType">보유량을 감소시킬 화폐의 타입</param>
        /// <param name="amount">감소시킬 금액</param>
        public static void Substract(CurrencyType currencyType, int amount)
        {
            // CurrencyType을 사용하여 해당 화폐 객체를 찾습니다.
            Currency currency = currencies[currenciesLink[currencyType]];

            // 화폐의 보유량을 감소시킵니다.
            currency.Amount -= amount;

            // 변경 사항 저장이 필요함을 저장 시스템에 알립니다.
            SaveController.MarkAsSaveIsRequired();

            // 화폐 변경 이벤트를 발생시킵니다. (변화량은 감소된 금액으로 전달)
            currency.InvokeChangeEvent(-amount);
        }

        /// <summary>
        /// 모든 화폐의 보유량 변경 이벤트에 대한 전역 콜백 함수를 구독합니다.
        /// 어떤 화폐든 보유량이 변경될 때마다 지정된 콜백 함수가 호출됩니다.
        /// </summary>
        /// <param name="currencyChange">구독할 콜백 함수</param>
        public static void SubscribeGlobalCallback(CurrencyCallback currencyChange)
        {
            // currencies 배열을 순회하며 각 화폐의 OnCurrencyChanged 이벤트에 콜백 함수를 추가합니다.
            for(int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged += currencyChange;
            }
        }

        /// <summary>
        /// 모든 화폐의 보유량 변경 이벤트에 대한 전역 콜백 함수 구독을 해지합니다.
        /// </summary>
        /// <param name="currencyChange">구독 해지할 콜백 함수</param>
        public static void UnsubscribeGlobalCallback(CurrencyCallback currencyChange)
        {
            // currencies 배열이 비어있지 않은 경우에만 처리합니다.
            if(!currencies.IsNullOrEmpty())
            {
                // currencies 배열을 순회하며 각 화폐의 OnCurrencyChanged 이벤트에서 콜백 함수를 제거합니다.
                for (int i = 0; i < currencies.Length; i++)
                {
                    currencies[i].OnCurrencyChanged -= currencyChange;
                }
            }
        }

        /// <summary>
        /// PoolManager가 언로드될 때 호출되는 정적 언로드 함수입니다.
        /// 정적 변수들을 초기 상태로 되돌려 메모리를 해제하고 초기화 상태를 리셋합니다.
        /// </summary>
        private static void UnloadStatic()
        {
            currencies = null; // 화폐 배열 참조 해제
            currenciesLink = null; // 딕셔너리 참조 해제
            isInitialized = false; // 초기화 상태 리셋
        }
    }

    // CurrencyCallback 델리게이트는 화폐 보유량 변경 이벤트에 사용될 콜백 함수의 시그니처를 정의합니다.
    // 변경된 화폐 객체와 변화량을 매개변수로 받습니다.
    public delegate void CurrencyCallback(Currency currency, int difference);
}