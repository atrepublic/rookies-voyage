// ==============================================
// 📌 ExperienceDatabase.cs
// ✅ ScriptableObject 기반 경험치 레벨 데이터 관리
// ✅ 각 레벨별 필요 경험치를 리스트로 저장
// ==============================================

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Experience Database", menuName = "Data/Experience Database")]
    public class ExperienceDatabase : ScriptableObject
    {
        [Tooltip("각 레벨별 경험치 데이터")]
        [SerializeField] private List<ExperienceLevelData> experienceData;
        public List<ExperienceLevelData> ExperienceData => experienceData;

        /// <summary>
        /// 📌 데이터 초기화: 각 레벨에 맞는 인덱스 지정
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < experienceData.Count; i++)
            {
                experienceData[i].SetLevel(i + 1);
            }
        }

        /// <summary>
        /// 📌 특정 레벨의 데이터 가져오기
        /// </summary>
        public ExperienceLevelData GetDataForLevel(int level)
        {
            return experienceData[Mathf.Clamp(level - 1, 0, experienceData.Count - 1)];
        }
    }

    [System.Serializable]
    public class WorldExperienceData
    {
        [Tooltip("월드 구분 번호")]
        [SerializeField] private int worldNumber;
        public int WorldNumber => worldNumber;

        [Tooltip("해당 월드의 최대 레벨")]
        [SerializeField] private int maxExpLevel;
        public int MaxExpLevel => maxExpLevel;
    }
}
