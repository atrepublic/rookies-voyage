/*
📄 TabAnimation.cs 요약
UI 탭(Tab)의 위치를 Vector2.LerpUnclamped()로 자연스럽게 이동시키는 애니메이션 클래스야.
TweenCase를 상속해서 트윈 시스템(Tween Animation)의 일부로 작동해.

🧩 주요 기능
RectTransform 객체의 anchoredPosition을 기준으로 탭 전환 시 부드러운 이동 애니메이션을 구현.

startValue에서 resultValue로 보간(Lerp)된 위치로 움직이며,
애니메이션이 끝나면 start와 result 값을 서로 바꿔서 반대 방향으로도 반복 가능해.

⚙️ 사용 용도
탭 UI 전환 애니메이션 (예: 캐릭터 선택, 스킬 페이지, 상점 탭 등)에서
좌우 이동 효과를 줄 때 사용됨.

특정 시점에서 탭을 열고 닫는 방식의 UI를 만들고 싶을 때도 적합함.
*/

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class TabAnimation : TweenCase
    {
        public RectTransform tweenObject;

        public Vector2 startValue;
        public Vector2 resultValue;

        public TabAnimation(RectTransform tweenObject, Vector2 resultValue)
        {
            this.resultValue = resultValue;
            this.tweenObject = tweenObject;

            startValue = tweenObject.anchoredPosition;

            parentObject = tweenObject.gameObject;
        }

        public override bool Validate()
        {
            return parentObject != null;
        }

        public override void DefaultComplete()
        {
            tweenObject.anchoredPosition = resultValue;
        }

        public override void Invoke(float deltaTime)
        {
            tweenObject.anchoredPosition = Vector2.LerpUnclamped(startValue, resultValue, Interpolate(state));

            if (state >= 1.0f)
            {
                state = 0;
                isCompleted = false;

                Vector2 tempStartValue = startValue;

                startValue = resultValue;
                resultValue = tempStartValue;
            }
        }
    }
}