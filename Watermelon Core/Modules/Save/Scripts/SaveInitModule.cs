// SaveInitModule.cs
// 이 스크립트는 게임 초기화 시스템(Initializer)에서 SaveController를 초기화하는 모듈입니다.
// InitModule을 상속받아 초기화 프로세스에 통합되며,
// 자동 저장 간격 및 클린 저장 시작 여부와 같은 SaveController 초기화 설정을 정의합니다.

using UnityEngine;

namespace Watermelon
{
    // InitModule로 등록하며 모듈 이름, 코어 모듈 여부, 초기화 순서를 설정합니다.
    [RegisterModule("Save Controller", core: true, order: 900)]
    public class SaveInitModule : InitModule
    {
        // 이 초기화 모듈의 이름입니다. "Save Controller"를 반환합니다.
        public override string ModuleName => "Save Controller";

        // 자동 저장 간격을 설정합니다. 0이면 자동 저장 비활성화.
        [Tooltip("자동 저장 간격 (초) 입니다. 0보다 크면 자동 저장이 활성화됩니다.")]
        [SerializeField] float autoSaveDelay = 0;
        // 게임 시작 시 기존 저장 데이터를 무시하고 새 저장 데이터를 생성할지 여부입니다.
        [Tooltip("게임 시작 시 기존 저장 데이터를 무시하고 새 저장 데이터를 생성할지 여부입니다.")]
        [SerializeField] bool cleanSaveStart = false;

        /// <summary>
        /// SaveInitModule과 관련된 컴포넌트(여기서는 SaveController)를 초기화하는 함수입니다.
        /// Initializer에 의해 게임 초기화 과정 중에 호출됩니다.
        /// </summary>
        public override void CreateComponent()
        {
            // autoSaveDelay 및 cleanSaveStart 설정 값을 사용하여 SaveController를 초기화합니다.
            SaveController.Init(autoSaveDelay, cleanSaveStart);
        }
    }
}