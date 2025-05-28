// 스크립트 설명: 모바일 환경에서 사용되는 UI 조이스틱의 동작을 처리하는 클래스입니다.
// 사용자 입력(터치, 드래그)을 감지하고 이동 방향 벡터를 제공하며, UI 상태 및 애니메이션을 관리합니다.
using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 시스템 인터페이스 사용을 위한 네임스페이스
using UnityEngine.UI; // UI 컴포넌트 사용을 위한 네임스페이스
using Watermelon; // 네임스페이스 (SetAlpha, Tween 등 유틸리티 함수 포함 가능)

namespace Watermelon
{
    // Animator 컴포넌트가 필요함을 명시
    [RequireComponent(typeof(Animator))]
    // 포인터 이벤트 인터페이스 및 커스텀 ControlBehavior 인터페이스 구현
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IControlBehavior // IControlBehavior는 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
    {
        // 조이스틱 싱글톤 인스턴스
        public static Joystick Instance { get; private set; }

        [Header("조이스틱")] // 헤더 추가
        [SerializeField]
        [Tooltip("조이스틱 배경 이미지를 표시하는 Image 컴포넌트")] // 주요 변수 한글 툴팁
        protected Image backgroundImage; // 배경 이미지

        [SerializeField]
        [Tooltip("조이스틱 핸들 이미지를 표시하는 Image 컴포넌트")] // 주요 변수 한글 툴팁
        protected Image handleImage; // 핸들 이미지

        [Space]
        [SerializeField]
        [Tooltip("조이스틱 활성화 시 배경 이미지 색상")] // 주요 변수 한글 툴팁
        Color backgroundActiveColor = Color.white; // 배경 활성 색상

        [SerializeField]
        [Tooltip("조이스틱 비활성화 시 배경 이미지 색상")] // 주요 변수 한글 툴팁
        Color backgroundDisableColor = Color.white; // 배경 비활성 색상

        [SerializeField]
        [Tooltip("조이스틱 활성화 시 핸들 이미지 색상")] // 주요 변수 한글 툴팁
        Color handleActiveColor = Color.white; // 핸들 활성 색상

        [SerializeField]
        [Tooltip("조이스틱 비활성화 시 핸들 이미지 색상")] // 주요 변수 한글 툴팁
        Color handleDisableColor = Color.white; // 핸들 비활성 색상

        [Space]
        [SerializeField]
        [Tooltip("조이스틱 핸들이 움직일 수 있는 최대 반경 계수")] // 주요 변수 한글 툴팁
        float handleRange = 1; // 핸들 이동 반경

        [SerializeField]
        [Tooltip("조이스틱 입력이 무시되는 최소 거리 (데드존)")] // 주요 변수 한글 툴팁
        float deadZone = 0; // 입력 무시 데드존

        [Header("튜토리얼")] // 헤더 추가
        [SerializeField]
        [Tooltip("조이스틱 튜토리얼 사용 여부")] // 주요 변수 한글 툴팁
        bool useTutorial; // 튜토리얼 사용 여부

        [SerializeField]
        [Tooltip("튜토리얼 시 표시될 포인터 게임 오브젝트")] // 주요 변수 한글 툴팁
        GameObject pointerGameObject; // 튜토리얼 포인터 오브젝트

        private RectTransform baseRectTransform; // 조이스틱 전체 영역의 RectTransform
        private RectTransform backgroundRectTransform; // 배경 이미지의 RectTransform
        private RectTransform handleRectTransform; // 핸들 이미지의 RectTransform

        private bool isActive; // 조이스틱이 현재 활성화되어(터치되어) 있는지 여부
        // 이동 입력이 0이 아닌지 확인하는 프로퍼티
        public bool IsMovementInputNonZero => isActive;

        private bool canDrag; // 조이스틱 드래그가 가능한 상태인지 여부

        private Canvas canvas; // 조이스틱이 속한 캔버스
        private Camera canvasCamera; // 캔버스 렌더 모드가 Screen Space - Camera일 경우 사용될 카메라

        [Tooltip("현재 조이스틱 입력 벡터 (정규화되지 않음)")] // 주요 변수 한글 툴팁
        protected Vector2 input = Vector2.zero; // 조이스틱 입력 벡터 (2D)

        // 외부에서 입력 벡터에 접근하기 위한 프로퍼티 (Vector3 형태로 변환)
        public Vector3 Input => input; // 현재 입력 벡터
        // 이동 입력 벡터 (Vector3 형태로 변환, Y축 무시)
        public Vector3 MovementInput => new Vector3(input.x, 0, input.y);

        private Vector2 defaultAnchoredPosition; // 배경 이미지의 기본 앵커 위치

        // 시야 입력이 0이 아닌지 확인하는 프로퍼티 (조이스틱은 시야 입력을 제공하지 않으므로 항상 false)
        public bool IsLookInputNonZero => false;
        // 시야 입력 벡터 (조이스틱은 시야 입력을 제공하지 않으므로 항상 Vector3.zero)
        public Vector3 LookInput => Vector3.zero;

        private Animator joystickAnimator; // 조이스틱 애니메이션을 제어하는 Animator 컴포넌트
        private bool isTutorialDisplayed; // 튜토리얼이 이미 표시되었는지 여부
        private bool hideVisualsActive; // UI 시각 요소를 숨김 상태로 설정했는지 여부

        // 조이스틱 이동 입력이 활성화되었을 때 발생할 이벤트
        public event SimpleCallback OnMovementInputActivated; // SimpleCallback은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정

        /// <summary>
        /// 조이스틱을 초기화하고 설정을 적용합니다.
        /// </summary>
        /// <param name="canvas">조이스틱이 속한 Canvas 컴포넌트.</param>
        public void Init(Canvas canvas)
        {
            this.canvas = canvas; // 캔버스 참조 설정

            Instance = this; // 싱글톤 인스턴스 설정

            joystickAnimator = GetComponent<Animator>(); // Animator 컴포넌트 가져오기

            // RectTransform 컴포넌트 가져오기
            baseRectTransform = GetComponent<RectTransform>();
            backgroundRectTransform = backgroundImage.rectTransform;
            handleRectTransform = handleImage.rectTransform;

            // 캔버스 렌더 모드가 Screen Space - Camera일 경우 카메라 참조 가져오기
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                canvasCamera = canvas.worldCamera;

            // UI 요소들의 피벗 및 앵커 설정
            Vector2 center = new Vector2(0.5f, 0.5f);
            backgroundRectTransform.pivot = center;
            handleRectTransform.anchorMin = center;
            handleRectTransform.anchorMax = center;
            handleRectTransform.pivot = center;
            handleRectTransform.anchoredPosition = Vector2.zero; // 핸들 초기 위치를 배경 중앙으로 설정

            isActive = false; // 초기 상태는 비활성

            // 튜토리얼 사용 여부에 따라 튜토리얼 관련 설정 적용
            if(useTutorial)
            {
                joystickAnimator.enabled = true; // 애니메이터 활성화
                isTutorialDisplayed = true; // 튜토리얼 표시 상태
                pointerGameObject.SetActive(true); // 포인터 오브젝트 활성화
            }
            else
            {
                joystickAnimator.enabled = false; // 애니메이터 비활성화
                isTutorialDisplayed = false; // 튜토리얼 미표시 상태
                pointerGameObject.SetActive(false); // 포인터 오브젝트 비활성화
            }

            // 초기 배경 및 핸들 색상 설정 (숨김 상태 고려)
            // SetAlpha 확장 메서드는 Watermelon 네임스페이스에 정의된 것으로 가정
            backgroundImage.color = backgroundDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);
            handleImage.color = handleDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);

