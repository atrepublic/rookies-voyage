// 이 스크립트는 캐릭터 애니메이션 이벤트를 처리하는 핸들러입니다.
// 애니메이션 클립에 포함된 이벤트를 통해 특정 함수를 호출하여 캐릭터의 동작과 연동합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 캐릭터 애니메이션 이벤트를 받아 처리하는 클래스입니다.
    // 주로 애니메이션 클립에서 특정 프레임에 호출되는 이벤트 함수를 구현합니다.
    public class CharacterAnimationHandler : MonoBehaviour
    {
        // 이 애니메이션 핸들러가 연결된 CharacterBehaviour 컴포넌트입니다.
        // 애니메이션 이벤트 발생 시 CharacterBehaviour의 메소드를 호출하는 데 사용됩니다.
        private CharacterBehaviour characterBehaviour;

        // 애니메이션 핸들러를 초기화하고 대상 CharacterBehaviour를 설정합니다.
        // characterBehaviour: 이 핸들러와 연결될 CharacterBehaviour 인스턴스
        public void Inititalise(CharacterBehaviour characterBehaviour)
        {
            this.characterBehaviour = characterBehaviour;
        }

        // 점프 애니메이션의 끝에서 호출되는 이벤트 함수입니다.
        // 애니메이션 클립의 이벤트 설정에 의해 호출됩니다.
        // 이 함수가 호출되면 CharacterBehaviour의 SpawnWeapon 메소드를 호출하여 무기를 생성합니다.
        public void JumpEnding()
        {
            // 연결된 characterBehaviour의 SpawnWeapon 메소드를 호출합니다.
            characterBehaviour.SpawnWeapon();
        }
    }
}