// LevelSave.cs
// 이 스크립트는 게임 진행 상황 중 레벨 관련 저장 데이터를 정의합니다.
// 현재 진행 중인 월드 및 레벨 인덱스, 마지막 완료 레벨에서의 코인 잔고 등의 정보를 저장합니다.
using UnityEngine;

namespace Watermelon.SquadShooter // SquadShooter 네임스페이스에 포함 (LevelSystem과 다름)
{
    // 이 클래스는 Unity 에디터에서 직렬화 및 저장/로드 가능하도록 설정됩니다.
    [System.Serializable]
    // ISaveObject 인터페이스를 상속받아 저장 시스템과 연동되도록 합니다. (ISaveObject 인터페이스는 외부 정의가 필요합니다.)
    public class LevelSave : ISaveObject
    {
        [Tooltip("현재 플레이어가 진행 중인 월드의 인덱스")] // WorldIndex 변수에 대한 툴팁
        public int WorldIndex;

        [Tooltip("현재 플레이어가 진행 중인 레벨의 인덱스")] // LevelIndex 변수에 대한 툴팁
        public int LevelIndex;

        [Tooltip("마지막으로 레벨을 완료했을 때의 코인 잔고")] // LastCompletedLevelCoinBalance 변수에 대한 툴팁
        public int LastCompletedLevelCoinBalance;

        /// <summary>
        /// LevelSave 클래스의 새로운 인스턴스를 초기화합니다.
        /// 기본값으로 첫 번째 월드와 레벨로 설정됩니다.
        /// </summary>
        public LevelSave()
        {
            WorldIndex = 0; // 첫 번째 월드로 초기화
            LevelIndex = 0; // 첫 번째 레벨로 초기화
        }

        /// <summary>
        /// 저장 데이터를 플러시(확정)하는 메서드입니다. (현재 구현에서는 비어 있습니다.)
        /// ISaveObject 인터페이스의 요구사항일 수 있습니다.
        /// </summary>
        public void Flush()
        {
            // 현재는 특별한 플러시 로직 없음
        }
    }
}