/*================================================================
 * FirstLevelTutorial.cs
 * ----------------------------------------------------------------
 * 📌 기능 요약
 *  - 첫 번째 레벨에서 플레이어에게 기본 조작을 안내하는 튜토리얼.
 *  - 목표 적을 처치하고, 출구까지 이동하도록 유도한다.
 *  - NavigationArrow, TutorialLabelBehaviour 를 활용해 시각적 안내를 제공한다.
 *  - 기존 로직은 그대로 유지하고, 한글 주석·툴팁만 추가하였다.
 * ================================================================*/

using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    /// <summary>
    /// ⭐ 첫 레벨 튜토리얼 로직을 담당하는 클래스
    /// ITutorial 인터페이스를 구현해 TutorialController 에 의해 관리된다.
    /// </summary>
    public sealed class FirstLevelTutorial : ITutorial
    {
        /*───────────────────────────────────────────────────────────
         * 📌 필드 / 프로퍼티
         *──────────────────────────────────────────────────────────*/

        private TutorialID tutorialId;                   // 세이브 파일 식별용 ID 값
        public TutorialID TutorialID => tutorialId;      // ITutorial 구현

        private bool isInitialised;                      // Init 여부
        public bool IsInitialised => isInitialised;      // ITutorial 구현

        [SerializeField, Tooltip("플레이어가 이동해야 할 출구 Transform 참조")]
        private Transform finishPointTransform;          // 출구 방향 지점

        [SerializeField, Tooltip("적 머리 위에 표시할 튜토리얼 라벨 Behaviour")]
        private TutorialLabelBehaviour tutorialLabelBehaviour; // 라벨 컴포넌트

        public bool IsActive   => saveData.isActive;     // 현재 활성화 여부 (세이브 기반)
        public bool IsFinished => saveData.isFinished;   // 완료 여부
        public int  Progress   => saveData.progress;     // 진행도 값(사용 안 함)

        private TutorialBaseSave saveData;               // 세이브 데이터 객체

        private LineNavigationArrowCase arrowCase;       // 네비게이션 화살표 인스턴스

        private CharacterBehaviour   characterBehaviour; // 플레이어 캐릭터
        private BaseEnemyBehavior    enemyBehavior;      // 첫 적 캐시

        private bool isCompleted;                        // 튜토리얼 완료 플래그

        /*───────────────────────────────────────────────────────────
         * 📌 ITutorial 메서드 구현
         *──────────────────────────────────────────────────────────*/

        /// <summary>
        /// 🔹 튜토리얼 초기화 – 세이브 데이터 로드
        /// </summary>
        public void Init()
        {
            if (isInitialised)
                return;

            isInitialised = true;

            // 세이브 파일 로드 (없으면 새로 생성)
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, tutorialId.ToString()));
        }

        /// <summary>
        /// 🔹 튜토리얼 시작 – 적/화살표/라벨 세팅 및 이벤트 구독
        /// </summary>
        public void StartTutorial()
        {
            // 세이브 플래그 갱신
            saveData.isActive = true;

            characterBehaviour = CharacterBehaviour.GetBehaviour();

            // 이미 완료됐다면 스킵
            if (isCompleted)
                return;

            LevelController.EnableManualExitActivation();

            // 첫 번째 적을 목표로 지정
            enemyBehavior = ActiveRoom.Enemies[0];

            // 화살표 생성
            arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, enemyBehavior.transform.position);
            arrowCase.FixArrowToTarget(enemyBehavior.transform);

            // 적 머리 위 라벨 표시
            tutorialLabelBehaviour.Activate("KILL THE ENEMY", enemyBehavior.transform, new Vector3(0, 20.0f, 0));

            // 적 사망 이벤트 구독
            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        /// <summary>
        /// 🔹 튜토리얼 강제 완료 처리 (스킵 등)
        /// </summary>
        public void FinishTutorial()
        {
            saveData.isFinished = true;
        }

        /// <summary>
        /// 🔹 레벨 언로드 시 호출 – 화살표/라벨 정리
        /// </summary>
        public void Unload()
        {
            if (arrowCase != null)
                arrowCase.DisableArrow();
        }

        /*───────────────────────────────────────────────────────────
         * 📌 내부 콜백
         *──────────────────────────────────────────────────────────*/

        /// <summary>
        /// ☑ 목표 적을 처치했을 때 호출되는 콜백
        /// </summary>
        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            if (enemy == enemyBehavior)
            {
                // 이벤트 해제
                BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;

                // 라벨 비활성화
                tutorialLabelBehaviour.Disable();

                // 기존 화살표 제거 후 출구 화살표로 교체
                if (arrowCase != null)
                {
                    arrowCase.DisableArrow();
                    arrowCase = null;
                }

                arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, finishPointTransform.position);

                LevelController.ActivateExit();

                LevelController.OnPlayerExitLevelEvent += OnPlayerExitLevel;
            }
        }

        /// <summary>
        /// ☑ 플레이어가 출구에 도달했을 때 호출되는 콜백
        /// </summary>
        private void OnPlayerExitLevel()
        {
            LevelController.OnPlayerExitLevelEvent -= OnPlayerExitLevel;

            if (arrowCase != null)
            {
                arrowCase.DisableArrow();
                arrowCase = null;
            }

            isCompleted = true;
        }
    }
}
