/*
📄 UIRoomIndicator.cs 요약
스테이지 또는 방(Room) 클리어 여부를 UI에서 시각적으로 표시하는 컴포넌트야.

🧩 주요 기능
roomCompleteIndicator 오브젝트를 통해 방이 클리어 되었는지 여부를 표시함.

Init() 함수에서는 초기 상태로 비활성화.

SetAsReached() 함수가 호출되면 클리어 표시 활성화.

⚙️ 사용 용도
게임 내 던전/스테이지 진행 상태를 UI에 표시할 때 사용됨.

예를 들어, 스테이지 선택 UI에서 현재 도달한 방 또는 클리어한 방에 체크 표시처럼 시각적 마킹을 줄 수 있어.
*/

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class UIRoomIndicator : MonoBehaviour
    {
        [SerializeField] GameObject roomCompleteIndicator;

        public void Init()
        {
            roomCompleteIndicator.SetActive(false);
        }

        public void SetAsReached()
        {
            roomCompleteIndicator.SetActive(true);
        }
    }
}