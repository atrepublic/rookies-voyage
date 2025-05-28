// ISkinsProvider.cs
// 이 인터페이스는 스킨 제공자 클래스들이 구현해야 할 스킨 관리 기능들을 정의합니다.
// 스킨 데이터 접근, 선택, 잠금 해제 기능 등을 포함합니다.

namespace Watermelon
{
    public interface ISkinsProvider
    {
        /// <summary>
        /// 스킨 ID를 통해 해당 스킨 데이터를 가져옵니다.
        /// </summary>
        ISkinData GetSkinData(string skinId);

        /// <summary>
        /// 주어진 스킨 데이터를 잠금 해제합니다. 선택 여부는 매개변수로 결정됩니다.
        /// </summary>
        void UnlockSkin(ISkinData skinData, bool select = false);

        /// <summary>
        /// 주어진 스킨 ID에 해당하는 스킨을 잠금 해제합니다. 선택 여부는 매개변수로 결정됩니다.
        /// </summary>
        void UnlockSkin(string skinId, bool select = false);

        /// <summary>
        /// 주어진 스킨이 현재 선택된 스킨인지 확인합니다.
        /// </summary>
        bool IsSkinSelected(ISkinData skinData);

        /// <summary>
        /// 주어진 스킨 ID가 현재 선택된 스킨인지 확인합니다.
        /// </summary>
        bool IsSkinSelected(string skinId);

        /// <summary>
        /// 주어진 스킨 데이터를 선택 상태로 설정합니다.
        /// </summary>
        void SelectSkin(ISkinData data);

        /// <summary>
        /// 주어진 스킨 ID에 해당하는 스킨을 선택 상태로 설정합니다.
        /// </summary>
        void SelectSkin(string skinId);
    }
}
