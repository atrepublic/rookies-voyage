// FloatingTextFollowBehaviour.cs
// 이 스크립트는 특정 위치를 따라가면서 떠오르는 텍스트의 동작을 제어합니다.
// 주로 게임 내에서 오브젝트의 위치 변화에 따라 텍스트가 함께 이동하고 애니메이션되는 경우에 사용됩니다.
using TMPro; // TextMesh Pro를 사용하기 위해 필요
using UnityEngine;
using Watermelon; // Tweening 및 Ease 기능을 위해 필요 (Watermelon 라이브러리는 외부 정의가 필요합니다.)

namespace Watermelon.SquadShooter // SquadShooter 네임스페이스에 포함
{
    // 떠오르는 텍스트의 기본 동작을 정의하는 추상 클래스 FloatingTextBaseBehavior를 상속받습니다. (FloatingTextBaseBehavior는 외부 정의가 필요합니다.)
    public class FloatingTextFollowBehaviour : FloatingTextBaseBehavior
    {
        [SerializeField, Tooltip("떠오르는 텍스트를 표시할 TextMeshProUGUI 컴포넌트")] // floatingText 변수에 대한 툴팁
        private TextMeshProUGUI floatingText;

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("텍스트가 최종적으로 이동할 목표 위치에서의 상대적인 오프셋")] // offset 변수에 대한 툴팁
        private Vector3 offset;
        [SerializeField, Tooltip("텍스트 이동 애니메이션에 걸리는 시간 (초)")] // time 변수에 대한 툴팁
        private float time;
        [SerializeField, Tooltip("텍스트 이동 애니메이션에 사용될 이징(Easing) 타입")] // easing 변수에 대한 툴팁
        private Ease.Type easing; // Ease.Type 열거형은 외부 정의가 필요합니다.

        [Space] // 에디터에서 시각적인 간격 조절
        [SerializeField, Tooltip("텍스트 크기 애니메이션에 걸리는 시간 (초)")] // scaleTime 변수에 대한 툴팁
        private float scaleTime;
        [SerializeField, Tooltip("텍스트 크기 애니메이션에 사용될 이징(Easing) 타입")] // scaleEasing 변수에 대한 툴팁
        private Ease.Type scaleEasing; // Ease.Type 열거형은 외부 정의가 필요합니다.

        // 텍스트의 기본 스케일 값을 저장
        private Vector3 defaultScale;

        /// <summary>
        /// 스크립트 인스턴스가 로드될 때 호출됩니다.
        /// 텍스트의 기본 스케일 값을 저장합니다.
        /// </summary>
        private void Awake()
        {
            // 현재 오브젝트의 로컬 스케일을 기본 스케일로 저장
            defaultScale = transform.localScale;
        }

        /// <summary>
        /// 떠오르는 텍스트를 활성화하고 애니메이션을 시작합니다.
        /// FloatingTextBaseBehavior 추상 클래스의 메서드를 오버라이드합니다.
        /// </summary>
        /// <param name="text">표시할 텍스트 내용</param>
        /// <param name="scaleMultiplier">텍스트 크기 애니메이션에 적용할 배율</param>
        /// <param name="color">텍스트의 색상 (현재 이 동작에서는 사용되지 않음)</param>
        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            // TextMeshProUGUI 컴포넌트에 텍스트 설정
            floatingText.text = text;

            // 초기 스케일을 0으로 설정하여 작게 시작하도록 함
            transform.localScale = Vector3.zero;
            // 기본 스케일에 배율을 곱한 값으로 scaleTime 동안 크기 애니메이션 적용 (DOScale 메서드는 Watermelon 라이브러리 기능으로 가정합니다.)
            transform.DOScale(defaultScale * scaleMultiplier, scaleTime).SetEasing(scaleEasing);

            // 현재 위치에서 offset만큼 이동하는 애니메이션을 time 동안 적용 (DOMove 메서드는 Watermelon 라이브러리 기능으로 가정합니다.)
            // 애니메이션 완료 시 오브젝트를 비활성화하는 콜백 함수 등록
            transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(delegate
            {
                gameObject.SetActive(false); // 게임 오브젝트 비활성화
            });
        }
    }
}