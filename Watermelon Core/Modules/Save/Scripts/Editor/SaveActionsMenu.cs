// SaveActionsMenu.cs
// 이 스크립트는 Unity 에디터의 'Actions' 메뉴에 게임 저장 데이터 관련 디버그 액션을 추가합니다.
// 플레이 모드가 아닐 때(에디터 상태) 저장 파일을 삭제하는 기능을 제공합니다.

using UnityEngine;
using Watermelon;
using UnityEditor;

namespace Watermelon
{
    // 게임 저장 데이터 관련 에디터 메뉴 항목을 제공하는 정적 클래스입니다.
    public static class SaveActionsMenu
    {
        /// <summary>
        /// 에디터 메뉴에 'Actions/Remove Save' 및 'Edit/Clear Save' 항목을 추가하고,
        /// 메뉴 선택 시 PlayerPrefs와 게임 저장 파일을 모두 삭제하는 함수입니다.
        /// </summary>
        // Unity 에디터 메뉴에 항목을 추가합니다. 우선순위를 설정하여 메뉴 순서를 제어합니다.
        [MenuItem("Actions/Remove Save", priority = 1)] // 'Actions' 메뉴의 첫 번째 항목으로 표시
        [MenuItem("Edit/Clear Save", priority = 270)] // 'Edit' 메뉴의 'Clear' 섹션에 표시
        private static void RemoveSave()
        {
            // Unity PlayerPrefs에 저장된 모든 데이터를 삭제합니다.
            PlayerPrefs.DeleteAll();
            // SaveController를 사용하여 게임 저장 파일을 삭제합니다.
            SaveController.DeleteSaveFile();

            // 콘솔에 저장 파일이 삭제되었음을 로그합니다.
            Debug.Log("Save files are removed!");
        }

        /// <summary>
        /// 'Actions/Remove Save' 메뉴 항목의 유효성을 검사하는 함수입니다.
        /// 플레이 모드가 아닐 때만 메뉴 항목을 활성화합니다.
        /// </summary>
        /// <returns>현재 에디터가 플레이 모드가 아니면 true, 플레이 모드이면 false</returns>
        // 메뉴 항목의 유효성 검사를 위한 함수임을 지정합니다. true를 반환하면 메뉴가 활성화됩니다.
        [MenuItem("Actions/Remove Save", true)]
        private static bool RemoveSaveValidation()
        {
            // 현재 애플리케이션이 플레이 모드인지 확인하고, 플레이 모드가 아닐 때만 true를 반환합니다.
            return !Application.isPlaying;
        }
    }
}