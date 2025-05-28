// SavePresets.cs
// 이 스크립트는 게임 저장 프리셋을 생성, 로드, 삭제, 관리하는 기능을 제공하는 정적 유틸리티 클래스입니다.
// Unity 에디터 환경에서 플레이어의 게임 저장 데이터를 관리하고 디버깅하기 위해 사용됩니다.
// 프리셋은 Persistent Data Path 내의 별도 폴더에 저장됩니다.

using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement; // 에디터 씬 관리 기능을 위해 필요
using UnityEditor; // Unity 에디터 관련 기능에 필요
#endif
using UnityEngine; // Application.persistentDataPath 등 Unity 기능에 필요
using System.IO; // 파일 I/O 작업을 위해 필요

namespace Watermelon
{
    // 게임 저장 프리셋 관리 기능을 제공하는 정적 클래스입니다.
    public class SavePresets
    {
        // 프리셋 파일 경로의 접두사입니다.
        private const string PRESET_FOLDER_PREFIX = "SavePresets/";
        // 저장 프리셋 파일이 저장될 기본 폴더 이름입니다.
        private const string PRESETS_FOLDER_NAME = "SavePresets";
        // 기본 게임 저장 파일 이름입니다.
        private const string SAVE_FILE_NAME = "save";
        // 저장 데이터가 수정되어 UI 갱신이 필요함을 나타내는 플래그입니다. SavePresetsWindow에서 사용됩니다.
        public static bool saveDataMofied = false;
        // 경로 구분자 문자입니다.
        private const char SEPARATOR = '/';
        // 기본 저장 프리셋 디렉토리(탭)의 이름입니다.
        public const string DEFAULT_DIRECTORY = "Custom";
        // 메타 파일의 확장자 접미사입니다.
        public const string META_SUFFIX = ".meta";

        /// <summary>
        /// 지정된 경로의 저장 프리셋 파일을 로드하여 현재 게임 저장 데이터로 설정하는 함수입니다.
        /// 에디터 플레이 모드가 아닐 때만 작동합니다.
        /// </summary>
        /// <param name="presetPath">로드할 저장 프리셋 파일의 전체 경로</param>
        private static void LoadSaveFromPath(string presetPath)
        {
#if UNITY_EDITOR // 이 코드는 Unity 에디터에서만 컴파일됩니다.
            // 현재 에디터가 플레이 모드이면 로드를 허용하지 않고 오류를 출력합니다.
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated in playmode!");
                return;
            }

            // 에디터가 컴파일 중이면 로드를 허용하지 않고 오류를 출력합니다.
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated during compiling!");
                return;
            }

            // 지정된 경로에 프리셋 파일이 존재하지 않으면 오류를 출력합니다.
            if (!File.Exists(presetPath))
            {
                Debug.LogError(string.Format("[Save Presets]: Preset  at path {0} doesn’t  exist!", presetPath));
                return;
            }

            // 현재 활성화된 씬의 이름을 가져옵니다.
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 현재 씬이 초기화 씬("Init") 또는 레벨 에디터 씬("Level Editor")이면 게임 씬("Game.unity")을 엽니다.
            // 이는 프리셋 로드 후 게임이 올바른 상태로 시작되도록 하기 위함입니다.
            if (currentSceneName.Equals("Init") || (currentSceneName.Equals("Level Editor")))
            {
                EditorSceneManager.OpenScene(Path.Combine(CoreEditor.FOLDER_SCENES, "Game.unity"));
            }

            // 현재 게임 저장 파일을 프리셋 파일로 덮어씁니다. (true는 덮어쓰기 허용)
            File.Copy(presetPath, GetSavePath(), true);

