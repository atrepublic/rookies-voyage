// MD_CurrencyData.cs
// 이 스크립트는 게임 내에서 사용되는 개별 통화의 메타데이터를 정의합니다.
// 통화가 드롭될 때 사용되는 모델, 획득 시 사운드, 항상 표시될지 여부 등의 정보를 담고 있습니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyData
    {
        [Tooltip("이 통화가 드롭될 때 사용되는 3D 모델 GameObject입니다.")]
        [SerializeField] GameObject dropModel;
        // 이 통화의 드롭 모델입니다.
        public GameObject DropModel => dropModel;

        [Tooltip("이 통화를 획득했을 때 재생될 오디오 클립입니다.")]
        [SerializeField] AudioClip dropPickupSound;
        // 이 통화를 획득했을 때 재생되는 사운드입니다.
        public AudioClip DropPickupSound => dropPickupSound;

        [Space]
        [Tooltip("이 통화가 UI 등에 항상 표시되어야 하는지 여부를 설정합니다.")]
        [SerializeField] bool displayAlways = false;
        // 이 통화가 항상 표시되어야 하는지 여부를 나타냅니다.
        public bool DisplayAlways => displayAlways;

        // 이 통화 데이터를 초기화하는 함수입니다. 현재는 비어 있습니다.
        public void Init(Currency currency)
        {
            // 초기화 로직 (필요에 따라 구현)
        }
    }
}