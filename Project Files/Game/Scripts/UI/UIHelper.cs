/*
📄 UIHelper.cs 요약
캐릭터 수에 따라 UI 패널의 크기를 반환해주는 유틸리티 클래스야.

GetPanelSize(int charactersCount) 함수는 캐릭터 수에 따라 적절한 패널 크기(Vector2) 를 반환해.

UI 레이아웃 정렬 시 패널 크기를 자동 조정할 수 있어, 캐릭터 선택창 등에 유용해.
*/

using UnityEngine;

namespace Watermelon.SquadShooter
{
    public static class UIHelper
    {
        public const float PANEL_HEIGHT = 115.0f;

        private static readonly Vector2[] PANEL_SIZES = new Vector2[]
        {
        new Vector2(200.0f, PANEL_HEIGHT),
        new Vector2(215.0f, PANEL_HEIGHT),
        new Vector2(240.0f, PANEL_HEIGHT),
        new Vector2(268.0f, PANEL_HEIGHT),
        new Vector2(285.0f, PANEL_HEIGHT),
        new Vector2(300.0f, PANEL_HEIGHT)
        };

        public static Vector2 GetPanelSize(int charactersCount)
        {
            if (PANEL_SIZES.IsInRange(charactersCount))
                return PANEL_SIZES[charactersCount];

            return PANEL_SIZES[PANEL_SIZES.Length - 1];
        }
    }
}