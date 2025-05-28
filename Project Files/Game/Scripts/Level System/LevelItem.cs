// LevelItem.cs
// 이 스크립트는 레벨 환경에 배치될 수 있는 아이템(예: 장애물, 환경 요소)의 데이터를 정의합니다.
// 아이템의 프리팹, 해시 값, 유형 정보를 포함하며, 오브젝트 풀링 기능을 위한 초기화/언로드 메서드를 제공합니다.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // 이 클래스는 Unity 에디터에서 직렬화 및 편집 가능하도록 설정됩니다.
    [System.Serializable]
    public class LevelItem
    {
        [SerializeField, Tooltip("이 레벨 아이템에 사용될 게임 오브젝트 프리팹")] // prefab 변수에 대한 툴팁
        private GameObject prefab;
        // 아이템 프리팹에 접근하기 위한 속성
        public GameObject Prefab => prefab;

        // 아이템을 식별하기 위한 해시 값 (에디터에서 숨김)
        [HideInInspector][SerializeField]
        private int hash;
        // 해시 값에 접근하기 위한 속성
        public int Hash => hash;

        [SerializeField, Tooltip("이 레벨 아이템의 유형 (예: 장애물, 환경 요소)")] // type 변수에 대한 툴팁
        private LevelItemType type; // LevelItemType 열거형은 외부 정의가 필요합니다.
        // 아이템 유형에 접근하기 위한 속성
        public LevelItemType Type => type;

        // 이 아이템을 위한 오브젝트 풀
        private Pool pool; // Pool 클래스는 외부 정의가 필요합니다.
        // 오브젝트 풀에 접근하기 위한 속성
        public Pool Pool => pool;

        /// <summary>
        /// 월드가 로드될 때 호출되어 오브젝트 풀을 초기화합니다.
        /// </summary>
        /// 
        /*
        public void OnWorldLoaded()
        {
            // 새로운 오브젝트 풀 생성 (Pool 클래스와 PoolManager는 외부 정의가 필요합니다.)
            pool = new Pool(prefab, $"WorldItem_{prefab.name}");
        }
        */
        public void OnWorldLoaded()
        {
            // prefab.name과 hash를 조합해서 고유한 풀 이름 생성
            string poolName = $"WorldItem_{prefab.name}_{hash}";
            
            // 고유 이름으로 풀 초기화
            pool = new Pool(prefab, poolName);
        }

        /// <summary>
        /// 월드가 언로드될 때 호출되어 오브젝트 풀을 파괴합니다.
        /// </summary>
        public void OnWorldUnloaded()
        {
            // 오브젝트 풀 파괴 (PoolManager 클래스는 외부 정의가 필요합니다.)
            PoolManager.DestroyPool(pool);
        }
    }
}