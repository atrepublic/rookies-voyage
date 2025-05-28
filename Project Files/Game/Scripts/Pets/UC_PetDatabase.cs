// UC_PetDatabase.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 📌 프로젝트에 존재하는 모든 펫 데이터를 관리하는 Database SO입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Squad Shooter/Pet Database", fileName = "UC_PetDatabase")]
    public class UC_PetDatabase : ScriptableObject
    {
        [Tooltip("게임에서 사용 가능한 모든 펫 데이터 목록")]
        public List<UC_PetData> pets = new List<UC_PetData>();

        private Dictionary<int, UC_PetData> petDict;

        public void Init()
        {
            petDict = pets.ToDictionary(p => p.petID);
        }

        public UC_PetData GetPetDataByID(int id)
        {
            if (petDict == null) Init();
            petDict.TryGetValue(id, out var data);
            return data;
        }

        public List<UC_PetData> GetAllPets() => pets;

        /// <summary>ID에 대응하는 리스트 내 인덱스 반환 (없으면 0)</summary>
        public int GetPetIndexByID(int id)
        {
            int idx = pets.FindIndex(p => p.petID == id);
            return idx >= 0 ? idx : 0;
        }
    }
}
