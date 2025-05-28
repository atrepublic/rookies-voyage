// 이 스크립트는 특정 레벨 유형에 대한 특별한 동작을 정의하기 위한 추상 ScriptableObject 기본 클래스입니다.
// 각 레벨의 생명주기 이벤트(초기화, 로드, 시작, 완료 등)에 반응하는 메소드들을 정의하며,
// 구체적인 레벨 특수 동작은 이 클래스를 상속받아 구현해야 합니다.
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // 특정 레벨 유형에 대한 고유한 동작을 정의하기 위한 추상 ScriptableObject 클래스입니다.
    // ScriptableObject로 생성되어 레벨 데이터와 함께 관리될 수 있습니다.
    public abstract class LevelSpecialBehaviour : ScriptableObject
    {
        // 레벨이 처음 초기화될 때 호출되는 메소드입니다.
        // 레벨 시작 전 필요한 설정이나 준비 작업을 수행합니다.
        public abstract void OnLevelInitialised();

        // 레벨이 로드될 때 호출되는 메소드입니다.
        // 씬 로드 후 필요한 오브젝트 참조 설정이나 상태 복원 등을 수행합니다.
        public abstract void OnLevelLoaded();
        // 레벨이 언로드될 때 호출되는 메소드입니다.
        // 씬 전환 등 레벨이 메모리에서 해제될 때 필요한 정리 작업을 수행합니다.
        public abstract void OnLevelUnloaded();

        // 레벨 플레이가 시작될 때 호출되는 메소드입니다.
        // 게임 플레이 시작 시 필요한 이벤트 발생이나 상태 설정을 수행합니다.
        public abstract void OnLevelStarted();
        // 레벨 목표 달성에 실패했을 때 호출되는 메소드입니다.
        // 실패 처리 로직을 수행합니다.
        public abstract void OnLevelFailed();
        // 레벨 목표를 성공적으로 달성했을 때 호출되는 메소드입니다.
        // 완료 처리 로직을 수행합니다.
        public abstract void OnLevelCompleted();

        // 플레이어가 새로운 방에 진입했을 때 호출되는 메소드입니다.
        // 방 진입 시 특정 동작이 필요할 때 사용합니다.
        public abstract void OnRoomEntered();
        // 플레이어가 현재 방에서 나갈 때 호출되는 메소드입니다.
        // 방 이탈 시 특정 동작이 필요할 때 사용합니다.
        public abstract void OnRoomLeaved();
    }
}