// ==============================================
// 📌 EliteCase.cs
// ✅ 적 유닛이 엘리트(강화형) 상태일 때 사용할 메시(Mesh)를 교체해주는 설정 클래스
// ✅ 일반 메시 ↔ 엘리트 메시 전환 기능 포함
// ✅ SkinnedMeshRenderer 또는 MeshFilter 기반 모델 모두 지원
// ==============================================

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 엘리트 적의 메시(Mesh)를 설정 및 전환하기 위한 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class EliteCase
    {
        [Tooltip("SkinnedMeshRenderer를 사용하는 메시 페어 리스트")]
        public List<MeshPair> pairs;

        [Tooltip("MeshFilter를 사용하는 단순 메시 페어 리스트")]
        public List<SimpleMeshPair> simplePairs;

        /// <summary>
        /// 📌 엘리트 메시로 교체
        /// </summary>
        public void SetElite()
        {
            pairs?.ForEach((pair) => pair.renderer.sharedMesh = pair.eliteMesh);
            simplePairs?.ForEach((pair) => pair.filter.mesh = pair.eliteMesh);
        }

        /// <summary>
        /// 📌 일반 메시로 교체
        /// </summary>
        public void SetRegular()
        {
            pairs?.ForEach((pair) => pair.renderer.sharedMesh = pair.simpleMesh);
            simplePairs?.ForEach((pair) => pair.filter.mesh = pair.simpleMesh);
        }

        /// <summary>
        /// 📌 설정된 MeshPair 및 SimpleMeshPair의 필드 유효성 검사
        /// </summary>
        public void Validate()
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].renderer == null || pairs[i].simpleMesh == null || pairs[i].eliteMesh == null)
                {
                    Debug.LogError("[Enemy Behavior] Elite enemy case is not properly configured. Please check if all references are assigned on enemy script field Elite Case.");
                    pairs.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < simplePairs.Count; i++)
            {
                if (simplePairs[i].filter == null || simplePairs[i].simpleMesh == null || simplePairs[i].eliteMesh == null)
                {
                    Debug.LogError("[Enemy Behavior] Elite enemy case is not properly configured. Please check if all references are assigned on enemy script field Elite Case.");
                    simplePairs.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// 📌 SkinnedMeshRenderer 기반 메시 페어 정의 구조체
        /// </summary>
        [System.Serializable]
        public struct MeshPair
        {
            [Tooltip("대상 SkinnedMeshRenderer")]
            public SkinnedMeshRenderer renderer;

            [Tooltip("일반 적용 메시")]
            public Mesh simpleMesh;

            [Tooltip("엘리트 상태에서 사용할 메시")]
            public Mesh eliteMesh;
        }

        /// <summary>
        /// 📌 MeshFilter 기반 메시 페어 정의 구조체
        /// </summary>
        [System.Serializable]
        public struct SimpleMeshPair
        {
            [Tooltip("대상 MeshFilter")]
            public MeshFilter filter;

            [Tooltip("일반 적용 메시")]
            public Mesh simpleMesh;

            [Tooltip("엘리트 상태에서 사용할 메시")]
            public Mesh eliteMesh;
        }
    }
}
