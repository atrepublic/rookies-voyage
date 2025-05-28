// 스크립트 설명: Unity 에디터에서 자주 사용되는 유틸리티 함수들을 모아 놓은 정적 클래스입니다.
// 애셋 검색 및 생성, 폴더 찾기, SerializedProperty 관련 작업 등의 기능을 제공합니다.
using System; // Type, String, IO 관련 네임스페이스
using System.IO; // Path, File, Directory 사용을 위한 네임스페이스
using System.Linq; // LINQ 확장 메서드 사용을 위한 네임스페이스
using System.Reflection; // Assembly, Type, MethodInfo 사용을 위한 네임스페이스 (GetTypes, GetMethod 등)
using System.Text.RegularExpressions; // Regex 사용을 위한 네임스페이스
using UnityEditor; // Unity 에디터 기능 사용을 위한 네임스페이스
using UnityEngine; // Debug, Application, GameObject, Object, Color, Vector2, Vector3, Vector4, Quaternion, Rect, Bounds, AnimationCurve, Gradient, Hash128, Vector2Int, Vector3Int, RectInt, BoundsInt 사용을 위한 네임스페이스
using Object = UnityEngine.Object; // UnityEngine.Object 사용 명시

namespace Watermelon
{
    // Unity 에디터 작업을 위한 유틸리티 함수를 제공하는 정적 클래스
    public static class EditorUtils
    {
        // 프로젝트 폴더의 전체 경로를 저장하는 읽기 전용 정적 변수
        public readonly static string projectFolderPath = Application.dataPath.Replace("/Assets", "/");

        // SerializedProperty 경로에서 배열 부분을 감지하기 위한 정규식 패턴
        private static readonly Regex arrayRegex = new Regex("Array\\.data");

        /// <summary>
        /// 특정 부모 타입을 상속받거나 동일한 모든 서브 타입(Subtype) 목록을 포함하는 GenericMenu를 생성합니다.
        /// 이 메뉴는 에디터에서 드롭다운 형태로 타입을 선택하는 데 사용될 수 있습니다.
        /// </summary>
        /// <param name="parentType">메뉴에 포함될 타입들의 부모 타입.</param>
        /// <param name="selectAction">메뉴에서 타입을 선택했을 때 실행될 Action (선택된 Type을 매개변수로 받음).</param>
        /// <param name="selectedType">현재 선택된 타입 (메뉴 항목에 체크 표시).</param>
        /// <param name="showAbstract">추상 클래스를 메뉴에 포함할지 여부.</param>
        /// <returns>서브 타입 목록을 포함하는 GenericMenu 객체.</returns>
        // Get SubType Menu - 원본 주석 번역
        public static GenericMenu GetSubTypeMenu(Type parentType, Action<Type> selectAction, Type selectedType = null, bool showAbstract = false)
        {
            GenericMenu menu = new GenericMenu(); // 새로운 GenericMenu 생성

            // 부모 타입이 추상 클래스가 아니면 메뉴에 부모 타입 자체를 추가
            if (!parentType.IsAbstract)
                menu.AddItem(new GUIContent(parentType.ToString()), parentType == selectedType, delegate { selectAction(parentType); });

            // 부모 타입을 상속받거나 동일한 모든 타입을 가져옴
            Type[] assemblyTypes = Assembly.GetAssembly(parentType).GetTypes(); // 부모 타입이 속한 어셈블리의 모든 타입 가져오기
            Type[] itemTypes = assemblyTypes.Where(type => type.IsSubclassOf(parentType) || type.Equals(parentType)).ToArray(); // 부모 타입을 상속받거나 동일한 타입만 필터링

            // 부모 타입을 직접 상속받는 1단계 서브 타입들을 찾음
            Type[] baseItemTypes = itemTypes.Where(type => type.BaseType == parentType).ToArray();
            // 1단계 서브 타입들을 순회하며 재귀적으로 서브 타입 메뉴 추가
            foreach (Type baseType in baseItemTypes)
            {
                SubType(ref menu, itemTypes, baseType, selectAction, "", selectedType, showAbstract); // 재귀 호출
            }

            return menu; // 생성된 메뉴 반환
        }

