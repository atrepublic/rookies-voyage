/*
 * CharacterUpgrade.cs
 * ---------------------
 * 이 스크립트는 캐릭터의 각 업그레이드 단계를 정의합니다.
 * 업그레이드에 필요한 재화 종류와 비용, 해당 업그레이드 적용 시 얻게 되는 능력치(CharacterStats),
 * 그리고 이 업그레이드가 캐릭터의 외형/단계(Stage)를 변경하는지 여부와 변경될 스테이지 인덱스를 포함합니다.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon; // CurrencyType 사용을 위해 추가

namespace Watermelon.SquadShooter
{
    // 직렬화 가능한 클래스로 선언하여 Inspector에서 편집하고 저장할 수 있도록 함
    [System.Serializable]
    public class CharacterUpgrade
    {
        [Tooltip("이 업그레이드를 구매하는 데 필요한 재화의 종류")]
        [SerializeField] CurrencyType currencyType;
        // 외부에서 재화 종류에 접근하기 위한 프로퍼티
        public CurrencyType CurrencyType => currencyType;

        [Tooltip("이 업그레이드를 구매하는 데 필요한 재화의 양 (가격)")]
        [SerializeField] int price;
        // 외부에서 가격에 접근하기 위한 프로퍼티
        public int Price => price;

        [Space] // 인스펙터 공백
        [Tooltip("이 업그레이드를 적용했을 때의 캐릭터 능력치")]
        [SerializeField] CharacterStats stats;
        // 외부에서 능력치 정보에 접근하기 위한 프로퍼티
        public CharacterStats Stats => stats;

        [Space] // 인스펙터 공백
        [Tooltip("이 업그레이드가 캐릭터의 외형/단계(Stage)를 변경하는지 여부 (진화 여부)")]
        [SerializeField] bool changeStage;
        // 외부에서 스테이지 변경 여부에 접근하기 위한 프로퍼티
        public bool ChangeStage => changeStage;

        [Tooltip("ChangeStage가 true일 경우, 변경될 캐릭터 스테이지의 인덱스 (CharacterData의 Stages 배열 기준, -1이면 변경 없음)")]
        [SerializeField] int stageIndex = -1;
        // 외부에서 스테이지 인덱스에 접근하기 위한 프로퍼티
        public int StageIndex => stageIndex;
    }
}