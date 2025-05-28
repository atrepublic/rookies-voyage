/*================================================================
 * TutorialHelper.cs
 * ----------------------------------------------------------------
 * 📌 기능 요약
 *  - Unity 에디터 상단 메뉴에 "Actions/Skip Tutorial" 항목을 추가한다.
 *  - 체크 여부를 EditorPrefs 로 저장하며, 플레이 중 실시간으로 TutorialController
 *    의 _isTutorialSkipped 값을 갱신하여 튜토리얼을 스킵할 수 있다.
 *  - 런타임(빌드)에서는 항상 false 를 반환한다.
 * ================================================================*/

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// ⭐ 에디터 전용 튜토리얼 스킵 토글 도우미
    /// </summary>
    public static class TutorialHelper
    {
        private const string MENU_NAME   = "Actions/Skip Tutorial"; // 메뉴 경로
        private const string PREFS_KEY   = "IsTutorialSkipped";     // EditorPrefs 키

        /// <summary>
        /// 🔹 에디터/런타임에서 스킵 상태를 조회한다.
        /// </summary>
        public static bool IsTutorialSkipped()
        {
#if UNITY_EDITOR
            return EditorSkipState;
#else
            return false; // 플레이어 빌드에서는 항상 false
#endif
        }

#if UNITY_EDITOR
        /*───────────────────────────────────────────────────────────
         * 📌 에디터 전용 구현 (MenuItem)
         *──────────────────────────────────────────────────────────*/

        /// <summary>
        /// EditorPrefs 에 저장된 스킵 플래그 (프로퍼티 래퍼)
        /// </summary>
        private static bool EditorSkipState
        {
            get => EditorPrefs.GetBool(PREFS_KEY, false);
            set => EditorPrefs.SetBool(PREFS_KEY, value);
        }

        /// <summary>
        /// 메뉴 클릭 시 스킵 상태 토글
        /// </summary>
        [MenuItem(MENU_NAME, priority = 200)]
        private static void ToggleSkipState()
        {
            bool current = EditorSkipState;
            EditorSkipState = !current;

            // 플레이 모드 중이면 TutorialController 의 정적 필드도 즉시 갱신
            if (Application.isPlaying)
            {
                typeof(TutorialController)
                    .GetField("isTutorialSkipped", BindingFlags.NonPublic | BindingFlags.Static)
                    ?.SetValue(null, !current);
            }
        }

        /// <summary>
        /// 메뉴 항목 좌측 체크박스 상태 갱신
        /// </summary>
        [MenuItem(MENU_NAME, true, priority = 200)]
        private static bool ToggleSkipStateValidate()
        {
            Menu.SetChecked(MENU_NAME, EditorSkipState);
            return true;
        }
#endif
    }
}
