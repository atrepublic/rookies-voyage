// ==============================================
// 📌 CapsuleSize.cs
// ✅ 캡슐 콜라이더의 크기와 중심 위치를 설정하는 구조체
// ✅ 주로 적, 캐릭터 등 충돌 판정용 CapsuleCollider 설정에 활용됨
// ✅ Gizmo 기능을 통해 에디터에서 시각적 확인 가능
// ==============================================

using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 캡슐 콜라이더의 중심 좌표, 반지름, 높이를 정의하는 구조체
    /// </summary>
    [System.Serializable]
    public struct CapsuleSize
    {
        [Tooltip("캡슐 콜라이더의 중심 위치 (로컬 좌표 기준)")]
        public Vector3 center;

        [Tooltip("캡슐 콜라이더의 반지름")]
        public float radius;

        [Tooltip("캡슐 콜라이더의 전체 높이")]
        public float height;

        /// <summary>
        /// 📌 설정된 값으로 캡슐 콜라이더에 적용
        /// </summary>
        /// <param name="capsuleCollider">적용 대상 캡슐 콜라이더</param>
        public void Apply(CapsuleCollider capsuleCollider)
        {
            capsuleCollider.center = center;
            capsuleCollider.radius = radius;
            capsuleCollider.height = height;
        }

        /// <summary>
        /// 📌 에디터 Gizmo로 캡슐 영역을 사각 박스로 시각화 (디버깅용)
        /// </summary>
        /// <param name="transform">기준 위치와 회전을 제공하는 트랜스폼</param>
        /// <param name="color">Gizmo 색상</param>
        public void DrawGizmo(Transform transform, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position + center, new Vector3(radius * 2f, height, radius * 2f));
        }
    }
}
