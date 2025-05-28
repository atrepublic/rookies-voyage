using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.LevelSystem;  // GameSettings 사용

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// UIPetsPage v1.02
    /// 로비의 펫 목록 페이지를 관리합니다.
    /// - PetPanelUI 인스턴스 생성 & 초기화
    /// - 뒤로가기 버튼 처리
    /// - 게임패드 탭 태그 활성화/비활성화
    /// </summary>
    public class UIPetsPage : UIUpgradesAbstractPage<PetPanelUI, UC_PetData>
    {
        // (상위 클래스에 선언된 필드이므로 중복 제거)
        // protected RectTransform safeAreaRectTransform;
        // protected Button backButton;
        // protected GameObject panelUIPrefab;
        // protected Transform panelsContainer;

        private UC_PetDatabase petDatabase;

        /// <summary>현재 선택된 패널 인덱스</summary>
        protected override int SelectedIndex => petDatabase.GetPetIndexByID(
            SaveController.GetSaveObject<UC_PetGlobalSave>("pet_global").SelectedPetID
        );

        /// <summary>페이지 초기화: 펫 데이터 로드 → 패널 생성 → 버튼 바인딩</summary>
        public override void Init()
        {
            // 1) 상위 클래스 초기화: 버튼 바인딩·안전영역 등록 등
            base.Init();

            petDatabase = GameSettings.GetSettings().PetDatabase;

            // 모든 펫 데이터로 PetPanelUI 생성
            foreach (var pet in petDatabase.GetAllPets())
            {
                var go    = Instantiate(panelUIPrefab, panelsContainer);
                var panel = go.GetComponent<PetPanelUI>();
                panel.Init(pet, this);
                itemPanels.Add(panel);
            }

            // 뒤로가기 버튼 클릭 시 메인메뉴로
            backButton.onClick.AddListener(() =>
                UIController.HidePage<UIPetsPage>(() =>
                    UIController.ShowPage<UIMainMenu>())
            );

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        /// <summary>Unlock/Upgrade 후 전체 패널 갱신</summary>
        public void RefreshPanels()
        {
            foreach (var panel in itemPanels)
                panel.RedrawUI();  // 이제 public
        }

        /// <summary>메인 메뉴 ‘펫’ 탭 하이라이트 여부</summary>
        public bool IsAnyActionAvailable()
        {
            return itemPanels.Any(panel =>
                !panel.IsUnlocked
                || (panel.GetLevel() < panel.Data.upgrades.Count
                    && CurrencyController.HasAmount(
                        CurrencyType.Coins,
                        panel.Data.upgrades[panel.GetLevel()].cost
                    ))
            );
        }

        /// <summary>특정 펫 데이터에 대응하는 PetPanelUI 반환</summary>
        public override PetPanelUI GetPanel(UC_PetData data)
        {
            return itemPanels.FirstOrDefault(p => p.Data == data);
        }

        // ===================================================================
        // ▶ UIUpgradesAbstractPage가 요구하는 추상 메서드 구현
        // ===================================================================

        /// <summary>게임패드 탭 ‘펫’ 태그 활성화</summary>
        protected override void EnableGamepadButtonTag()
        {
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Pets);
        }

        /// <summary>페이지가 닫힐 때 호출 (onFinish 콜백 포함)</summary>
        protected override void HidePage(SimpleCallback onFinish)
        {
            UIController.HidePage<UIPetsPage>(onFinish);
        }

        public override void PlayShowAnimation()
        {
            // 기본 등장 애니메이션 실행
            base.PlayShowAnimation();
        }

        public override void PlayHideAnimation()
        {
            // UI 업그레이드 페이지 기본 해제 로직(이벤트 언구독)
            base.PlayHideAnimation();
            // 페이지 닫힘 콜백 호출 → DisableCanvas()
            UIController.OnPageClosed(this);
        }
    }
}
