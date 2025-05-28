/*
제공된 LevelEditorBase.cs 스크립트는 Unity 에디터에서 사용되는 레벨 에디터 창을 만들기 위한 추상 기본 클래스입니다.
주요 기능 및 용도:
레벨 에디터 창 기본 틀 제공: Unity 에디터의 창으로 동작하며, 레벨 에디터 구축에 필요한 기본적인 구조와 유틸리티 함수들을 제공합니다.
레벨 데이터베이스 관리: 레벨 데이터를 로드하고 관리하는 기능을 포함합니다. 필요한 경우 레벨 데이터베이스 에셋을 생성하는 기능도 제공합니다.
에디터 GUI 그리기: 레벨 에디터 창의 사용자 인터페이스(GUI)를 그리기 위한 기본적인 틀을 제공합니다. 레벨 데이터베이스의 속성을 표시하거나, 레벨 목록을 표시하는 기능 등이 포함될 수 있습니다.
레벨 데이터 조작 유틸리티: 레벨 데이터를 조작하거나 표시하기 위한 여러 유용한 함수들을 포함합니다. 예를 들어, 레벨 파일이 null인지 확인하거나, 레벨 데이터의 유효성을 검증하는 기능 등이 있습니다.
상속을 통한 확장: 이 클래스는 추상 클래스이므로, 실제 레벨 에디터는 이 클래스를 상속받아 구체적인 레벨 데이터 타입과 에디터 기능을 정의하게 됩니다.
유틸리티 함수 제공: 프로젝트 경로 가져오기, 폴더 생성, 씬 열기, 특정 영역 색칠하기, GUI 속성 변경 감지 등 에디터 개발에 유용한 여러 정적(static) 유틸리티 함수들을 제공합니다.
LevelRepresentationBase 내부 클래스: 레벨 데이터를 에디터에서 표현하기 위한 기본 클래스로, 레벨 데이터 오브젝트의 직렬화 관리, 필드 읽기, 유효성 검증, GUI 속성 표시 등의 기능을 추상적으로 정의합니다.
요약하자면, LevelEditorBase.cs는 Unity에서 커스텀 레벨 에디터 창을 개발하기 위한 기반을 제공하며, 레벨 데이터 관리, GUI 구성, 기본적인 데이터 조작 및 유틸리티 기능을 추상화하여 상속받는 클래스에서 실제 기능을 구현하도록 설계되었습니다.
*/
// LevelEditorBase.cs
// 이 스크립트는 Unity 에디터의 레벨 에디터 창을 위한 추상 기본 클래스입니다.
// 레벨 데이터베이스 로딩 및 관리, 에디터 창 GUI 그리기, 레벨 데이터 조작을 위한 기본 틀과 유틸리티 함수들을 제공합니다.
// 실제 레벨 에디터 구현은 이 클래스를 상속받아 구체적인 기능을 정의합니다.

// 경고 비활성화: Unity 직렬화 관련 경고(SerializeField 되었으나 값이 할당되지 않은 경우)를 무시합니다.
#pragma warning disable 649

using UnityEngine;
using UnityEditor; // Unity 에디터 기능을 사용하기 위해 필요
using System;
using System.IO; // 파일 시스템 경로 처리를 위해 필요
using System.Text; // 문자열 처리를 위해 필요
using UnityEditor.SceneManagement; // 씬 관리 기능을 사용하기 위해 필요
using System.Collections.Generic; // List, Dictionary 등 컬렉션을 사용하기 위해 필요
using System.Linq; // LINQ 확장 메서드 (예: GetUnmarkedProperties)를 사용하기 위해 필요. LevelEditorUtils에서 사용될 것으로 가정합니다.

namespace Watermelon // Watermelon 네임스페이스에 포함
{
    // EditorWindow를 상속받아 Unity 에디터 창으로 동작합니다.
    public abstract class LevelEditorBase : EditorWindow
    {
        // 현재 열려 있는 에디터 창 인스턴스
        public static EditorWindow window;

        // 에디터 창의 기본 최소 크기
        public static readonly int DEFAULT_WINDOW_MIN_SIZE = 200;

        // 에디터 창의 기본 제목
        public static readonly string DEFAULT_LEVEL_EDITOR_TITLE = "Level Editor";

        // 기본 레벨 파일 저장 폴더 이름
        protected static string DEFAULT_LEVEL_FOLDER_NAME = "Levels";

        // LevelEditorBase의 현재 활성 인스턴스 (싱글턴 패턴)
        private static LevelEditorBase instance;

