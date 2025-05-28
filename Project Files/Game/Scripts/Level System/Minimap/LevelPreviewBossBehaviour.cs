// 이 스크립트는 보스 레벨의 미리보기 동작을 정의합니다.
// LevelPreviewBaseBehaviour를 상속받아 보스 레벨 미리보기에 특화된 초기화, 활성화, 잠금, 완료 동작을 구현합니다.
// 현재는 기본 구현만 포함하고 있으며, 실제 동작은 이 클래스 내에서 추가되어야 합니다.
namespace Watermelon.SquadShooter
{
    // 보스 레벨 미리보기 동작을 담당하는 클래스입니다.
    // LevelPreviewBaseBehaviour의 추상 메소드들을 오버라이드하여 보스 레벨에 맞는 기능을 구현합니다.
    public class LevelPreviewBossBehaviour : LevelPreviewBaseBehaviour
    {
        // 보스 레벨 미리보기를 초기화하는 메소드입니다.
        // TODO: 보스 레벨 미리보기에 필요한 초기화 로직을 여기에 추가합니다.
        public override void Init()
        {

        }

        // 보스 레벨 미리보기를 활성화하는 메소드입니다.
        // TODO: 보스 레벨이 플레이 가능 상태가 될 때 필요한 활성화 로직을 여기에 추가합니다.
        // animate: 활성화 시 애니메이션을 적용할지 여부 (현재 미사용)
        public override void Activate(bool animate = false)
        {

        }

        // 보스 레벨 미리보기를 완료 상태로 설정하는 메소드입니다.
        // TODO: 보스 레벨이 완료되었을 때 필요한 로직을 여기에 추가합니다.
        public override void Complete()
        {

        }

        // 보스 레벨 미리보기를 잠금 상태로 설정하는 메소드입니다.
        // TODO: 보스 레벨이 해금되지 않았을 때 필요한 잠금 로직을 여기에 추가합니다.
        public override void Lock()
        {

        }
    }
}