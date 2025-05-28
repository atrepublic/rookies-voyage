/*
📄 UILevelNumberText.cs 요약
현재 게임 레벨 정보를 UI 텍스트로 표시하고, 등장/퇴장 애니메이션을 제어하는 컴포넌트야.

"LEVEL X" 형태의 텍스트를 표시하며, UIScaleAnimation을 사용한 크기 확대/축소 효과로 등장과 퇴장을 연출해.

Show()와 Hide() 메서드를 통해 레벨 숫자 텍스트의 표시 여부를 제어할 수 있음.
*/

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Text))]
    public class UILevelNumberText : MonoBehaviour
    {
        private const string LEVEL_LABEL = "LEVEL {0}";
        private static UILevelNumberText instance;

        [SerializeField] UIScaleAnimation uIScalableObject;

        private static UIScaleAnimation UIScalableObject => instance.uIScalableObject;
        private static Text levelNumberText;

        private static bool IsDisplayed = false;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<Text>();
        }

        private void Start()
        {
            UpdateLevelNumber();
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        public static void Show(bool immediately = true)
        {
            if (IsDisplayed)
                return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(scaleMultiplier: 1.05f, immediately: immediately);
        }

        public static void Hide(bool immediately = true)
        {
            if (!IsDisplayed)
                return;

            if (immediately)
                IsDisplayed = false;

            UIScalableObject.Hide(scaleMultiplier: 1.05f, immediately: immediately, onCompleted: delegate
           {

               IsDisplayed = false;
               levelNumberText.enabled = false;
           });
        }

        private void UpdateLevelNumber()
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, "X");
        }

    }
}
