// 스크립트 설명: Unity UI의 레거시 Text 컴포넌트를 TextMeshProUGUI 컴포넌트로 일괄 변환하는 에디터 도구입니다.
// 선택된 게임 오브젝트의 Text 컴포넌트를 제거하고, TextMeshProUGUI 컴포넌트를 추가하여 기존 설정을 최대한 마이그레이션합니다.
using TMPro; // TextMeshProUGUI 관련 네임스페이스
using UnityEditor; // Unity 에디터 기능 사용을 위한 네임스페이스
using UnityEngine; // GameObject, Selection, Undo, FontStyle, TextAnchor, HorizontalWrapMode 관련 네임스페이스
using UnityEngine.UI; // Text 관련 네임스페이스

namespace Watermelon
{
    // TextMeshPro 관련 유틸리티 함수를 제공하는 정적 클래스 (주로 에디터 기능)
    public static class TMPUtils
    {
        // Unity 에디터의 Text 컴포넌트 컨텍스트 메뉴에 "Replace Text Component With Text Mesh Pro" 메뉴 항목 추가
        [MenuItem("CONTEXT/Text/Replace Text Component With Text Mesh Pro", validate = true)]
        /// <summary>
        /// "Replace Text Component With Text Mesh Pro" 메뉴 항목의 활성화/비활성화 상태를 결정합니다.
        /// 하나 이상의 게임 오브젝트가 선택되어 있고, 선택된 모든 오브젝트에 Text 컴포넌트가 있을 때만 메뉴를 활성화합니다.
        /// </summary>
        /// <returns>메뉴를 활성화할 수 있으면 true, 그렇지 않으면 false.</returns>
        private static bool TextSelectedValidation()
        {
            var selectedObjects = Selection.gameObjects; // 현재 선택된 게임 오브젝트 목록 가져오기
            if (selectedObjects.Length == 0) return false; // 선택된 오브젝트가 없으면 메뉴 비활성화

            // 선택된 각 오브젝트를 순회하며 Text 컴포넌트가 있는지 확인
            foreach (var selectedObject in selectedObjects)
            {
                var text = selectedObject.GetComponent<Text>(); // Text 컴포넌트 가져오기
                if (!text) return false; // 하나라도 Text 컴포넌트가 없으면 메뉴 비활성화
            }

            return true; // 모든 조건을 만족하면 메뉴 활성화
        }

        // Unity 에디터의 Text 컴포넌트 컨텍스트 메뉴에 "Replace Text Component With Text Mesh Pro" 메뉴 항목 추가
        [MenuItem("CONTEXT/Text/Replace Text Component With Text Mesh Pro")]
        /// <summary>
        /// 선택된 모든 게임 오브젝트의 레거시 Text 컴포넌트를 TextMeshProUGUI 컴포넌트로 교체합니다.
        /// 기존 Text 컴포넌트의 설정(텍스트 내용, 폰트 크기, 스타일, 정렬, 줄바꿈, 색상 등)을 TextMeshProUGUI로 마이그레이션합니다.
        /// Undo 기능을 지원하여 되돌릴 수 있습니다.
        /// </summary>
        private static void ReplaceSelectedObjects()
        {
            var selectedObjects = Selection.gameObjects; // 현재 선택된 게임 오브젝트 목록 가져오기
            // 작업 실행 전 Undo 기능을 위해 선택된 오브젝트들의 상태 기록
            Undo.RecordObjects(selectedObjects, "Text Component를 Text Mesh Pro Component로 교체"); // 한글 Undo 메시지

            // 선택된 각 오브젝트를 순회하며 교체 작업 수행
            foreach (var selectedObject in selectedObjects)
            {
                var textComp = selectedObject.GetComponent<Text>(); // 레거시 Text 컴포넌트 가져오기
                var textSizeDelta = textComp.rectTransform.sizeDelta; // Text 컴포넌트의 RectTransform 사이즈 Delta 값 저장
                // text 컴포넌트는 메모리에 여전히 살아있으므로 설정은 그대로 유지됩니다.
                // text component is still alive in memory, so the settings are still intact - 원본 주석 번역
                // Undo 기능을 지원하며 Text 컴포넌트를 즉시 파괴
                Undo.DestroyObjectImmediate(textComp);

                // Undo 기능을 지원하며 TextMeshProUGUI 컴포넌트 추가
                var tmp = Undo.AddComponent<TextMeshProUGUI>(selectedObject);

                // 기존 Text 컴포넌트의 설정을 TextMeshProUGUI로 마이그레이션
                tmp.text = textComp.text; // 텍스트 내용 복사
                tmp.fontSize = textComp.fontSize; // 폰트 크기 복사

                var fontStyle = textComp.fontStyle; // 폰트 스타일 가져오기
                switch (fontStyle) // 폰트 스타일에 따라 TextMeshProUGUI 스타일 설정
                {
                    case FontStyle.Normal: tmp.fontStyle = FontStyles.Normal; break;
                    case FontStyle.Bold: tmp.fontStyle = FontStyles.Bold; break;
                    case FontStyle.Italic: tmp.fontStyle = FontStyles.Italic; break;
                    case FontStyle.BoldAndItalic: tmp.fontStyle = FontStyles.Bold | FontStyles.Italic; break; // Bold와 Italic 플래그 조합
                }

                tmp.enableAutoSizing = textComp.resizeTextForBestFit; // 자동 크기 조절 설정 복사
                tmp.fontSizeMin = textComp.resizeTextMinSize; // 최소 폰트 크기 복사
                tmp.fontSizeMax = textComp.resizeTextMaxSize; // 최대 폰트 크기 복사

                var alignment = textComp.alignment; // 텍스트 정렬 설정 가져오기
                switch (alignment) // 텍스트 정렬에 따라 TextMeshProUGUI 정렬 설정
                {
                    case TextAnchor.UpperLeft: tmp.alignment = TextAlignmentOptions.TopLeft; break;
                    case TextAnchor.UpperCenter: tmp.alignment = TextAlignmentOptions.Top; break;
                    case TextAnchor.UpperRight: tmp.alignment = TextAlignmentOptions.TopRight; break;
                    case TextAnchor.MiddleLeft: tmp.alignment = TextAlignmentOptions.MidlineLeft; break;
                    case TextAnchor.MiddleCenter: tmp.alignment = TextAlignmentOptions.Midline; break;
                    case TextAnchor.MiddleRight: tmp.alignment = TextAlignmentOptions.MidlineRight; break;
                    case TextAnchor.LowerLeft: tmp.alignment = TextAlignmentOptions.BottomLeft; break;
                    case TextAnchor.LowerCenter: tmp.alignment = TextAlignmentOptions.Bottom; break;
                    case TextAnchor.LowerRight: tmp.alignment = TextAlignmentOptions.BottomRight; break;
                }

                // Unity 버전 6000 이상 (Unity 2022+)에서 Horizontal Overflow 설정 마이그레이션
#if UNITY_6000
                tmp.textWrappingMode = textComp.horizontalOverflow == HorizontalWrapMode.Wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
#else // 이전 Unity 버전
                tmp.enableWordWrapping = textComp.horizontalOverflow == HorizontalWrapMode.Wrap; // 줄바꿈 설정 복사
#endif

                tmp.color = textComp.color; // 색상 복사
                tmp.raycastTarget = textComp.raycastTarget; // 레이캐스트 타겟 설정 복사
                tmp.richText = textComp.supportRichText; // Rich Text 지원 설정 복사

                tmp.rectTransform.sizeDelta = textSizeDelta; // RectTransform 사이즈 Delta 값 복원
            }
        }
    }
}