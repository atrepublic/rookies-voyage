// ==============================================
// 📌 ExperienceLevelData.cs
// ✅ 특정 레벨에 필요한 경험치 데이터를 담고 있는 클래스
// ✅ ExperienceDatabase 에서 사용되며, 레벨당 경험치 요구량 정의
// ==============================================

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ExperienceLevelData
    {
        [Tooltip("이 레벨에 도달하기 위해 필요한 총 경험치")]
        [SerializeField] private int experienceRequired;
        public int ExperienceRequired => experienceRequired;

        [Tooltip("레벨 값 (자동 설정됨)")]
        public int Level { get; private set; }

        /// <summary>
        /// 📌 해당 데이터에 레벨 값을 할당
        /// </summary>
        public void SetLevel(int level)
        {
            Level = level;
        }

        /// <summary>
        /// 📌 이 레벨에 필요한 경험치 수치를 설정
        /// </summary>
        public void SetExperienceRequred(int amount)
        {
            experienceRequired = amount;
        }
    }
}
