// 이 스크립트는 튜토리얼 레벨에 대한 특수 동작을 정의하는 ScriptableObject입니다.
// LevelSpecialBehaviour를 상속받아 레벨 생명주기 이벤트에 반응하며,
// ITutorial 인터페이스를 구현하여 튜토리얼 관련 데이터를 제공하고 상태를 관리합니다.
// 튜토리얼 진행 상태 저장 및 로드, 내비게이션 화살표 표시, 튜토리얼 레이블 관리 등의 기능을 포함합니다.
using UnityEngine;
using Watermelon.SquadShooter;
using System.Collections.Generic;

namespace Watermelon.LevelSystem
{
    // 튜토리얼 레벨의 특수 동작을 정의하는 ScriptableObject 자산입니다.
    // Project 창에서 에셋으로 생성할 수 있습니다.
    [CreateAssetMenu(fileName = "Level Tutorial Behaviour", menuName = "Data/New Level/Behaviours/Tutorial")]
    // LevelSpecialBehaviour를 상속하고 ITutorial 인터페이스를 구현합니다.
    public sealed class TutorialLevelSpecialBehaviour : LevelSpecialBehaviour, ITutorial
    {
        // 이 튜토리얼의 고유 식별자입니다.
        [Tooltip("이 튜토리얼의 고유 식별자입니다.")]
        [SerializeField] TutorialID tutorialId;
        // 튜토리얼 ID를 가져오는 프로퍼티입니다.
        public TutorialID TutorialID => tutorialId;

        // 튜토리얼이 초기화되었는지 여부를 나타냅니다. 씬 로드 간에 상태를 유지하지 않습니다.
        [System.NonSerialized]
        private bool isInitialised;
        // 튜토리얼 초기화 상태를 가져오는 프로퍼티입니다.
        public bool IsInitialised => isInitialised;

        // 현재 방의 출구 지점 트랜스폼입니다. 내비게이션 화살표의 목표 지점으로 사용될 수 있습니다.
        private Transform finishPointTransform;
        // 튜토리얼 메시지를 표시하는 레이블 동작 컴포넌트입니다.
        private TutorialLabelBehaviour tutorialLabelBehaviour;

        // 튜토리얼이 현재 활성화 상태인지 여부를 나타냅니다. (저장 데이터 기반)
        public bool IsActive => saveData.isActive;
        // 튜토리얼이 이미 완료되었는지 여부를 나타냅니다. (저장 데이터 기반)
        public bool IsFinished => saveData.isFinished;
        // 튜토리얼 진행 단계 또는 관련 진행도입니다. (저장 데이터 기반)
        public int Progress => saveData.progress;

        // 튜토리얼의 저장 데이터입니다.
        private TutorialBaseSave saveData;

        // 내비게이션 화살표를 관리하는 케이스입니다. 플레이어를 목표 지점으로 안내하는 데 사용됩니다.
        private LineNavigationArrowCase arrowCase;

        // 플레이어 캐릭터의 동작 컴포넌트입니다.
        private CharacterBehaviour characterBehaviour;
        // 현재 튜토리얼 단계의 대상 적(Enemy) 동작 컴포넌트입니다.
        private BaseEnemyBehavior enemyBehavior;

        // 게임 UI 페이지에 대한 참조입니다.
        private UIGame gameUI;

        // 튜토리얼 특수 동작을 초기화합니다.
        // 저장 데이터를 로드하고 게임 UI 페이지에 대한 참조를 얻습니다.
        public void Init()
        {
            // 초기화 상태를 true로 설정합니다.
            isInitialised = true;

            // 튜토리얼 ID를 기반으로 저장 데이터를 로드하거나 새로 생성합니다.
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, tutorialId.ToString()));

