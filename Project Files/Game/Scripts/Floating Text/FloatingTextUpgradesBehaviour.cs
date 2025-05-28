// FloatingTextUpgradesBehaviour.cs
// 이 스크립트는 업그레이드 획득 등과 관련된 떠오르는 텍스트 및 아이콘의 동작을 제어합니다.
// 텍스트와 아이콘이 함께 나타나 크기 및 페이드 애니메이션 후 사라지는 데 사용됩니다.
using UnityEngine;
using UnityEngine.UI; // UI 관련 컴포넌트를 사용하기 위해 필요
using Watermelon; // Tweening 및 Ease 기능을 위해 필요 (Watermelon 라이브러리는 외부 정의가 필요합니다.)

namespace Watermelon.SquadShooter // SquadShooter 네임스페이스에 포함
{
    // 떠오르는 텍스트의 기본 동작을 정의하는 추상 클래스 FloatingTextBaseBehavior를 상속받습니다. (FloatingTextBaseBehavior는 외부 정의가 필요합니다.)
    public class FloatingTextUpgradesBehaviour : FloatingTextBaseBehavior
    {
        [SerializeField, Tooltip("텍스트와 아이콘을 포함하는 컨테이너 트랜스폼")] // containerTransform 변수에 대한 툴팁
        private Transform containerTransform;
        [SerializeField, Tooltip("컨테이너의 알파(투명도)를 제어하는 CanvasGroup 컴포넌트")] // containerCanvasGroup 변수에 대한 툴팁
        private CanvasGroup containerCanvasGroup;
        [SerializeField, Tooltip("표시될 아이콘을 위한 Image 컴포넌트")] // iconImage 변수에 대한 툴팁
        private Image iconImage;
        [SerializeField, Tooltip("떠오르는 텍스트를 표시할 Text 컴포넌트")] // floatingText 변수에 대한 툴팁
        private Text floatingText; // Legacy Text 컴포넌트 사용

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("컨테이너가 최종적으로 이동할 로컬 위치에서의 오프셋")] // offset 변수에 대한 툴팁
        private Vector3 offset;
        [SerializeField, Tooltip("컨테이너 이동 애니메이션에 걸리는 시간 (초)")] // time 변수에 대한 툴팁
        private float time;
        [SerializeField, Tooltip("컨테이너 이동 애니메이션에 사용될 이징(Easing) 타입")] // easing 변수에 대한 툴팁
        private Ease.Type easing; // Ease.Type 열거형은 외부 정의가 필요합니다.

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("컨테이너 크기 애니메이션에 걸리는 시간 (초)")] // scaleTime 변수에 대한 툴팁
        private float scaleTime;
        [SerializeField, Tooltip("컨테이너 크기 애니메이션에 사용될 애니메이션 커브")] // scaleAnimationCurve 변수에 대한 툴팁
        private AnimationCurve scaleAnimationCurve;

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("컨테이너 페이드 아웃 애니메이션에 걸리는 시간 (초)")] // fadeTime 변수에 대한 툴팁
        private float fadeTime;
        [SerializeField, Tooltip("컨테이너 페이드 아웃 애니메이션에 사용될 이징(Easing) 타입")] // fadeEasing 변수에 대한 툴팁
        private Ease.Type fadeEasing; // Ease.Type 열거형은 외부 정의가 필요합니다.

        // 텍스트가 특정 트랜스폼을 따라갈지 여부 및 관련 정보
        private Transform targetTransform; // 따라갈 목표 트랜스폼
        private Vector3 targetOffset; // 목표 트랜스폼으로부터의 상대적인 오프셋
        private bool fixToTarget; // 목표 트랜스폼을 따라가는 모드 활성화 여부

        /// <summary>
        /// 모든 Update 함수가 호출된 후 프레임이 끝날 때 호출됩니다.
        /// fixToTarget 모드가 활성화된 경우, 목표 트랜스폼의 위치에 오프셋을 더한 위치로 현재 오브젝트를 이동시킵니다.
        /// </summary>
        private void LateUpdate()
        {
            // fixToTarget 모드가 활성화된 경우
            if (fixToTarget)
                // 현재 오브젝트의 위치를 목표 트랜스폼 위치 + 목표 오프셋으로 설정
                transform.position = targetTransform.position + targetOffset;
        }

        /// <summary>
        /// 표시될 아이콘과 텍스트/아이콘의 색상을 설정합니다.
        /// </summary>
        /// <param name="icon">표시할 아이콘 스프라이트</param>
        /// <param name="color">아이콘과 텍스트에 적용할 색상</param>
        public void SetIconAndColor(Sprite icon, Color color)
        {
            // Image 컴포넌트의 스프라이트 및 색상 설정
            iconImage.sprite = icon;
            iconImage.color = color;

            // Text 컴포넌트의 색상 설정
            floatingText.color = color;
        }

        /// <summary>
        /// 떠오르는 텍스트(및 아이콘)를 활성화하고 업그레이드 획득 애니메이션을 시작합니다.
        /// FloatingTextBaseBehavior 추상 클래스의 메서드를 오버라이드합니다.
        /// </summary>
        /// <param name="text">표시할 텍스트 내용 (예: 업그레이드 이름)</param>
        /// <param name="scaleMultiplier">컨테이너 크기 애니메이션에 적용할 배율</param>
        /// <param name="color">텍스트와 아이콘의 색상 (SetIconAndColor 메서드에서 설정 가능)</param>
        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            // 목표 따라가기 모드를 비활성화합니다.
            fixToTarget = false;

            // Text 컴포넌트에 텍스트 내용 설정
            floatingText.text = text;

            // 컨테이너의 알파(투명도)를 1.0(완전히 불투명)으로 설정하여 보이게 함
            containerCanvasGroup.alpha = 1.0f;

            // 컨테이너의 초기 로컬 스케일을 0으로 설정하여 작게 시작하도록 함
            containerTransform.localScale = Vector3.zero;
            // 컨테이너의 로컬 스케일을 Vector3.one * scaleMultiplier 값으로 scaleTime 동안 크기 애니메이션 적용 (DOScale 메서드는 Watermelon 라이브러리 기능으로 가정합니다.)
            // 애니메이션 커브를 사용하여 크기 변화 속도 제어 (SetCurveEasing 메서드는 Watermelon 라이브러리 기능으로 가정합니다.)
            containerTransform.DOScale(Vector3.one * scaleMultiplier, scaleTime).SetCurveEasing(scaleAnimationCurve);

            // 컨테이너의 알파(투명도)를 0.0(완전히 투명)으로 fadeTime 동안 페이드 아웃 애니메이션 적용 (DOFade 메서드는 Watermelon 라이브러리 기능으로 가정합니다.)
            containerCanvasGroup.DOFade(0.0f, fadeTime).SetEasing(fadeEasing);

            // 컨테이너의 초기 로컬 위치를 0으로 설정
            containerTransform.localPosition = Vector3.zero;
            // 컨테이너를 offset 로컬 위치로 time 동안 이동 애니메이션 적용 (DOLocalMove 메서드는 Watermelon 라이브러리 기능으로 가정합니다.)
            // 애니메이션 완료 시 오브젝트 비활성화 및 부모 해제
            containerTransform.DOLocalMove(offset, time).SetEasing(easing).OnComplete(delegate // OnComplete 메서드는 Watermelon 라이브러리 기능으로 가정합니다.
            {
                gameObject.SetActive(false); // 게임 오브젝트 비활성화
                transform.SetParent(null); // 부모 오브젝트 해제 (월드 공간으로 이동)
            });

            // 색상 설정은 SetIconAndColor 메서드를 통해 별도로 호출해야 합니다.
            // SetIconAndColor(iconSprite, color); // 필요하다면 이 줄 추가
        }

        /// <summary>
        /// 이 떠오르는 텍스트를 특정 목표 트랜스폼의 위치에 고정하고 따라가도록 설정합니다.
        /// LateUpdate에서 이 설정에 따라 위치가 업데이트됩니다.
        /// </summary>
        /// <param name="target">따라갈 목표 트랜스폼</param>
        /// <param name="offset">목표 트랜스폼으로부터의 상대적인 오프셋</param>
        public void FixToTarget(Transform target, Vector3 offset)
        {
            // 목표 따라가기 모드를 활성화합니다.
            fixToTarget = true;

            // 목표 오프셋과 목표 트랜스폼을 저장합니다.
            targetOffset = offset;
            targetTransform = target;
        }
    }
}