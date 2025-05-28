// TimerRewardsHolder.cs
// 이 스크립트는 일정 시간이 지난 후 무료 보상을 받을 수 있는 타이머 기반 보상 시스템을 구현합니다.
// 저장된 시간과 현재 시간을 비교하여 버튼 활성화/비활성화를 처리하고, 보상을 적용합니다.

using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.IAPStore
{
    public sealed class TimerRewardsHolder : RewardsHolder
    {
        private const string DEFAULT_BUTTON_TEXT = "FREE";

        [Header("설정")]
        [Tooltip("보상 타이머 저장에 사용할 고유 ID")]
        [SerializeField]
        [Group("Settings")]
        private string saveID = "uniqueTimerSaveID";

        [Header("설정"), Space]
        [Tooltip("보상을 받을 때 클릭할 버튼 참조")]
        [SerializeField]
        [Group("Settings")]
        private Button button;

        [Header("설정")]
        [Tooltip("남은 시간을 표시할 TMP_Text 컴포넌트 참조")]
        [SerializeField]
        [Group("Settings")]
        private TMP_Text timerText;

        [Header("설정")]
        [Tooltip("보상 재사용 대기 시간(분 단위)")]
        [SerializeField]
        [Group("Settings")]
        private int timerDurationInMinutes;

        // 보상 시 포맷된 시간을 저장하는 SimpleLongSave 객체
        private SimpleLongSave save;
        // 타이머 시작 시각
        private DateTime timerStartTime;

        // 문자열 빌딩을 위한 StringBuilder
        private StringBuilder sb;

        /// <summary>
        /// Awake: 보상 컴포넌트를 초기화하고 저장된 타이머 값을 불러온 후,
        /// 보상 비활성화 여부를 검사하며 버튼 클릭 리스너를 설정합니다.
        /// </summary>
        private void Awake()
        {
            // RewardsHolder에서 Awake/Start 시 호출되는 초기화 로직
            InitializeComponents();

            // 저장된 시간을 불러옵니다.
            save = SaveController.GetSaveObject<SimpleLongSave>($"TimerProduct_{saveID}");
            timerStartTime = DateTime.FromBinary(save.Value);

            // 비활성화 조건을 가진 보상이 있다면 이 게임오브젝트를 비활성화합니다.
            for (int i = 0; i < rewards.Length; i++)
            {
                if (rewards[i].CheckDisableState())
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            sb = new StringBuilder();

            // 버튼 클릭 시 OnButtonClicked 호출
            button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// FormatTimer: TimeSpan을 "HH:MM:SS" 또는 "MM:SS" 형식의 문자열로 변환합니다.
        /// </summary>
        /// <param name="timeSpan">포맷할 TimeSpan</param>
        /// <returns>포맷된 시간 문자열</returns>
        private string FormatTimer(TimeSpan timeSpan)
        {
            sb.Clear();

            if (timeSpan.Hours > 0)
            {
                sb.Append(timeSpan.Hours);
                sb.Append(':');
            }

            sb.Append(timeSpan.Minutes.ToString("00"));
            sb.Append(':');
            sb.Append(timeSpan.Seconds.ToString("00"));

            return sb.ToString();
        }

        /// <summary>
        /// Update: 매 프레임 현재 시간과 시작 시간을 비교하여
        /// 버튼 활성화 여부와 타이머 텍스트를 갱신합니다.
        /// </summary>
        private void Update()
        {
            TimeSpan elapsed = DateTime.Now - timerStartTime;
            TimeSpan duration = TimeSpan.FromMinutes(timerDurationInMinutes);

            if (elapsed > duration)
            {
                // 대기 시간이 지나면 버튼 활성화 및 텍스트 기본 표시
                button.enabled = true;
                timerText.text = DEFAULT_BUTTON_TEXT;
            }
            else
            {
                // 대기 시간 이전에는 버튼 비활성화 및 남은 시간 표시
                button.enabled = false;
                timerText.text = FormatTimer(duration - elapsed);

                // 텍스트 폭에 따라 버튼 및 텍스트 Rect 크기 조정
                float preferredWidth = timerText.preferredWidth;
                if (preferredWidth < 270f) preferredWidth = 270f;

                timerText.rectTransform.sizeDelta =
                    timerText.rectTransform.sizeDelta.SetX(preferredWidth + 5f);
                button.image.rectTransform.sizeDelta =
                    button.image.rectTransform.sizeDelta.SetX(preferredWidth + 10f);
            }
        }

        /// <summary>
        /// IsAvailable: 보상이 사용 가능 상태인지 반환합니다.
        /// </summary>
        /// <returns>타이머가 만료되었으면 true, 그렇지 않으면 false</returns>
        public bool IsAvailable()
        {
            TimeSpan elapsed = DateTime.Now - timerStartTime;
            TimeSpan duration = TimeSpan.FromMinutes(timerDurationInMinutes);
            return elapsed > duration;
        }

        /// <summary>
        /// OnButtonClicked: 버튼 클릭 시 타이머를 초기화하고 보상을 적용하며 저장합니다.
        /// </summary>
        private void OnButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            // 버튼 클릭 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // 타이머 시작 시간을 현재 시간으로 갱신 후 보상 적용
            save.Value = DateTime.Now.ToBinary();
            timerStartTime = DateTime.Now;

            ApplyRewards();

            // 변경된 저장 정보 마크
            SaveController.MarkAsSaveIsRequired();
        }
    }
}
