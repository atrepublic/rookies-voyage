// SkinPreviewAttribute.cs
// 인스펙터에서 특정 필드의 스킨을 시각적으로 미리보기 위해 사용되는 속성입니다.
// 현재는 빈 클래스이지만, 커스텀 에디터에서 이 속성이 붙은 필드를 감지하여 처리할 수 있도록 합니다.

using System;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 인스펙터에서 스킨 미리보기를 지원하도록 만드는 커스텀 속성입니다.
    /// (에디터 확장과 함께 사용됩니다)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SkinPreviewAttribute : PropertyAttribute
    {
        // 현재는 기능이 없지만, Editor에서 이 Attribute를 인식하여 동작을 수행할 수 있습니다.
    }
}