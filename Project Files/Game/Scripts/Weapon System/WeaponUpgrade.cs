// 이 스크립트는 특정 무기의 강화 단계별 데이터를 정의하는 직렬화 가능한 클래스입니다.
// 강화 단계별 가격, 화폐 타입, 프리팹, 스탯(데미지, 사거리, 연사력 등) 정보를 포함합니다.
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // System.Serializable 속성을 사용하여 이 클래스의 객체를 직렬화하여 저장하고 로드할 수 있도록 합니다.
    [System.Serializable]
    public class WeaponUpgrade
    {
        [Tooltip("이 강화 단계로 업그레이드하는 데 필요한 비용입니다.")]
        [SerializeField] int price;
        // 강화 비용에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public int Price => price;

        [Tooltip("이 강화 단계로 업그레이드하는 데 사용되는 화폐 타입입니다.")]
        [SerializeField] CurrencyType currencyType;
        // 사용되는 화폐 타입에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public CurrencyType CurrencyType => currencyType;

        [Tooltip("이 강화 단계의 무기를 나타내는 미리보기 Sprite입니다.")]
        [SerializeField] Sprite previewSprite;
        // 미리보기 Sprite에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public Sprite PreviewSprite => previewSprite;

        [Header("프리팹")]
        [Tooltip("이 강화 단계에서 사용될 무기 모델 프리팹입니다.")]
        [SerializeField] GameObject weaponPrefab;
        // 무기 모델 프리팹에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public GameObject WeaponPrefab => weaponPrefab;

        [Tooltip("이 강화 단계에서 발사될 투사체(총알) 프리팹입니다.")]
        [SerializeField] GameObject bulletPrefab;
        // 투사체 프리팹에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public GameObject BulletPrefab => bulletPrefab;

        [Header("데이터")]
        [Tooltip("이 강화 단계의 무기 데미지 값입니다 (최소/최대 값).")]
        [SerializeField] DuoInt damage; // DuoInt는 두 개의 정수 값을 저장하는 사용자 정의 구조체일 수 있습니다.
        // 데미지 값에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public DuoInt Damage => damage;

        [Tooltip("이 강화 단계의 무기 공격 사거리입니다.")]
        [SerializeField] float rangeRadius;
        // 공격 사거리에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public float RangeRadius => rangeRadius;

        [Tooltip("초당 발사 횟수입니다 (연사력).")] // 기존 영어 툴팁 번역 및 추가 설명
        [SerializeField] float fireRate;
        // 연사력에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public float FireRate => fireRate;

        [Tooltip("발사체의 퍼짐 정도입니다.")]
        [SerializeField] float spread;
        // 퍼짐 정도에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public float Spread => spread;

        [Tooltip("이 강화 단계의 무기 파워 수치입니다. 게임 내에서 무기 성능을 나타내는 지표로 사용될 수 있습니다.")]
        [SerializeField] int power;
        // 파워 수치에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public int Power => power;

        [Tooltip("한 번 발사 시 생성되는 투사체 수입니다 (최소/최대 값).")]
        [SerializeField] DuoInt bulletsPerShot = new DuoInt(1, 1); // 기본값 설정
        // 발사체 수에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public DuoInt BulletsPerShot => bulletsPerShot;

        [Tooltip("발사체 속도입니다 (최소/최대 값).")]
        [SerializeField] DuoFloat bulletSpeed; // DuoFloat는 두 개의 float 값을 저장하는 사용자 정의 구조체일 수 있습니다.
        // 발사체 속도에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public DuoFloat BulletSpeed => bulletSpeed;

        // key upgrade - "ideal" way to play the game, based on this upgrades sequence is built economy (기존 영어 주석)
        [Tooltip("주요 강화 단계 번호입니다 (-1은 주요 강화 단계가 아님을 의미). 경제 시스템의 기준이 될 수 있습니다.")] // 기존 영어 주석 번역 및 툴팁 추가
        [SerializeField] int keyUpgradeNumber = -1; // 기본값 -1 설정
        // 주요 강화 단계 번호에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public int KeyUpgradeNumber => keyUpgradeNumber;
    }
}