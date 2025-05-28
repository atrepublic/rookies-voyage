// ISkinData.cs
/// <summary>
/// 스킨 데이터에 필요한 인터페이스입니다.
/// 스킨의 식별, 해시, 잠금 상태, 데이터베이스 제공자, 초기화 및 잠금 해제 기능을 정의합니다.
/// </summary>
using UnityEngine;

namespace Watermelon
{
    public interface ISkinData
    {
        /// <summary>스킨의 고유 식별자(ID)를 가져옵니다.</summary>
        string ID { get; }
        /// <summary>스킨 데이터의 해시 값을 가져옵니다.</summary>
        int Hash { get; }
        /// <summary>스킨이 잠금 해제 상태인지 여부를 가져옵니다.</summary>
        bool IsUnlocked { get; }
        /// <summary>이 스킨을 제공하는 데이터베이스를 가져옵니다.</summary>
        AbstractSkinDatabase SkinsProvider { get; }

        /// <summary>
        /// 스킨 데이터를 초기화합니다.
        /// database provider를 설정하고 저장된 상태를 불러옵니다.
        /// </summary>
        void Init(AbstractSkinDatabase provider);
        /// <summary>
        /// 스킨을 잠금 해제 상태로 변경합니다.
        /// </summary>
        void Unlock();
    }
}