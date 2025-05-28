// 스크립트 기능 요약:
// 이 스크립트는 게임 초기화 과정에서 화폐 시스템(CurrencyController)을 설정하는 역할을 하는 InitModule입니다.
// CurrencyDatabase ScriptableObject를 참조하여 CurrencyController에 전달하고,
// CurrencyController의 Init 함수를 호출하여 화폐 시스템을 초기화합니다.
// [RegisterModule] 속성을 통해 Watermelon Core의 초기화 시스템에 자동으로 등록됩니다.

using UnityEngine; // SerializeField 속성 사용을 위해 필요

namespace Watermelon
{
    // RegisterModule 속성을 통해 이 클래스가 Watermelon Core의 초기화 모듈임을 등록하고,
    // 에디터에서의 기본 활성화 상태를 설정합니다. ("Currencies", false)
    [RegisterModule("Currencies", false)]
    // CurrencyInitModule 클래스는 화폐 시스템 초기화 단계를 담당하는 InitModule입니다.
    public class CurrencyInitModule : InitModule
    {
        // ModuleName 속성은 이 초기화 모듈의 이름을 문자열로 반환합니다.
        // "Currencies"라는 고정된 이름을 사용합니다.
        public override string ModuleName => "Currencies";

        // currenciesDatabase: 화폐 시스템 초기화에 사용될 CurrencyDatabase ScriptableObject에 대한 참조입니다.
        // Inspector에서 할당됩니다.
        [SerializeField]
        [Tooltip("화폐 시스템 초기화에 사용될 CurrencyDatabase")]
        CurrencyDatabase currenciesDatabase;
        // Database 속성: currenciesDatabase 변수의 값을 읽기 전용으로 제공합니다.
        public CurrencyDatabase Database => currenciesDatabase;

        /// <summary>
        /// 초기화 시스템에 의해 호출되어 화폐 시스템 컴포넌트를 생성하고 초기화하는 함수입니다.
        /// CurrencyController.Init 함수를 호출하고 currenciesDatabase를 전달하여 실제 초기화를 수행합니다.
        /// </summary>
        public override void CreateComponent()
        {
            // CurrencyController의 Init 함수를 호출하여 화폐 시스템을 초기화합니다.
            // 이때 Inspector에서 할당된 CurrencyDatabase를 전달합니다.
            CurrencyController.Init(currenciesDatabase);
        }
    }
}