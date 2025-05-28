// 이 스크립트는 총기를 잡는 캐릭터의 손 위치 정보를 정의하는 직렬화 가능한 클래스입니다.
// 기본 손 위치 정보와 특정 캐릭터에 대한 오버라이드 정보를 포함할 수 있습니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // System.Serializable 속성을 사용하여 이 클래스의 객체를 직렬화하여 저장하고 로드할 수 있도록 합니다.
    [System.Serializable]
    public class GunHolder
    {
        [Tooltip("기본 캐릭터에게 적용될 총기 잡는 위치 데이터입니다.")]
        [SerializeField] HolderData defaultHolderData;
        // 기본 총기 잡는 위치 데이터에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public HolderData DefaultHolderData => defaultHolderData;

        [Tooltip("특정 캐릭터에게 기본 설정을 덮어쓸 총기 잡는 위치 데이터 배열입니다.")]
        [SerializeField] CharacterHolderData[] holderDataOverrides;

        /// <summary>
        /// 주어진 캐릭터 데이터에 해당하는 총기 잡는 위치 데이터를 가져옵니다.
        /// 특정 캐릭터에 대한 오버라이드 데이터가 있으면 해당 데이터를 반환하고, 없으면 기본 데이터를 반환합니다.
        /// </summary>
        /// <param name="character">총기 잡는 위치 데이터를 가져올 캐릭터 데이터</param>
        /// <returns>해당 캐릭터의 총기 잡는 위치 데이터</returns>
        public HolderData GetHolderData(CharacterData character)
        {
            // 오버라이드 데이터 배열이 null이 아니거나 비어있지 않으면
            if(!holderDataOverrides.IsNullOrEmpty()) // IsNullOrEmpty()는 사용자 정의 확장 함수일 수 있습니다.
            {
                // 오버라이드 데이터 배열을 순회합니다.
                foreach(CharacterHolderData holderData in holderDataOverrides)
                {
                    // 현재 오버라이드 데이터가 주어진 캐릭터 데이터와 일치하면 해당 데이터를 반환합니다.
                    if(holderData.Character == character)
                    {
                        return holderData;
                    }
                }
            }

            // 오버라이드 데이터를 찾지 못하면 기본 데이터를 반환합니다.
            return defaultHolderData;
        }

        // 총기의 왼손 및 오른손 잡는 위치 트랜스폼을 정의하는 직렬화 가능한 내부 클래스입니다.
        [System.Serializable]
        public class HolderData
        {
            [Tooltip("총기의 왼손이 잡을 위치를 나타내는 트랜스폼입니다.")]
            public Transform LeftHandHolder;
            [Tooltip("총기의 오른손이 잡을 위치를 나타내는 트랜스폼입니다.")]
            public Transform RightHandHolder;
        }

        // 특정 캐릭터에 대한 총기 잡는 위치 데이터를 정의하는 직렬화 가능한 내부 클래스입니다.
        // 기본 HolderData를 상속받아 캐릭터 데이터 필드를 추가합니다.
        [System.Serializable]
        public class CharacterHolderData : HolderData
        {
            [Space]
            [Tooltip("이 설정이 적용될 특정 캐릭터 데이터입니다.")]
            [SerializeField] CharacterData character;
            // 이 설정이 적용될 캐릭터 데이터에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
            public CharacterData Character => character;
        }
    }
}