// InitializerInitModuleEditor.cs
// 이 스크립트는 InitializerInitModule ScriptableObject의 Unity 에디터 커스텀 인스펙터 역할을 합니다.
// InitializerInitModule이 생성될 때, 미리 정의된 시스템 메시지 Canvas 프리팹을 찾아 자동으로 연결합니다.

using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    // InitializerInitModule 타입에 대한 커스텀 에디터임을 지정합니다.
    [CustomEditor(typeof(InitializerInitModule))]
    public class InitializerInitModuleEditor : InitModuleEditor // InitModuleEditor를 상속받습니다.
    {
        /// <summary>
        /// InitializerInitModule 객체가 생성될 때 호출되는 함수입니다.
        /// 시스템 메시지 Canvas 프리팹을 찾아 'systemMessagesPrefab' 속성에 자동으로 연결합니다.
        /// </summary>
        public override void OnCreated()
        {
            // 에셋 데이터베이스에서 "Core System Messages Canvas" 이름의 GameObject 프리팹을 찾습니다.
            GameObject canvasPrefab = EditorUtils.GetAsset<GameObject>("Core System Messages Canvas");
            // Canvas 프리팹을 찾았으면
            if (canvasPrefab != null)
            {
                // 직렬화된 객체를 업데이트하여 최신 상태를 반영합니다.
                serializedObject.Update();
                // InitializerInitModule의 'systemMessagesPrefab' 속성을 찾아 찾은 Canvas 프리팹으로 설정합니다.
                serializedObject.FindProperty("systemMessagesPrefab").objectReferenceValue = canvasPrefab;
                // 변경된 속성을 적용합니다.
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}