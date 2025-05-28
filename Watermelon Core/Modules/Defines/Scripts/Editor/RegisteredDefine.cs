// 스크립트 기능 요약:
// 이 스크립트는 코드의 DefineAttribute를 통해 등록되는 정의 심볼(Define Symbol)의 정보를 저장하는 데이터 구조입니다.
// 어떤 정의 심볼이 어떤 어셈블리 또는 타입과 관련되어 있는지를 연결하는 역할을 합니다.
// 주로 DefineManager와 같은 다른 스크립트에서 자동 정의 심볼을 관리하고 확인할 때 사용됩니다.

using UnityEngine; // Tooltip 속성 사용을 위해 필요

namespace Watermelon
{
    // RegisteredDefine 클래스는 DefineAttribute를 통해 등록된 정의 심볼의 정보를 저장합니다.
    public class RegisteredDefine
    {
        // Define: 등록된 정의 심볼의 이름입니다. 예: "MODULE_MONETIZATION"
        [Tooltip("등록된 정의 심볼의 이름")]
        public string Define { get; private set; }
        // AssemblyType: 해당 정의 심볼과 관련된 어셈블리 또는 타입의 이름입니다.
        // 이를 통해 특정 모듈이나 기능이 프로젝트에 존재하는지 확인할 수 있습니다.
        [Tooltip("정의 심볼과 관련된 어셈블리 또는 타입 이름")]
        public string AssemblyType { get; private set; }

        /// <summary>
        /// RegisteredDefine 클래스의 생성자입니다.
        /// 정의 심볼 이름과 관련 어셈블리/타입 이름을 직접 지정하여 새로운 RegisteredDefine 객체를 생성합니다.
        /// </summary>
        /// <param name="define">등록할 정의 심볼의 이름</param>
        /// <param name="assemblyType">정의 심볼과 관련된 어셈블리 또는 타입의 이름</param>
        public RegisteredDefine(string define, string assemblyType)
        {
            Define = define;
            AssemblyType = assemblyType;
        }

        /// <summary>
        /// RegisteredDefine 클래스의 생성자입니다.
        /// DefineAttribute 객체로부터 정보를 가져와 새로운 RegisteredDefine 객체를 생성합니다.
        /// </summary>
        /// <param name="defineAttribute">정보를 가져올 DefineAttribute 객체</param>
        public RegisteredDefine(DefineAttribute defineAttribute)
        {
            Define = defineAttribute.Define;
            AssemblyType = defineAttribute.AssemblyType;
        }
    }
}