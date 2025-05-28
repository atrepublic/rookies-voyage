// 이 스크립트는 게임 내 모든 무기를 관리하는 중앙 컨트롤러입니다.
// 무기 데이터 로드, 무기 선택, 강화 상태 확인 및 업데이트, 카드 추가 등의 기능을 수행합니다.
// 싱글톤 패턴으로 구현되어 게임 전체에서 접근 가능합니다.
using System.Collections.Generic;
using System.Linq; // Linq 기능을 사용하기 위해 필요합니다.
using UnityEngine;
using Watermelon.LevelSystem; // LevelSystem 네임스페이스의 요소를 사용하기 위해 필요합니다.

namespace Watermelon.SquadShooter
{
    public class WeaponsController : MonoBehaviour
    {
        // 무기 데이터베이스 ScriptableObject에 대한 참조입니다.
        private static WeaponDatabase database;

        // WeaponsController의 싱글톤 인스턴스입니다.
        private static WeaponsController instance;

        // 무기 관련 전역 저장 데이터입니다.
        private static GlobalWeaponsSave save;
        // 주요 강화 단계 (Key Upgrade)를 저장하는 리스트입니다.
        private static List<WeaponUpgrade> keyUpgradeStages = new List<WeaponUpgrade>();

        // 게임 내 모든 무기 데이터 배열입니다.
        private static WeaponData[] weapons;
        // 모든 무기 데이터에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public static WeaponData[] Weapons => weapons;

        // 무기의 기본 파워 값입니다.
        public static int BasePower { get; private set; }
        // 현재 선택된 무기의 인덱스입니다. 저장 데이터와 연동됩니다.
        public static int SelectedWeaponIndex
        {
            get { return save.selectedWeaponIndex; }
            private set { save.selectedWeaponIndex = value; }
        }

        // 새로운 무기가 선택되었을 때 발생하는 이벤트입니다.
        public static event SimpleCallback NewWeaponSelected;
        // 무기가 강화되었을 때 발생하는 이벤트입니다.
        public static event SimpleCallback WeaponUpgraded;
        // 무기 카드 수량이 변경되었을 때 발생하는 이벤트입니다.
        public static event SimpleCallback WeaponCardsAmountChanged;
        // 무기가 잠금 해제되었을 때 발생하는 이벤트입니다.
        public static event WeaponDelagate WeaponUnlocked;

        /// <summary>
        /// WeaponsController를 초기화합니다.
        /// 무기 데이터베이스를 로드하고, 저장 데이터를 불러오며, 무기 데이터를 초기화합니다.
        /// 주요 강화 단계 정보를 수집하고 기본 파워를 설정합니다.
        /// </summary>
        /// <param name="database">사용할 무기 데이터베이스</param>
        public void Init(WeaponDatabase database)
        {
            // 싱글톤 인스턴스를 설정합니다.
            instance = this;

            // 무기 데이터베이스를 할당합니다.
            WeaponsController.database = database;

            // 전역 무기 저장 데이터를 불러오거나 생성합니다.
            save = SaveController.GetSaveObject<GlobalWeaponsSave>("weapon_save");

            // 데이터베이스에서 무기 데이터를 가져옵니다.
            weapons = database.Weapons;

            // 각 무기 데이터를 초기화하고 주요 강화 단계를 수집합니다.
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].Init(); // 개별 무기 데이터를 초기화합니다.

                for (int j = 0; j < weapons[i].Upgrades.Length; j++)
                {
                    WeaponUpgrade currentStage = weapons[i].Upgrades[j];

                    // 주요 강화 단계인 경우 리스트에 추가합니다.
                    if (currentStage.KeyUpgradeNumber != -1)
                    {
                        keyUpgradeStages.Add(currentStage);
                    }

                    // KeyUpgradeNumber가 0인 단계를 기본 파워로 설정합니다.
                    if (currentStage.KeyUpgradeNumber == 0)
                    {
                        BasePower = currentStage.Power;
                    }
                }
            }

            // 주요 강화 단계를 KeyUpgradeNumber 순서로 정렬합니다.
            keyUpgradeStages = keyUpgradeStages.OrderBy(s => s.KeyUpgradeNumber).ToList(); // ToList()를 사용하여 정렬된 결과를 List로 다시 할당