        // 경로 구분자 문자열
        protected const string PATH_SEPARATOR = "/";
        // "Assets" 폴더 이름 상수
        private const string ASSETS = "Assets";
        // 레벨 데이터베이스 에셋 파일 이름 상수
        private const string LEVEL_DATABASE_ASSET_FULL_NAME = "Levels Database.asset";
        // 레벨 데이터베이스를 찾을 수 없을 때 표시할 메시지 상수
        private const string LEVELS_DATABASE_NOT_FOUND_MESSAGE = "Levels Database can't be found.";
        // 레벨 데이터베이스 생성 버튼에 표시될 라벨 상수
        private const string CREATE_LEVELS_DATABASE_LABEL = "Create Levels Database";

        // 문자열 생성을 위한 StringBuilder 인스턴스
        public StringBuilder stringBuilder;

        // 로드된 레벨 데이터베이스 오브젝트에 대한 참조
        protected UnityEngine.Object levelsDatabase;

        // 레벨 데이터베이스 오브젝트의 직렬화된 표현
        protected SerializedObject levelsDatabaseSerializedObject;

        // LevelEditorSetting 속성이 붙지 않은 직렬화된 속성들의 컬렉션
        private IEnumerable<SerializedProperty> unmarkedProperties; // LevelEditorUtils.GetUnmarkedProperties 메서드에서 사용될 것으로 가정합니다.

        // 에디터 창 콘텐츠 영역의 스크롤 위치를 저장하는 벡터
        protected Vector2 contentScrollViewVector;

        [SerializeField, Tooltip("에디터 창의 설정 및 제어 정보")] // windowConfiguration 변수에 대한 툴팁
        // 에디터 창의 설정을 담는 객체 (WindowConfiguration 클래스는 외부 정의가 필요합니다.)
        private WindowConfiguration windowConfiguration;

        // GUI가 초기화되었는지 여부를 나타내는 플래그
        private bool guiInitialized;

        // GUI의 기본 색상
        private static Color defaultGUIColor;

        // 에디터 동작 모드의 백업 값 (2D/3D 전환 시 사용)
        private EditorBehaviorMode backupBehaviourMode;

        // 현재 선택된 에디터 리스트 항목의 인덱스
        protected int selectedEditorList;

        // LevelEditorBase의 현재 활성 인스턴스를 가져오는 속성 (싱글턴)
        public static LevelEditorBase Instance { get => instance; }

        // 레벨 파일이 저장될 전체 폴더 경로를 가져오는 속성
        public string LEVELS_FOLDER_PATH { get => LEVELS_DATABASE_FOLDER_PATH + PATH_SEPARATOR + LEVELS_FOLDER_NAME; }

        // 레벨 파일 저장 폴더 이름을 가져오는 속성 (상속받는 클래스에서 오버라이드 가능)
        protected virtual string LEVELS_FOLDER_NAME { get => "Levels"; }

        // 레벨 데이터베이스가 저장될 기본 폴더 경로를 가져오는 속성 (상속받는 클래스에서 오버라이드 가능)
        protected virtual string LEVELS_DATABASE_FOLDER_PATH { get => "Assets/Project Files/Data/Level System"; }

        // GUI의 기본 색상을 가져오는 속성
        protected Color DefaultGUIColor { get => defaultGUIColor; }

        // Unity 에디터에서 한 줄의 기본 높이를 가져오는 속성
        public static float SINGLE_LINE_HEIGHT { get => EditorGUIUtility.singleLineHeight; }

        // Unity 에디터에서 라벨의 기본 너비를 설정하거나 가져오는 속성
        public static float LABEL_WIDTH { get => EditorGUIUtility.labelWidth; set => EditorGUIUtility.labelWidth = value; }

        /// <summary>
        /// Unity 에디터 메뉴에 레벨 에디터 창을 표시하는 메뉴 아이템 메서드입니다.
        /// "Window/Level Editor" 또는 "Tools/Level Editor" 메뉴를 통해 접근 가능합니다.
        /// </summary>
        [MenuItem("Window/Level Editor")]
        [MenuItem("Tools/Level Editor")]
        static void ShowWindow()
        {
            // 상속받는 구체적인 에디터 클래스 타입을 가져옵니다.
            System.Type childType = GetChildType();
            // 해당 타입의 에디터 창을 열거나 가져옵니다.
            window = EditorWindow.GetWindow(childType);
            // 에디터 창의 제목 설정
            window.titleContent = new GUIContent(DEFAULT_LEVEL_EDITOR_TITLE);
            // 에디터 창의 최소 크기 설정
            window.minSize = new Vector2(DEFAULT_WINDOW_MIN_SIZE, DEFAULT_WINDOW_MIN_SIZE);
            // 에디터 창 표시
            window.Show();
        }

