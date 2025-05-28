// ==============================================
// 📌 ExperienceStarsManager.cs
// ✅ 경험치 획득 시 UI 별 아이콘 애니메이션과 이펙트 연출을 담당
// ✅ 스타 UI 생성, 이동 경로 계산, 파티클 처리 및 UI 갱신 연동
// ==============================================

using System.Collections.Generic;
using UnityEngine;
using Watermelon.UI.Particle;

namespace Watermelon
{
    public class ExperienceStarsManager : MonoBehaviour
    {
        private const string TRAIL_POOL_NAME = "Custom UI Trail";
        private const string PARTICLE_POOL_NAME = "Custom UI Particle";

        [Header("별 이동 연출 데이터")]
        [Tooltip("별의 비행 경로 및 스케일 등을 정의한 ScriptableObject")]
        [SerializeField] private ExperienceStarsFlightData starsData;

        [Header("별 UI 요소")]
        [Tooltip("별 아이콘이 배치될 부모 RectTransform")]
        [SerializeField] private RectTransform starsHolder;

        [Tooltip("별 UI 프리팹")]
        [SerializeField] private GameObject starUIPrefab;

        [Tooltip("도착 지점 아이콘 Transform")]
        [SerializeField] private Transform starIconTransform;

        [Tooltip("도착 시 아이콘에 Bounce 연출")]
        [SerializeField] private JuicyBounce starIconBounce;

        [Header("파티클 이펙트")]
        [Tooltip("파티클을 배치할 부모 RectTransform")]
        [SerializeField] private RectTransform particlesParent;

        [Tooltip("트레일 파티클 프리팹")]
        [SerializeField] private GameObject trailPrefab;

        [Tooltip("별 파티클 프리팹")]
        [SerializeField] private GameObject particlePrefab;

        private PoolGeneric<UIParticleTrail> trailPool;
        private PoolGeneric<UIParticle> particlePool;
        private Pool starsPool;

        private List<ExpStarData> starsInfo = new List<ExpStarData>();
        private System.Action OnComplete;

        private ExperienceUIController experienceUIController;

        /// <summary>
        /// 📌 초기화: 별/파티클 풀 생성 및 Bounce 연출 연결
        /// </summary>
        public void Init(ExperienceUIController experienceUIController)
        {
            this.experienceUIController = experienceUIController;

            AssignPools();
            starIconBounce.Init(starIconTransform);
        }

        /// <summary>
        /// 📌 오브젝트 풀 생성
        /// </summary>
        private void AssignPools()
        {
            trailPool = new PoolGeneric<UIParticleTrail>(trailPrefab, TRAIL_POOL_NAME, particlesParent);
            particlePool = new PoolGeneric<UIParticle>(particlePrefab, PARTICLE_POOL_NAME, particlesParent);
            starsPool = new Pool(starUIPrefab, starUIPrefab.name, transform);
        }

        /// <summary>
        /// 📌 객체 파괴 시 풀 제거
        /// </summary>
        private void OnDestroy()
        {
            if (trailPool != null) PoolManager.DestroyPool(trailPool);
            if (particlePool != null) PoolManager.DestroyPool(particlePool);
            if (starsPool != null) PoolManager.DestroyPool(starsPool);
        }

        /// <summary>
        /// 📌 지정한 수 만큼의 별을 화면 중앙에서 도착 지점까지 애니메이션 실행
        /// </summary>
        public void PlayXpGainedAnimation(int starsAmount, Vector3 screenView, System.Action OnComplete = null)
        {
            this.OnComplete = OnComplete;

            starsAmount = Mathf.Clamp(starsAmount, 1, 10);

            for (int i = 0; i < starsAmount; i++)
            {
                RectTransform starRect = starsPool.GetPooledObject().GetComponent<RectTransform>();
                starRect.SetParent(transform.parent);

                Vector3 worldPos = Camera.main.ViewportToWorldPoint(screenView);
                starRect.anchoredPosition = Camera.main.WorldToScreenPoint(worldPos) +
                    new Vector3(Random.Range(-25f, 25f), Random.Range(-25f, 25f), 0f);

                starRect.SetParent(starsHolder);

                Vector2 startDir = Random.insideUnitCircle.normalized;
                Vector2 endPoint = Vector2.zero;

                var data = new ExpStarData()
                {
                    star = starRect,
                    startPoint = starRect.anchoredPosition,
                    middlePoint = starRect.anchoredPosition + startDir * starsData.FirstStageDistance,
                    key1 = starRect.anchoredPosition + startDir * starsData.Key1,
                    key2 = endPoint + starsData.Key2,
                    endPoint = endPoint,
                    startTime = Time.time,
                    duration1 = starsData.FirstStageDuration,
                    duration2 = starsData.SecondStageDuration
                };

                data.SetCurves(starsData);

                var trail = trailPool.GetPooledComponent();
                trail.SetPool(particlePool);
                trail.AnchoredPos = starRect.anchoredPosition;
                trail.transform.localScale = Vector3.one;

                data.SetTrail(trail);
                starsInfo.Add(data);
            }
        }

