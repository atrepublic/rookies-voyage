// TweenInitModule.cs v1.02
// 이 모듈은 Tween 시스템을 초기화합니다.
// InitModule을 상속받아 커스텀 이징 함수, 트윈 개수 설정, 로깅 옵션을 받아
// 게임 시작 시 Tween 컴포넌트를 추가 및 초기화합니다.

#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Tween", core: true, order: 800)]
    public class TweenInitModule : InitModule
    {
        /// <summary>
        /// 모듈의 고유 이름을 반환합니다.
        /// </summary>
        public override string ModuleName => "Tween";

        /// <summary>
        /// 커스텀 이징 함수 배열: Ease.Init() 호출 시 사용됩니다.
        /// </summary>
        [SerializeField]
        [Tooltip("커스텀 이징 함수를 지정합니다.")]
        private CustomEasingFunction[] customEasingFunctions;

        [Space]
        /// <summary>
        /// Update 루프에서 처리할 Tween 최대 개수입니다.
        /// </summary>
        [SerializeField]
        [Tooltip("Update에서 처리할 Tween 최대 개수")]
        private int tweensUpdateCount = 300;

        /// <summary>
        /// FixedUpdate 루프에서 처리할 Tween 최대 개수입니다.
        /// </summary>
        [SerializeField]
        [Tooltip("FixedUpdate에서 처리할 Tween 최대 개수")]
        private int tweensFixedUpdateCount = 30;

        /// <summary>
        /// LateUpdate 루프에서 처리할 Tween 최대 개수입니다.
        /// </summary>
        [SerializeField]
        [Tooltip("LateUpdate에서 처리할 Tween 최대 개수")]
        private int tweensLateUpdateCount = 0;

        [Space]
        /// <summary>
        /// 초기화 과정에서 상세 로그 출력을 활성화할지 여부입니다.
        /// </summary>
        [SerializeField]
        [Tooltip("초기화 시 상세 로그 출력 여부")]
        private bool verboseLogging;

        /// <summary>
        /// 모듈 초기화 시 호출됩니다.
        /// Tween 컴포넌트를 게임 오브젝트에 추가하고, 지정된 설정으로 초기화합니다.
        /// Ease.Init()을 통해 커스텀 이징 함수도 등록합니다.
        /// </summary>
        public override void CreateComponent()
        {
            Tween tween = Initializer.GameObject.AddComponent<Tween>();
            tween.Init(tweensUpdateCount, tweensFixedUpdateCount, tweensLateUpdateCount, verboseLogging);

            Ease.Init(customEasingFunctions);
        }
    }
}
