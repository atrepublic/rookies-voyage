
// ==============================================
// CharacterGlobalSave.cs
// ==============================================
// 선택된 캐릭터의 ID를 저장하는 전역 저장 클래스입니다.
// 게임 전체에서 사용되는 단일 세이브 객체로, 현재 선택된 캐릭터 정보를 유지합니다.


using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterGlobalSave : ISaveObject
    {
        public string SelectedCharacterID;

        public void Flush()
        {

        }
    }
}