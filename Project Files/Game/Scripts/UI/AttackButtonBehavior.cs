// 이 스크립트는 공격 버튼의 동작을 제어하는 UI 컴포넌트입니다.
// UnityEngine.UI.Button을 상속받아 버튼 클릭 및 게임패드 입력에 반응하며,
// 공격 상태(눌림/떼짐)를 관리하고 외부에 이벤트를 알립니다. 재장전 UI 표시 기능도 포함합니다.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    // 공격 버튼의 동작을 정의하는 클래스입니다.
    // UnityEngine.UI.Button을 상속받아 UI 버튼으로서의 기능을 확장합니다.
    public class AttackButtonBehavior : Button
    {
        // AttackButtonBehavior의 싱글톤 인스턴스입니다.
        private static AttackButtonBehavior instance;

        // 재장전 상태를 시각적으로 표시하는 원형 채우기 이미지입니다.
        private Image radialFillImage;
        // 게임패드 입력을 처리하는 UIGamepadButton 컴포넌트입니다.
        private UIGamepadButton uiGamepadButton; // UIGamepadButton은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

        // 공격 버튼이 현재 눌려있는지 여부를 나타내는 프로퍼티입니다. (읽기 전용)
        public static bool IsButtonPressed { get; private set; }

        // 공격 버튼의 눌림/떼짐 상태가 변경될 때 발생하는 이벤트입니다.
        public static event SimpleBoolCallback onStatusChanged; // SimpleBoolCallback은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

        // Unity 생명주기 메소드: 오브젝트가 생성되고 로드될 때 호출됩니다.
        // 싱글톤 인스턴스를 설정하고 필요한 컴포넌트 참조를 가져옵니다.
        protected override void Awake()
        {
            // 싱글톤 인스턴스를 설정합니다.
            instance = this;

            // 자식 오브젝트에서 Image 컴포넌트를 찾아 radialFillImage에 할당합니다. (재장전 이미지)
            radialFillImage = transform.GetChild(0).GetComponent<Image>();
            // 동일 게임 오브젝트에 부착된 UIGamepadButton 컴포넌트를 가져옵니다.
            uiGamepadButton = GetComponent<UIGamepadButton>(); // UIGamepadButton은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

            // Button 클래스의 Awake 메소드를 호출합니다.
            base.Awake();
        }

        // Unity 생명주기 메소드: 매 프레임마다 호출됩니다.
        // 게임패드 입력을 감지하여 공격 버튼 상태를 업데이트합니다.
        private void Update()
        {
            // 현재 입력 타입이 게임패드인지 확인합니다.
            if (Control.InputType == InputType.Gamepad) // Control 및 InputType은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            {
                // UIGamepadButton에 설정된 버튼 타입에 해당하는 게임패드 버튼이 이번 프레임에 눌렸는지 확인합니다.
                if (GamepadControl.WasButtonPressedThisFrame(uiGamepadButton.ButtonType)) // GamepadControl 및 GamepadButtonType은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 버튼 눌림 상태로 설정하고 상태 변경 이벤트를 발생시킵니다.
                    IsButtonPressed = true;
                    onStatusChanged?.Invoke(true);
                }
                // UIGamepadButton에 설정된 버튼 타입에 해당하는 게임패드 버튼이 이번 프레임에 떼어졌는지 확인합니다.
                else if (GamepadControl.WasButtonReleasedThisFrame(uiGamepadButton.ButtonType)) // GamepadControl 및 GamepadButtonType은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 버튼 떼짐 상태로 설정하고 상태 변경 이벤트를 발생시킵니다.
                    IsButtonPressed = false;
                    onStatusChanged?.Invoke(false);
                }
            }
        }

        // UnityEngine.EventSystems.IPointerUpHandler 인터페이스 구현 메소드: 포인터(마우스, 터치 등)가 버튼 위에서 떼어졌을 때 호출됩니다.
        // 버튼 떼짐 상태로 설정하고 상태 변경 이벤트를 발생시킵니다.
        // eventData: 포인터 이벤트 데이터
        public override void OnPointerUp(PointerEventData eventData)
        {
            // Button 클래스의 OnPointerUp 메소드를 호출합니다.
            base.OnPointerUp(eventData);

            // 버튼 떼짐 상태로 설정하고 상태 변경 이벤트를 발생시킵니다.
            IsButtonPressed = false;
            onStatusChanged?.Invoke(false);
        }

        // UnityEngine.EventSystems.IPointerDownHandler 인터페이스 구현 메소드: 포인터(마우스, 터치 등)가 버튼 위에서 눌렸을 때 호출됩니다.
        // 버튼 눌림 상태로 설정하고 상태 변경 이벤트를 발생시킵니다.
        // eventData: 포인터 이벤트 데이터
        public override void OnPointerDown(PointerEventData eventData)
        {
            // Button 클래스의 OnPointerDown 메소드를 호출합니다.
            base.OnPointerDown(eventData);

            // 버튼 눌림 상태로 설정하고 상태 변경 이벤트를 발생시킵니다.
            IsButtonPressed = true;
            onStatusChanged?.Invoke(true);
        }

        // 재장전 UI의 채우기 정도를 설정하는 정적 메소드입니다.
        // t: 채우기 정도 (0.0f ~ 1.0f)
        public static void SetReloadFill(float t)
        {
            // 싱글톤 인스턴스가 유효한지 확인합니다.
            if (instance == null) return;

            // radialFillImage의 fillAmount를 설정하여 재장전 상태를 시각적으로 표시합니다.
            instance.radialFillImage.fillAmount = t;
        }
    }
}