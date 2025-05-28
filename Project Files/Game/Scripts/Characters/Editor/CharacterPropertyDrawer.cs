// CharacterPropertyDrawer.cs
// 이 스크립트는 Unity 에디터에서 CharacterData 타입의 속성을 시각적으로 더 보기 좋게 표시하기 위한 커스텀 PropertyDrawer입니다.
// CharacterData 객체 필드 옆에 해당 캐릭터의 프리뷰 이미지를 표시하여 편집 편의성을 높입니다.

using UnityEditor;
using UnityEngine;
using Watermelon.SquadShooter; // CharacterData 타입이 정의된 네임스페이스를 가져옵니다.

namespace Watermelon
{
    // CharacterData 타입에 대해 이 PropertyDrawer를 사용하도록 지정합니다.
    [CustomPropertyDrawer(typeof(CharacterData))]
    public class CharacterPropertyDrawer : PropertyDrawer
    {
        // UI 요소 그리기에 사용될 상수 값들입니다.
        private const int PREVIEW_BOX_WIDTH = 58; // 프리뷰 이미지 박스의 너비
        private const int PREVIEW_BOX_HEIGHT = 58; // 프리뷰 이미지 박스의 높이
        private const int PREVIEW_BOX_PADDING = 2; // 프리뷰 이미지와 박스 경계 사이의 간격
        private const int ELEMENT_SPACING = 2; // UI 요소 간의 세로 간격

        /// <summary>
        /// Unity 에디터 인스펙터에서 CharacterData 속성을 그리는 함수입니다.
        /// 속성 라벨, 객체 필드, 그리고 객체의 프리뷰 이미지를 함께 그립니다.
        /// </summary>
        /// <param name="position">속성이 그려질 영역의 Rect</param>
        /// <param name="property">그릴 SerializedProperty (CharacterData 객체 참조)</param>
        /// <param name="label">속성에 표시될 라벨</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // GUI 그리기 시작을 알립니다. (Undo/Redo 시스템 통합을 위해 필요)
            EditorGUI.BeginProperty(position, label, property);

            // 프리뷰 박스 공간을 제외한 속성 필드의 너비를 계산합니다.
            position.width -= PREVIEW_BOX_WIDTH + ELEMENT_SPACING;

            // 속성의 라벨을 그립니다.
            EditorGUI.LabelField(position, label);

            // 객체 필드를 그릴 위치를 라벨 너비만큼 오른쪽으로 이동하고 너비를 조정합니다.
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;

            // 객체 필드(ObjectField)를 그릴 Rect를 계산합니다. (한 줄 높이)
            Rect propertyPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // 실제 CharacterData 객체 선택 필드와 프리뷰 박스를 그립니다.
            DrawBlock(propertyPosition, property);