        /// <summary>
        /// 메뉴 아이템 "Window/Level Editor" 및 "Tools/Level Editor"가 활성화될지 여부를 결정하는 검증 메서드입니다.
        /// 상속받는 LevelEditorBase 클래스가 존재하는 경우에만 메뉴를 활성화합니다.
        /// </summary>
        /// <returns>메뉴 아이템을 활성화할지 여부</returns>
        [MenuItem("Window/Level Editor", true)]
        [MenuItem("Tools/Level Editor", true)]
        static bool ValidateMenuItem()
        {
            // 상속받는 LevelEditorBase 클래스가 있는지 확인합니다.
            System.Type childType = GetChildType();
            // 자식 타입이 null이 아니면 true를 반환하여 메뉴를 활성화합니다.
            return (childType != null);
        }

        /// <summary>
        /// 현재 어셈블리에서 LevelEditorBase를 상속받는 첫 번째 타입을 찾아 반환합니다.
        /// </summary>
        /// <returns>LevelEditorBase를 상속받는 타입, 없으면 null</returns>
        static System.Type GetChildType()
        {
            // 현재 애플리케이션 도메인에 로드된 모든 어셈블리를 순회합니다.
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 어셈블리 내의 모든 타입을 순회합니다.
                foreach (System.Type classType in assembly.GetTypes())
                {
                    // 현재 타입이 LevelEditorBase를 상속받는지 확인합니다.
                    if (classType.IsSubclassOf(typeof(LevelEditorBase)))
                    {
                        // 상속받는 타입을 찾으면 즉시 반환합니다.
                        return classType;
                    }
                }
            }

            // 상속받는 타입을 찾지 못했으면 null을 반환합니다.
            return null;
        }

        /// <summary>
        /// 에디터 창이 활성화될 때 호출됩니다.
        /// 변수 초기화, 필요한 폴더 생성, 레벨 데이터베이스 로딩 및 직렬화 객체 생성 등을 수행합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            // 변수 초기화
            stringBuilder = new StringBuilder();
            defaultGUIColor = GUI.color;
            guiInitialized = false;
            instance = this; // 싱글턴 인스턴스 설정

            // 레벨 데이터베이스 폴더와 레벨 파일 폴더가 없으면 생성합니다.
            CreateFolderIfNotExist(LEVELS_DATABASE_FOLDER_PATH);
            CreateFolderIfNotExist(LEVELS_FOLDER_PATH);
            // 지정된 타입의 레벨 데이터베이스 에셋을 로드합니다. (EditorUtils.GetAsset 메서드는 외부 정의가 필요합니다.)
            levelsDatabase = EditorUtils.GetAsset(GetLevelsDatabaseType());

