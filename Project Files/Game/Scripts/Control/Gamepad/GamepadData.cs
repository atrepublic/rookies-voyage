// 스크립트 설명: 게임패드 버튼에 대한 정보(아이콘 등)를 담는 ScriptableObject입니다.
// 각 게임패드 버튼 타입별 아이콘 스프라이트를 관리하고 제공하는 기능을 포함합니다.
using System.Collections; // List 사용을 위한 네임스페이스 (List<ButtonData> data)
using System.Collections.Generic; // Dictionary, List 사용을 위한 네임스페이스
using UnityEngine;

namespace Watermelon
{
    // Unity 에디터에서 ScriptableObject로 생성할 수 있도록 메뉴 추가
    [CreateAssetMenu(fileName = "Gamepad Data", menuName = "Data/Core/Gamepad Data")] // 메뉴 경로 지정
    public class GamepadData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("게임패드 각 버튼에 대한 아이콘 데이터 목록")] // 주요 변수 한글 툴팁
        List<ButtonData> data; // 버튼 아이콘 데이터 목록

        // 게임패드 버튼 타입과 해당 아이콘 스프라이트를 매핑하는 딕셔너리
        private Dictionary<GamepadButtonType, Sprite> iconsDictionary;

        /// <summary>
        /// GamepadData를 초기화하고 버튼 타입별 아이콘 딕셔너리를 생성합니다.
        /// </summary>
        public void Init()
        {
            iconsDictionary = new Dictionary<GamepadButtonType, Sprite>(); // 딕셔너리 생성

            // 버튼 데이터 목록을 순회하며 딕셔너리에 버튼 타입과 아이콘 스프라이트 추가
            for(int i = 0; i < data.Count; i++)
            {
                iconsDictionary.Add(data[i].button, data[i].icon);
            }
        }

        /// <summary>
        /// 지정된 게임패드 버튼 타입에 해당하는 아이콘 스프라이트를 가져옵니다.
        /// </summary>
        /// <param name="button">아이콘을 가져올 게임패드 버튼 타입.</param>
        /// <returns>해당 버튼의 아이콘 스프라이트.</returns>
        public Sprite GetButtonIcon(GamepadButtonType button)
        {
            // 딕셔너리에서 버튼 타입에 해당하는 아이콘 스프라이트를 찾아 반환
            // 만약 해당 키가 없으면 예외 발생 가능성이 있으므로 사용 시 주의 필요
            return iconsDictionary[button];
        }

        // 게임패드 각 버튼에 대한 데이터(버튼 타입, 아이콘)를 담는 내부 클래스
        [System.Serializable] // Unity 에디터에서 인스펙터에 표시될 수 있도록 직렬화 가능하게 설정
        public class ButtonData
        {
            [Tooltip("게임패드 버튼 타입")] // 주요 변수 한글 툴팁
            public GamepadButtonType button; // 게임패드 버튼 타입

            [Tooltip("해당 버튼에 사용될 아이콘 스프라이트")] // 주요 변수 한글 툴팁
            public Sprite icon; // 버튼 아이콘 스프라이트
        }
    }
}