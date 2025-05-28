// EnemyEntityData.cs
// 이 스크립트는 레벨 데이터에 사용될 적 엔티티의 데이터를 저장하는 클래스입니다.
// 적의 유형, 위치, 회전, 스케일, 엘리트 여부 및 이동 경로 정보를 포함하며, 데이터 비교를 위한 Equals 및 GetHashCode 메서드를 구현합니다.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.SquadShooter; // 적 유형 정의가 포함된 네임스페이스

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class EnemyEntityData : IEquatable<EnemyEntityData>
    {
        [Tooltip("적의 유형")] // EnemyType 변수에 대한 툴팁
        public EnemyType EnemyType;

        [Tooltip("적의 초기 위치")] // Position 변수에 대한 툴팁
        public Vector3 Position;

        [Tooltip("적의 초기 회전")] // Rotation 변수에 대한 툴팁
        public Quaternion Rotation;

        [Tooltip("적의 초기 스케일")] // Scale 변수에 대한 툴팁
        public Vector3 Scale = Vector3.one; // 기본값으로 Vector3.one 설정

        [Tooltip("적이 엘리트 타입인지 여부")] // IsElite 변수에 대한 툴팁
        public bool IsElite;

        [Tooltip("적이 순찰하거나 이동할 경로 지점 배열")] // PathPoints 변수에 대한 툴팁
        public Vector3[] PathPoints;

        /// <summary>
        /// EnemyEntityData 클래스의 새로운 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="enemyType">적의 유형</param>
        /// <param name="position">적의 초기 위치</param>
        /// <param name="rotation">적의 초기 회전</param>
        /// <param name="scale">적의 초기 스케일</param>
        /// <param name="isElite">적이 엘리트 타입인지 여부</param>
        /// <param name="pathPoints">적이 순찰하거나 이동할 경로 지점 배열</param>
        public EnemyEntityData(EnemyType enemyType, Vector3 position, Quaternion rotation, Vector3 scale, bool isElite, Vector3[] pathPoints)
        {
            EnemyType = enemyType;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            IsElite = isElite;
            PathPoints = pathPoints;
        }

        /// <summary>
        /// 현재 오브젝트와 다른 오브젝트의 같음을 비교합니다.
        /// </summary>
        /// <param name="obj">비교할 다른 오브젝트</param>
        /// <returns>두 오브젝트가 같으면 true, 그렇지 않으면 false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as EnemyEntityData);
        }

        /// <summary>
        /// 현재 오브젝트와 다른 EnemyEntityData 인스턴스의 같음을 비교합니다.
        /// </summary>
        /// <param name="other">비교할 다른 EnemyEntityData 인스턴스</param>
        /// <returns>두 인스턴스가 같으면 true, 그렇지 않으면 false</returns>
        public bool Equals(EnemyEntityData other)
        {
            // PathPoints 배열 비교를 위해 SequenceEqual 사용
            return other is not null &&
                EnemyType == other.EnemyType &&
                Position.Equals(other.Position) &&
                Rotation.Equals(other.Rotation) &&
                Scale.Equals(other.Scale) &&
                IsElite == other.IsElite &&
                (PathPoints == other.PathPoints || (PathPoints != null && other.PathPoints != null && PathPoints.SequenceEqual(other.PathPoints)));
        }

        /// <summary>
        /// 현재 오브젝트에 대한 해시 코드를 가져옵니다.
        /// </summary>
        /// <returns>현재 오브젝트의 해시 코드</returns>
        public override int GetHashCode()
        {
            // PathPoints 배열의 해시 코드를 포함하여 계산
            var hashCode = new HashCode();
            hashCode.Add(EnemyType);
            hashCode.Add(Position);
            hashCode.Add(Rotation);
            hashCode.Add(Scale);
            hashCode.Add(IsElite);
            // 배열의 경우 각 요소의 해시 코드를 합하거나 다른 방식으로 처리할 수 있습니다.
            // 간단하게 배열 자체의 해시 코드를 사용하거나, 각 요소의 해시 코드를 결합합니다.
            if (PathPoints != null)
            {
                foreach (var point in PathPoints)
                {
                    hashCode.Add(point);
                }
            }
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// 두 EnemyEntityData 인스턴스가 같은지 비교합니다.
        /// </summary>
        /// <param name="left">왼쪽 EnemyEntityData 인스턴스</param>
        /// <param name="right">오른쪽 EnemyEntityData 인스턴스</param>
        /// <returns>두 인스턴스가 같으면 true, 그렇지 않으면 false</returns>
        public static bool operator ==(EnemyEntityData left, EnemyEntityData right)
        {
            return EqualityComparer<EnemyEntityData>.Default.Equals(left, right);
        }

        /// <summary>
        /// 두 EnemyEntityData 인스턴스가 다른지 비교합니다.
        /// </summary>
        /// <param name="left">왼쪽 EnemyEntityData 인스턴스</param>
        /// <param name="right">오른쪽 EnemyEntityData 인스턴스</param>
        /// <returns>두 인스턴스가 다르면 true, 그렇지 않으면 false</returns>
        public static bool operator !=(EnemyEntityData left, EnemyEntityData right)
        {
            return !(left == right);
        }
    }
}