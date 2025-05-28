// ISaveObject.cs
// 이 스크립트는 게임 내에서 저장 가능한 모든 데이터 객체가 구현해야 하는 인터페이스를 정의합니다.
// 이 인터페이스를 구현하는 객체는 SaveController에 의해 저장/로드될 수 있습니다.

namespace Watermelon
{
    // 게임 저장 시스템에 의해 관리되는 모든 저장 가능한 객체가 구현해야 하는 인터페이스입니다.
    public interface ISaveObject
    {
        /// <summary>
        /// 저장 객체의 현재 상태를 직렬화 가능한 데이터 구조로 동기화하는 함수입니다.
        /// SaveController가 데이터를 저장하기 전에 이 함수를 호출하여 최신 상태를 반영하도록 합니다.
        /// </summary>
        public void Flush();
    }
}