using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.UI.Particle
{
    /// <summary>
    ///   UI 파티클 관리 클래스.
    ///   개별 파티클의 생성, 이동, 소멸 등을 처리합니다.
    /// </summary>
    public class UIParticle : MonoBehaviour
    {
        /// <summary>
        ///   파티클 이미지 컴포넌트.
        /// </summary>
        [Tooltip("파티클 이미지 컴포넌트")]
        [SerializeField] Image image;

        /// <summary>
        ///   파티클 생성 시간.
        /// </summary>
        public float spawnTime;

        /// <summary>
        ///   파티클 생존 시간.
        /// </summary>
        public float lifetime;

        /// <summary>
        ///   파티클 시작 색상.
        /// </summary>
        public Color32 startColor;

        /// <summary>
        ///   파티클 시작 크기.
        /// </summary>
        public Vector2 startSize;

        /// <summary>
        ///   파티클 속도.
        /// </summary>
        public Vector2 velocity;

        /// <summary>
        ///   파티클 감속도.
        /// </summary>
        public float dumping;

        /// <summary>
        ///   파티클 각속도.
        /// </summary>
        public Vector3 angularVelocity;

        /// <summary>
        ///   파티클 활성화 여부.
        ///   true이면 활성화, false이면 비활성화.
        /// </summary>
        public bool IsActive { get => gameObject.activeInHierarchy; set => gameObject.SetActive(value); }

        /// <summary>
        ///   파티클 스프라이트.
        /// </summary>
        public Sprite Sprite { get => image.sprite; set => image.sprite = value; }

        /// <summary>
        ///   파티클 크기.
        /// </summary>
        public Vector2 Size { get => image.rectTransform.sizeDelta; set => image.rectTransform.sizeDelta = value; }

        /// <summary>
        ///   파티클 위치.
        /// </summary>
        public Vector2 AnchoredPosition { get => image.rectTransform.anchoredPosition; set => image.rectTransform.anchoredPosition = value; }

        /// <summary>
        ///   파티클 회전 각도.
        /// </summary>
        public Vector3 EulerAngles { get => image.rectTransform.eulerAngles; set => image.rectTransform.eulerAngles = value; }

        /// <summary>
        ///   파티클 색상.
        /// </summary>
        public Color32 Color { get => image.color; set => image.color = value; }

        /// <summary>
        ///   파티클 설정.
        /// </summary>
        public UIParticleSettings Settings { get; private set; }

        /// <summary>
        ///   파티클 초기화.
        ///   파티클 설정, 스프라이트, 생성 시간, 생존 시간, 속도, 감속도, 색상, 크기 등을 설정합니다.
        /// </summary>
        /// <param name="settings">파티클 설정</param>
        /// <param name="timeSpend">생성 지연 시간</param>
        /// <param name="targetAnchoredPosition">생성 위치</param>
        /// <param name="normalizedVelocity">이동 방향 (정규화된 값)</param>
        public void Init(UIParticleSettings settings, float timeSpend, Vector2 targetAnchoredPosition, Vector2 normalizedVelocity)
        {
            Settings = settings;

            Sprite = settings.sprite;

            spawnTime = Time.time - timeSpend;

            lifetime = settings.lifetime.Random();

            // 3D 속도 사용 여부에 따라 속도 계산
            if (settings.speed3d)
            {
                velocity = settings.speed3dValues.Random();
            }
            else
            {
                velocity = Quaternion.Euler(0, 0, settings.angle.Random()) * normalizedVelocity * settings.speed.Random();
            }

            // 특정 위치에서 생성되는 경우 속도 방향 보정
            if (targetAnchoredPosition != Vector2.zero)
            {
                velocity = Vector2.Lerp(velocity.normalized, targetAnchoredPosition.normalized, settings.spherizeDirection) * velocity.magnitude;
            }

            dumping = settings.dumping.Random();

            startColor = settings.startDuoColor.RandomBetween();
            Color = startColor;

            // 수명에 따른 색상 변화 사용 시 색상 보정
            if (settings.colorOverLifetime)
            {
                Color *= settings.colorOverLifetimeGradient.Evaluate(0);
            }

            // 수명에 따른 회전 사용 시 각속도 설정
            if (settings.rotationOverLifetime)
            {
                angularVelocity = new Vector3(0, 0, settings.angularSpeed.Random());
            }
            else
            {
                angularVelocity = Vector3.zero;
            }

            AnchoredPosition = targetAnchoredPosition + velocity * timeSpend;

            startSize = Vector2.one * settings.startSize.Random();
            Size = startSize;
        }

        /// <summary>
        ///   파티클 업데이트.
        ///   시간 경과에 따라 파티클 이동, 회전, 크기, 색상 등을 변경하고,
        ///   생존 시간이 다 되면 true를 반환하여 파티클 제거를 알립니다.
        /// </summary>
        /// <returns>파티클 생존 여부 (true: 소멸, false: 생존)</returns>
        public bool Tick()
        {
            var time = Time.time - spawnTime;

            if (lifetime <= time) return true;

            velocity += Vector2.down * Settings.gravityModifier * Time.deltaTime;

            AnchoredPosition += velocity * Time.deltaTime;
            EulerAngles += angularVelocity * Time.deltaTime;

            var t = time / lifetime;

            Size = startSize * Settings.scaleCurve.Evaluate(t);

            // 수명에 따른 색상 변화 적용
            if (Settings.colorOverLifetime)
            {
                Color = startColor * Settings.colorOverLifetimeGradient.Evaluate(t);
            }

            return false;
        }
    }
}