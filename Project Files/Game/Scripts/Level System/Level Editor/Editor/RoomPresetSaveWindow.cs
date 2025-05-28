// 이 스크립트는 Unity 에디터에서 룸 프리셋을 저장하기 위한 모달 창을 관리합니다.
// 사용자로부터 프리셋 이름을 입력받아 콜백 함수를 통해 저장 기능을 실행합니다.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Unity 에디터 기능 사용을 위해 추가

namespace Watermelon.SquadShooter
{
    // 룸 프리셋 저장을 위한 에디터 창 클래스입니다.
    public class RoomPresetSaveWindow : EditorWindow
    {
        private static RoomPresetSaveWindow window;
        // 프리셋 이름 저장 완료 시 호출될 콜백 함수
        private Action<string> calback;
        // 사용자로부터 입력받을 프리셋 이름
        private string presetName;

        // 룸 프리셋 저장 창을 생성하고 표시합니다.
        // <param name="calback">프리셋 이름 입력 완료 후 호출될 콜백 함수입니다.</param>
        public static void CreateRoomPresetSaveWindow(Action<string> calback)
        {
            // RoomPresetSaveWindow 창을 가져오거나 새로 생성합니다.
            window = (RoomPresetSaveWindow)GetWindow(typeof(RoomPresetSaveWindow));
            window.minSize = new Vector2(300, 56); // 창 최소 크기 설정
            window.maxSize = new Vector2(700, 56); // 창 최대 크기 설정
            window.calback = calback; // 콜백 함수 설정
            window.ShowPopup(); // 팝업 창으로 표시
        }

        // 창의 GUI를 그리는 함수입니다.
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(); // 수직 레이아웃 시작

            // 프리셋 이름 입력 필드
            presetName = EditorGUILayout.TextField("프리셋 이름:", presetName);

            EditorGUILayout.BeginHorizontal(); // 버튼을 위한 수평 레이아웃 시작

            // 취소 버튼
            if (GUILayout.Button("취소", EditorCustomStyles.buttonRed))
            {
                Close(); // 창 닫기
            }

            // 저장 버튼
            if (GUILayout.Button("저장", EditorCustomStyles.buttonGreen))
            {
                calback?.Invoke(presetName); // 콜백 함수 호출 (입력된 프리셋 이름 전달)
                Close(); // 창 닫기
            }

            EditorGUILayout.EndHorizontal(); // 수평 레이아웃 종료
            EditorGUILayout.EndVertical(); // 수직 레이아웃 종료
        }
    }
}