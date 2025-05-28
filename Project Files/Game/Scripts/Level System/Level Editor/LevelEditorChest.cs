/// <summary>
/// LevelEditorChest.cs
///
/// 레벨 에디터에서 상자(Chest)를 배치하고, 보상 설정(화폐 종류/양/드랍 아이템 수량)을 관리하는 스크립트입니다.
/// 게임 내 상자 배치 및 보상 설정을 위한 데이터 역할을 합니다.
/// </summary>

#pragma warning disable 649
using UnityEngine;

namespace Watermelon.LevelSystem
{
    public class LevelEditorChest : MonoBehaviour
    {
        [Tooltip("상자의 타입을 설정합니다.")]
        public LevelChestType type;

        [Tooltip("상자에서 드랍될 보상의 화폐 종류를 설정합니다.")]
        public CurrencyType rewardCurrency;

        [Tooltip("상자에서 획득할 보상 화폐의 총량을 설정합니다.")]
        public int rewardValue = 5;

        [Tooltip("보상 화폐를 몇 개의 아이템으로 나누어 드랍할지 설정합니다.")]
        public int droppedCurrencyItemsAmount = 5;
    }
}
