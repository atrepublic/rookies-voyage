// ==============================================
// 📌 WeaponCase.cs
// ✅ 캐릭터 손이나 홀더에 장착된 무기의 위치/회전/스케일 및 상태를 관리
// ✅ 무기 활성화, 비활성화, 초기화 기능 제공
// ==============================================

using System;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 무기 트랜스폼과 위치 정보 등을 관리하는 케이스 클래스
    /// </summary>
    [Serializable]
    public class WeaponCase
    {
        [Tooltip("장착된 무기 오브젝트의 Transform")]
        public Transform weaponTransform;

        [Tooltip("무기 장착 위치의 기준 Transform (예: 손 위치)")]
        public Transform weaponHolderTransform;

        /// <summary>무기 위치 (로컬 기준)</summary>
        public Vector3 LocalPosition { get; set; }

        /// <summary>무기 회전 (로컬 기준)</summary>
        public Quaternion LocalRotation { get; set; }

        /// <summary>무기 스케일 (로컬 기준)</summary>
        public Vector3 LocalScale { get; set; }

        /// <summary>
        /// 📌 무기 초기화 (활성화)
        /// </summary>
        public void Init()
        {
            if (weaponTransform != null)
                weaponTransform.gameObject.SetActive(true);
        }

        /// <summary>
        /// 📌 무기 비활성화 (숨기기)
        /// </summary>
        public void Activate()
        {
            if (weaponTransform != null)
                weaponTransform.gameObject.SetActive(false);
        }

        /// <summary>
        /// 📌 무기 다시 활성화 (되돌리기)
        /// </summary>
        public void Reset()
        {
            if (weaponTransform != null)
                weaponTransform.gameObject.SetActive(true);
        }
    }
}
