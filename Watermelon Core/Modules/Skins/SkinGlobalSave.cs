// SkinGlobalSave.cs
// 이 스크립트는 전체 게임에서 현재 선택된 스킨의 ID를 저장하기 위한 클래스입니다.
// ISaveObject를 구현하여 세이브 데이터로 직렬화되고, 나중에 다시 로드할 수 있도록 합니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SkinGlobalSave : ISaveObject
    {
        [Tooltip("현재 선택된 스킨의 고유 ID")]
        public string SelectedSkinID;

        /// <summary>
        /// 저장된 정보를 영구 저장소에 즉시 저장합니다.
        /// 현재는 구현되어 있지 않지만, 세이브 컨트롤러에서 호출될 수 있습니다.
        /// </summary>
        public void Flush()
        {
            // 저장 로직이 필요한 경우 여기에 작성합니다.
        }
    }
}
