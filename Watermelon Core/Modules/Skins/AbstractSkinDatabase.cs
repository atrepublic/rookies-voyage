// AbstractSkinDatabase.cs
/// <summary>
/// 스킨 데이터베이스의 추상 기본 클래스입니다.
/// 스킨 수, 스킨 타입, 데이터 조회 및 초기화 메서드를 정의합니다.
/// ScriptableObject로 구현되어 에셋으로 저장됩니다.
/// </summary>
using System;
using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractSkinDatabase : ScriptableObject
    {
        /// <summary>데이터베이스에 등록된 스킨의 총 개수를 가져옵니다.</summary>
        public abstract int SkinsCount { get; }

        /// <summary>데이터베이스가 관리하는 스킨 데이터의 타입을 가져옵니다.</summary>
        public abstract Type SkinType { get; }

        /// <summary>인덱스로 스킨 데이터를 가져옵니다.</summary>
        public abstract ISkinData GetSkinData(int index);

        /// <summary>ID로 스킨 데이터를 가져옵니다.</summary>
        public abstract ISkinData GetSkinData(string id);

        /// <summary>데이터베이스를 초기화합니다. 스킨 데이터 초기화 로직을 구현하세요.</summary>
        public abstract void Init();
    }
}