            // 게임을 시작합니다 (에디터 플레이 모드 진입).
            EditorApplication.isPlaying = true;
#endif
        }

        /// <summary>
        /// 현재 게임 저장 데이터를 지정된 이름과 탭(폴더) 이름으로 저장 프리셋 파일로 생성하는 함수입니다.
        /// 에디터 플레이 모드인 경우 SaveController를 통해 저장하고, 아니면 현재 저장 파일을 복사합니다.
        /// </summary>
        /// <param name="saveName">생성할 저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장할 탭(폴더) 이름 (기본값: DEFAULT_DIRECTORY)</param>
        private static void CreateSavePreset(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
#if UNITY_EDITOR // 이 코드는 Unity 에디터에서만 컴파일됩니다.
            // 에디터 플레이 모드이면 현재 게임 상태를 SaveController를 통해 강제로 저장합니다. (스레드 사용 안 함)
            if (EditorApplication.isPlaying)
                SaveController.Save(true, false);

            // 저장 프리셋 이름이 비어 있으면 오류를 출력합니다.
            if (string.IsNullOrEmpty(saveName))
            {
                Debug.LogError("[Save Presets]: Preset name can't be empty!");
                return;
            }

            // 저장 프리셋의 기본 디렉토리("SavePresets")가 존재하지 않으면 생성합니다.
            if (!Directory.Exists(GetDirectoryPath()))
            {
                Directory.CreateDirectory(GetDirectoryPath());
            }

            // 지정된 탭(폴더) 디렉토리가 존재하지 않으면 생성합니다.
            if (!Directory.Exists(GetDirectoryPath(tabName)))
            {
                Directory.CreateDirectory(GetDirectoryPath(tabName));
            }

            // 현재 게임 저장 파일의 경로를 가져옵니다.
            string savePath = GetSavePath();

            // 생성할 저장 프리셋 파일의 경로를 가져옵니다.
            string presetPath = GetPresetPath(saveName, tabName);

            // 에디터 플레이 모드이면 SaveController의 PresetsSave 기능을 사용하여 현재 GlobalSave 객체를 프리셋 파일로 저장합니다.
            if (EditorApplication.isPlaying)
            {
                SaveController.PresetsSave(PRESET_FOLDER_PREFIX + tabName + SEPARATOR + saveName);
            }
            else // 에디터 플레이 모드가 아니면
            {
                // 현재 게임 저장 파일이 존재하지 않으면 오류를 출력합니다.
                if (!File.Exists(savePath))
                {
                    Debug.LogError("[Save Presets]: Save file doesn’t exist!");

                    return;
                }

                // 현재 게임 저장 파일을 지정된 프리셋 경로로 복사합니다. (true는 덮어쓰기 허용)
                File.Copy(savePath, presetPath, true);
            }

            // 생성된 프리셋 파일의 생성 날짜를 현재 시간으로 설정합니다.
            File.SetCreationTime(presetPath, DateTime.Now);

            // 저장 데이터가 수정되었음을 나타내는 플래그를 true로 설정하여 SavePresetsWindow가 갱신되도록 합니다.
            saveDataMofied = true;
#endif
        }

        /// <summary>
        /// 지정된 이름과 탭(폴더)의 저장 프리셋을 로드하는 정적 함수입니다.
        /// 내부적으로 LoadSaveFromPath 함수를 호출합니다.
        /// </summary>
        /// <param name="saveName">로드할 저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장 프리셋이 속한 탭(폴더) 이름 (기본값: DEFAULT_DIRECTORY)</param>
        public static void LoadSave(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
            // 지정된 이름과 탭으로 저장 프리셋 파일의 전체 경로를 가져옵니다.
            string presetPath = GetPresetPath(saveName, tabName);
            // 해당 경로의 저장 프리셋을 로드합니다.
            LoadSaveFromPath(presetPath);
        }

        /// <summary>
        /// 지정된 이름과 탭(폴더) 이름으로 저장 프리셋을 생성하는 정적 함수입니다.
        /// 선택적으로 고유 ID를 설정할 수 있습니다.
        /// 내부적으로 CreateSavePreset 함수를 호출하고 메타 파일에 ID를 저장합니다.
        /// </summary>
        /// <param name="saveName">생성할 저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장할 탭(폴더) 이름 (기본값: DEFAULT_DIRECTORY)</param>
        /// <param name="id">저장 프리셋의 고유 ID (기본값: 빈 문자열, saveName 사용)</param>
        public static void CreateSave(string saveName, string tabName = DEFAULT_DIRECTORY, string id = "")
        {
#if UNITY_EDITOR // 이 코드는 Unity 에디터에서만 컴파일됩니다.
            // ID가 비어 있으면 파일 이름을 ID로 사용합니다.
            if (id.Length == 0)
            {
                id = saveName;
            }

            // 저장 프리셋 파일을 생성합니다.
            CreateSavePreset(saveName, tabName);
            // 생성된 프리셋 파일의 메타 파일에 고유 ID를 설정합니다.
            SetId(saveName, tabName, id);
#endif
        }

        /// <summary>
        /// 지정된 이름과 탭(폴더)의 저장 프리셋 메타 파일에 고유 ID를 저장하는 함수입니다.
        /// </summary>
        /// <param name="name">저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장 프리셋이 속한 탭(폴더) 이름</param>
        /// <param name="id">저장할 고유 ID 문자열</param>
        public static void SetId(string name, string tabName, string id)
        {
            // 지정된 이름과 탭으로 프리셋 파일 경로를 가져오고 ".meta" 접미사를 추가하여 메타 파일 경로를 만듭니다.
            string presetPath = GetPresetPath(name, tabName) + META_SUFFIX;
            // 메타 파일에 고유 ID 문자열을 씁니다. (기존 내용 덮어쓰기)
            File.WriteAllText(presetPath, id);
        }

        /// <summary>
        /// 지정된 고유 ID를 가진 저장 프리셋 파일의 전체 경로를 찾아 반환하는 함수입니다.
        /// </summary>
        /// <param name="id">찾고자 하는 저장 프리셋의 고유 ID</param>
        /// <returns>해당 ID를 가진 프리셋 파일의 전체 경로, 찾지 못하면 빈 문자열</returns>
        private static string GetPresetPathById(string id)
        {
            // 저장 프리셋의 기본 디렉토리 경로를 가져옵니다.
            string directoryPath = SavePresets.GetDirectoryPath();
            // 기본 디렉토리 내의 모든 서브 디렉토리(탭) 목록을 가져옵니다.
            string[] directoryEntries = Directory.GetDirectories(directoryPath);
            string[] fileEntries;

            // 각 서브 디렉토리(탭)를 순회합니다.
            for (int i = 0; i < directoryEntries.Length; i++)
            {
                // 현재 디렉토리 내의 모든 파일 목록을 가져옵니다.
                fileEntries = Directory.GetFiles(directoryEntries[i]);

                // 현재 디렉토리 내의 각 파일을 순회합니다.
                for (int j = 0; j < fileEntries.Length; j++)
                {
                    // 파일이 메타 파일(.meta)인 경우에만 처리합니다.
                    if (fileEntries[j].EndsWith(SavePresets.META_SUFFIX))
                    {
                        // 메타 파일의 내용을 읽어와 지정된 ID와 비교합니다.
                        if (File.ReadAllText(fileEntries[j]).Equals(id))
                        {
                            // ID가 일치하면 메타 파일 경로에서 ".meta" 접미사를 제거하여 실제 프리셋 파일 경로를 반환합니다.
                            return fileEntries[j].Replace(SavePresets.META_SUFFIX,string.Empty);
                        }
                    }
                }
            }

            // 지정된 ID를 가진 프리셋을 찾지 못하면 빈 문자열을 반환합니다.
            return string.Empty;
        }

        /// <summary>
        /// 지정된 고유 ID를 가진 저장 프리셋을 로드하는 정적 함수입니다.
        /// 내부적으로 GetPresetPathById를 사용하여 경로를 찾고 LoadSaveFromPath를 호출합니다.
        /// </summary>
        /// <param name="id">로드할 저장 프리셋의 고유 ID</param>
        public static void LoadSaveById(string id)
        {
            // 지정된 ID를 가진 저장 프리셋 파일의 전체 경로를 가져옵니다.
            string presetPath = GetPresetPathById(id);

            // 프리셋 경로를 찾지 못했으면 오류를 출력하고 함수를 종료합니다.
            if (presetPath.Length == 0)
            {
                Debug.LogError(string.Format("[Save Presets]: Preset with id {0} doesn’t  exist!", id));
                return;
            }

            // 찾은 경로의 저장 프리셋을 로드합니다.
            LoadSaveFromPath(presetPath);
        }

        /// <summary>
        /// 지정된 이름과 탭(폴더)의 저장 프리셋 파일 및 해당 메타 파일을 삭제하는 정적 함수입니다.
        /// </summary>
        /// <param name="saveName">삭제할 저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장 프리셋이 속한 탭(폴더) 이름 (기본값: DEFAULT_DIRECTORY)</param>
        public static void RemoveSave(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
            // 지정된 이름과 탭으로 저장 프리셋 파일의 전체 경로를 가져옵니다.
            string presetPath = GetPresetPath(saveName, tabName);

            // 프리셋 파일이 존재하면 삭제합니다.
            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
            }

            // 메타 파일 경로를 만듭니다.
            presetPath += META_SUFFIX;

            // 메타 파일이 존재하면 삭제합니다.
            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
            }

            // 저장 데이터가 수정되었음을 나타내는 플래그를 true로 설정하여 SavePresetsWindow가 갱신되도록 합니다.
            saveDataMofied = true;
        }

        /// <summary>
        /// 지정된 이름과 탭(폴더)의 저장 프리셋 파일이 존재하는지 확인하는 함수입니다.
        /// </summary>
        /// <param name="saveName">확인할 저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장 프리셋이 속한 탭(폴더) 이름 (기본값: DEFAULT_DIRECTORY)</param>
        /// <returns>프리셋 파일이 존재하면 true, 그렇지 않으면 false</returns>
        public static bool IsSaveExist(string saveName, string tabName = DEFAULT_DIRECTORY)
        {
            // 지정된 이름과 탭으로 저장 프리셋 파일의 전체 경로를 가져옵니다.
            string presetPath = GetPresetPath(saveName, tabName);
            // 해당 경로에 파일이 존재하는지 확인하여 반환합니다.
            return File.Exists(presetPath);
        }

        /// <summary>
        /// 지정된 고유 ID를 가진 저장 프리셋 파일이 존재하는지 확인하는 함수입니다.
        /// ID에 해당하는 메타 파일은 있지만 실제 프리셋 파일이 없는 경우, 메타 파일을 삭제합니다.
        /// </summary>
        /// <param name="id">확인할 저장 프리셋의 고유 ID</param>
        /// <returns>프리셋 파일이 존재하면 true, 그렇지 않으면 false</returns>
        public static bool IsSaveExistById(string id)
        {
            // 지정된 ID를 가진 저장 프리셋 파일의 전체 경로를 가져옵니다.
            string presetPath = GetPresetPathById(id);

            // 프리셋 경로를 찾지 못했으면 (ID가 존재하지 않으면) false를 반환합니다.
            if(presetPath.Length == 0)
            {
                return false;
            }

            // 찾은 경로에 실제 프리셋 파일이 존재하는지 확인하여 반환합니다.
            if (File.Exists(presetPath))
            {
                return true;
            }
            else // 메타 파일은 있지만 실제 프리셋 파일이 없는 경우
            {
                // 해당 메타 파일을 삭제합니다.
                File.Delete(presetPath + META_SUFFIX);
                return false; // 파일이 존재하지 않으므로 false를 반환합니다.
            }
        }

        /// <summary>
        /// 지정된 고유 ID를 가진 저장 프리셋 파일 및 해당 메타 파일을 삭제하는 정적 함수입니다.
        /// </summary>
        /// <param name="id">삭제할 저장 프리셋의 고유 ID</param>
        public static void RemoveSaveById(string id)
        {
            // 지정된 ID를 가진 저장 프리셋 파일의 전체 경로를 가져옵니다.
            string presetPath = GetPresetPathById(id);

            // 프리셋 경로를 찾지 못했으면 (ID가 존재하지 않으면) 함수를 종료합니다.
            if (presetPath.Length == 0)
            {
                return;
            }

            // 실제 프리셋 파일이 존재하면 삭제합니다.
            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
            }

            // 메타 파일 경로를 만듭니다.
            presetPath += META_SUFFIX;

            // 메타 파일이 존재하면 삭제합니다.
            if (File.Exists(presetPath))
            {
                File.Delete(presetPath);
            }
            // 저장 데이터가 수정되었음을 나타내는 플래그를 true로 설정하여 SavePresetsWindow가 갱신되도록 합니다.
            saveDataMofied = true;
        }

        /// <summary>
        /// 기본 게임 저장 파일의 전체 경로(Persistent Data Path 내)를 가져오는 함수입니다.
        /// </summary>
        /// <returns>기본 게임 저장 파일의 전체 경로</returns>
        public static string GetSavePath()
        {
            // Application.persistentDataPath와 기본 저장 파일 이름을 결합하여 경로를 만듭니다.
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        /// <summary>
        /// 지정된 이름과 탭(폴더)의 저장 프리셋 파일 전체 경로를 가져오는 함수입니다.
        /// </summary>
        /// <param name="saveName">저장 프리셋 파일 이름</param>
        /// <param name="tabName">저장 프리셋이 속한 탭(폴더) 이름</param>
        /// <returns>저장 프리셋 파일의 전체 경로</returns>
        public static string GetPresetPath(string saveName, string tabName)
        {
            // Application.persistentDataPath, 프리셋 폴더 이름, 탭 이름, 파일 이름을 결합하여 경로를 만듭니다.
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME, tabName, saveName);
        }

        /// <summary>
        /// 저장 프리셋의 기본 디렉토리("SavePresets") 전체 경로를 가져오는 함수입니다.
        /// </summary>
        /// <returns>저장 프리셋 기본 디렉토리의 전체 경로</returns>
        public static string GetDirectoryPath()
        {
            // Application.persistentDataPath와 프리셋 폴더 이름을 결합하여 경로를 만듭니다.
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME);
        }

        /// <summary>
        /// 지정된 탭(폴더)의 전체 경로를 가져오는 함수입니다.
        /// </summary>
        /// <param name="tabName">경로를 가져올 탭(폴더) 이름</param>
        /// <returns>지정된 탭(폴더)의 전체 경로</returns>
        public static string GetDirectoryPath(string tabName)
        {
            // Application.persistentDataPath, 프리셋 폴더 이름, 탭 이름을 결합하여 경로를 만듭니다.
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME, tabName);
        }

        /// <summary>
        /// 주어진 파일 또는 디렉토리 경로에서 파일 이름만 가져오는 함수입니다.
        /// </summary>
        /// <param name="path">경로 문자열</param>
        /// <returns>경로의 마지막 부분 (파일 또는 디렉토리 이름)</returns>
        public static string GetFileName(string path)
        {
            // Path.GetFileName 유틸리티 함수를 사용하여 파일 이름만 가져옵니다.
            return Path.GetFileName(path);
        }

        /// <summary>
        /// 주어진 파일 또는 디렉토리 경로에서 디렉토리 경로만 가져오는 함수입니다.
        /// </summary>
        /// <param name="path">경로 문자열</param>
        /// <returns>경로에서 파일 이름을 제외한 디렉토리 부분</returns>
        public static string GetDirectoryName(string path)
        {
            // Path.GetDirectoryName 유틸리티 함수를 사용하여 디렉토리 경로만 가져옵니다.
            return Path.GetDirectoryName(path);
        }
    }
}