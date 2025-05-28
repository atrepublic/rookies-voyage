/*
📄 UIGeneralPowerIndicator.cs 요약
게임 내 '전투력(General Power)' 수치를 UI로 표시하고 강조 애니메이션을 통해 시각적 피드백을 제공하는 UI 컴포넌트야.

🧩 주요 기능
BalanceController에서 관리되는 현재 전투력 수치를 텍스트로 표시함.

전투력이 상승했을 때, 텍스트가 커졌다가 작아지는 DOScale 애니메이션과 함께 화살표 이미지가 잠깐 표시됨.

Show(), Hide(), ShowImmediately() 메서드를 통해 UI의 Fade In/Out 표시 컨트롤도 가능함.

⚙️ 사용 용도
캐릭터 강화, 무기 업그레이드 등으로 전투력이 변화할 때 시각적인 강조 효과 제공.

로비 UI나 캐릭터 정보 패널에서 일반적인 전투력 수치를 항상 또는 필요할 때만 보여줄 수 있음.

*/

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class UIGeneralPowerIndicator : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Image arrowImage;

        private CanvasGroup canvasGroup;

        private TweenCase fadeTweenCase;
        private TweenCase delayTweenCase;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            arrowImage.gameObject.SetActive(false);

            UpdateText();
        }

        private void OnEnable()
        {
            BalanceController.BalanceUpdated += UpdateText;
        }

        private void OnDisable()
        {
            BalanceController.BalanceUpdated -= UpdateText;
        }

        private void OnDestroy()
        {
            fadeTweenCase.KillActive();
            delayTweenCase.KillActive();
        }

        public void UpdateText(bool highlight = false)
        {
            float delay = highlight ? 0.5f : 0f;

            delayTweenCase = Tween.DelayedCall(delay, () =>
            {
                text.text = BalanceController.CurrentGeneralPower.ToString();
            });

            if (highlight)
            {
                arrowImage.gameObject.SetActive(true);
                text.transform.DOPushScale(1.3f, 1f, 0.6f, 0.4f, Ease.Type.SineIn, Ease.Type.SineOut).OnComplete(() =>
                {
                    arrowImage.gameObject.SetActive(false);
                });
            }
        }

        public void Show()
        {
            UpdateText();

            gameObject.SetActive(true);

            fadeTweenCase.KillActive();

            fadeTweenCase = canvasGroup.DOFade(1, 0.3f);
        }

        public void ShowImmediately()
        {
            UpdateText();

            fadeTweenCase.KillActive();

            gameObject.SetActive(true);

            canvasGroup.alpha = 1.0f;
        }

        public void Hide()
        {
            fadeTweenCase.KillActive();

            fadeTweenCase = canvasGroup.DOFade(0, 0.3f).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}