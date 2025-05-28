// 스크립트 설명: Unity의 Texture2D 클래스에 대한 확장 메서드를 모아 놓은 정적 클래스입니다.
// 텍스처에 직접 점이나 선을 그리는 등의 픽셀 조작 기능을 제공합니다.
using UnityEngine; // Texture2D, Vector2, Color, Mathf 사용을 위한 네임스페이스

namespace Watermelon
{
    // Texture2D 클래스에 유용한 확장 메서드를 제공하는 정적 클래스
    public static class TextureExtensions
    {
        /// <summary>
        /// Texture2D에 두 점(p1, p2) 사이의 선을 그립니다.
        /// Bresenham's line algorithm과 유사한 방식을 사용하여 픽셀을 설정합니다.
        /// </summary>
        /// <param name="texture">선을 그릴 Texture2D 객체 (확장 메서드의 대상).</param>
        /// <param name="p1">선의 시작점 좌표 (Vector2).</param>
        /// <param name="p2">선의 끝점 좌표 (Vector2).</param>
        /// <param name="col">선에 사용할 색상 (Color).</param>
        public static void DrawLine(this Texture2D texture, Vector2 p1, Vector2 p2, Color col)
        {
            // 현재 위치를 시작점(p1)으로 초기화
            Vector2 t = p1;
            // 두 점 사이의 거리 역수 계산 (한 번에 이동할 거리의 비율)
            float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
            // 이동 진행률 카운터
            float ctr = 0;

            // 현재 위치(t)가 끝점(p2)의 정수 좌표와 같아질 때까지 반복
            while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
            {
                // 시작점(p1)과 끝점(p2) 사이를 ctr 비율만큼 보간하여 현재 위치(t) 업데이트
                t = Vector2.Lerp(p1, p2, ctr);
                // 진행률 증가 (거리 역수만큼 이동)
                ctr += frac;
                // 현재 위치(t)의 정수 좌표에 해당하는 픽셀 색상 설정
                texture.SetPixel((int)t.x, (int)t.y, col);
            }
            // 마지막 점의 픽셀 색상 설정 (반복문 조건에 의해 마지막 점이 처리되지 않을 수 있으므로 추가)
            texture.SetPixel((int)p2.x, (int)p2.y, col);
        }

        /// <summary>
        /// Texture2D의 지정된 좌표(x, y)를 중심으로 일정 반경(radius)의 큰 점을 그립니다.
        /// 간단한 사각형 형태로 픽셀을 설정합니다.
        /// </summary>
        /// <param name="texture">점을 그릴 Texture2D 객체 (확장 메서드의 대상).</param>
        /// <param name="x">점의 중심 X 좌표.</param>
        /// <param name="y">점의 중심 Y 좌표.</param>
        /// <param name="radius">점의 반경 (점의 크기).</param>
        /// <param name="color">점에 사용할 색상 (Color).</param>
        public static void DrawBigDot(this Texture2D texture, int x, int y, int radius, Color color)
        {
            // 반경의 절반 계산
            int halfRadius = radius / 2;
            // 중심 좌표를 기준으로 반경 절반만큼 떨어진 사각형 범위 순회
            for (int i = x - halfRadius; i < x + halfRadius; i++)
            {
                for (int j = y - halfRadius; j < y + halfRadius; j++)
                {
                    // 현재 픽셀 좌표가 텍스처 범위 내에 있는지 확인
                    if (i >= 0 && j >= 0 && i < texture.width && j < texture.height)
                    {
                        // 해당 픽셀 색상 설정
                        texture.SetPixel(i, j, color);
                    }
                }
            }
        }
    }
}