// UC_PetInitModule.cs
// 펫 시스템 초기화 모듈을 정의합니다.
// 게임 초기화 과정 중 PetDatabase 초기화 및 PetLoadingTask 등록을 담당합니다.

using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// InitModule로 등록되어 게임 시작 시 실행되는 펫 시스템 초기화 모듈입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Squad Shooter/Init Module/Pet System Init", fileName = "PetInitModule")]
    [RegisterModule("Pet System", core: false, order: 1000)]
    public class PetInitModule : InitModule
    {
        /// <summary>
        /// 이 모듈의 이름을 반환합니다.
        /// </summary>
        public override string ModuleName => "Pet System Init";

        /// <summary>
        /// 초기화 과정 중 PetDatabase를 초기화하고 PetLoadingTask를 등록합니다.
        /// </summary>
        public override void CreateComponent()
        {
            // 1) Pet 데이터베이스 초기화
            GameSettings.GetSettings().PetDatabase.Init();

            // 2) 로딩 태스크에 펫 로딩 작업 추가
            GameLoading.AddTask(new PetLoadingTask());

            //Debug.Log(ModuleName);
        }
    }
}
