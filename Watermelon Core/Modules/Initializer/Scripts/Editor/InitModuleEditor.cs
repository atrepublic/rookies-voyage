// InitModuleEditor.cs
// 이 스크립트는 모든 InitModuleEditor의 추상 기본 클래스를 정의합니다.
// InitModule Editor들이 상속받아 구현할 수 있는 공통적인 에디터 기능을 위한 가상(virtual) 메서드들을 포함합니다.
// 예를 들어, 모듈 생성/제거 시 로직, 커스텀 버튼, 메뉴 항목 준비 등의 기능을 위한 확장 포인트를 제공합니다.

using UnityEditor;

namespace Watermelon
{
    // InitModule 에디터들을 위한 추상 기본 클래스입니다. CustomInspector를 상속받아 커스텀 인스펙터 기능을 활용합니다.
    public abstract class InitModuleEditor : CustomInspector
    {
        /// <summary>
        /// 이 InitModule Editor가 연결된 InitModule 객체가 처음 생성될 때 호출되는 가상 함수입니다.
        /// 파생 클래스에서 InitModule 객체 생성 시 필요한 초기화 로직을 구현할 수 있습니다.
        /// </summary>
        public virtual void OnCreated() { }

        /// <summary>
        /// 이 InitModule Editor가 연결된 InitModule 객체가 제거될 때 호출되는 가상 함수입니다.
        /// 파생 클래스에서 InitModule 객체 제거 시 필요한 정리 로직을 구현할 수 있습니다.
        /// </summary>
        public virtual void OnRemoved() { }

        /// <summary>
        /// InitModule Editor의 인스펙터 GUI에 커스텀 버튼을 추가하기 위해 호출되는 가상 함수입니다.
        /// 파생 클래스에서 GUILayout.Button 등을 사용하여 추가 버튼 UI를 그릴 수 있습니다.
        /// </summary>
        public virtual void Buttons() { }

        /// <summary>
        /// InitModule Editor의 컨텍스트 메뉴(우클릭 메뉴) 항목을 준비하기 위해 호출되는 가상 함수입니다.
        /// 파생 클래스에서 genericMenu에 커스텀 메뉴 항목을 추가할 수 있습니다.
        /// </summary>
        /// <param name="genericMenu">메뉴 항목을 추가할 GenericMenu 참조</param>
        public virtual void PrepareMenuItems(ref GenericMenu genericMenu) { }
    }
}