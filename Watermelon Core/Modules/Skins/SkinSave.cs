// SkinSave.cs
// 이 클래스는 각 스킨의 잠금 해제 상태를 저장하는 용도로 사용됩니다.
// ISaveObject를 구현하여 세이브 데이터로 직렬화되고, 저장 및 불러오기 기능을 제공합니다.

namespace Watermelon
{
    [System.Serializable]
    public class SkinSave : ISaveObject
    {
        //[Tooltip("이 스킨이 잠금 해제되었는지 여부를 저장합니다.")]
        public bool IsUnlocked = false;

        /// <summary>
        /// 저장 상태를 영구 저장소에 즉시 반영합니다.
        /// 현재는 구현되어 있지 않으며, 필요 시 확장 가능합니다.
        /// </summary>
        public void Flush()
        {
            // 저장 시 동작할 로직이 있다면 여기에 작성
        }
    }
}
