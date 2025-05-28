// LevelsDatabase.cs
// 이 스크립트는 게임 내 모든 레벨 데이터를 관리하는 ScriptableObject 데이터베이스입니다.
// 여러 월드 데이터를 포함하며, 특정 월드나 레벨 데이터를 가져오고 다음 레벨 존재 여부를 확인하는 기능을 제공합니다.
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // Unity 에디터에서 "Data/New Level/Levels Database" 메뉴를 통해 이 ScriptableObject를 생성할 수 있도록 합니다.
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Data/New Level/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField, Tooltip("게임에 포함된 모든 월드 데이터 배열")] // worlds 변수에 대한 툴팁
        private WorldData[] worlds; // WorldData 클래스는 외부 정의가 필요합니다.
        // 월드 데이터 배열에 접근하기 위한 속성
        public WorldData[] Worlds => worlds;

        /// <summary>
        /// 레벨 데이터베이스를 초기화하고 각 월드 데이터를 설정합니다.
        /// </summary>
        public void Init()
        {
            // 모든 월드 데이터를 순회하며 초기화
            for (int i = 0; i < worlds.Length; i++)
            {
                worlds[i].Init(); // WorldData 클래스와 Init 메서드는 외부 정의가 필요합니다.
            }
        }

        /// <summary>
        /// 특정 인덱스에 해당하는 월드 데이터를 가져옵니다.
        /// 인덱스가 유효 범위를 벗어나면 월드 배열의 길이로 나눈 나머지 인덱스의 월드를 반환하여 무한 루프를 방지할 수 있습니다.
        /// </summary>
        /// <param name="worldIndex">가져올 월드의 인덱스</param>
        /// <returns>해당 월드 데이터</returns>
        public WorldData GetWorld(int worldIndex)
        {
            // 인덱스가 월드 배열 범위 내에 있는지 확인 (IsInRange 확장 메서드는 외부 정의가 필요합니다.)
            if (worlds.IsInRange(worldIndex))
            {
                // 범위 내에 있으면 해당 월드 반환
                return worlds[worldIndex];
            }

            // 범위를 벗어나면 인덱스를 월드 배열 길이로 나눈 나머지 인덱스의 월드 반환 (반복)
            return worlds[worldIndex % worlds.Length];
        }

        /// <summary>
        /// 데이터베이스에 있는 월드들 중에서 무작위 레벨 하나를 가져옵니다.
        /// 유효한 레벨을 찾을 때까지 반복합니다.
        /// </summary>
        /// <returns>무작위 레벨 데이터</returns>
        public LevelData GetRandomLevel()
        {
            LevelData tempLevel = null;

            // 유효한 레벨을 찾을 때까지 반복
            do
            {
                // 월드 배열에서 무작위 월드 가져오기 (GetRandomItem 확장 메서드는 외부 정의가 필요합니다.)
                WorldData randomWorld = worlds.GetRandomItem();
                if (randomWorld != null)
                {
                    // 무작위 월드에서 무작위 레벨 가져오기 (GetRandomItem 확장 메서드는 외부 정의가 필요합니다.)
                    LevelData randomLevel = randomWorld.Levels.GetRandomItem(); // WorldData.Levels 속성은 외부 정의가 필요합니다.
                    if (randomLevel != null)
                        tempLevel = randomLevel;
                }
            }
            while (tempLevel == null); // 유효한 레벨을 찾을 때까지 반복

            return tempLevel;
        }

        /// <summary>
        /// 특정 월드 및 레벨 인덱스에 해당하는 레벨 데이터를 가져옵니다.
        /// 해당 레벨이 존재하지 않으면 무작위 레벨을 반환합니다.
        /// </summary>
        /// <param name="worldIndex">월드의 인덱스</param>
        /// <param name="levelIndex">레벨의 인덱스</param>
        /// <returns>해당 레벨 데이터 또는 무작위 레벨 데이터</returns>
        public LevelData GetLevel(int worldIndex, int levelIndex)
        {
            // 특정 인덱스의 월드 데이터 가져오기
            WorldData world = GetWorld(worldIndex);
            if (world != null)
            {
                // 월드 내에 레벨 인덱스가 범위 내에 있는지 확인 (IsInRange 확장 메서드는 외부 정의가 필요합니다.)
                if (world.Levels.IsInRange(levelIndex)) // WorldData.Levels 속성은 외부 정의가 필요합니다.
                {
                    // 범위 내에 있으면 해당 레벨 반환
                    return world.Levels[levelIndex]; // WorldData.Levels 속성은 외부 정의가 필요합니다.
                }
            }

            // 해당 레벨이 없으면 무작위 레벨 반환
            return GetRandomLevel();
        }

        /// <summary>
        /// 특정 월드 및 현재 레벨 인덱스 다음 레벨이 존재하는지 확인합니다.
        /// </summary>
        /// <param name="worldIndex">월드의 인덱스</param>
        /// <param name="levelIndex">현재 레벨의 인덱스</param>
        /// <returns>다음 레벨이 존재하면 true, 그렇지 않으면 false</returns>
        public bool DoesNextLevelExist(int worldIndex, int levelIndex)
        {
            // 특정 인덱스의 월드 데이터 가져오기
            WorldData world = GetWorld(worldIndex);
            if (world != null)
            {
                // 월드 내에 현재 레벨 인덱스 + 1에 해당하는 레벨이 범위 내에 있는지 확인 (IsInRange 확장 메서드는 외부 정의가 필요합니다.)
                if (world.Levels.IsInRange(levelIndex + 1)) // WorldData.Levels 속성은 외부 정의가 필요합니다.
                {
                    // 다음 레벨이 존재함
                    return true;
                }
            }

            // 다음 레벨이 존재하지 않음
            return false;
        }

        /// <summary>
        /// 데이터베이스에 포함된 월드의 총 개수를 반환합니다.
        /// </summary>
        /// <returns>월드의 총 개수</returns>
        public int GetWorldsAmount()
        {
            return worlds.Length;
        }
    }
}