        /// <summary>
        /// GetSubTypeMenu 메서드에서 내부적으로 사용되는 재귀 함수입니다.
        /// 특정 타입을 부모로 하는 모든 서브 타입들을 GenericMenu에 계층적으로 추가합니다.
        /// </summary>
        /// <param name="menu">요소를 추가할 GenericMenu 객체 (참조 전달).</param>
        /// <param name="itemTypes">전체 관련 서브 타입 목록.</param>
        /// <param name="baseType">현재 재귀 단계의 부모 타입.</param>
        /// <param name="selectAction">메뉴 항목 선택 시 실행될 Action.</param>
        /// <param name="defaultPath">메뉴 항목 경로의 현재까지의 기본 경로.</param>
        /// <param name="selectedType">현재 선택된 타입.</param>
        /// <param name="showAbstract">추상 클래스를 메뉴에 포함할지 여부.</param>
        private static void SubType(ref GenericMenu menu, Type[] itemTypes, Type baseType, Action<Type> selectAction, string defaultPath = "", Type selectedType = null, bool showAbstract = false)
        {
            // 현재 baseType을 직접 상속받는 서브 타입들을 찾음
            Type[] subItemTypes = itemTypes.Where(type => type.BaseType == baseType).ToArray();

            if (subItemTypes.Length > 0) // 서브 타입이 존재하면
            {
                // 추상 클래스 포함 옵션이 켜져 있거나 현재 타입이 추상 클래스가 아니면 메뉴에 추가
                if (showAbstract || !baseType.IsAbstract)
                    // 메뉴 경로: 기본 경로 + 현재 타입 이름 + "/" + 현재 타입 이름
                    menu.AddItem(new GUIContent(defaultPath + baseType.ToString() + "/" + baseType.ToString()), baseType == selectedType, delegate { selectAction(baseType); });

                // 현재 타입의 각 서브 타입에 대해 재귀 호출
                foreach (Type subType in subItemTypes)
                {
                    // 메뉴 경로: 기본 경로 + 현재 타입 이름 + "/"
                    SubType(ref menu, itemTypes, subType, selectAction, defaultPath + baseType.ToString() + "/", selectedType);
                }
            }
            else // 서브 타입이 없으면 (가장 하위 타입)
            {
                // 추상 클래스 포함 옵션이 켜져 있거나 현재 타입이 추상 클래스가 아니면 메뉴에 추가
                if (showAbstract || !baseType.IsAbstract)
                    // 메뉴 경로: 기본 경로 + 현재 타입 이름
                    menu.AddItem(new GUIContent(defaultPath + baseType.ToString()), baseType == selectedType, delegate { selectAction(baseType); });
            }
        }

        /// <summary>
        /// 지정된 SerializedProperty가 참조하는 애셋을 프로젝트 창에서 선택합니다.
        /// </summary>
        /// <param name="serializedProperty">선택할 애셋을 참조하는 SerializedProperty.</param>
        public static void SelectAsset(SerializedProperty serializedProperty)
        {
            // 프로퍼티가 참조하는 오브젝트 가져오기
            Object objectReference = serializedProperty.objectReferenceValue;

            if (objectReference != null) // 오브젝트 참조가 유효하면
            {
                EditorUtility.FocusProjectWindow(); // Unity 프로젝트 창에 포커스
                Selection.activeObject = objectReference; // 해당 오브젝트를 선택
            }
        }

        /// <summary>
        /// 지정된 Unity Object 애셋을 프로젝트 창에서 선택합니다.
        /// </summary>
        /// <param name="objectReference">선택할 Unity Object 애셋.</param>
        public static void SelectAsset(Object objectReference)
        {
            if (objectReference != null) // 오브젝트 참조가 유효하면
            {
                EditorUtility.FocusProjectWindow(); // Unity 프로젝트 창에 포커스
                Selection.activeObject = objectReference; // 해당 오브젝트를 선택
            }
        }