            // 배경 이미지의 기본 앵커 위치 저장
            defaultAnchoredPosition = backgroundRectTransform.anchoredPosition;

            // 현재 입력 타입이 UI 조이스틱이면 컨트롤러로 설정
            if (Control.InputType == InputType.UIJoystick) // Control, InputType은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
            {
                Control.SetControl(this); // Control에 이 조이스틱을 컨트롤러로 설정 (Control에 정의된 것으로 가정)
            }
            else // 다른 입력 타입이면 조이스틱 오브젝트 비활성화
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 포인터(터치 또는 클릭)가 조이스틱 영역 안에서 눌러졌을 때 호출됩니다.
        /// 드래그 가능 여부를 판단하고 조이스틱을 활성화합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // WorldSpaceRaycaster를 사용하여 UI 외부의 월드 공간 레이캐스트 결과를 확인합니다.
            // WorldSpaceRaycaster는 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
            canDrag = !WorldSpaceRaycaster.Raycast(eventData);

            if (!canDrag) return; // 드래그 불가능 상태면 함수 종료

            // 튜토리얼이 표시 중이라면 숨김 처리
            if (!isTutorialDisplayed)
            {
                isTutorialDisplayed = true;

                joystickAnimator.enabled = false;
                pointerGameObject.SetActive(false);
            }

