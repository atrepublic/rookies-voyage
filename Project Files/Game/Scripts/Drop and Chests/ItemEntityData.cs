// 스크립트 설명: 게임 씬에 배치되는 개별 아이템 엔티티(드롭 아이템 등)의 데이터를 저장하는 클래스입니다.
// 아이템의 해시 값, 위치, 회전, 스케일 정보를 포함하며 비교 기능을 제공합니다.
using System; // IEquatable 사용을 위한 네임스페이스
using System.Collections;
using System.Collections.Generic; // EqualityComparer 사용을 위한 네임스페이스
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // Unity 에디터에서 인스펙터 창에 표시될 수 있도록 직렬화 가능하게 설정
    // 다른 ItemEntityData 객체와의 비교를 위해 IEquatable 인터페이스 구현
    [System.Serializable]
    public class ItemEntityData : IEquatable<ItemEntityData>
    {
        [Tooltip("아이템 엔티티를 식별하는 해시 값")] // 주요 변수 한글 툴팁
        public int Hash; // 아이템 해시 값 (고유 식별자 역할)

        [Tooltip("아이템 엔티티의 씬 내 위치")] // 주요 변수 한글 툴팁
        public Vector3 Position; // 위치

        [Tooltip("아이템 엔티티의 씬 내 회전")] // 주요 변수 한글 툴팁
        public Quaternion Rotation; // 회전

        [Tooltip("아이템 엔티티의 씬 내 스케일")] // 주요 변수 한글 툴팁
        public Vector3 Scale; // 스케일

        /// <summary>
        /// ItemEntityData 클래스의 생성자입니다.
        /// </summary>
        /// <param name="hash">아이템 해시 값.</param>
        /// <param name="position">아이템의 위치.</param>
        /// <param name="rotation">아이템의 회전.</param>
        /// <param name="scale">아이템의 스케일.</param>
        public ItemEntityData(int hash, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        /// <summary>
        /// 다른 객체와 이 ItemEntityData 객체가 같은지 비교합니다.
        /// </summary>
        /// <param name="obj">비교할 다른 객체.</param>
        /// <returns>객체가 같으면 true, 그렇지 않으면 false.</returns>
        public override bool Equals(object obj)
        {
            // 비교 대상이 ItemEntityData 타입인지 확인하고 IEquatable<ItemEntityData>의 Equals를 호출합니다.
            return Equals(obj as ItemEntityData);
        }

        /// <summary>
        /// 다른 ItemEntityData 객체와 이 객체가 같은지 비교합니다. (IEquatable 인터페이스 구현)
        /// 해시 값, 위치, 회전, 스케일이 모두 같으면 동일한 객체로 간주합니다.
        /// </summary>
        /// <param name="other">비교할 다른 ItemEntityData 객체.</param>
        /// <returns>객체가 같으면 true, 그렇지 않으면 false.</returns>
        public bool Equals(ItemEntityData other)
        {
            // 비교 대상이 null이 아니고, 해시, 위치, 회전, 스케일 필드의 값이 같은지 확인
            return other is not null && // C# 7 이상의 패턴 매칭 사용 (Unity 2023+ 문법)
                   Hash == other.Hash &&
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
            // 해시, 위치, 회전, 스케일 필드를 사용하여 해시 코드 조합 (C# 8 이상의 HashCode.Combine 사용, Unity 2023+ 문법)
            return HashCode.Combine(Hash, Position, Rotation, Scale);
        }

        /// <summary>
        /// 두 ItemEntityData 객체가 같은지 비교하는 == 연산자 오버로드.
        /// </summary>
        /// <param name="left">왼쪽 객체.</param>
        /// <param name="right">오른쪽 객체.</param>
        /// <returns>두 객체가 같으면 true, 그렇지 않으면 false.</returns>
        public static bool operator ==(ItemEntityData left, ItemEntityData right)
        {
            // EqualityComparer<T>.Default를 사용하여 비교 (null 처리 포함)
            return EqualityComparer<ItemEntityData>.Default.Equals(left, right);
        }

        /// <summary>
        /// 두 ItemEntityData 객체가 다른지 비교하는 != 연산자 오버로드.
        /// </summary>
        /// <param name="left">왼쪽 객체.</param>
        /// <param name="right">오른쪽 객체.</param>
        /// <returns>두 객체가 다르면 true, 그렇지 않으면 false.</returns>
        public static bool operator !=(ItemEntityData left, ItemEntityData right)
        {
            return !(left == right); // == 연산자 결과를 반전하여 사용
        }
    }
}