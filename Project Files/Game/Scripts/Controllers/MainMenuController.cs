// MainMenuController.cs  v1.02
/*****************************************************************************************
 *  스크립트 기능 및 용도
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • 메인 메뉴 로직을 제어합니다.
 *    - 게임 설정에 맞춰 각종 컨트롤러(무기, 캐릭터, 밸런스, 경험치, 파티클)를 초기화합니다.
 *    - 플레이어에게 캐릭터 제안 UI 또는 메인 메뉴 UI를 보여줍니다.
 *    - 레벨 선택 시 저장된 데이터로 게임 씬을 로드합니다.
 *    - 기본 제공 펫(루키, petID 0)을 항상 언락 상태로 보장합니다.
 *****************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    [DefaultExecutionOrder(-10)]
    public class MainMenuController : MonoBehaviour
    {
        #region ── 에디터 노출 필드 ─────────────────────────────────────────────────
        [Tooltip("UIController 컴포넌트: 메뉴 UI 페이지 초기화 및 전환을 담당합니다.")]
        [SerializeField] private UIController uiController;

        [Tooltip("PedestalBehavior 컴포넌트: 메인 메뉴 캐릭터 전시대를 제어합니다.")]
        [SerializeField] private PedestalBehavior pedestalBehavior;
        #endregion

        #region ── 내부 캐싱된 컨트롤러 참조 ─────────────────────────────────────────
        private static WeaponsController    weaponsController;
        private static CharactersController charactersController;
        private static BalanceController    balanceController;
        private static ExperienceController experienceController;
        private static ParticlesController  particlesController;
        #endregion

        #region ── 초기화 ─────────────────────────────────────────────────────────────
        /// <summary>
        ///  메인 메뉴 진입 시 실행됩니다.
        ///  • 주요 컨트롤러를 CacheComponent로 가져와 초기화하고,
        ///  • UI 및 전시대(pedestal)를 초기화한 뒤 적절한 UI 페이지를 표시합니다.
        ///  • 기본 제공 펫(루키, petID 0)이 언락되었는지 확인하고, 미언락 시 언락 처리합니다.
        /// </summary>
        private void Awake()
        {
            // 컨트롤러 컴포넌트 캐싱
            gameObject.CacheComponent(out weaponsController);
            gameObject.CacheComponent(out charactersController);
            gameObject.CacheComponent(out balanceController);
            gameObject.CacheComponent(out experienceController);
            gameObject.CacheComponent(out particlesController);

            // 게임 설정 로드
            GameSettings gameSettings = GameSettings.GetSettings();

            // UI 초기화
            uiController.Init();

            // 컨트롤러별 데이터 초기화
            weaponsController.Init(gameSettings.WeaponDatabase);
            charactersController.Init(gameSettings.CharactersDatabase);
            balanceController.Init(gameSettings.BalanceDatabase);
            experienceController.Init(gameSettings.ExperienceDatabase);

            // 파티클 이펙트 컨트롤러 초기화
            particlesController.Init();

            // 전시대 초기화
            pedestalBehavior.Init();

            //PetManager.Instance.SpawnSelectedPet();

            // [추가] 기본 제공 펫(루키, petID 0) 언락 보장
            {
                var petSave = SaveController.GetSaveObject<UC_PetSave>("pet");
                if (!petSave.HasPet(0))
                {
                    petSave.UnlockPet(0);
                    SaveController.Save(forceSave: true);
                }
            }

            // UI 페이지 구성
            uiController.InitPages();

            // 캐릭터 제안 필요 여부에 따라 제안 UI 또는 메인 메뉴 UI 표시
            UICharacterSuggestion suggestionUI = UIController.GetPage<UICharacterSuggestion>();
            if (suggestionUI != null && LevelController.NeedCharacterSugession)
            {
                UIController.ShowPage<UICharacterSuggestion>();
            }
            else
            {
                UIController.ShowPage<UIMainMenu>();
            }
            
            PetManager.Instance.SpawnSelectedPet(); 
        }
        #endregion

        #region ── 로딩 완료 통지 ────────────────────────────────────────────────────
        /// <summary>
        ///  모든 준비가 끝난 후 호출됩니다.
        ///  • 로딩 화면을 숨깁니다.
        /// </summary>
        private void Start()
        {
            GameLoading.MarkAsReadyToHide();
        }
        #endregion

        #region ── 레벨 로드 ─────────────────────────────────────────────────────────
        /// <summary>
        ///  사용자가 선택한 월드 및 레벨을 저장하고 게임 씬으로 전환합니다.
        ///  • LevelSave에 선택 정보를 기록한 뒤 Overlay로 씬 전환 애니메이션을 수행합니다.
        /// </summary>
        /// <param name="worldIndex">선택한 월드 인덱스 (0-based)</param>
        /// <param name="levelIndex">선택한 레벨 인덱스 (0-based)</param>
        public void LoadLevel(int worldIndex, int levelIndex)
        {
            // 레벨 저장 데이터 가져오기
            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>("level");
            levelSave.WorldIndex = worldIndex;
            levelSave.LevelIndex = levelIndex;

            // Overlay 애니메이션과 씬 전환
            Overlay.Show(0.3f, () =>
            {
                SceneManager.LoadScene("Game");
                Overlay.Hide(0.3f);
            });
        }
        #endregion
    }
}
