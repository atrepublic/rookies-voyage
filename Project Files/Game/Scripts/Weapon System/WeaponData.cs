// 이 스크립트는 개별 무기의 데이터를 정의하는 ScriptableObject입니다.
// 무기의 이름, 희귀도, 아이콘, 드랍 프리팹, 강화 정보 등을 포함합니다.
// 무기 시스템에서 각 무기의 기본 정보와 강화 상태를 관리하는 데 사용됩니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Weapon Data", menuName = "Data/Weapon System/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [UniqueID]
        [Tooltip("무기의 고유 식별자입니다.")]
        [SerializeField] string id;
        public string ID => id;

        [Tooltip("무기의 표시 이름입니다.")]
        [SerializeField] string weaponName;
        public string WeaponName => weaponName;

        [Tooltip("무기의 희귀도입니다.")]
        [SerializeField] Rarity rarity;
        public Rarity Rarity => rarity;

        [Tooltip("무기 아이콘 이미지입니다.")]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [Tooltip("게임 월드에 드랍될 무기 프리팹입니다.")]
        [SerializeField] GameObject dropPrefab;
        public GameObject DropPrefab => dropPrefab;

        [Tooltip("무기의 강화 단계별 데이터 배열입니다.")]
        [SerializeField] WeaponUpgrade[] upgrades;
        public WeaponUpgrade[] Upgrades => upgrades;

        // 무기의 희귀도에 따른 데이터를 가져옵니다.
        public RarityData RarityData => WeaponsController.GetRarityData(rarity);

        // 무기의 현재 저장 상태 데이터입니다.
        private WeaponSave save;
        public WeaponSave Save => save;

        // 무기의 현재 강화 레벨입니다.
        public int UpgradeLevel => save.UpgradeLevel;
        // 무기 강화를 위한 현재 보유 카드 수량입니다.
        public int CardsAmount => save.CardsAmount;

        /// <summary>
        /// 무기 데이터를 초기화하고 저장된 상태를 로드합니다.
        /// </summary>
        public void Init()
        {
            save = SaveController.GetSaveObject<WeaponSave>($"Weapon_{id}");
        }

        /// <summary>
        /// 현재 강화 레벨에 해당하는 강화 데이터를 가져옵니다.
        /// </summary>
        /// <returns>현재 강화 데이터</returns>
        public WeaponUpgrade GetCurrentUpgrade()
        {
            return upgrades[save.UpgradeLevel];
        }

        /// <summary>
        /// 다음 강화 레벨에 해당하는 강화 데이터를 가져옵니다.
        /// </summary>
        /// <returns>다음 강화 데이터 (다음 강화 레벨이 없으면 null 반환)</returns>
        public WeaponUpgrade GetNextUpgrade()
        {
            if (upgrades.IsInRange(save.UpgradeLevel + 1))
            {
                return upgrades[save.UpgradeLevel + 1];
            }

            return null;
        }

        /// <summary>
        /// 지정된 인덱스에 해당하는 강화 데이터를 가져옵니다.
        /// 인덱스가 유효 범위를 벗어나면 가장 가까운 유효한 인덱스의 데이터를 반환합니다.
        /// </summary>
        /// <param name="index">가져올 강화 데이터의 인덱스</param>
        /// <returns>지정된 인덱스의 강화 데이터</returns>
        public WeaponUpgrade GetUpgrade(int index)
        {
            return upgrades[Mathf.Clamp(index, 0, upgrades.Length - 1)];
        }

        /// <summary>
        /// 현재 강화 레벨의 인덱스를 가져옵니다.
        /// </summary>
        /// <returns>현재 강화 레벨 인덱스</returns>
        public int GetCurrentUpgradeIndex()
        {
            return save.UpgradeLevel;
        }

        /// <summary>
        /// 무기가 최대 강화 레벨인지 확인합니다.
        /// </summary>
        /// <returns>최대 강화 레벨이면 true, 아니면 false</returns>
        public bool IsMaxUpgrade()
        {
            return !upgrades.IsInRange(save.UpgradeLevel + 1);
        }

        /// <summary>
        /// 무기를 다음 레벨로 강화합니다.
        /// </summary>
        public void Upgrade()
        {
            if (upgrades.IsInRange(save.UpgradeLevel + 1))
            {
                save.UpgradeLevel += 1;

                WeaponsController.OnWeaponUpgraded(this);
            }
        }
    }
}