            // GUI 그리기 종료를 알립니다. (Undo/Redo 시스템 통합 마무리)
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// CharacterData 객체 선택 필드와 해당 객체의 프리뷰 이미지를 그리는 도우미 함수입니다.
        /// </summary>
        /// <param name="propertyPosition">CharacterData 객체 선택 필드가 그려질 시작 Rect</param>
        /// <param name="property">그릴 SerializedProperty (CharacterData 객체 참조)</param>
        private void DrawBlock(Rect propertyPosition, SerializedProperty property)
        {
            // 프리뷰 박스의 Y 위치 계산을 위해 현재 Y 위치를 저장합니다.
            float defaultYPosition = propertyPosition.y;

            // "Selected character:" 라벨을 그립니다.
            EditorGUI.LabelField(propertyPosition, "Selected character:");

            // 다음 UI 요소를 그릴 위치를 한 줄 높이 + 간격만큼 아래로 이동합니다.
            propertyPosition.y += EditorGUIUtility.singleLineHeight + ELEMENT_SPACING;

            // CharacterData 타입의 객체 필드(ObjectField)를 그립니다.
            // 이 필드를 통해 인스펙터에서 CharacterData ScriptableObject를 할당할 수 있습니다.
            property.objectReferenceValue = EditorGUI.ObjectField(propertyPosition, property.objectReferenceValue, typeof(CharacterData), false);

            // 다음 UI 요소를 그릴 위치를 한 줄 높이 + 간격만큼 아래로 이동합니다.
            propertyPosition.y += EditorGUIUtility.singleLineHeight + ELEMENT_SPACING;

            // 프리뷰 이미지를 표시할 박스의 Rect를 계산합니다.
            // 객체 필드 오른쪽 상단에 위치하도록 합니다.
            Rect boxRect = new Rect(propertyPosition.x + propertyPosition.width + ELEMENT_SPACING, defaultYPosition, PREVIEW_BOX_WIDTH, PREVIEW_BOX_HEIGHT);

            // 프리뷰 이미지를 담을 빈 박스를 그립니다.
            GUI.Box(boxRect, GUIContent.none);

            // SerializedProperty에 CharacterData 객체가 할당되어 있으면 프리뷰 이미지를 가져와 그립니다.
            if(property.objectReferenceValue != null)
            {
                // 할당된 객체를 CharacterData 타입으로 형변환합니다.
                CharacterData character = (CharacterData)property.objectReferenceValue;

                // CharacterData의 PreviewSprite로부터 에셋 프리뷰 텍스처를 가져옵니다.
                Texture2D previewTexture = AssetPreview.GetAssetPreview(character.PreviewSprite);

                // CharacterData 객체가 유효하고 프리뷰 텍스처가 있으면
                if (character != null && previewTexture != null) // Added null check for previewTexture
                {
                    // 프리뷰 박스 내부에 실제 프리뷰 텍스처를 그립니다.
                    GUI.DrawTexture(new Rect(boxRect.x + PREVIEW_BOX_PADDING, boxRect.y + PREVIEW_BOX_PADDING, PREVIEW_BOX_WIDTH - PREVIEW_BOX_PADDING * 2, PREVIEW_BOX_HEIGHT - PREVIEW_BOX_PADDING * 2), previewTexture);
                }
                else // CharacterData 객체가 없거나 프리뷰 텍스처를 가져오지 못했으면
                {
                    // 누락된 아이콘을 가져와 그립니다.
                    GUI.DrawTexture(new Rect(boxRect.x + PREVIEW_BOX_PADDING, boxRect.y + PREVIEW_BOX_PADDING, PREVIEW_BOX_WIDTH - PREVIEW_BOX_PADDING * 2, PREVIEW_BOX_HEIGHT - PREVIEW_BOX_PADDING * 2), EditorCustomStyles.GetMissingIcon());
                }
            }
            else // SerializedProperty에 할당된 CharacterData 객체가 null이면
            {
                // 누락된 아이콘을 가져와 그립니다.
                GUI.DrawTexture(new Rect(boxRect.x + PREVIEW_BOX_PADDING, boxRect.y + PREVIEW_BOX_PADDING, PREVIEW_BOX_WIDTH - PREVIEW_BOX_PADDING * 2, PREVIEW_BOX_HEIGHT - PREVIEW_BOX_PADDING * 2), EditorCustomStyles.GetMissingIcon());
            }
        }

        /// <summary>
        /// CharacterData 속성의 높이를 계산하는 함수입니다.
        /// 객체 필드와 라벨, 그리고 프리뷰 박스를 포함할 수 있도록 충분한 높이를 반환합니다.
        /// </summary>
        /// <param name="property">높이를 계산할 SerializedProperty</param>
        /// <param name="label">속성에 표시될 라벨</param>
        /// <returns>속성의 높이 (픽셀)</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 세 줄 높이 + 간격만큼의 높이를 계산하고, 최소 높이(프리뷰 박스 높이)와 비교하여 더 큰 값을 반환합니다.
            return Mathf.Max(EditorGUIUtility.singleLineHeight * 3 + ELEMENT_SPACING * 2, PREVIEW_BOX_HEIGHT + ELEMENT_SPACING);
        }
    }
}