// BalanceDebugText.cs  v1.01
/*****************************************************************************************
 *  스크립트 기능 및 용도
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • BalanceController의 BalanceUpdated 이벤트를 수신하여 현재 난이도·업그레이드 정보를
 *    화면에 텍스트 형태로 실시간 표시합니다. (디버그용 오버레이)
 *  • Create() 정적 메서드는 전용 Canvas와 TextMeshProUGUI 컴포넌트를 동적으로 생성하여
 *    씬 어디서든 쉽게 디버그 텍스트를 띄울 수 있도록 합니다.
 *  • 최종 표시 포맷:  
 *      lvl: 1-3   (월드-레벨)  
 *      pwr: 12/18 (현재 업그레이드 파워 / 요구 파워)  
 *      upg: -1    (업그레이드 차이)  
 *      dif: Hard  (난이도 프리셋 Note)
 *****************************************************************************************/


using TMPro;
using UnityEngine;

namespace Watermelon.SquadShooter
{
        /// <summary>
    ///  난이도 디버그 텍스트 출력 컴포넌트<br/>
    ///  ────────────────────────────────────────────────<br/>
    ///  • BalanceController.ShowDebugText 옵션이 켜져 있을 때 자동 생성됩니다.<br/>
    ///  • TextMeshProUGUI 컴포넌트를 통해 지정 좌표(기본 좌측 상단)에 정보를 표시합니다.
    /// </summary>
    /// 
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BalanceDebugText : MonoBehaviour
    {
        private TextMeshProUGUI difficultyText;

        private LevelSave levelSave;

        private void OnEnable()
        {
            BalanceController.BalanceUpdated += OnBalanceUpdated;
        }

        private void OnDisable()
        {
            BalanceController.BalanceUpdated -= OnBalanceUpdated;
        }

        private void Awake()
        {
            difficultyText = GetComponent<TextMeshProUGUI>();

            levelSave = SaveController.GetSaveObject<LevelSave>("level");
        }

        /// <summary>
        ///  TextMeshProUGUI 내용 업데이트
        /// </summary>
        private void UpdateText()
        {
            if (difficultyText != null)
            {
                difficultyText.SetText("lvl: " + (levelSave.WorldIndex + 1) + "-" + (levelSave.LevelIndex + 1)
                    + "\npwr: " + BalanceController.CurrentGeneralPower + "/" + BalanceController.PowerRequirement
                    + "\nupg: " + BalanceController.UpgradesDifference
                    + "\ndif: " + BalanceController.CurrentDifficulty.Note);
            }
        }

        private void OnBalanceUpdated(bool highlight)
        {
            UpdateText();
        }

        /// <summary>
        ///  디버그 텍스트를 표시할 Canvas·TMP 오브젝트를 동적으로 생성합니다.
        /// </summary>

        public static BalanceDebugText Create()
        {

            // 1) 최상위 Canvas 생성
            GameObject canvasObject = new GameObject("[Balance Canvas]");
            canvasObject.transform.SetParent(UIController.MainCanvas.transform);
            canvasObject.transform.ResetLocal();

            RectTransform rectTransform = canvasObject.GetOrSetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 99;

             // 2) 텍스트 오브젝트 생성
            GameObject devTextObject = new GameObject("[Dev Text]");
            devTextObject.transform.SetParent(canvasObject.transform);
            devTextObject.transform.ResetLocal();

            RectTransform devRectTransform = devTextObject.AddComponent<RectTransform>();
            devRectTransform.anchorMin = new Vector2(0, 1);
            devRectTransform.anchorMax = new Vector2(0, 1);
            devRectTransform.pivot = new Vector2(0, 1);

            devRectTransform.sizeDelta = new Vector2(300, 145);
            devRectTransform.anchoredPosition = new Vector2(35, -325);

            TextMeshProUGUI text = devTextObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 28;

            // 3) BalanceDebugText 컴포넌트 부착 및 초기 표시
            BalanceDebugText balanceDebugText = devTextObject.AddComponent<BalanceDebugText>();
            balanceDebugText.UpdateText();

            return balanceDebugText;
        }
    }
}