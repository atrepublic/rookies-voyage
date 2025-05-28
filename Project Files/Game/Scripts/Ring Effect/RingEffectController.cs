// 이 스크립트는 링 효과 게임 오브젝트를 생성하고 관리하는 컨트롤러입니다.
// 오브젝트 풀링을 사용하여 링 효과 인스턴스를 효율적으로 재사용하고,
// 특정 위치와 설정으로 링 효과 애니메이션을 시작하는 정적 메소드를 제공합니다.
using UnityEngine;

namespace Watermelon
{
    // 링 효과 게임 오브젝트를 생성하고 관리하는 컨트롤러 클래스입니다.
    // 오브젝트 풀링을 활용하여 링 효과 인스턴스를 관리합니다.
    // [System.Serializable] 어트리뷰트가 붙어 있으나, MonoBehaviour에 직접 붙는 경우는 흔치 않으며
    // 다른 MonoBehaviour나 ScriptableObject의 필드로 사용될 때 직렬화되도록 의도되었을 수 있습니다.
    [System.Serializable]
    public class RingEffectController : MonoBehaviour
    {
        // RingEffectController의 싱글톤 인스턴스입니다.
        private static RingEffectController ringEffectController;

        // 링 효과 게임 오브젝트의 원본 프리팹입니다. 오브젝트 풀링에 사용됩니다.
        [Tooltip("링 효과 게임 오브젝트의 원본 프리팹입니다. 오브젝트 풀링에 사용됩니다.")]
        [SerializeField] GameObject ringEffectPrefab;
        // 링 효과에 사용될 기본 색상 그라디언트입니다.
        [Tooltip("링 효과에 사용될 기본 색상 그라디언트입니다.")]
        [SerializeField] Gradient defaultGradient;

        // 링 효과 게임 오브젝트 풀입니다.
        private Pool ringEffectPool; // Pool 클래스는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

        // Unity 생명주기 메소드: 오브젝트가 로드될 때 호출됩니다.
        // 싱글톤 인스턴스를 설정하고 오브젝트 풀을 초기화합니다.
        private void Awake()
        {
            // 싱글톤 인스턴스를 설정합니다.
            ringEffectController = this;

            // 링 효과 프리팹과 이름으로 오브젝트 풀을 생성합니다.
            ringEffectPool = new Pool(ringEffectPrefab, ringEffectPrefab.name); // Pool 클래스는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        }

        // Unity 생명주기 메소드: 오브젝트가 파괴될 때 호출됩니다.
        // 생성된 오브젝트 풀을 정리합니다.
        private void OnDestroy()
        {
            // 링 효과 풀이 null이 아니면 풀을 파괴합니다.
            if (ringEffectPool != null)
                PoolManager.DestroyPool(ringEffectPool); // PoolManager 클래스는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        }

        // 기본 그라디언트를 사용하여 특정 위치에 링 효과 애니메이션을 시작하는 정적 메소드입니다.
        // position: 링 효과가 생성될 월드 위치
        // targetSize: 링의 최종 크기
        // time: 애니메이션 지속 시간
        // easing: 애니메이션에 적용될 이징(Easing) 함수 유형 (Watermelon 라이브러리 사용)
        // 반환값: 시작된 RingEffectCase 인스턴스
        public static RingEffectCase SpawnEffect(Vector3 position, float targetSize, float time, Ease.Type easing) // Ease.Type은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        {
            // 내부적으로 오버로드된 SpawnEffect 메소드를 호출하여 기본 그라디언트를 전달합니다.
            return SpawnEffect(position, ringEffectController.defaultGradient, targetSize, time, easing);
        }

        // 지정된 그라디언트를 사용하여 특정 위치에 링 효과 애니메이션을 시작하는 정적 메소드입니다.
        // position: 링 효과가 생성될 월드 위치
        // gradient: 링 색상 변화에 사용될 그라디언트
        // targetSize: 링의 최종 크기
        // time: 애니메이션 지속 시간
        // easing: 애니메이션에 적용될 이징(Easing) 함수 유형 (Watermelon 라이브러리 사용)
        // 반환값: 시작된 RingEffectCase 인스턴스
        public static RingEffectCase SpawnEffect(Vector3 position, Gradient gradient, float targetSize, float time, Ease.Type easing) // Ease.Type은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
        {
            // 오브젝트 풀에서 링 효과 게임 오브젝트를 가져옵니다.
            GameObject ringObject = ringEffectController.ringEffectPool.GetPooledObject(); // Pool 클래스는 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.
            // 링 오브젝트의 위치를 설정합니다.
            ringObject.transform.position = position;
            // 링 오브젝트의 초기 스케일을 0으로 설정하여 애니메이션 시작 시 보이지 않게 합니다.
            ringObject.transform.localScale = Vector3.zero;
            // 링 오브젝트를 활성화합니다.
            ringObject.SetActive(true);

            // RingEffectCase 인스턴스를 생성하고 필요한 정보를 전달합니다.
            RingEffectCase ringEffectCase = new RingEffectCase(ringObject, targetSize, gradient);

            // 트윈의 지속 시간과 이징 함수를 설정합니다. (Watermelon 라이브러리 사용)
            ringEffectCase.SetDuration(time);
            ringEffectCase.SetEasing(easing);
            // 트윈 애니메이션을 시작합니다. (Watermelon 라이브러리 사용)
            ringEffectCase.StartTween();

            // 시작된 RingEffectCase 인스턴스를 반환합니다.
            return ringEffectCase;
        }
    }
}