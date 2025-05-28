// 이 스크립트는 레벨 미리보기 동작을 위한 추상 기본 클래스입니다.
// 각 레벨 미리보기 유형(기본, 보스 등)이 구현해야 할 공통 인터페이스를 정의합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 레벨 미리보기 동작의 기본 클래스입니다.
    // 각 레벨 미리보기 유형은 이 클래스를 상속받아 추상 메소드를 구현해야 합니다.
    public abstract class LevelPreviewBaseBehaviour : MonoBehaviour
    {
        // 레벨 미리보기를 초기화하는 메소드입니다.
        // 필요한 설정이나 초기 상태를 정의합니다.
        public abstract void Init();
        // 레벨 미리보기를 활성화하는 메소드입니다.
        // 레벨이 플레이 가능 상태가 될 때 호출됩니다. 애니메이션 효과를 선택적으로 적용할 수 있습니다.
        // animate: 활성화 시 애니메이션을 적용할지 여부
        public abstract void Activate(bool animate = false);
        // 레벨 미리보기를 잠금 상태로 설정하는 메소드입니다.
        // 해당 레벨이 아직 해금되지 않았을 때 호출됩니다.
        public abstract void Lock();
        // 레벨 미리보기를 완료 상태로 설정하는 메소드입니다.
        // 해당 레벨이 성공적으로 완료되었을 때 호출됩니다.
        public abstract void Complete();
    }
}