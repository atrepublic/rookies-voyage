// 스크립트 설명: 게임 내 드롭 가능한 아이템들의 설정 정보를 담는 ScriptableObject입니다.
// 사용자 정의 드롭 아이템 목록과 드롭 애니메이션 설정을 포함합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // Unity 에디터에서 ScriptableObject로 생성할 수 있도록 메뉴 추가
    [CreateAssetMenu(fileName = "Dropable Item Settings", menuName = "Data/Dropable Item Settings")]
    public class DropableItemSettings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("사용자 정의 드롭 아이템 설정 목록")] // 주요 변수 한글 툴팁
        CustomDropItem[] customDropItems; // 사용자 정의 드롭 아이템 배열
        // 사용자 정의 드롭 아이템 목록에 접근하기 위한 프로퍼티
        public CustomDropItem[] CustomDropItems => customDropItems;

        [SerializeField]
        [Tooltip("드롭 애니메이션 설정 목록")] // 주요 변수 한글 툴팁
        DropAnimation[] dropAnimations; // 드롭 애니메이션 배열
        // 드롭 애니메이션 목록에 접근하기 위한 프로퍼티
        public DropAnimation[] DropAnimations => dropAnimations;
    }
}