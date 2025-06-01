/*
 * CharacterStats.cs
 * ---------------------
 * 이 스크립트는 캐릭터의 특정 업그레이드 레벨 또는 단계에서의 능력치를 정의합니다.
 * 체력, 총알 데미지 배율, 전투력(Power),  치명타 능력치 등의 데이터를 포함합니다.
 * 또한, 게임 경제 밸런싱의 기준이 되는 '키 업그레이드 번호'를 가질 수 있습니다.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 직렬화 가능한 클래스로 선언하여 Inspector에서 편집하고 저장할 수 있도록 함
    [System.Serializable]
    public class CharacterStats
    {
        [Tooltip("해당 레벨/단계에서의 캐릭터 체력")]
        [SerializeField] int health;
        // 외부에서 체력 값에 접근하기 위한 프로퍼티
        public int Health => health;

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ MoveSpeed 필드 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        [Tooltip("해당 레벨/단계에서의 캐릭터 이동 속도")]
        [SerializeField] float moveSpeed = 5.0f; // 기본값 예시, 실제 값은 각 CharacterUpgrade에서 설정
        public float MoveSpeed => moveSpeed;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        [Space] // 인스펙터 공백
        [Tooltip("해당 레벨/단계에서의 캐릭터 총알 데미지 배율 (기본값 1.0)")]
        [SerializeField] float bulletDamageMultiplier = 1.0f;
        // 외부에서 총알 데미지 배율 값에 접근하기 위한 프로퍼티
        public float BulletDamageMultiplier => bulletDamageMultiplier;

        [Tooltip("해당 레벨/단계에서의 캐릭터 전투력 (UI 표시 및 계산용)")]
        [SerializeField] int power;
        // 외부에서 전투력 값에 접근하기 위한 프로퍼티
        public int Power => power;

        // 주석: key upgrade - 게임 플레이의 "이상적인" 경로. 이 업그레이드 순서에 기반하여 경제가 구축됩니다.
        [Tooltip("키 업그레이드 번호. 게임 밸런싱의 기준이 되는 주요 업그레이드 식별자 (-1이면 일반 업그레이드)")]
        [SerializeField] int keyUpgradeNumber = -1; // 기본값을 -1로 명시
        // 외부에서 키 업그레이드 번호에 접근하기 위한 프로퍼티
        public int KeyUpgradeNumber => keyUpgradeNumber;


        [Header("치명타 능력치")]
        [Tooltip("치명타 발생 확률 (0.0 ~ 1.0)")]
        [SerializeField] float critChance = 0.1f;
        public float CritChance => critChance;

        [Tooltip("치명타 배수 (예: 2.0은 2배 데미지)")]
        [SerializeField] float critMultiplier = 2.0f;
        public float CritMultiplier => critMultiplier;
    }
}