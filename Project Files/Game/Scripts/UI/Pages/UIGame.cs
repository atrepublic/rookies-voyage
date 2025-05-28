//====================================================================================================
// 해당 스크립트: UIGame.cs
// 기능: 게임 플레이 중의 UI (조이스틱, 공격 버튼, 레벨 정보, 일시 정지 메뉴 등)를 관리합니다.
// 용도: 게임 플레이에 필요한 사용자 입력을 처리하고, 현재 레벨 및 진행 상황 정보를 표시하며,
//      게임 일시 정지 및 재개/종료 기능을 제공합니다.
//====================================================================================================
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [Tooltip("노치 디자인 등 안전 영역을 고려하여 UI를 배치할 RectTransform입니다.")]
        [SerializeField] private RectTransform safeAreaRectTransform;
        [Tooltip("플레이어 이동 입력을 처리하는 조이스틱 컴포넌트입니다.")]
        [SerializeField] private Joystick joystick;
        [Tooltip("플레이어 공격 입력을 처리하는 공격 버튼 컴포넌트입니다.")]
        [SerializeField] private AttackButtonBehavior attackButton;
        [Tooltip("떠다니는 텍스트 (예: 획득 코인)가 표시될 컨테이너 RectTransform입니다.")]
        [SerializeField] private RectTransform floatingTextHolder;

        [Space]
        [Tooltip("현재 레벨/구역 정보를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI areaText;

        [Space]
        [Tooltip("방 진행 상황을 나타내는 UI 인디케이터들이 배치될 컨테이너 Transform입니다.")]
        [SerializeField] private Transform roomsHolder;
        [Tooltip("방 진행 상황을 나타내는 UI 인디케이터 프리팹입니다.")]
        [SerializeField] private GameObject roomIndicatorUIPrefab;

        [Space]
        [Tooltip("화면 페이드 인/아웃 애니메이션에 사용되는 Image 컴포넌트입니다.")]
        [SerializeField] private Image fadeImage;
        [Tooltip("현재 보유 코인 수를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] private TextMeshProUGUI coinsText;

        [Header("Pause Panel")]
        [Tooltip("게임 일시 정지 버튼입니다.")]
        [SerializeField] private Button pauseButton;
        /// <summary>
        /// 게임 일시 정지 버튼에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public Button PauseButton => pauseButton;

        [Space]
        [Tooltip("게임 일시 정지 패널 게임 오브젝트입니다.")]
        [SerializeField] private GameObject pausePanelObject;
        [Tooltip("게임 일시 정지 패널의 투명도를 조절하는 CanvasGroup 컴포넌트입니다.")]
        [SerializeField] private CanvasGroup pausePanelCanvasGroup;
        [Tooltip("일시 정지된 게임을 재개하는 버튼입니다.")]
        [SerializeField] private Button pauseResumeButton;
        [Tooltip("일시 정지된 게임을 종료하는 버튼입니다.")]
        [SerializeField] private Button pauseExitButton;

        /// <summary>
        /// 플레이어 이동 입력을 처리하는 조이스틱 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public Joystick Joystick => joystick;
        /// <summary>
        /// 떠다니는 텍스트 (예: 획득 코인)가 표시될 컨테이너 RectTransform에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public RectTransform FloatingTextHolder => floatingTextHolder;

        private List<UIRoomIndicator> roomIndicators = new List<UIRoomIndicator>(); // 방 진행 상황 UI 인디케이터 목록

        /// <summary>
        /// 오브젝트가 시작될 때 호출되는 함수입니다.
        /// 게임 설정에 따라 공격 버튼 활성화 여부를 설정합니다.
        /// </summary>
        private void Start()
        {
            // 게임 설정에 따라 공격 버튼 활성화 여부 설정
            attackButton.gameObject.SetActive(GameSettings.GetSettings().UseAttackButton);
        }

        /// <summary>
        /// 화면 페이드 애니메이션을 실행하는 함수입니다.
        /// 지정된 시간, 시작/종료 투명도, 보간 방식, 완료 시 콜백 함수를 사용하여 페이드 애니메이션을 제어합니다.
        /// </summary>
        /// <param name="time">애니메이션 지속 시간</param>
        /// <param name="startAlpha">시작 투명도 (0 ~ 1)</param>
        /// <param name="targetAlpha">목표 투명도 (0 ~ 1)</param>
        /// <param name="easing">애니메이션 보간 방식</param>
        /// <param name="callback">애니메이션 완료 시 호출될 콜백 함수</param>
        /// <param name="disableOnComplete">true이면 애니메이션 완료 후 페이드 이미지 게임 오브젝트 비활성화</param>
        public void FadeAnimation(float time, float startAlpha, float targetAlpha, Ease.Type easing, SimpleCallback callback, bool disableOnComplete = false)
        {
            fadeImage.gameObject.SetActive(true); // 페이드 이미지 게임 오브젝트 활성화
            fadeImage.color = fadeImage.color.SetAlpha(startAlpha); // 시작 투명도 설정
            // 페이드 애니메이션 시작
            fadeImage.DOFade(targetAlpha, time).SetEasing(easing).OnComplete(delegate
            {
                callback?.Invoke(); // 콜백 함수 호출

                // 애니메이션 완료 후 비활성화 옵션 처리
                if (disableOnComplete)
                    fadeImage.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// UI 페이지 초기화 함수입니다.
        /// 일시 정지 관련 버튼 이벤트 리스너를 추가하고 조이스틱 및 안전 영역을 초기화합니다.
        /// </summary>
        public override void Init()
        {
            // 일시 정지 관련 버튼 클릭 이벤트에 핸들러 함수 연결
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            pauseExitButton.onClick.AddListener(OnPauseExitButtonClicked);
            pauseResumeButton.onClick.AddListener(OnPauseResumeButtonClicked);

            // 조이스틱 초기화
            joystick.Init(UIController.MainCanvas);

            // 안전 영역(노치 디자인 등) 설정
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            // UI 컨트롤러에 페이지 닫힘 이벤트 알림
            UIController.OnPageClosed(this);
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 일시 정지 버튼을 활성화하고 페이지 열림 이벤트를 호출하며, 게임패드 버튼 태그를 설정합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            pauseButton.gameObject.SetActive(true); // 일시 정지 버튼 활성화

            UIController.OnPageOpened(this); // UI 컨트롤러에 페이지 열림 이벤트 알림

            // 딜레이 후 게임 관련 게임패드 버튼 태그 활성화 및 메인 메뉴 관련 태그 비활성화
            Tween.DelayedCall(0.3f, () =>
            {
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
                UIGamepadButton.DisableTag(UIGamepadButtonTag.MainMenu);
            });
        }

        /// <summary>
        /// 방 진행 상황 UI를 초기화하는 함수입니다.
        /// 각 방에 대한 UI 인디케이터를 생성하고 초기 상태를 설정합니다.
        /// </summary>
        /// <param name="rooms">레벨의 방 데이터 배열</param>
        public void InitRoomsUI(RoomData[] rooms)
        {
            roomIndicators.Clear(); // 기존 방 인디케이터 목록 초기화

            // 각 방 데이터에 대해 UI 인디케이터 생성 및 초기화
            for (int i = 0; i < rooms.Length; i++)
            {
                GameObject indicatorObject = Instantiate(roomIndicatorUIPrefab); // 인디케이터 프리팹 인스턴스화
                indicatorObject.transform.SetParent(roomsHolder); // 방 컨테이너를 부모로 설정

                UIRoomIndicator roomIndicator = indicatorObject.GetComponent<UIRoomIndicator>(); // UIRoomIndicator 컴포넌트 가져오기
                roomIndicators.Add(roomIndicator); // 목록에 추가

                roomIndicators[i].Init(); // 인디케이터 초기화

                // 첫 번째 방은 도달 상태로 설정
                if (i == 0)
                    roomIndicators[i].SetAsReached();
            }

            // 현재 레벨/구역 정보 텍스트 업데이트
            areaText.text = string.Format(LevelController.AREA_TEXT, ActiveRoom.CurrentWorldIndex + 1, ActiveRoom.CurrentLevelIndex + 1);
        }

        /// <summary>
        /// 도달한 방에 해당하는 UI 인디케이터 상태를 업데이트하는 함수입니다.
        /// </summary>
        /// <param name="roomReachedIndex">도달한 방의 인덱스</param>
        public void UpdateReachedRoomUI(int roomReachedIndex)
        {
            // 도달한 방 인덱스에 해당하는 인디케이터를 도달 상태로 설정 (인덱스 범위를 목록 크기로 나눈 나머지 사용)
            roomIndicators[roomReachedIndex % roomIndicators.Count].SetAsReached();
        }

        /// <summary>
        /// 현재 보유 코인 수를 표시하는 텍스트를 업데이트하는 함수입니다.
        /// </summary>
        /// <param name="newAmount">새로운 코인 개수</param>
        public void UpdateCoinsText(int newAmount)
        {
            // 코인 텍스트 업데이트 (CurrencyHelper를 사용하여 형식 지정)
            coinsText.text = CurrencyHelper.Format(newAmount);
        }

        #region Pause
        /// <summary>
        /// 일시 정지 패널의 '재개' 버튼 클릭 시 호출되는 함수입니다.
        /// 게임 시간을 정상화하고 일시 정지 패널을 숨깁니다.
        /// </summary>
        private void OnPauseResumeButtonClicked()
        {
            // 게임이 활성 상태가 아니면 리턴
            if (!GameController.IsGameActive)
                return;

            Time.timeScale = 1.0f; // 게임 시간을 정상화

            // 일시 정지 패널 페이드 아웃 애니메이션 (비확장 시간 사용) 및 완료 시 비활성화
            pausePanelCanvasGroup.alpha = 0.0f;
            pausePanelCanvasGroup.DOFade(0.0f, 0.3f, unscaledTime: true).OnComplete(() =>
            {
                pausePanelObject.SetActive(false); // 패널 게임 오브젝트 비활성화
            });
        }

        /// <summary>
        /// 일시 정지 패널의 '종료' 버튼 클릭 시 호출되는 함수입니다.
        /// 오버레이를 표시하고 게임을 종료 처리합니다.
        /// </summary>
        private void OnPauseExitButtonClicked()
        {
            // 오버레이 표시
            Overlay.Show(0.3f, () =>
            {
                Time.timeScale = 1.0f; // 게임 시간 정상화

                LevelController.UnloadLevel(); // 레벨 언로드

                GameController.OnLevelExit(); // 게임 컨트롤러에 레벨 종료 이벤트 알림

                Overlay.Hide(0.3f, null); // 오버레이 숨김
            });
        }

        /// <summary>
        /// 게임 일시 정지 버튼 클릭 시 호출되는 함수입니다.
        /// 게임 시간을 멈추고 일시 정지 패널을 표시합니다.
        /// </summary>
        private void OnPauseButtonClicked()
        {
            Time.timeScale = 0.0f; // 게임 시간 중지

            pausePanelObject.SetActive(true); // 일시 정지 패널 게임 오브젝트 활성화
            pausePanelCanvasGroup.alpha = 0.0f; // 초기 투명도 0으로 설정
            pausePanelCanvasGroup.DOFade(1.0f, 0.3f, unscaledTime: true); // 일시 정지 패널 페이드 인 애니메이션 (비확장 시간 사용)
        }
        #endregion
    }
}