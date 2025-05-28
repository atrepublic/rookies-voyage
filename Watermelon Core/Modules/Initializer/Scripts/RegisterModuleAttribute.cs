// RegisterModuleAttribute.cs
// 이 스크립트는 InitModule 클래스에 메타데이터를 추가하기 위한 사용자 정의 속성(Attribute)을 정의합니다.
// 이 속성을 사용하여 InitModule의 이름, 코어 모듈 여부, 초기화 순서 등을 지정할 수 있습니다.
// 이는 Initializer 시스템에서 InitModule을 인식하고 관리하는 데 사용됩니다.

using System;

namespace Watermelon
{
    // 이 속성이 클래스에만 적용될 수 있으며, 한 클래스에 여러 번 적용될 수 없도록 지정합니다.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterModuleAttribute : Attribute
    {
        // 이 모듈의 경로 또는 이름을 나타냅니다.
        public string Path { get; private set; }
        // 이 모듈이 코어 시스템의 일부인지 여부를 나타냅니다.
        public bool Core { get; private set; }
        // 초기화 시 이 모듈이 실행될 순서를 나타냅니다. 숫자가 낮을수록 먼저 실행됩니다.
        public int Order { get; private set; }

        /// <summary>
        /// RegisterModuleAttribute 클래스의 생성자입니다.
        /// 모듈의 경로, 코어 모듈 여부, 초기화 순서를 설정합니다.
        /// </summary>
        /// <param name="path">모듈의 경로 또는 이름</param>
        /// <param name="core">코어 모듈인지 여부 (기본값: false)</param>
        /// <param name="order">초기화 순서 (기본값: 0)</param>
        public RegisterModuleAttribute(string path, bool core = false, int order = 0)
        {
            // 생성자 매개변수로 받은 값을 속성의 해당 속성에 할당합니다.
            Path = path;
            Core = core;
            Order = order;
        }
    }
}