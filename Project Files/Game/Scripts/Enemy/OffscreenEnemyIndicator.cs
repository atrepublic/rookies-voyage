// ==============================================
// 📌 OffscreenEnemyIndicator.cs
// ✅ 화면 밖에 있는 적을 UI 화살표로 표시하는 컴포넌트
// ✅ 적의 위치를 기준으로 방향 회전과 화면 경계 위치를 계산하여 표시
// ✅ 적이 다시 화면 안으로 들어오면 UI를 자동으로 숨김
// ==============================================

using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class OffscreenEnemyIndicator : MonoBehaviour
    {
        [Tooltip("화살표에 사용할 UI 이미지 컴포넌트")]
        [SerializeField] private Image image;

        // 내부 변수들
        private RectTransform rect;
        private BaseEnemyBehavior enemy;
        private TweenCase fadeCase;

        private bool IsEnabled { get; set; }
        private bool IsTransparent { get; set; }

        private Vector2 screenSize;
        private Vector2 centerViewportPos;
        private Vector2 parentViewportMin;
        private Vector2 parentViewportMax;

        /// <summary>
        /// 📌 RectTransform 캐싱
        /// </summary>
        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 📌 처음 활성화 시 UI 투명하게 시작
        /// </summary>
        private void OnEnable()
        {
            image.SetAlpha(0f);
        }

        /// <summary>
        /// 📌 적 오브젝트와 부모 뷰포트 기준값을 설정하여 초기화
        /// </summary>
        public void Init(BaseEnemyBehavior enemy, RectTransform parent, RectTransform baseCanvasRect)
        {
            screenSize = baseCanvasRect.rect.size;
            var pixelViewportSize = new Vector2(1f / screenSize.x, 1f / screenSize.y);

            float minX = parent.offsetMin.x;
            float maxX = screenSize.x + parent.offsetMax.x;

            float minY = parent.offsetMin.y;
            float maxY = screenSize.y + parent.offsetMax.y;

            float centerX = (minX + maxX) / 2f;
            float centerY = (minY + maxY) / 2f;

            centerViewportPos = new Vector2(centerX * pixelViewportSize.x, centerY * pixelViewportSize.y);

            parentViewportMin = new Vector2(minX * pixelViewportSize.x, minY * pixelViewportSize.y);
            parentViewportMax = new Vector2(maxX * pixelViewportSize.x, maxY * pixelViewportSize.y);

            this.enemy = enemy;
            IsEnabled = enemy.IsVisible;

            if (!IsEnabled)
                Show();
        }

        /// <summary>
        /// 📌 화살표 표시 (페이드 인)
        /// </summary>
        public void Show()
        {
            fadeCase.KillActive();
            fadeCase = image.DOFade(1, 0.3f);
            IsTransparent = false;
        }

        /// <summary>
        /// 📌 화살표 숨기기 (페이드 아웃)
        /// </summary>
        public void Hide()
        {
            fadeCase.KillActive();
            fadeCase = image.DOFade(0, 0.3f).OnComplete(() => IsTransparent = true);
        }

        /// <summary>
        /// 📌 매 프레임마다 적이 화면 안/밖에 있는지 확인하고, 화살표 위치/회전 계산
        /// </summary>
        private void Update()
        {
            if (IsEnabled)
            {
                if (!enemy.IsVisible)
                {
                    IsEnabled = false;
                    Show();
                }
            }
            else
            {
                if (enemy.IsVisible)
                {
                    IsEnabled = true;
                    Hide();
                }
            }

            if (!IsTransparent)
            {
                Vector3 enemyScreenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
                Vector2 enemyViewportPosition = Camera.main.ScreenToViewportPoint(enemyScreenPosition);

                // 방향 설정 (회전 각도)
                if (enemyViewportPosition.x <= 0)
                {
                    if (enemyViewportPosition.y <= 0) rect.localRotation = Quaternion.Euler(0, 0, -135);
                    else if (enemyViewportPosition.y >= 1) rect.localRotation = Quaternion.Euler(0, 0, 135);
                    else rect.localRotation = Quaternion.Euler(0, 0, 180);
                }
                else if (enemyViewportPosition.x >= 1)
                {
                    if (enemyViewportPosition.y <= 0) rect.localRotation = Quaternion.Euler(0, 0, -45);
                    else if (enemyViewportPosition.y >= 1) rect.localRotation = Quaternion.Euler(0, 0, 45);
                    else rect.localRotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    if (enemyViewportPosition.y <= 0) rect.localRotation = Quaternion.Euler(0, 0, -90);
                    else if (enemyViewportPosition.y >= 1) rect.localRotation = Quaternion.Euler(0, 0, 90);
                }

                // 화면 경계 내 위치로 제한
                enemyViewportPosition.x = Mathf.Clamp(enemyViewportPosition.x, parentViewportMin.x, parentViewportMax.x);
                enemyViewportPosition.y = Mathf.Clamp(enemyViewportPosition.y, parentViewportMin.y, parentViewportMax.y);

                // 위치 계산
                Vector2 anchoredPos = new Vector2(
                    (enemyViewportPosition.x - parentViewportMin.x) * screenSize.x,
                    (enemyViewportPosition.y - parentViewportMin.y) * screenSize.y
                );
                rect.anchoredPosition = anchoredPos;
            }
        }

        /// <summary>
        /// 📌 특정 X 위치로 방향 벡터 이동 보정 (사용되지 않음)
        /// </summary>
        private Vector2 MoveXToN(Vector2 viewportPos, float n)
        {
            var direction = (viewportPos - centerViewportPos).normalized;
            var k = (n - centerViewportPos.x) / direction.x;
            var y = centerViewportPos.y + k * direction.y;
            return new Vector2(n, y);
        }

        /// <summary>
        /// 📌 특정 Y 위치로 방향 벡터 이동 보정 (사용되지 않음)
        /// </summary>
        private Vector2 MoveYToN(Vector2 viewportPos, float n)
        {
            var direction = (viewportPos - centerViewportPos).normalized;
            var k = (n - centerViewportPos.y) / direction.y;
            var x = centerViewportPos.x + k * direction.x;
            return new Vector2(x, n);
        }
    }
}
