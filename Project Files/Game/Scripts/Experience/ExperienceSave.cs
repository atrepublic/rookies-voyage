// ==============================================
// 📌 ExperienceSave.cs
// ✅ 경험치 시스템 저장 데이터 구조
// ✅ 현재 레벨, 경험치, 수집한 경험치를 저장함
// ==============================================

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ExperienceSave : ISaveObject
    {
        [Tooltip("현재 레벨")]
        public int CurrentLevel = 1;

        [Tooltip("현재 누적된 경험치")]
        public int CurrentExperiencePoints;

        [Tooltip("이번 플레이에서 수집한 경험치 (임시)")]
        public int CollectedExperiencePoints;

        /// <summary>
        /// 📌 저장 시 호출되는 메서드 (현재 구현 없음)
        /// </summary>
        public void Flush()
        {
            // 저장 처리용 (필요 시 구현)
        }
    }
}
