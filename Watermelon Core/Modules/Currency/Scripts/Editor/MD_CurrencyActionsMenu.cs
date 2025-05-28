// MD_CurrencyActionsMenu.cs
// 이 스크립트는 Unity 에디터의 'Actions' 메뉴에 통화 관련 디버그 액션을 추가합니다.
// 플레이 모드에서 많은 양의 코인을 얻거나 모든 코인을 잃는 기능을 제공합니다.

using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    public static class CurrencyActionsMenu
    {
        /// <summary>
        /// 에디터 메뉴에 'Actions/Lots of Money' 항목을 추가하고 실행 시 많은 코인을 지급합니다.
        /// 우선순위 21로 다른 액션 메뉴 항목과의 순서를 조정합니다.
        /// </summary>
        [MenuItem("Actions/Lots of Money", priority = 21)]
        private static void LotsOfMoney()
        {
            // 통화 타입을 Coins로, 수량을 2,000,000으로 설정합니다.
            CurrencyController.Set(CurrencyType.Coins, 2000000);
        }

        /// <summary>
        /// 'Actions/Lots of Money' 메뉴 항목의 유효성을 검사하는 함수입니다.
        /// 플레이 모드일 때만 메뉴 항목을 활성화합니다.
        /// </summary>
        /// <returns>플레이 모드이면 true, 아니면 false</returns>
        [MenuItem("Actions/Lots of Money", true)]
        private static bool LotsOfMoneyValidation()
        {
            // 현재 애플리케이션이 플레이 모드인지 확인합니다.
            return Application.isPlaying;
        }

        /// <summary>
        /// 에디터 메뉴에 'Actions/No Money' 항목을 추가하고 실행 시 모든 코인을 0으로 설정합니다.
        /// 우선순위 22로 'Lots of Money' 항목 아래에 표시되도록 합니다.
        /// </summary>
        [MenuItem("Actions/No Money", priority = 22)]
        private static void NoMoney()
        {
            // 통화 타입을 Coins로, 수량을 0으로 설정합니다.
            CurrencyController.Set(CurrencyType.Coins, 0);
        }

        /// <summary>
        /// 'Actions/No Money' 메뉴 항목의 유효성을 검사하는 함수입니다.
        /// 플레이 모드일 때만 메뉴 항목을 활성화합니다.
        /// </summary>
        /// <returns>플레이 모드이면 true, 아니면 false</returns>
        [MenuItem("Actions/No Money", true)]
        private static bool NoMoneyValidation()
        {
            // 현재 애플리케이션이 플레이 모드인지 확인합니다.
            return Application.isPlaying;
        }
    }
}