// BalanceDatabase.cs  v1.01
/*****************************************************************************************
 *  스크립트 기능 및 용도
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • 게임 난이도 밸런싱에 사용되는 프리셋(DifficultySettings)을 보관하는 ScriptableObject입니다.
 *  • 밸런스 컨트롤러(BalanceController)가 레벨 로딩 시 본 데이터를 참조하여
 *    난이도 계산 및 디버그 설정을 수행합니다.
 *  • 에디터 인스펙터에서 ‘IgnoreDifficulty’ 체크 시 난이도 자동 조정을 무시하며,
 *    ‘ShowDebugText’ 체크 시 화면에 난이도 관련 실시간 디버그 텍스트를 표시합니다.
 *****************************************************************************************/

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    ///  난이도 밸런스 데이터베이스 (ScriptableObject)
    ///  ────────────────────────────────────────────────
    ///  • BalanceController.Init() 에서 주입되어 사용됩니다.
    ///  • 인스펙터에서 난이도 프리셋 배열을 자유롭게 편집할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Balance Database", menuName = "Data/Balance Database")]
    public class BalanceDatabase : ScriptableObject
    {
        #region ── 에디터 노출 필드 ─────────────────────────────────────────────────
        [Tooltip("✓ 체크 시, 난이도 자동 조정 로직을 완전히 무시합니다.")]
        [SerializeField] private bool ignoreDifficulty = false;
        public bool IgnoreDifficulty => ignoreDifficulty;

        [Tooltip("✓ 체크 시, BalanceDebugText 오브젝트로 실시간 난이도 정보를 표시합니다.")]
        [SerializeField] private bool showDebugText = false;
        public bool ShowDebugText => showDebugText;

        [Tooltip("레벨 요구 업그레이드 대비 차이에 따라 적용될 난이도 프리셋 목록")]
        [SerializeField] private DifficultySettings[] difficultyPresets;
        public DifficultySettings[] DifficultyPresets => difficultyPresets;
        #endregion
    }
}
