// AbstractSkinData.cs
/// <summary>
/// ISkinData를 구현하는 기본 추상 클래스입니다.
/// 스킨 ID, 해시, 잠금 상태 및 데이터베이스 제공자 관리 기능을 포함합니다.
/// </summary>
using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractSkinData : ISkinData
    {
        [SerializeField, UniqueID, Tooltip("스킨의 고유 식별자 (ID)")]
        private string id;

        /// <summary>스킨의 고유 식별자(ID)를 반환합니다.</summary>
        public string ID => id;

        /// <summary>스킨 ID의 해시 코드를 반환합니다.</summary>
        public int Hash { get; private set; }

        /// <summary>이 스킨을 제공하는 데이터베이스를 반환합니다.</summary>
        public AbstractSkinDatabase SkinsProvider { get; private set; }

        /// <summary>스킨이 잠금 해제되어 있는지 여부를 반환합니다.</summary>
        public bool IsUnlocked => save.IsUnlocked;

        [Tooltip("저장된 스킨 잠금 상태를 관리하는 객체")]
        private SkinSave save;

        /// <summary>
        /// 스킨 데이터를 초기화합니다.
        /// 저장된 데이터에서 잠금 상태를 불러오고, 해시 값을 계산하며, 데이터베이스 제공자를 설정합니다.
        /// </summary>
        public virtual void Init(AbstractSkinDatabase provider)
        {
            save = SaveController.GetSaveObject<SkinSave>(id);
            Hash = id.GetHashCode();
            SkinsProvider = provider;
        }

        /// <summary>
        /// 스킨을 잠금 해제 상태로 변경합니다.
        /// </summary>
        public void Unlock()
        {
            save.IsUnlocked = true;
        }
    }
}