// 스크립트 설명: UI 게임패드 버튼의 기능을 그룹화하거나 상태를 나타내는 데 사용되는 태그를 정의하는 열거형(Enum)입니다.
// [Flags] 속성을 사용하여 여러 태그를 조합하여 사용할 수 있습니다.
using System; // [Flags] 속성 사용을 위한 네임스페이스

namespace Watermelon
{
    // UIGamepadButton에 태그를 지정하여 여러 상태를 나타낼 수 있도록 하는 열거형
    // [Flags] 속성을 사용하면 비트 연산을 통해 여러 값을 동시에 가질 수 있습니다.
    [Flags]
    public enum UIGamepadButtonTag
    {
        None = 0, // 태그 없음
        Settings = 1, // 설정 관련 태그
        MainMenu = 2, // 메인 메뉴 관련 태그
        Weapons = 4, // 무기 관련 태그
        Characters = 8, // 캐릭터 관련 태그
        Pets = 512,  // ✅ 펫 페이지 태그 추가
        Game = 16, // 게임 플레이 중 관련 태그
        Pause = 32, // 일시 정지 관련 태그
        Complete = 64, // 게임 완료 관련 태그
        CharacterSuggestion = 128, // 캐릭터 추천 관련 태그
        GameOver = 256, // 게임 오버 관련 태그

        // 필요한 다른 태그가 있다면 여기에 추가, 각 값은 2의 제곱수로 지정
        // 예: Inventory = 512, Shop = 1024 등
    }
}