using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 📌 선택된 펫 ID를 저장하는 전역 저장 클래스입니다.
    /// 게임 전체에서 사용되는 단일 세이브 객체로, 마지막으로 선택된 펫 정보를 유지합니다.
    /// </summary>
    [System.Serializable]
    public class UC_PetGlobalSave : ISaveObject
    {
        [Tooltip("현재 선택된 펫의 고유 ID (UC_PetData.petID)")]
        public int SelectedPetID;

        /// <summary>
        /// 저장이 디스크에 기록되기 직전에 호출되는 메서드입니다.
        /// </summary>
        public void Flush()
        {
            // 필요 시 저장 직전 처리 로직 추가
        }
    }
}
