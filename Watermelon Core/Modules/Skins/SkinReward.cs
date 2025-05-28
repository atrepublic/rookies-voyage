// SkinReward.cs
// 이 스크립트는 스킨을 보상으로 지급하는 시스템을 구현한 클래스입니다.
// 특정 스킨 ID를 기반으로 해당 스킨을 잠금 해제하며, 이미 해제된 경우에는 비활성화 조건을 검사합니다.

using UnityEngine;

namespace Watermelon
{
    public class SkinReward : Reward
    {
        [SkinPicker]
        [SerializeField, Tooltip("보상으로 지급할 스킨의 ID")]
        private string skinID;

        [SerializeField, Tooltip("스킨이 이미 잠금 해제된 경우 보상 오브젝트를 비활성화할지 여부")]
        private bool disableIfSkinIsUnlocked;

        private SkinController skinsController;

        // 시작 시 SkinController 인스턴스를 참조
        private void Start()
        {
            skinsController = SkinController.Instance;
        }

        // 스크립트 활성화 시 이벤트 등록
        private void OnEnable()
        {
            SkinController.SkinUnlocked += OnSkinUnlocked;
        }

        // 스크립트 비활성화 시 이벤트 해제
        private void OnDisable()
        {
            SkinController.SkinUnlocked -= OnSkinUnlocked;
        }

        /// <summary>
        /// 실제 보상을 지급하는 함수입니다. 해당 스킨을 잠금 해제하고 선택합니다.
        /// </summary>
        public override void ApplyReward()
        {
            skinsController.UnlockSkin(skinID, true);
        }

        /// <summary>
        /// 스킨이 이미 잠금 해제되어 있는지에 따라 비활성화 상태를 결정합니다.
        /// </summary>
        public override bool CheckDisableState()
        {
            if (disableIfSkinIsUnlocked)
            {
                return skinsController.IsSkinUnlocked(skinID);
            }

            return false;
        }

        /// <summary>
        /// 스킨이 잠금 해제될 때 이벤트를 받아서 오브젝트를 비활성화합니다.
        /// </summary>
        private void OnSkinUnlocked(ISkinData skinData)
        {
            if (disableIfSkinIsUnlocked && skinData.ID == skinID)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
