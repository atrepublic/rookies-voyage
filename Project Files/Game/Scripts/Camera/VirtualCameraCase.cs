// 스크립트 설명: 특정 카메라 타입에 대한 Cinemachine 가상 카메라 설정을 담는 클래스입니다.
// 연결된 가상 카메라 참조와 카메라 흔들림(Shake) 기능 등을 포함합니다.
using Unity.Cinemachine; // Cinemachine 관련 네임스페이스
using UnityEngine;
using Watermelon; // Tween 관련 네임스페이스

namespace Watermelon
{
    // Unity 에디터에서 인스펙터 창에 표시될 수 있도록 직렬화 가능하게 설정
    [System.Serializable]
    public class VirtualCameraCase
    {
        [SerializeField]
        [Tooltip("이 설정이 적용될 카메라 타입")] // 주요 변수 한글 툴팁
        CameraType cameraType; // 카메라 타입
        // 카메라 타입에 접근하기 위한 프로퍼티
        public CameraType CameraType => cameraType;

        [SerializeField]
        [Tooltip("이 설정에 연결된 Cinemachine 가상 카메라 컴포넌트")] // 주요 변수 한글 툴팁
        CinemachineCamera virtualCamera; // Cinemachine 가상 카메라
        // Cinemachine 가상 카메라에 접근하기 위한 프로퍼티
        public CinemachineCamera VirtualCamera => virtualCamera;

        // 카메라 흔들림 효과를 위한 CinemachineBasicMultiChannelPerlin 컴포넌트 참조
        private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
        // 외부에서 CinemachineBasicMultiChannelPerlin에 접근하기 위한 프로퍼티
        public CinemachineBasicMultiChannelPerlin CinemachineBasicMultiChannelPerlin => cinemachineBasicMultiChannelPerlin;

        private TweenCase shakeTweenCase; // 카메라 흔들림 애니메이션 트윈 케이스

        /// <summary>
        /// VirtualCameraCase 설정을 초기화합니다.
        /// 연결된 가상 카메라에서 CinemachineBasicMultiChannelPerlin 컴포넌트를 가져옵니다.
        /// </summary>
        public void Init()
        {
            // 가상 카메라에서 CinemachineBasicMultiChannelPerlin 컴포넌트 가져오기
            cinemachineBasicMultiChannelPerlin = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        /// <summary>
        /// 지정된 시간과 강도로 카메라 흔들림 효과를 적용합니다.
        /// 페이드 인, 지속 시간, 페이드 아웃 설정이 가능합니다.
        /// </summary>
        /// <param name="fadeInTime">흔들림 강도가 최대로 올라가는 시간.</param>
        /// <param name="fadeOutTime">흔들림 강도가 다시 0으로 줄어드는 시간.</param>
        /// <param name="duration">최대 강도로 흔들림을 유지하는 시간.</param>
        /// <param name="gain">흔들림의 최대 강도.</param>
        public void Shake(float fadeInTime, float fadeOutTime, float duration, float gain)
        {
            shakeTweenCase.KillActive(); // 기존 흔들림 트윈 중지

            gain *= 2; // 흔들림 강도 조정 (원래 코드에 있던 로직 유지)

            // 흔들림 강도를 0에서 최대 강도까지 페이드 인하는 트윈 시작 (Tween에 정의된 것으로 가정)
            shakeTweenCase = Tween.DoFloat(0.0f, gain, fadeInTime, (float fadeInValue) =>
            {
                // CinemachineBasicMultiChannelPerlin 컴포넌트의 AmplitudeGain을 업데이트하여 흔들림 강도 조절
                if(cinemachineBasicMultiChannelPerlin != null) // 컴포넌트 null 체크 추가
                {
                     cinemachineBasicMultiChannelPerlin.AmplitudeGain = fadeInValue;
                }

            }).OnComplete(() => // 페이드 인 완료 시 실행될 콜백
            {
                // 최대 강도로 흔들림을 유지하는 시간만큼 지연 호출 (Tween에 정의된 것으로 가정)
                shakeTweenCase = Tween.DelayedCall(duration, () =>
                {
                    // 흔들림 강도를 최대에서 0까지 페이드 아웃하는 트윈 시작 (Tween에 정의된 것으로 가정)
                    shakeTweenCase = Tween.DoFloat(gain, 0.0f, fadeOutTime, (float fadeOutValue) =>
                    {
                         // CinemachineBasicMultiChannelPerlin 컴포넌트의 AmplitudeGain을 업데이트하여 흔들림 강도 조절
                         if(cinemachineBasicMultiChannelPerlin != null) // 컴포넌트 null 체크 추가
                         {
                             cinemachineBasicMultiChannelPerlin.AmplitudeGain = fadeOutValue;
                         }
                    });
                });
            });
        }
    }
}