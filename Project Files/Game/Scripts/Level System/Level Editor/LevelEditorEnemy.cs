/// <summary>
/// LevelEditorEnemy.cs
///
/// 레벨 에디터에서 적(Enemy) 배치, 경로 설정, 엘리트 여부를 관리하는 스크립트입니다.
/// 에디터 모드에서 경로 포인트 추가/수정, 시각적 Gizmo 표시 기능을 지원합니다.
/// </summary>

#pragma warning disable 649
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [ExecuteInEditMode]
    public class LevelEditorEnemy : MonoBehaviour
    {
#if UNITY_EDITOR
        [Tooltip("적의 타입을 설정합니다.")]
        public EnemyType type;

        [Tooltip("이 적이 엘리트인지 여부를 설정합니다.")]
        public bool isElite;

        [Tooltip("적이 이동할 순찰 경로 포인트 리스트입니다.")]
        public List<Transform> pathPoints;

        [Tooltip("경로 포인트를 저장할 부모 오브젝트입니다.")]
        public Transform pathPointsContainer;

        // Gizmo 관련 내부 변수들
        private const int LINE_HEIGHT = 5;
        private Color enemyColor;
        private Color defaultColor;
        private Color goldColor;
        private Material enemyMaterial;
        private StartPointHandles startPointHandles;
        private bool isStartPointHandlesInited;

        /// <summary>
        /// 에디터 모드에서 초기화 작업을 수행합니다.
        /// </summary>
        public void Awake()
        {
            pathPoints = new List<Transform>();
            enemyColor = Random.ColorHSV(0f, 1, 0.75f, 1, 1f, 1, 1, 1);
            enemyMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            enemyMaterial.color = enemyColor;
            goldColor = new Color(1, 204 / 255f, 0);
            isStartPointHandlesInited = false;
        }

        /// <summary>
        /// 에디터 모드에서 매 프레임 경로 포인트를 업데이트합니다.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < pathPointsContainer.childCount; i++)
            {
                if (!pathPoints.Contains(pathPointsContainer.GetChild(i)))
                {
                    pathPoints.Add(pathPointsContainer.GetChild(i));
                }
            }

            for (int i = pathPoints.Count - 1; i >= 0; i--)
            {
                if (pathPoints[i] == null)
                {
                    pathPoints.RemoveAt(i);
                }
            }

            if (isElite && !isStartPointHandlesInited)
            {
                startPointHandles = gameObject.AddComponent<StartPointHandles>();
                startPointHandles.diskRadius = 0.7f;
                startPointHandles.thickness = 7f;
                startPointHandles.diskColor = goldColor;
                startPointHandles.displayText = false;
                isStartPointHandlesInited = true;
            }
            else if (!isElite && isStartPointHandlesInited)
            {
                DestroyImmediate(startPointHandles);
                isStartPointHandlesInited = false;
            }
        }

        /// <summary>
        /// 새로운 순찰 포인트를 추가합니다.
        /// </summary>
        [Button]
        public void AddPathPoint()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(pathPointsContainer);
            sphere.transform.position = pathPointsContainer.transform.parent.transform.position.SetY(0) + Vector3.back;
            sphere.transform.localScale = Vector3.one * 0.78125f;
            pathPoints.Add(sphere.transform);

            sphere.GetComponent<MeshRenderer>().sharedMaterial = enemyMaterial;
            Selection.activeGameObject = sphere;
        }

        /// <summary>
        /// 기존 모든 순찰 포인트에 적용할 머티리얼을 설정합니다.
        /// </summary>
        [Button]
        public void ApplyMaterialToPathPoints()
        {
            MeshRenderer renderer;

            for (int i = 0; i < pathPoints.Count; i++)
            {
                renderer = pathPoints[i].GetComponent<MeshRenderer>();
                renderer.sharedMaterial = enemyMaterial;
            }
        }

        /// <summary>
        /// Gizmo 선을 그립니다.
        /// </summary>
        private void DrawLine(Vector3 tempLineStart, Vector3 tempLineEnd)
        {
            Vector3 offset = Vector3.zero.AddToY(0.01f);

            for (int i = 0; i < LINE_HEIGHT; i++)
            {
                Gizmos.DrawLine(tempLineStart + i * offset, tempLineEnd + i * offset);
            }
        }

        /// <summary>
        /// 에디터에서 Gizmo를 그리는 기능입니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = enemyColor;

            Gizmos.DrawSphere(transform.position + Vector3.up, 0.3125f);

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                DrawLine(pathPoints[i].transform.position, pathPoints[i + 1].transform.position);
            }

            Gizmos.color = defaultColor;
        }

        /// <summary>
        /// 순찰 경로 포인트들의 월드 좌표를 배열로 반환합니다.
        /// </summary>
        public Vector3[] GetPathPoints()
        {
            Vector3[] result = new Vector3[pathPoints.Count];

            for (int i = 0; i < pathPoints.Count; i++)
            {
                result[i] = pathPoints[i].localPosition + pathPointsContainer.transform.parent.localPosition;
            }

            return result;
        }
#endif
    }
}
