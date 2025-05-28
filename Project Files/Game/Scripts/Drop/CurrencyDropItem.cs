// 스크립트 설명: 화폐 타입 드롭 아이템의 생성 정보를 제공하는 클래스입니다.
// IDropItem 인터페이스를 구현하여 드롭 매니저에서 사용됩니다.
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class CurrencyDropItem : IDropItem // IDropItem 인터페이스 구현
    {
        // 드롭 아이템의 타입을 반환합니다. 이 클래스는 화폐 타입입니다.
        public DropableItemType DropItemType => DropableItemType.Currency;

        [Tooltip("사용 가능한 모든 화폐 데이터 배열")] // 주요 변수 한글 툴팁
        private Currency[] availableCurrencies; // 사용 가능한 화폐 데이터 목록

        /// <summary>
        /// 주어진 드롭 데이터를 기반으로 화폐 드롭 아이템의 게임 오브젝트(모델)를 가져옵니다.
        /// </summary>
        /// <param name="dropData">드롭 아이템 데이터.</param>
        /// <returns>화폐 드롭 아이템 모델 게임 오브젝트 또는 null.</returns>
        public GameObject GetDropObject(DropData dropData)
        {
            CurrencyType currencyType = dropData.CurrencyType; // 드롭 데이터에서 화폐 타입 가져오기
            if(availableCurrencies != null) // 화폐 목록이 초기화되었는지 확인
            {
                 for(int i = 0; i < availableCurrencies.Length; i++)
                 {
                     // 드롭 데이터의 화폐 타입과 일치하는 화폐 데이터 찾기
                     if(availableCurrencies[i].CurrencyType == currencyType)
                     {
                         return availableCurrencies[i].Data.DropModel; // 해당 화폐의 드롭 모델 반환
                     }
                 }
            }


            return null; // 일치하는 화폐 데이터나 모델이 없으면 null 반환
        }

        /// <summary>
        /// 이 드롭 아이템 타입이 사용할 수 있는 화폐 목록을 설정합니다.
        /// </summary>
        /// <param name="currencies">사용 가능한 화폐 데이터 배열.</param>
        public void SetCurrencies(Currency[] currencies)
        {
            availableCurrencies = currencies;
        }

        /// <summary>
        /// 드롭 아이템 타입 초기화 시 호출됩니다. (현재 기능 없음)
        /// </summary>
        public void Init()
        {
            // 초기화 로직 (필요하다면 여기에 추가)
        }

        /// <summary>
        /// 드롭 아이템 타입 언로드 시 호출됩니다. (현재 기능 없음)
        /// </summary>
        public void Unload()
        {
            // 언로드 로직 (필요하다면 여기에 추가)
        }
    }
}