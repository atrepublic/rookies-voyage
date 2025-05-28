// 스크립트 설명: 게임 내 상자(Chest)의 기본적인 데이터 정보를 담는 클래스입니다.
// 상자의 프리팹과 타입 정보를 포함합니다.
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // Unity 에디터에서 인스펙터 창에 표시될 수 있도록 직렬화 가능하게 설정
    [System.Serializable]
    public class ChestData
    {
        [SerializeField]
        [Tooltip("이 상자 데이터에 해당하는 상자 게임 오브젝트 프리팹")] // 주요 변수 한글 툴팁
        GameObject prefab; // 상자 프리팹
        // 상자 프리팹에 접근하기 위한 프로퍼티
        public GameObject Prefab => prefab;

        [SerializeField]
        [Tooltip("이 상자 데이터의 레벨 상자 타입")] // 주요 변수 한글 툴팁
        LevelChestType type; // 레벨 상자 타입
        // 레벨 상자 타입에 접근하기 위한 프로퍼티
        public LevelChestType Type => type; // LevelChestType은 Watermelon.LevelSystem 네임스페이스 또는 다른 곳에 정의된 것으로 가정

        /// <summary>
        /// 상자 데이터 초기화 시 호출됩니다. (현재 기능 없음)
        /// </summary>
        public void Init()
        {
            // 초기화 로직 (필요하다면 여기에 추가)
        }

        /// <summary>
        /// 상자 데이터 언로드 시 호출됩니다. (현재 기능 없음)
        /// </summary>
        public void Unload()
        {
            // 언로드 로직 (필요하다면 여기에 추가)
        }
    }
}