/*
📄 SettingsHapticToggleButton.cs 요약 정리
이 스크립트는 SettingsButtonBase를 상속받아 햅틱(진동) 피드백 설정을 켜고 끄는 토글 버튼 기능을 구현합니다.

⭐ 주요 기능
- 햅틱 설정 상태(활성/비활성)에 따라 버튼의 스프라이트를 변경하여 시각적으로 표시합니다.
- 사용자가 버튼을 클릭하면 햅틱 설정을 토글하고, 관련 사운드를 재생합니다.
- 전역 햅틱 설정(Haptic.StateChanged 이벤트)의 변경을 감지하여 버튼의 상태를 자동으로 업데이트합니다.
- 게임패드 등으로 버튼이 선택되었을 때, 선택 강조 이미지를 페이드 인/아웃 효과와 함께 표시합니다.

🛠️ 사용 용도
- 게임 설정 UI에서 사용자가 햅틱 피드백 사용 여부를 직접 제어할 수 있도록 하는 토글 버튼으로 사용됩니다.
*/

#pragma warning disable 0649 // 할당되지 않은 필드에 대한 경고를 비활성화합니다. (SerializeField로 Unity 에디터에서 할당됨)

using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요합니다.
// using DG.Tweening; // DOTween 사용 시 필요할 수 있습니다. (예: DOFade, TweenCase)

namespace Watermelon
{
    /// <summary>
    /// 햅틱(진동) 피드백 설정을 켜고 끄는 토글 버튼 클래스입니다.
    /// SettingsButtonBase를 상속받습니다.
    /// </summary>
    public class SettingsHapticToggleButton : SettingsButtonBase
    {
        [Tooltip("햅틱 활성/비활성 상태를 표시하는 주 이미지 UI 요소입니다.")]
        [SerializeField] Image imageRef;
        [Tooltip("버튼이 게임패드 등으로 선택되었을 때 표시되는 선택 강조 이미지 UI 요소입니다.")]
        [SerializeField] Image selectionImage;

        [Space] // Unity 에디터 인스펙터에서 시각적 간격을 추가합니다.
        [Tooltip("햅틱 기능이 활성화되었을 때 표시할 스프라이트입니다.")]
        [SerializeField] Sprite activeSprite;
        [Tooltip("햅틱 기능이 비활성화되었을 때 표시할 스프라이트입니다.")]
        [SerializeField] Sprite disableSprite;

        // 현재 햅틱 설정이 활성화되어 있는지 여부를 나타냅니다. Haptic.IsActive와 동기화됩니다.
        private bool isActive = true;

        // 선택 강조 이미지의 페이드 애니메이션을 제어하기 위한 TweenCase 객체입니다. (DOTween 사용 가정)
        private TweenCase selectionFadeCase;

        /// <summary>
        /// SettingsButtonBase로부터 상속받은 초기화 메서드입니다.
        /// 이 클래스에서는 특별한 초기화 로직이 필요하지 않아 비워둡니다.
        /// </summary>
        public override void Init()
        {
            // 이 버튼 타입에 대한 특정 초기화 로직이 있다면 여기에 작성합니다.
        }

        /// <summary>
        /// Unity 생명주기 메서드: 이 컴포넌트(또는 게임 오브젝트)가 활성화될 때 호출됩니다.
        /// 현재 햅틱 상태를 가져와 UI를 갱신하고, 햅틱 상태 변경 이벤트에 구독합니다.
        /// </summary>
        private void OnEnable()
        {
            // Haptic 클래스에서 현재 햅틱 활성화 상태를 가져옵니다. (Haptic은 정적 클래스 또는 싱글톤으로 가정)
            isActive = Haptic.IsActive;

            // 현재 상태에 맞게 버튼의 스프라이트를 갱신합니다.
            Redraw();

            // 햅틱 상태가 변경될 때마다 OnStateChanged 메서드가 호출되도록 이벤트를 구독합니다.
            Haptic.StateChanged += OnStateChanged;
        }