            // 무기 업데이트 상태를 확인합니다 (예: 카드 수량에 따른 잠금 해제).
            CheckWeaponUpdateState();
        }

        /// <summary>
        /// 현재 Key Upgrade 레벨보다 크거나 같은 가장 가까운 Key Upgrade의 파워 값을 가져옵니다.
        /// </summary>
        /// <param name="currentKeyUpgrade">현재 Key Upgrade 레벨</param>
        /// <returns>해당 Key Upgrade 단계의 파워 값</returns>
        public static int GetCeilingKeyPower(int currentKeyUpgrade)
        {
            // 주요 강화 단계 리스트를 역순으로 탐색합니다.
            for (int i = keyUpgradeStages.Count - 1; i >= 0; i--)
            {
                // 현재 Key Upgrade 레벨보다 작거나 같은 Key Upgrade 단계를 찾으면 해당 파워를 반환합니다.
                if (keyUpgradeStages[i].KeyUpgradeNumber <= currentKeyUpgrade)
                {
                    return keyUpgradeStages[i].Power;
                }
            }

            // 해당하는 단계를 찾지 못하면 (예: currentKeyUpgrade가 0보다 작을 때) 첫 번째 Key Upgrade 단계의 파워를 반환합니다.
            return keyUpgradeStages[0].Power;
        }

        /// <summary>
        /// 각 무기의 업데이트 상태 (주로 잠금 해제 상태)를 확인합니다.
        /// 필요한 카드 수량이 충족되면 무기를 잠금 해제(레벨 1로 강화)하고 이벤트를 발생시킵니다.
        /// </summary>
        public void CheckWeaponUpdateState()
        {
            // 모든 무기를 순회합니다.
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponData weapon = weapons[i];

                // 무기가 잠금 상태이고 (UpgradeLevel == 0) 다음 강화(잠금 해제)에 필요한 카드 수량을 충족하면
                if (weapon.UpgradeLevel == 0 && weapons[i].CardsAmount >= weapon.GetNextUpgrade().Price)
                {
                    // 무기를 강화(잠금 해제)합니다.
                    weapon.Upgrade();

                    // 무기 잠금 해제 이벤트를 발생시킵니다.
                    WeaponUnlocked?.Invoke(weapons[i]);
                }
            }
        }

        /// <summary>
        /// 특정 무기 데이터 객체를 사용하여 해당 무기를 선택합니다.
        /// 내부적으로 무기 인덱스를 찾아 SelectWeapon(int weaponIndex) 함수를 호출합니다.
        /// </summary>
        /// <param name="weapon">선택할 무기 데이터</param>
        public static void SelectWeapon(WeaponData weapon)
        {
            int weaponIndex = 0;
            // 데이터베이스에서 해당 무기의 인덱스를 찾습니다.
            for (int i = 0; i < database.Weapons.Length; i++)
            {
                if (database.Weapons[i] == weapon)
                {
                    weaponIndex = i;
                    break; // 찾았으면 반복을 중단합니다.
                }
            }

            // 찾은 인덱스로 무기를 선택합니다.
            SelectWeapon(weaponIndex);
        }

        /// <summary>
        /// 지정된 인덱스의 무기를 선택합니다.
        /// 선택된 무기 인덱스를 저장하고, 캐릭터의 장착 무기를 업데이트하며, 무기 선택 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="weaponIndex">선택할 무기의 인덱스</param>
        public static void SelectWeapon(int weaponIndex)
        {
            // 선택된 무기 인덱스를 설정합니다.
            SelectedWeaponIndex = weaponIndex;

            // 캐릭터 행동 컴포넌트를 가져옵니다.
            CharacterBehaviour characterBehavior = CharacterBehaviour.GetBehaviour();
            // 캐릭터가 존재하는 경우 무기를 장착합니다.
            if(characterBehavior != null)
            {
                // 현재 선택된 무기 데이터를 가져옵니다.
                WeaponData weapon = GetCurrentWeapon();

                // 캐릭터에게 현재 무기와 강화 상태를 장착하도록 합니다.
                characterBehavior.SetGun(weapon, weapon.GetCurrentUpgrade(), true);
                // 캐릭터의 그래픽을 업데이트합니다.
                characterBehavior.Graphics.Grunt();
            }

            // 새로운 무기 선택 이벤트를 발생시킵니다.
            NewWeaponSelected?.Invoke();
        }

        /// <summary>
        /// 특정 무기에 지정된 수량의 카드를 추가합니다.
        /// </summary>
        /// <param name="weapon">카드를 추가할 무기 데이터</param>
        /// <param name="amount">추가할 카드 수량</param>
        public static void AddCard(WeaponData weapon, int amount)
        {
            // 무기 저장 데이터에 카드 수량을 추가합니다.
            weapon.Save.CardsAmount += amount;

            // 무기 카드 수량 변경 이벤트를 발생시킵니다.
            WeaponCardsAmountChanged?.Invoke();
        }

        /// <summary>
        /// 여러 무기에 각각 카드 1개씩을 추가합니다.
        /// </summary>
        /// <param name="weapons">카드를 추가할 무기 데이터 리스트</param>
        public static void AddCards(List<WeaponData> weapons)
        {
            // 무기 리스트가 null이거나 비어있으면 함수를 종료합니다.
            if (weapons.IsNullOrEmpty())
                return;

            // 리스트의 각 무기에 카드 1개씩을 추가합니다.
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].Save.CardsAmount += 1;
            }

            // 무기 카드 수량 변경 이벤트를 발생시킵니다.
            WeaponCardsAmountChanged?.Invoke();
        }

        /// <summary>
        /// 현재 선택된 무기 데이터를 가져옵니다.
        /// </summary>
        /// <returns>현재 선택된 무기 데이터</returns>
        public static WeaponData GetCurrentWeapon()
        {
            // 저장된 선택된 무기 인덱스를 사용하여 데이터베이스에서 무기를 가져옵니다.
            return database.GetWeaponByIndex(save.selectedWeaponIndex);
        }

        /// <summary>
        /// 무기 ID를 사용하여 특정 무기 데이터를 가져옵니다.
        /// </summary>
        /// <param name="weaponID">가져올 무기의 고유 ID</param>
        /// <returns>해당 ID의 무기 데이터</returns>
        public static WeaponData GetWeapon(string weaponID)
        {
            // 데이터베이스에서 무기 ID로 무기 데이터를 가져옵니다.
            return database.GetWeapon(weaponID);
        }

        /// <summary>
        /// 희귀도 타입을 사용하여 해당 희귀도의 설정 데이터를 가져옵니다.
        /// </summary>
        /// <param name="rarity">가져올 희귀도 타입</param>
        /// <returns>해당 희귀도의 설정 데이터</returns>
        public static RarityData GetRarityData(Rarity rarity)
        {
            // 데이터베이스에서 희귀도 타입으로 희귀도 설정 데이터를 가져옵니다.
            return database.GetRarityData(rarity);
        }

        /// <summary>
        /// 무기가 강화되었을 때 호출되는 함수입니다.
        /// 강화 사운드를 재생하고, 캐릭터의 장착 무기를 업데이트하며, 무기 강화 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="weapon">강화된 무기 데이터</param>
        public static void OnWeaponUpgraded(WeaponData weapon)
        {
            // 강화 사운드를 재생합니다.
            AudioController.PlaySound(AudioController.AudioClips.upgrade);

            // 캐릭터 행동 컴포넌트를 가져옵니다.
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            // 캐릭터가 존재하는 경우 장착 무기를 업데이트합니다.
            if(characterBehaviour != null)
            {
                // 현재 선택된 무기 데이터를 가져옵니다.
                WeaponData currentWeapon = GetCurrentWeapon();

                // 캐릭터에게 현재 무기와 강화 상태를 업데이트하도록 합니다.
                characterBehaviour.SetGun(currentWeapon, currentWeapon.GetCurrentUpgrade(), true, true, true);
            }

            // 무기 강화 이벤트를 발생시킵니다.
            WeaponUpgraded?.Invoke();
        }

        /// <summary>
        /// 개발용 함수: 모든 무기를 잠금 해제(레벨 1로 강화)합니다.
        /// </summary>
        public static void UnlockAllWeaponsDev()
        {
            // 데이터베이스의 모든 무기를 순회하며 강화합니다.
            for (int i = 0; i < database.Weapons.Length; i++)
            {
                database.Weapons[i].Upgrade();
            }
        }

        /// <summary>
        /// 특정 무기가 잠금 해제되었는지 확인합니다.
        /// </summary>
        /// <param name="weapon">확인할 무기 데이터</param>
        /// <returns>잠금 해제되었으면 true, 아니면 false</returns>
        public static bool IsWeaponUnlocked(WeaponData weapon)
        {
            // 무기의 강화 레벨이 0보다 크면 잠금 해제된 것으로 간주합니다.
            return weapon.Save.UpgradeLevel > 0;
        }

        // 무기 관련 전역 저장 데이터를 정의하는 직렬화 가능한 클래스입니다.
        [System.Serializable]
        public class GlobalWeaponsSave : ISaveObject // ISaveObject 인터페이스를 구현하여 저장 시스템과 연동됩니다.
        {
            [Tooltip("현재 선택된 무기의 인덱스입니다.")]
            public int selectedWeaponIndex;

            // GlobalWeaponsSave 객체가 생성될 때 호출되는 생성자입니다.
            public GlobalWeaponsSave()
            {
                // 초기 선택된 무기 인덱스를 0으로 설정합니다.
                selectedWeaponIndex = 0;
            }

            /// <summary>
            /// 저장 객체의 데이터를 플러시하는 함수입니다.
            /// 현재 구현에서는 별다른 동작을 수행하지 않습니다.
            /// </summary>
            public void Flush()
            {
                // 이 함수는 저장 시스템에서 데이터를 저장 매체에 실제로 쓰기 전에 호출될 수 있지만,
                // 현재 이 클래스 내에서는 플러시할 추가적인 임시 데이터가 없습니다.
            }
        }

        // 무기 이벤트를 위한 델리게이트입니다. WeaponData 객체를 인자로 받습니다.
        public delegate void WeaponDelagate(WeaponData weapon);
    }
}