        /// <summary>
        /// 프로젝트 전체에서 특정 타입의 애셋을 하나 찾아 반환합니다.
        /// 같은 타입의 애셋이 여러 개 있는 경우 첫 번째 검색된 애셋을 반환합니다.
        /// </summary>
        /// <param name="type">찾을 애셋의 타입.</param>
        /// <returns>찾은 애셋 또는 null.</returns>
        // Get asset in project - 원본 주석 번역 (Type 매개변수 버전)
        public static Object GetAsset(Type type)
        {
            // AssetDatabase.FindAssets를 사용하여 지정된 타입 이름("t:TypeName")으로 애셋 검색
            string[] assets = AssetDatabase.FindAssets("t:" + type.Name);
            if (assets.Length > 0) // 검색된 애셋이 있다면
            {
                // 첫 번째 검색 결과(GUID)의 경로를 가져와 애셋 로드 후 반환
                return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), type);
            }

            return null; // 찾지 못하면 null 반환
        }

        /// <summary>
        /// 프로젝트 전체에서 특정 타입의 애셋을 하나 찾아 반환합니다.
        /// 같은 타입의 애셋이 여러 개 있는 경우 첫 번째 검색된 애셋을 반환합니다.
        /// 선택적으로 이름을 지정하여 검색할 수 있습니다.
        /// </summary>
        /// <typeparam name="T">찾을 애셋의 타입.</typeparam>
        /// <param name="name">애셋 이름 (선택 사항).</param>
        /// <returns>찾은 애셋 또는 null.</returns>
        // Get asset in project - 원본 주석 번역 (Generic 버전)
        public static T GetAsset<T>(string name = "") where T : Object
        {
            // AssetDatabase.FindAssets를 사용하여 타입 이름과 선택적 이름으로 애셋 검색
            string[] assets = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(T).Name);
            if (assets.Length > 0) // 검색된 애셋이 있다면
            {
                // 첫 번째 검색 결과(GUID)의 경로를 가져와 애셋 로드 후 T 타입으로 캐스팅하여 반환
                return (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(T));
            }

            return null; // 찾지 못하면 null 반환
        }

        /// <summary>
        /// 프로젝트 전체에서 특정 이름과 타입의 애셋을 찾아 반환합니다.
        /// 이름이 같은 애셋이 여러 개 있는 경우 첫 번째 검색된 애셋을 반환합니다.
        /// </summary>
        /// <typeparam name="T">찾을 애셋의 타입.</typeparam>
        /// <param name="name">찾을 애셋의 이름.</param>
        /// <returns>찾은 애셋 또는 null.</returns>
        // Get asset in project - 원본 주석 번역 (이름 기준 검색 버전)
        public static T GetAssetByName<T>(string name = "") where T : Object
        {
            // AssetDatabase.FindAssets를 사용하여 타입 이름과 선택적 이름으로 애셋 검색
            string[] assets = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(T).Name);
            if (assets.Length > 0) // 검색된 애셋이 있다면
            {
                string assetPath;
                for (int i = 0; i < assets.Length; i++) // 검색된 애셋들을 순회하며
                {
                    assetPath = AssetDatabase.GUIDToAssetPath(assets[i]); // GUID로부터 애셋 경로 가져오기
                    if (Path.GetFileNameWithoutExtension(assetPath) == name) // 파일 이름(확장자 제외)이 지정된 이름과 일치하면
                    {
                        return (T)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)); // 해당 애셋 로드 후 T 타입으로 캐스팅하여 반환
                    }
                }
            }

            return null; // 찾지 못하면 null 반환
        }

        /// <summary>
        /// 프로젝트 전체에서 특정 타입의 모든 애셋을 찾아 배열로 반환합니다.
        /// 선택적으로 이름을 지정하여 검색 범위를 좁힐 수 있습니다.
        /// </summary>
        /// <typeparam name="T">찾을 애셋들의 타입.</typeparam>
        /// <param name="name">애셋 이름 (선택 사항).</param>
        /// <returns>찾은 애셋들의 배열 또는 null.</returns>
        // Get assets in project - 원본 주석 번역
        public static T[] GetAssets<T>(string name = "") where T : Object
        {
            // AssetDatabase.FindAssets를 사용하여 타입 이름과 선택적 이름으로 애셋 검색
            string[] assetsPath = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(T).Name);
            if (assetsPath.Length > 0) // 검색된 애셋이 있다면
            {
                T[] assets = new T[assetsPath.Length]; // 결과 애셋을 담을 배열 생성

                for (int i = 0; i < assets.Length; i++) // 검색된 애셋 경로들을 순회하며
                {
                    // 각 경로의 애셋을 로드 후 T 타입으로 캐스팅하여 배열에 저장
                    assets[i] = (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetsPath[i]), typeof(T));
                }

                return assets; // 애셋 배열 반환
            }

            return null; // 찾지 못하면 null 반환
        }

        /// <summary>
        /// 프로젝트에 특정 타입의 애셋이 존재하는지 확인합니다.
        /// 선택적으로 이름을 지정하여 특정 이름의 애셋 존재 여부를 확인할 수 있습니다.
        /// </summary>
        /// <typeparam name="T">확인할 애셋의 타입.</typeparam>
        /// <param name="name">애셋 이름 (선택 사항).</param>
        /// <returns>애셋이 존재하면 true, 그렇지 않으면 false.</returns>
        // Check if project contains asset - 원본 주석 번역
        public static bool HasAsset<T>(string name = "") where T : Object
        {
            // AssetDatabase.FindAssets를 사용하여 타입 이름과 선택적 이름으로 애셋 검색
            string[] assets = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(T).Name);
            if (assets.Length > 0) // 검색된 애셋이 하나라도 있다면
            {
                return true; // 애셋이 존재함
            }

            return false; // 애셋이 존재하지 않음
        }

        /// <summary>
        /// 지정된 타입의 ScriptableObject 애셋을 특정 경로에 생성합니다.
        /// </summary>
        /// <typeparam name="T">생성할 ScriptableObject의 타입.</typeparam>
        /// <param name="type">생성할 ScriptableObject의 실제 Type 객체.</param>
        /// <param name="path">애셋이 생성될 프로젝트 내의 경로 (확장자 제외).</param>
        /// <param name="refresh">애셋 생성 후 AssetDatabase를 새로고침할지 여부.</param>
        /// <returns>생성된 ScriptableObject 애셋.</returns>
        // Create ScriptableObject at path - 원본 주석 번역 (Type 매개변수 버전)
        public static T CreateAsset<T>(System.Type type, string path, bool refresh = false) where T : ScriptableObject
        {
            // 지정된 Type의 ScriptableObject 인스턴스 생성
            T scriptableObject = (T)ScriptableObject.CreateInstance(type);

            // 애셋 저장 경로 설정 (경로 + ".asset" 확장자)
            string itemPath = path + ".asset";

            // 지정된 경로에 애셋 생성
            AssetDatabase.CreateAsset(scriptableObject, itemPath);

            // 애셋 데이터 저장
            AssetDatabase.SaveAssets();

            if (refresh) // 새로고침 옵션이 켜져 있으면
                AssetDatabase.Refresh(); // AssetDatabase 새로고침

            return scriptableObject; // 생성된 애셋 반환
        }

        /// <summary>
        /// 지정된 타입(T)의 ScriptableObject 애셋을 특정 경로에 생성합니다.
        /// </summary>
        /// <typeparam name="T">생성할 ScriptableObject의 타입.</typeparam>
        /// <param name="path">애셋이 생성될 프로젝트 내의 경로 (확장자 제외).</param>
        /// <param name="refresh">애셋 생성 후 AssetDatabase를 새로고침할지 여부.</param>
        /// <returns>생성된 ScriptableObject 애셋.</returns>
        // Create ScriptableObject at path - 원본 주석 번역 (Generic 버전)
        public static T CreateAsset<T>(string path, bool refresh = false) where T : ScriptableObject
        {
            // 지정된 타입(T)의 ScriptableObject 인스턴스 생성
            T scriptableObject = (T)ScriptableObject.CreateInstance(typeof(T));

            // 애셋 저장 경로 설정 (경로 + ".asset" 확장자)
            string itemPath = path + ".asset";

            // 지정된 경로에 애셋 생성
            AssetDatabase.CreateAsset(scriptableObject, itemPath);

            // 애셋 데이터 저장
            AssetDatabase.SaveAssets();

            if (refresh) // 새로고침 옵션이 켜져 있으면
                AssetDatabase.Refresh(); // AssetDatabase 새로고침

            return scriptableObject; // 생성된 애셋 반환
        }

        /// <summary>
        /// 현재 프로젝트 창에서 선택된 위치에 지정된 이름으로 ScriptableObject 애셋을 생성합니다.
        /// 이름이 중복될 경우 숫자를 붙여 고유한 이름을 만듭니다.
        /// </summary>
        /// <typeparam name="T">생성할 ScriptableObject의 타입.</typeparam>
        /// <param name="assetName">생성할 애셋의 이름.</param>
        /// <returns>생성된 ScriptableObject 애셋.</returns>
        public static T CreateScriptableObject<T>(string assetName) where T : ScriptableObject
        {
            // 지정된 타입(T)의 ScriptableObject 인스턴스 생성
            T scriptableObject = ScriptableObject.CreateInstance<T>();

            // 현재 프로젝트 창에서 선택된 오브젝트의 경로 가져오기
            string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            // 선택된 경로가 폴더이면 그대로 사용하고, 파일이면 파일이 속한 폴더 경로를 사용
            selectionPath = Directory.Exists(selectionPath) ? selectionPath : Path.GetDirectoryName(selectionPath).Replace("\\", "/");

            // 경로가 비어있으면 기본값으로 "Assets" 폴더 사용
            if (string.IsNullOrEmpty(selectionPath))
                selectionPath = "Assets";

            string assetExtension = ".asset"; // 애셋 파일 확장자
            string resultPath = selectionPath + "/" + assetName + assetExtension; // 기본 파일 경로

            // 동일한 이름의 파일이 이미 존재하는 경우 숫자를 붙여 고유한 이름 생성
            for (int i = 1; File.Exists(resultPath); i++)
                resultPath = selectionPath + "/" + assetName + " " + i + assetExtension;

            // 최종 결정된 경로에 애셋 생성
            AssetDatabase.CreateAsset(scriptableObject, resultPath);

            // 생성된 애셋을 프로젝트 창에서 선택
            Selection.activeObject = scriptableObject;

            return scriptableObject; // 생성된 애셋 반환
        }

        /// <summary>
        /// 프로젝트 폴더("Assets") 내에서 지정된 이름의 폴더를 재귀적으로 찾아 전체 경로를 반환합니다.
        /// </summary>
        /// <param name="folderName">찾을 폴더의 이름.</param>
        /// <returns>찾은 폴더의 전체 경로 또는 찾지 못했을 경우 빈 문자열.</returns>
        public static string FindFolderPath(string folderName)
        {
            // Application.dataPath (Assets 폴더 경로)를 시작으로 재귀 함수 호출
            string resultFolder = FindSubfolder(folderName, Application.dataPath);

            // 폴더를 찾지 못했을 경우 경고 메시지 출력
            if (string.IsNullOrEmpty(resultFolder))
            {
                Debug.LogWarning("폴더 '" + folderName + "' 를 찾을 수 없습니다!"); // 한글 경고 메시지
            }

            return resultFolder; // 찾은 폴더 경로 또는 빈 문자열 반환
        }

        /// <summary>
        /// FindFolderPath 메서드에서 내부적으로 사용되는 재귀 함수입니다.
        /// 특정 경로를 시작으로 하위 폴더들을 순회하며 지정된 이름의 폴더를 찾습니다.
        /// </summary>
        /// <param name="folderName">찾을 폴더의 이름.</param>
        /// <param name="rootPath">검색을 시작할 루트 경로.</param>
        /// <returns>찾은 폴더의 전체 경로 또는 찾지 못했을 경우 빈 문자열.</returns>
        private static string FindSubfolder(string folderName, string rootPath)
        {
            // 현재 경로의 모든 하위 디렉토리 목록 가져오기
            string[] subdirectoryEntries = Directory.GetDirectories(rootPath);

            string result = ""; // 검색 결과 경로 (초기값 빈 문자열)

            // 하위 디렉토리들을 순회하며
            foreach (string subdirectory in subdirectoryEntries)
            {
                // 현재 하위 디렉토리 이름이 찾을 폴더 이름과 일치하면
                if (string.Compare(Path.GetFileName(subdirectory), folderName) == 0)
                    return subdirectory; // 해당 경로 반환 (검색 종료)

                // 현재 하위 디렉토리 안에서 재귀적으로 폴더 검색
                result = FindSubfolder(folderName, subdirectory);

                // 재귀 호출 결과 폴더를 찾았으면 (결과가 비어있지 않으면)
                if (!string.IsNullOrEmpty(result))
                    break; // 순회 중단
            }

            return result; // 최종 검색 결과 경로 반환 (찾았으면 경로, 못 찾았으면 빈 문자열)
        }

        /// <summary>
        /// 지정된 SerializedProperty가 배열 요소인지(또는 배열 자체인지) 확인합니다.
        /// </summary>
        /// <param name="property">확인할 SerializedProperty.</param>
        /// <returns>배열 요소 또는 배열이면 true, 그렇지 않으면 false.</returns>
        public static bool IsArray(SerializedProperty property)
        {
            // 프로퍼티 경로를 기반으로 IsArray(string) 메서드 호출
            return IsArray(property.propertyPath);
        }

        /// <summary>
        /// 지정된 프로퍼티 경로가 배열 요소 경로인지(또는 배열 자체 경로인지) 정규식을 사용하여 확인합니다.
        /// </summary>
        /// <param name="propertyPath">확인할 프로퍼티 경로 문자열.</param>
        /// <returns>배열 요소 경로 또는 배열 자체 경로이면 true, 그렇지 않으면 false.</returns>
        public static bool IsArray(string propertyPath)
        {
            // arrayRegex 정규식 패턴과 프로퍼티 경로가 일치하는지 확인
            return arrayRegex.IsMatch(propertyPath);
        }

        /// <summary>
        /// 절대 경로를 Unity 프로젝트의 "Assets" 폴더를 기준으로 하는 상대 경로로 변환합니다.
        /// </summary>
        /// <param name="absolutePath">변환할 절대 경로.</param>
        /// <returns>변환된 상대 경로 또는 입력이 null 또는 빈 문자열이면 null.</returns>
        public static string ConvertToRelativePath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath)) return null; // 입력이 유효하지 않으면 null 반환

            // 프로젝트의 Assets 폴더 URI와 절대 경로 URI 생성
            Uri projectUri = new Uri(Application.dataPath);
            Uri absoluteUri = new Uri(absolutePath);

            // 프로젝트 URI를 기준으로 절대 URI의 상대 경로 계산 후 URL 이스케이프 해제
            string relativePath = Uri.UnescapeDataString(projectUri.MakeRelativeUri(absoluteUri).ToString());

            // 상대 경로가 비어있으면 "Assets" 폴더를 기본값으로 사용
            if (string.IsNullOrEmpty(relativePath))
                relativePath = "Assets";

            // 경로 구분자(\\)를 슬래시(/)로 변환하여 반환
            return relativePath.Replace("\\", "/");
        }
    }
}