        /// <summary>
        /// Unity 생명주기 메서드: 이 컴포넌트(또는 게임 오브젝트)가 비활성화될 때 호출됩니다.
        /// 햅틱 상태 변경 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        /// </summary>
        private void OnDisable()
        {
            // OnEnable에서 구독했던 햅틱 상태 변경 이벤트를 해제합니다.
            Haptic.StateChanged -= OnStateChanged;
        }

        /// <summary>
        /// 전역 햅틱 설정(Haptic.StateChanged)이 변경되었을 때 호출되는 콜백 메서드입니다.
        /// </summary>
        /// <param name="value">새로운 햅틱 활성화 상태입니다 (true: 활성, false: 비활성).</param>
        private void OnStateChanged(bool value)
        {
            // 내부 isActive 상태를 새로운 값으로 업데이트합니다.
            isActive = value;
            // 변경된 상태에 따라 버튼의 스프라이트를 다시 그립니다.
            Redraw();
        }

        /// <summary>
        /// 현재 햅틱 활성화 상태(isActive)에 따라 버튼의 이미지를 적절한 스프라이트로 변경합니다.
        /// </summary>
        private void Redraw()
        {
            // isActive 상태에 따라 imageRef의 스프라이트를 activeSprite 또는 disableSprite로 설정합니다.
            imageRef.sprite = isActive ? activeSprite : disableSprite;
        }

        /// <summary>
        /// 버튼이 클릭되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 구현)
        /// 현재 햅틱 설정을 반전시키고 버튼 클릭 사운드를 재생합니다.
        /// </summary>
        public override void OnClick()
        {
            // Haptic.IsActive 상태를 현재 isActive의 반대 값으로 설정하여 토글합니다.
            Haptic.IsActive = !isActive;

            // AudioController를 사용하여 버튼 클릭 사운드를 재생합니다.
            // AudioController와 AudioClips.buttonSound는 프로젝트의 사운드 시스템 및 오디오 클립으로 가정합니다.
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        /// <summary>
        /// 이 버튼이 선택되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 재정의)
        /// 선택 상태를 업데이트하고, 선택 강조 이미지를 페이드 인 효과로 표시합니다.
        /// </summary>
        public override void Select()
        {
            base.IsSelected = true; // IsSelected를 직접 설정 (원본 코드에서는 base.Select()가 아님)

            // 진행 중인 선택 이미지 페이드 애니메이션이 있다면 중지시킵니다. (KillActive는 DOTween 확장 메서드로 가정)
            selectionFadeCase.KillActive();

            // 선택 강조 이미지를 활성화하고, 알파값을 0으로 설정한 후, 0.2초 동안 목표 알파값(0.2f)까지 페이드 인 애니메이션을 실행합니다.
            // color.SetAlpha 및 DOFade는 확장 메서드 또는 DOTween 기능으로 가정합니다.
            selectionImage.gameObject.SetActive(true);
            selectionImage.color = selectionImage.color.SetAlpha(0.0f);
            selectionFadeCase = selectionImage.DOFade(0.2f, 0.2f);
        }

        /// <summary>
        /// 이 버튼이 선택 해제되었을 때 호출되는 메서드입니다. (SettingsButtonBase에서 상속 및 재정의)
        /// 선택 상태를 업데이트하고, 선택 강조 이미지를 즉시 숨깁니다.
        /// </summary>
        public override void Deselect()
        {
            base.IsSelected = false; // IsSelected를 직접 설정 (원본 코드에서는 base.Deselect()가 아님)

            // 진행 중인 선택 이미지 페이드 애니메이션이 있다면 중지시킵니다.
            selectionFadeCase.KillActive();

            // 선택 강조 이미지를 비활성화하고, 알파값을 0으로 설정합니다.
            selectionImage.gameObject.SetActive(false);
            selectionImage.color = selectionImage.color.SetAlpha(0.0f);
        }
    }
}