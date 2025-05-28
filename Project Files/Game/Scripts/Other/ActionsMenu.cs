// 이 스크립트는 Unity 에디터의 "Actions" 메뉴에 개발 및 테스트용 단축 기능을 추가합니다.
// 경험치 레벨 변경, 모든 무기 해금, 레벨 건너뛰기 등의 메뉴 아이템을 포함하며,
// #if UNITY_EDITOR 전처리기 지시문을 사용하여 에디터 환경에서만 컴파일되도록 합니다.
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    // Unity 에디터의 "Actions" 메뉴에 개발 도구를 추가하는 정적 클래스입니다.
    // 이 클래스는 에디터 환경에서만 유효합니다.
    public static class ActionsMenu
    {
#if UNITY_EDITOR
        // "Actions/Next XP Lvl" 메뉴 아이템을 생성하고 우선순위를 설정합니다.
        // 게임 실행 중에 현재 경험치 레벨을 1 올립니다. (개발용)
        [MenuItem("Actions/Next XP Lvl", priority = 36)]
        private static void GetNextLevel()
        {
            // 게임이 실행 중일 때만 작동합니다.
            if (Application.isPlaying)
            {
                // 경험치 컨트롤러를 사용하여 다음 레벨로 설정합니다. (개발용 메소드)
                ExperienceController.SetLevelDev(ExperienceController.CurrentLevel + 1);
            }
        }

        // "Actions/Set No XP" 메뉴 아이템을 생성하고 우선순위를 설정합니다.
        // 게임 실행 중에 경험치 레벨을 1로 초기화합니다. (개발용)
        [MenuItem("Actions/Set No XP", priority = 37)]
        private static void NoXP()
        {
            // 게임이 실행 중일 때만 작동합니다.
            if (Application.isPlaying)
            {
                // 경험치 컨트롤러를 사용하여 레벨을 1로 설정합니다. (개발용 메소드)
                ExperienceController.SetLevelDev(1);
            }
        }

        // "Actions/All Weapons" 메뉴 아이템을 생성하고 우선순위를 설정합니다.
        // 게임 실행 중에 모든 무기를 해금하고 UI를 업데이트합니다. (개발용)
        [MenuItem("Actions/All Weapons", priority = 51)]
        private static void UnlockAllWeapons()
        {
            // 게임이 실행 중일 때만 작동합니다.
            if (Application.isPlaying)
            {
                // 무기 컨트롤러를 사용하여 모든 무기를 해금합니다. (개발용 메소드)
                WeaponsController.UnlockAllWeaponsDev();
                // 무기 UI 페이지를 가져와 UI를 업데이트합니다.
                UIController.GetPage<UIWeaponPage>().UpdateUI();
            }
        }

        // "Actions/Prev Level (menu) [P]" 메뉴 아이템을 생성하고 우선순위를 설정합니다.
        // 현재 레벨에서 이전 레벨로 이동합니다. (메뉴에서 사용될 것으로 예상되는 개발용 기능)
        [MenuItem("Actions/Prev Level (menu) [P]", priority = 71)]
        public static void PrevLevel()
        {
            // 레벨 컨트롤러를 사용하여 이전 레벨로 이동합니다. (개발용 메소드)
            LevelController.PrevLevelDev();
        }

        // "Actions/Next Level (menu) [N]" 메뉴 아이템을 생성하고 우선순위를 설정합니다.
        // 현재 레벨에서 다음 레벨로 이동합니다. (메뉴에서 사용될 것으로 예상되는 개발용 기능)
        [MenuItem("Actions/Next Level (menu) [N]", priority = 72)]
        public static void NextLevel()
        {
            // 레벨 컨트롤러를 사용하여 다음 레벨로 이동합니다. (개발용 메소드)
            LevelController.NextLevelDev();
        }

        // "Actions/Print Shorcuts" 메뉴 아이템을 생성하고 우선순위를 설정합니다.
        // 콘솔에 개발용 단축키 목록을 출력합니다.
        [MenuItem("Actions/Print Shorcuts", priority = 150)]
        private static void PrintShortcuts()
        {
            // Debug.Log를 사용하여 콘솔에 단축키 정보를 출력합니다.
            Debug.Log("H - heal player \nD - toggle player damage \nN - skip level\nR - skip room\n\n");
        }
        
                [MenuItem("Actions/Unlock All Characters", priority = 52)]
        private static void UnlockAllCharacters()
        {
            // 플레이 모드에서만 동작
            if (!Application.isPlaying) return;

            // 데이터베이스에서 가장 높은 해금 레벨 추출
            var db = CharactersController.GetDatabase();
            int maxRequired = db.Characters.Max(c => c.RequiredLevel);

            // 플레이어 레벨을 최대로 설정 → 모든 캐릭터 해금
            ExperienceController.SetLevelDev(maxRequired);

            // UI가 열려 있다면 즉시 갱신 (페이지 재초기화)
            var page = UIController.GetPage<UICharactersPanel>();
            if (page != null)
                page.Init(); 
        }
#endif
    }
}