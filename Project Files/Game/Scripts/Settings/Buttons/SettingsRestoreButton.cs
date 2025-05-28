using UnityEngine.EventSystems;

namespace Watermelon
{
    /// <summary>
    ///   설정 복원 버튼 스크립트.
    ///   인앱 구매 복원 기능을 처리합니다.
    /// </summary>
    public class SettingsRestoreButton : SettingsButtonBase
    {
        /// <summary>
        ///   초기화 함수.
        ///   Monetization 모듈 활성화 여부에 따라 게임 오브젝트를 활성화/비활성화합니다.
        /// </summary>
        public override void Init()
        {
#if MODULE_MONETIZATION
            gameObject.SetActive(Monetization.IsActive);
#else
            gameObject.SetActive(false);
#endif
        }

        /// <summary>
        ///   버튼 클릭 시 호출되는 함수.
        ///   인앱 구매를 복원하고, 버튼 클릭 사운드를 재생합니다.
        /// </summary>
        public override void OnClick()
        {
#if MODULE_MONETIZATION
            IAPManager.RestorePurchases();
#endif

            // 버튼 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        /// <summary>
        ///   버튼 선택 시 호출되는 함수.
        ///   버튼을 선택 상태로 변경하고, 현재 선택된 게임 오브젝트를 설정합니다.
        /// </summary>
        public override void Select()
        {
            IsSelected = true;

            Button.Select();

            // 이전 선택을 지우고 현재 버튼을 선택
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(Button.gameObject, new BaseEventData(EventSystem.current));
        }

        /// <summary>
        ///   버튼 선택 해제 시 호출되는 함수.
        ///   버튼을 비선택 상태로 변경합니다.
        /// </summary>
        public override void Deselect()
        {
            IsSelected = false;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}