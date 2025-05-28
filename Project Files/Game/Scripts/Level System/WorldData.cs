// WorldData.cs
// 이 스크립트는 게임 내 단일 월드의 데이터 구조를 정의하는 ScriptableObject입니다.
// 월드의 미리보기 이미지, 고유 음악, 월드 유형, 포함된 레벨 데이터, 레벨 아이템, 방 환경 프리셋, 월드 사용자 지정 오브젝트 정보를 관리합니다.
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // Unity 에디터에서 "Data/New Level/World" 메뉴를 통해 이 ScriptableObject를 생성할 수 있도록 합니다.
    [CreateAssetMenu(fileName = "World", menuName = "Data/New Level/World")]
    public class WorldData : ScriptableObject
    {
        [SerializeField, Tooltip("월드의 미리보기 이미지")] // previewSprite 변수에 대한 툴팁
        private Sprite previewSprite;
        // 미리보기 이미지에 접근하기 위한 속성
        public Sprite PreviewSprite => previewSprite;

        [SerializeField, Tooltip("이 월드만의 고유 배경 음악 오디오 클립")] // uniqueWorldMusicClip 변수에 대한 툴팁
        private AudioClip uniqueWorldMusicClip;
        // 고유 배경 음악에 접근하기 위한 속성
        public AudioClip UniqueWorldMusicClip => uniqueWorldMusicClip;

        [SerializeField, Tooltip("이 월드의 유형")] // worldType 변수에 대한 툴팁
        private WorldType worldType; // WorldType 열거형은 외부 정의가 필요합니다.
        // 월드 유형에 접근하기 위한 속성
        public WorldType WorldType => worldType;

        [SerializeField, Tooltip("이 월드에 포함된 레벨 데이터 배열"), LevelEditorSetting] // LevelEditorSetting은 커스텀 에디터 속성으로 가정합니다.
        private LevelData[] levels; // LevelData 클래스는 외부 정의가 필요합니다.
        // 레벨 데이터 배열에 접근하기 위한 속성
        public LevelData[] Levels => levels;

        [SerializeField, Tooltip("이 월드에서 사용될 수 있는 레벨 아이템(장애물, 환경 요소 등) 배열"), LevelEditorSetting]
        private LevelItem[] items; // LevelItem 클래스는 외부 정의가 필요합니다.
        // 레벨 아이템 배열에 접근하기 위한 속성
        public LevelItem[] Items => items;

        [SerializeField, Tooltip("이 월드에서 사용될 수 있는 방 환경 사전 설정 배열"), LevelEditorSetting]
        private RoomEnvironmentPreset[] roomEnvPresets; // RoomEnvironmentPreset 클래스는 외부 정의가 필요합니다.
        // 방 환경 프리셋 배열에 접근하기 위한 속성
        public RoomEnvironmentPreset[] RoomEnvPresets => roomEnvPresets;

        [SerializeField, Tooltip("이 월드에 배치될 사용자 지정 오브젝트 데이터 배열"), LevelEditorSetting]
        private CustomObjectData[] worldCustomObjects; // CustomObjectData 클래스는 외부 정의가 필요합니다.
        // 월드 사용자 지정 오브젝트 데이터 배열에 접근하기 위한 속성
        public CustomObjectData[] WorldCustomObjects => worldCustomObjects;

        // 레벨 아이템을 해시 값으로 빠르게 찾기 위한 딕셔너리
        private Dictionary<int, LevelItem> itemsDisctionary;

        /// <summary>
        /// 월드 데이터를 초기화하고 포함된 레벨 및 아이템 데이터를 설정합니다.
        /// </summary>
        public void Init()
        {
            // 모든 레벨 데이터를 순회하며 초기화
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].Init(this); // LevelData 클래스와 Init 메서드는 외부 정의가 필요합니다.
            }

            // 레벨 아이템 딕셔너리 초기화 및 설정
            itemsDisctionary = new Dictionary<int, LevelItem>();

            // 모든 레벨 아이템을 딕셔너리에 추가 (해시 값을 키로 사용)
            for (int i = 0; i < items.Length; i++)
            {
                itemsDisctionary.Add(items[i].Hash, items[i]); // LevelItem.Hash 속성은 외부 정의가 필요합니다.
            }
        }

        /// <summary>
        /// 월드 로드 시 오브젝트 풀을 생성하고 아이템 데이터를 로드합니다.
        /// </summary>
        public void LoadWorld()
        {
            // 모든 레벨 아이템의 오브젝트 풀 생성
            for (int i = 0; i < items.Length; i++)
            {
                items[i].OnWorldLoaded(); // LevelItem 클래스와 OnWorldLoaded 메서드는 외부 정의가 필요합니다.
            }
        }

        /// <summary>
        /// 월드 언로드 시 오브젝트 풀을 파괴하고 아이템 데이터를 언로드합니다.
        /// </summary>
        public void UnloadWorld()
        {
            // 모든 레벨 아이템의 오브젝트 풀 해제 (파괴)
            for (int i = 0; i < items.Length; i++)
            {
                items[i].OnWorldUnloaded(); // LevelItem 클래스와 OnWorldUnloaded 메서드는 외부 정의가 필요합니다.
            }
        }

        /// <summary>
        /// 주어진 해시 값에 해당하는 레벨 아이템 데이터를 가져옵니다.
        /// </summary>
        /// <param name="hash">가져올 레벨 아이템의 해시 값</param>
        /// <returns>해당 레벨 아이템 데이터</returns>
        public LevelItem GetLevelItem(int hash)
        {
            // 딕셔너리에서 해시 값으로 레벨 아이템 검색 및 반환
            return itemsDisctionary[hash];
        }
    }
}