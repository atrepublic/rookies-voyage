// ==============================================
// CharacterDynamicAnimation.cs
// ==============================================
// 캐릭터 선택 패널에서 사용되는 UI 애니메이션 정보를 담는 데이터 클래스입니다.
// 특정 캐릭터가 해금되었을 때 실행할 애니메이션을 정의합니다.

using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterDynamicAnimation
    {
        [Tooltip("애니메이션을 실행할 캐릭터 패널 UI")]
        public CharacterPanelUI CharacterPanel;

        [Tooltip("애니메이션 실행 전 지연 시간 (초)")]
        public float Delay;

        [Tooltip("애니메이션 시작 시 호출할 콜백 함수")]
        public SimpleCallback OnAnimationStarted;

        // 생성자
        public CharacterDynamicAnimation(CharacterPanelUI characterPanel, float delay, SimpleCallback onAnimationStarted)
        {
            CharacterPanel = characterPanel;
            Delay = delay;
            OnAnimationStarted = onAnimationStarted;
        }
    }
}
