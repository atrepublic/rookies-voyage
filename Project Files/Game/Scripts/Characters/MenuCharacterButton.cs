/*
 * MenuCharacterButton.cs
 * ---------------------
 * 이 스크립트는 메인 메뉴 화면에서 '캐릭터' 메뉴(패널)로 이동하는 버튼의 동작을 정의합니다.
 * MenuPanelButton을 상속받아 기본적인 메뉴 버튼 기능을 활용하며,
 * 캐릭터 패널에 업그레이드 가능 알림 등이 있을 경우 버튼에 하이라이트 효과를 표시하는 로직과
 * 버튼 클릭 시 캐릭터 패널 UI를 표시하는 로직을 구현합니다.
 */

using JetBrains.Annotations;
using Watermelon; // Watermelon 프레임워크 네임스페이스
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 캐릭터 메뉴 버튼 클래스 정의, MenuPanelButton 상속
    // sealed 키워드는 이 클래스가 더 이상 상속될 수 없음을 나타냅니다.
    public sealed class MenuCharacterButton : MenuPanelButton
    {
        [Tooltip("캐릭터 패널 UI 컴포넌트 참조")]
        private UICharactersPanel characterPanel; // UICharactersPanel은 캐릭터 선택/업그레이드 화면 전체 UI

        /// <summary>
        /// 버튼 초기화 시 호출됩니다. (MenuPanelButton 오버라이드)
        /// </summary>
        public override void Init()
        {
            base.Init(); // 부모 클래스의 Init 호출

            // UI 컨트롤러를 통해 캐릭터 패널 UI 컴포넌트 가져오기
            characterPanel = UIController.GetPage<UICharactersPanel>();
        }

        /// <summary>
        /// 이 버튼에 하이라이트(예: 느낌표 아이콘) 표시가 필요한지 여부를 결정합니다. (MenuPanelButton 오버라이드)
        /// </summary>
        /// <returns>하이라이트가 필요하면 true</returns>
        protected override bool IsHighlightRequired()
        {
            // 캐릭터 패널에 구매 가능한 업그레이드나 새로운 캐릭터 등 확인 필요한 액션이 있는지 확인
            return characterPanel.IsAnyActionAvailable();
        }

        /// <summary>
        /// 버튼이 클릭되었을 때 호출되는 로직입니다. (MenuPanelButton 오버라이드)
        /// </summary>
        protected override void OnButtonClicked()
        {
            // 현재 활성화된 메인 메뉴 UI(UIMainMenu)를 숨기고,
            // 숨겨진 후 콜백 함수로 캐릭터 패널 UI(UICharactersPanel)를 표시합니다.
            UIController.HidePage<UIMainMenu>(() =>
            {
                UIController.ShowPage<UICharactersPanel>();
            });

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}