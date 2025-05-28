// SkinPickerAttribute.cs
// 이 속성(Attribute)은 인스펙터에서 특정 타입의 스킨 데이터베이스를 선택할 수 있도록 도와주는 커스텀 에디터용 특성입니다.
// 스킨을 선택할 수 있는 드롭다운을 제공할 때 사용됩니다.

using System;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 특정 필드에 스킨 데이터베이스를 선택할 수 있는 UI를 제공하는 커스텀 속성입니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SkinPickerAttribute : PropertyAttribute
    {
        /// <summary>
        /// 이 속성이 사용할 스킨 데이터베이스 타입입니다.
        /// </summary>
        public Type DatabaseType { get; private set; }

        /// <summary>
        /// 기본 생성자. 데이터베이스 타입을 설정하지 않습니다.
        /// </summary>
        public SkinPickerAttribute() { }

        /// <summary>
        /// 스킨 데이터베이스 타입을 설정하는 생성자입니다.
        /// </summary>
        public SkinPickerAttribute(Type databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}
