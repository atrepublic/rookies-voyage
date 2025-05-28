// 스크립트 설명: 게임 씬에 배치되는 개별 상자 엔티티의 데이터를 저장하는 클래스입니다.
// 상자의 타입, 보상 정보, 위치, 회전, 스케일 등의 상태 정보를 포함하며 비교 기능을 제공합니다.
using System; // IEquatable 사용을 위한 네임스페이스
using System.Collections.Generic; // EqualityComparer 사용을 위한 네임스페이스
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // Unity 에디터에서 인스펙터 창에 표시될 수 있도록 직렬화 가능하게 설정
    // 다른 ChestEntityData 객체와의 비교를 위해 IEquatable 인터페이스 구현
    [System.Serializable]
    public class ChestEntityData : IEquatable<ChestEntityData>
    {
        [Tooltip("상자 엔티티 데이터가 초기화되었는지 여부")] // 주요 변수 한글 툴팁
        public bool IsInited = false; // 초기화 상태 여부

        [Tooltip("이 상자 엔티티의 레벨 상자 타입")] // 주요 변수 한글 툴팁
        public LevelChestType ChestType; // 상자 타입 (LevelChestType은 Watermelon.LevelSystem 네임스페이스 또는 다른 곳에 정의된 것으로 가정)

        [Tooltip("상자 개봉 시 보상으로 지급될 화폐의 타입")] // 주요 변수 한글 툴팁
        public CurrencyType RewardCurrency; // 보상 화폐 타입 (CurrencyType은 Watermelon.SquadShooter 또는 다른 곳에 정의된 것으로 가정)

        [Tooltip("상자 개봉 시 보상으로 지급될 화폐의 수량")] // 주요 변수 한글 툴팁
        public int RewardValue = 5; // 보상 수량

        [Tooltip("상자 개봉 시 드롭될 화폐 아이템의 개수")] // 주요 변수 한글 툴팁
        public int DroppedCurrencyItemsAmount = 1; // 드롭될 화폐 아이템 개수

        [Tooltip("상자 엔티티의 씬 내 위치")] // 주요 변수 한글 툴팁
        public Vector3 Position; // 위치

        [Tooltip("상자 엔티티의 씬 내 회전")] // 주요 변수 한글 툴팁
        public Quaternion Rotation; // 회전

        [Tooltip("상자 엔티티의 씬 내 스케일")] // 주요 변수 한글 툴팁
        public Vector3 Scale; // 스케일

        /// <summary>
        /// ChestEntityData 클래스의 생성자입니다.
        /// </summary>
        /// <param name="chestType">상자 타입.</param>
        /// <param name="position">상자의 위치.</param>
        /// <param name="rotation">상자의 회전.</param>
        /// <param name="scale">상자의 스케일.</param>
        /// <param name="rewardCurrency">보상 화폐 타입.</param>
        /// <param name="rewardValue">보상 수량.</param>
        /// <param name="droppedCurrencyItemsAmount">드롭될 화폐 아이템 개수.</param>
        public ChestEntityData(LevelChestType chestType, Vector3 position, Quaternion rotation, Vector3 scale, CurrencyType rewardCurrency, int rewardValue, int droppedCurrencyItemsAmount)
        {
            ChestType = chestType;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            RewardCurrency = rewardCurrency;
            RewardValue = rewardValue;
            DroppedCurrencyItemsAmount = droppedCurrencyItemsAmount;
        }

        /// <summary>
        /// 다른 객체와 이 ChestEntityData 객체가 같은지 비교합니다.
        /// </summary>
        /// <param name="obj">비교할 다른 객체.</param>
        /// <returns>객체가 같으면 true, 그렇지 않으면 false.</returns>
        public override bool Equals(object obj)
        {
            // 비교 대상이 ChestEntityData 타입인지 확인하고 IEquatable<ChestEntityData>의 Equals를 호출합니다.
            return Equals(obj as ChestEntityData);
        }

        /// <summary>
        /// 다른 ChestEntityData 객체와 이 객체가 같은지 비교합니다. (IEquatable 인터페이스 구현)
        /// 모든 관련 필드의 값이 같으면 동일한 객체로 간주합니다.
        /// </summary>
        /// <param name="other">비교할 다른 ChestEntityData 객체.</param>
        /// <returns>객체가 같으면 true, 그렇지 않으면 false.</returns>
        public bool Equals(ChestEntityData other)
        {
            // 비교 대상이 null이 아니고, 모든 필드의 값이 같은지 확인
            return other is not null && // C# 7 이상의 패턴 매칭 사용 (Unity 2023+ 문법)
                   ChestType == other.ChestType &&
                   RewardCurrency == other.RewardCurrency &&
                   RewardValue == other.RewardValue &&
                   DroppedCurrencyItemsAmount == other.DroppedCurrencyItemsAmount &&
                   Position.Equals(other.Position) &&
                   Rotation.Equals(other.Rotation) &&
                   Scale.Equals(other.Scale);
        }

        /// <summary>
        /// 이 객체의 해시 코드를 계산합니다.
        /// Equals 메서드가 true를 반환하는 두 객체는 동일한 해시 코드를 반환해야 합니다.
        /// </summary>
        /// <returns>이 객체의 해시 코드.</returns>
        public override int GetHashCode()
        {
            // 모든 관련 필드를 사용하여 해시 코드 조합 (C# 8 이상의 HashCode.Combine 사용, Unity 2023+ 문법)
            return HashCode.Combine(ChestType, RewardCurrency, RewardValue, DroppedCurrencyItemsAmount, Position, Rotation, Scale);
        }

        /// <summary>
        /// 두 ChestEntityData 객체가 같은지 비교하는 == 연산자 오버로드.
        /// </summary>
        /// <param name="left">왼쪽 객체.</param>
        /// <param name="right">오른쪽 객체.</param>
        /// <returns>두 객체가 같으면 true, 그렇지 않으면 false.</returns>
        public static bool operator ==(ChestEntityData left, ChestEntityData right)
        {
            // EqualityComparer<T>.Default를 사용하여 비교 (null 처리 포함)
            return EqualityComparer<ChestEntityData>.Default.Equals(left, right);
        }

        /// <summary>
        /// 두 ChestEntityData 객체가 다른지 비교하는 != 연산자 오버로드.
        /// </summary>
        /// <param name="left">왼쪽 객체.</param>
        /// <param name="right">오른쪽 객체.</param>
        /// <returns>두 객체가 다르면 true, 그렇지 않으면 false.</returns>
        public static bool operator !=(ChestEntityData left, ChestEntityData right)
        {
            return !(left == right); // == 연산자 결과를 반전하여 사용
        }
    }
}