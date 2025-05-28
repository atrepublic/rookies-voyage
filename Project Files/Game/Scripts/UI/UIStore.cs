//====================================================================================================
// 해당 스크립트: UIStore.cs
// 기능: 게임 내 상점 UI를 관리하고 표시합니다.
// 용도: 플레이어가 게임 아이템을 구매하거나 무료 보상을 받을 수 있는 상점 패널을 제어하고,
//      아이템 목록 표시, 애니메이션, 버튼 상호작용 등을 처리합니다.
//====================================================================================================
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.IAPStore
{
    public class UIStore : UIPage
    {
        private const float DEFAULT_STORE_HEIGHT_OFFSET = 300; // 상점 콘텐츠 높이 계산 시 사용되는 기본 오프셋

        [BoxGroup("References", "References")]
        [Tooltip("노치 디자인 또는 안전 영역을 고려하여 UI를 배치할 RectTransform입니다.")]
        [SerializeField] private RectTransform safeAreaTransform;
        [BoxGroup("References")]
        [Tooltip("코인 수를 표시하는 CurrencyUIPanelSimple 컴포넌트입니다.")]
        [SerializeField] private CurrencyUIPanelSimple coinsUI;

        [BoxGroup("Scroll View", "Scroll View")]
        [Tooltip("상점 아이템 목록의 레이아웃을 관리하는 VerticalLayoutGroup 컴포넌트입니다.")]
        [SerializeField] private VerticalLayoutGroup layout;
        [BoxGroup("Scroll View")]
        [Tooltip("상점 아이템들이 배치될 스크롤 뷰의 콘텐츠 RectTransform입니다.")]
        [SerializeField] private RectTransform content;

        [BoxGroup("Buttons", "Buttons")]
        [Tooltip("상점 패널을 닫는 버튼입니다.")]
        [SerializeField] private Button closeButton;
        [BoxGroup("Buttons")]
        [Tooltip("타이머 보상(무료 코인)을 관리하는 TimerRewardsHolder 컴포넌트입니다.")]
        [SerializeField] private TimerRewardsHolder freeCoinsButton;

        private TweenCase[] appearTweenCases; // 상점 아이템 등장 애니메이션 트윈 케이스 배열
        private Transform[] offersTransforms; // 상점 아이템 오브젝트들의 Transform 배열

        /// <summary>
        /// 오브젝트가 생성될 때 호출되는 함수입니다.
        /// 상점 아이템 오브젝트들을 가져오고 닫기 버튼 이벤트 리스너를 추가합니다.
        /// </summary>
        private void Awake()
        {
            // 상점 아이템 오브젝트들의 Transform을 배열에 저장
            offersTransforms = new Transform[content.childCount];
            for (int i = 0; i < offersTransforms.Length; i++)
            {
                offersTransforms[i] = content.GetChild(i);
            }

            // 닫기 버튼 클릭 이벤트에 핸들러 함수 연결
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        /// <summary>
        /// UI 페이지 초기화 함수입니다.
        /// 안전 영역을 설정하고 코인 UI를 초기화합니다.
        /// </summary>
        public override void Init()
        {
            // 안전 영역(노치 디자인 등) 설정
            NotchSaveArea.RegisterRectTransform(safeAreaTransform);

            // 코인 UI 초기화
            coinsUI.Init();
        }

        /// <summary>
        /// 상점 패널이 숨겨지는 애니메이션을 실행하는 함수입니다.
        /// 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            // UI 컨트롤러에 페이지 닫힘 이벤트 알림
            UIController.OnPageClosed(this);
        }

        /// <summary>
        /// 상점 패널이 나타나는 애니메이션을 실행하는 함수입니다.
        /// 상점 아이템 및 닫기 버튼의 등장 애니메이션을 처리합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            // 기존 등장 애니메이션 트윈 케이스 중단
            appearTweenCases.KillActive();

            // 상점 콘텐츠의 초기 높이 계산
            float height = layout.padding.top + layout.padding.bottom + DEFAULT_STORE_HEIGHT_OFFSET;

            // 활성화된 상점 아이템 오브젝트만 필터링
            Transform[] activeOffers = offersTransforms.Where(x => x.gameObject.activeSelf).ToArray();
            // 등장 애니메이션 트윈 케이스 배열 초기화
            appearTweenCases = new TweenCase[activeOffers.Length];
            for (int i = 0; i < activeOffers.Length; i++)
            {
                RectTransform offerRectTransform = (RectTransform)activeOffers[i].transform;
                offerRectTransform.localScale = Vector3.zero; // 초기 스케일을 0으로 설정

                // 상점 아이템 스케일 애니메이션 시작 (딜레이 적용 및 CircOut 보간)
                appearTweenCases[i] = offerRectTransform.DOScale(1.0f, 0.3f, i * 0.05f).SetEasing(Ease.Type.CircOut);

                // 활성화된 아이템 높이를 전체 높이에 추가
                height += offerRectTransform.sizeDelta.y;
            }

            // 아이템 간 간격(spacing)을 전체 높이에 추가
            height += activeOffers.Length * layout.spacing;

            // 닫기 버튼 스케일 애니메이션 시작
            closeButton.transform.localScale = Vector3.zero; // 초기 스케일을 0으로 설정
            closeButton.transform.DOScale(1.0f, 0.3f, 0.2f).SetEasing(Ease.Type.BackOut);

            // 스크롤 뷰 콘텐츠의 크기 및 위치 설정
            content.sizeDelta = new Vector2(0, height); // 콘텐츠 높이 설정
            content.anchoredPosition = Vector2.zero; // 콘텐츠 위치 초기화

            // 마지막 아이템의 등장 애니메이션 완료 시 페이지 열림 이벤트 호출
            appearTweenCases[^1].OnComplete(() =>
            {
                UIController.OnPageOpened(this);
            });
        }

        /// <summary>
        /// 상점 패널을 숨기는 함수입니다.
        /// 상점 패널을 숨기고 메인 메뉴 패널을 표시합니다.
        /// </summary>
        public void Hide()
        {
            // 기존 등장 애니메이션 트윈 케이스 중단
            appearTweenCases.KillActive();

            // 상점 페이지 숨김 애니메이션 실행 및 완료 시 메인 메뉴 페이지 표시
            UIController.HidePage<UIStore>(() =>
            {
                UIController.ShowPage<UIMainMenu>();
            });
        }

        /// <summary>
        /// 닫기 버튼 클릭 시 호출되는 함수입니다.
        /// 햅틱 피드백, 사운드 재생 후 상점 패널을 숨기고 메인 메뉴 패널을 표시합니다.
        /// </summary>
        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            // 햅틱 피드백 재생 (MODULE_HAPTIC 정의 시)
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // 상점 페이지 숨김 애니메이션 실행 및 완료 시 메인 메뉴 페이지 표시
            UIController.HidePage<UIStore>(() =>
            {
                UIController.ShowPage<UIMainMenu>();
            });
        }

        /// <summary>
        /// 무료 코인을 받을 수 있는지 확인하는 함수입니다.
        /// </summary>
        /// <returns>무료 코인을 받을 수 있으면 true, 아니면 false를 반환합니다.</returns>
        public bool IsFreeCoinsAvailable()
        {
            // 무료 코인 버튼 오브젝트가 있고, 사용 가능한 상태인지 확인
            if (freeCoinsButton != null)
                return freeCoinsButton.IsAvailable();

            return false; // 무료 코인 버튼이 없으면 항상 false 반환
        }

        /// <summary>
        /// 통화(코인)를 생성하여 특정 위치에서 코인 UI로 날아가는 애니메이션을 실행하는 함수입니다.
        /// </summary>
        /// <param name="spawnRectTransform">통화가 생성될 시작 위치의 RectTransform</param>
        /// <param name="currencyType">생성될 통화의 타입</param>
        /// <param name="amount">생성될 통화의 개수</param>
        /// <param name="completeCallback">애니메이션 완료 시 호출될 콜백 함수</param>
        public void SpawnCurrencyCloud(RectTransform spawnRectTransform, CurrencyType currencyType, int amount, SimpleCallback completeCallback = null)
        {
            // FloatingCloud를 사용하여 통화(코인) 스폰 및 애니메이션 실행
            // currencyType.ToString()을 사용하여 통화 타입 문자열 전달
            FloatingCloud.SpawnCurrency(currencyType.ToString(), spawnRectTransform, coinsUI.RectTransform, amount, null, completeCallback);
        }
    }
}