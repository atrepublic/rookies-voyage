// SkinPickerWindow.cs
// 이 스크립트는 Unity 에디터에서 스킨을 선택할 수 있는 커스텀 윈도우를 제공합니다.
// 사용자는 등록된 스킨 데이터베이스에서 스킨을 검색하고, 시각적으로 미리보기 후 선택할 수 있습니다.

using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Watermelon
{
    public class SkinPickerWindow : EditorWindow
    {
        private HandlerData handlerData;
        private SerializedProperty property;

        private int tabIndex;
        private Vector2 scrollView;
        private GUIStyle boxStyle;

        private string selectedID;
        private static SkinPickerWindow window;

        /// <summary>
        /// 스킨 선택 윈도우를 띄우고 초기화합니다.
        /// </summary>
        public static void PickSkin(SerializedProperty property, System.Type filterType)
        {
            if (window != null)
                window.Close();

            window = GetWindow<SkinPickerWindow>(true);
            window.titleContent = new GUIContent("Skins Picker");
            window.property = property;
            window.selectedID = property.stringValue;
            window.handlerData = new HandlerData(filterType);
            window.tabIndex = window.handlerData.GetTabIndex(window.selectedID);
            window.scrollView = window.CalculateScrollView();
            window.Show();
        }

        /// <summary>
        /// 선택된 스킨 ID에 따라 초기 스크롤 위치를 계산합니다.
        /// </summary>
        private Vector2 CalculateScrollView()
        {
            if (string.IsNullOrEmpty(selectedID))
                return Vector2.zero;

            return new Vector2(0, 0); // 선택 위치 미사용, 추후 개선 가능
        }

        /// <summary>
        /// 윈도우가 활성화될 때 스타일 설정
        /// </summary>
        private void OnEnable()
        {
            boxStyle = new GUIStyle(EditorCustomStyles.Skin.box);
            boxStyle.overflow = new RectOffset(0, 0, 0, 0);
        }

        /// <summary>
        /// 커스텀 윈도우의 GUI 그리기 함수
        /// </summary>
        private void OnGUI()
        {
            if (property == null)
            {
                Close();
                return;
            }

            if (handlerData.Tabs.Length > 1)
            {
                tabIndex = GUI.Toolbar(new Rect(0, 0, Screen.width, 30), tabIndex, handlerData.TabNames, EditorCustomStyles.tab);
                GUILayout.Space(30);
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-14);
            scrollView = EditorGUILayout.BeginScrollView(scrollView);

            if (handlerData.Tabs.IsInRange(tabIndex))
            {
                HandlerData.Tab selectedTab = handlerData.Tabs[tabIndex];
                AbstractSkinDatabase provider = selectedTab.SkinsProvider;
                int skinsCount = provider.SkinsCount;

                if (skinsCount > 0)
                {
                    for (int i = 0; i < skinsCount; i++)
                    {
                        ISkinData skin = provider.GetSkinData(i);
                        Color defaultColor = GUI.backgroundColor;

                        if (selectedID == skin.ID)
                            GUI.backgroundColor = Color.yellow;

                        Rect elementRect = EditorGUILayout.BeginVertical(boxStyle, GUILayout.MinHeight(58), GUILayout.ExpandHeight(false));
                        GUILayout.Space(58);

                        if (GUI.Button(elementRect, GUIContent.none, GUIStyle.none))
                        {
                            if (selectedID == skin.ID)
                            {
                                Close();
                                return;
                            }
                            selectedID = skin.ID;
                            SelectSkin(selectedID);
                        }

                        using (new EditorGUI.DisabledScope(true))
                        {
                            elementRect.x += 4;
                            elementRect.width -= 8;
                            elementRect.y += 4;
                            elementRect.height -= 8;

                            DrawElement(elementRect, skin, selectedTab.GetPreview(skin), i);
                        }

                        EditorGUILayout.EndVertical();
                        GUI.backgroundColor = defaultColor;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Skins list is empty!", MessageType.Info);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 선택된 스킨 ID를 SerializedProperty에 저장합니다.
        /// </summary>
        private void SelectSkin(string ID)
        {
            property.serializedObject.Update();
            property.stringValue = ID;
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 각 스킨 항목의 미리보기와 정보를 출력하는 함수입니다.
        /// </summary>
        private void DrawElement(Rect rect, ISkinData skinData, Object previewObject, int index)
        {
            float defaultYPosition = rect.y;
            rect.width -= 60;

            Rect propertyPosition = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propertyPosition, $"Skin #{index + 1}");

            propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.LabelField(propertyPosition, skinData.ID);

            Rect boxRect = new Rect(rect.x + propertyPosition.width + 2, defaultYPosition, 58, 58);
            GUI.Box(boxRect, GUIContent.none);

            Texture2D previewTexture = AssetPreview.GetAssetPreview(previewObject);
            if (previewTexture != null)
                GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), previewTexture);
            else
                GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), EditorCustomStyles.GetMissingIcon());
        }

        /// <summary>
        /// 내부 데이터 핸들러 클래스. 필터된 타입 기반으로 탭과 스킨 목록을 구성합니다.
        /// </summary>
        private class HandlerData
        {
            public bool IsInitialised { get; private set; }
            public string[] TabNames { get; private set; }
            public Tab[] Tabs { get; private set; }

            public HandlerData(System.Type filteredType)
            {
                if (filteredType != null)
                {
                    Tabs = new Tab[1];
                    Tabs[0] = new Tab(EditorSkinsProvider.GetSkinsProvider(filteredType));
                }
                else
                {
                    List<AbstractSkinDatabase> skinProviders = EditorSkinsProvider.SkinsDatabases;
                    Tabs = new Tab[skinProviders.Count];
                    TabNames = new string[skinProviders.Count];

                    for (int i = 0; i < skinProviders.Count; i++)
                    {
                        Tabs[i] = new Tab(skinProviders[i]);
                        TabNames[i] = Tabs[i].Name;
                    }
                }

                IsInitialised = true;
            }

            public int GetTabIndex(string selectedID)
            {
                for (int i = 0; i < Tabs.Length; i++)
                {
                    if (Tabs[i].SkinsProvider.GetSkinData(selectedID) != null)
                        return i;
                }
                return 0;
            }

            /// <summary>
            /// 개별 탭 클래스. 데이터베이스와 프리뷰 필드 참조를 관리합니다.
            /// </summary>
            public class Tab
            {
                public string Name { get; private set; }
                public AbstractSkinDatabase SkinsProvider { get; private set; }

                private FieldInfo previewFieldInfo;

                public Tab(AbstractSkinDatabase skinsProvider)
                {
                    SkinsProvider = skinsProvider;
                    Name = GetProviderName();

                    previewFieldInfo = SkinsProvider.SkinType.GetFields(ReflectionUtils.FLAGS_INSTANCE)
                        .First(x => x.GetCustomAttribute<SkinPreviewAttribute>() != null);
                }

                public Object GetPreview(object value)
                {
                    object preview = previewFieldInfo.GetValue(value);
                    return preview as Object;
                }

                private string GetProviderName()
                {
                    if (SkinsProvider == null)
                        return "NULL";

                    return SkinsProvider.GetType().Name
                        .Replace("Skins", "")
                        .Replace("Skin", "")
                        .Replace("Provider", "")
                        .Replace("Database", "")
                        .Replace("Data", "");
                }
            }
        }
    }
}
