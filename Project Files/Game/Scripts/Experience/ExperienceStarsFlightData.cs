// ==============================================
// 📌 ExperienceStarsFlightData.cs
// ✅ 경험치 획득 시 별 아이콘이 날아가는 연출을 제어하는 데이터
// ✅ Linear + Bezier 경로 설정 및 연출 커브를 제어
// ==============================================

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Data/UI Particles/Stars Flight Data", fileName = "Stars Flight Data")]
    public class ExperienceStarsFlightData : ScriptableObject
    {
        [Header("1단계 (직선 이동)")]
        [Tooltip("1단계 이동 경로 커브")]
        [SerializeField] private AnimationCurve pathCurve1;

        [Tooltip("1단계 별 크기 변화 커브")]
        [SerializeField] private AnimationCurve starsScale1;

        public AnimationCurve PathCurve1 => pathCurve1;
        public AnimationCurve StarsScale1 => starsScale1;

        [Space]
        [Tooltip("1단계 이동 거리 (랜덤 범위)")]
        [SerializeField] private DuoFloat firstStageDistance;

        [Tooltip("1단계 소요 시간 (랜덤 범위)")]
        [SerializeField] private DuoFloat firstStageDuration;

        public float FirstStageDistance => firstStageDistance.Random();
        public float FirstStageDuration => firstStageDuration.Random();

        [Header("2단계 (베지어 이동)")]
        [Tooltip("2단계 이동 경로 커브")]
        [SerializeField] private AnimationCurve pathCurve2;

        [Tooltip("2단계 별 크기 변화 커브")]
        [SerializeField] private AnimationCurve starsScale2;

        public AnimationCurve PathCurve2 => pathCurve2;
        public AnimationCurve StarsScale2 => starsScale2;

        [Space]
        [Tooltip("2단계 시작 시 곡선 강도 키값")]
        [SerializeField] private DuoFloat key1;

        [Tooltip("2단계 키 포인트 위치값")]
        [SerializeField] private DuoVector3 key2;

        public float Key1 => key1.Random();
        public Vector2 Key2 => key2.Random();

        [Space]
        [Tooltip("2단계 소요 시간 (랜덤 범위)")]
        [SerializeField] private DuoFloat secondStageDuration;

        public float SecondStageDuration => secondStageDuration.Random();
    }
}