        /// <summary>
        /// 📌 프레임마다 별 애니메이션 경로 업데이트
        /// </summary>
        private void Update()
        {
            if (starsInfo.Count == 0) return;

            for (int i = 0; i < starsInfo.Count; i++)
            {
                var data = starsInfo[i];

                if (data.Update())
                {
                    starsInfo.RemoveAt(i);
                    i--;

                    starIconBounce.Bounce();
                    experienceUIController.OnStarHitted();
                }
            }

            if (starsInfo.Count == 0)
                OnComplete?.Invoke();
        }

        #region 테스트용 버튼
        [Button] public void Spawn2Stars() => ExperienceController.GainExperience(2);
        [Button] public void Spawn5Stars() => ExperienceController.GainExperience(5);
        [Button] public void Spawn10Stars() => ExperienceController.GainExperience(10);
        #endregion

        /// <summary>
        /// 📌 별의 비행 데이터를 담는 내부 클래스
        /// </summary>
        private class ExpStarData
        {
            public RectTransform star;

            public Vector2 startPoint, middlePoint;
            public Vector2 key1, key2;
            public Vector2 endPoint;

            public float startTime, duration1, duration2;

            private ExperienceStarsFlightData data;
            private UIParticleTrail trail;

            public void SetCurves(ExperienceStarsFlightData data)
            {
                this.data = data;
            }

            public void SetTrail(UIParticleTrail trail)
            {
                this.trail = trail;
                trail.Init();
                trail.IsPlaying = true;
            }

            /// <summary>
            /// 📌 별 이동 경로 업데이트
            /// </summary>
            public bool Update()
            {
                float elapsed = Time.time - startTime;

                if (elapsed > duration1)
                {
                    float t = (elapsed - duration1) / duration2;
                    if (t >= 1f)
                    {
                        star.gameObject.SetActive(false);
                        trail.DisableWhenReady = true;
                        trail.IsPlaying = false;
                        return true;
                    }

                    SecondStageUpdate(t);
                }
                else
                {
                    float t = elapsed / duration1;
                    FirstStageUpdate(t);
                }

                return false;
            }

            /// <summary>
            /// 📌 1단계: 직선 이동
            /// </summary>
            public void FirstStageUpdate(float t)
            {
                var prevPos = star.anchoredPosition;

                star.anchoredPosition = Vector2.Lerp(startPoint, middlePoint, data.PathCurve1.Evaluate(t));
                star.localScale = Vector3.one * data.StarsScale1.Evaluate(t);

                trail.NormalizedVelocity = (star.anchoredPosition - prevPos).normalized;
                trail.AnchoredPos = star.anchoredPosition;
            }

            /// <summary>
            /// 📌 2단계: 베지어 곡선 이동
            /// </summary>
            public void SecondStageUpdate(float t)
            {
                var prevPos = star.anchoredPosition;

                star.anchoredPosition = Bezier.EvaluateCubic(middlePoint, key1, key2, endPoint, data.PathCurve2.Evaluate(t));
                star.localScale = Vector3.one * data.StarsScale2.Evaluate(t);

                trail.NormalizedVelocity = (star.anchoredPosition - prevPos).normalized;
                trail.AnchoredPos = star.anchoredPosition;
            }
        }
    }
}
