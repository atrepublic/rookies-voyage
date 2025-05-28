// InitModule.cs
// 이 스크립트는 게임 초기화 시스템에서 사용될 모든 초기화 모듈의 추상 기본 클래스를 정의합니다.
// 각 초기화 모듈은 고유한 이름과 컴포넌트 생성 로직을 가져야 합니다.

using UnityEngine;

namespace Watermelon
{
    // 게임 초기화 모듈의 추상 기본 클래스입니다. ScriptableObject를 상속받아 에셋으로 관리될 수 있습니다.
    public abstract class InitModule : ScriptableObject
    {
        /// <summary>
        /// 이 초기화 모듈의 이름을 가져옵니다. 각 모듈은 고유한 이름을 반환해야 합니다.
        /// </summary>
        // 모듈의 이름을 정의하는 추상 속성입니다. 파생 클래스에서 구현해야 합니다.
        public abstract string ModuleName { get; }

        /// <summary>
        /// 이 초기화 모듈과 관련된 게임 오브젝트 또는 컴포넌트를 생성하고 설정하는 추상 함수입니다.
        /// Initializer에 의해 게임 초기화 과정 중에 호출됩니다.
        /// </summary>
        // 이 모듈과 관련된 컴포넌트를 생성하는 추상 메서드입니다. 파생 클래스에서 구현해야 합니다.
        public abstract void CreateComponent();
    }
}