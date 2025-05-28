using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.UI.Particle
{
    /// <summary>
    ///   UI 파티클 설정을 담는 스크립터블 오브젝트 클래스.
    ///   파티클의 모양, 크기, 색상, 수명, 속도 등 다양한 설정을 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/UI Particles/Custom UI Particle Settings", fileName = "Custom UI Particle Settings")]
    public class UIParticleSettings : ScriptableObject
    {
        /// <summary>
        ///   생성될 UI 파티클 프리팹.
        ///   UIParticle 컴포넌트가 부착되어 있어야 합니다.
        /// </summary>
        [Tooltip("생성될 UI 파티클 프리팹")]
        public GameObject uiParticlePrefab;

        /// <summary>
        ///   파티클에 적용될 스프라이트.
        /// </summary>
        [Tooltip("파티클에 적용될 스프라이트")]
        public Sprite sprite;

        [Space]

        /// <summary>
        ///   Awake 시에 파티클 시스템을 자동으로 재생할지 여부.
        ///   true이면 재생, false이면 정지.
        /// </summary>
        [Tooltip("Awake 시에 파티클 시스템을 자동으로 재생할지 여부")]
        public bool playOnAwake;

        /// <summary>
        ///   파티클 시스템 시작 지연 시간 (초).
        /// </summary>
        [Tooltip("파티클 시스템 시작 지연 시간 (초)")]
        public float startDelay;

        /// <summary>
        ///   파티클 시작 크기 범위.
        ///   DuoFloat를 사용하여 최소/최대 크기를 지정합니다.
        /// </summary>
        [Tooltip("파티클 시작 크기 범위")]
        public DuoFloat startSize;

        /// <summary>
        ///   파티클 시작 색상 범위.
        ///   DuoColor를 사용하여 두 가지 색상 사이의 랜덤 색상을 지정합니다.
        /// </summary>
        [Tooltip("파티클 시작 색상 범위")]
        public DuoColor startDuoColor;

        /// <summary>
        ///   파티클에 적용될 중력 계수.
        ///   1이면 일반 중력, 0이면 중력 없음.
        /// </summary>
        [Tooltip("파티클에 적용될 중력 계수")]
        public float gravityModifier;

        [Space]

        /// <summary>
        ///   파티클 생성 영역의 모양.
        /// </summary>
        [Tooltip("파티클 생성 영역의 모양")]
        public Shape shape;

        /// <summary>
        ///   원형 생성 영역의 반지름.
        ///   Shape가 Circle일 때 사용됩니다.
        /// </summary>
        [Tooltip("원형 생성 영역의 반지름")]
        public float circleRadius;

        /// <summary>
        ///   사각형 생성 영역의 크기.
        ///   Shape가 Rect일 때 사용됩니다.
        /// </summary>
        [Tooltip("사각형 생성 영역의 크기")]
        public Vector2 rectSize;

        /// <summary>
        ///   파티클 속도의 구체화 정도 (0 ~ 1).
        ///   0이면 평면, 1이면 구형으로 퍼져나갑니다.
        /// </summary>
        [Tooltip("파티클 속도의 구체화 정도 (0 ~ 1)")]
        [Range(0f, 1f)] public float spherizeDirection;

        [Space]

        /// <summary>
        ///   파티클 수명 범위.
        ///   DuoFloat를 사용하여 최소/최대 수명을 지정합니다.
        /// </summary>
        [Tooltip("파티클 수명 범위")]
        public DuoFloat lifetime;

        /// <summary>
        ///   초당 생성될 파티클 수.
        /// </summary>
        [Tooltip("초당 생성될 파티클 수")]
        public int emissionPerSecond;

        /// <summary>
        ///   Burst 이벤트 설정 배열.
        ///   특정 시점에 여러 파티클을 한 번에 생성하는 이벤트를 정의합니다.
        /// </summary>
        [Tooltip("Burst 이벤트 설정 배열")]
        public BurstSettings[] bursts;

        [Space]

        /// <summary>
        ///   파티클 생성 각도 범위.
        ///   DuoFloat를 사용하여 최소/최대 각도를 지정합니다.
        /// </summary>
        [Tooltip("파티클 생성 각도 범위")]
        public DuoFloat angle;

        [Space]

        /// <summary>
        ///   3D 속도 사용 여부.
        ///   true이면 3D 속도, false이면 2D 속도를 사용합니다.
        /// </summary>
        [Tooltip("3D 속도 사용 여부")]
        public bool speed3d;

        /// <summary>
        ///   2D 속도 범위.
        ///   speed3d가 false일 때 사용됩니다.
        /// </summary>
        [Tooltip("2D 속도 범위")]
        public DuoFloat speed;

        /// <summary>
        ///   3D 속도 범위.
        ///   speed3d가 true일 때 사용됩니다.
        /// </summary>
        [Tooltip("3D 속도 범위")]
        public DuoVector3 speed3dValues;

        /// <summary>
        ///   속도 감쇠 범위.
        ///   파티클 속도가 시간에 따라 감소하는 정도를 지정합니다.
        /// </summary>
        [Tooltip("속도 감쇠 범위")]
        public DuoFloat dumping;

        [Space]

        /// <summary>
        ///   파티클 크기 변화 커브.
        ///   파티클 수명에 따른 크기 변화를 정의합니다.
        /// </summary>
        [Tooltip("파티클 크기 변화 커브")]
        public AnimationCurve scaleCurve;

        [Space]

        /// <summary>
        ///   수명에 따른 회전 사용 여부.
        ///   true이면 회전, false이면 회전 없음.
        /// </summary>
        [Tooltip("수명에 따른 회전 사용 여부")]
        public bool rotationOverLifetime;

        /// <summary>
        ///   각속도 범위.
        ///   rotationOverLifetime이 true일 때 사용됩니다.
        /// </summary>
        [Tooltip("각속도 범위")]
        public DuoFloat angularSpeed;

        [Space]

        /// <summary>
        ///   수명에 따른 색상 변화 사용 여부.
        ///   true이면 색상 변화, false이면 색상 고정.
        /// </summary>
        [Tooltip("수명에 따른 색상 변화 사용 여부")]
        public bool colorOverLifetime;

        /// <summary>
        ///   수명에 따른 색상 변화 그라디언트.
        ///   colorOverLifetime이 true일 때 사용됩니다.
        /// </summary>
        [Tooltip("수명에 따른 색상 변화 그라디언트")]
        public Gradient colorOverLifetimeGradient;


        /// <summary>
        ///   파티클 생성 영역의 모양을 정의하는 열거형.
        /// </summary>
        public enum Shape
        {
            /// <summary>
            ///   점.
            /// </summary>
            Point = 0,

            /// <summary>
            ///   원.
            /// </summary>
            Circle = 1,

            /// <summary>
            ///   사각형.
            /// </summary>
            Rect = 2,
        }

        /// <summary>
        ///   Burst 이벤트 설정을 담는 클래스.
        /// </summary>
        [System.Serializable]
        public class BurstSettings
        {
            /// <summary>
            ///   한 번에 생성될 파티클 수.
            /// </summary>
            [Tooltip("한 번에 생성될 파티클 수")]
            public int count;

            /// <summary>
            ///   Burst 이벤트 반복 횟수.
            ///   -1이면 무한 반복.
            /// </summary>
            [Tooltip("Burst 이벤트 반복 횟수. -1이면 무한 반복.")]
            public int loopsCount;

            /// <summary>
            ///   Burst 이벤트 사이의 간격 (초).
            /// </summary>
            [Tooltip("Burst 이벤트 사이의 간격 (초)")]
            public float interval;

            /// <summary>
            ///   Burst 이벤트 시작 지연 시간 (초).
            /// </summary>
            [Tooltip("Burst 이벤트 시작 지연 시간 (초)")]
            public float delay;
        }

        /// <summary>
        ///   두 개의 float 값 사이의 랜덤 값을 반환하는 구조체.
        /// </summary>
        [System.Serializable]
        public struct DuoFloat
        {
            /// <summary>
            ///   최소값.
            /// </summary>
            [Tooltip("최소값")]
            public float Min;

            /// <summary>
            ///   최대값.
            /// </summary>
            [Tooltip("최대값")]
            public float Max;

            /// <summary>
            ///   최소값과 최대값 사이의 랜덤 float 값을 반환합니다.
            /// </summary>
            /// <returns>최소값과 최대값 사이의 랜덤 float 값</returns>
            public float Random()
            {
                return UnityEngine.Random.Range(Min, Max);
            }
        }

        /// <summary>
        ///   두 개의 Vector3 값 사이의 랜덤 값을 반환하는 구조체.
        /// </summary>
        [System.Serializable]
        public struct DuoVector3
        {
            /// <summary>
            ///   최소 Vector3 값.
            /// </summary>
            [Tooltip("최소 Vector3 값")]
            public Vector3 Min;

            /// <summary>
            ///   최대 Vector3 값.
            /// </summary>
            [Tooltip("최대 Vector3 값")]
            public Vector3 Max;

            /// <summary>
            ///   최소 Vector3 값과 최대 Vector3 값 사이의 랜덤 Vector3 값을 반환합니다.
            /// </summary>
            /// <returns>최소 Vector3 값과 최대 Vector3 값 사이의 랜덤 Vector3 값</returns>
            public Vector3 Random()
            {
                return new Vector3(UnityEngine.Random.Range(Min.x, Max.x), UnityEngine.Random.Range(Min.y, Max.y), UnityEngine.Random.Range(Min.z, Max.z));
            }
        }

        /// <summary>
        ///   두 개의 Color32 값 사이의 랜덤 색상을 반환하는 구조체.
        /// </summary>
        [System.Serializable]
        public struct DuoColor
        {
            /// <summary>
            ///   첫 번째 색상.
            /// </summary>
            [Tooltip("첫 번째 색상")]
            public Color32 First;

            /// <summary>
            ///   두 번째 색상.
            /// </summary>
            [Tooltip("두 번째 색상")]
            public Color32 Second;

            /// <summary>
            ///   첫 번째 색상과 두 번째 색상 사이의 랜덤 Color32 값을 반환합니다.
            /// </summary>
            /// <returns>첫 번째 색상과 두 번째 색상 사이의 랜덤 Color32 값</returns>
            public Color32 RandomBetween()
            {
                return Color.Lerp(First, Second, UnityEngine.Random.value);
            }
        }
    }
}