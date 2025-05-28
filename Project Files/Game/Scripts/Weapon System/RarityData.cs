//====================================================================================================
// 해당 스크립트: RarityData.cs
// 기능: 아이템 희귀도별 데이터를 저장하는 스크립터블 오브젝트 또는 직렬화 가능한 클래스입니다.
// 용도: 각 희귀도 등급에 해당하는 이름, 메인 색상, 텍스트 색상 등의 정보를 정의하여
//      게임 내에서 희귀도에 따른 UI나 효과를 일관되게 적용할 수 있도록 합니다.
//====================================================================================================
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 시스템에서 직렬화 가능하도록 설정 (인스펙터에 표시 가능)
    [System.Serializable]
    public class RarityData
    {
        [Tooltip("해당 데이터가 나타내는 희귀도 등급입니다.")]
        [SerializeField] private Rarity rarity;
        /// <summary>
        /// 해당 데이터가 나타내는 희귀도 등급을 가져오는 프로퍼티입니다.
        /// </summary>
        public Rarity Rarity => rarity;

        [Tooltip("해당 희귀도의 표시 이름입니다.")]
        [SerializeField] private string name;
        /// <summary>
        /// 해당 희귀도의 표시 이름을 가져오는 프로퍼티입니다.
        /// </summary>
        public string Name => name;

        [Tooltip("해당 희귀도를 나타내는 메인 색상입니다.")]
        [SerializeField] private Color mainColor;
        /// <summary>
        /// 해당 희귀도를 나타내는 메인 색상을 가져오는 프로퍼티입니다.
        /// </summary>
        public Color MainColor => mainColor;

        [Tooltip("해당 희귀도 이름 텍스트에 사용될 색상입니다.")]
        [SerializeField] private Color textColor;
        /// <summary>
        /// 해당 희귀도 이름 텍스트에 사용될 색상을 가져오는 프로퍼티입니다.
        /// </summary>
        public Color TextColor => textColor;
    }
}