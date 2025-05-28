// 이 스크립트는 게임패드 입력을 사용하여 설정(Settings) UI 메뉴를 탐색하고 조작할 수 있도록 합니다.
// UI 요소들을 게임패드 조작 가능한 버튼 목록으로 관리하고, 방향키 및 버튼 입력에 따라
// 현재 선택된 UI 요소를 변경하고 해당 요소의 동작(클릭 등)을 실행합니다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    // 이 컴포넌트가 부착된 게임 오브젝트에는 UISettings 컴포넌트가 반드시 필요함을 명시합니다.
    [RequireComponent(typeof(UISettings))]
    // 게임패드 입력을 통해 UI 설정을 조작하는 클래스입니다.
    // MonoBehaviour를 상속받아 게임 오브젝트에 컴포넌트로 추가됩니다.
    public class GamepadUISettings : MonoBehaviour
    {
        // 현재 게임패드 조작으로 선택된 버튼의 인덱스입니다.
        private int selectedButtonId;
        // 현재 선택된 IGamepadButton 인스턴스를 가져오는 프로퍼티입니다.
        private IGamepadButton SelectedButton => gamepadButtons[selectedButtonId];

        // 게임패드 조작 가능한 UI 버튼 목록입니다. IGamepadButton 인터페이스를 구현하는 객체를 담습니다.
        private List<IGamepadButton> gamepadButtons;

        // 이 GamepadUISettings 컴포넌트가 연결된 UISettings 컴포넌트입니다.
        private UISettings settingsUI;

        // Unity 생명주기 메소드: 오브젝트가 생성되고 로드될 때 호출됩니다.
        // 필요한 컴포넌트를 가져오고 UI 페이지 열림/닫힘 이벤트에 구독합니다.
        private void Awake()
        {
            // 동일 게임 오브젝트에 부착된 UISettings 컴포넌트를 가져옵니다.
            settingsUI = GetComponent<UISettings>();

            // UIController의 페이지 열림/닫힘 이벤트에 핸들러 메소드를 등록합니다.
            UIController.PageOpened += OnPageOpened;
            UIController.PageClosed += OnPageClosed;
        }

        // Unity 생명주기 메소드: 첫 번째 프레임 업데이트 이전에 호출됩니다.
        // 게임패드 조작 가능한 UI 버튼 목록을 구성합니다.
        private void Start()
        {
            // 게임패드 버튼 목록을 초기화합니다.
            gamepadButtons = new List<IGamepadButton>();

            // UISettings 컴포넌트에서 UI 요소들이 포함된 RectTransform을 가져옵니다.
            RectTransform contentTransform = settingsUI.ContentRectTransform;
            // 자식 오브젝트 수를 가져옵니다.
            int childCount = contentTransform.childCount;

            // ContentTransform의 모든 자식 오브젝트를 순회합니다.
            for(int i = 0; i < childCount; i++)
            {
                // 현재 자식 오브젝트를 가져옵니다.
                Transform child = contentTransform.GetChild(i);
                // 자식 오브젝트가 비활성화 상태이면 건너뜁니다.
                if (!child.gameObject.activeSelf) continue;

                // 자식 오브젝트에서 SettingsButtonBase 컴포넌트를 가져와 게임패드 버튼으로 추가 시도합니다.
                SettingsButtonBase settingsButtonBase = child.GetComponent<SettingsButtonBase>();
                if(settingsButtonBase != null)
                {
                    // SettingsButtonBase가 있으면 GamepadButton 래퍼로 만들어 목록에 추가합니다.
                    gamepadButtons.Add(new GamepadButton(settingsButtonBase));
                }
                else
                {
                    // SettingsButtonBase가 없으면 SettingsElementsGroup 컴포넌트를 가져와 그룹으로 추가 시도합니다.
                    SettingsElementsGroup settingsElementsGroup = child.GetComponent<SettingsElementsGroup>();
                    if(settingsElementsGroup != null)
                    {
                        // SettingsElementsGroup이 있으면 GamepadGroupButtons 래퍼로 만들어 목록에 추가합니다.
                        gamepadButtons.Add(new GamepadGroupButtons(settingsElementsGroup));
                    }
                }
            }
        }

        // Unity 생명주기 메소드: 오브젝트가 파괴될 때 호출됩니다.
        // UI 페이지 열림/닫힘 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        private void OnDestroy()
        {
            // 구독했던 이벤트 핸들러 메소드를 해제합니다.
            UIController.PageOpened -= OnPageOpened;
            UIController.PageClosed -= OnPageClosed;
        }

        // UI 페이지가 열렸을 때 호출되는 이벤트 핸들러 메소드입니다.
        // 열린 페이지가 이 컴포넌트의 설정 UI 페이지인 경우 게임패드 조작을 활성화합니다.
        // page: 열린 UI 페이지 인스턴스
        // pageType: 열린 UI 페이지의 타입
        private void OnPageOpened(UIPage page, Type pageType)
        {
            // 열린 페이지가 이 컴포넌트의 settingsUI와 동일한지 확인합니다.
            if(page == settingsUI)
            {
                // Control 시스템이 초기화되었고 현재 입력 타입이 게임패드인지 확인합니다.
                if (Control.IsInitialized && Control.InputType == InputType.Gamepad)
                {
                    // 첫 번째 버튼을 선택된 상태로 만들고 해당 버튼의 Select 메소드를 호출합니다.
                    selectedButtonId = 0;
                    SelectedButton.Select();
                }

                // 설정 UI에 해당하는 게임패드 버튼 태그를 활성화합니다.
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Settings); // UIGamepadButton 및 UIGamepadButtonTag는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            }
        }

        // UI 페이지가 닫혔을 때 호출되는 이벤트 핸들러 메소드입니다.
        // 닫힌 페이지가 이 컴포넌트의 설정 UI 페이지인 경우 게임패드 조작 관련 상태를 초기화합니다.
        // page: 닫힌 UI 페이지 인스턴스
        // pageType: 닫힌 UI 페이지의 타입
        private void OnPageClosed(UIPage page, Type pageType)
        {
            // 닫힌 페이지가 이 컴포넌트의 settingsUI와 동일한지 확인합니다.
            if (page == settingsUI)
            {
                // Control 시스템이 초기화되었고 현재 입력 타입이 게임패드인지 확인합니다.
                if (Control.IsInitialized && Control.InputType == InputType.Gamepad)
                {
                    // 현재 선택된 버튼의 Deselect 메소드를 호출하고 선택 인덱스를 초기화합니다.
                    SelectedButton.Deselect();
                    selectedButtonId = 0;
                }

                // 설정 UI에 해당하는 게임패드 버튼 태그를 비활성화합니다.
                UIGamepadButton.DisableTag(UIGamepadButtonTag.Settings); // UIGamepadButtonTag는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            }
        }

        // Unity 생명주기 메소드: 매 프레임마다 호출됩니다.
        // 게임패드 입력을 감지하고 UI 조작을 처리합니다.
        private void Update()
        {
            // 설정 UI 페이지가 표시되지 않았거나 Control 시스템이 초기화되지 않았으면 업데이트를 건너뜁니다.
            if (!settingsUI.IsPageDisplayed) return; // IsPageDisplayed는 UISettings에 정의되어 있을 것으로 가정합니다.
            if (!Control.IsInitialized) return; // Control는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

            // 현재 입력 타입이 게임패드인지 확인합니다.
            if (Control.InputType == InputType.Gamepad) // InputType은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            {
                // 게임패드의 D-Pad 아래 버튼이 이번 프레임에 눌렸는지 확인합니다.
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DDown)) // GamepadControl 및 GamepadButtonType는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 현재 선택된 버튼이 목록의 마지막 버튼이 아닌 경우 다음 버튼으로 이동합니다.
                    if (selectedButtonId < gamepadButtons.Count - 1)
                    {
                        // 현재 선택된 버튼의 Deselect 메소드를 호출합니다.
                        SelectedButton.Deselect();

                        // 선택 인덱스를 증가시키고 새로운 버튼을 선택 상태로 만듭니다.
                        selectedButtonId++;
                        SelectedButton.Select();
                    }
                }
                // 게임패드의 D-Pad 위 버튼이 이번 프레임에 눌렸는지 확인합니다.
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DUp)) // GamepadControl 및 GamepadButtonType는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 현재 선택된 버튼이 목록의 첫 번째 버튼이 아닌 경우 이전 버튼으로 이동합니다.
                    if (selectedButtonId > 0)
                    {
                        // 현재 선택된 버튼의 Deselect 메소드를 호출합니다.
                        SelectedButton.Deselect();

                        // 선택 인덱스를 감소시키고 새로운 버튼을 선택 상태로 만듭니다.
                        selectedButtonId--;
                        SelectedButton.Select();
                    }
                }
                // 게임패드의 D-Pad 왼쪽 버튼이 이번 프레임에 눌렸는지 확인합니다.
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DLeft)) // GamepadControl 및 GamepadButtonType는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 현재 선택된 버튼의 OnLeftButtonPressed 메소드를 호출합니다.
                    SelectedButton.OnLeftButtonPressed();
                }
                // 게임패드의 D-Pad 오른쪽 버튼이 이번 프레임에 눌렸는지 확인합니다.
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DRight)) // GamepadControl 및 GamepadButtonType는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 현재 선택된 버튼의 OnRightButtonPressed 메소드를 호출합니다.
                    SelectedButton.OnRightButtonPressed();
                }
                // 게임패드의 A 버튼이 이번 프레임에 눌렸는지 확인합니다.
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.A)) // GamepadControl 및 GamepadButtonType는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // 현재 선택된 버튼의 OnClick 메소드를 호출합니다.
                    SelectedButton.OnClick();
                }
                // 게임패드의 B 버튼이 이번 프레임에 눌렸는지 확인합니다.
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.B)) // GamepadControl 및 GamepadButtonType는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
                {
                    // UISettings의 OnCloseButtonClicked 메소드를 호출하여 UI 페이지를 닫습니다.
                    settingsUI.OnCloseButtonClicked(); // OnCloseButtonClicked는 UISettings에 정의되어 있을 것으로 가정합니다.
                }
            }
        }

        // 게임패드 조작 가능한 UI 요소가 구현해야 하는 인터페이스입니다.
        // 선택/선택 해제, 좌/우 버튼 입력, 클릭 액션을 정의합니다.
        private interface IGamepadButton
        {
            // UI 요소가 게임패드 조작으로 선택되었을 때 호출됩니다.
            public void Select();
            // UI 요소가 게임패드 조작으로 선택 해제되었을 때 호출됩니다.
            public void Deselect();

            // 게임패드의 왼쪽 버튼이 눌렸을 때 호출됩니다.
            public void OnLeftButtonPressed();
            // 게임패드의 오른쪽 버튼이 눌렸을 때 호출됩니다.
            public void OnRightButtonPressed();

            // 게임패드의 클릭/확인 버튼이 눌렸을 때 호출됩니다.
            public void OnClick();
        }

        // 단일 SettingsButtonBase 컴포넌트를 래핑하여 IGamepadButton 인터페이스를 구현하는 클래스입니다.
        private class GamepadButton : IGamepadButton
        {
            // 래핑하는 SettingsButtonBase 컴포넌트입니다.
            private SettingsButtonBase button; // SettingsButtonBase는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

            // GamepadButton 클래스의 생성자입니다.
            // 래핑할 SettingsButtonBase 컴포넌트를 전달받습니다.
            // button: 래핑할 SettingsButtonBase 컴포넌트
            public GamepadButton(SettingsButtonBase button)
            {
                this.button = button;
            }

            // IGamepadButton 인터페이스 구현: UI 요소가 선택되었을 때 호출됩니다.
            // 래핑된 버튼의 Select 메소드를 호출합니다.
            public void Select()
            {
                button.Select(); // Select 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
            }

            // IGamepadButton 인터페이스 구현: UI 요소가 선택 해제되었을 때 호출됩니다.
            // 래핑된 버튼의 Deselect 메소드를 호출합니다.
            public void Deselect()
            {
                button.Deselect(); // Deselect 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
            }

            // IGamepadButton 인터페이스 구현: 게임패드의 왼쪽 버튼이 눌렸을 때 호출됩니다.
            // 단일 버튼에는 해당 기능이 없으므로 비어 있습니다.
            public void OnLeftButtonPressed() { }
            // IGamepadButton 인터페이스 구현: 게임패드의 오른쪽 버튼이 눌렸을 때 호출됩니다.
            // 단일 버튼에는 해당 기능이 없으므로 비어 있습니다.
            public void OnRightButtonPressed() { }

            // IGamepadButton 인터페이스 구현: 게임패드의 클릭/확인 버튼이 눌렸을 때 호출됩니다.
            // 래핑된 버튼의 OnClick 메소드를 호출합니다.
            public void OnClick()
            {
                button.OnClick(); // OnClick 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
            }
        }

        // SettingsElementsGroup 내의 여러 SettingsButtonBase 컴포넌트를 래핑하여 IGamepadButton 인터페이스를 구현하는 클래스입니다.
        // 그룹 내에서 좌우 방향키로 하위 버튼을 선택하고 조작할 수 있도록 합니다.
        private class GamepadGroupButtons : IGamepadButton
        {
            // 그룹 내의 SettingsButtonBase 컴포넌트 배열입니다.
            private SettingsButtonBase[] buttons; // SettingsButtonBase는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

            // 현재 그룹 내에서 선택된 하위 버튼의 인덱스입니다.
            private int buttonIndex = 0;

            // GamepadGroupButtons 클래스의 생성자입니다.
            // 버튼 그룹에 해당하는 SettingsElementsGroup 컴포넌트를 전달받아 하위 버튼 목록을 구성합니다.
            // elementsGroup: 하위 버튼들을 포함하는 SettingsElementsGroup 컴포넌트
            public GamepadGroupButtons(SettingsElementsGroup elementsGroup) // SettingsElementsGroup은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            {
                // SettingsElementsGroup의 모든 자식에서 SettingsButtonBase 컴포넌트를 찾아 배열로 저장합니다.
                // activeSelf가 true인 오브젝트만 포함합니다.
                buttons = Array.FindAll(elementsGroup.GetComponentsInChildren<SettingsButtonBase>(), x => x.gameObject.activeSelf);
                // 초기 선택 인덱스를 0으로 설정합니다.
                buttonIndex = 0;
            }

            // IGamepadButton 인터페이스 구현: UI 요소 (그룹)가 선택되었을 때 호출됩니다.
            // 그룹 내의 첫 번째 하위 버튼을 선택 상태로 만듭니다.
            public void Select()
            {
                // 선택 인덱스를 0으로 초기화하고 첫 번째 버튼의 Select 메소드를 호출합니다.
                buttonIndex = 0;
                buttons[buttonIndex].Select(); // Select 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
            }

            // IGamepadButton 인터페이스 구현: UI 요소 (그룹)가 선택 해제되었을 때 호출됩니다.
            // 현재 선택된 하위 버튼의 Deselect 메소드를 호출하고 선택 인덱스를 초기화합니다.
            public void Deselect()
            {
                // 현재 선택된 버튼의 Deselect 메소드를 호출합니다.
                buttons[buttonIndex].Deselect(); // Deselect 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
                // 선택 인덱스를 0으로 초기화합니다.
                buttonIndex = 0;
            }

            // IGamepadButton 인터페이스 구현: 게임패드의 왼쪽 버튼이 눌렸을 때 호출됩니다.
            // 그룹 내에서 이전 하위 버튼으로 선택을 이동합니다.
            public void OnLeftButtonPressed()
            {
                int tempIndex = buttonIndex; // 임시 인덱스를 저장합니다.

                tempIndex--; // 인덱스를 감소시킵니다.
                if (tempIndex < 0) // 인덱스가 0보다 작아지면 0으로 유지합니다. (첫 번째 버튼에서 더 이상 왼쪽으로 이동 불가)
                    tempIndex = 0;

                // 선택 인덱스가 변경되었는지 확인합니다.
                if (buttonIndex != tempIndex)
                {
                    // 현재 버튼의 Deselect 메소드를 호출합니다.
                    buttons[buttonIndex].Deselect(); // Deselect 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
                    // 이전 버튼의 Select 메소드를 호출합니다.
                    buttons[tempIndex].Select(); // Select 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.

                    // 선택 인덱스를 업데이트합니다.
                    buttonIndex = tempIndex;
                }
            }

            // IGamepadButton 인터페이스 구현: 게임패드의 오른쪽 버튼이 눌렸을 때 호출됩니다.
            // 그룹 내에서 다음 하위 버튼으로 선택을 이동합니다.
            public void OnRightButtonPressed()
            {
                int tempIndex = buttonIndex; // 임시 인덱스를 저장합니다.

                tempIndex++; // 인덱스를 증가시킵니다.
                if (tempIndex >= buttons.Length) // 인덱스가 버튼 배열의 범위를 벗어나면 마지막 인덱스로 유지합니다. (마지막 버튼에서 더 이상 오른쪽으로 이동 불가)
                    tempIndex = buttons.Length - 1;

                // 선택 인덱스가 변경되었는지 확인합니다.
                if (buttonIndex != tempIndex)
                {
                    // 현재 버튼의 Deselect 메소드를 호출합니다.
                    buttons[buttonIndex].Deselect(); // Deselect 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
                    // 다음 버튼의 Select 메소드를 호출합니다.
                    buttons[tempIndex].Select(); // Select 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.

                    // 선택 인덱스를 업데이트합니다.
                    buttonIndex = tempIndex;
                }
            }

            // IGamepadButton 인터페이스 구현: 게임패드의 클릭/확인 버튼이 눌렸을 때 호출됩니다.
            // 현재 그룹 내에서 선택된 하위 버튼의 OnClick 메소드를 호출합니다.
            public void OnClick()
            {
                buttons[buttonIndex].OnClick(); // OnClick 메소드는 SettingsButtonBase에 정의되어 있을 것으로 가정합니다.
            }
        }
    }
}