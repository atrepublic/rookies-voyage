// UC_PetSave.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 📌 펫 업그레이드 레벨 저장용 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class UC_PetSave : ISaveObject
    {
        [Tooltip("펫별 업그레이드 레벨 정보 목록")]
        public List<PetUpgradeData> petUpgrades = new List<PetUpgradeData>();

        public void Flush() { /* 저장 직전 처리 */ }

        [System.Serializable]
        public class PetUpgradeData
        {
            [Tooltip("펫의 고유 ID (UC_PetData.petID)")]
            public int petID;
            [Tooltip("해당 펫의 현재 업그레이드 레벨 (0부터 시작; -1은 미언락)")]
            public int upgradeLevel;
        }

        /// <summary>펫 현재 레벨 조회 (언락 전에는 -1 반환)</summary>
        public int GetLevel(int petID)
        {
            var entry = petUpgrades.FirstOrDefault(p => p.petID == petID);
            return entry != null ? entry.upgradeLevel : -1;
        }

        /// <summary>펫이 언락되었는지 여부</summary>
        public bool HasPet(int petID) => GetLevel(petID) >= 0;

        /// <summary>펫 언락 (레벨 0으로 추가)</summary>
        public void UnlockPet(int petID)
        {
            if (!HasPet(petID))
                petUpgrades.Add(new PetUpgradeData { petID = petID, upgradeLevel = 0 });
                SaveController.MarkAsSaveIsRequired();
        }

        /// <summary>펫 레벨 설정</summary>
        public void SetLevel(int petID, int level)
        {
            var entry = petUpgrades.FirstOrDefault(p => p.petID == petID);
            if (entry != null)
                entry.upgradeLevel = level;
        }
    }
}
