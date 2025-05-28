// ==============================================
// CharacterData.cs
// ==============================================
// 각 캐릭터의 이름, 레벨 조건, 스테이지별 프리팹, 업그레이드 정보 등을 포함한
// ScriptableObject 기반 캐릭터 데이터 클래스입니다.
// 캐릭터의 현재 상태(업그레이드, 해금 여부 등)를 판별하고 관련 정보를 반환합니다.

using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "Data/Character System/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("캐릭터 고유 ID")]
        [UniqueID]
        [SerializeField] private string id;
        public string ID => id;

        [Tooltip("캐릭터 이름")]
        [SerializeField] private string characterName;
        public string CharacterName => characterName;

        [Tooltip("해금에 필요한 경험치 레벨")]
        [SerializeField] private int requiredLevel;
        public int RequiredLevel => requiredLevel;

        [Header("프리뷰 및 락 이미지")]
        [Tooltip("캐릭터 프리뷰 스프라이트")]
        [SerializeField] private Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [Tooltip("잠금 상태일 때 보여줄 스프라이트")]
        [SerializeField] private Sprite lockedSprite;
        public Sprite LockedSprite => lockedSprite;

        [Header("관련 프리팹")]
        [Tooltip("드랍 시 표시할 프리팹")]
        [SerializeField] private GameObject dropPrefab;
        public GameObject DropPrefab => dropPrefab;

        [Header("스테이지 / 업그레이드 정보")]
        [Tooltip("캐릭터 스테이지 데이터 목록")]
        [SerializeField] private CharacterStageData[] stages;
        public CharacterStageData[] Stages => stages;

        [Tooltip("업그레이드 단계 데이터")]
        [SerializeField] private CharacterUpgrade[] upgrades;
        public CharacterUpgrade[] Upgrades => upgrades;

        private CharacterSave save;
        public CharacterSave Save => save;

        // 초기화: 저장 데이터 로드 및 에디터 검증
        public void Init()
        {
            save = SaveController.GetSaveObject<CharacterSave>($"Character_{id}");

#if UNITY_EDITOR
            if (stages.IsNullOrEmpty())
                Debug.LogError("[Character]: Character has no stages!", this);
#endif
        }

        // 현재 업그레이드 기준으로 해당하는 스테이지 데이터 반환
        public CharacterStageData GetCurrentStage()
        {
            for (int i = save.UpgradeLevel; i >= 0; i--)
            {
                if (upgrades[i].ChangeStage)
                    return stages[upgrades[i].StageIndex];
            }

            return stages[0];
        }

        // 특정 업그레이드 인덱스 기준으로 스테이지 반환
        public CharacterStageData GetStage(int index)
        {
            index = Mathf.Clamp(index, 0, upgrades.Length - 1);

            for (int i = index; i >= 0; i--)
            {
                if (upgrades[i].ChangeStage)
                    return stages[upgrades[i].StageIndex];
            }

            return stages[0];
        }

        // 현재 스테이지 인덱스 반환
        public int GetCurrentStageIndex()
        {
            for (int i = save.UpgradeLevel; i >= 0; i--)
            {
                if (upgrades[i].ChangeStage)
                    return i;
            }

            return 0;
        }

        // 특정 업그레이드 데이터 반환
        public CharacterUpgrade GetUpgrade(int index)
        {
            return upgrades[Mathf.Clamp(index, 0, upgrades.Length - 1)];
        }

        // 현재 업그레이드 데이터 반환
        public CharacterUpgrade GetCurrentUpgrade()
        {
            return upgrades[save.UpgradeLevel];
        }

        // 다음 업그레이드 데이터 반환 (없으면 null)
        public CharacterUpgrade GetNextUpgrade()
        {
            if (upgrades.IsInRange(save.UpgradeLevel + 1))
            {
                return upgrades[save.UpgradeLevel + 1];
            }

            return null;
        }

        // 현재 업그레이드 인덱스 반환
        public int GetCurrentUpgradeIndex()
        {
            return save.UpgradeLevel;
        }

        // 최대 업그레이드 상태인지 확인
        public bool IsMaxUpgrade()
        {
            return !upgrades.IsInRange(save.UpgradeLevel + 1);
        }

        // 업그레이드 수행
        public void UpgradeCharacter()
        {
            if (upgrades.IsInRange(save.UpgradeLevel + 1))
            {
                save.UpgradeLevel += 1;
                CharactersController.OnCharacterUpgraded(this);
            }
        }

        // 현재 선택된 캐릭터인지 확인
        public bool IsSelected()
        {
            return CharactersController.SelectedCharacter == this;
        }

        // 해금 조건 충족 여부 확인
        public bool IsUnlocked()
        {
            return ExperienceController.CurrentLevel >= requiredLevel;
        }
    }
}
