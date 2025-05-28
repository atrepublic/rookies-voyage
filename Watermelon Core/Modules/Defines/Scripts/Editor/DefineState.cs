// 스크립트 기능 요약:
// 이 스크립트는 특정 정의 심볼(Define Symbol)의 이름과 해당 심볼의 활성화 상태를 저장하는 데 사용되는 간단한 데이터 구조입니다.
// 주로 DefineManager와 같은 다른 스크립트에서 정의 심볼의 현재 상태를 추적하고 관리하기 위해 사용됩니다.
// [System.Serializable] 속성을 통해 Unity 에디터에서 직렬화되어 인스펙터 창 등에 표시될 수 있습니다.

using UnityEngine; // Tooltip 속성 사용을 위해 필요

namespace Watermelon
{
    // DefineState 클래스는 정의 심볼의 이름과 상태를 저장하는 직렬화 가능한 데이터 클래스입니다.
    [System.Serializable]
    public class DefineState
    {
        // define: 정의 심볼의 이름을 저장하는 문자열 변수입니다. 예: "UNITY_EDITOR"
        [Tooltip("정의 심볼의 이름")]
        private string define;
        // Define 속성: define 변수의 값을 읽기 전용으로 제공합니다.
        public string Define => define;

        // state: 해당 정의 심볼이 현재 활성화(true) 상태인지 비활성화(false) 상태인지를 저장하는 불리언 변수입니다.
        [Tooltip("정의 심볼의 활성화 상태")]
        private bool state;
        // State 속성: state 변수의 값을 읽기 전용으로 제공합니다.
        public bool State => state;

        /// <summary>
        /// DefineState 클래스의 생성자입니다.
        /// 새로운 DefineState 객체를 생성할 때 정의 심볼의 이름과 초기 상태를 설정합니다.
        /// </summary>
        /// <param name="define">설정할 정의 심볼의 이름</param>
        /// <param name="state">설정할 정의 심볼의 초기 활성화 상태</param>
        public DefineState(string define, bool state)
        {
            this.define = define;
            this.state = state;
        }
    }
}