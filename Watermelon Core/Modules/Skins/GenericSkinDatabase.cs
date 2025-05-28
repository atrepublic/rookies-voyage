// GenericSkinDatabase.cs
/// <summary>
/// 제네릭 스킨 데이터베이스 추상 클래스입니다.
/// T 타입의 스킨 데이터 배열을 관리하고, 인덱스 또는 ID로 조회 기능을 제공합니다.
/// CreateAssetMenu 속성을 통해 에디터에서 에셋 생성이 가능합니다.
/// </summary>
using System;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "New Skin Database", menuName = "Data/Skins/Generic Skin Database")]
    public abstract class GenericSkinDatabase<T> : AbstractSkinDatabase where T : ISkinData
    {
        [SerializeField, Tooltip("등록된 스킨 데이터 목록")]
        private T[] skins;

        /// <summary>등록된 스킨 데이터 배열을 반환합니다.</summary>
        public T[] Skins => skins;

        /// <summary>등록된 스킨의 총 개수를 반환합니다.</summary>
        public override int SkinsCount => skins != null ? skins.Length : 0;

        /// <summary>관리하는 스킨 데이터 타입(T)을 반환합니다.</summary>
        public override Type SkinType => typeof(T);

        /// <summary>
        /// 데이터베이스를 초기화하고, 각 스킨 데이터의 Init 메서드를 호출합니다.
        /// </summary>
        public override void Init()
        {
            if (skins == null) return;
            for (int i = 0; i < skins.Length; i++)
            {
                skins[i].Init(this);
            }
        }

        /// <summary>
        /// 인덱스로 스킨 데이터를 조회하여 반환합니다.
        /// 범위를 벗어나면 예외를 발생시킵니다.
        /// </summary>
        public override ISkinData GetSkinData(int index)
        {
            if (skins == null || index < 0 || index >= skins.Length)
                throw new IndexOutOfRangeException($"스킨 인덱스가 범위를 벗어났습니다: {index}");
            return skins[index];
        }

        /// <summary>
        /// ID로 스킨 데이터를 조회하여 반환합니다. 존재하지 않으면 null을 반환합니다.
        /// </summary>
        public override ISkinData GetSkinData(string id)
        {
            if (skins == null) return null;
            foreach (var skin in skins)
            {
                if (skin.ID == id)
                    return skin;
            }
            return null;
        }
    }
}