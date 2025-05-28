// PetManager.cs (업그레이드 연동 통합 v1.10)
/*****************************************************************************************
 *  📌 펫 관리: 레벨/룸 및 메뉴 진입 시 선택된 펫 스폰 및 위치 갱신
 *────────────────────────────────────────────────────────────────────────────────────────
 *  • 언락된 펫만 스폰하며, 선택된 펫의 SpawnPoint + 오프셋 위치에 정확히 배치합니다.
 *  • 메뉴 씬에서도 SpawnSelectedPet() 호출로 펫을 즉시 스폰할 수 있습니다.
 *  • 펫이 이미 존재하면 NavMeshAgent.Warp로 위치만 갱신합니다.
 *  • [추가] 펫 업그레이드 레벨 정보 로딩 및 SetData 호출 연동
 *****************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;
using UnityEngine.AI;

[DefaultExecutionOrder(-20)]
public class PetManager : MonoBehaviour
{
    public static PetManager Instance { get; private set; }

    [Header("펫 생성/이동 설정")]
    [SerializeField] private Transform petContainer;
    [SerializeField] private float spawnOffsetX = 2f;
    [SerializeField] private float spawnOffsetZ = 2f;

    private PetController currentPet;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (petContainer != null && petContainer.parent != null)
            petContainer.SetParent(null);

        var cb = CharacterBehaviour.GetBehaviour();
        if (cb != null)
            playerTransform = cb.transform;
        else
            Debug.LogWarning("PetManager: 플레이어 CharacterBehaviour를 찾을 수 없습니다.");

        LevelController.OnLevelStartedEvent += OnRoomOrLevelStarted;
        LevelController.OnRoomStartedEvent  += OnRoomOrLevelStarted;
    }

    private void OnDestroy()
    {
        LevelController.OnLevelStartedEvent -= OnRoomOrLevelStarted;
        LevelController.OnRoomStartedEvent  -= OnRoomOrLevelStarted;
    }

    public void SpawnSelectedPet()
    {
        var pedestal = Object.FindFirstObjectByType<PedestalBehavior>();
        if (pedestal != null)
        {
            if (currentPet != null)
            {
                Destroy(currentPet.gameObject);
                currentPet = null;
            }
            if (petContainer != null)
            {
                foreach (Transform child in petContainer)
                    Destroy(child.gameObject);
            }
            int selectedID = SaveController.GetSaveObject<UC_PetGlobalSave>("pet_global").SelectedPetID;
            pedestal.ShowPreviewPet(selectedID);
            return;
        }

        OnRoomOrLevelStarted();
    }

    private void OnRoomOrLevelStarted()
    {
        if (SceneManager.GetActiveScene().name != "Game")
            return;

        if (playerTransform == null)
        {
            var cb = CharacterBehaviour.GetBehaviour();
            if (cb != null)
                playerTransform = cb.transform;
        }

        var petSave = SaveController.GetSaveObject<UC_PetSave>("pet");
        var globalSave = SaveController.GetSaveObject<UC_PetGlobalSave>("pet_global");
        int selectedID = globalSave.SelectedPetID;
        if (!petSave.HasPet(selectedID))
            return;

        var rooms = LevelController.CurrentLevelData.Rooms;
        int idx = LevelController.CurrentRoomIndex;
        Vector3 basePos = rooms[idx].SpawnPoint;
        Vector3 targetPos = new Vector3(
            basePos.x + spawnOffsetX,
            basePos.y,
            basePos.z + spawnOffsetZ
        );

        if (currentPet != null)
        {
            var agent = currentPet.Agent;
            if (agent != null && agent.isOnNavMesh)
                agent.Warp(targetPos);

            currentPet.transform.position = targetPos;
            return;
        }

        var petData = GameSettings.GetSettings().PetDatabase.GetPetDataByID(selectedID);
        if (petData == null || petData.petPrefab == null)
        {
            Debug.LogWarning($"PetManager: 펫 Prefab이 없습니다 (ID={selectedID})");
            return;
        }

        var petObj = Instantiate(petData.petPrefab, targetPos, Quaternion.identity, petContainer);
        petObj.name = $"Pet_{petData.petName}";

        currentPet = petObj.GetComponent<PetController>();
        if (currentPet != null)
        {
            // [추가] 업그레이드 정보 반영
            // 업그레이드 레벨이 -1일 경우 초기화
        int upgradeLevel = petSave.GetLevel(selectedID);
        if (upgradeLevel < 0)
        {
            petSave.SetLevel(selectedID, 0);
            upgradeLevel = 0;
        }
            currentPet.SetData(petData, upgradeLevel);
            currentPet.Init(playerTransform);
        }
        else
        {
            Debug.LogWarning("PetManager: PetController 컴포넌트를 찾을 수 없습니다.");
        }
    }

    public PetController GetCurrentPet() => currentPet;
}
