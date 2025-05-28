// ProjectInitSettings.cs
// 이 스크립트는 프로젝트 전반의 초기화 설정을 관리하는 ScriptableObject입니다.
// Initializer가 게임 시작 시 실행할 InitModule 목록을 포함하며,
// Initializer에 의해 호출되어 모든 모듈을 초기화하고 실행합니다.

#pragma warning disable 0649 // 사용되지 않는 필드에 대한 경고를 비활성화합니다. (SerializeField 필드에 사용)

using UnityEngine;

namespace Watermelon
{
    // 프로젝트 초기화 설정을 담는 ScriptableObject입니다.
    public class ProjectInitSettings : ScriptableObject
    {
        [Tooltip("게임 초기화 시 실행될 InitModule 목록입니다.")]
        [SerializeField] InitModule[] modules; // 초기화 모듈 배열입니다.
        // 이 프로젝트에서 사용되는 초기화 모듈 목록을 가져옵니다.
        public InitModule[] Modules => modules;

        /// <summary>
        /// ProjectInitSettings를 초기화하고 포함된 모든 InitModule을 실행하는 함수입니다.
        /// Initializer에 의해 게임 시작 시 호출됩니다.
        /// </summary>
        /// <param name="initializer">초기화 프로세스를 관리하는 Initializer 인스턴스</param>
        public void Init(Initializer initializer)
        {
            // 포함된 InitModule 목록을 순회하며 각 모듈을 실행합니다.
            for (int i = 0; i < modules.Length; i++)
            {
                // 모듈이 null이 아니면 해당 모듈의 CreateComponent() 함수를 호출합니다.
                if(modules[i] != null)
                {
                    modules[i].CreateComponent();
                }
            }
        }

        /// <summary>
        /// 포함된 InitModule 목록에서 지정된 타입의 InitModule을 찾아 반환하는 제네릭 함수입니다.
        /// </summary>
        /// <typeparam name="T">찾고자 하는 InitModule의 타입</typeparam>
        /// <returns>지정된 타입의 InitModule 인스턴스, 없으면 null</returns>
        public T GetModule<T>() where T : InitModule
        {
            // 포함된 InitModule 목록을 순회합니다.
            foreach (var module in modules)
            {
                // 모듈이 null이 아니고 지정된 타입과 일치하면 해당 모듈을 반환합니다.
                if (module != null && module is T)
                {
                    return (T)module;
                }
            }

            // 지정된 타입의 모듈을 찾지 못하면 null을 반환합니다.
            return null;
        }
    }
}