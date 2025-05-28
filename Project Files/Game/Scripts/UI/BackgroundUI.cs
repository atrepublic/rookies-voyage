// 이 스크립트는 커스텀 UI 그래픽 요소를 위한 기본 배경 클래스입니다.
// UnityEngine.UI.Graphic을 상속받으며, CanvasRenderer 컴포넌트가 필요함을 명시합니다.
// 구체적인 배경 그리기 로직은 이 클래스를 상속받아 구현될 수 있습니다.
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    // 이 컴포넌트가 부착된 게임 오브젝트에는 CanvasRenderer 컴포넌트가 반드시 필요함을 명시합니다.
    [RequireComponent(typeof(CanvasRenderer))]
    // 커스텀 배경 UI 요소를 위한 기본 클래스입니다.
    // UnityEngine.UI.Graphic을 상속받아 UI 캔버스 시스템과 통합됩니다.
    public class BackgroundUI : Graphic
    {
        // 이 클래스 자체는 추가적인 변수나 메소드 구현을 포함하고 있지 않습니다.
        // 구체적인 배경 표현 방식(예: 점 배경, 패턴 배경 등)은 이 클래스를 상속받는 파생 클래스에서 정의됩니다.
    }
}