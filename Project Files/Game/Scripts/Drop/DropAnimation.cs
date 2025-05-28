// 스크립트 설명: 드롭 아이템의 낙하 애니메이션 설정을 정의하는 클래스입니다.
// 낙하 스타일, 애니메이션 커브, 시간, 오프셋, 반경 등의 정보를 포함합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // Unity 에디터에서 인스펙터 창에 표시될 수 있도록 직렬화 가능하게 설정
    [System.Serializable]
    public class DropAnimation
    {
        [SerializeField]
        [Tooltip("드롭 아이템의 낙하 스타일")] // 주요 변수 한글 툴팁
        DropFallingStyle fallStyle; // 낙하 스타일
        // 낙하 스타일에 접근하기 위한 프로퍼티
        public DropFallingStyle FallStyle => fallStyle;

        [SerializeField]
        [Tooltip("아이템의 수평 이동 애니메이션 커브")] // 주요 변수 한글 툴팁
        AnimationCurve fallAnimationCurve; // 수평 낙하 애니메이션 커브
        // 수평 낙하 애니메이션 커브에 접근하기 위한 프로퍼티
        public AnimationCurve FallAnimationCurve => fallAnimationCurve;

        [SerializeField]
        [Tooltip("아이템의 수직 이동 애니메이션 커브")] // 주요 변수 한글 툴팁
        AnimationCurve fallYAnimationCurve; // 수직 낙하 애니메이션 커브
        // 수직 낙하 애니메이션 커브에 접근하기 위한 프로퍼티
        public AnimationCurve FallYAnimationCurve => fallYAnimationCurve;

        [SerializeField]
        [Tooltip("낙하 애니메이션 재생 시간")] // 주요 변수 한글 툴팁
        float fallTime; // 낙하 애니메이션 재생 시간
        // 낙하 시간에 접근하기 위한 프로퍼티
        public float FallTime => fallTime;

        [SerializeField]
        [Tooltip("아이템이 생성될 때 추가되는 수직 오프셋")] // 주요 변수 한글 툴팁
        float offsetY; // 생성 시 수직 오프셋
        // 수직 오프셋에 접근하기 위한 프로퍼티
        public float OffsetY => offsetY;

        [SerializeField]
        [Tooltip("아이템이 착지할 목표 지점의 무작위 반경")] // 주요 변수 한글 툴팁
        float radius; // 착지 지점 무작위 반경
        // 반경에 접근하기 위한 프로퍼티
        public float Radius => radius;
    }
}