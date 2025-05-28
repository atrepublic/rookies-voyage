//====================================================================================================
// 해당 스크립트: FlyingTextBehavior.cs
// 기능: 화면에 떠다니는 텍스트(예: 획득한 아이템 개수)의 움직임 및 애니메이션을 제어합니다.
// 용도: 아이템 획득 시 획득 개수를 표시하는 텍스트가 특정 위치로 이동하고 사라지는 애니메이션을 구현합니다.
//====================================================================================================
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class FlyingTextBehavior : MonoBehaviour
    {
        private RectTransform rectTransform; // 텍스트의 위치 및 크기 조정을 위한 RectTransform 컴포넌트
        private CanvasGroup canvasGroup; // 텍스트의 투명도 조정을 위한 CanvasGroup 컴포넌트

        [Tooltip("텍스트 내용을 표시하는 Text 컴포넌트입니다.")]
        [SerializeField] private Text text;
        [Tooltip("텍스트와 함께 표시될 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] private Image image;

        [Tooltip("텍스트 크기 애니메이션에 사용되는 AnimationCurve입니다.")]
        [SerializeField] private AnimationCurve scaleAnimationCurve;

        private int amount; // 표시될 숫자 값 (예: 획득한 아이템 개수)
        /// <summary>
        /// 표시될 숫자 값을 가져오기 위한 프로퍼티입니다.
        /// </summary>
        public int Amount => amount;

        /// <summary>
        /// 여러 개의 떠다니는 텍스트가 있을 경우 순서를 지정하기 위한 프로퍼티입니다.
        /// </summary>
        public int Order { get; set; }

        private float startTime; // 텍스트 애니메이션 시작 시간

        // 스케일 애니메이션에 사용될 최소/최대 스케일 값
        private static float minScale = 1f;
        private static float maxScale = 1.5f;

        // 애니메이션의 최대 지속 시간
        private static float maxDuration = 10f;

        // 애니메이션 중 추가될 스케일 값의 최소/최대 범위
        private static float minValue = 0.1f;
        private static float maxValue = 0.2f;

        // 애니메이션 속도의 최소/최대 범위
        private static float minAnimSpeed = 0.6f;
        private static float maxAnimSpeed = 0.75f;

        // 애니메이션 보간 함수 (QuadIn 타입)
        private static Ease.IEasingFunction quadIn = Ease.GetFunction(Ease.Type.QuadIn);

        private bool isAlive = false; // 텍스트가 현재 활성 상태인지 나타내는 플래그

        /// <summary>
        /// 오브젝트가 생성될 때 호출되는 함수입니다.
        /// 필요한 컴포넌트를 가져옵니다.
        /// </summary>
        private void Awake()
        {
            // RectTransform 및 CanvasGroup 컴포넌트 가져오기
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 떠다니는 텍스트를 초기화하고 애니메이션을 시작하는 함수입니다.
        /// 부모, 아이콘, 시작 위치를 설정합니다.
        /// </summary>
        /// <param name="parent">텍스트의 부모 RectTransform</param>
        /// <param name="icon">텍스트와 함께 표시될 아이콘 스프라이트</param>
        /// <param name="position">텍스트의 시작 위치</param>
        public void Init(RectTransform parent, Sprite icon, Vector2 position)
        {
            // 초기 투명도를 0으로 설정
            canvasGroup.alpha = 0;

            // 부모 설정
            rectTransform.SetParent(parent);

            // 스케일 및 회전 초기화, 시작 위치 설정
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition = position - Vector2.up * 50;

            // 아이콘 이미지 설정
            image.sprite = icon;

            // 페이드 인 애니메이션 시작
            canvasGroup.DOFade(1, 0.2f);
            // 시작 위치로 이동하는 애니메이션 시작
            rectTransform.DOAnchoredPosition(position, 0.5f).SetEasing(Ease.Type.QuadOut);

            // 숫자 값 초기화
            amount = 0;

            // 애니메이션 시작 시간 기록
            startTime = Time.time;

            // 활성 상태 플래그 설정
            isAlive = true;
        }

        /// <summary>
        /// 매 프레임 호출되는 함수입니다.
        /// 텍스트의 스케일 애니메이션을 업데이트합니다.
        /// </summary>
        private void Update()
        {
            // 활성 상태가 아니면 업데이트하지 않음
            if (!isAlive) return;

            // 애니메이션 경과 시간 계산
            var time = Time.time - startTime;

            // 애니메이션 진행 상태 (0에서 1 사이 값) 계산 (QuadIn 보간 적용)
            var t = quadIn.Interpolate(Mathf.Clamp01(Mathf.InverseLerp(0, maxDuration, time)));

            // 시간에 따른 스케일 및 추가 스케일 값 계산
            var scale = Mathf.Lerp(minScale, maxScale, t);
            var value = Mathf.Lerp(minValue, maxValue, t);

            // 시간에 따른 애니메이션 속도 계산
            var speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, t);

            // 애니메이션 커브를 사용하여 최종 스케일 값 계산
            var animationTime = scaleAnimationCurve.Evaluate((time * speed) % 1);

            // 최종 스케일 적용
            rectTransform.localScale = Vector3.one * (scale + value * animationTime);
        }

        /// <summary>
        /// 표시될 텍스트의 숫자 값을 업데이트하는 함수입니다.
        /// </summary>
        /// <param name="amount">업데이트할 숫자 값</param>
        public void UpdateText(int amount)
        {
            this.amount = amount;

            // 텍스트 내용 업데이트 (예: +10)
            text.text = $"+{this.amount}";
        }

        /// <summary>
        /// 텍스트를 최종 위치로 날아가게 하고 사라지는 애니메이션을 실행하는 함수입니다.
        /// 애니메이션 완료 시 콜백 함수를 호출할 수 있습니다.
        /// </summary>
        /// <param name="finalPosition">텍스트가 이동할 최종 위치</param>
        /// <param name="onComplete">애니메이션 완료 시 호출될 콜백 함수</param>
        public void Fly(Vector2 finalPosition, SimpleCallback onComplete = null)
        {
            // 최종 위치로 이동하는 애니메이션 시작 (QuadOutIn 보간 적용)
            rectTransform.DOAnchoredPosition(finalPosition, 0.6f).SetEasing(Ease.Type.QuadOutIn).OnComplete(onComplete);
            // 스케일 축소 애니메이션 시작 (SineOut 보간 적용)
            rectTransform.DOScale(0.5f, 0.6f).SetEasing(Ease.Type.SineOut);

            // 딜레이 후 페이드 아웃 애니메이션 시작
            Tween.DelayedCall(0.4f, () => canvasGroup.DOFade(0, 0.2f));

            // 비활성 상태 플래그 설정
            isAlive = false;
        }
    }
}