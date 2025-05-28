/*================================================================
 * TutorialController.cs
 * ----------------------------------------------------------------
 * 📌 기능 요약
 *  - 프로젝트 전역에서 튜토리얼(ITutorial 구현체)을 등록·관리한다.
 *  - NavigationArrowController 초기화 및 TutorialLabel 풀링을 담당한다.
 *  - 에디터 단축 메뉴( TutorialHelper )와 연동해 "튜토리얼 스킵" 기능 제공.
 *  - 기존 로직은 그대로 유지하고, 한글 주석·툴팁만 추가하였다.
 * ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    /// <summary>
    /// ⭐ 튜토리얼 전반을 제어하는 싱글턴형 컨트롤러
    /// </summary>
    [StaticUnload]
    public class TutorialController : MonoBehaviour
    {
        /*───────────────────────────────────────────────────────────
         * 📌 정적/인스턴스 필드
         *──────────────────────────────────────────────────────────*/

        private static TutorialController tutorialController;                     // 싱글턴 캐시
        private static List<ITutorial>    registeredTutorials = new();             // 등록된 튜토리얼 목록

        [UnpackNested]
        [SerializeField, Tooltip("NavigationArrowController 참조 (중첩 해제)")]
        private NavigationArrowController navigationArrowController;              // 네비게이션 화살표 컨트롤러

        [Space]
        [SerializeField, Tooltip("TutorialLabelBehaviour 를 포함한 라벨 프리팹")]
        private GameObject labelPrefab;                                           // 라벨 프리팹

        private static Pool labelPool;                                            // 라벨 오브젝트 풀

        private static bool isTutorialSkipped;                                    // 스킵 여부 플래그

        /*───────────────────────────────────────────────────────────
         * 📌 Unity 이벤트
         *──────────────────────────────────────────────────────────*/

        /// <summary>
        /// 🔹 초기화 – 풀 생성 및 화살표 컨트롤러 세팅
        /// </summary>
        public void Init()
        {
            tutorialController = this;

            isTutorialSkipped = TutorialHelper.IsTutorialSkipped();

            // 라벨 풀 생성
            labelPool = new Pool(labelPrefab, labelPrefab.name);

            // 화살표 컨트롤러 초기화
            navigationArrowController.Init();
        }

        private void LateUpdate()
        {
            navigationArrowController.LateUpdate();
        }

        private void OnDestroy()
        {
            labelPool?.Destroy();

            tutorialController.navigationArrowController.Unload();

            if(!registeredTutorials.IsNullOrEmpty())
            {
                foreach(ITutorial tutorial in registeredTutorials)
                    tutorial.Unload();

                registeredTutorials.Clear();
            }
        }

        /*───────────────────────────────────────────────────────────
         * 📌 튜토리얼 관리 Static API
         *──────────────────────────────────────────────────────────*/

        /// <summary>
        /// 🔸 튜토리얼 ID 로 검색하여 반환 (필요 시 Init 자동 수행)
        /// </summary>
        public static ITutorial GetTutorial(TutorialID tutorialID)
        {
            for(int i = 0; i < registeredTutorials.Count; i++)
            {
                if (registeredTutorials[i].TutorialID == tutorialID)
                {
                    if (!registeredTutorials[i].IsInitialised)
                        registeredTutorials[i].Init();

                    if (isTutorialSkipped)
                        registeredTutorials[i].FinishTutorial();

                    return registeredTutorials[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 🔸 튜토리얼을 강제 활성화 (Init 포함)
        /// </summary>
        public static void ActivateTutorial(ITutorial tutorial)
        {
            if (!tutorial.IsInitialised)
                tutorial.Init();

            if (isTutorialSkipped)
                tutorial.FinishTutorial();
        }

        /// <summary>
        /// 🔸 튜토리얼 등록 (중복 방지)
        /// </summary>
        public static void RegisterTutorial(ITutorial tutorial)
        {
            if (registeredTutorials.Contains(tutorial))
                return;

            registeredTutorials.Add(tutorial);
        }

        /// <summary>
        /// 🔸 튜토리얼 제거
        /// </summary>
        public static void RemoveTutorial(ITutorial tutorial)
        {
            registeredTutorials.Remove(tutorial);
        }

        /// <summary>
        /// 🔸 새 튜토리얼 라벨을 풀에서 받아 활성화 후 반환
        /// </summary>
        public static TutorialLabelBehaviour CreateTutorialLabel(string text, Transform parentTransform, Vector3 offset)
        {
            GameObject labelObject = labelPool.GetPooledObject();
            labelObject.transform.position = parentTransform.position + offset;

            TutorialLabelBehaviour tutorialLabelBehaviour = labelObject.GetComponent<TutorialLabelBehaviour>();
            tutorialLabelBehaviour.Activate(text, parentTransform, offset);

            return tutorialLabelBehaviour;
        }

        /// <summary>
        /// 🔸 StaticUnload 특성에 의해 호출 – 정적 필드 리셋
        /// </summary>
        private static void UnloadStatic()
        {
            registeredTutorials.Clear();
            labelPool          = null;
            isTutorialSkipped  = false;
        }
    }
}
