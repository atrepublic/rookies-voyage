// 이 스크립트는 Unity 에디터에서 레벨에 아이템을 추가하기 위한 창을 관리합니다.
// 에셋을 선택하고 해당 아이템의 월드, 타입 등을 설정하여 레벨 데이터베이스에 추가하는 기능을 제공합니다.
// 프리랩 유효성 검사 기능도 포함합니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Unity 에디터 기능 사용을 위해 추가
using System;
using UnityEngine.AI; // NavMesh 관련 기능 사용을 위해 추가
using Unity.AI.Navigation; // NavMesh 관련 기능 사용을 위해 추가

namespace Watermelon.LevelSystem
{
    // 레벨 아이템 추가 기능을 제공하는 에디터 창 클래스입니다.
    public class AddItemWindow : EditorWindow
    {
        private static EditorWindow window;
        private const int PREVIEW_SIZE = 128; // 프리뷰 이미지 크기
        private const string ITEMS_PROPERTY_PATH = "items"; // LevelsDatabase 내 아이템 목록 프로퍼티 경로
        private const string PREFAB_PROPERTY_PATH = "prefab"; // 아이템 데이터 내 프리팹 참조 프로퍼티 경로
        private const string TYPE_PROPERTY_PATH = "type"; // 아이템 데이터 내 타입 프로퍼티 경로
        private const string HASH_PROPERTY_PATH = "hash"; // 아이템 데이터 내 해시 프로퍼티 경로

        // 레벨 데이터베이스 ScriptableObject
        private LevelsDatabase levelsDatabase;
        // 월드 선택 드롭다운에 표시될 이름 배열
        private string[] worldSelection;
        // 현재 선택된 프리랩의 인덱스 (여러 개 선택 시)
        private int selectedIndex;
        // 레벨 아이템 타입 선택 드롭다운에 표시될 이름 배열
        private string[] levelItemTypeSelection;
        // 드롭다운에서 선택된 월드의 인덱스
        private int selectedWorld;
        // 드롭다운에서 선택된 아이템 타입의 인덱스
        private int selectedType;
        // 추가할 프리팹 GameObject 목록
        private static List<GameObject> refList;
        // GUI 레이아웃에 사용될 사각형 영역 변수들
        private Rect globalRect;
        private Rect layoutRect;
        private Rect textureRect;
        // 프리팹 유효성 검사 상태
        private ValidataionStatus status;
        // 유효성 검사 메시지
        private string validationMessage;
        // 중앙 정렬된 GUIStyle
        private GUIStyle centeredLabelStyle;
        // 스타일 초기화 여부
        private bool stylesInited;

        // Assets 메뉴에 아이템 추가 메뉴 항목을 추가합니다.
        // 메뉴 경로는 "Assets/Add into Level Editor"이며 우선순위는 100입니다.
        [MenuItem("Assets/Add into Level Editor", priority = 100)]
        public static void OpenWindow()
        {
            // 현재 선택된 GameObject들을 refList에 추가합니다.
            refList = new List<GameObject>();
            refList.AddRange(Selection.gameObjects);

            // 각 에셋의 프리뷰 이미지를 미리 생성합니다.
            for (int i = 0; i < refList.Count; i++)
            {
                AssetPreview.GetAssetPreview(refList[i]);
            }

            // AddItemWindow 창을 가져오거나 새로 생성합니다.
            window = EditorWindow.GetWindow(typeof(AddItemWindow));
            window.titleContent = new GUIContent("새 레벨 아이템 추가 중"); // 창 제목 설정
            window.maxSize = new Vector2(300, 300); // 창 최대 크기 설정
            window.minSize = new Vector2(300, 300); // 창 최소 크기 설정
            window.Show(); // 창 표시
        }

        // "Assets/Add into Level Editor" 메뉴 항목의 유효성을 검사합니다.
        // GameObject가 선택되어 있을 때만 메뉴 항목을 활성화합니다.
        [MenuItem("Assets/Add into Level Editor", true, 0)]
        public static bool ValidateOpenWindow()
        {
            return Selection.activeGameObject != null; // 활성 GameObject가 있을 때 true 반환 (메뉴 활성화)
        }

        // 창이 활성화될 때 호출됩니다.
        private void OnEnable()
        {
            // LevelsDatabase 에셋을 로드합니다.
            levelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();
            // 월드 선택 드롭다운에 표시될 이름 배열을 LevelsDatabase 데이터로 채웁니다.
            worldSelection = new string[levelsDatabase.Worlds.Length];
            selectedIndex = 0; // 선택된 프리팹 인덱스 초기화

            for (int i = 0; i < worldSelection.Length; i++)
            {
                worldSelection[i] = levelsDatabase.Worlds[i].WorldType.ToString();
            }

            // LevelItemType 열거형의 이름을 가져와 아이템 타입 선택 드롭다운 배열을 채웁니다.
            levelItemTypeSelection = Enum.GetNames(typeof(LevelItemType));

            textureRect = new Rect(); // 텍스처 표시 영역 Rect 초기화
            selectedWorld = -1; // 선택된 월드 인덱스 초기화
            selectedType = -1; // 선택된 타입 인덱스 초기화
        }

