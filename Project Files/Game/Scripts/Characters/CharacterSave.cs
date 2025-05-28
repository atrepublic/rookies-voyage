/*
 * CharacterSave.cs
 * ---------------------
 * 이 스크립트는 개별 캐릭터의 저장 데이터를 정의합니다.
 * 현재는 캐릭터의 업그레이드 레벨만 저장합니다.
 * ISaveObject 인터페이스를 구현하여 Watermelon 프레임워크의 저장 시스템과 연동됩니다.
 */

using Watermelon; // Watermelon 프레임워크 네임스페이스
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 직렬화 가능한 클래스로 선언하여 저장/로드 가능하게 함
    [System.Serializable]
    public class CharacterSave : ISaveObject // ISaveObject 인터페이스 구현
    {
        [Tooltip("캐릭터의 현재 업그레이드 레벨 (0부터 시작)")]
        public int UpgradeLevel = 0; // 기본값은 0

        /// <summary>
        /// ISaveObject 인터페이스 구현 함수.
        /// 저장이 실제로 디스크에 기록되기 직전에 호출될 수 있습니다. (현재는 사용 안 함)
        /// </summary>
        public void Flush()
        {
            // 필요시 저장 직전 처리 로직 추가
        }
    }
}