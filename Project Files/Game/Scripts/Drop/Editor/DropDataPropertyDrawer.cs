// 스크립트 설명: Unity 에디터에서 DropData 타입을 인스펙터에 표시하는 방식을 사용자 정의하는 PropertyDrawer입니다.
// 드롭 타입에 따라 관련 필드만 보이도록 동적으로 레이아웃을 변경합니다.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Unity 에디터 기능 사용을 위한 네임스페이스
using Watermelon.SquadShooter; // DropableItemType, DropData 사용을 위한 네임스페이스

namespace Watermelon.LevelSystem
{
    // DropData 타입에 대해 이 PropertyDrawer를 사용하도록 지정
    [CustomPropertyDrawer(typeof(DropData))]
    public class DropDataPropertyDrawer : UnityEditor.PropertyDrawer
    {
        // 인스펙터 레이아웃에 사용할 컬럼(열) 수
        private const int ColumnCount = 3;
        // 각 컬럼 사이의 간격 크기
        private const int GapSize = 4;
        // 컬럼 사이의 총 간격 수 (컬럼 수 - 1)
        private const int GapCount = ColumnCount - 1;

        /// <summary>
        /// DropData 프로퍼티를 인스펙터에 그리는 방식을 정의합니다.
        /// </summary>
        /// <param name="position">프로퍼티가 그려질 영역.</param>
        /// <param name="property">그려질 SerializedProperty (DropData).</param>
        /// <param name="label">프로퍼티의 레이블.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 프로퍼티 그리기 시작
            EditorGUI.BeginProperty(position, label, property);

            // 현재 그리기 영역의 위치와 크기 정보 가져오기
            var x = position.x;
            var y = position.y;
            // 각 컬럼의 너비 계산
            var width = (position.width - GapCount * GapSize) / ColumnCount;
            // 한 줄 높이 가져오기
            var height = EditorGUIUtility.singleLineHeight;
            // 다음 컬럼으로 이동할 오프셋 (컬럼 너비 + 간격)
            var offset = width + GapSize;

            // DropType 프로퍼티 찾기
            var dropTypeProperty = property.FindPropertyRelative("DropType");
            // DropType 값을 열거형으로 가져오기
            DropableItemType dropType = (DropableItemType)dropTypeProperty.intValue;

            // DropType 열거형 필드 그리기
            EditorGUI.PropertyField(new Rect(x, y, width, height), dropTypeProperty, GUIContent.none);

            // 원래 레이블 너비 저장
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            // DropType에 따라 추가 필드 표시
            if (dropType == DropableItemType.Currency)
            {
                // 화폐 타입일 경우 수량과 화폐 타입 필드 표시
                EditorGUI.PropertyField(new Rect(x + offset, y, width, height), property.FindPropertyRelative("Amount"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(x + offset + offset, y, width, height), property.FindPropertyRelative("CurrencyType"), GUIContent.none);
            }
            else if (dropType == DropableItemType.WeaponCard)
            {
                // 무기 카드 타입일 경우 수량과 무기 데이터 필드 표시
                EditorGUI.PropertyField(new Rect(x + offset, y, width, height), property.FindPropertyRelative("Amount"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(x + offset + offset, y, width, height), property.FindPropertyRelative("Weapon"), GUIContent.none);
            }
            else if (dropType == DropableItemType.Heal)
            {
                // 회복 아이템 타입일 경우 수량 필드만 크게 표시
                EditorGUI.PropertyField(new Rect(x + offset, y, width + width + GapSize, height), property.FindPropertyRelative("Amount"), GUIContent.none);
            }
            else if (dropType == DropableItemType.Weapon)
            {
                // 무기 타입일 경우 무기 데이터와 레벨 필드 표시 (레이블 너비 조절)
                EditorGUIUtility.labelWidth = 68; // 레이블 너비 임시 변경
                EditorGUI.PropertyField(new Rect(x + offset, y, width + width + GapSize, height), property.FindPropertyRelative("Weapon"), new GUIContent("Weapon"));
                EditorGUI.PropertyField(new Rect(x + offset, y + EditorGUIUtility.singleLineHeight + 2, width + width + GapSize, height), property.FindPropertyRelative("Level"), new GUIContent("Level"));
                EditorGUIUtility.labelWidth = originalLabelWidth; // 원래 레이블 너비 복원
            }
            else if (dropType == DropableItemType.Character)
            {
                 // 캐릭터 타입일 경우 캐릭터 데이터와 레벨 필드 표시 (레이블 너비 조절 및 추가 간격)
                EditorGUIUtility.labelWidth = 68; // 레이블 너비 임시 변경
                EditorGUI.PropertyField(new Rect(x + offset, y, width + width + GapSize, height), property.FindPropertyRelative("Character"), GUIContent.none);
                // 캐릭터 레벨 필드는 추가적인 수직 간격 후에 표시
                EditorGUI.PropertyField(new Rect(x + offset, y + EditorGUIUtility.singleLineHeight + 2 + EditorGUIUtility.singleLineHeight + 2 + EditorGUIUtility.singleLineHeight + 2, width + width + GapSize, height), property.FindPropertyRelative("Level"), new GUIContent("Level"));
                EditorGUIUtility.labelWidth = originalLabelWidth; // 원래 레이블 너비 복원
            }

            // 프로퍼티 그리기 종료
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// DropData 프로퍼티가 인스펙터에서 차지할 높이를 계산합니다.
        /// 드롭 타입에 따라 높이가 달라집니다.
        /// </summary>
        /// <param name="property">높이를 계산할 SerializedProperty (DropData).</param>
        /// <param name="label">프로퍼티의 레이블.</param>
        /// <returns>프로퍼티가 차지할 높이.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // DropType 프로퍼티 찾기
            var dropTypeProperty = property.FindPropertyRelative("DropType");
            // DropType 값을 열거형으로 가져오기
            DropableItemType dropType = (DropableItemType)dropTypeProperty.intValue;

            // 무기 타입일 경우 필요한 높이 계산 (두 줄)
            if(dropType == DropableItemType.Weapon)
                return EditorGUIUtility.singleLineHeight * 2 + 2; // 한 줄 높이 * 2 + 간격

            // 캐릭터 타입일 경우 필요한 높이 계산 (네 줄)
            // 캐릭터 데이터 필드가 여러 줄 공간을 차지하는 것처럼 보이므로 계산된 높이가 더 큽니다.
            // 정확한 높이 계산은 포함된 필드들의 높이에 따라 달라질 수 있으나, 원래 로직을 따릅니다.
            if(dropType == DropableItemType.Character)
                return EditorGUIUtility.singleLineHeight * 4 + 6; // 한 줄 높이 * 4 + 간격 (원래 코드의 오프셋 계산 기반)

            // 다른 타입일 경우 기본 높이 반환 (한 줄)
            return base.GetPropertyHeight(property, label);
        }
    }
}