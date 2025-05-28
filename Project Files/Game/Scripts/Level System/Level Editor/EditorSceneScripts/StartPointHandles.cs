// 이 스크립트는 Unity 에디터에서 시작 지점 오브젝트에 대한 기즈모 설정을 관리합니다.
// 씬 뷰에 원반 형태의 기즈모와 텍스트를 표시하는 데 사용되는 속성들을 정의합니다.
using UnityEngine;
using Watermelon;
// InfoBox 어트리뷰트를 사용하기 위해 editor 네임스페이스가 필요할 수 있습니다.
// using UnityEditor;

namespace Watermelon.SquadShooter
{
    public class StartPointHandles : MonoBehaviour
    {
        [Header("원반")]
        // 기즈모로 표시될 원반의 색상입니다.
        [Tooltip("기즈모로 표시될 원반의 색상입니다.")]
        public Color diskColor;
        // 기즈모로 표시될 원반의 반지름입니다.
        [Tooltip("기즈모로 표시될 원반의 반지름입니다.")]
        public float diskRadius;

        [Header("텍스트")]
        // 기즈모 텍스트의 위치 오프셋입니다.
        [Tooltip("기즈모 텍스트의 위치 오프셋입니다.")]
        public Vector3 textPositionOffset;
        // 기즈모 텍스트의 색상입니다.
        [Tooltip("기즈모 텍스트의 색상입니다.")]
        public Color textColor;
        // 씬 뷰에 텍스트를 표시할지 여부입니다.
        [Tooltip("씬 뷰에 텍스트를 표시할지 여부입니다.")]
        public bool displayText;
        // useTextVariable이 false이면 게임 오브젝트의 이름이 표시됩니다.
        [InfoBox("useTextVariable이 false이면 게임 오브젝트의 이름이 표시됩니다.", InfoBoxType.Normal)]
        // 아래의 'text' 변수를 사용할지, 아니면 게임 오브젝트의 이름을 사용할지 여부입니다.
        [Tooltip("아래의 'text' 변수를 사용할지, 아니면 게임 오브젝트의 이름을 사용할지 여부입니다.")]
        public bool useTextVariable;
        // useTextVariable이 true일 때 표시될 텍스트입니다.
        [Tooltip("useTextVariable이 true일 때 표시될 텍스트입니다.")]
        public string text;
        // 기즈모 선의 두께입니다. (텍스트가 아닌 다른 기즈모 표시에 영향을 줄 수 있습니다. StartPointHandles의 사용 방식에 따라 다를 수 있습니다.)
        [Tooltip("기즈모 선의 두께입니다. (텍스트가 아닌 다른 기즈모 표시에 영향을 줄 수 있습니다.)")]
        public float thickness;
    }
}