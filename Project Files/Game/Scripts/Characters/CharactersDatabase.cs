/*
 * CharactersDatabase.cs
 * ---------------------
 * 이 스크립트는 게임에 등장하는 모든 캐릭터 데이터(CharacterData)를 관리하는
 * ScriptableObject 기반의 데이터베이스입니다.
 * 캐릭터 목록을 저장하고, ID나 조건에 따라 특정 캐릭터를 찾는 기능을 제공합니다.
 */

using System.Linq; // Linq 네임스페이스 사용
using UnityEngine;
using Watermelon; // Watermelon 프레임워크 네임스페이스

namespace Watermelon.SquadShooter
{
    // ScriptableObject 생성 메뉴에 "Data/Characters/Character Database" 항목 추가
    [CreateAssetMenu(fileName = "Character Database", menuName = "Data/Characters/Character Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [Tooltip("게임 내 모든 캐릭터 데이터 배열")]
        [SerializeField] CharacterData[] characters;
        // 외부에서 캐릭터 데이터 배열에 접근하기 위한 프로퍼티
        public CharacterData[] Characters => characters;

        /// <summary>
        /// 캐릭터 데이터베이스를 초기화합니다.
        /// 캐릭터 배열을 필요 레벨 순으로 정렬하고 각 캐릭터 데이터를 초기화합니다.
        /// </summary>
        public void Init()
        {
            // Linq를 사용하여 RequiredLevel 기준으로 캐릭터 배열 정렬 시도 (주의: 실제 배열 순서를 바꾸진 않음)
            // characters = characters.OrderBy(c => c.RequiredLevel).ToArray(); // 필요 시 실제 배열 재정렬
            // 원본 코드에서는 정렬 후 할당하지 않았으므로, 원본 로직 유지 (정렬된 결과를 사용하지 않음)
            characters.OrderBy(c => c.RequiredLevel); // 이 줄은 사실상 효과 없음

            // 각 캐릭터 데이터 초기화 함수 호출
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].Init();
            }
        }

        /// <summary>
        /// 기본 캐릭터(보통 배열의 첫 번째 캐릭터)를 반환합니다.
        /// </summary>
        /// <returns>기본 캐릭터 데이터</returns>
        public CharacterData GetDefaultCharacter()
        {
            // 배열의 첫 번째 요소를 반환 (Linq 사용)
            return characters.First();
        }

        /// <summary>
        /// 주어진 ID와 일치하는 캐릭터 데이터를 찾아서 반환합니다.
        /// </summary>
        /// <param name="characterID">찾을 캐릭터의 ID</param>
        /// <returns>찾은 캐릭터 데이터 (없으면 null)</returns>
        public CharacterData GetCharacter(string characterID)
        {
            // 캐릭터 배열 순회
            for (int i = 0; i < characters.Length; i++)
            {
                // ID가 일치하는 캐릭터를 찾으면 반환
                if (characters[i].ID == characterID)
                    return characters[i];
            }

            // ID가 일치하는 캐릭터가 없으면 null 반환
            return null;
        }

        /// <summary>
        /// 현재 플레이어 레벨 기준으로 마지막으로 잠금 해제된 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>마지막으로 잠금 해제된 캐릭터 데이터 (모두 잠겨있으면 첫 번째 캐릭터)</returns>
        public CharacterData GetLastUnlockedCharacter()
        {
            // 캐릭터 배열 순회 (RequiredLevel 순으로 정렬되어 있다고 가정)
            for (int i = 0; i < characters.Length; i++)
            {
                // 현재 플레이어 레벨보다 높은 해제 레벨을 가진 캐릭터를 찾으면
                if (characters[i].RequiredLevel > ExperienceController.CurrentLevel)
                {
                    // 바로 이전 캐릭터가 마지막으로 잠금 해제된 캐릭터임
                    // Clamp를 사용하여 인덱스가 배열 범위를 벗어나지 않도록 함 (0 미만 또는 길이 이상 방지)
                    return characters[Mathf.Clamp(i - 1, 0, characters.Length - 1)];
                }
            }

            // 모든 캐릭터의 해제 레벨이 현재 플레이어 레벨 이하이면 마지막 캐릭터 반환
            // 또는 배열이 비어있다면 null을 반환해야 할 수도 있음 (현재 코드는 null 반환)
            // 원본 코드대로 null 반환 로직 유지
            return null;
            // 만약 모든 캐릭터가 해제된 경우 마지막 캐릭터를 반환하려면:
            // return characters.Length > 0 ? characters[characters.Length - 1] : null;
        }

        /// <summary>
        /// 현재 플레이어 레벨 기준으로 다음에 잠금 해제될 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>다음에 잠금 해제될 캐릭터 데이터 (모두 해제되었으면 null)</returns>
        public CharacterData GetNextCharacterToUnlock()
        {
            // 캐릭터 배열 순회 (RequiredLevel 순으로 정렬되어 있다고 가정)
            for (int i = 0; i < characters.Length; i++)
            {
                // 현재 플레이어 레벨보다 높은 해제 레벨을 가진 첫 번째 캐릭터를 찾으면
                if (characters[i].RequiredLevel > ExperienceController.CurrentLevel)
                {
                    // 해당 캐릭터가 다음에 잠금 해제될 캐릭터임
                    return characters[i];
                }
            }

            // 모든 캐릭터가 이미 잠금 해제되었으면 null 반환
            return null;
        }
    }
}