            // 배경 이미지 위치를 터치된 스크린 위치의 앵커 위치로 설정
            backgroundRectTransform.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);

            // 배경 및 핸들 색상을 활성 색상으로 변경 (숨김 상태 고려)
            backgroundImage.color = backgroundActiveColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);
            handleImage.color = handleActiveColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);

            isActive = true; // 조이스틱 활성화 상태로 변경

            OnMovementInputActivated?.Invoke(); // 이동 입력 활성화 이벤트 발생 (null 조건부 연산자 사용)

            OnDrag(eventData); // 터치 즉시 드래그 처리 함수 호출
        }

        /// <summary>
        /// 포인터가 조이스틱 영역 안에서 드래그될 때 호출됩니다.
        /// 핸들의 위치를 업데이트하고 입력 벡터를 계산합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (!isActive || !canDrag) // 조이스틱이 활성 상태가 아니거나 드래그 불가능 상태면 처리 중지
                return;

            // 배경 이미지의 스크린 좌표 및 크기 계산
            Vector2 position = RectTransformUtility.WorldToScreenPoint(canvasCamera, backgroundRectTransform.position);
            Vector2 radius = backgroundRectTransform.sizeDelta / 2;
            // 입력 벡터 계산 (터치 위치 - 배경 중앙 위치) / (배경 반경 * 캔버스 스케일)
            input = (eventData.position - position) / (radius * canvas.scaleFactor);
            // 계산된 입력 벡터를 사용하여 핸들 위치 및 데드존 처리
            HandleInput(input.magnitude, input.normalized, radius, canvasCamera); // HandleInput 메서드 호출
            // 핸들 이미지의 앵커 위치를 입력 벡터와 핸들 이동 반경을 곱하여 설정
            handleRectTransform.anchoredPosition = input * radius * handleRange;
        }

        /// <summary>
        /// 입력 벡터의 크기(magnitude)와 정규화된 벡터를 기반으로 최종 입력 값을 처리하고 데드존을 적용합니다.
        /// </summary>
        /// <param name="magnitude">입력 벡터의 크기.</param>
        /// <param name="normalised">입력 벡터의 정규화된 벡터.</param>
        /// <param name="radius">조이스틱 배경의 반경.</param>
        /// <param name="cam">사용된 카메라.</param>
        protected void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            // 입력 크기가 데드존보다 크면
            if (magnitude > deadZone)
            {
                // 입력 크기가 1보다 크면 정규화된 벡터 사용 (반경 제한)
                if (magnitude > 1)
                    input = normalised;
            }
            else // 입력 크기가 데드존 이하이면 입력을 0으로 설정
            {
                input = Vector2.zero;
            }
        }

        /// <summary>
        /// 포인터가 조이스틱 영역 안에서 떨어졌을 때 호출됩니다.
        /// 조이스틱 상태를 비활성화하고 초기 상태로 되돌립니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            // WorldSpaceRaycaster의 포인터 업 처리 함수 호출 (WorldSpaceRaycaster에 정의된 것으로 가정)
            WorldSpaceRaycaster.OnPointerUp(eventData);

            if (!isActive) // 조이스틱이 이미 비활성 상태라면 처리 중지
                return;

            isActive = false; // 조이스틱 비활성 상태로 변경

            ResetControl(); // 조이스틱 상태 초기화
        }

        /// <summary>
        /// 조이스틱의 시각적 요소와 입력 상태를 초기 상태로 되돌립니다.
        /// </summary>
        public void ResetControl()
        {
            isActive = false; // 활성 상태 비활성화

            // 배경 및 핸들 색상을 비활성 색상으로 변경 (숨김 상태 고려)
            // SetAlpha 확장 메서드는 Watermelon 네임스페이스에 정의된 것으로 가정
            backgroundImage.color = backgroundDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);
            handleImage.color = handleDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);

            backgroundRectTransform.anchoredPosition = defaultAnchoredPosition; // 배경 이미지 위치를 기본 위치로 되돌림

            input = Vector2.zero; // 입력 벡터 초기화
            handleRectTransform.anchoredPosition = Vector2.zero; // 핸들 위치 초기화 (배경 중앙)
        }

        /// <summary>
        /// 스크린 좌표를 조이스틱의 기본 RectTransform에 대한 앵커 위치로 변환합니다.
        /// </summary>
        /// <param name="screenPosition">변환할 스크린 좌표.</param>
        /// <returns>변환된 앵커 위치.</returns>
        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            // RectTransformUtility를 사용하여 스크린 좌표를 로컬 좌표로 변환
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRectTransform, screenPosition, canvasCamera, out localPoint))
            {
                // 피벗 오프셋 계산
                Vector2 pivotOffset = baseRectTransform.pivot * baseRectTransform.sizeDelta;
                // 최종 앵커 위치 계산 및 반환
                return localPoint - (backgroundRectTransform.anchorMax * baseRectTransform.sizeDelta) + pivotOffset;
            }
            return Vector2.zero; // 변환 실패 시 Vector2.zero 반환
        }

        /// <summary>
        /// 조이스틱 게임 오브젝트를 활성화하여 이동 컨트롤을 가능하게 합니다. (IControlBehavior 인터페이스 구현)
        /// </summary>
        public void EnableMovementControl()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 조이스틱 게임 오브젝트를 비활성화하여 이동 컨트롤을 불가능하게 합니다. (IControlBehavior 인터페이스 구현)
        /// </summary>
        public void DisableMovementControl()
        {
            gameObject.SetActive(false); // 오브젝트 비활성화
            isActive = false; // 활성 상태 비활성화

            ResetControl(); // 상태 초기화
        }

        /// <summary>
        /// 조이스틱의 시각적 요소를 투명하게 만들어 숨깁니다.
        /// </summary>
        public void HideVisuals()
        {
            hideVisualsActive = true; // 숨김 상태 활성화

            // 배경 및 핸들 이미지를 투명하게 설정 (SetAlpha 확장 메서드 사용)
            backgroundImage.color = backgroundImage.color.SetAlpha(0f);
            handleImage.color = backgroundImage.color.SetAlpha(0f);
        }

        /// <summary>
        /// 조이스틱의 시각적 요소를 다시 보이게 합니다.
        /// </summary>
        public void ShowVisuals()
        {
            hideVisualsActive = false; // 숨김 상태 비활성화

            // 배경 및 핸들 이미지를 불투명하게 설정 (SetAlpha 확장 메서드 사용)
            backgroundImage.color = backgroundImage.color.SetAlpha(1f);
            handleImage.color = backgroundImage.color.SetAlpha(1f);
        }

        // 조이스틱이 터치되었을 때 호출될 콜백 함수의 델리게이트 타입 정의 (현재 사용되지 않음)
        public delegate void OnJoystickTouchedCallback();
    }
}