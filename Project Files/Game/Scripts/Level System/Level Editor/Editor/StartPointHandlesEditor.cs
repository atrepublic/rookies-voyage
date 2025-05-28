// 이 스크립트는 StartPointHandles 컴포넌트의 에디터 확장 기능을 정의합니다.
// 씬 뷰에 StartPointHandles를 시각적으로 표시하기 위한 기즈모를 그립니다.
// 원형 디스크와 텍스트 라벨을 포함할 수 있습니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Unity 에디터 기능 사용을 위해 추가

namespace Watermelon.SquadShooter
{
    // StartPointHandles 컴포넌트에 대한 커스텀 에디터입니다.
    [CustomEditor(typeof(StartPointHandles))]
    public class StartPointHandlesEditor : Editor
    {
        // 씬 뷰에 기즈모를 그리는 함수입니다.
        // StartPointHandles 컴포넌트가 선택되었거나 선택되지 않았을 때 모두 호출됩니다.
        // <param name="startPointHandles">기즈모를 그릴 StartPointHandles 컴포넌트 인스턴스입니다.</param>
        // <param name="gizmoType">현재 기즈모의 타입입니다.</param>
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawHandles(StartPointHandles startPointHandles, GizmoType gizmoType)
        {
            Handles.color = startPointHandles.diskColor; // 핸들 색상을 디스크 색상으로 설정
            // 원형 디스크 기즈모를 그립니다. (위치, 노멀 방향, 반지름, 두께)
            Handles.DrawWireDisc(startPointHandles.transform.position, startPointHandles.transform.up, startPointHandles.diskRadius, startPointHandles.thickness);

            // 텍스트 표시가 비활성화되어 있으면 여기서 함수 종료
            if (!startPointHandles.displayText)
            {
                return;
            }

            Color backupColor = GUI.color; // 현재 GUI 색상 백업
            GUI.color = startPointHandles.textColor; // 텍스트 색상으로 GUI 색상 설정

            // 텍스트 변수를 사용할지 오브젝트 이름을 사용할지 결정하여 라벨을 그립니다.
            if (startPointHandles.useTextVariable)
            {
                // 설정된 텍스트 변수 값을 라벨로 그립니다. (위치 + 오프셋, 텍스트)
                Handles.Label(startPointHandles.transform.position + startPointHandles.textPositionOffset, startPointHandles.text);
            }
            else
            {
                // 게임 오브젝트의 이름을 라벨로 그립니다. (위치 + 오프셋, 오브젝트 이름)
                Handles.Label(startPointHandles.transform.position + startPointHandles.textPositionOffset, startPointHandles.gameObject.name);
            }

            GUI.color = backupColor; // 백업했던 GUI 색상으로 되돌림
        }
    }
}