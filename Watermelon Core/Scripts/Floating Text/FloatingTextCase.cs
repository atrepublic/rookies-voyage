// FloatingTextCase.cs
// 이 스크립트는 부동 텍스트(FloatingText)를 관리하는 클래스로, 이름 식별, 동작 스크립트와 오브젝트 풀 초기화를 담당합니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class FloatingTextCase
    {
        [SerializeField]
        [Tooltip("부동 텍스트 케이스의 고유 이름(식별용)")]
        private string name;

        /// <summary>
        /// 부동 텍스트 케이스의 이름을 반환합니다.
        /// </summary>
        public string Name => name;

        [SerializeField]
        [Tooltip("부동 텍스트 동작을 수행하는 베이스 컴포넌트 참조")]
        private FloatingTextBaseBehavior floatingTextBehavior;

        /// <summary>
        /// 부동 텍스트 동작을 수행하는 베이스 컴포넌트를 반환합니다.
        /// </summary>
        public FloatingTextBaseBehavior FloatingTextBehavior => floatingTextBehavior;

        [System.NonSerialized]
        private Pool floatingTextPool;

        /// <summary>
        /// 부동 텍스트 오브젝트 풀을 반환합니다.
        /// </summary>
        public Pool FloatingTextPool => floatingTextPool;

        /// <summary>
        /// 부동 텍스트 오브젝트 풀을 초기화합니다.
        /// </summary>
        public void Init()
        {
            // Pool 생성: floatingTextBehavior가 붙은 게임오브젝트를 기반으로 풀 생성
            floatingTextPool = new Pool(floatingTextBehavior.gameObject, $"FloatingText_{name}");
            //floatingTextPool = new Pool(floatingTextBehavior.gameObject);
        }
    }
}
