// 스크립트 기능 요약:
// 이 스크립트는 사용자 정의 Attribute인 DefineAttribute를 정의합니다.
// 이 Attribute는 스크립트 클래스에 적용하여 특정 스크립팅 정의 심볼(Scripting Define Symbol)과 해당 심볼이 활성화되기 위한 조건(관련 어셈블리 또는 타입)을 연결하는 데 사용됩니다.
// DefineManager와 같은 시스템에서 이 Attribute를 사용하여 프로젝트의 코드 기반으로 자동 정의 심볼 목록을 생성하고 관리할 수 있습니다.

using System;
using UnityEngine; // Tooltip 속성 사용을 위해 필요

namespace Watermelon
{
    // AttributeUsage 속성은 이 Attribute가 클래스에만 적용될 수 있으며, 하나의 클래스에 여러 번 적용될 수 있도록 설정합니다.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    // DefineAttribute 클래스는 Attribute를 상속받아 사용자 정의 Attribute로 작동합니다.
    public class DefineAttribute : Attribute
    {
        // Define: 이 Attribute와 연결될 스크립팅 정의 심볼의 이름을 저장하는 속성입니다.
        [Tooltip("이 Attribute와 연결될 스크립팅 정의 심볼의 이름")]
        public string Define { get; private set; }
        // AssemblyType: 이 정의 심볼이 활성화되기 위한 조건으로 사용될 어셈블리 또는 타입의 이름입니다.
        // 이 문자열에 해당하는 어셈블리 또는 타입이 프로젝트에 존재하면 DefineManager에서 해당 정의 심볼을 활성화합니다.
        // 비어있을 경우, DefineManager에서 단순히 이 정의 심볼의 존재를 인식하는 용도로 사용될 수 있습니다 (프로젝트 정의).
        [Tooltip("정의 심볼 활성화 조건으로 사용될 어셈블리 또는 타입의 전체 이름")]
        public string AssemblyType { get; private set; }

        /// <summary>
        /// DefineAttribute 클래스의 생성자입니다.
        /// 새로운 DefineAttribute를 생성할 때 연결할 정의 심볼의 이름과 관련 어셈블리/타입 이름을 설정합니다.
        /// </summary>
        /// <param name="define">이 Attribute와 연결될 정의 심볼의 이름</param>
        /// <param name="assemblyType">정의 심볼 활성화 조건으로 사용될 어셈블리 또는 타입의 전체 이름 (기본값: 빈 문자열)</param>
        public DefineAttribute(string define, string assemblyType = "")
        {
            Define = define; // 정의 심볼 이름 설정

            AssemblyType = assemblyType; // 관련 어셈블리/타입 이름 설정
        }
    }
}