using TMPro;
using UnityEngine;
using Watermelon; // Watermelon 프레임워크의 Tween 관련 기능 사용

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 피격 시 나타나는 플로팅 텍스트의 동작을 제어합니다.
    /// 일반 공격 및 치명타 공격에 따라 다른 색상과 크기로 텍스트를 표시하고 애니메이션(이동, 회전, 스케일)합니다.
    /// </summary>
    public class FloatingTextHitBehaviour : FloatingTextBaseBehavior
    {
        [Header("애니메이션 타이밍 및 기본 설정")]
        [SerializeField, Tooltip("텍스트 애니메이션이 시작되기 전까지의 지연 시간입니다. (단위: 초)")]
        private float animationStartDelay = 0.0f;

        [SerializeField, Tooltip("모든 애니메이션 완료 후, 게임 오브젝트가 비활성화될 때까지의 추가 지연 시간입니다. (단위: 초)")]
        private float disableDelayAfterAnimation = 0.3f;

        [SerializeField, Tooltip("텍스트의 기본 크기에 추가로 곱해지는 초기 스케일 배율입니다. 이 스크립트 자체의 설정값입니다.")]
        private float initialScaleMultiplier = 1.0f;

        [Header("회전 애니메이션 설정")]
        [SerializeField, Tooltip("텍스트의 회전 애니메이션에 소요되는 시간입니다. (단위: 초)")]
        private float rotationAnimationDuration = 0.3f; // 이전 'movementAnimationDuration'에서 명확하게 변경

        [SerializeField, Tooltip("텍스트의 회전 애니메이션에 사용될 이징(Easing) 타입입니다.")]
        private Ease.Type rotationEasingType = Ease.Type.Linear; // 이전 'movementEasingType'에서 명확하게 변경

        [Header("스케일 애니메이션 설정")]
        [SerializeField, Tooltip("텍스트 크기가 원래대로 복원되는 애니메이션에 소요되는 시간입니다. (단위: 초)")]
        private float scaleRestoreDuration = 0.3f;

        [SerializeField, Tooltip("텍스트 크기 복원 애니메이션에 사용될 이징(Easing) 타입입니다.")]
        private Ease.Type scaleRestoreEasingType = Ease.Type.QuintIn;

        [Header("이동 애니메이션 설정")] // 새로 추가된 섹션
        [SerializeField, Tooltip("텍스트가 위로 이동할 거리입니다. (월드 단위)")]
        private float upwardMoveDistance = 0.5f;

        [SerializeField, Tooltip("텍스트가 위로 이동하는 데 걸리는 시간입니다. (단위: 초)")]
        private float upwardMoveDuration = 0.5f;

        [SerializeField, Tooltip("텍스트 위로 이동 애니메이션에 사용될 이징(Easing) 타입입니다.")]
        private Ease.Type upwardMoveEasingType = Ease.Type.SineOut;

        [Header("치명타 표시 설정")]
        [Tooltip("치명타 발생 시 텍스트에 적용할 색상입니다.")]
        [SerializeField] private Color criticalHitColor = new Color(1f, 0.4f, 0f);

        [Tooltip("치명타 발생 시, 기본 스케일에 추가로 곱해질 배율입니다. (예: 2.0 입력 시 2배 크기)")]
        [SerializeField] private float criticalHitScaleFactor = 2.0f;

        // 프리팹의 원본 로컬 스케일 값 (Awake 시점에 저장)
        private Vector3 originalPrefabScale;
        // 현재 활성화된 트윈 애니메이션들을 관리하는 컬렉션
        private TweenCaseCollection activeTweens;

        /// <summary>
        /// 컴포넌트가 처음 로드될 때 호출됩니다.
        /// 이 오브젝트의 초기 스케일 값을 저장하고, 트윈 컬렉션을 초기화합니다.
        /// </summary>
        private void Awake()
        {
            originalPrefabScale = transform.localScale;
            activeTweens = new TweenCaseCollection();
        }

        /// <summary>
        /// 이 게임 오브젝트가 파괴될 때 호출됩니다.
        /// 진행 중인 모든 트윈 애니메이션을 중지시켜 예상치 못한 오류나 리소스 누수를 방지합니다.
        /// </summary>
        private void OnDestroy()
        {
            activeTweens?.KillActive();
        }

        /// <summary>
        /// 플로팅 텍스트를 활성화하고, 지정된 애니메이션을 시작합니다.
        /// 치명타 여부에 따라 텍스트의 색상과 크기를 다르게 설정합니다.
        /// </summary>
        /// <param name="textToShow">화면에 표시할 텍스트 문자열입니다.</param>
        /// <param name="externalScaleMultiplier">외부(예: 무기 스크립트)에서 전달하는 추가적인 스케일 배율입니다.</param>
        /// <param name="defaultColor">일반 공격(치명타가 아닐 경우) 시 사용할 기본 텍스트 색상입니다.</param>
        /// <param name="isCriticalHit">이 공격이 치명타인지 여부를 나타내는 불리언 값입니다.</param>
        public override void Activate(string textToShow, float externalScaleMultiplier, Color defaultColor, bool isCriticalHit)
        {

           // Debug.Log($"[FloatingTextHitBehaviour] Activate 시작 - World Position: {transform.position}, isCriticalHit: {isCriticalHit}");
            // 디버그 로그: 치명타 발생 시 관련 정보 출력 (문제 해결 시 제거 또는 주석 처리 가능)
            if (isCriticalHit)
            {
                float actualCritScaleFactor = criticalHitScaleFactor;
                Vector3 finalCalculatedScale = originalPrefabScale * initialScaleMultiplier * externalScaleMultiplier * actualCritScaleFactor;
                Color finalAppliedColor = criticalHitColor;
                //Debug.Log($"[치명타 발생!] 내용: '{textToShow}', 최종 스케일: {finalCalculatedScale}, 색상: {finalAppliedColor}");
            }
            
            activeTweens.KillActive();
            activeTweens = new TweenCaseCollection();

            if (textRef == null)
            {
                Debug.LogError("[FloatingTextHitBehaviour] TextMeshPro 참조(textRef)가 설정되지 않았습니다! 오브젝트: " + gameObject.name, this);
                gameObject.SetActive(false);
                return;
            }

            textRef.text = textToShow;
            textRef.color = isCriticalHit ? criticalHitColor : defaultColor;

            int rotationDirectionSign = Random.value < 0.5f ? -1 : 1;
            float currentCritScaleFactor = isCriticalHit ? criticalHitScaleFactor : 1.0f;

            transform.localScale = originalPrefabScale * initialScaleMultiplier * externalScaleMultiplier * currentCritScaleFactor;
            transform.localRotation = Quaternion.Euler(70, 0, 18 * rotationDirectionSign);

            // 애니메이션 시작 전 오브젝트의 시작 월드 위치 저장 (이동 애니메이션 기준점으로 사용)
            Vector3 startWorldPosition = transform.position;

            activeTweens += Tween.DelayedCall(animationStartDelay, () =>
            {
                // 텍스트 회전 애니메이션: 원래 각도(기울어지지 않은 상태)로 복원
                activeTweens += transform.DOLocalRotate(Quaternion.Euler(70, 0, 0), rotationAnimationDuration)
                                      .SetEasing(rotationEasingType)
                                      .OnComplete(() =>
                {
                    activeTweens += Tween.DelayedCall(disableDelayAfterAnimation, () =>
                    {
                        gameObject.SetActive(false);
                        // OnAnimationCompleted?.Invoke(); // 필요시 기본 클래스의 완료 이벤트 호출
                    });
                });

                // 텍스트 스케일 애니메이션: 프리팹의 원래 기본 크기(originalPrefabScale)로 복원
                activeTweens += transform.DOScale(originalPrefabScale, scaleRestoreDuration)
                                      .SetEasing(scaleRestoreEasingType);

                // [신규] 텍스트 위로 이동 애니메이션: 시작 월드 위치 기준으로 위로 이동
                activeTweens += transform.DOMove(startWorldPosition + (Vector3.up * upwardMoveDistance), upwardMoveDuration)
                                      .SetEasing(upwardMoveEasingType);
            });
        }
    }
}