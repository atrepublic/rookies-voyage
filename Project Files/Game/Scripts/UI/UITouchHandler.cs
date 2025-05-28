/*
📄 UITouchHandler.cs 요약
UI 상에서 손가락 또는 마우스 드래그 입력을 감지하고, 그 방향 및 세기를 계산하는 입력 처리 컴포넌트야.

🧩 주요 기능
IPointerDownHandler, IDragHandler, IPointerUpHandler 인터페이스를 통해 터치/드래그 입력을 추적해.

입력의 방향 및 거리(offset) 를 기준으로 ClampedOffset(0~1 범위) 를 계산해서 다른 시스템에 입력 강도를 넘겨줄 수 있어.

GetInputDirection() 메서드를 통해 월드 좌표 기준 입력 방향을 계산해서 외부에서 활용 가능.

⚙️ 사용 용도
캐릭터 이동 제어용 드래그 입력 처리

UI 상에서 탭이나 슬라이드 같은 입력의 방향/세기를 읽어 게임에 반영할 수 있음.
*/

#pragma warning disable 0414

using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class UITouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public static bool Enabled { get; set; }

        private static bool isPointerDown;

        public static float ClampedOffset { get; private set; }

        [SerializeField] float maxOffset;
        [SerializeField] float minOffset;

        [SerializeField] float snappingLerp;

        private static Vector2 center;
        private static Vector2 absolutePosition;

        public static Vector2 Offset { get => absolutePosition - center; set => absolutePosition = center + value; }

        private void Awake()
        {
            isPointerDown = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;

            center = eventData.position;
            absolutePosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;

            center = absolutePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            absolutePosition = eventData.position;

            if (Offset.magnitude < minOffset) return;
            if (Offset.magnitude > maxOffset)
            {
                center = absolutePosition - Offset.normalized * maxOffset;
            }

            ClampedOffset = Mathf.Clamp01(Mathf.InverseLerp(minOffset, maxOffset, Offset.magnitude));
        }

        private void Update()
        {
            if (isPointerDown)
            {
                center = Vector2.Lerp(center, absolutePosition, snappingLerp * Time.deltaTime);

                ClampedOffset = Mathf.Clamp01(Mathf.InverseLerp(minOffset, maxOffset, Offset.magnitude));
            }
        }

        public static Vector3 GetInputDirection()
        {
            if (!isPointerDown) return Vector3.zero;

            if (ClampedOffset <= 0) return Vector3.zero;

            Vector3 prevPoint = center;
            Vector3 currentpoint = absolutePosition;

            prevPoint.z = 1;
            currentpoint.z = 1;

            Vector3 worldPrevPoint = Camera.main.ScreenToWorldPoint(prevPoint);
            Vector3 worldCurrentPoint = Camera.main.ScreenToWorldPoint(currentpoint);

            return (worldCurrentPoint - worldPrevPoint).normalized;
        }
    }
}