// 이 스크립트는 점(Dots) 패턴 배경의 시각적 속성을 정의하는 직렬화 가능한 클래스입니다.
// BackgroundUI 컴포넌트에 적용될 머티리얼의 쉐이더 속성(색상, 이동 속도 등)을 설정하는 데 사용됩니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 점 배경의 설정을 담는 직렬화 가능한 클래스입니다.
    // MonoBehaviour가 아니므로 씬의 오브젝트에 직접 부착되지 않고, 다른 MonoBehaviour나 ScriptableObject의 필드로 사용됩니다.
    [System.Serializable]
    public class DotsBackground
    {
        // 쉐이더의 "_BaseColor" 속성에 접근하기 위한 PropertyToID입니다.
        private static readonly int BASE_COLOR_HASH = Shader.PropertyToID("_BaseColor");
        // 쉐이더의 "_DotsColor" 속성에 접근하기 위한 PropertyToID입니다.
        private static readonly int DOTS_COLOR_HASH = Shader.PropertyToID("_DotsColor");

        // 쉐이더의 "_MovingSpeed" 속성에 접근하기 위한 PropertyToID입니다.
        private static readonly int MOVING_SPEED_HASH = Shader.PropertyToID("_MovingSpeed");

        // 이 점 배경 설정이 적용될 BackgroundUI 컴포넌트 참조입니다.
        [Tooltip("이 점 배경 설정이 적용될 BackgroundUI 컴포넌트 참조입니다.")]
        [SerializeField] BackgroundUI backgroundImage;
        // 적용 대상 BackgroundUI 컴포넌트를 가져오는 프로퍼티입니다.
        public BackgroundUI BackgroundImage => backgroundImage;

        // 배경의 기본 색상입니다.
        [Tooltip("배경의 기본 색상입니다.")]
        [SerializeField] Color baseColor = Color.white;
        // 배경 기본 색상을 가져오는 프로퍼티입니다.
        public Color BaseColor => baseColor;

        // 점 패턴의 색상입니다.
        [Tooltip("점 패턴의 색상입니다.")]
        [SerializeField] Color dotsColor = Color.black;
        // 점 색상을 가져오는 프로퍼티입니다.
        public Color DotsColor => dotsColor;

        [Space] // 인스펙터에서 공간을 추가합니다.
        // 점 패턴의 이동 속도입니다. (Vector2로 X, Y 방향 속도 정의)
        [Tooltip("점 패턴의 이동 속도입니다. (Vector2로 X, Y 방향 속도 정의)")]
        [SerializeField] Vector2 movingSpeed;

        // 설정된 속성 값들을 대상 BackgroundUI의 머티리얼에 적용하는 메소드입니다.
        public void ApplyParams()
        {
            // 대상 BackgroundUI 컴포넌트로부터 머티리얼을 가져옵니다.
            Material material = backgroundImage.material; // material 프로퍼티는 Graphic 클래스에 정의되어 있습니다.

            // Shader.PropertyToID로 얻은 해시 값을 사용하여 머티리얼의 색상 속성을 설정합니다.
            material.SetColor(BASE_COLOR_HASH, baseColor);
            material.SetColor(DOTS_COLOR_HASH, dotsColor);

            // Shader.PropertyToID로 얻은 해시 값을 사용하여 머티리얼의 벡터 속성을 설정합니다.
            material.SetVector(MOVING_SPEED_HASH, movingSpeed);
        }
    }
}