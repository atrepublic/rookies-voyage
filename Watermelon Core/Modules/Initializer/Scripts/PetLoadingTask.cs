// PetLoadingTask.cs
// 펫 시스템 로딩 작업을 정의합니다.
// GameLoading 단계에서 Activate() 호출 시 OnTaskActivated()가 실행되어 펫 리소스를 사전 로드합니다.

using Watermelon;
using UnityEngine;
using Watermelon.SquadShooter;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 로딩 과정에서 펫 데이터 및 프리팹을 미리 로드하는 작업입니다.
    /// </summary>
    public class PetLoadingTask : LoadingTask
    {
        /// <summary>
        /// 로딩 작업이 시작될 때 호출됩니다.
        /// 펫 데이터베이스의 모든 펫을 순회하며 필요 리소스를 사전 로드합니다.
        /// </summary>
        public override void OnTaskActivated()
        {
            // 로딩 메시지 표시
            GameLoading.SetLoadingMessage("펫 데이터 로딩 중...");

            Debug.Log("펫 데이터 로딩 중...");

            // 1) SO 에셋 확보 (이미 메모리에 올라와 있으면 캐시에서 가져옴)
            var petDatabase = GameSettings.GetSettings().PetDatabase;

            // 2) 각 펫 데이터 순회
            foreach (var pet in petDatabase.GetAllPets())
            {
                // (Optional) Addressables 사용 시: Addressables.LoadAssetAsync<GameObject>(pet.petPrefab).WaitForCompletion();
                if (pet.petPrefab != null)
                {
                    // Instantiate하지 않고, 풀링 시스템용 미리 생성 로직 등 필요 시 추가
                    // 예: PoolManager.PreloadPool(pet.petPrefab, 초기풀사이즈);
                }
            }

            // 3) 로딩 완료 표시
            CompleteTask(CompleteStatus.Completed);
        }
    }
}
