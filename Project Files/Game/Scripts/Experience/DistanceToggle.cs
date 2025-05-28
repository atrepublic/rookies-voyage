// ==============================================
// 📌 DistanceToggle.cs
// ✅ 플레이어와의 거리 기반으로 오브젝트 활성화/비활성화 토글 처리 시스템
// ✅ IDistanceToggle 인터페이스를 구현한 오브젝트들을 갱신 대상에 등록하고 거리 조건에 따라 상태 전환
// ==============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class DistanceToggle
    {
        [Tooltip("거리 기반으로 토글 처리될 오브젝트 목록")]
        private static List<IDistanceToggle> distanceToggles = new List<IDistanceToggle>();

        private static int distanceTogglesCount;

        [Tooltip("거리 토글 시스템 활성화 여부")]
        private static bool isActive;
        public static bool IsActive => isActive;

        private static Vector3 tempDistance;
        private static float tempDistanceMagnitude;
        private static bool tempIsVisible;

        private static Transform playerTransform;

        private static Coroutine updateCoroutine;

        /// <summary>
        /// 📌 DistanceToggle 시스템 초기화 (플레이어 트랜스폼 지정)
        /// </summary>
        public static void Init(Transform transform)
        {
            playerTransform = transform;
            distanceToggles = new List<IDistanceToggle>();
            distanceTogglesCount = 0;
            isActive = true;

            updateCoroutine = Tween.InvokeCoroutine(UpdateCoroutine());
        }

        /// <summary>
        /// 📌 거리 갱신 코루틴 (프레임마다 토글 상태 검사)
        /// </summary>
        private static IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                if (isActive)
                {
                    for (int i = 0; i < distanceTogglesCount; i++)
                    {
                        if (!distanceToggles[i].IsShowing)
                            continue;

                        tempIsVisible = distanceToggles[i].IsVisible;

                        tempDistance = playerTransform.position - distanceToggles[i].DistancePointPosition;
                        tempDistance.y = 0;
                        tempDistanceMagnitude = tempDistance.magnitude;

                        if (!tempIsVisible && tempDistanceMagnitude <= distanceToggles[i].ShowingDistance)
                        {
                            distanceToggles[i].PlayerEnteredZone();
                        }
                        else if (tempIsVisible && tempDistanceMagnitude > distanceToggles[i].ShowingDistance)
                        {
                            distanceToggles[i].PlayerLeavedZone();
                        }
                    }
                }

                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
            }
        }

        public static void AddObject(IDistanceToggle toggle)
        {
            distanceToggles.Add(toggle);
            distanceTogglesCount++;
        }

        public static void RemoveObject(IDistanceToggle toggle)
        {
            distanceToggles.Remove(toggle);
            distanceTogglesCount--;
        }

        /// <summary>
        /// 📌 지정된 토글 오브젝트가 거리 안에 있는지 확인
        /// </summary>
        public static bool IsInRange(IDistanceToggle toggle)
        {
            tempDistance = playerTransform.position - toggle.DistancePointPosition;
            tempDistance.y = 0;
            tempDistanceMagnitude = tempDistance.magnitude;
            return tempDistanceMagnitude <= toggle.ShowingDistance;
        }

        public static void Enable() => isActive = true;
        public static void Disable() => isActive = false;

        /// <summary>
        /// 📌 시스템 언로드 시 코루틴 종료
        /// </summary>
        public static void Unload()
        {
            if (updateCoroutine != null)
                Tween.StopCustomCoroutine(updateCoroutine);

            isActive = false;
        }
    }
}
