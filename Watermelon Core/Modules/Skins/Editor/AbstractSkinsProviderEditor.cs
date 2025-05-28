// AbstractSkinsProviderEditor.cs
// 이 스크립트는 Unity 에디터에서 AbstractSkinDatabase 인스펙터에 사용자 정의 UI를 추가해주는 커스텀 에디터입니다.
// 해당 데이터베이스가 SkinsController에 등록되어 있는지 확인하고, 미등록 상태라면 등록 버튼을 제공합니다.

using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(AbstractSkinDatabase), true)]
    public class AbstractSkinsProviderEditor : CustomInspector
    {
        private SkinController skinsController;
        private bool isRegistered;

        /// <summary>
        /// 에디터가 활성화될 때 호출되며, SkinController를 찾고 해당 스킨 데이터베이스가 등록되어 있는지 확인합니다.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            AbstractSkinDatabase database = (AbstractSkinDatabase)target;

            // 에디터 전용 데이터베이스 목록에 등록
            EditorSkinsProvider.AddDatabase(database);

#if UNITY_6000
            skinsController = GameObject.FindFirstObjectByType<SkinController>();
#else
            skinsController = GameObject.FindObjectOfType<SkinController>();
#endif

            // 해당 데이터베이스가 SkinsController에 등록되어 있는지 확인
            if(skinsController != null && skinsController.Handler != null)
            {
                isRegistered = skinsController.Handler.HasSkinsProvider(database);
            }
        }

        /// <summary>
        /// 인스펙터에 커스텀 UI를 추가합니다. 등록되지 않은 데이터베이스일 경우 경고 메시지 및 추가 버튼 표시
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(skinsController != null)
            {
                if (!isRegistered)
                {
                    GUILayout.Space(12);

                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.HelpBox("이 데이터베이스는 현재 SkinsController에 등록되어 있지 않습니다.", MessageType.Warning);

                    if (GUILayout.Button("Skins Handler에 추가"))
                    {
                        SerializedObject skinsHandlerSerializedObject = new SerializedObject(skinsController);

                        skinsHandlerSerializedObject.Update();

                        SerializedProperty handlerProperty = skinsHandlerSerializedObject.FindProperty("handler");
                        SerializedProperty providersProperty = handlerProperty.FindPropertyRelative("skinProviders");

                        int index = providersProperty.arraySize;
                        providersProperty.arraySize = index + 1;

                        SerializedProperty providerProperty = providersProperty.GetArrayElementAtIndex(index);
                        providerProperty.objectReferenceValue = target;

                        skinsHandlerSerializedObject.ApplyModifiedProperties();

                        isRegistered = true;
                    }

                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}
