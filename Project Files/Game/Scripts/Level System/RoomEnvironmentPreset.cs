// RoomEnvironmentPreset.cs
// 이 스크립트는 방 환경의 사전 설정(프리셋) 데이터를 정의합니다.
// 프리셋 이름, 포함될 아이템 엔티티, 플레이어 스폰 위치, 출구 지점 위치 정보를 포함합니다.
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // 이 클래스는 Unity 에디터에서 직렬화 및 편집 가능하도록 설정됩니다.
    [System.Serializable]
    public class RoomEnvironmentPreset
    {
        [SerializeField, Tooltip("환경 프리셋의 이름")] // name 변수에 대한 툴팁
        private string name;

        [SerializeField, Tooltip("이 환경 프리셋에 포함될 아이템 엔티티 데이터 배열")] // itemEntities 변수에 대한 툴팁
        private ItemEntityData[] itemEntities; // ItemEntityData 클래스는 외부 정의가 필요합니다.

        [SerializeField, Tooltip("이 환경 프리셋에서 플레이어가 스폰될 위치")] // spawnPos 변수에 대한 툴팁
        private Vector3 spawnPos;

        [SerializeField, Tooltip("이 환경 프리셋에서 출구 지점이 위치할 위치")] // exitPointPos 변수에 대한 툴팁
        private Vector3 exitPointPos;
    }
}