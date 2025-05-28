// 이 스크립트는 간단한 형태의 레벨 미리보기 동작을 정의합니다.
// LevelPreviewBaseBehaviour를 상속받지만, 현재는 특별한 시각적 요소나 복잡한 로직 없이
// 기본 메소드들을 빈 구현으로 오버라이드하고 있습니다.
namespace Watermelon.SquadShooter
{
    // 간단한 레벨 미리보기 동작을 담당하는 클래스입니다.
    // LevelPreviewBaseBehaviour의 추상 메소드들을 오버라이드하지만, 현재는 구체적인 기능 구현은 포함하고 있지 않습니다.
    public class LevelPreviewSimpleBehaviour : LevelPreviewBaseBehaviour
    {
        // 간단한 레벨 미리보기를 초기화하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 초기화 로직을 추가할 수 있습니다.
        public override void Init()
        {
            // 초기화 로직 (필요하다면 추가)
        }

        // 간단한 레벨 미리보기를 활성화하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 활성화 로직을 추가할 수 있습니다.
        // animate: 활성화 시 애니메이션을 적용할지 여부 (현재 미사용)
        public override void Activate(bool animate = false)
        {
            // 활성화 로직 (필요하다면 추가)
        }

        // 간단한 레벨 미리보기를 완료 상태로 설정하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 완료 로직을 추가할 수 있습니다.
        public override void Complete()
        {
            // 완료 로직 (필요하다면 추가)
        }

        // 간단한 레벨 미리보기를 잠금 상태로 설정하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 잠금 로직을 추가할 수 있습니다.
        public override void Lock()
        {
            // 잠금 로직 (필요하다면 추가)
        }
    }
}