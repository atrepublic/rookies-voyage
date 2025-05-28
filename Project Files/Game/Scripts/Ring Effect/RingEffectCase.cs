// 이 스크립트는 특정 시각 효과를 위한 링 애니메이션을 제어하는 커스텀 트윈 케이스입니다.
// TweenCase 기본 클래스를 상속받아 링 오브젝트의 크기와 색상을 시간에 따라 변화시키는 애니메이션을 구현합니다.
// MaterialPropertyBlock을 사용하여 링의 머티리얼 속성을 동적으로 변경합니다.
using UnityEngine;

namespace Watermelon
{
    // 링 효과 애니메이션을 위한 커스텀 트윈 케이스입니다.
    // TweenCase 클래스를 상속받아 시간 경과에 따른 링의 크기와 색상 변화를 관리합니다.
    public class RingEffectCase : TweenCase
    {
        // 쉐이더의 "_Scale" 속성에 접근하기 위한 PropertyToID입니다.
        private static readonly int SHADER_SCALE_PROPERTY = Shader.PropertyToID("_Scale");
        // 쉐이더의 "_Color" 속성에 접근하기 위한 PropertyToID입니다.
        private static readonly int SHADER_COLOR_PROPERTY = Shader.PropertyToID("_Color");

        // 제어할 링 게임 오브젝트입니다.
        private GameObject ringGameObject;
        // 링 게임 오브젝트의 MeshRenderer 컴포넌트입니다.
        private MeshRenderer ringMeshRenderer;

        // 링의 머티리얼 속성을 동적으로 설정하기 위한 MaterialPropertyBlock입니다.
        private MaterialPropertyBlock materialPropertyBlock;

        // 링이 최종적으로 도달할 크기입니다.
        private float targetSize;
        // 링의 색상 변화를 정의하는 그라디언트입니다.
        private Gradient targetGradient;

        // RingEffectCase 클래스의 생성자입니다.
        // 애니메이션할 링 오브젝트, 목표 크기, 색상 그라디언트를 설정하고 초기 머티리얼 속성을 적용합니다.
        // gameObject: 애니메이션할 링 게임 오브젝트
        // targetSize: 링의 최종 크기
        // targetGradient: 링의 색상 변화에 사용될 그라디언트
        public RingEffectCase(GameObject gameObject, float targetSize, Gradient targetGradient)
        {
            // 전달받은 게임 오브젝트와 해당 MeshRenderer 컴포넌트를 저장합니다.
            ringGameObject = gameObject;
            ringMeshRenderer = ringGameObject.GetComponent<MeshRenderer>();

            // 목표 그라디언트와 목표 크기를 저장합니다.
            this.targetGradient = targetGradient;
            this.targetSize = targetSize;

            // MaterialPropertyBlock 인스턴스를 생성합니다.
            materialPropertyBlock = new MaterialPropertyBlock();

            // 링의 MeshRenderer로부터 현재 MaterialPropertyBlock 설정을 가져옵니다.
            ringMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            // 쉐이더의 "_Color" 속성을 그라디언트의 시작 색상(시간 0.0f)으로 설정합니다.
            materialPropertyBlock.SetColor(SHADER_COLOR_PROPERTY, targetGradient.Evaluate(0.0f));
            // 설정된 MaterialPropertyBlock을 링의 MeshRenderer에 적용합니다.
            ringMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            // 주석 처리된 부분: 원래는 "_Scale" 쉐이더 속성을 설정하려고 했지만,
            // 실제 크기 조절은 transform.localScale을 사용하므로 주석 처리된 것으로 보입니다.
            //materialPropertyBlock.SetFloat(SHADER_SCALE_PROPERTY, 0.1f);
        }

        // TweenCase의 오버라이드 메소드: 트윈이 기본적으로 완료되었을 때 호출됩니다.
        // 링의 최종 크기와 색상을 설정하고 게임 오브젝트를 비활성화합니다.
        public override void DefaultComplete()
        {
            // 링 게임 오브젝트의 로컬 스케일을 목표 크기로 설정합니다.
            // ToVector3() 확장 메소드는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            ringGameObject.transform.localScale = targetSize.ToVector3();

            // MaterialPropertyBlock 설정을 다시 가져와 최종 색상을 적용합니다.
            ringMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            // 쉐이더의 "_Color" 속성을 그라디언트의 끝 색상(시간 1.0f)으로 설정합니다.
            materialPropertyBlock.SetColor(SHADER_COLOR_PROPERTY, targetGradient.Evaluate(1.0f));
            // 설정된 MaterialPropertyBlock을 링의 MeshRenderer에 적용합니다.
            ringMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            // 트윈 완료 후 링 게임 오브젝트를 비활성화하여 풀로 반환될 준비를 합니다.
            ringGameObject.SetActive(false);

            // 주석 처리된 부분: 원래는 "_Scale" 쉐이더 속성을 설정하려고 했지만,
            // 실제 크기 조절은 transform.localScale을 사용하므로 주석 처리된 것으로 보입니다.
            //materialPropertyBlock.SetFloat(SHADER_SCALE_PROPERTY, targetSize);
        }

        // TweenCase의 오버라이드 메소드: 매 프레임마다 호출되어 트윈의 진행 상태에 따라 링의 크기와 색상을 업데이트합니다.
        // deltaTime: 이전 프레임 이후 경과된 시간
        public override void Invoke(float deltaTime)
        {
            // 트윈의 현재 상태(0.0f에서 1.0f 사이)를 보간 함수에 적용하여 보간된 상태 값을 가져옵니다.
            float interpolatedState = Interpolate(State);

            // MaterialPropertyBlock 설정을 다시 가져와 색상을 업데이트합니다.
            ringMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            // 쉐이더의 "_Color" 속성을 현재 보간된 상태에 해당하는 그라디언트 색상으로 설정합니다.
            materialPropertyBlock.SetColor(SHADER_COLOR_PROPERTY, targetGradient.Evaluate(interpolatedState));
            // 설정된 MaterialPropertyBlock을 링의 MeshRenderer에 적용합니다.
            ringMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            // 링 게임 오브젝트의 로컬 스케일을 보간된 상태에 따라 0.1f에서 목표 크기까지 선형 보간하여 설정합니다.
            // Mathf.LerpUnclamped는 보간 상태가 0.0f~1.0f 범위를 벗어나더라도 보간을 수행합니다.
            ringGameObject.transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.1f, targetSize, interpolatedState);

            // 주석 처리된 부분: 원래는 "_Scale" 쉐이더 속성을 설정하려고 했지만,
            // 실제 크기 조절은 transform.localScale을 사용하므로 주석 처리된 것으로 보입니다.
            //materialPropertyBlock.SetFloat(SHADER_SCALE_PROPERTY, Mathf.LerpUnclamped(0.1f, targetSize, interpolatedState));
        }

        // TweenCase의 오버라이드 메소드: 트윈이 유효한 상태인지 확인합니다.
        // 링의 MeshRenderer 컴포넌트가 유효한지 확인하여 유효성을 판단합니다.
        public override bool Validate()
        {
            // 링의 MeshRenderer 컴포넌트가 null이 아니면 유효한 것으로 간주합니다.
            return ringMeshRenderer != null;
        }
    }
}