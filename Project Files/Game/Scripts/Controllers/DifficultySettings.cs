// DifficultySettings.cs  v1.01
/*****************************************************************************************
 *  스크립트 기능 및 용도
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • BalanceDatabase에서 참조되는 ‘난이도 프리셋’ 데이터 구조를 정의합니다.
 *  • 각 필드는 플레이어 업그레이드 부족/과잉 시 적에게 적용할 배율(체력·데미지·회복량)과
 *    업그레이드 차이(UpgradeDifference)를 포함합니다.
 *  • DifficultySettings 배열을 임의로 확장/편집하여 세밀한 난이도 곡선을 설계할 수 있습니다.
 *****************************************************************************************/

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    ///  난이도 프리셋 데이터 (직렬화 가능)
    /// </summary>
    [System.Serializable]
    public class DifficultySettings
    {
        #region ── 프리셋 메타 ──────────────────────────────────────────────────────
        [Tooltip("UI·디버그 텍스트에 표시될 난이도 메모(예: Easy / Normal / Hard 등)")]
        [SerializeField] private string note = "Normal";   // 표현용 텍스트
        public string Note => note;
        #endregion

        #region ── 전투 배율 값 ────────────────────────────────────────────────────
        [Tooltip("적 최대 체력에 곱해질 배수 (1.0 = 100%)")]
        [SerializeField] private float healthMult = 1.0f;
        public float HealthMult => healthMult;

        [Tooltip("적 공격력에 곱해질 배수 (1.0 = 100%)")]
        [SerializeField] private float damageMult = 1.0f;
        public float DamageMult => damageMult;

        [Tooltip("적이 회복할 때 사용되는 회복량 배수 (1.0 = 100%)")]
        [SerializeField] private float restoredHpMult = 1.0f;
        public float RestoredHpMult => restoredHpMult;
        #endregion

        #region ── 업그레이드 차이 임계값 ──────────────────────────────────────────
        [Tooltip("플레이어 업그레이드와 요구 업그레이드 사이의 차이가 이 값 미만일 때 이 프리셋이 적용됩니다.")]
        [SerializeField] private int upgradeDifference = 1;
        public int UpgradeDifference => upgradeDifference;
        #endregion

        #region ── 생성자 ──────────────────────────────────────────────────────────
        /// <summary>
        ///  파라미터 기본 생성자 (배수 값 1.0, 차이 1)
        /// </summary>
        public DifficultySettings()
        {
            healthMult      = 1.0f;
            damageMult      = 1.0f;
            restoredHpMult  = 1.0f;
            upgradeDifference = 1;
        }

        /// <summary>
        ///  노트 문자열만 지정하는 생성자 (기타 값은 기본 1.0)
        /// </summary>
        public DifficultySettings(string note)
        {
            this.note       = note;
            healthMult      = 1.0f;
            damageMult      = 1.0f;
            restoredHpMult  = 1.0f;
            upgradeDifference = 1;
        }
        #endregion
    }
}
