// 스크립트 기능 요약:
// 이 스크립트는 오브젝트 풀링 시스템에서 사용되는 풀(Pool) 객체가 구현해야 할 인터페이스를 정의합니다.
// 특정 타입의 오브젝트를 풀링하기 위한 필수적인 기능들(초기화, 오브젝트 가져오기, 오브젝트 생성, 모두 반환, 비우기)을 명시하여
// 다양한 풀 구현체가 일관된 방식으로 작동하도록 보장합니다.

using UnityEngine;

namespace Watermelon
{
    // IPool 인터페이스는 모든 풀 구현체가 따라야 할 계약을 정의합니다.
    public interface IPool
    {
        // Name: 풀의 고유한 이름을 가져오는 속성입니다.
        public string Name { get; }

        /// <summary>
        /// 풀을 초기화하는 함수입니다.
        /// 풀 사용 전에 반드시 호출되어야 합니다.
        /// </summary>
        public void Init();

        /// <summary>
        /// 풀에서 사용 가능한 오브젝트 하나를 가져오는 함수입니다.
        /// 사용 가능한 오브젝트가 없으면 새로 생성하거나 (최대 크기 제한이 없는 경우) null을 반환할 수 있습니다.
        /// </summary>
        /// <returns>풀링된 GameObject 또는 사용 가능한 오브젝트가 없으면 null</returns>
        public GameObject GetPooledObject();

        /// <summary>
        /// 풀에 지정된 개수만큼의 오브젝트를 미리 생성하여 채워두는 함수입니다.
        /// 게임 시작 시 미리 오브젝트를 생성하여 런타임 부하를 줄일 수 있습니다.
        /// </summary>
        /// <param name="count">미리 생성할 오브젝트의 개수</param>
        public void CreatePoolObjects(int count);

        /// <summary>
        /// 현재 풀에서 사용 중인 모든 오브젝트를 풀로 반환(비활성화)하는 함수입니다.
        /// </summary>
        /// <param name="resetParent">true이면 오브젝트의 부모를 풀의 기본 컨테이너로 재설정합니다.</param>
        public void ReturnToPoolEverything(bool resetParent = false);

        /// <summary>
        /// 풀에 있는 모든 오브젝트를 파괴하고 풀을 비우는 함수입니다.
        /// 풀 사용이 끝날 때 리소스를 해제하는 데 사용될 수 있습니다. 성능 부하가 클 수 있으므로 주의해서 사용해야 합니다.
        /// </summary>
        public void Clear();
    }
}