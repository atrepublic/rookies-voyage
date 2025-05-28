// 스크립트 기능 요약:
// 이 스크립트는 Unity의 Asset Postprocessor를 상속받아 에셋 임포트 또는 삭제 시 이벤트를 처리합니다.
// 스크립트(.cs) 또는 어셈블리(.dll) 파일 변경이 감지되면 DefineManager를 통해 자동 정의 심볼(Auto Defines)을 확인하도록 플래그를 설정합니다.
// 또한, 스크립트 리로드 완료 시 자동 정의 심볼을 확인하는 기능을 수행하여, 코드 변경에 따라 필요한 정의 심볼이 자동으로 설정되도록 돕습니다.

using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    // AssetPostprocessor를 상속받아 에셋 변경 이벤트를 처리합니다.
    public class DefinePostprocessor : AssetPostprocessor
    {
        // PREFS_KEY: EditorPrefs에 자동 정의 확인 필요 상태를 저장하기 위한 키입니다.
        [Tooltip("자동 정의 확인 필요 상태를 EditorPrefs에 저장하기 위한 키")]
        private const string PREFS_KEY = "DefinesCheck";

        /// <summary>
        /// 스크립트 리로드가 완료된 후 호출되는 콜백 함수입니다.
        /// 컴파일 또는 업데이트 중이 아니면 DefineManager의 자동 정의 확인 기능을 호출합니다.
        /// Unity 2019.3부터 지원되는 DidReloadScripts 콜백을 사용합니다.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AssemblyReload()
        {
            // Unity 에디터가 컴파일 중이거나 업데이트 중이거나 Core 폴더 경로가 설정되지 않은 경우,
            // 지연 호출을 사용하여 컴파일/업데이트가 완료될 때까지 대기합니다.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || string.IsNullOrEmpty(CoreEditor.FOLDER_CORE))
            {
                EditorApplication.delayCall += AssemblyReload;
                return;
            }

            // 컴파일 및 업데이트가 완료되면 DefineManager의 자동 정의 확인 기능을 지연 호출로 실행합니다.
            // 지연 호출을 사용하는 이유는 스크립트 리로드 직후 바로 실행 시 예기치 않은 문제가 발생할 수 있기 때문입니다.
            EditorApplication.delayCall += () => DefineManager.CheckAutoDefines();
        }

        /// <summary>
        /// 에셋이 임포트, 삭제, 이동된 후 호출되는 콜백 함수입니다.
        /// Unity 2018.1부터 지원되는 OnPostprocessAllAssets 콜백을 사용합니다.
        /// 스크립트 또는 DLL 파일의 변경이 감지되면 DefineManager를 통해 자동 정의 심볼을 확인하도록 플래그를 설정하고,
        /// 에디터가 유휴 상태일 때 자동 정의 확인 기능을 실행합니다.
        /// </summary>
        /// <param name="importedAssets">새로 임포트된 에셋 경로 배열</param>
        /// <param name="deletedAssets">삭제된 에셋 경로 배열</param>
        /// <param name="movedAssets">이동된 에셋의 새 경로 배열</param>
        /// <param name="movedFromAssetPaths">이동된 에셋의 이전 경로 배열</param>
        /// <param name="didDomainReload">도메인 리로드가 발생했는지 여부</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // 임포트되거나 삭제된 에셋 목록을 기반으로 자동 정의 확인 필요 여부를 검증합니다.
            ValidateRequirement(importedAssets, deletedAssets);

            // Unity 에디터가 컴파일 중이거나 업데이트 중이거나 Core 폴더 경로가 설정되지 않은 경우,
            // 지연 호출을 사용하여 컴파일/업데이트가 완료될 때까지 대기합니다.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || string.IsNullOrEmpty(CoreEditor.FOLDER_CORE))
            {
                EditorApplication.delayCall += () => OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths, didDomainReload);
                return;
            }

            // EditorPrefs에 자동 정의 확인이 필요하다는 플래그가 설정되어 있으면,
            // DefineManager의 자동 정의 확인 기능을 실행하고 플래그를 초기화합니다.
            if (EditorPrefs.GetBool(PREFS_KEY, false))
            {
                DefineManager.CheckAutoDefines();
                EditorPrefs.SetBool(PREFS_KEY, false);
            }
        }

        /// <summary>
        /// 임포트되거나 삭제된 에셋 목록에 스크립트(.cs) 또는 DLL(.dll) 파일이 포함되어 있는지 확인합니다.
        /// 이러한 파일이 변경되면 자동 정의 심볼을 다시 확인할 필요가 있다고 판단하여 EditorPrefs에 플래그를 설정합니다.
        /// </summary>
        /// <param name="importedAssets">새로 임포트된 에셋 경로 배열</param>
        /// <param name="deletedAssets">삭제된 에셋 경로 배열</param>
        private static void ValidateRequirement(string[] importedAssets, string[] deletedAssets)
        {
            // 임포트된 에셋 목록이 비어있지 않으면 순회하며 검사합니다.
            if (!importedAssets.IsNullOrEmpty())
            {
                foreach (string str in importedAssets)
                {
                    // 에셋 경로가 .cs 또는 .dll로 끝나는 경우, 자동 정의 확인 필요 플래그를 설정하고 함수를 종료합니다.
                    if (str.EndsWith(".cs") || str.EndsWith(".dll"))
                    {
                        EditorPrefs.SetBool(PREFS_KEY, true);
                        return;
                    }
                }
            }

            // 삭제된 에셋 목록이 비어있지 않으면 순회하며 검사합니다.
            if (!deletedAssets.IsNullOrEmpty())
            {
                foreach (string str in deletedAssets)
                {
                    // 에셋 경로가 .cs 또는 .dll로 끝나는 경우, 자동 정의 확인 필요 플래그를 설정하고 함수를 종료합니다.
                    if (str.EndsWith(".cs") || str.EndsWith(".dll"))
                    {
                        EditorPrefs.SetBool(PREFS_KEY, true);
                        return;
                    }
                }
            }
        }
    }
}