            // 게임 UI 페이지에 대한 참조를 가져옵니다.
            gameUI = UIController.GetPage<UIGame>();
        }

        // 튜토리얼을 시작합니다.
        // 튜토리얼이 아직 완료되지 않았으면 활성화 상태로 설정하고 플레이어 캐릭터 참조를 가져옵니다.
        public void StartTutorial()
        {
            // 튜토리얼이 이미 완료되었으면 시작하지 않습니다.
            if (saveData.isFinished) return;

            // 튜토리얼을 활성화 상태로 설정합니다.
            saveData.isActive = true;

            // 플레이어 캐릭터 동작 컴포넌트에 대한 참조를 가져옵니다.
            characterBehaviour = CharacterBehaviour.GetBehaviour();
        }

        // 적이 사망했을 때 호출되는 이벤트 핸들러 메소드입니다.
        // 현재 튜토리얼 대상 적이 사망했는지 확인하고, 필요한 후속 조치(화살표, 레이블 비활성화, 다음 목표 설정 등)를 수행합니다.
        // enemy: 사망한 적의 동작 컴포넌트
        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            // 사망한 적이 현재 튜토리얼의 대상 적인지 확인합니다.
            if (enemy == enemyBehavior)
            {
                // 적 사망 이벤트 구독을 해제합니다.
                BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;

                // 내비게이션 화살표가 있다면 비활성화하고 참조를 해제합니다.
                if (arrowCase != null)
                {
                    arrowCase.DisableArrow();
                    arrowCase = null;
                }

                // 튜토리얼 레이블을 비활성화합니다.
                tutorialLabelBehaviour.Disable();

                // 플레이어 위치에서 방의 출구 지점까지 안내하는 내비게이션 화살표를 등록합니다.
                arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, finishPointTransform.position);

                // 레벨 출구를 활성화하여 플레이어가 다음 레벨로 이동할 수 있도록 합니다.
                LevelController.ActivateExit();

                // 플레이어가 레벨 출구로 나갈 때 호출될 이벤트 핸들러를 구독합니다.
                LevelController.OnPlayerExitLevelEvent += OnPlayerExitLevel;
            }
        }

        // 플레이어가 레벨 출구로 나갔을 때 호출되는 이벤트 핸들러 메소드입니다.
        // 레벨 출구 이벤트 구독을 해제하고, 내비게이션 화살표를 비활성화한 후 튜토리얼을 완료 상태로 만듭니다.
        private void OnPlayerExitLevel()
        {
            // 레벨 출구 이벤트 구독을 해제합니다.
            LevelController.OnPlayerExitLevelEvent -= OnPlayerExitLevel;

            // 내비게이션 화살표가 있다면 비활성화하고 참조를 해제합니다.
            if (arrowCase != null)
            {
                arrowCase.DisableArrow();
                arrowCase = null;
            }

            // 튜토리얼 완료 처리를 수행합니다.
            FinishTutorial();
        }

        // 튜토리얼을 완료 상태로 설정하는 메소드입니다.
        // 저장 데이터의 완료 상태를 업데이트하고, 초기화 상태를 해제하며, 게임 UI의 일시정지 버튼을 활성화합니다.
        public void FinishTutorial()
        {
            // 저장 데이터에 튜토리얼 완료 상태를 true로 설정합니다.
            saveData.isFinished = true;

            // 초기화 상태를 false로 설정합니다.
            isInitialised = false;

            // 게임 UI의 일시정지 버튼을 활성화합니다.
            gameUI.PauseButton.gameObject.SetActive(true);
        }

        // 튜토리얼 리소스를 언로드하는 메소드입니다.
        // 내비게이션 화살표와 튜토리얼 레이블을 비활성화합니다.
        public void Unload()
        {
            // 내비게이션 화살표가 있다면 비활성화합니다.
            if (arrowCase != null)
                arrowCase.DisableArrow();

            // 튜토리얼 레이블이 있다면 비활성화합니다.
            if (tutorialLabelBehaviour != null)
                tutorialLabelBehaviour.Disable();
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 레벨 완료 시 호출됩니다.
        // 현재 튜토리얼 로직에서는 추가적인 처리가 필요 없습니다.
        public override void OnLevelCompleted()
        {
            // 레벨 완료 시 동작 (필요하다면 추가)
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 레벨 실패 시 호출됩니다.
        // 현재 튜토리얼 로직에서는 추가적인 처리가 필요 없습니다.
        public override void OnLevelFailed()
        {
            // 레벨 실패 시 동작 (필요하다면 추가)
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 레벨 초기화 시 호출됩니다.
        // 튜토리얼의 초기화 상태를 설정하고, 튜토리얼 컨트롤러에 튜토리얼 활성화를 요청합니다.
        public override void OnLevelInitialised()
        {
            // 초기화 상태를 false로 설정합니다. (재초기화 대비)
            isInitialised = false;

            // 튜토리얼 컨트롤러에 현재 튜토리얼의 활성화를 요청합니다.
            TutorialController.ActivateTutorial(this);
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 레벨 로드 시 호출됩니다.
        // 현재 방의 출구 지점 트랜스폼을 가져오고, 튜토리얼이 초기화된 상태라면 튜토리얼 시작 메소드를 호출합니다.
        public override void OnLevelLoaded()
        {
            // 현재 방의 첫 번째 출구 지점 트랜스폼을 가져옵니다.
            // ActiveRoom은 게임의 레벨/룸 관리 시스템에 따라 정의될 것입니다.
            finishPointTransform = ActiveRoom.ExitPoints[0].transform;

            // 튜토리얼이 초기화된 상태라면 튜토리얼 시작 메소드를 호출합니다.
            if (isInitialised)
                StartTutorial();
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 레벨 플레이 시작 시 호출됩니다.
        // 튜토리얼이 완료되지 않았으면 레벨 출구 수동 활성화를 설정하고, 대상 적을 가져와
        // 내비게이션 화살표와 튜토리얼 레이블을 표시하며, 적 사망 이벤트 핸들러를 구독합니다.
        public override void OnLevelStarted()
        {
            // 튜토리얼이 이미 완료되었으면 시작 로직을 수행하지 않습니다.
            if (saveData.isFinished) return;

            // 레벨 출구가 자동으로 활성화되지 않도록 수동 활성화 모드를 설정합니다.
            LevelController.EnableManualExitActivation();

            // 현재 방의 첫 번째 적을 튜토리얼 대상으로 설정합니다.
            // ActiveRoom은 게임의 레벨/룸 관리 시스템에 따라 정의될 것입니다.
            enemyBehavior = ActiveRoom.Enemies[0];

            // 플레이어 위치에서 대상 적 위치까지 안내하는 내비게이션 화살표를 등록합니다.
            arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, enemyBehavior.transform.position);
            // 화살표가 대상 적을 따라다니도록 설정합니다.
            arrowCase.FixArrowToTarget(enemyBehavior.transform);

            // 대상 적 위치 위에 튜토리얼 메시지 레이블을 생성합니다.
            tutorialLabelBehaviour = TutorialController.CreateTutorialLabel("KILL THE ENEMY", enemyBehavior.transform, new Vector3(0, 3.0f, 0));

            // 게임 UI의 일시정지 버튼을 비활성화하여 플레이어가 튜토리얼 중 일시정지하지 못하도록 합니다.
            gameUI.PauseButton.gameObject.SetActive(false);

            // 적 사망 이벤트에 OnEnemyDied 메소드를 구독하여 적 사망 시 알림을 받습니다.
            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 레벨 언로드 시 호출됩니다.
        // 튜토리얼 리소스를 언로드합니다.
        public override void OnLevelUnloaded()
        {
            // 튜토리얼 리소스 언로드 메소드를 호출합니다.
            Unload();
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 플레이어가 방 진입 시 호출됩니다.
        // 현재 튜토리얼 로직에서는 추가적인 처리가 필요 없습니다.
        public override void OnRoomEntered()
        {
            // 방 진입 시 동작 (필요하다면 추가)
        }

        // LevelSpecialBehaviour의 오버라이드 메소드: 플레이어가 방 이탈 시 호출됩니다.
        // 현재 튜토리얼 로직에서는 추가적인 처리가 필요 없습니다.
        public override void OnRoomLeaved()
        {
            // 방 이탈 시 동작 (필요하다면 추가)
        }
    }
}