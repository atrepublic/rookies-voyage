// 이 스크립트는 모든 무기 데이터와 희귀도 설정을 포함하는 ScriptableObject 데이터베이스입니다.
// 무기 ID 또는 인덱스를 사용하여 특정 무기 데이터를 조회하거나, 희귀도별 설정을 가져오는 데 사용됩니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Weapon Database", menuName = "Data/Weapon System/Weapon Database")]
    public class WeaponDatabase : ScriptableObject
    {
        [Tooltip("게임 내 모든 무기 데이터 배열입니다.")]
        [SerializeField] WeaponData[] weapons;
        public WeaponData[] Weapons => weapons;

        [Tooltip("무기 희귀도별 설정 데이터 배열입니다.")]
        [SerializeField] RarityData[] raritySettings;
        public RarityData[] RaritySettings => raritySettings;

        /// <summary>
        /// 무기 ID를 사용하여 특정 무기 데이터를 가져옵니다.
        /// </summary>
        /// <param name="weaponID">찾을 무기의 고유 ID</param>
        /// <returns>해당 ID의 무기 데이터 (없으면 오류 로깅 후 첫 번째 무기 반환)</returns>
        public WeaponData GetWeapon(string weaponID)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].ID == weaponID)
                    return weapons[i];
            }

            // 지정된 ID의 무기를 찾을 수 없습니다. 오류를 로깅합니다.
            Debug.LogError($"Weapon with id ({weaponID}) can't be found");

            // 무기를 찾지 못했으므로 기본값으로 첫 번째 무기를 반환합니다.
            return weapons[0];
        }

        /// <summary>
        /// 인덱스를 사용하여 특정 무기 데이터를 가져옵니다.
        /// 인덱스가 배열 범위를 벗어나면 순환하여 유효한 인덱스의 무기를 반환합니다.
        /// </summary>
        /// <param name="index">가져올 무기의 인덱스</param>
        /// <returns>해당 인덱스의 무기 데이터</returns>
        public WeaponData GetWeaponByIndex(int index)
        {
            return weapons[index % weapons.Length];
        }

        /// <summary>
        /// 희귀도 타입을 사용하여 해당 희귀도의 설정 데이터를 가져옵니다.
        /// </summary>
        /// <param name="rarity">찾을 희귀도 타입</param>
        /// <returns>해당 희귀도의 설정 데이터 (없으면 오류 로깅 후 첫 번째 희귀도 설정 반환)</returns>
        public RarityData GetRarityData(Rarity rarity)
        {
            for (int i = 0; i < raritySettings.Length; i++)
            {
                if (raritySettings[i].Rarity.Equals(rarity))
                    return raritySettings[i];
            }

            // 지정된 희귀도 타입의 데이터를 찾을 수 없습니다. 오류를 로깅합니다.
            Debug.LogError("Rarity data of type: " + rarity + " is not found");

            // 데이터를 찾지 못했으므로 기본값으로 첫 번째 희귀도 설정을 반환합니다.
            return raritySettings[0];
        }
    }
}