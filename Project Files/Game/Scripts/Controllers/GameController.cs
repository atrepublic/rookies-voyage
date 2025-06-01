// GameController.cs  v1.01
/*****************************************************************************************
 *  스크립트 기능 및 용도
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • 게임 실행 흐름 전반을 관리하는 싱글톤 스타일 컨트롤러입니다.
 *    - 초기 Awake()에서 UI, 카메라, 레벨, 컨트롤러(Experience, Weapons, Characters 등)를
 *      순차적으로 초기화하고 첫 플레이어를 생성합니다.
 *    - 레벨 완료, 실패, 재시도, 리바이브 등 주요 게임 상태 전환 메서드를 제공합니다.
 *    - 게임 오버 시 전면 광고 호출, 결과 화면 노출 등을 처리합니다.
 *****************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    [DefaultExecutionOrder(-10)]
    public class GameController : MonoBehaviour
    {
        #region ── 에디터 노출 필드 ─────────────────────────────────────────────────
        [Tooltip("UIController 컴포넌트: UI 초기화 및 페이지 전환 제어를 담당합니다.")]
        [SerializeField] private UIController uiController;

        [Tooltip("CameraController 컴포넌트: 게임 플레이 카메라를 초기화 및 제어합니다.")]
        [SerializeField] private CameraController cameraController;
        #endregion

        #region ── 내부 캐싱된 컨트롤러 참조 ─────────────────────────────────────────
        private static ParticlesController    particlesController;
        private static FloatingTextController floatingTextController;
        private static ExperienceController   experienceController;
        private static WeaponsController      weaponsController;
        private static CharactersController   charactersController;
        private static BalanceController      balanceController;
        private static EnemyController        enemyController;
        private static TutorialController     tutorialController;
        #endregion

        #region ── 게임 상태 변수 ─────────────────────────────────────────────────────
        private static bool isGameActive;
        public  static bool IsGameActive => isGameActive;
        #endregion

        #region ── 초기화 및 해제 ────────────────────────────────────────────────────
        /// <summary>
        ///  게임 시작 시 호출됩니다.
        ///  • 모든 매니저 컨트롤러를 CacheComponent로 가져와 초기화
        ///  • UI, 튜토리얼, 파티클, 플로팅 텍스트 초기화
        ///  • 레벨 생성, 경험치·캐릭터·무기·밸런스 데이터 초기화
        ///  • 플레이어 스폰 및 카메라 초기화, UI 페이지 전환 및 레벨 로드
        /// </summary>
        private void Awake()
        {
            // 캐싱된 컴포넌트 초기화
            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out experienceController);
            gameObject.CacheComponent(out weaponsController);
            gameObject.CacheComponent(out charactersController);
            gameObject.CacheComponent(out balanceController);
            gameObject.CacheComponent(out enemyController);
            gameObject.CacheComponent(out tutorialController);

            // 게임 설정 로드
            GameSettings gameSettings = GameSettings.GetSettings();

            // UI 초기화
            uiController.Init();

            // 튜토리얼·파티클·플로팅 텍스트 초기화
            tutorialController.Init();
            particlesController.Init();
            //floatingTextController.Init();
            if (gameSettings.FloatingTextPresets != null)
                {
                    floatingTextController.Init(gameSettings.FloatingTextPresets); // ✅ 수정된 호출
                }
                else
                {
                    Debug.LogWarning("[GameController] Awake: GameSettings에 FloatingTextPresets가 설정되지 않았습니다. FloatingTextController가 비어있는 상태로 초기화될 수 있습니다.");
                    floatingTextController.Init(new FloatingTextController.FloatingTextCase[0]); // 빈 배열로라도 초기화
                }

            // 레벨 오브젝트 생성
            LevelController.CreateLevelObject();

            // 주요 컨트롤러 데이터 초기화
            experienceController.Init(gameSettings.ExperienceDatabase);
            charactersController.Init(gameSettings.CharactersDatabase);
            weaponsController.Init(gameSettings.WeaponDatabase);
            balanceController.Init(gameSettings.BalanceDatabase);
            enemyController.Init();

            // 플레이어 생성 및 카메라 연결
            CharacterBehaviour player = LevelController.SpawnPlayer();
            cameraController.Init(player.transform);

            // ▶ 여기에 추가: PetController 자동 초기화
            var petController = Object.FindFirstObjectByType<PetController>();
            // 또는 더 빠른 탐색이 필요하면
            // var petController = Object.FindAnyObjectByType<PetController>();


            if (petController != null)
            {
                petController.Init(player.transform);
            }

            //PetManager.Instance.SpawnPet();

            // UI 페이지 세팅
            uiController.InitPages();
            UIController.ShowPage<UIGame>();

            // 현재 레벨 로드 및 활성화
            LevelController.LoadCurrentLevel();
            LevelController.ActivateLevel(() => {
                
            // ▶ 레벨 활성화 후 펫 소환
            //PetManager.Instance.SpawnPet();
            isGameActive = true; });
        }

        /// <summary>
        ///  오브젝트 파괴 전 호출됩니다.
        ///  • 레벨 언로드 및 Tween 모든 콜백 제거
        /// </summary>
        private void OnDestroy()
        {
            LevelController.Unload();
            Tween.RemoveAll();
        }
        #endregion

        #region ── 레벨 완료/종료 처리 ────────────────────────────────────────────────
        /// <summary>
        ///  레벨 클리어 시 호출됩니다.
        ///  • UIComplete 페이지에 결과 데이터 전달 후 전환
        ///  • 경험치 획득 처리, 레벨 언로드
        ///  • 게임 활성화 플래그 비활성화
        /// </summary>
        public static void LevelComplete()
        {
            if (!isGameActive) return;

            LevelData current = LevelController.CurrentLevelData;

            UIComplete completePage = UIController.GetPage<UIComplete>();
            completePage.SetData(
                ActiveRoom.CurrentWorldIndex + 1,
                ActiveRoom.CurrentLevelIndex + 1,
                current.GetCoinsReward(),
                current.XPAmount,
                current.GetCardsReward());

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();

            ExperienceController.GainExperience(current.XPAmount);

            LevelController.UnloadLevel();
            isGameActive = false;
        }

        /// <summary>
        ///  클리어 결과 창 종료 시 호출됩니다.
        ///  • 저장 후 메인 메뉴 씬으로 이동
        /// </summary>
        public static void OnLevelCompleteClosed()
        {
            Overlay.Show(0.3f, () => {
                SaveController.Save(true);
                SceneManager.LoadScene("Menu");
                Overlay.Hide(0.3f);
            });
        }

        /// <summary>
        ///  레벨 중단(Exit) 시 호출됩니다.
        ///  • 캐릭터 제안 비활성화 후 메인 메뉴 이동
        /// </summary>
        public static void OnLevelExit()
        {
            isGameActive = false;
            LevelController.DisableCharacterSuggestion();
            SceneManager.LoadScene("Menu");
        }
        #endregion

        #region ── 레벨 실패 처리 ─────────────────────────────────────────────────
        /// <summary>
        ///  레벨 실패 시 호출됩니다.
        ///  • UIGame 화면 감추고 UIGameOver 표시
        ///  • 실패 시 전면 광고 호출을 위해 이벤트 등록
        ///  • LevelController에 실패 알림, 활성화 플래그 비활성화
        /// </summary>
        public static void OnLevelFailded()
        {
            if (!isGameActive) return;

            UIController.HidePage<UIGame>(() => {
                UIController.ShowPage<UIGameOver>();
                UIController.PageOpened += OnFailedPageOpened;
            });

            LevelController.OnLevelFailed();
            isGameActive = false;
        }

        /// <summary>
        ///  실패 페이지 열림 시 호출됩니다.
        ///  • 페이지가 UIGameOver면 전면 광고 호출 후 이벤트 제거
        /// </summary>
        private static void OnFailedPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType == typeof(UIGameOver))
            {
                AdsManager.ShowInterstitial(null);
                UIController.PageOpened -= OnFailedPageOpened;
            }
        }
        #endregion

        #region ── 재시작 및 부활 ─────────────────────────────────────────────────
        /// <summary>
        ///  다시 시작(Replay) 버튼 클릭 시 호출됩니다.
        ///  • 게임 활성화, 현재 레벨 언로드 후 "Game" 씬 재로드
        /// </summary>
        public static void OnReplayLevel()
        {
            isGameActive = true;
            Overlay.Show(0.3f, () => {
                LevelController.UnloadLevel();
                SceneManager.LoadScene("Game");
                Overlay.Hide(0.3f);
            });
        }

        /// <summary>
        ///  부활(Revive) 시 호출됩니다.
        ///  • 게임 활성화, 게임오버 UI 닫고 캐릭터 부활 후 UIGame 표시
        /// </summary>
        public static void OnRevive()
        {
            isGameActive = true;
            UIController.HidePage<UIGameOver>(() => {
                LevelController.ReviveCharacter();
                UIController.ShowPage<UIGame>();
            });
        }
        #endregion

        #region ── 오디오 제어 ─────────────────────────────────────────────────────
        /// <summary>
        ///  특정 AudioClip을 현재 재생 중인 뮤직 소스로 설정해 재생합니다.
        /// </summary>
        public static void PlayCustomMusic(AudioClip music)
        {
            if (music == null) return;

            MusicSource source = MusicSource.ActiveMusicSource;
            if (source != null)
            {
                source.AudioSource.clip = music;
                source.Activate();
            }
        }
        #endregion
    }
}
