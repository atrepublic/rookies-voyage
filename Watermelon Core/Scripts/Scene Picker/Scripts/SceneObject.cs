// 스크립트 설명: Unity 에디터에서 씬 애셋을 직렬화하여 저장하고 관리하기 위한 클래스입니다.
// 씬 파일 자체의 참조와 함께 씬의 경로 및 이름을 저장하며, 빌드 설정 관련 정보를 처리합니다.
using UnityEngine; // Object 사용을 위한 네임스페이스
#if UNITY_EDITOR // Unity 에디터 환경에서만 UnityEditor 네임스페이스 사용 가능하도록 조건부 컴파일
using UnityEditor; // AssetDatabase 사용을 위한 네임스페이스
#endif

namespace Watermelon
{
    // 시스템이 직렬화/역직렬화 콜백을 받을 수 있도록 인터페이스 구현
    [System.Serializable] // Unity 인스펙터에 표시되도록 직렬화 가능하게 설정
    public class SceneObject : ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("Unity 프로젝트에서 이 클래스가 참조하는 씬 애셋 파일")] // 주요 변수 한글 툴팁
        Object scene; // 씬 애셋 참조

        [SerializeField]
        [Tooltip("이 씬 애셋의 프로젝트 내 상대 경로")] // 주요 변수 한글 툴팁
        string path; // 씬 파일 경로
        // 씬 경로에 접근하기 위한 프로퍼티
        public string Path => path;

        [SerializeField]
        [Tooltip("이 씬 애셋의 이름")] // 주요 변수 한글 툴팁
        string name; // 씬 이름
        // 씬 이름에 접근하기 위한 프로퍼티
        public string Name => name;

        /// <summary>
        /// 오브젝트가 역직렬화된 후에 호출됩니다. (현재 이 메서드에서는 별도의 로직 없음)
        /// </summary>
        public void OnAfterDeserialize()
        {
            // 역직렬화 후 필요한 로직을 여기에 추가할 수 있습니다.
        }

        /// <summary>
        /// 오브젝트가 직렬화되기 전에 호출됩니다.
        /// 씬 애셋 참조가 유효하면 씬의 이름과 경로를 저장합니다. 유효하지 않으면 초기화합니다.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // 씬 애셋 참조가 null이 아니면
            if(scene != null)
            {
                name = scene.name; // 씬 이름 저장

#if UNITY_EDITOR // 에디터 환경에서만 AssetDatabase 사용 가능
                path = AssetDatabase.GetAssetPath(scene); // 씬 파일 경로 저장
#endif

                return; // 저장 완료 후 함수 종료
            }

            // 씬 애셋 참조가 null이면 이름과 경로 초기화
            name = "";
            path = "";
        }
    }
}