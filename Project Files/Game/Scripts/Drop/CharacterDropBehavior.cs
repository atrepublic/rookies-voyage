// 스크립트 설명: 캐릭터 드롭 아이템의 동작을 정의하는 클래스입니다.
// 캐릭터 데이터 설정 및 획득 시 캐릭터 정보 업데이트 기능을 구현합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class CharacterDropBehavior : BaseDropBehavior
    {
        [SerializeField]
        [Tooltip("이 드롭 아이템에 해당하는 캐릭터 데이터")] // 주요 변수 한글 툴팁
        CharacterData character; // 캐릭터 데이터

        [SerializeField]
        [Tooltip("이 드롭 아이템에 해당하는 캐릭터 레벨")] // 주요 변수 한글 툴팁
        int characterLevel; // 캐릭터 레벨

        /// <summary>
        /// 캐릭터 드롭 아이템의 캐릭터 데이터와 레벨을 설정합니다.
        /// </summary>
        /// <param name="character">설정할 캐릭터 데이터.</param>
        /// <param name="characterLevel">설정할 캐릭터 레벨.</param>
        public void SetCharacterData(CharacterData character, int characterLevel)
        {
            if (character == null)
            {
                Debug.LogError("캐릭터 데이터가 Null입니다!"); // 한글 로그 메시지

                return;
            }

            this.character = character;
            this.characterLevel = characterLevel;
        }

        /// <summary>
        /// 캐릭터 카드 드롭 아이템 획득에 대한 보상을 적용합니다.
        /// 캐릭터의 외형 및 능력치를 업데이트합니다.
        /// </summary>
        /// <param name="autoReward">자동 보상 적용 여부 (이 스크립트에서는 사용되지 않음).</param>
        public override void ApplyReward(bool autoReward = false)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour(); // 현재 플레이어 캐릭터의 Behaviour 가져오기
            if (characterBehaviour != null)
            {
                CharacterStageData currentStage = character.GetStage(characterLevel); // 현재 레벨에 해당하는 캐릭터 스테이지 데이터 가져오기
                CharacterUpgrade currentUpgrade = character.GetUpgrade(characterLevel); // 현재 레벨에 해당하는 캐릭터 업그레이드 데이터 가져오기

                // 캐릭터 외형 및 능력치 업데이트
                characterBehaviour.SetGraphics(currentStage.Prefab, false, false);
                characterBehaviour.SetStats(currentUpgrade.Stats);
            }
        }
    }
}