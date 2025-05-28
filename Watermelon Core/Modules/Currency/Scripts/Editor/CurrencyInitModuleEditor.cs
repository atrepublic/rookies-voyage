// CurrencyInitModuleEditor.cs
// 이 스크립트는 CurrencyInitModule의 Unity 에디터 커스텀 인스펙터 역할을 합니다.
// CurrencyInitModule이 생성될 때, 필요한 CurrencyDatabase ScriptableObject가 없으면 자동으로 생성하고 연결합니다.

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Watermelon
{
    [CustomEditor(typeof(CurrencyInitModule))]
    public class CurrencyInitModuleEditor : InitModuleEditor
    {
        /// <summary>
        /// CurrencyInitModule 객체가 생성될 때 호출되는 함수입니다.
        /// CurrencyDatabase가 있는지 확인하고, 없으면 생성하여 연결합니다.
        /// </summary>
        public override void OnCreated()
        {
            // 에셋 데이터베이스에서 기존 CurrencyDatabase 객체를 찾습니다.
            CurrencyDatabase currenciesDatabase = EditorUtils.GetAsset<CurrencyDatabase>();

            // CurrencyDatabase 객체가 존재하지 않으면 새로 생성합니다.
            if (currenciesDatabase == null)
            {
                // CurrencyDatabase ScriptableObject 인스턴스를 생성합니다.
                currenciesDatabase = (CurrencyDatabase)ScriptableObject.CreateInstance<CurrencyDatabase>();
                // ScriptableObject의 이름을 설정합니다.
                currenciesDatabase.name = "Currencies Database";

                // 현재 편집 대상(CurrencyInitModule)의 에셋 경로를 가져옵니다.
                string referencePath = AssetDatabase.GetAssetPath(target);
                // 에셋 경로에서 디렉토리 경로를 가져옵니다.
                string directoryPath = Path.GetDirectoryName(referencePath);

                // ScriptableObject를 저장할 고유한 파일 경로를 생성합니다.
                string assetPath = Path.Combine(directoryPath, currenciesDatabase.name + ".asset");
                // 동일한 이름의 파일이 이미 존재하면 고유한 이름으로 조정합니다.
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                // 결정된 경로에 ScriptableObject 에셋을 생성하고 저장합니다.
                AssetDatabase.CreateAsset(currenciesDatabase, assetPath);
                AssetDatabase.SaveAssets();

                // 편집 대상(CurrencyInitModule)의 변경사항을 에디터에 알립니다.
                EditorUtility.SetDirty(target);
            }

            // 직렬화된 객체를 업데이트하여 최신 상태를 반영합니다.
            serializedObject.Update();
            // CurrencyInitModule의 'currenciesDatabase' 속성을 찾고 생성하거나 찾은 CurrencyDatabase 객체로 설정합니다.
            serializedObject.FindProperty("currenciesDatabase").objectReferenceValue = currenciesDatabase;
            // 변경된 속성을 적용합니다.
            serializedObject.ApplyModifiedProperties();
        }
    }
}