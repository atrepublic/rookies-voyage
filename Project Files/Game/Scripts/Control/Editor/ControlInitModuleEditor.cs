// 스크립트 설명: ControlInitModule 컴포넌트의 인스펙터 모양을 사용자 정의하는 Unity 에디터 확장 스크립트입니다.
// ControlInitModule이 생성될 때 특정 데이터를 자동으로 할당하는 기능을 제공합니다.
using UnityEditor; // Unity 에디터 기능 사용을 위한 네임스페이스

namespace Watermelon
{
    // ControlInitModule 타입에 대해 이 CustomEditor를 사용하도록 지정
    [CustomEditor(typeof(ControlInitModule))] // ControlInitModule은 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정
    public class ControlInitModuleEditor : InitModuleEditor // InitModuleEditor 상속 (이전 파일에서 정의된 것으로 가정)
    {
        /// <summary>
        /// Target 오브젝트(ControlInitModule)가 처음 생성될 때 호출됩니다.
        /// GamepadData 애셋을 찾아 serializedObject에 할당합니다.
        /// </summary>
        public override void OnCreated()
        {
            // EditorUtils를 사용하여 GamepadData 타입의 애셋을 찾습니다. (EditorUtils에 정의된 것으로 가정)
            GamepadData gamepadData = EditorUtils.GetAsset<GamepadData>(); // GamepadData는 Watermelon 네임스페이스 또는 다른 곳에 정의된 것으로 가정

            // GamepadData 애셋을 찾았다면
            if(gamepadData != null )
            {
                serializedObject.Update(); // serializedObject의 최신 상태를 가져옴
                // "gamepadData"라는 이름의 프로퍼티를 찾아 찾은 GamepadData 애셋을 할당
                serializedObject.FindProperty("gamepadData").objectReferenceValue = gamepadData;
                serializedObject.ApplyModifiedProperties(); // 변경된 프로퍼티를 적용
            }
        }
    }
}