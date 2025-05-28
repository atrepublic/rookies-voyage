// ==============================================
// 📌 ExperienceUIController.cs
// ✅ 경험치 UI를 제어하는 컨트롤러
// ✅ 레벨 텍스트, 게이지 바, 경험치 애니메이션 등을 처리
// ==============================================

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ExperienceUIController : MonoBehaviour
    {
        [Tooltip("채워지는 경험치 바 (메인)")]
        [SerializeField] private SlicedFilledImage expProgressFillImage;

        [Tooltip("하얀색 경험치 바 (추가 연출용)")]
        [SerializeField] private SlicedFilledImage expProgressBackFillImage;

        [Tooltip("현재 레벨 표시 텍스트")]
        [SerializeField] private TextMeshProUGUI expLevelText;

        [Tooltip("경험치 수치 텍스트")]
        [SerializeField] private TextMeshProUGUI expProgressText;

        [Header("경험치 애니메이션 관리")]
        [SerializeField] private ExperienceStarsManager starsManager;

        private int displayedExpPoints;

        private int hittedStarsAmount = 0;
        private int fixedStarsAmount;

        private float currentFillAmount;
        private float targetFillAmount;

        private TweenCase whiteFillbarCase;
        private TweenCase fillTweenCase;
        private TweenCase floatTweenCase;

        /// <summary>
        /// 📌 초기화 및 UI 업데이트, 이벤트 연결
        /// </summary>
        public void Init()
        {
            starsManager.Init(this);
            UpdateUI(true);
            ExperienceController.ExperienceGained += OnExperienceGained;
        }

        private void OnDestroy()
        {
            whiteFillbarCase.KillActive();
            fillTweenCase.KillActive();
            floatTweenCase.KillActive();

            ExperienceController.ExperienceGained -= OnExperienceGained;
        }

        /// <summary>
        /// 📌 경험치 획득 시 애니메이션 트리거
        /// </summary>
        private void OnExperienceGained(int experience)
        {
            PlayXpGainedAnimation(experience, () =>
            {
                UpdateUI(false);
            });
        }

        /// <summary>
        /// 📌 별(스타) 애니메이션 실행
        /// </summary>
        public void PlayXpGainedAnimation(int starsAmount, System.Action OnComplete = null)
        {
            hittedStarsAmount = 0;
            fixedStarsAmount = starsAmount;

            int currentLevelExp = ExperienceController.CurrentLevelData.ExperienceRequired;
            int requiredExp = ExperienceController.NextLevelData.ExperienceRequired;

            targetFillAmount = Mathf.InverseLerp(currentLevelExp, requiredExp, ExperienceController.ExperiencePoints);
            currentFillAmount = expProgressFillImage.fillAmount;

            Camera mainCamera = Camera.main;

            starsManager.PlayXpGainedAnimation(starsAmount, new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane), () =>
            {
                UpdateUI(false, OnComplete);
            });
        }

        /// <summary>
        /// 📌 스타(별) 하나가 도착했을 때 호출됨
        /// </summary>
        public void OnStarHitted()
        {
            hittedStarsAmount++;

            if (whiteFillbarCase != null)
                whiteFillbarCase.Kill();

            expProgressBackFillImage.gameObject.SetActive(true);
            whiteFillbarCase = expProgressBackFillImage
                .DOFillAmount(Mathf.Lerp(currentFillAmount, targetFillAmount, Mathf.InverseLerp(0, fixedStarsAmount, hittedStarsAmount)), 0.1f)
                .SetEasing(Ease.Type.SineIn);
        }

        /// <summary>
        /// 📌 UI 상태를 즉시 또는 애니메이션으로 갱신
        /// </summary>
        public void UpdateUI(bool instantly, System.Action OnComplete = null)
        {
            int currentLevelExp = ExperienceController.CurrentLevelData.ExperienceRequired;
            int requiredExp = ExperienceController.NextLevelData.ExperienceRequired;

            int firstValue = ExperienceController.ExperiencePoints - currentLevelExp;
            int secondValue = requiredExp - currentLevelExp;

            float fillAmount = Mathf.InverseLerp(currentLevelExp, requiredExp, ExperienceController.ExperiencePoints);

            if (instantly)
            {
                expProgressBackFillImage.fillAmount = fillAmount;
                expProgressFillImage.fillAmount = fillAmount;
                expProgressBackFillImage.gameObject.SetActive(false);
                expLevelText.text = ExperienceController.CurrentLevel.ToString();
                expProgressText.text = $"{firstValue}/{secondValue}";

                OnComplete?.Invoke();
            }
            else
            {
                RunFillAnimation(fillAmount, secondValue, displayedExpPoints, firstValue, OnComplete);
            }

            displayedExpPoints = firstValue;
        }

        /// <summary>
        /// 📌 경험치 바와 텍스트 애니메이션 실행
        /// </summary>
        private void RunFillAnimation(float newFillAmount, float requiredExp, int displayedExpPoints, int currentExpPoints, System.Action OnComplete = null)
        {
            fillTweenCase = Tween.DelayedCall(0.5f, () =>
            {
                fillTweenCase = expProgressFillImage
                    .DOFillAmount(newFillAmount, 0.3f)
                    .SetEasing(Ease.Type.SineIn)
                    .OnComplete(() =>
                    {
                        expLevelText.text = ExperienceController.CurrentLevel.ToString();
                        OnComplete?.Invoke();

                        expProgressBackFillImage.fillAmount = expProgressFillImage.fillAmount;
                        expProgressBackFillImage.gameObject.SetActive(false);
                    });

                floatTweenCase = Tween.DoFloat(displayedExpPoints, currentExpPoints, 0.3f, (value) =>
                {
                    expProgressText.text = $"{(int)value}/{requiredExp}";
                }).SetEasing(Ease.Type.SineIn);
            });
        }
    }
}
