// ==============================================
// 📌 EnemyData.cs
// ✅ 개별 적 유닛의 설정 정보를 담고 있는 데이터 클래스
// ✅ 적 타입, 프리팹, 능력치, 에디터 아이콘 등 포함
// ✅ EnemiesDatabase와 연동되어 게임 내 적 스폰에 사용됨
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 적 유닛 1종의 정보를 정의하는 클래스
    /// </summary>
    [System.Serializable]
    public class EnemyData
    {
        [Tooltip("이 적의 고유 타입 (예: Zombie, Ranged, Suicide 등)")]
        [SerializeField] private EnemyType enemyType;

        /// <summary>
        /// 적의 타입
        /// </summary>
        public EnemyType EnemyType => enemyType;

        [Tooltip("게임에서 사용될 적 유닛 프리팹")]
        [SerializeField] private GameObject prefab;

        /// <summary>
        /// 적 프리팹 오브젝트
        /// </summary>
        public GameObject Prefab => prefab;

        [Tooltip("이 적의 스탯 정보 (체력, 공격력, 이동속도 등)")]
        [SerializeField] private EnemyStats stats;

        /// <summary>
        /// 적의 능력치 정보
        /// </summary>
        public EnemyStats Stats => stats;

        [Header("Editor")]

        [Tooltip("에디터에서 표시할 아이콘")]
        [SerializeField] private Texture2D icon;

        /// <summary>
        /// 에디터용 아이콘 이미지
        /// </summary>
        public Texture2D Icon => icon;

        [Tooltip("아이콘에 적용할 색상 틴트")]
        [SerializeField] private Color iconTint;

        /// <summary>
        /// 에디터 아이콘 틴트 색상
        /// </summary>
        public Color IconTint => iconTint;
    }
}
