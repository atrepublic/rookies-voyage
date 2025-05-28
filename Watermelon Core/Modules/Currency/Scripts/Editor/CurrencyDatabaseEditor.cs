// CurrencyDatabaseEditor.cs
// 이 스크립트는 Unity 에디터에서 CurrencyDatabase ScriptableObject의 커스텀 인스펙터 창을 제공합니다.
// 통화 데이터베이스를 시각적으로 관리하고, 통화 아이콘들을 사용하여 Sprite Atlas를 생성하는 기능을 포함합니다.
// Sprite Atlas는 TextMeshPro에서 통화 아이콘을 텍스트처럼 사용하기 위해 필요합니다.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.U2D;

#if MODULE_TMP
using TMPro;
#endif

namespace Watermelon
{
    [CustomEditor(typeof(CurrencyDatabase))]
    public class CurrencyDatabaseEditor : CustomInspector
    {
        // 편집 중인 CurrencyDatabase 객체 참조입니다.
        private CurrencyDatabase currencyDatabase;

        /// <summary>
        /// 에디터 창이 활성화될 때 호출됩니다.
        /// 대상 객체를 CurrencyDatabase로 형변환하여 참조합니다.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            // 편집 대상 객체를 CurrencyDatabase 타입으로 가져옵니다.
            currencyDatabase = (CurrencyDatabase)target;
        }

        /// <summary>
        /// 인스펙터 GUI를 그릴 때 호출됩니다.
        /// 기본 인스펙터 GUI를 그리고, Sprite Atlas 생성 버튼을 추가합니다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

#if MODULE_TMP
            // TextMeshPro 모듈이 활성화되어 있을 때만 Sprite Atlas 생성 버튼을 표시합니다.
            if (GUILayout.Button("Create Sprite Atlas"))
            {
                CreateAtlas(); // 버튼 클릭 시 Sprite Atlas 생성 함수를 호출합니다.
            }
#endif
        }