            // 레벨 데이터베이스가 성공적으로 로드된 경우
            if (levelsDatabase != null)
            {
                // 레벨 데이터베이스 오브젝트의 직렬화된 표현을 생성합니다.
                levelsDatabaseSerializedObject = new SerializedObject(levelsDatabase);
                // LevelEditorSetting 속성이 붙지 않은 속성들을 가져옵니다. (LevelEditorUtils.GetUnmarkedProperties 메서드는 외부 정의가 필요합니다.)
                unmarkedProperties = LevelEditorUtils.GetUnmarkedProperties(levelsDatabaseSerializedObject);
                // 레벨 데이터베이스의 필드 값을 읽어옵니다. (추상 메서드)
                ReadLevelDatabaseFields();
                // 변수들을 초기화합니다. (추상 메서드)
                InitializeVariables();
                // 어셈블리 리로드 전에 호출될 콜백 메서드를 등록합니다.
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            }
        }

        /// <summary>
        /// 레벨 데이터베이스에서 LevelEditorSetting 속성이 붙지 않은 모든 속성들을 기본 GUILayout으로 표시합니다.
        /// </summary>
        public virtual void DisplayProperties()
        {
            // LevelEditorSetting 속성이 붙지 않은 각 속성에 대해
            foreach (SerializedProperty item in unmarkedProperties)
            {
                // Unity 에디터의 기본 속성 필드를 사용하여 해당 속성을 그립니다.
                EditorGUILayout.PropertyField(item);
            }
        }

        /// <summary>
        /// LevelEditorSetting 속성이 붙지 않은 레벨 데이터베이스의 속성들을 표시하는 탭 내용을 그립니다.
        /// 이 메서드는 에디터 GUI의 한 탭으로 사용될 수 있습니다.
        /// </summary>
        public virtual void DisplayPropertiesTab()
        {
            // unmarkedProperties의 열거자를 가져옵니다.
            IEnumerator<SerializedProperty> enumerator = unmarkedProperties.GetEnumerator();

            // 표시할 속성이 없는 경우 안내 메시지를 표시합니다.
            if (!enumerator.MoveNext())
            {
                EditorGUILayout.HelpBox("This tab is used to display database fields without [LevelEditorSetting] attribvute.", MessageType.Info);
                return;
            }

            // 세로 레이아웃 시작 (가로 최대 너비 제한)
            Rect rect = EditorGUILayout.BeginVertical();
            // 기본 라벨 너비 백업
            float backupLabelWidth = EditorGUIUtility.labelWidth;
            // 레이아웃 너비에 맞춰 라벨 너비 조정 (최소값 보장)
            EditorGUIUtility.labelWidth = Mathf.Max(backupLabelWidth, (rect.width - 8) / 2f);
            // 속성들을 표시합니다.
            DisplayProperties();
            // 라벨 너비를 원래대로 복원
            EditorGUIUtility.labelWidth = backupLabelWidth;
            // 세로 레이아웃 종료
            EditorGUILayout.EndVertical();
        }


        /// <summary>
        /// 에디터 GUI 스타일을 정의하는 메서드입니다. (상속받는 클래스에서 오버라이드 가능)
        /// </summary>
        protected virtual void Styles()
        {
            // 기본 구현은 비어 있습니다.
        }

        /// <summary>
        /// 에디터 창의 설정을 구성하는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        /// <param name="builder">WindowConfiguration 객체를 만들기 위한 빌더 객체</param>
        /// <returns>구성된 WindowConfiguration 객체</returns>
        protected abstract WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder); // WindowConfiguration 클래스는 외부 정의가 필요합니다.

        /// <summary>
        /// 레벨 데이터베이스의 타입을 가져오는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        /// <returns>레벨 데이터베이스의 Type 객체</returns>
        protected abstract System.Type GetLevelsDatabaseType();

        /// <summary>
        /// 단일 레벨 데이터의 타입을 가져오는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        /// <returns>레벨 데이터의 Type 객체</returns>
        public abstract System.Type GetLevelType();

        /// <summary>
        /// 레벨 데이터베이스의 필드 값을 읽어와 에디터에 표시하기 위해 준비하는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        protected abstract void ReadLevelDatabaseFields();

        /// <summary>
        /// 에디터 창 사용에 필요한 변수들을 초기화하는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        protected abstract void InitializeVariables();

        /// <summary>
        /// SetUpWindowConfiguration에서 설정된 내용을 기반으로 에디터 창의 실제 속성을 적용합니다.
        /// 제목, 최소/최대 크기 제한 등을 설정합니다.
        /// </summary>
        private void ApplyWindowConfiguration()
        {
            // 윈도우 설정 객체가 null이거나 윈도우 객체가 null이면 작업을 수행하지 않습니다.
            if (windowConfiguration == null)
            {
                //Debug.Log("windowConfiguration == null");
                return;
            }

            if (window == null)
            {
                //Debug.Log("window == null");
                return;
            }

            // 윈도우 제목 설정
            window.titleContent = new GUIContent(windowConfiguration.WindowTitle);

            // 최소 크기 제한이 설정되어 있다면 적용
            if (windowConfiguration.RestrictWindowMinSize)
            {
                window.minSize = windowConfiguration.WindowMinSize;
            }

            // 최대 크기 제한이 설정되어 있다면 적용
            if (windowConfiguration.RestrictWindowMaxSize)
            {
                window.maxSize = windowConfiguration.WindowMaxSize;
            }
        }

        #region Scene level editor bugs fix
        // 씬 레벨 에디터 버그 수정 관련 영역

        // 에디터 스크립트 업데이트 또는 리임포트 시 발생할 수 있는 버그 수정
        /// <summary>
        /// 어셈블리 리로드 직전에 호출되는 콜백 메서드입니다.
        /// 윈도우 설정에 따라 스크립트 리로드 시 에디터 창을 닫을지 결정합니다.
        /// </summary>
        public virtual void OnBeforeAssemblyReload()
        {
            // 윈도우 설정이 있고, 스크립트 리로드 시 창을 닫도록 설정되어 있으며, 창이 열려 있는 경우
            if ((windowConfiguration != null) && (!windowConfiguration.KeepWindowOpenOnScriptReload) && (window != null))
            {
                // 에디터 창을 닫습니다.
                window.Close();
            }
        }

        // 게임 실행 중 에디터 창이 활성화되어 있을 때 발생할 수 있는 버그 수정
        /// <summary>
        /// Unity 에디터가 플레이 모드로 전환되려고 하거나 이미 플레이 모드인 경우 에디터 창을 닫습니다.
        /// </summary>
        /// <returns>플레이 모드로 인해 창이 닫혔으면 true, 그렇지 않으면 false</returns>
        public virtual bool WindowClosedInPlaymode()
        {
            // Unity 에디터가 플레이 모드로 전환되려고 하거나 이미 플레이 모드인 경우
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // 에디터 창이 열려 있다면
                if (window != null)
                {
                    // 에디터 창을 닫습니다.
                    window.Close();
                }
                // 창이 닫혔음을 알립니다.
                return true;
            }
            else
            {
                // 플레이 모드가 아니므로 창을 닫지 않았음을 알립니다.
                return false;
            }
        }

        #endregion

        /// <summary>
        /// 에디터 창의 GUI를 그리는 핵심 메서드입니다. 매 프레임 업데이트됩니다.
        /// GUI 초기화, 플레이 모드 체크, 레벨 데이터베이스 로드 여부 확인, 콘텐츠 영역 그리기 등을 처리합니다.
        /// </summary>
        public void OnGUI()
        {
            // GUI가 아직 초기화되지 않았다면 초기화합니다.
            if (!guiInitialized)
            {
                InitializeGUI();
                guiInitialized = true;
            }

            // 플레이 모드로 인해 창이 닫혔는지 확인하고, 닫혔다면 더 이상 GUI를 그리지 않습니다.
            if (WindowClosedInPlaymode())
            {
                return;
            }

            // 레벨 데이터베이스가 로드되지 않았다면 생성 버튼을 그립니다.
            if (levelsDatabase == null)
            {
                DrawCreateLevelDatabase();
                return;
            }

            // 윈도우 설정에 따라 콘텐츠 영역의 높이를 제한할지 결정합니다.
            if (windowConfiguration.RestictContentHeight)
            {
                // 높이와 너비 모두 제한하여 세로 레이아웃 시작
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(windowConfiguration.ContentMaxSize.x), GUILayout.MaxHeight(windowConfiguration.ContentMaxSize.y));
            }
            else
            {
                // 너비만 제한하여 세로 레이아웃 시작
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(windowConfiguration.ContentMaxSize.x));
            }

            // 스크롤 뷰 시작
            contentScrollViewVector = EditorGUILayout.BeginScrollView(contentScrollViewVector);
            // 실제 콘텐츠 내용을 그립니다. (추상 메서드)
            DrawContent();
            // 스크롤 뷰 종료
            EditorGUILayout.EndScrollView();
            // 세로 레이아웃 종료
            EditorGUILayout.EndVertical();

            // GUI 그리기 후 추가 작업을 수행합니다. (가상 메서드)
            AfterOnGUI();

            // 직렬화된 오브젝트의 변경 사항을 실제 오브젝트에 적용하고 에셋에 저장될 준비를 합니다.
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
        }


        /// <summary>
        /// 에디터 GUI를 초기화하는 메서드입니다.
        /// 윈도우 설정, 스타일 설정, 윈도우 속성 적용 등을 수행합니다.
        /// </summary>
        private void InitializeGUI()
        {
            // 윈도우 설정을 구성합니다.
            windowConfiguration = SetUpWindowConfiguration(new WindowConfiguration.Builder());
            // 에디터 스타일을 설정합니다.
            Styles();
            // 구성된 윈도우 설정을 실제 에디터 창에 적용합니다.
            ApplyWindowConfiguration();
        }

        /// <summary>
        /// 레벨 데이터베이스가 없을 때 데이터베이스를 생성하는 GUI를 그립니다.
        /// 오류 메시지와 생성 버튼을 포함합니다.
        /// </summary>
        private void DrawCreateLevelDatabase()
        {
            // 세로 레이아웃 시작
            EditorGUILayout.BeginVertical();
            // 데이터베이스를 찾을 수 없다는 오류 메시지를 표시합니다. (넓은 범위로 표시)
            EditorGUILayout.HelpBox(LEVELS_DATABASE_NOT_FOUND_MESSAGE, MessageType.Error, true);

            // "Create Levels Database" 버튼을 그립니다.
            if (GUILayout.Button(CREATE_LEVELS_DATABASE_LABEL))
            {
                // 버튼 클릭 시:
                // 지정된 타입의 ScriptableObject 인스턴스를 생성하고 에셋으로 저장합니다.
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(GetLevelsDatabaseType()), LEVELS_DATABASE_FOLDER_PATH + PATH_SEPARATOR + LEVEL_DATABASE_ASSET_FULL_NAME);
                // 에셋 데이터베이스를 새로고침하여 생성된 에셋을 인식시킵니다.
                AssetDatabase.Refresh();
                // 에디터 창을 다시 활성화하여 새로 생성된 데이터베이스를 로드합니다.
                OnEnable();
            }

            // 세로 레이아웃 종료
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 특정 레벨 오브젝트를 여는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        /// <param name="levelObject">열 레벨 데이터 오브젝트</param>
        /// <param name="index">레벨의 인덱스</param>
        public abstract void OpenLevel(UnityEngine.Object levelObject, int index);

        /// <summary>
        /// 에디터 리스트에서 특정 레벨을 나타낼 라벨 문자열을 가져오는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        /// <param name="levelObject">레벨 데이터 오브젝트</param>
        /// <param name="index">레벨의 인덱스</param>
        /// <returns>레벨을 나타내는 라벨 문자열</returns>
        public abstract string GetLevelLabel(UnityEngine.Object levelObject, int index);

        /// <summary>
        /// 특정 레벨 오브젝트의 데이터를 초기 상태로 클리어하는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        /// <param name="levelObject">클리어할 레벨 데이터 오브젝트</param>
        public abstract void ClearLevel(UnityEngine.Object levelObject);

        /// <summary>
        /// 전역 검증 과정에서 특정 레벨에 대한 오류를 로그로 출력하는 메서드입니다. (가상 메서드, 필요에 따라 오버라이드)
        /// 기본 구현은 오버라이드되지 않았다는 오류 메시지를 출력합니다.
        /// </summary>
        /// <param name="levelObject">검증할 레벨 데이터 오브젝트</param>
        /// <param name="index">레벨의 인덱스</param>
        public virtual void LogErrorsForGlobalValidation(UnityEngine.Object levelObject, int index)
        {
            // 메서드가 오버라이드되지 않았다는 오류 메시지 출력
            Debug.LogError("LogErrorsForGlobalValidation method not overriden.");
        }

        /// <summary>
        /// 에디터 창의 주요 콘텐츠 영역을 그리는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
        /// 이 메서드 안에서 레벨 목록 표시, 레벨 편집 UI 등이 그려집니다.
        /// </summary>
        protected abstract void DrawContent();

        /// <summary>
        /// OnGUI 메서드의 콘텐츠 그리기 부분 완료 후 추가 작업을 수행하는 가상 메서드입니다. (필요에 따라 오버라이드)
        /// </summary>
        protected virtual void AfterOnGUI()
        {
            // 기본 구현은 비어 있습니다.
        }

        #region useful functions
        // 유용한 유틸리티 함수들을 모아 놓은 영역

        /// <summary>
        /// 현재 프로젝트의 전체 경로를 가져옵니다.
        /// Application.dataPath에서 "Assets" 부분을 제거하여 루트 경로를 얻습니다.
        /// </summary>
        /// <returns>프로젝트의 전체 경로</returns>
        public static string GetProjectPath()
        {
            return Application.dataPath.Replace(ASSETS, string.Empty);
        }

        /// <summary>
        /// 지정된 경로에 폴더가 존재하지 않으면 생성합니다.
        /// </summary>
        /// <param name="directoryPath">생성할 폴더의 경로</param>
        public static void CreateFolderIfNotExist(string directoryPath)
        {
            // 폴더가 존재하는지 확인
            if (!Directory.Exists(directoryPath))
            {
                // 존재하지 않으면 폴더 생성
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 지정된 경로의 씬을 엽니다.
        /// </summary>
        /// <param name="scenePath">열 씬 파일의 경로</param>
        public static void OpenScene(string scenePath)
        {
            // EditorSceneManager를 사용하여 씬 열기
            EditorSceneManager.OpenScene(scenePath);
        }

        /// <summary>
        /// 지정된 사각형 영역을 특정 색상으로 그립니다.
        /// </summary>
        /// <param name="rect">그릴 사각형 영역</param>
        /// <param name="color">사용할 색상</param>
        public static void DrawColorRect(Rect rect, Color color)
        {
            // GUI 색상을 지정된 색상으로 변경
            GUI.color = color;
            // 흰색 텍스처를 사용하여 사각형 영역을 그립니다.
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            // GUI 색상을 기본 색상으로 복원
            GUI.color = defaultGUIColor;
        }

        /// <summary>
        /// SerializedProperty를 GUI로 그리고, 값 변경이 있었는지 확인합니다.
        /// 변경이 있었으면 true를 반환하고, 없으면 false를 반환합니다.
        /// </summary>
        /// <param name="serializedProperty">그릴 SerializedProperty</param>
        /// <param name="content">속성 필드에 표시될 GUIContent (라벨 등)</param>
        /// <returns>값이 변경되었으면 true</returns>
        public static bool IsPropertyChanged(SerializedProperty serializedProperty, GUIContent content)
        {
            // 변경 감지 시작
            EditorGUI.BeginChangeCheck();
            // 속성 필드 그리기 (라벨 포함)
            EditorGUILayout.PropertyField(serializedProperty, content);
            // 변경 감지 종료 및 결과 반환
            return EditorGUI.EndChangeCheck();
        }

        /// <summary>
        /// SerializedProperty를 GUI로 그리고, 값 변경이 있었는지 확인합니다. (라벨은 속성 자체에서 가져옴)
        /// 변경이 있었으면 true를 반환하고, 없으면 false를 반환합니다.
        /// </summary>
        /// <param name="serializedProperty">그릴 SerializedProperty</param>
        /// <returns>값이 변경되었으면 true</returns>
        public static bool IsPropertyChanged(SerializedProperty serializedProperty)
        {
            // 변경 감지 시작
            EditorGUI.BeginChangeCheck();
            // 속성 필드 그리기 (라벨은 속성 자체에서 가져옴)
            EditorGUILayout.PropertyField(serializedProperty);
            // 변경 감지 종료 및 결과 반환
            return EditorGUI.EndChangeCheck();
        }

        /// <summary>
        /// GUI 요소들을 비활성화 그룹으로 시작합니다. 이 그룹 내의 요소들은 상호작용할 수 없습니다.
        /// </summary>
        /// <param name="value">true이면 그룹 비활성화</param>
        public static void BeginDisabledGroup(bool value)
        {
            // 비활성화 그룹 시작
            EditorGUI.BeginDisabledGroup(value);
        }

        /// <summary>
        /// GUI 비활성화 그룹을 종료합니다.
        /// </summary>
        public static void EndDisabledGroup()
        {
            // 비활성화 그룹 종료
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Unity 에디터의 기본 동작 모드를 2D로 전환합니다.
        /// 원래 모드를 백업해 둡니다.
        /// </summary>
        public void StartBehaviourMode2D()
        {
            // 현재 기본 동작 모드 백업
            backupBehaviourMode = EditorSettings.defaultBehaviorMode;
            // 기본 동작 모드를 2D로 설정
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        }

        /// <summary>
        /// Unity 에디터의 기본 동작 모드를 StartBehaviourMode2D에서 백업해 둔 원래 모드로 복원합니다.
        /// </summary>
        public void EndBehaviourMode2D()
        {
            // 기본 동작 모드를 백업해 둔 값으로 복원
            EditorSettings.defaultBehaviorMode = backupBehaviourMode;
        }

        /// <summary>
        /// 지정된 라벨 문자열을 그리는 데 필요한 너비를 계산합니다.
        /// </summary>
        /// <param name="label">너비를 계산할 라벨 문자열</param>
        /// <returns>라벨의 픽셀 너비</returns>
        public static float GetLabelWidth(string label)
        {
            // GUI 스킨의 라벨 스타일을 사용하여 텍스트 크기 계산
            return GUI.skin.label.CalcSize(new GUIContent(label)).x;
        }

        /// <summary>
        /// 지정된 크기와 색상의 단색 Texture2D를 생성합니다.
        /// </summary>
        /// <param name="width">텍스처 너비</param>
        /// <param name="height">텍스처 높이</param>
        /// <param name="color">사용할 색상</param>
        /// <returns>생성된 Texture2D</returns>
        public static Texture2D MakeColoredTexture(int width, int height, Color color)
        {
            // 픽셀 색상 배열 생성
            Color[] pixels = new Color[width * height];

            // 모든 픽셀을 지정된 색상으로 채웁니다.
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            // 새로운 Texture2D 객체 생성
            Texture2D result = new Texture2D(width, height);
            // 픽셀 배열을 텍스처에 적용
            result.SetPixels(pixels);
            // 변경 사항 적용
            result.Apply();
            return result;
        }

        #endregion

        // 내부 클래스: 레벨 데이터를 에디터에서 표현하기 위한 기본 클래스
        protected abstract class LevelRepresentationBase
        {
            // 상수: 레벨 번호 접두사, 구분자, Null 파일 표시, 잘못됨 표시
            protected const string NUMBER = "#";
            protected const string SEPARATOR = " | ";
            protected const string NULL_FILE = "[Null file]";
            private const string INCORRECT = "[Incorrect]";

            // 레벨 데이터 오브젝트의 직렬화된 표현
            protected SerializedObject serializedLevelObject;
            // 실제 레벨 데이터 오브젝트
            protected UnityEngine.Object levelObject;
            // 레벨 파일이 null인지 여부
            private bool nullLevel;
            // 레벨 검증 시 발견된 오류 라벨 목록
            public List<string> errorLabels;
            // LevelEditorSetting 속성이 붙지 않은 직렬화된 속성들의 컬렉션
            private IEnumerable<SerializedProperty> unmarkedProperties;
            // 에디터 리스트에서 선택된 항목의 인덱스 (내부 사용)
            protected int selectedEditorList;

            // 레벨 파일이 null인지 여부를 가져오는 속성
            public bool NullLevel { get => nullLevel; }

            // 레벨 검증이 활성화되었는지 여부를 나타내는 가상 속성 (필요에 따라 오버라이드)
            protected virtual bool LEVEL_CHECK_ENABLED { get => false; }
            // 레벨이 올바른 상태인지 여부를 가져오는 속성 (오류 라벨이 없으면 올바름)
            public bool IsLevelCorrect { get => errorLabels.Count == 0; }

            /// <summary>
            /// LevelRepresentationBase 클래스의 새로운 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="levelObject">이 표현이 나타낼 레벨 데이터 오브젝트</param>
            public LevelRepresentationBase(UnityEngine.Object levelObject)
            {
                this.levelObject = levelObject;
                // 레벨 오브젝트가 null인지 확인
                nullLevel = (levelObject == null);
                // 오류 라벨 목록 초기화
                errorLabels = new List<string>();

                // 레벨 오브젝트가 null이 아닌 경우
                if (!nullLevel)
                {
                    // 레벨 오브젝트의 직렬화된 표현 생성
                    serializedLevelObject = new SerializedObject(levelObject);
                    // 레벨 데이터 필드를 읽어옵니다. (추상 메서드)
                    ReadFields();
                    // LevelEditorSetting 속성이 붙지 않은 속성들을 가져옵니다. (LevelEditorUtils.GetUnmarkedProperties 메서드에서 사용될 것으로 가정합니다.)
                    unmarkedProperties = LevelEditorUtils.GetUnmarkedProperties(serializedLevelObject);
                }
            }

            /// <summary>
            /// 레벨 데이터 오브젝트에서 필드 값을 읽어와 내부 변수에 저장하는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
            /// </summary>
            protected abstract void ReadFields();

            /// <summary>
            /// 레벨 표현의 데이터를 초기 상태로 클리어하는 추상 메서드입니다. (상속받는 클래스에서 구현해야 합니다.)
            /// </summary>
            public abstract void Clear();

            /// <summary>
            /// 직렬화된 오브젝트의 변경 사항을 실제 레벨 데이터 오브젝트에 적용합니다.
            /// </summary>
            public void ApplyChanges()
            {
                // 레벨이 null이 아닌 경우에만 변경 사항 적용
                if (!NullLevel)
                {
                    serializedLevelObject.ApplyModifiedProperties();
                }
            }

            /// <summary>
            /// 에디터 리스트에 표시될 레벨의 라벨 문자열을 생성합니다.
            /// 레벨 번호, 이름, 그리고 검증 결과(잘못된 경우)를 포함합니다.
            /// </summary>
            /// <param name="index">레벨의 인덱스</param>
            /// <param name="stringBuilder">문자열 생성을 위한 StringBuilder 객체</param>
            /// <returns>표시될 레벨 라벨 문자열</returns>
            public virtual string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                // StringBuilder 초기화
                stringBuilder.Clear();
                // 레벨 번호 추가
                stringBuilder.Append(NUMBER);
                stringBuilder.Append(index + 1);
                stringBuilder.Append(SEPARATOR);

                // 레벨 파일이 null인 경우
                if (NullLevel)
                {
                    // Null 파일임을 표시
                    stringBuilder.Append(NULL_FILE);
                }
                else
                {
                    // 레벨 오브젝트의 이름 추가
                    stringBuilder.Append(levelObject.name);

                    // 레벨 검증이 활성화된 경우
                    if (LEVEL_CHECK_ENABLED)
                    {
                        // 레벨 검증 수행
                        ValidateLevel();

                        // 레벨이 올바르지 않은 경우
                        if (!IsLevelCorrect)
                        {
                            // 구분자 및 "Incorrect" 표시 추가
                            stringBuilder.Append(SEPARATOR);
                            stringBuilder.Append(INCORRECT);
                        }
                    }
                }

                // 최종 라벨 문자열 반환
                return stringBuilder.ToString();
            }

            /// <summary>
            /// 레벨 데이터의 유효성을 검증하는 가상 메서드입니다. (필요에 따라 오버라이드하여 구체적인 검증 로직 구현)
            /// 검증 실패 시 errorLabels 리스트에 오류 내용을 추가합니다.
            /// </summary>
            public virtual void ValidateLevel()
            {
                // 기본 구현은 비어 있습니다.
            }

            /// <summary>
            /// 이 레벨 표현에 대한 LevelEditorSetting 속성이 붙지 않은 속성들을 기본 GUILayout으로 표시합니다.
            /// </summary>
            public virtual void DisplayProperties()
            {
                // LevelEditorSetting 속성이 붙지 않은 각 속성에 대해
                foreach (SerializedProperty item in unmarkedProperties)
                {
                    // Unity 에디터의 기본 속성 필드를 사용하여 해당 속성을 그립니다.
                    EditorGUILayout.PropertyField(item);
                }
            }
        }
    }
}

// -----------------
// 레벨 에디터 기본 클래스 v 1.4
// -----------------

// 변경 이력 (Changelog)
// v 1.4
// • 윈도우 콘텐츠 높이 제한 설정을 분리함
// • 모든 유용한 메서드를 정적(static)으로 변경함
// • LevelsHandler에 콜백을 추가함
// • LevelsHandler의 IndexChangeWindow 버그를 수정함
// • LevelEditorBase에 더 많은 유용한 기능을 추가함
// v 1.3
// • 커스텀 리스트에서 스타일 초기화 버그를 수정함
// • 일부 메서드 순서를 재정렬함
// v 1.2
// • Reordable list를 커스텀 리스트로 대체함
// v 1.1
// • 전역 검증 지원을 추가함
// • 검증 로직을 변경함
// v 1 기본 버전 동작