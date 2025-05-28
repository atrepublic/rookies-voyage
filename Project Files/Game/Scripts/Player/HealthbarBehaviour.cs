// 이 스크립트는 체력 바의 표시 및 동작을 관리하는 컴포넌트입니다.
// 대상 오브젝트의 체력 상태에 따라 체력 바의 채우기 이미지, 색상, 텍스트, 보호막 이미지를 업데이트하고
// 체력 변화에 반응하여 체력 바의 표시 상태(숨김/표시)를 제어합니다.
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon; // DOFillAmount, DOFade 등의 트위닝 기능을 제공하는 외부 라이브러리(Watermelon) 사용 가정

namespace Watermelon.SquadShooter
{
    // 게임 오브젝트에 추가되어 체력 바 UI를 관리하는 클래스입니다.
    // [System.Serializable] 어트리뷰트가 붙어 있으나, MonoBehaviour에 직접 붙는 경우는 흔치 않으며
    // 다른 MonoBehaviour나 ScriptableObject의 필드로 사용될 때 직렬화되도록 의도되었을 수 있습니다.
    [System.Serializable]
    public class HealthbarBehaviour : MonoBehaviour
    {
        // 체력 바 UI 오브젝트의 트랜스폼입니다. 부모 오브젝트를 따라다니는 데 사용됩니다.
        [Tooltip("체력 바 UI 오브젝트의 트랜스폼입니다. 부모 오브젝트를 따라다니는 데 사용됩니다.")]
        [SerializeField] Transform healthBarTransform;
        // 체력 바 UI 오브젝트의 트랜스폼을 가져오는 프로퍼티입니다.
        public Transform HealthBarTransform => healthBarTransform;

        // 부모 오브젝트 위치로부터 체력 바 UI의 상대적인 위치 오프셋입니다.
        [Tooltip("부모 오브젝트 위치로부터 체력 바 UI의 상대적인 위치 오프셋입니다.")]
        [SerializeField] Vector3 healthbarOffset;
        // 체력 바 위치 오프셋을 가져오는 프로퍼티입니다.
        public Vector3 HealthbarOffset => healthbarOffset;

