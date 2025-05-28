// PedestalBehavior.cs v1.01
/*****************************************************************************************
 *  📌 발판(Pedestal) 확장: 펫 미리보기 기능 추가
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • petPreviewSpawnPoint 위치에 선택된 펫만 보여줍니다.
 *  • 이전 프리뷰는 제거하며, AI 및 이동 로직은 비활성화합니다.
 *****************************************************************************************/

using UnityEngine;
using Watermelon;                 // SaveController
using Watermelon.SquadShooter;    // UC_PetDatabase, UC_PetGlobalSave, UC_PetSave
using Watermelon.LevelSystem;       // GameSettings
using UnityEngine.AI;               // NavMeshAgent
// UC_PetDatabase, UC_PetGlobalSave, UC_PetSave

namespace Watermelon.SquadShooter
{
    public class PedestalBehavior : MonoBehaviour
    {
        [Tooltip("이 발판에 표시될 CharacterBehaviour 컴포넌트 참조입니다.")]
        [SerializeField] private CharacterBehaviour characterBehaviour;

        [Header("펫 미리보기 설정")]
        [Tooltip("펫 프리뷰를 생성할 위치(Transform)")]
        [SerializeField] private Transform petPreviewSpawnPoint;

        // 현재 띄워진 프리뷰 객체
        private GameObject currentPreviewPet;

        /// <summary>
        /// 발판 초기화: 캐릭터와 무기 세팅 후, 현재 선택된 펫 프리뷰 갱신
        /// </summary>
        public void Init()
        {
            // 기존 캐릭터/무기 초기화 로직
            CharacterData character = CharactersController.SelectedCharacter;
            CharacterStageData characterStage = character.GetCurrentStage();
            CharacterUpgrade characterUpgrade = character.GetCurrentUpgrade();
            characterBehaviour.SetStats(characterUpgrade.Stats);
            characterBehaviour.Init();
            characterBehaviour.DisableAgent();
            characterBehaviour.SetGraphics(characterStage.Prefab, false, false);
            WeaponData weapon = WeaponsController.GetCurrentWeapon();
            characterBehaviour.SetGun(weapon, weapon.GetCurrentUpgrade());

            // 펫 프리뷰 갱신
            ShowPreviewPet(SaveController.GetSaveObject<UC_PetGlobalSave>("pet_global").SelectedPetID);
        }

        /// <summary>
        /// 선택된 펫 ID로 프리뷰를 생성합니다.
        /// 이전 프리뷰는 제거하고, 미리보기용으로 AI/Agent를 비활성화합니다.
        /// </summary>
        /// <param name="petID">언락된 펫 ID</param>
        public void ShowPreviewPet(int petID)
        {
            // 이전 프리뷰 제거
            if (currentPreviewPet != null)
            {
                Destroy(currentPreviewPet);
                currentPreviewPet = null;
            }

            // 저장된 언락 상태 확인
            var petSave = SaveController.GetSaveObject<UC_PetSave>("pet");
            if (!petSave.HasPet(petID))
                return;

            // 펫 데이터 조회
            var petData = GameSettings.GetSettings().PetDatabase.GetPetDataByID(petID);
            if (petData == null || petData.petPrefab == null)
                return;

            // 프리뷰 생성
            currentPreviewPet = Instantiate(petData.petPrefab, petPreviewSpawnPoint.position, Quaternion.identity, transform);
            currentPreviewPet.name = "PreviewPet_" + petData.petName;

            // 미리보기용 컴포넌트 비활성화
            var agent = currentPreviewPet.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            var petCtrl = currentPreviewPet.GetComponent<PetController>();
            if (petCtrl != null) petCtrl.enabled = false;
        }
    }
}
