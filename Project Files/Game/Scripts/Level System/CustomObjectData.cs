// CustomObjectData.cs
// 이 스크립트는 레벨 데이터에 사용될 사용자 지정 오브젝트의 데이터를 저장하는 클래스입니다.
// 오브젝트의 프리팹, 위치, 회전, 스케일 정보를 포함하며, 데이터 비교를 위한 Equals 및 GetHashCode 메서드를 구현합니다.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class CustomObjectData : IEquatable<CustomObjectData>
    {
        [Tooltip("참조할 게임 오브젝트 프리팹")] // PrefabRef 변수에 대한 툴팁
        public GameObject PrefabRef;

        [Tooltip("오브젝트의 위치")] // Position 변수에 대한 툴팁
        public Vector3 Position;

        [Tooltip("오브젝트의 회전")] // Rotation 변수에 대한 툴팁
        public Quaternion Rotation;

        [Tooltip("오브젝트의 스케일")] // Scale 변수에 대한 툴팁
        public Vector3 Scale;

        /// <summary>
        /// CustomObjectData 클래스의 새로운 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="prefabRef">참조할 게임 오브젝트 프리팹</param>
        /// <param name="position">오브젝트의 위치</param>
        /// <param name="rotation">오브젝트의 회전</param>
        /// <param name="scale">오브젝트의 스케일</param>
        public CustomObjectData(GameObject prefabRef, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PrefabRef = prefabRef;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        /// <summary>
        /// 현재 오브젝트와 다른 오브젝트의 같음을 비교합니다.
        /// </summary>
        /// <param name="obj">비교할 다른 오브젝트</param>
        /// <returns>두 오브젝트가 같으면 true, 그렇지 않으면 false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CustomObjectData);
        }

        /// <summary>
        /// 현재 오브젝트와 다른 CustomObjectData 인스턴스의 같음을 비교합니다.
        /// </summary>
        /// <param name="other">비교할 다른 CustomObjectData 인스턴스</param>
        /// <returns>두 인스턴스가 같으면 true, 그렇지 않으면 false</returns>
        public bool Equals(CustomObjectData other)
        {
            return other is not null &&
                EqualityComparer<GameObject>.Default.Equals(PrefabRef, other.PrefabRef) &&
                Position.Equals(other.Position) &&
                Rotation.Equals(other.Rotation) &&
                Scale.Equals(other.Scale);
        }

        /// <summary>
        /// 현재 오브젝트에 대한 해시 코드를 가져옵니다.
        /// </summary>
        /// <returns>현재 오브젝트의 해시 코드</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(PrefabRef, Position, Rotation, Scale);
        }

        /// <summary>
        /// 두 CustomObjectData 인스턴스가 같은지 비교합니다.
        /// </summary>
        /// <param name="left">왼쪽 CustomObjectData 인스턴스</param>
        /// <param name="right">오른쪽 CustomObjectData 인스턴스</param>
        /// <returns>두 인스턴스가 같으면 true, 그렇지 않으면 false</returns>
        public static bool operator ==(CustomObjectData left, CustomObjectData right)
        {
            return EqualityComparer<CustomObjectData>.Default.Equals(left, right);
        }

        /// <summary>
        /// 두 CustomObjectData 인스턴스가 다른지 비교합니다.
        /// </summary>
        /// <param name="left">왼쪽 CustomObjectData 인스턴스</param>
        /// <param name="right">오른쪽 CustomObjectData 인스턴스</param>
        /// <returns>두 인스턴스가 다르면 true, 그렇지 않으면 false</returns>
        public static bool operator !=(CustomObjectData left, CustomObjectData right)
        {
            return !(left == right);
        }
    }
}