        [Space] // 인스펙터에서 공간을 추가합니다.
        // 체력 바 UI 전체의 투명도를 제어하는 CanvasGroup 컴포넌트입니다.
        [Tooltip("체력 바 UI 전체의 투명도를 제어하는 CanvasGroup 컴포넌트입니다.")]
        [SerializeField] CanvasGroup healthBarCanvasGroup;
        // 현재 체력 비율을 시각적으로 표시하는 채우기 이미지입니다. (일반적으로 녹색/빨간색)
        [Tooltip("현재 체력 비율을 시각적으로 표시하는 채우기 이미지입니다. (일반적으로 녹색/빨간색)")]
        [SerializeField] Image healthFillImage;
        // 체력 변화 시 부드러운 애니메이션을 위한 마스크 채우기 이미지입니다.
        [Tooltip("체력 변화 시 부드러운 애니메이션을 위한 마스크 채우기 이미지입니다.")]
        [SerializeField] Image maskFillImage;
        // 체력 값을 텍스트로 표시하는 TextMeshProUGUI 컴포넌트입니다.
        [Tooltip("체력 값을 텍스트로 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField] TextMeshProUGUI healthText;
        // 보호막 유무 또는 레벨 등을 표시하는 이미지입니다.
        [Tooltip("보호막 유무 또는 레벨 등을 표시하는 이미지입니다.")]
        [SerializeField] Image shieldImage;

        [Space] // 인스펙터에서 공간을 추가합니다.
        // 일반 상태일 때 체력 바의 색상입니다.
        [Tooltip("일반 상태일 때 체력 바의 색상입니다.")]
        [SerializeField] Color standartHealthbarColor;
        // 특수 상태일 때 체력 바의 색상입니다. (예: 보스)
        [Tooltip("특수 상태일 때 체력 바의 색상입니다.")]
        [SerializeField] Color specialHealthbarColor;
        // 일반 상태일 때 보호막 이미지의 색상입니다.
        [Tooltip("일반 상태일 때 보호막 이미지의 색상입니다.")]
        [SerializeField] Color standartShieldColor;
        // 특수 상태일 때 보호막 이미지의 색상입니다. (예: 보스)
        [Tooltip("특수 상태일 때 보호막 이미지의 색상입니다.")]
        [SerializeField] Color specialShieldColor;

        // 이 체력 바가 추적할 체력 정보(CurrentHealth, MaxHealth)를 제공하는 대상입니다.
        private IHealth targetHealth;
        // 이 체력 바가 위치를 따라갈 부모 오브젝트의 트랜스폼입니다.
        private Transform parentTransform;
        // 체력 바를 항상 표시할지 여부를 나타냅니다. false이면 체력 감소 시에만 표시됩니다.
        private bool showAlways;

        // 체력 바의 기본 위치 오프셋입니다.
        private Vector3 defaultOffset;

        // 체력 바가 초기화되었는지 여부를 나타냅니다.
        private bool isInitialised;
        // 체력 바 패널이 현재 활성화(표시) 상태인지 여부를 나타냅니다.
        private bool isPanelActive;
        // 체력 바가 현재 비활성화(숨김) 상태인지 여부를 나타냅니다.
        private bool isDisabled;
        // 체력 바에 표시될 레벨 값입니다. -1이면 실제 체력 값이 표시됩니다.
        private int level;

        // 마스크 채우기 이미지의 트위닝 애니메이션을 관리하는 TweenCase입니다.
        private TweenCase maskTweenCase;
        // 체력 바 패널(CanvasGroup)의 투명도 트위닝 애니메이션을 관리하는 TweenCase입니다.
        private TweenCase panelTweenCase;

        // 체력 바를 초기화하고 설정하는 메소드입니다.
        // targetHealth의 체력 상태를 추적하고 parentTransform을 따라다니도록 설정합니다.
        // parentTransform: 체력 바가 따라갈 부모 오브젝트의 트랜스폼
        // targetHealth: 체력 정보를 제공하는 대상 (IHealth 인터페이스 구현)
        // showAlways: 체력 바를 항상 표시할지 여부 (false이면 체력 감소 시 표시)
        // defaultOffset: 부모 오브젝트 기준 기본 위치 오프셋
        // level: 체력 대신 표시할 레벨 값 (기본값 -1: 체력 값 표시)
        // isSpecial: 특수 색상을 사용할지 여부
        public void Init(Transform parentTransform, IHealth targetHealth, bool showAlways, Vector3 defaultOffset, int level = -1, bool isSpecial = false)
        {
            // 전달받은 값으로 내부 변수를 설정합니다.
            this.targetHealth = targetHealth;
            this.parentTransform = parentTransform;
            this.defaultOffset = defaultOffset;
            this.level = level;
            this.showAlways = showAlways;

            // 초기 상태를 설정합니다.
            isDisabled = false;
            isPanelActive = false;

            // 체력 바 트랜스폼의 부모를 null로 설정하여 독립적인 위치를 갖도록 합니다.
            healthBarTransform.SetParent(null);
            // 체력 바 게임 오브젝트를 활성화합니다.
            healthBarTransform.gameObject.SetActive(true);

            // 특수 상태 여부에 따라 체력 바와 보호막 이미지의 색상을 설정합니다.
            if (isSpecial)
            {
                healthFillImage.color = specialHealthbarColor;
                shieldImage.color = specialShieldColor;
            }
            else
            {
                healthFillImage.color = standartHealthbarColor;
                shieldImage.color = standartShieldColor;
            }

            // 현재 체력 상태에 맞춰 체력 바를 다시 그립니다.
            RedrawHealth();

            // showAlways 설정에 따라 체력 바 패널의 초기 투명도를 설정합니다.
            healthBarCanvasGroup.alpha = showAlways ? 1.0f : 0.0f;

            // 초기화 완료 상태로 설정합니다.
            isInitialised = true;
        }

        // 부모 오브젝트의 위치를 따라 체력 바를 업데이트하는 메소드입니다.
        // 주로 LateUpdate 등에서 호출되어 오브젝트 이동 후 위치를 업데이트하는 데 사용됩니다.
        public void FollowUpdate()
        {
            // 초기화된 상태에서만 작동합니다.
            if (isInitialised)
            {
                // 부모 오브젝트 위치와 기본 오프셋을 더한 위치로 체력 바를 이동합니다.
                healthBarTransform.position = parentTransform.position + defaultOffset;
                // 카메라 방향을 향하도록 체력 바의 회전을 설정합니다. (Billboard 효과)
                healthBarTransform.rotation = Camera.main.transform.rotation;
            }
        }

        // 대상의 체력 변화 시 호출되는 메소드입니다.
        // 체력 바 채우기 이미지, 텍스트를 업데이트하고, 체력 변화에 따라 체력 바 패널의 표시 상태를 제어합니다.
        public void OnHealthChanged()
        {
            // 체력 바가 비활성화 상태이면 업데이트하지 않습니다.
            if (isDisabled)
                return;

            // 대상 체력 정보가 유효한지 확인합니다.
            if (targetHealth == null)
                return;

            // 현재 체력 비율에 따라 체력 채우기 이미지의 fillAmount를 업데이트합니다.
            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;

            // 기존 마스크 트위닝 애니메이션이 있다면 중지합니다. (Watermelon 라이브러리 사용)
            maskTweenCase.KillActive();

            // 마스크 이미지의 fillAmount를 현재 체력 비율로 부드럽게 애니메이션합니다.
            maskTweenCase = maskFillImage.DOFillAmount(healthFillImage.fillAmount, 0.3f).SetEasing(Ease.Type.QuintIn); // Ease.Type.QuintIn은 Watermelon 라이브러리에 정의되어 있을 것으로 가정합니다.

            // 레벨 값이 -1이면 실제 체력 값을 텍스트로 표시합니다.
            if (level == -1)
            {
                healthText.text = targetHealth.CurrentHealth.ToString("F0"); // 소수점 없이 표시
            }

            // 항상 표시 설정이 아닌 경우에만 체력 변화에 따라 패널 표시 상태를 제어합니다.
            if (!showAlways)
            {
                // 체력이 최대 체력보다 작고 패널이 비활성화 상태이면 패널을 활성화합니다.
                if (healthFillImage.fillAmount < 1.0f && !isPanelActive)
                {
                    isPanelActive = true; // 패널 활성화 상태로 설정

                    // 기존 패널 트위닝 애니메이션이 있다면 중지합니다. (Watermelon 라이브러리 사용)
                    panelTweenCase.KillActive();

                    // 패널의 투명도를 0에서 1로 부드럽게 애니메이션하여 표시합니다.
                    panelTweenCase = healthBarCanvasGroup.DOFade(1.0f, 0.5f); // Watermelon 라이브러리 사용
                }
                // 체력이 최대 체력과 같거나 크고 패널이 활성화 상태이면 패널을 비활성화합니다.
                else if (healthFillImage.fillAmount >= 1.0f && isPanelActive)
                {
                    isPanelActive = false; // 패널 비활성화 상태로 설정

                    // 기존 패널 트위닝 애니메이션이 있다면 중지합니다. (Watermelon 라이브러리 사용)
                    panelTweenCase.KillActive();

                    // 패널의 투명도를 1에서 0으로 부드럽게 애니메이션하여 숨깁니다.
                    panelTweenCase = healthBarCanvasGroup.DOFade(0.0f, 0.5f); // Watermelon 라이브러리 사용
                }
            }
        }

        // 체력 바를 비활성화 상태로 만들고 숨기는 메소드입니다.
        // 페이드 아웃 애니메이션 후 게임 오브젝트를 비활성화합니다.
        public void DisableBar()
        {
            // 이미 비활성화 상태이면 아무것도 하지 않습니다.
            if (isDisabled)
                return;

            // 비활성화 상태로 설정합니다.
            isDisabled = true;

            // 패널의 투명도를 0으로 페이드 아웃하고, 완료 후 게임 오브젝트를 비활성화합니다. (Watermelon 라이브러리 사용)
            healthBarCanvasGroup.DOFade(0.0f, 0.3f).OnComplete(delegate
            {
                healthBarTransform.gameObject.SetActive(false);
            });
        }

        // 체력 바를 활성화 상태로 만들고 표시하는 메소드입니다.
        // 게임 오브젝트를 활성화하고 페이드 인 애니메이션으로 표시합니다.
        public void EnableBar()
        {
            // 이미 활성화 상태이면 아무것도 하지 않습니다.
            if (!isDisabled)
                return;

            // 비활성화 상태를 해제합니다.
            isDisabled = false;

            // 체력 바 게임 오브젝트를 활성화합니다.
            healthBarTransform.gameObject.SetActive(true);
            // 패널의 투명도를 1로 페이드 인하여 표시합니다. (Watermelon 라이브러리 사용)
            healthBarCanvasGroup.DOFade(1.0f, 0.3f);
        }

        // 현재 체력 상태에 맞춰 체력 바의 시각적 요소를 즉시 업데이트하는 메소드입니다.
        // fillAmount, 마스크 fillAmount, 텍스트, 보호막 이미지 표시 여부를 업데이트합니다.
        public void RedrawHealth()
        {
            // 현재 체력 비율에 따라 체력 채우기 이미지와 마스크 이미지의 fillAmount를 설정합니다.
            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;
            maskFillImage.fillAmount = healthFillImage.fillAmount;

            // 레벨 값이 -1이면 보호막 이미지를 숨기고 실제 체력 값을 텍스트로 표시합니다.
            if (level == -1)
            {
                shieldImage.gameObject.SetActive(false);
                healthText.text = targetHealth.CurrentHealth.ToString("F0"); // 소수점 없이 표시
            }
            // 레벨 값이 -1이 아니면 보호막 이미지를 표시하고 레벨 값을 텍스트로 표시합니다.
            else
            {
                shieldImage.gameObject.SetActive(true);
                healthText.text = level.ToString();
            }
        }

        // 체력 바를 강제로 비활성화하고 게임 오브젝트까지 완전히 숨기는 메소드입니다.
        public void ForceDisable()
        {
            // 비활성화 상태로 설정합니다.
            isDisabled = true;

            // 체력 바 게임 오브젝트와 CanvasGroup 게임 오브젝트를 비활성화하여 완전히 숨깁니다.
            healthBarTransform.gameObject.SetActive(false);
            healthBarCanvasGroup.gameObject.SetActive(false);
        }

        // 체력 바 게임 오브젝트를 파괴하는 메소드입니다.
        public void Destroy()
        {
            // 비활성화 상태로 설정합니다.
            isDisabled = true;

            // 체력 바 게임 오브젝트를 파괴합니다.
            MonoBehaviour.Destroy(healthBarTransform.gameObject);
        }
    }

    // 체력 정보를 제공하는 객체가 구현해야 하는 인터페이스입니다.
    // 현재 체력과 최대 체력 정보를 노출합니다.
    public interface IHealth
    {
        // 현재 체력 값을 가져옵니다.
        float CurrentHealth { get; }
        // 최대 체력 값을 가져옵니다.
        float MaxHealth { get; }
    }
}