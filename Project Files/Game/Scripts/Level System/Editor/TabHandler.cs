// 이 스크립트는 Unity 에디터에서 탭 인터페이스를 관리하는 핸들러입니다.
// 여러 개의 탭을 추가하고, 각 탭의 내용을 표시하며, 탭 전환 시 특정 액션을 실행할 수 있도록 합니다.
// GUILayout.Toolbar를 사용하여 탭 버튼을 그립니다.

#pragma warning disable 649

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class TabHandler
    {
        // 탭 이름 배열
        private string[] tabNames;
        // 탭 목록 (Tab 클래스 인스턴스 리스트)
        private List<Tab> tabs;
        // 이전 선택된 탭의 인덱스
        private int previousTabIndex;
        // 현재 선택된 탭의 인덱스
        private int selectedTabIndex;

        // 툴바에 사용할 GUIStyle
        private GUIStyle toolBarStyle;
        // 툴바 스타일 사용 여부
        private bool useToolBarStyle;
        // 툴바 스타일 설정 여부
        private bool toolBarStyleSet;
        // 툴바 비활성화 여부
        [Tooltip("탭 툴바를 비활성화할지 여부입니다.")]
        public bool toolBarDisabled;

        // TabHandler 클래스의 생성자
        // <param name="useToolBarStyle">툴바 스타일을 사용할지 여부입니다. 기본값은 true입니다.</param>
        public TabHandler(bool useToolBarStyle = true)
        {
            this.useToolBarStyle = useToolBarStyle;
            toolBarDisabled = false; // 툴바는 기본적으로 활성화됨
            tabs = new List<Tab>(); // 탭 목록 초기화
        }

        // 새로운 탭을 추가합니다.
        // 탭 목록에 탭을 추가하고 탭 이름 배열을 업데이트합니다.
        // <param name="tab">추가할 Tab 인스턴스입니다.</param>
        public void AddTab(Tab tab)
        {
            tabs.Add(tab); // 탭 목록에 추가
            tabNames = new string[tabs.Count]; // 탭 이름 배열 크기 재설정

            // 탭 이름 배열 업데이트
            for (int i = 0; i < tabNames.Length; i++)
            {
                tabNames[i] = tabs[i].name;
            }
        }

        // 툴바에 사용할 GUIStyle을 설정합니다.
        // <param name="style">설정할 GUIStyle입니다.</param>
        public void SetToolBarStyle(GUIStyle style)
        {
            toolBarStyle = style; // 스타일 설정
            toolBarStyleSet = true; // 스타일 설정 플래그 설정
        }

        // 초기 선택된 탭 인덱스를 설정합니다.
        // <param name="index">설정할 탭 인덱스입니다.</param>
        public void SetTabIndex(int index)
        {
            previousTabIndex = index; // 이전 인덱스 설정
            selectedTabIndex = index; // 현재 인덱스 설정
        }

        // 탭 인터페이스를 화면에 표시하고 사용자의 상호작용을 처리합니다.
        // GUILayout.Toolbar를 사용하여 탭 버튼을 그리고 선택된 탭의 내용을 표시합니다.
        public void DisplayTab()
        {
            EditorGUI.BeginDisabledGroup(toolBarDisabled); // 툴바 비활성화 여부에 따라 GUI 그룹 비활성화

            // 설정된 스타일에 따라 툴바를 그립니다.
            if (toolBarStyleSet && useToolBarStyle)
            {
                selectedTabIndex = GUILayout.Toolbar(previousTabIndex, tabNames, toolBarStyle);
            }
            else
            {
                selectedTabIndex = GUILayout.Toolbar(previousTabIndex, tabNames);
            }

            EditorGUI.EndDisabledGroup(); // GUI 그룹 비활성화 해제

            // 탭이 변경되었는지 확인하고, 변경되었다면 새로운 탭의 openTabFunction을 호출합니다.
            if (selectedTabIndex != previousTabIndex)
            {
                tabs[selectedTabIndex].openTabFunction?.Invoke();
            }

            previousTabIndex = selectedTabIndex; // 현재 인덱스를 이전 인덱스로 업데이트
            tabs[selectedTabIndex].displayFunction?.Invoke(); // 현재 선택된 탭의 displayFunction을 호출하여 내용 표시
        }

        // 기본 툴바 스타일을 설정합니다.
        // EditorCustomStyles.tab 스타일을 사용합니다.
        public void SetDefaultToolbarStyle()
        {
            toolBarStyle = new GUIStyle(EditorCustomStyles.tab); // 기본 스타일 설정
            toolBarStyleSet = true; // 스타일 설정 플래그 설정
        }

        // 탭을 나타내는 내부 클래스
        public class Tab
        {
            // 탭의 이름
            [Tooltip("탭의 이름입니다.")]
            public string name;
            // 탭 내용 표시 함수
            [Tooltip("탭이 선택되었을 때 내용을 표시하는 함수입니다.")]
            public Action displayFunction;
            // 탭 열기 함수 (선택될 때 한 번 호출)
            [Tooltip("탭이 새로 선택될 때 호출되는 함수입니다.")]
            public Action openTabFunction;

            // Tab 클래스의 생성자 (displayFunction만 있는 경우)
            // <param name="name">탭의 이름입니다.</param>
            // <param name="displayFunction">탭 내용 표시 함수입니다.</param>
            public Tab(string name, Action displayFunction)
            {
                this.name = name; // 이름 설정
                this.displayFunction = displayFunction; // 내용 표시 함수 설정
            }

            // Tab 클래스의 생성자 (displayFunction과 openTabFunction 모두 있는 경우)
            // <param name="name">탭의 이름입니다.</param>
            // <param name="displayFunction">탭 내용 표시 함수입니다.</param>
            // <param name="openTabFunction">탭 열기 함수입니다.</param>
            public Tab(string name, Action displayFunction, Action openTabFunction) : this(name, displayFunction)
            {
                this.openTabFunction = openTabFunction; // 탭 열기 함수 설정
            }
        }
    }
}