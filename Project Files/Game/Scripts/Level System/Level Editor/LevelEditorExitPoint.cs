/// <summary>
/// LevelEditorExitPoint.cs
///
/// 레벨 에디터에서 플레이어가 클리어 후 도달해야 하는 출구(Exit Point)를 설정하는 스크립트입니다.
/// 출구 포인트의 위치를 기준으로 스테이지 클리어 조건을 처리할 수 있습니다.
/// </summary>

#pragma warning disable 649
using UnityEngine;

namespace Watermelon.LevelSystem
{
    public class LevelEditorExitPoint : MonoBehaviour
    {
        // 현재는 별도 설정값 없이 Transform 위치를 사용합니다.
        // 향후 필요 시 출구 특성 추가 가능
    }
}
