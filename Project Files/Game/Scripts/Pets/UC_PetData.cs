// UC_PetData.cs
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 펫 시스템을 위한 데이터 SO.
    /// 펫의 기본 정보 및 업그레이드 데이터를 포함합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Squad Shooter/Pet Data", fileName = "UC_PetData")]
    public class UC_PetData : ScriptableObject
    {
        [Tooltip("펫의 고유 ID (중복 불가)")]
        public int petID;

        [Tooltip("펫 이름")]
        public string petName;

        [Tooltip("펫 아이콘 (로비 UI용)")]
        public Sprite previewSprite;                        // 기존 petPreviewSprite → previewSprite :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}

        [Tooltip("인게임에서 스폰할 펫 프리팹")]
        public GameObject petPrefab;

        [Tooltip("펫의 기본 HP (현재는 데미지만 관리, 죽음은 없음)")]
        public int baseHP = 100;

        [Tooltip("펫이 잠금 해제되기 위한 플레이어 최소 레벨")]
        public int requiredPlayerLevel = 1;

        [Tooltip("펫 언락 비용 (코인)")]
        public int unlockCost;                              // 신규 필드

        [Tooltip("펫 전용 무기 프리팹")]
        public GameObject petWeaponPrefab;

        [Tooltip("기본 공격력 (레벨 0 기준)")]
        public float baseAttackPower = 3f;

        [Tooltip("펫 능력치 업그레이드 정보 목록")]
        public List<PetUpgrade> upgrades;

        [System.Serializable]
        public class PetUpgrade
        {
            [Tooltip("업그레이드 단계 레벨")]
            public int level;

            [Tooltip("업그레이드 비용 (코인)")]
            public int cost;                               // 기존 price → cost

            [Tooltip("업그레이드 당 공격력 증가량")]
            public float attackPowerIncrease;
        }
    }
}