#if MODULE_TMP
        /// <summary>
        /// 통화 아이콘들로부터 Sprite Atlas를 생성하는 함수입니다.
        /// TextMeshPro에서 사용할 Sprite Asset도 함께 생성합니다.
        /// </summary>
        private void CreateAtlas()
        {
            // CurrencyDatabase 객체가 유효한지 확인합니다.
            if (currencyDatabase == null) return;

            // 아틀라스에 포함될 스프라이트 데이터 목록을 생성합니다.
            List<TMPAtlasGenerator.SpriteData> atlasElements = new List<TMPAtlasGenerator.SpriteData>();

            // CurrencyDatabase에 정의된 통화 목록을 가져옵니다.
            Currency[] currencies = currencyDatabase.Currencies;

            // Sprite Atlas 생성을 위해 Sprite Mesh Type을 FullRect로 강제 설정합니다.
            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i].Icon != null)
                {
                    // 스프라이트 아이콘의 경로를 가져와 TextureImporter를 얻습니다.
                    TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(currencies[i].Icon));

                    // 텍스처 임포트 설정을 읽고 SpriteMeshType을 FullRect로 변경합니다.
                    TextureImporterSettings settings = new TextureImporterSettings();
                    textureImporter.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    textureImporter.SetTextureSettings(settings);
                    textureImporter.SaveAndReimport(); // 설정 변경사항을 저장하고 다시 임포트합니다.
                }
            }

            // AssetDatabase를 새로고침하여 변경사항을 반영합니다.
            AssetDatabase.Refresh();

            // 아틀라스 생성을 위한 스프라이트 데이터를 목록에 추가합니다.
            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i].Icon != null)
                {
                    // 스프라이트와 해당 통화 타입 이름으로 SpriteData 객체를 생성합니다.
                    atlasElements.Add(new TMPAtlasGenerator.SpriteData(currencies[i].Icon, currencies[i].CurrencyType.ToString()));
                }
            }

            // TMPAtlasGenerator를 사용하여 Sprite Atlas 생성 코루틴을 시작합니다.
            TMPAtlasGenerator.Create(atlasElements, "");
        }

        // TextMeshPro용 Sprite Atlas와 Sprite Asset을 생성하는 헬퍼 클래스입니다.
        public class TMPAtlasGenerator
        {
            // 아틀라스 파일 경로 저장을 위한 EditorPrefs 키입니다.
            private const string FILE_PATH_SAVE = "atlas_generator_file_path";
            // 아틀라스에 포함될 SpriteData 요소 목록입니다.
            private List<SpriteData> elements;

            // 생성될 아틀라스 파일 경로입니다.
            private string filePath;

            /// <summary>
            /// TMPAtlasGenerator 클래스의 생성자입니다.
            /// 아틀라스 파일 경로를 설정하고 요소 목록을 초기화합니다.
            /// </summary>
            /// <param name="path">생성될 아틀라스 파일 경로</param>
            public TMPAtlasGenerator(string path)
            {
                this.filePath = path;

                elements = new List<SpriteData>();
            }

            /// <summary>
            /// 아틀라스에 추가할 SpriteData 요소를 목록에 추가합니다.
            /// </summary>
            /// <param name="element">추가할 SpriteData 요소</param>
            public void Add(SpriteData element)
            {
                elements.Add(element);
            }

            /// <summary>
            /// 현재 요소 목록을 기반으로 Sprite Atlas 파일을 저장합니다.
            /// </summary>
            public void Save()
            {
                // 새로운 SpriteAtlasAsset을 생성합니다.
                SpriteAtlasAsset spriteAtlasAsset = new SpriteAtlasAsset();
                // 요소 목록에 있는 스프라이트들을 SpriteAtlasAsset에 추가합니다.
                spriteAtlasAsset.Add(elements.Select(x => (Object)x.Sprite).ToArray());

                // 지정된 경로에 SpriteAtlasAsset을 저장합니다.
                SpriteAtlasAsset.Save(spriteAtlasAsset, filePath);

                // AssetDatabase를 저장하고 새로고침하여 변경사항을 반영합니다.
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 저장된 Sprite Atlas 파일을 가져와 임포터 설정을 변경합니다.
                SpriteAtlasImporter spriteAtlasImporter = (SpriteAtlasImporter)SpriteAtlasImporter.GetAtPath(filePath);
                // 패킹 설정을 변경합니다. (Tight Packing 비활성화, Alpha Dilation 활성화)
                spriteAtlasImporter.packingSettings = new SpriteAtlasPackingSettings()
                {
                    enableTightPacking = false,
                    enableAlphaDilation = true
                };

                // 설정 변경사항을 저장하고 다시 임포트합니다.
                spriteAtlasImporter.SaveAndReimport();

                // AssetDatabase를 새로고침합니다.
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// 지정된 경로에 생성된 Sprite Atlas 객체를 비동기적으로 가져오는 코루틴입니다.
            /// </summary>
            /// <returns>Sprite Atlas 객체를 반환하는 IEnumerator</returns>
            public IEnumerator<SpriteAtlas> GetSpriteAtlas()
            {
                SpriteAtlas spriteAtlas = null;

                // Sprite Atlas 객체가 로드될 때까지 기다립니다.
                do
                {
                    yield return null; // 다음 프레임까지 대기합니다.

                    // 지정된 경로에서 Sprite Atlas 객체를 로드합니다.
                    spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(filePath);
                }
                while (spriteAtlas == null); // 로드될 때까지 반복합니다.

                // 로드된 Sprite Atlas 객체를 반환합니다.
                yield return spriteAtlas;
            }

            /// <summary>
            /// 아틀라스에 포함될 요소 목록이 비어 있는지 확인합니다.
            /// </summary>
            /// <returns>요소 목록이 비어 있으면 true, 아니면 false</returns>
            public bool IsEmpty()
            {
                return elements.IsNullOrEmpty();
            }

            /// <summary>
            /// 생성된 Sprite Atlas를 기반으로 TextMeshPro용 Sprite Asset을 생성합니다.
            /// </summary>
            /// <param name="spriteAtlas">Sprite Asset 생성에 사용할 Sprite Atlas 객체</param>
            public void CreateSpriteAsset(SpriteAtlas spriteAtlas)
            {
                // 아틀라스 파일 경로에서 파일 이름과 확장자, 디렉토리 경로를 가져옵니다.
                string fileNameWithExtension = Path.GetFileName(this.filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.filePath);
                string filePath = this.filePath.Replace(fileNameWithExtension, "");

                // 새로운 TMP_SpriteAsset 인스턴스를 생성합니다.
                TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                // Sprite Asset을 지정된 경로에 .asset 파일로 생성합니다.
                AssetDatabase.CreateAsset(spriteAsset, filePath + fileNameWithoutExtension + ".asset");

                // 리플렉션을 사용하여 Sprite Asset의 버전을 설정합니다. (내부 로직, 변경 시 주의)
                ReflectionUtils.InjectInstanceComponent(spriteAsset, "m_Version", "1.1.0");

                // Sprite Asset의 해시 코드를 계산하고 설정합니다.
                spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteAsset.name);

                // SpriteGlyph 및 SpriteCharacter 테이블을 초기화합니다.
                List<TMP_SpriteGlyph> spriteGlyphTable = new List<TMP_SpriteGlyph>();
                List<TMP_SpriteCharacter> spriteCharacterTable = new List<TMP_SpriteCharacter>();

                // Sprite Atlas에 포함된 스프라이트 개수를 가져옵니다.
                int spriteCount = spriteAtlas.spriteCount;
                Sprite[] sprites = new Sprite[spriteCount];

                // Sprite Atlas에 포함된 모든 스프라이트를 가져옵니다.
                spriteAtlas.GetSprites(sprites);

                // 각 스프라이트에 대해 TMP_SpriteGlyph 및 TMP_SpriteCharacter를 생성합니다.
                for (int i = 0; i < sprites.Length; i++)
                {
                    Sprite sprite = sprites[i];
                    // 스프라이트 이름에서 "(Clone)" 접미사를 제거하여 실제 이름을 가져옵니다.
                    string realName = sprite.name.Substring(0, sprite.name.Length - 7);
                    // 실제 이름에 해당하는 SpriteData 요소를 가져옵니다.
                    SpriteData linkedElement = GetAtlasElement(realName);

                    // 새로운 TMP_SpriteGlyph를 생성합니다.
                    TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph();
                    spriteGlyph.index = (uint)i; // 인덱스를 설정합니다.

                    // 오버라이드 데이터가 설정되어 있으면 해당 값으로 GlyphMetrics 및 Scale을 설정합니다.
                    if ((linkedElement != null) && (linkedElement.OverrideDataSet))
                    {
                        spriteGlyph.metrics = linkedElement.GlyphMetrics;
                        spriteGlyph.scale = linkedElement.Scale;
                    }
                    else // 오버라이드 데이터가 없으면 스프라이트 Rect 정보를 기반으로 설정합니다.
                    {
                        spriteGlyph.metrics = new GlyphMetrics(sprite.rect.width, sprite.rect.height, -sprite.pivot.x, sprite.rect.height - sprite.pivot.y, sprite.rect.width);
                        spriteGlyph.scale = 1.0f;
                    }

                    // GlyphRect를 설정합니다.
                    spriteGlyph.glyphRect = new GlyphRect(sprite.textureRect);
                    spriteGlyph.sprite = sprite; // 스프라이트 참조를 설정합니다.
                    spriteGlyphTable.Add(spriteGlyph); // Glyph 테이블에 추가합니다.

                    // 새로운 TMP_SpriteCharacter를 생성합니다.
                    TMP_SpriteCharacter spriteCharacter = new TMP_SpriteCharacter(0xFFFE, spriteGlyph); // 기본 유니코드 값으로 생성합니다.

                    // 연결된 SpriteData 요소가 있으면 해당 이름으로 SpriteCharacter 이름을 설정합니다.
                    if (linkedElement != null)
                    {
                        spriteCharacter.name = linkedElement.Name;
                    }
                    else // 없으면 실제 스프라이트 이름을 사용합니다.
                    {
                        spriteCharacter.name = realName;
                    }

                    spriteCharacter.scale = 1.0f; // 기본 스케일을 설정합니다.

                    spriteCharacterTable.Add(spriteCharacter); // Character 테이블에 추가합니다.
                }

                // 리플렉션을 사용하여 Sprite Asset에 Character 및 Glyph 테이블을 주입합니다. (내부 로직, 변경 시 주의)
                ReflectionUtils.InjectInstanceComponent(spriteAsset, "m_SpriteCharacterTable", spriteCharacterTable);
                ReflectionUtils.InjectInstanceComponent(spriteAsset, "m_GlyphTable", spriteGlyphTable);

                // Sprite Asset의 스프라이트 시트 텍스처를 설정합니다.
                spriteAsset.spriteSheet = spriteGlyphTable[0].sprite.texture;

                // Sprite Asset을 위한 기본 Material을 추가합니다.
                AddDefaultMaterial(spriteAsset);

                // Sprite Asset의 조회 테이블을 업데이트합니다.
                spriteAsset.UpdateLookupTables();

                // Sprite Asset의 변경사항을 에디터에 알립니다.
                EditorUtility.SetDirty(spriteAsset);

                // AssetDatabase를 저장합니다.
                AssetDatabase.SaveAssets();

                // 생성된 Sprite Asset을 다시 임포트합니다.
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(spriteAsset));
            }

            /// <summary>
            /// TextMeshPro Sprite Asset을 위한 기본 Material을 생성하고 추가합니다.
            /// </summary>
            /// <param name="spriteAsset">Material을 추가할 Sprite Asset 객체</param>
            private void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
            {
                // TextMeshPro Sprite 셰이더를 찾습니다.
                UnityEngine.Shader shader = UnityEngine.Shader.Find("TextMeshPro/Sprite");
                // 셰이더를 사용하여 새로운 Material을 생성합니다.
                Material material = new Material(shader);
                // Material의 메인 텍스처를 Sprite Asset의 스프라이트 시트로 설정합니다.
                material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

                // Sprite Asset의 Material로 설정합니다.
                spriteAsset.material = material;
                // Material이 Hierarchy에 표시되지 않도록 HideFlags를 설정합니다.
                material.hideFlags = HideFlags.HideInHierarchy;
                // Material을 Sprite Asset에 서브 에셋으로 추가합니다.
                AssetDatabase.AddObjectToAsset(material, spriteAsset);
            }

            /// <summary>
            /// 주어진 스프라이트 이름에 해당하는 SpriteData 요소를 아틀라스 요소 목록에서 찾습니다.
            /// </summary>
            /// <param name="spriteName">찾을 스프라이트 이름</param>
            /// <returns>해당하는 SpriteData 요소, 없으면 null</returns>
            private SpriteData GetAtlasElement(string spriteName)
            {
                foreach (SpriteData element in elements)
                {
                    if (element.Sprite.name == spriteName)
                        return element;
                }

                return null;
            }

            /// <summary>
            /// TMPAtlasGenerator 인스턴스를 생성하고 아틀라스 생성 코루틴을 시작합니다.
            /// 정적 헬퍼 함수입니다.
            /// </summary>
            /// <param name="atlasElements">아틀라스에 포함될 SpriteData 요소 목록</param>
            /// <param name="path">아틀라스 파일 경로 (비어 있으면 저장 대화상자가 열림)</param>
            /// <returns>생성된 TMPAtlasGenerator 인스턴스</returns>
            public static TMPAtlasGenerator Create(List<SpriteData> atlasElements, string path)
            {
                TMPAtlasGenerator atlasGenerator = new TMPAtlasGenerator(path);

                // 에디터 코루틴을 사용하여 아틀라스 생성 코루틴을 실행합니다.
                EditorCoroutines.Execute(atlasGenerator.AtlasCoroutine(atlasElements));

                return atlasGenerator;
            }

            /// <summary>
            /// Sprite Atlas 생성 과정을 단계별로 처리하는 코루틴입니다.
            /// 파일 경로 선택, 아틀라스 저장, Sprite Atlas 로드 대기, Sprite Asset 생성, TMP Settings 연결 등을 수행합니다.
            /// </summary>
            /// <param name="atlasElements">아틀라스에 포함될 SpriteData 요소 목록</param>
            /// <returns>코루틴 실행을 위한 IEnumerator</returns>
            public IEnumerator AtlasCoroutine(List<SpriteData> atlasElements)
            {
                // 파일 경로가 비어 있으면 저장 대화상자를 열어 사용자로부터 경로를 입력받습니다.
                if (string.IsNullOrEmpty(filePath))
                {
                    // 이전에 저장된 경로를 가져옵니다.
                    string savedPath = EditorPrefs.GetString(FILE_PATH_SAVE);
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        savedPath = Path.GetDirectoryName(savedPath);
                    }
                    else
                    {
                        savedPath = "Assets"; // 기본 경로 설정
                    }

                    // 저장 파일 대화상자를 열어 아틀라스 파일을 저장할 경로를 선택합니다.
                    filePath = EditorUtility.SaveFilePanelInProject("Generated Atlas", "GeneratedAtlas", "spriteatlasv2", "Select atlas path", savedPath);

                    // 경로 선택이 취소되었으면 오류 메시지를 출력하고 코루틴을 종료합니다.
                    if (string.IsNullOrEmpty(filePath))
                    {
                        Debug.LogError("[Atlas Generator]: Path can't be empty!");

                        yield break;
                    }

                    // 선택된 경로의 디렉토리를 EditorPrefs에 저장하여 다음에 사용합니다.
                    EditorPrefs.SetString(FILE_PATH_SAVE, Path.GetDirectoryName(filePath));
                }

                // 아틀라스 요소 목록이 비어 있으면 오류 메시지를 출력하고 코루틴을 종료합니다.
                if (atlasElements.IsNullOrEmpty())
                {
                    Debug.LogError("[Atlas Generator]: Sprites list is empty!");

                    yield break;
                }

                // 제공된 아틀라스 요소들을 내부 목록에 추가합니다.
                for (int i = 0; i < atlasElements.Count; i++)
                {
                    elements.Add(atlasElements[i]);
                }

                // 기존 에셋이 존재하고 오버라이드할 경우 GlyphMetrics 및 Scale 정보를 복사할지 확인합니다.
                if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(filePath, AssetPathToGUIDOptions.OnlyExistingAssets)))
                {
                    // 생성될 TMP Sprite Asset 파일 경로를 가져옵니다.
                    string TMP_AssetFilePath = filePath.Replace(".spriteatlasv2", ".asset");
                    // 기존 TMP Sprite Asset 파일을 로드합니다.
                    TMP_SpriteAsset TMP_AssetFile = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(TMP_AssetFilePath);

                    // 기존 파일이 있고 오버라이드 여부를 묻는 대화상자에서 'Yes'를 선택한 경우
                    if ((TMP_AssetFile != null) && (EditorUtility.DisplayDialog("Asset override", "Do you want to copy sprite GlyphMetrics and scale from overriden asset?", "Yes", "No")))
                    {
                        // 기존 Sprite Asset의 Character 테이블을 순회하며 정보를 복사합니다.
                        for (int i = 0; i < TMP_AssetFile.spriteCharacterTable.Count; i++)
                        {
                            // 현재 생성될 아틀라스 요소 목록을 순회합니다.
                            for (int j = 0; j < elements.Count; j++)
                            {
                                // 아직 오버라이드 데이터가 설정되지 않은 요소에 대해서만 처리합니다.
                                if (!elements[j].OverrideDataSet)
                                {
                                    // 요소의 이름과 기존 Sprite Asset의 Character 이름이 같으면
                                    if (elements[j].Name.Equals(TMP_AssetFile.spriteCharacterTable[i].name))
                                    {
                                        // GlyphMetrics와 Scale 정보를 복사하고 오버라이드 데이터 설정 플래그를 true로 설정합니다.
                                        elements[j].GlyphMetrics = TMP_AssetFile.spriteCharacterTable[i].glyph.metrics;
                                        elements[j].Scale = TMP_AssetFile.spriteCharacterTable[i].glyph.scale;
                                        elements[j].OverrideDataSet = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // Sprite Atlas 파일을 저장합니다.
                Save();

                // Sprite Atlas 객체가 로드될 때까지 기다리는 코루틴을 실행합니다.
                IEnumerator<SpriteAtlas> spriteAtlasEnumerator = GetSpriteAtlas();
                while (spriteAtlasEnumerator.MoveNext())
                {
                    yield return null; // 다음 프레임까지 대기합니다.
                }

                // 로드된 Sprite Atlas 객체를 가져옵니다.
                SpriteAtlas spriteAtlas = spriteAtlasEnumerator.Current;
                // Sprite Atlas 로드에 실패했으면 오류 메시지를 출력하고 코루틴을 종료합니다.
                if (spriteAtlas == null)
                {
                    Debug.LogError("[Currencies]: Failed to create Sprite Atlas!");

                    yield break;
                }

                // Sprite Atlas 객체의 변경사항을 에디터에 알립니다.
                EditorUtility.SetDirty(spriteAtlas);

                yield return null; // 한 프레임 대기합니다.

                // Sprite Atlas를 기반으로 TextMeshPro Sprite Asset을 생성합니다.
                CreateSpriteAsset(spriteAtlas);

                // 생성된 Sprite Asset을 TextMeshPro Settings에 연결할지 확인합니다.
                // TextMeshPro Settings 객체를 가져옵니다.
                TMP_Settings settings = EditorUtils.GetAsset<TMP_Settings>();

                // TMP Settings 객체가 존재하면
                if (settings != null)
                {
                    // TMP Settings에 연결할지 묻는 대화상자를 엽니다.
                    if (EditorUtility.DisplayDialog("Linking asset to TMP Settings", "Do you want to add created \"Sprite Atlas\" to \"TMP Settings\"?", "Yes", "Cancel"))
                    {
                        // TMP Settings 객체의 SerializedObject를 가져옵니다.
                        SerializedObject settingsSerializedObject = new SerializedObject(settings);
                        // 기본 Sprite Asset 속성을 찾습니다.
                        SerializedProperty defaultAssetProperty = settingsSerializedObject.FindProperty("m_defaultSpriteAsset");
                        // 생성된 TMP Sprite Asset 파일 경로를 가져옵니다.
                        string TMP_AssetFilePath = filePath.Replace(".spriteatlasv2", ".asset");
                        // 생성된 TMP Sprite Asset 파일을 로드합니다.
                        TMP_SpriteAsset TMP_AssetFile = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(TMP_AssetFilePath);

                        // 기본 Sprite Asset이 비어 있거나 오버라이드 또는 Fallback 목록 추가 여부를 묻는 대화상자에서 'Override'를 선택한 경우
                        if ((defaultAssetProperty.objectReferenceValue == null) || EditorUtility.DisplayDialog("Linking asset to TMP Settings", "Do you want to override \"Default Sprite Asset\" reference with created \"Sprite Atlas\" or add reference to fallback list of current \"Default Sprite Asset\" ?", "Override", "Add to fallback list"))
                        {
                            // 기본 Sprite Asset 속성에 생성된 Sprite Asset을 할당합니다.
                            defaultAssetProperty.objectReferenceValue = TMP_AssetFile;
                            // 변경사항을 적용합니다.
                            settingsSerializedObject.ApplyModifiedProperties();
                        }
                        else // 'Add to fallback list'를 선택한 경우
                        {
                            // 현재 기본 Sprite Asset의 SerializedObject를 가져옵니다.
                            SerializedObject spriteAssetSerializedObject = new SerializedObject(defaultAssetProperty.objectReferenceValue);
                            // Fallback Sprite Assets 목록 속성을 찾습니다.
                            SerializedProperty fallbackAssetsProperty = spriteAssetSerializedObject.FindProperty("fallbackSpriteAssets");
                            // Fallback 목록 크기를 늘리고 생성된 Sprite Asset을 추가합니다.
                            fallbackAssetsProperty.arraySize++;
                            fallbackAssetsProperty.GetArrayElementAtIndex(fallbackAssetsProperty.arraySize - 1).objectReferenceValue = TMP_AssetFile;
                            // 변경사항을 적용합니다.
                            spriteAssetSerializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            // Sprite Atlas 생성에 사용되는 개별 스프라이트 데이터를 나타내는 클래스입니다.
            public class SpriteData
            {
                // 원본 스프라이트 객체입니다.
                private Sprite sprite;
                // 스프라이트의 이름입니다.
                private string name;
                // GlyphMetrics 및 Scale 오버라이드 데이터가 설정되었는지 여부입니다.
                private bool overrideDataSet;
                // 오버라이드된 GlyphMetrics 데이터입니다.
                private GlyphMetrics glyphMetrics;
                // 오버라이드된 스케일 값입니다.
                private float scale;

                // 원본 스프라이트 객체에 접근하는 속성입니다.
                public Sprite Sprite => sprite;
                // 스프라이트 이름에 접근하는 속성입니다.
                public string Name => name;

                // 오버라이드 데이터 설정 여부를 가져오거나 설정하는 속성입니다.
                public bool OverrideDataSet { get => overrideDataSet; set => overrideDataSet = value; }
                // 오버라이드된 GlyphMetrics 데이터에 접근하는 속성입니다.
                public GlyphMetrics GlyphMetrics { get => glyphMetrics; set => glyphMetrics = value; }
                // 오버라이드된 스케일 값에 접근하는 속성입니다.
                public float Scale { get => scale; set => scale = value; }

                /// <summary>
                /// SpriteData 클래스의 생성자입니다.
                /// </summary>
                /// <param name="sprite">원본 스프라이트 객체</param>
                /// <param name="name">스프라이트 이름</param>
                public SpriteData(Sprite sprite, string name)
                {
                    this.sprite = sprite;
                    this.name = name;
                    overrideDataSet = false; // 오버라이드 데이터 설정 여부는 기본적으로 false입니다.
                }
            }
        }
#endif
    }
}