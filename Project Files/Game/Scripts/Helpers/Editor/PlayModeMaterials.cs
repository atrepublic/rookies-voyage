// PlayModeMaterials.cs
// 이 스크립트는 Unity 에디터의 기능 확장으로, 플레이 모드(게임 실행 중)일 때 Material(.mat) 파일이 저장되는 것을 방지합니다.
// 게임 실행 중 Material 변경 사항이 에셋 파일에 의도치 않게 저장되는 것을 막아 개발 워크플로우를 개선하는 데 도움을 줍니다.
using System.IO; // 파일 경로 처리를 위해 필요
using System.Linq; // LINQ 확장 메서드(Where, ToArray)를 사용하기 위해 필요
using UnityEditor; // Unity 에디터 관련 기능(AssetModificationProcessor, EditorApplication)을 사용하기 위해 필요

namespace Watermelon.SquadShooter // SquadShooter 네임스페이스에 포함
{
    // AssetModificationProcessor를 상속받아 에셋 수정 및 저장 이벤트를 가로챕니다.
    public class PlayModeMaterials : AssetModificationProcessor
    {
        /// <summary>
        /// 에셋이 저장되기 전에 호출되는 정적 메서드입니다.
        /// 플레이 모드일 경우 Material 파일의 저장을 막습니다.
        /// </summary>
        /// <param name="paths">저장될 에셋 파일 경로들의 배열</param>
        /// <returns>실제로 저장될 에셋 파일 경로들의 배열</returns>
        static string[] OnWillSaveAssets(string[] paths)
        {
            // EditorApplication.isPlaying은 현재 Unity 에디터가 플레이 모드인지 여부를 나타냅니다.
            if (EditorApplication.isPlaying)
            {
                // 플레이 모드인 경우:
                // 저장될 경로들 중에서 확장자가 ".mat"(Material 파일)이 아닌 경로들만 필터링하여 반환합니다.
                // 즉, Material 파일은 저장 대상에서 제외됩니다.
                return paths.Where(path => Path.GetExtension(path) != ".mat").ToArray();
            }
            else
            {
                // 플레이 모드가 아닌 경우 (에디트 모드):
                // 모든 파일의 저장을 허용합니다. 원래의 경로 배열을 그대로 반환합니다.
                return paths;
            }
        }
    }
}