//====================================================================================================
// 해당 스크립트: UIUpgradesAbstractPage.cs
// 기능: 무기 및 캐릭터와 같은 업그레이드 가능한 아이템의 UI 페이지에 대한 추상 클래스입니다.
// 용도: 업그레이드 아이템 목록을 스크롤하여 표시하고, 각 아이템 패널의 초기화 및 관리,
//      통화 변경에 따른 UI 업데이트, 게임패드 입력 처리를 위한 공통 기능을 제공합니다.
//====================================================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // T: UIUpgradeAbstractPanel을 상속하는 패널 타입 (예: WeaponPanelUI, CharacterPanelUI)
    // K: 패널이 나타내는 데이터 타입 (예: WeaponData, CharacterData)
    public abstract class UIUpgradesAbstractPage<T, K> : UIPage where T : UIUpgradeAbstractPanel
    {
        // 스크롤 뷰에서 아이템 패널 위치 계산에 사용되는 상수
        protected const float SCROLL_SIDE_OFFSET = 50; // 스크롤 뷰 양옆의 오프셋
        protected const float SCROLL_ELEMENT_WIDTH = 415f; // 개별 아이템 패널의 예상 너비

        [Tooltip("노치 디자인 등 안전 영역을 고려하여 UI를 배치할 RectTransform입니다.")]
        [SerializeField] protected RectTransform safeAreaRectTransform;

        [Space]
        [Tooltip("이전 페이지로 돌아가는 버튼입니다.")]
        [SerializeField] protected Button backButton;

        [Space]
        [Tooltip("개별 업그레이드 아이템 패널 UI 프리팹입니다.")]
        [SerializeField] protected GameObject panelUIPrefab;
        [Tooltip("아이템 패널들이 배치될 컨테이너 Transform입니다.")]
        [SerializeField] protected Transform panelsContainer;

        [Space]
        [Tooltip("업그레이드 페이지의 배경 패널 RectTransform입니다.")]
        [SerializeField] protected RectTransform backgroundPanelRectTransform;
        [Tooltip("닫기 버튼의 RectTransform입니다.")]
        [SerializeField] protected RectTransform closeButtonRectTransform;
        [Tooltip("아이템 패널 목록을 스크롤하는 ScrollRect 컴포넌트입니다.")]
        [SerializeField] protected ScrollRect scrollView;

        [Space]
        [Tooltip("선택되지 않은 아이템 패널의 등장 애니메이션에 사용되는 AnimationCurve입니다.")]
        [SerializeField] protected AnimationCurve panelScaleAnimationCurve;
        [Tooltip("선택된 아이템 패널의 등장 애니메이션에 사용되는 AnimationCurve입니다.")]
        [SerializeField] protected AnimationCurve selectedPanelScaleAnimationCurve;

        protected UIGamepadButton gamepadCloseButton; // 닫기 버튼의 게임패드 버튼 컴포넌트
        /// <summary>
        /// 닫기 버튼의 UIGamepadButton 컴포넌트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public UIGamepadButton GamepadCloseButton => gamepadCloseButton;

        protected List<T> itemPanels = new List<T>(); // 생성된 아이템 패널 목록

        /// <summary>
        /// 이 페이지에서 사용되는 게임패드 버튼 태그를 활성화하는 추상 함수입니다.
        /// 하위 클래스에서 구현해야 합니다.
        /// </summary>
        protected abstract void EnableGamepadButtonTag();

        /// <summary>
        /// UI 페이지 초기화 함수입니다.
        /// 닫기 버튼의 게임패드 컴포넌트를 가져오고 클릭 이벤트를 설정하며,
        /// 아이템 패널 목록을 초기화하고 안전 영역 및 태블릿 환경 설정을 처리합니다.
        /// </summary>
        public override void Init()
        {
            // 닫기 버튼의 게임패드 버튼 컴포넌트 가져오기
            gamepadCloseButton = backButton.GetComponent<UIGamepadButton>();

            // 닫기 버튼 클릭 이벤트에 핸들러 함수 연결
            backButton.onClick.AddListener(BackButton);

            itemPanels = new List<T>(); // 아이템 패널 목록 초기화

            // 태블릿 환경인 경우 배경 패널 크기 조절
            if (UIController.IsTablet)
            {
                var scrollSize = backgroundPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                backgroundPanelRectTransform.sizeDelta = scrollSize;
            }

            // 안전 영역(노치 디자인 등) 설정
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            //Debug.Log("부모 Init  실행 ");
        }

        /// <summary>
        /// 새로운 아이템 패널 UI를 생성하고 초기화하는 함수입니다.
        /// 프리팹을 인스턴스화하고 컨테이너에 배치하며, 목록에 추가합니다.
        /// </summary>
        /// <returns>생성된 새로운 아이템 패널 UI 객체</returns>
        protected T AddNewPanel()
        {
            var newPanelObject = Instantiate(panelUIPrefab); // 패널 UI 프리팹 인스턴스화
            newPanelObject.transform.SetParent(panelsContainer); // 컨테이너를 부모로 설정
            newPanelObject.transform.ResetLocal(); // 로컬 위치, 회전, 스케일 리셋

            var newPanel = newPanelObject.GetComponent<T>(); // 해당 타입의 컴포넌트 가져오기

            itemPanels.Add(newPanel); // 아이템 패널 목록에 추가

            return newPanel; // 생성된 패널 반환
        }

        /// <summary>
        /// 매 프레임 호출되는 업데이트 함수입니다.
        /// 페이지가 활성화 상태일 때 게임패드 입력에 따른 아이템 선택 및 스크롤을 처리합니다.
        /// </summary>
        protected virtual void Update()
        {
            // 캔버스가 활성화되어 있지 않으면 업데이트하지 않음
            if (!Canvas.enabled) return;

            T newSelectedPanel = null; // 새로 선택될 패널 임시 저장 변수

            // 게임패드 D-Pad 왼쪽 버튼 입력 처리
            if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DLeft))
            {
                // 현재 선택된 아이템의 왼쪽 아이템 선택 시도
                for (int i = 0; i < itemPanels.Count; i++)
                {
                    // 현재 선택된 아이템이고, 왼쪽으로 이동 가능하며, 이동할 아이템이 잠금 해제된 상태인 경우
                    if (SelectedIndex == i && i > 0 && itemPanels[i - 1].IsUnlocked)
                    {
                        itemPanels[i - 1].Select(); // 왼쪽 아이템 선택
                        newSelectedPanel = itemPanels[i - 1]; // 새로 선택된 패널 설정
                        break; // 반복문 종료
                    }
                }
            }
            // 게임패드 D-Pad 오른쪽 버튼 입력 처리
            else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DRight))
            {
                // 현재 선택된 아이템의 오른쪽 아이템 선택 시도
                for (int i = 0; i < itemPanels.Count; i++)
                {
                    // 현재 선택된 아이템이고, 오른쪽으로 이동 가능하며, 이동할 아이템이 잠금 해제된 상태인 경우
                    if (SelectedIndex == i && i < itemPanels.Count - 1 && itemPanels[i + 1].IsUnlocked)
                    {
                        itemPanels[i + 1].Select(); // 오른쪽 아이템 선택
                        newSelectedPanel = itemPanels[i + 1]; // 새로 선택된 패널 설정
                        break; // 반복문 종료
                    }
                }
            }

            // 새로 선택된 패널이 있으면 해당 패널이 보이도록 스크롤 뷰 위치 조정
            if (newSelectedPanel != null)
            {
                // 새로 선택된 패널이 보이도록 스크롤 뷰 콘텐츠의 X축 위치 계산 및 클램프
                float scrollOffsetX = Mathf.Clamp(-(newSelectedPanel.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET), -scrollView.content.sizeDelta.x, 0);
                scrollView.content.anchoredPosition = new Vector2(scrollOffsetX, 0); // 스크롤 뷰 위치 설정
                scrollView.StopMovement(); // 스크롤 뷰 움직임 중지
            }
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 통화 변경 이벤트를 구독하고, 배경 패널 등장 애니메이션, 스크롤 뷰 위치 설정,
        /// 아이템 패널 스케일 애니메이션을 처리하며, 완료 시 게임패드 태그 활성화 및 페이지 열림 이벤트를 호출합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            // 통화 변경 이벤트 구독
            for (int i = 0; i < CurrencyController.Currencies.Length; i++)
            {
                CurrencyController.Currencies[i].OnCurrencyChanged += OnCurrencyAmountChanged;
            }

            // 배경 패널 등장 애니메이션 (아래에서 위로 이동, BackOutLight 커스텀 보간)
            backgroundPanelRectTransform.anchoredPosition = new Vector2(0, -1500); // 초기 위치 설정 (화면 아래)
            backgroundPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight")); // 목표 위치 (화면 중앙)

            // 현재 선택된 아이템이 보이도록 스크롤 뷰 초기 위치 설정
            float scrollOffsetX = -(itemPanels[SelectedIndex].RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);
            scrollView.content.anchoredPosition = new Vector2(scrollOffsetX, 0); // 스크롤 뷰 위치 설정
            scrollView.StopMovement(); // 스크롤 뷰 움직임 중지

            // 각 아이템 패널의 스케일 등장 애니메이션 실행
            for (int i = 0; i < itemPanels.Count; i++)
            {
                RectTransform panelTransform = itemPanels[i].RectTransform; // 패널의 RectTransform 가져오기

                panelTransform.localScale = Vector2.zero; // 초기 스케일을 0으로 설정

                // 선택된 패널과 나머지 패널의 스케일 애니메이션 방식 및 딜레이 다르게 적용
                if (i == SelectedIndex)
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.2f).SetCurveEasing(selectedPanelScaleAnimationCurve); // 선택된 패널 애니메이션 (지정된 커브 사용)
                }
                else
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.3f).SetCurveEasing(panelScaleAnimationCurve); // 나머지 패널 애니메이션 (지정된 커브 사용)
                }

                itemPanels[i].OnPanelOpened(); // 각 패널의 열림 이벤트 호출
            }

            // 딜레이 후 게임패드 버튼 태그 활성화 및 페이지 열림 이벤트 호출
            Tween.DelayedCall(0.9f, () => {
                EnableGamepadButtonTag(); // 하위 클래스에서 정의된 게임패드 버튼 태그 활성화
                UIController.OnPageOpened(this); // UI 컨트롤러에 페이지 열림 이벤트 알림
            });
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다. (현재는 통화 변경 이벤트 구독 해제만 처리)
        /// </summary>
        public override void PlayHideAnimation()
        {
            // 통화 변경 이벤트 구독 해제
            for (int i = 0; i < CurrencyController.Currencies.Length; i++)
            {
                CurrencyController.Currencies[i].OnCurrencyChanged -= OnCurrencyAmountChanged;
            }
        }

        /// <summary>
        /// 통화(돈)의 개수가 변경되었을 때 호출되는 함수입니다.
        /// 모든 아이템 패널의 돈 관련 UI를 업데이트합니다.
        /// </summary>
        /// <param name="currency">변경된 통화 정보</param>
        /// <param name="difference">변경량</param>
        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            // 모든 아이템 패널의 돈 관련 UI 업데이트 함수 호출
            for (int i = 0; i < itemPanels.Count; i++)
            {
                itemPanels[i].OnMoneyAmountChanged();
            }
        }

        /// <summary>
        /// 이 페이지를 숨기는 추상 함수입니다.
        /// 하위 클래스에서 UIController를 사용하여 페이지를 숨기는 로직을 구현해야 합니다.
        /// </summary>
        /// <param name="onFinish">숨김 애니메이션 완료 시 호출될 콜백 함수</param>
        protected abstract void HidePage(SimpleCallback onFinish);
        /// <summary>
        /// 특정 데이터에 해당하는 아이템 패널을 가져오는 추상 함수입니다.
        /// 하위 클래스에서 해당 데이터 타입에 맞는 패널을 찾는 로직을 구현해야 합니다.
        /// </summary>
        /// <param name="type">찾으려는 아이템 데이터</param>
        /// <returns>해당 데이터와 일치하는 아이템 패널 객체, 없으면 null 반환</returns>
        public abstract T GetPanel(K type);
        /// <summary>
        /// 현재 선택된 아이템의 인덱스를 가져오는 추상 프로퍼티입니다.
        /// 하위 클래스에서 현재 선택된 아이템의 인덱스를 반환하는 로직을 구현해야 합니다.
        /// </summary>
        protected abstract int SelectedIndex { get; }

        #region Buttons

        /// <summary>
        /// '뒤로 가기' 버튼 클릭 시 호출되는 함수입니다.
        /// 이 페이지를 숨기고 메인 메뉴 페이지를 표시합니다.
        /// </summary>
        public void BackButton()
        {
            // 이 페이지를 숨기고 완료 시 메인 메뉴 페이지 표시
            HidePage(UIController.ShowPage<UIMainMenu>);

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        #endregion
    }
}