        // 창의 GUI를 그리는 함수입니다.
        private void OnGUI()
        {
            // GUIStyle이 초기화되지 않았다면 초기화합니다.
            if (!stylesInited)
            {
                centeredLabelStyle = new GUIStyle(GUI.skin.label);
                centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
                stylesInited = true;
            }

            globalRect = EditorGUILayout.BeginVertical(); // 전체 수직 레이아웃 시작

            // 여러 개의 프리팹이 선택된 경우, 좌우 화살표 버튼과 현재 인덱스/총 개수를 표시합니다.
            if(refList.Count > 1)
            {
                EditorGUILayout.BeginHorizontal(); // 수평 레이아웃 시작
                GUILayout.FlexibleSpace(); // 유연한 공간 추가
                EditorGUI.BeginDisabledGroup(selectedIndex == 0); // 첫 번째 프리팹일 경우 왼쪽 버튼 비활성화

                // 왼쪽 화살표 버튼
                if (GUILayout.Button("⇦"))
                {
                    selectedIndex--; // 인덱스 감소
                }

                EditorGUI.EndDisabledGroup(); // 비활성화 그룹 종료

                EditorGUILayout.LabelField($"{selectedIndex + 1}/{refList.Count}", centeredLabelStyle, GUILayout.Width(54)); // 현재 인덱스/총 개수 표시

                EditorGUI.BeginDisabledGroup(selectedIndex == refList.Count - 1); // 마지막 프리팹일 경우 오른쪽 버튼 비활성화

                // 오른쪽 화살표 버튼
                if (GUILayout.Button("⇨"))
                {
                    selectedIndex++; // 인덱스 증가
                }

                EditorGUI.EndDisabledGroup(); // 비활성화 그룹 종료
                GUILayout.FlexibleSpace(); // 유연한 공간 추가
                EditorGUILayout.EndHorizontal(); // 수평 레이아웃 종료
            }

            layoutRect = EditorGUILayout.BeginVertical(); // 프리뷰 이미지 표시를 위한 수직 레이아웃 시작
            GUILayout.Space(PREVIEW_SIZE); // 프리뷰 이미지 크기만큼 공간 확보
            EditorGUILayout.EndVertical(); // 수직 레이아웃 종료

            // 프리뷰 이미지 표시 영역 설정 및 이미지 그리기
            textureRect.Set(layoutRect.x + layoutRect.width/2f - PREVIEW_SIZE/2f, layoutRect.y, PREVIEW_SIZE, PREVIEW_SIZE);
            GUI.DrawTexture(textureRect, AssetPreview.GetAssetPreview(refList[selectedIndex])); // 현재 선택된 프리팹의 프리뷰 이미지 그리기

            // 월드 및 타입 선택 드롭다운 메뉴 표시
            selectedWorld = EditorGUILayout.Popup("월드:",selectedWorld, worldSelection);
            selectedType = EditorGUILayout.Popup("타입:", selectedType, levelItemTypeSelection);
            validationMessage = GetValidationMessage(); // 유효성 검사 메시지 가져오기

            // 유효성 검사 상태에 따라 다른 타입의 HelpBox를 표시합니다.
            if(status == ValidataionStatus.PrefabInvalid)
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Error); // 오류 메시지
            }
            else if(status == ValidataionStatus.FieldsNotSet)
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning); // 경고 메시지
            }
            else
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Info); // 정보 메시지
            }

            EditorGUILayout.BeginHorizontal(); // 버튼을 위한 수평 레이아웃 시작

            // 취소 버튼
            if (GUILayout.Button("취소", EditorCustomStyles.buttonRed))
            {
                Close(); // 창 닫기
            }

            EditorGUI.BeginDisabledGroup(status != ValidataionStatus.PrefabValid); // 프리팹이 유효할 때만 추가 버튼 활성화

            // 추가 버튼
            if(GUILayout.Button("추가", EditorCustomStyles.buttonGreen))
            {
                AddNewElement(); // 새로운 요소 추가 함수 호출
            }

            EditorGUI.EndDisabledGroup(); // 비활성화 그룹 종료

            EditorGUILayout.EndHorizontal(); // 수평 레이아웃 종료
            EditorGUILayout.EndVertical(); // 전체 수직 레이아웃 종료
        }

        // 선택된 프리팹들을 LevelsDatabase에 새로운 아이템으로 추가합니다.
        private void AddNewElement()
        {
            // 선택된 월드의 SerializedObject와 아이템 목록 SerializedProperty를 가져옵니다.
            SerializedObject worldObject = new SerializedObject(levelsDatabase.Worlds[selectedWorld]);
            SerializedProperty itemsProperty = worldObject.FindProperty(ITEMS_PROPERTY_PATH);

            // 선택된 각 프리팹에 대해 처리합니다.
            for (int index = 0; index < refList.Count; index++)
            {
                // 고유한 해시 값 생성 및 중복 검사
                int hash = TimeUtils.GetCurrentUnixTimestamp().GetHashCode() + index;
                bool unique = true;

                do
                {
                    if (!unique)
                    {
                        // 중복 시 새로운 해시 값 생성
                        hash = (TimeUtils.GetCurrentUnixTimestamp() + UnityEngine.Random.Range(1, 9999)).GetHashCode();
                    }

                    // 기존 아이템 목록에서 해시 값 중복 확인
                    for (int i = 0; unique && (i < itemsProperty.arraySize); i++)
                    {
                        if (itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(HASH_PROPERTY_PATH).intValue == hash)
                        {
                            unique = false;
                        }
                    }

                } while (!unique);

                itemsProperty.arraySize++; // 아이템 배열 크기 증가

                // 새로 추가된 요소의 SerializedProperty를 가져와 값 설정
                SerializedProperty newElement = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1);
                newElement.FindPropertyRelative(HASH_PROPERTY_PATH).intValue = hash; // 해시 값 설정
                newElement.FindPropertyRelative(TYPE_PROPERTY_PATH).intValue = selectedType; // 타입 설정
                newElement.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue = refList[index]; // 프리팹 참조 설정
            }

            worldObject.ApplyModifiedProperties(); // SerializedObject 변경 사항 적용
            Close(); // 창 닫기
        }

        // 선택된 프리팹들의 유효성을 검사하고 결과를 메시지로 반환합니다.
        // <returns>유효성 검사 결과 메시지 문자열입니다.</returns>
        private string GetValidationMessage()
        {
            // 월드 또는 타입이 설정되지 않은 경우
            if((selectedWorld == -1) || (selectedType == -1))
            {
                status = ValidataionStatus.FieldsNotSet; // 상태를 "필드 미설정"으로 설정
                return "월드와 타입 팝업에 값을 설정해주세요.";
            }

            status = ValidataionStatus.PrefabInvalid; // 기본 상태를 "프리팹 유효하지 않음"으로 설정

            // 각 프리팹에 대해 유효성 검사 수행
            for (int i = 0; i < refList.Count; i++)
            {
                // Collider 컴포넌트가 있는지 확인
                if (refList[i].GetComponent<Collider>() == null)
                {
                    return $"프리팹 #{i + 1}에 Collider가 없습니다.";
                }

                // 아이템 타입이 Obstacle인 경우 추가 검사
                if (selectedType == (int)LevelItemType.Obstacle)
                {
                    // NavMeshObstacle 컴포넌트가 있는지 확인
                    if (refList[i].GetComponent<NavMeshObstacle>() == null)
                    {
                        return $"프리팹 #{i + 1}에 NavMeshObstacle이 없습니다.";
                    }

                    // NavMeshModifier 컴포넌트가 있는지 확인
                    if (refList[i].GetComponent<NavMeshModifier>() == null)
                    {
                        return $"프리팹 #{i + 1}에 NavMeshModifier가 없습니다.";
                    }

                    // 레이어가 "Obstacle"인지 확인
                    if (refList[i].layer != LayerMask.NameToLayer("Obstacle"))
                    {
                        return $"프리팹 #{i + 1}에 잘못된 레이어가 할당되었습니다. Obstacle 타입 아이템에는 'Obstacle' 레이어만 올바릅니다.";
                    }

                }
                // 아이템 타입이 Environment인 경우 추가 검사
                else if (selectedType == (int)LevelItemType.Environment)
                {
                    // 레이어가 "Obstacle" 또는 "Ground"인지 확인
                    if (!((refList[i].layer == LayerMask.NameToLayer("Obstacle")) || (refList[i].layer == LayerMask.NameToLayer("Ground"))))
                    {
                        return $"프리팹 #{i + 1}에 잘못된 레이어가 할당되었습니다. Environment 타입 아이템에는 'Obstacle' 또는 'Ground' 레이어가 올바릅니다.";
                    }
                }

            }

            status = ValidataionStatus.PrefabValid; // 모든 검사를 통과하면 상태를 "프리팹 유효함"으로 설정
            return "모든 프리팹이 유효성 검사를 통과했습니다."; // 성공 메시지
        }

        // 유효성 검사 상태를 나타내는 열거형
        private enum ValidataionStatus
        {
            PrefabInvalid, // 프리팹이 유효하지 않음
            FieldsNotSet, // 필요한 필드가 설정되지 않음
            PrefabValid // 프리팹이 유효함
        }
    }
}