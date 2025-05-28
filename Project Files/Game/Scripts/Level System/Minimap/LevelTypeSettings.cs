// 이 스크립트는 특정 레벨 유형에 대한 설정을 담는 직렬화 가능한 클래스입니다.
// 레벨 유형과 해당 레벨에 사용될 미리보기 오브젝트 프리팹을 연결하는 데 사용됩니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 레벨 유형별 설정을 저장하는 직렬화 가능한 클래스입니다.
    // 이 클래스는 Unity 인스펙터에서 설정 값을 편집할 수 있도록 [System.Serializable] 어트리뷰트가 적용되었습니다.
    [System.Serializable]
    public class LevelTypeSettings
    {
        // 이 설정이 적용될 레벨의 유형입니다.
        [Tooltip("이 설정이 적용될 레벨의 유형입니다.")]
        [SerializeField] LevelType levelType;
        // 이 레벨 유형의 레벨에서 사용될 미리보기 오브젝트 프리팹입니다.
        [Tooltip("이 레벨 유형의 레벨에서 사용될 미리보기 오브젝트 프리팹입니다.")]
        [SerializeField] GameObject previewObject;

        // 이 설정의 레벨 유형을 가져오는 프로퍼티입니다.
        public LevelType LevelType => levelType;
        // 이 설정의 미리보기 오브젝트 프리팹을 가져오는 프로퍼티입니다.
        public GameObject PreviewObject => previewObject;

        // 레벨 유형 설정을 초기화하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 초기화 로직을 추가할 수 있습니다.
        public void Init()
        {
            // 초기화 로직 (필요하다면 추가)
        }

        // 레벨 유형 설정을 언로드하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 리소스 해제 등의 언로드 로직을 추가할 수 있습니다.
        public void Unload()
        {
            // 언로드 로직 (필요하다면 추가)
        }
    }
}