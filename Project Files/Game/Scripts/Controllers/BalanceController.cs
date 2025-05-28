// BalanceController.cs  v1.01
/*****************************************************************************************
 *  스크립트 기능 및 용도
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • 현재 레벨(LevelController.CurrentLevelData)과 플레이어의 무기/캐릭터 업그레이드 상황을
 *    비교하여 요구 업그레이드(PowerRequirement)를 계산합니다.
 *  • 요구 업그레이드 대비 실제 업그레이드 차이(UpgradesDifference)에 따라
 *    DifficultySettings(난이도 프리셋)를 결정합니다.
 *  • BalanceUpdated 이벤트를 통해 난이도 변동 사실을 UI 등 외부 모듈에 알립니다.
 *  • 개발 편의를 위해 디버그 텍스트(BalanceDebugText)를 화면에 표시할 수 있습니다.
 *
 *  ※ 내부 로직은 원본과 동일하며, 변수·함수명은 유지했습니다. (동작 변경 없음)
 *  ※ Unity 2023 LTS 권장 문법을 반영하고, 핵심 변수 및 함수 위에 한글 주석과 툴팁을 추가했습니다.
 *****************************************************************************************/

using System.Linq;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class BalanceController : MonoBehaviour
    {
        #region ── 정적/상수 필드 ───────────────────────────────────────────────────
        /// <summary>
        ///  업그레이드 차이가 계산되지 않았을 때 사용되는 기본 난이도 프리셋
        /// </summary>
        private static readonly DifficultySettings DEFAULT_DIFFICULTY = new("Default");
        #endregion

        #region ── 인스펙터/인스턴스 필드 ───────────────────────────────────────────
        [Tooltip("현재 난이도 및 업그레이드를 화면에 출력하는 디버그 텍스트 오브젝트")]
        private BalanceDebugText debugText;
        #endregion

        #region ── 정적 프로퍼티 ────────────────────────────────────────────────────
        public static DifficultySettings CurrentDifficulty { get; private set; }
        public static int PowerRequirement { get; private set; }
        public static int CurrentGeneralPower =>
            CharactersController.SelectedCharacter.GetCurrentUpgrade().Stats.Power +
            WeaponsController.GetCurrentWeapon().GetCurrentUpgrade().Power;
        public static int UpgradesDifference { get; private set; }
        #endregion

        #region ── 이벤트 ────────────────────────────────────────────────────────────
        public static event SimpleBoolCallback BalanceUpdated;
        #endregion

        #region ── 정적 참조 ─────────────────────────────────────────────────────────
        private static BalanceDatabase database;
        #endregion

        #region ── 초기화 / 해제 ─────────────────────────────────────────────────────
        /// <summary>
        ///  외부에서 BalanceDatabase 를 주입하여 컨트롤러를 초기화합니다.
        ///  (GameController → balanceController.Init 호출)
        /// </summary>
        public void Init(BalanceDatabase database)
        {
            BalanceController.database = database;

            // 캐릭터/무기 업그레이드 및 선택 이벤트 구독
            CharactersController.OnCharacterUpgradedEvent += OnCharacterUpgraded;
            CharactersController.OnCharacterSelectedEvent += OnCharacterSelected;
            WeaponsController.WeaponUpgraded += OnWeaponUpgraded;
            WeaponsController.NewWeaponSelected += OnWeaponSelected;

            // 최초 난이도 계산
            UpdateDifficulty(false);

            // 디버그 텍스트 생성(옵션)
            if (database.ShowDebugText)
                debugText = BalanceDebugText.Create();
        }

        private void OnDestroy()
        {
            // 정적 필드 초기화
            CurrentDifficulty = DEFAULT_DIFFICULTY;
            PowerRequirement = 1;
            UpgradesDifference = 0;

            // 이벤트 구독 해제
            CharactersController.OnCharacterUpgradedEvent -= OnCharacterUpgraded;
            CharactersController.OnCharacterSelectedEvent -= OnCharacterSelected;
            WeaponsController.WeaponUpgraded -= OnWeaponUpgraded;
            WeaponsController.NewWeaponSelected -= OnWeaponSelected;

            if (debugText != null) Destroy(debugText.gameObject);
        }
        #endregion

        #region ── 핵심 로직 : 난이도 계산 ───────────────────────────────────────────
        /// <summary>
        ///  현재 레벨 요구 업그레이드와 플레이어 업그레이드를 비교하여 난이도를 갱신합니다.
        /// </summary>
        /// <param name="highlight">UI 강조 연출 여부</param>
        public static void UpdateDifficulty(bool highlight)
        {
            // 레벨 데이터 또는 난이도 프리셋이 없거나 무시 플래그가 켜져 있으면 기본 값 유지
            if (LevelController.CurrentLevelData == null || database.DifficultyPresets.IsNullOrEmpty() || database.IgnoreDifficulty)
            {
                CurrentDifficulty = DEFAULT_DIFFICULTY;
                PowerRequirement = 1;

                BalanceUpdated?.Invoke(highlight);
                return;
            }

            // 무기·캐릭터 업그레이드 요구값 합산 (최소 상한선을 ceiling 방식으로 계산)
            PowerRequirement =
                WeaponsController.GetCeilingKeyPower(LevelController.CurrentLevelData.RequiredUpg) +
                CharactersController.GetCeilingUpgradePower(LevelController.CurrentLevelData.RequiredUpg);

            // 실제 업그레이드와의 차이 계산 (6단계마다 DifficultyPresets 업데이트가 권장됨)
            UpgradesDifference = Mathf.RoundToInt((PowerRequirement - CurrentGeneralPower) / 6f);

            // 업그레이드 차이를 기준으로 DifficultySettings 선택
            DifficultySettings tempPreset = null;
            DifficultySettings[] presets = database.DifficultyPresets.OrderBy(x => x.UpgradeDifference).ToArray();
            foreach (DifficultySettings preset in presets)
            {
                if (UpgradesDifference < preset.UpgradeDifference)
                {
                    tempPreset = preset;
                    break;
                }
            }
            // 조건에 맞는 프리셋이 없으면 가장 마지막(가장 어려운) 난이도 선택
            if (tempPreset == null) tempPreset = presets[^1];
            CurrentDifficulty = tempPreset;

            BalanceUpdated?.Invoke(highlight);
        }
        #endregion

        #region ── 콜백: 캐릭터/무기 변경 대응 ──────────────────────────────────────
        private void OnCharacterUpgraded(CharacterData _) => UpdateDifficulty(true);
        private void OnCharacterSelected(CharacterData _) => UpdateDifficulty(false);
        private void OnWeaponUpgraded() => UpdateDifficulty(true);
        private void OnWeaponSelected() => UpdateDifficulty(false);
        #endregion
    }
}
