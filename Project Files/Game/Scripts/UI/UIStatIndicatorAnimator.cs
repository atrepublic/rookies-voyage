// UIStatIndicatorAnimator.cs (로그 정리 버전)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Watermelon; // DOTween 확장 메서드를 사용하기 위함 (DOPushScale 등)

public class UIStatIndicatorAnimator : MonoBehaviour
{
    [Tooltip("애니메이션을 적용할 TextMeshProUGUI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI statTextComponent;

    [Tooltip("표시/숨김 처리할 화살표 Image 컴포넌트")]
    [SerializeField] private Image arrowImageComponent;

    private TweenCase pushScaleTweenCase; // 텍스트 스케일 애니메이션 트윈

    private void Awake()
    {
        // 초기에는 화살표 이미지를 비활성화 상태로 설정
        if (arrowImageComponent != null)
        {
            arrowImageComponent.gameObject.SetActive(false);
        }
        else
        {
            // Inspector에서 할당되지 않았을 경우 경고 (개발 편의성)
            Debug.LogWarning($"[{gameObject.name}] UIStatIndicatorAnimator: arrowImageComponent가 Inspector에 할당되지 않았습니다. 애니메이션이 정상 작동하지 않을 수 있습니다.", gameObject);
        }

        if (statTextComponent == null)
        {
            // Inspector에서 할당되지 않았을 경우 경고 (개발 편의성)
            Debug.LogWarning($"[{gameObject.name}] UIStatIndicatorAnimator: statTextComponent가 Inspector에 할당되지 않았습니다. 애니메이션이 정상 작동하지 않을 수 있습니다.", gameObject);
        }
    }

    /// <summary>
    /// 능력치 증가 시 강조 애니메이션을 재생합니다.
    /// </summary>
    public void PlayHighlightAnimation()
    {
        if (statTextComponent == null || arrowImageComponent == null)
        {
            // 필수 컴포넌트가 없으면 애니메이션을 재생할 수 없습니다.
            return;
        }

        // 진행 중인 이전 스케일 애니메이션이 있다면 중지
        if(pushScaleTweenCase != null)
        {
            pushScaleTweenCase.KillActive();
        }
        
        // 화살표 이미지 활성화
        arrowImageComponent.gameObject.SetActive(true);
        
        // 텍스트 크기 변경 애니메이션 (DOPushScale 사용)
        pushScaleTweenCase = statTextComponent.transform.DOPushScale(1.3f, 1f, 0.6f, 0.4f, Ease.Type.SineIn, Ease.Type.SineOut).OnComplete(() =>
            {
                if (arrowImageComponent != null) 
                {
                    arrowImageComponent.gameObject.SetActive(false);
                }
            });
    }

    private void OnDestroy()
    {
        // 이 오브젝트가 파괴될 때 관련 트윈도 확실히 중지
        if(pushScaleTweenCase != null)
        {
            pushScaleTweenCase.KillActive();
        }
    }
}