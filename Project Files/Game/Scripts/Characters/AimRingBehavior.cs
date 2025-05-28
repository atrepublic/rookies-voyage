// -----------------------------
// AimRingBehavior.cs
// -----------------------------
// 이 스크립트는 조준 반경을 시각적으로 표현하기 위한 링 메쉬를 생성하고,
// 회전하며 플레이어(또는 특정 대상)를 따라다니는 시각 이펙트 역할을 합니다.
// 메쉬는 반지형으로 생성되며, Stripe + Gap 패턴으로 구성됩니다.

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AimRingBehavior : MonoBehaviour
    {
        [Header("조준 링 설정")]
        [Tooltip("링의 두께")]
        [SerializeField] private float width;

        [Tooltip("360도를 몇 개의 정점으로 나눌지")]
        [SerializeField] private int detalisation;

        [Tooltip("띠의 길이 (Stripe 구간)")]
        [SerializeField] private float stripeLength;

        [Tooltip("띠 사이 간격 길이 (Gap 구간)")]
        [SerializeField] private float gapLength;

        [Space(5f)]
        [Tooltip("회전 속도")]
        [SerializeField] private float rotationSpeed;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;

        private readonly List<Vector3> vertices = new();
        private readonly List<int> triangles = new();

        private Transform followTransform;
        private float radius;

        // 대상 Transform을 받아 초기화
        public void Init(Transform followTransform)
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            mesh = new Mesh();
            meshFilter.mesh = mesh;

            transform.SetParent(null);
            this.followTransform = followTransform;
        }

        // 조준 반경 설정 및 메쉬 재생성
        public void SetRadius(float radius)
        {
            if (radius == 0)
            {
                Debug.LogError("Aiming radius can't be 0!");
            }

            this.radius = Mathf.Clamp(radius, 1, float.MaxValue);
            GenerateMesh();
        }

        // 매 프레임 대상 위치로 이동 + 회전 적용
        public void UpdatePosition()
        {
            transform.position = followTransform.position;
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        // 링 표시
        public void Show()
        {
            meshRenderer.enabled = true;
        }

        // 링 숨김
        public void Hide()
        {
            meshRenderer.enabled = false;
        }

        // 링 메쉬를 생성하는 함수 (스트라이프 + 간격 패턴)
        private void GenerateMesh()
        {
            mesh = new Mesh { name = "Generated Mesh" };
            meshFilter.mesh = mesh;

            float stepAngle = 360f / detalisation;

            float stripeAngle = 180f * stripeLength / (Mathf.PI * radius);
            int stripeSectorsAmount = Mathf.Clamp(Mathf.FloorToInt(stripeAngle / stepAngle), 1, int.MaxValue);

            float gapAngle = 180f * gapLength / (Mathf.PI * radius);
            int gapSectorsAmount = Mathf.Clamp(Mathf.FloorToInt(gapAngle / stepAngle), 1, int.MaxValue);

            vertices.Clear();
            triangles.Clear();
            mesh.Clear();

            float currentAngle = 0;

            while (currentAngle < 360f)
            {
                for (int i = 0; i < stripeSectorsAmount && currentAngle < 360f; i++)
                {
                    vertices.Add(GetPoint(radius, Mathf.Deg2Rad * currentAngle));
                    vertices.Add(GetPoint(radius + width, Mathf.Deg2Rad * currentAngle));
                    vertices.Add(GetPoint(radius, Mathf.Deg2Rad * (currentAngle + stepAngle)));

                    vertices.Add(GetPoint(radius + width, Mathf.Deg2Rad * currentAngle));
                    vertices.Add(GetPoint(radius + width, Mathf.Deg2Rad * (currentAngle + stepAngle)));
                    vertices.Add(GetPoint(radius, Mathf.Deg2Rad * (currentAngle + stepAngle)));

                    int trisCount = triangles.Count;
                    triangles.AddRange(new int[]
                    {
                        trisCount + 2, trisCount + 1, trisCount,
                        trisCount + 5, trisCount + 4, trisCount + 3
                    });

                    currentAngle += stepAngle;
                }

                for (int i = 0; i < gapSectorsAmount && currentAngle < 360f; i++)
                {
                    currentAngle += stepAngle;
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
        }

        // 반지름과 각도로 3D 공간상의 좌표 계산
        private Vector3 GetPoint(float radius, float angle)
        {
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        // 플레이어가 사라질 경우 이 오브젝트 제거
        public void OnPlayerDestroyed()
        {
            Destroy(gameObject);
        }
    }
}
