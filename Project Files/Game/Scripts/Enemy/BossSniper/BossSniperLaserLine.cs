// ==============================================
// 📌 BossSniperLaserLine.cs
// ✅ 보스 스나이퍼가 사용하는 레이저 시각화 라인 제어 클래스
// ✅ 메쉬 렌더러를 기반으로 색상, 위치, 회전, 크기 등을 설정 가능
// ==============================================

using UnityEngine;

namespace Watermelon.Enemy.BossSniper
{
    /// <summary>
    /// 보스 스나이퍼 레이저 한 줄의 시각적 표현 및 설정을 담당하는 클래스
    /// </summary>
    public class BossSniperLaserLine
    {
        [Tooltip("레이저에 사용되는 메쉬 렌더러")]
        private MeshRenderer meshRenderer;

        /// <summary>
        /// 📌 메쉬 렌더러 초기화
        /// </summary>
        public void Init(MeshRenderer meshRenderer)
        {
            this.meshRenderer = meshRenderer;
        }

        /// <summary>
        /// 📌 레이저 색상 설정
        /// </summary>
        public void SetColor(Color color)
        {
            meshRenderer.material.SetColor("_BaseColor", color);
        }

        /// <summary>
        /// 📌 레이저 활성/비활성 상태 설정
        /// </summary>
        public void SetActive(bool isActive)
        {
            meshRenderer.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 📌 레이저의 위치, 회전, 크기를 계산하여 배치
        /// </summary>
        public void Init(Vector3 startPos, Vector3 hitPos, Vector3 scale)
        {
            // 시작 지점과 히트 지점 사이의 중간 위치
            Vector3 middlePoint = (startPos + hitPos) * 0.5f;

            meshRenderer.transform.position = middlePoint;
            meshRenderer.transform.localScale = scale;
            meshRenderer.transform.rotation = Quaternion.LookRotation((hitPos - startPos).normalized);
        }
    }
}
