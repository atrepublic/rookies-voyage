// ==============================================
// 📌 KeyboardControl.cs
// ✅ 키보드 입력을 통해 플레이어 이동을 제어하는 클래스
// ✅ WASD 및 화살표 키로 이동 방향 입력을 받아 Control 시스템에 연동
// ✅ Input System 모듈이 설치된 경우 Gamepad 및 Mouse 입력도 감지 가능
// ==============================================

using UnityEngine;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Watermelon
{
    public class KeyboardControl : MonoBehaviour, IControlBehavior
    {
        [Tooltip("현재 키보드 입력 방향")]
        public Vector3 MovementInput { get; private set; }

        [Tooltip("현재 입력값이 존재하는지 여부")]
        public bool IsMovementInputNonZero { get; private set; }

        [Tooltip("이동 입력이 활성화된 상태인지 여부")]
        private bool IsMovementControlActive;

        [Tooltip("처음 이동 입력이 감지되었을 때 호출되는 이벤트")]
        public event SimpleCallback OnMovementInputActivated;

        /// <summary>
        /// 📌 키보드 컨트롤 초기화
        /// </summary>
        public void Init()
        {
            if (Control.InputType == InputType.Keyboard)
            {
                Control.SetControl(this);

                enabled = true;
                IsMovementControlActive = true;
            }
            else
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 📌 키보드 입력 감지 및 이동 벡터 갱신
        /// </summary>
        private void Update()
        {
#if MODULE_INPUT_SYSTEM
            // Gamepad 입력 감지되면 자동 전환
            if (Gamepad.current != null &&
                Gamepad.current.wasUpdatedThisFrame &&
                !Gamepad.current.CheckStateIsAtDefaultIgnoringNoise())
            {
                Control.ChangeInputType(InputType.Gamepad);
                Destroy(this);
                return;
            }

            // 마우스 클릭 위치 레이캐스트
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                WorldSpaceRaycaster.Raycast(Mouse.current.position.value);
            }

            if (!IsMovementControlActive || Keyboard.current == null)
                return;

            float horizontalInput = 0;
            float verticalInput = 0;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontalInput += 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontalInput -= 1;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                verticalInput += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                verticalInput -= 1;

            // 이동 방향 벡터 설정
            MovementInput = Vector3.ClampMagnitude(new Vector3(horizontalInput, 0, verticalInput), 1);

            // 처음 입력 감지 시 이벤트 호출
            if (!IsMovementInputNonZero && MovementInput.magnitude > 0.1f)
            {
                IsMovementInputNonZero = true;
                OnMovementInputActivated?.Invoke();
            }

            IsMovementInputNonZero = MovementInput.magnitude > 0.1f;
#endif
        }

        #region Control 관리 함수

        /// <summary>
        /// 📌 이동 입력 비활성화
        /// </summary>
        public void DisableMovementControl()
        {
            IsMovementControlActive = false;
        }

        /// <summary>
        /// 📌 이동 입력 활성화
        /// </summary>
        public void EnableMovementControl()
        {
            IsMovementControlActive = true;
        }

        /// <summary>
        /// 📌 이동 상태 초기화
        /// </summary>
        public void ResetControl()
        {
            IsMovementInputNonZero = false;
            MovementInput = Vector3.zero;
        }

        #endregion
    }
}
