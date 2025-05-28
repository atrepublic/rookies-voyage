#pragma warning disable 649

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    ///   설정 사운드 토글 버튼 스크립트.
    ///   사운드 활성화/비활성화 기능을 처리합니다.
    /// </summary>
    public class SettingsSoundToggleButton : SettingsButtonBase
    {
        /// <summary>
        ///   전체 오디오 타입에 적용할지 여부.
        ///   true이면 모든 오디오 타입에, false이면 특정 오디오 타입에 적용됩니다.
        /// </summary>
        [Tooltip("전체 오디오 타입에 적용할지 여부")]
        [SerializeField] bool universal;

        /// <summary>
        ///   적용할 특정 오디오 타입. universal이 false일 때만 사용됩니다.
        /// </summary>
        [Tooltip("적용할 특정 오디오 타입")]
        [HideIf("universal")]
        [SerializeField] AudioType type;

        /// <summary>
        ///   이미지 참조.
        ///   사운드 상태에 따라 스프라이트를 변경합니다.
        /// </summary>
        [Tooltip("사운드 상태에 따라 스프라이트를 변경할 이미지")]
        [SerializeField] Image imageRef;

        /// <summary>
        ///   선택 효과 이미지.
        ///   버튼 선택 시 페이드 효과를 표시합니다.
        /// </summary>
        [Tooltip("버튼 선택 시 페이드 효과를 표시할 이미지")]
        [SerializeField] Image selectionImage;

        /// <summary>
        ///   활성화 상태 스프라이트.
        /// </summary>
        [Tooltip("활성화 상태 스프라이트")]
        [SerializeField] Sprite activeSprite;

        /// <summary>
        ///   비활성화 상태 스프라이트.
        /// </summary>
        [Tooltip("비활성화 상태 스프라이트")]
        [SerializeField] Sprite disableSprite;

        /// <summary>
        ///   현재 활성화 상태.
        /// </summary>
        private bool isActive = true;

        /// <summary>
        ///   사용 가능한 오디오 타입 배열. universal이 true일 때 사용됩니다.
        /// </summary>
        private AudioType[] availableAudioTypes;

        /// <summary>
        ///   선택 효과 페이드 트윈.
        /// </summary>
        private TweenCase selectionFadeCase;

        /// <summary>
        ///   초기화 함수.
        ///   universal이 true이면 사용 가능한 모든 오디오 타입을 배열에 저장합니다.
        /// </summary>
        public override void Init()
        {
            if (universal)
            {
                availableAudioTypes = EnumUtils.GetEnumArray<AudioType>();
            }
        }

        /// <summary>
        ///   컴포넌트 활성화 시 호출되는 함수.
        ///   현재 상태를 가져와서 UI를 그리고, AudioController의 VolumeChanged 이벤트에 콜백을 등록합니다.
        /// </summary>
        private void OnEnable()
        {
            isActive = GetState();

            Redraw();

            AudioController.VolumeChanged += OnVolumeChanged;
        }

        /// <summary>
        ///   컴포넌트 비활성화 시 호출되는 함수.
        ///   AudioController의 VolumeChanged 이벤트에서 콜백을 제거합니다.
        /// </summary>
        private void OnDisable()
        {
            AudioController.VolumeChanged -= OnVolumeChanged;
        }

        /// <summary>
        ///   UI를 다시 그리는 함수.
        ///   활성화 상태에 따라 스프라이트를 변경합니다.
        /// </summary>
        private void Redraw()
        {
            imageRef.sprite = isActive ? activeSprite : disableSprite;
        }

        /// <summary>
        ///   버튼 클릭 시 호출되는 함수.
        ///   활성화 상태를 토글하고, 상태를 설정하고, 버튼 클릭 사운드를 재생합니다.
        /// </summary>
        public override void OnClick()
        {
            isActive = !isActive;

            SetState(isActive);

            // 버튼 사운드 재생
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        /// <summary>
        ///   VolumeChanged 이벤트 콜백 함수.
        ///   universal이 true이거나 이벤트의 audioType이 현재 타입과 같으면 UI를 다시 그립니다.
        /// </summary>
        /// <param name="audioType">변경된 오디오 타입</param>
        /// <param name="volume">변경 후 볼륨</param>
        private void OnVolumeChanged(AudioType audioType, float volume)
        {
            if (universal || audioType == type)
            {
                isActive = GetState();

                Redraw();
            }
        }

        /// <summary>
        ///   현재 상태를 가져오는 함수.
        ///   universal이 true이면 모든 오디오 타입이 활성화되어 있는지 확인하고,
        ///   false이면 특정 오디오 타입이 활성화되어 있는지 확인합니다.
        /// </summary>
        /// <returns>현재 활성화 상태 (true: 활성화, false: 비활성화)</returns>
        private bool GetState()
        {
            if (universal)
            {
                foreach (AudioType audioType in availableAudioTypes)
                {
                    if (!AudioController.IsAudioTypeActive(audioType))
                        return false;
                }

                return true;
            }

            return AudioController.IsAudioTypeActive(type);
        }

        /// <summary>
        ///   상태를 설정하는 함수.
        ///   universal이 true이면 모든 오디오 타입의 볼륨을 설정하고,
        ///   false이면 특정 오디오 타입의 볼륨을 설정합니다.
        /// </summary>
        /// <param name="state">설정할 상태 (true: 활성화, false: 비활성화)</param>
        private void SetState(bool state)
        {
            float volume = state ? 1.0f : 0.0f;

            if (universal)
            {
                foreach (AudioType audioType in availableAudioTypes)
                {
                    AudioController.SetVolume(audioType, volume);
                }

                return;
            }

            AudioController.SetVolume(type, volume);
        }

        /// <summary>
        ///   버튼 선택 시 호출되는 함수.
        ///   선택 효과를 표시합니다.
        /// </summary>
        public override void Select()
        {
            IsSelected = true;

            selectionFadeCase.KillActive();

            selectionImage.gameObject.SetActive(true);
            selectionImage.color = selectionImage.color.SetAlpha(0.0f);
            selectionFadeCase = selectionImage.DOFade(0.2f, 0.2f);
        }

        /// <summary>
        ///   버튼 선택 해제 시 호출되는 함수.
        ///   선택 효과를 숨깁니다.
        /// </summary>
        public override void Deselect()
        {
            IsSelected = false;

            selectionFadeCase.KillActive();

            selectionImage.gameObject.SetActive(false);
            selectionImage.color = selectionImage.color.SetAlpha(0.0f);
        }
    }
}