// 스크립트 기능 요약:
// 이 스크립트는 게임 내 단일 화폐(통화)의 속성과 상태를 나타내는 클래스입니다.
// 화폐의 종류, 기본 금액, 아이콘, 관련 데이터, 저장 상태 등을 관리합니다.
// 화폐 금액 변경 시 이벤트를 발생시키는 기능을 포함하여, UI나 다른 시스템에서 화폐 변화에 반응할 수 있도록 합니다.
// [System.Serializable] 속성을 통해 Unity 에디터에서 직렬화되어 인스펙터 창 등에 표시될 수 있습니다.

using UnityEngine; // SerializeField, Sprite, GameObject, AudioClip 속성 사용을 위해 필요

namespace Watermelon
{
    // Currency 클래스는 게임 내 개별 화폐의 정보를 저장하고 관리하는 직렬화 가능한 데이터 클래스입니다.
    [System.Serializable]
    public class Currency
    {
        // currencyType: 이 화폐의 종류를 나타내는 열거형 값입니다. 어떤 종류의 화폐인지 구분하는 데 사용됩니다.
        [SerializeField]
        [Tooltip("이 화폐의 종류")]
        CurrencyType currencyType;
        // CurrencyType 속성: currencyType 변수의 값을 읽기 전용으로 제공합니다.
        public CurrencyType CurrencyType => currencyType;

        // defaultAmount: 새로운 게임 시작 시 이 화폐의 기본 보유량입니다.
        [SerializeField]
        [Tooltip("새 게임 시작 시 기본 보유량")]
        int defaultAmount = 0;
        // DefaultAmount 속성: defaultAmount 변수의 값을 읽기 전용으로 제공합니다.
        public int DefaultAmount => defaultAmount;

        // icon: 이 화폐를 나타내는 스프라이트(이미지)입니다. UI에 표시될 수 있습니다.
        [SerializeField]
        [Tooltip("이 화폐를 나타내는 아이콘 스프라이트")]
        Sprite icon;
        // Icon 속성: icon 변수의 값을 읽기 전용으로 제공합니다.
        public Sprite Icon => icon;

        // data: 이 화폐와 관련된 추가 데이터를 포함하는 CurrencyData 객체에 대한 참조입니다.
        // 화폐별 특화된 설정을 담을 수 있습니다.
        [SerializeField]
        [Tooltip("이 화폐와 관련된 추가 데이터")]
        CurrencyData data;
        // Data 속성: data 변수의 값을 읽기 전용으로 제공합니다.
        public CurrencyData Data => data;

        // floatingCloud: 이 화폐가 획득될 때 플로팅 클라우드 효과를 사용할지 여부 및 관련 설정을 담는 객체입니다.
        [SerializeField]
        [Tooltip("화폐 획득 시 플로팅 클라우드 효과 설정")]
        FloatingCloudCase floatingCloud;
        // FloatingCloud 속성: floatingCloud 변수의 값을 읽기 전용으로 제공합니다.
        public FloatingCloudCase FloatingCloud => floatingCloud;

        // Amount: 현재 이 화폐의 보유량입니다. Save 객체의 Amount에 접근하여 값을 가져오거나 설정합니다.
        [Tooltip("현재 보유량")]
        public int Amount { get => save.Amount; set => save.Amount = value; }

        // AmountFormatted: 현재 보유량을 형식화된 문자열로 반환합니다. (예: "1.2k", "1.5M") CurrencyHelper를 사용합니다.
        [Tooltip("현재 보유량을 형식화된 문자열로 표시")]
        public string AmountFormatted => CurrencyHelper.Format(save.Amount);

        // OnCurrencyChanged: 이 화폐의 보유량이 변경될 때 호출되는 이벤트입니다.
        // CurrencyController에서 이 이벤트를 발생시켜 다른 리스너들에게 알립니다.
        [Tooltip("화폐 보유량 변경 시 발생하는 이벤트")]
        public event CurrencyCallback OnCurrencyChanged;

        // save: 이 화폐의 보유량을 저장하고 로드하는 데 사용되는 Save 객체입니다.
        // ISaveObject 인터페이스를 구현합니다.
        [Tooltip("화폐 보유량 저장 객체")]
        private Save save;

        /// <summary>
        /// 화폐 객체를 초기화하는 함수입니다.
        /// 관련 데이터 객체를 초기화하고 이 화폐 객체에 대한 참조를 전달합니다.
        /// </summary>
        public void Init()
        {
            data.Init(this);
        }

        /// <summary>
        /// 이 화폐 객체에 저장 객체를 설정하는 함수입니다.
        /// CurrencyController에서 로드된 저장 데이터를 할당할 때 사용됩니다.
        /// </summary>
        /// <param name="save">설정할 Save 객체</param>
        public void SetSave(Save save)
        {
            this.save = save;
        }

        /// <summary>
        /// 화폐 보유량 변경 이벤트를 발생시키는 함수입니다.
        /// 보유량의 변화량(difference)을 함께 전달합니다.
        /// </summary>
        /// <param name="difference">화폐 보유량의 변화량 (+값은 증가, -값은 감소)</param>
        public void InvokeChangeEvent(int difference)
        {
            OnCurrencyChanged?.Invoke(this, difference); // 이벤트 리스너들에게 호출
        }

        // Save 클래스는 화폐의 저장 가능한 상태(보유량)를 나타내는 내부 클래스입니다.
        [System.Serializable]
        public class Save : ISaveObject
        {
            // amount: 이 화폐의 저장된 보유량입니다. 초기값 -1은 로드되지 않았음을 나타낼 수 있습니다.
            [SerializeField]
            [Tooltip("저장된 화폐 보유량")]
            int amount = -1;
            // Amount 속성: amount 변수에 대한 접근자입니다.
            public int Amount { get => amount; set => amount = value; }

            /// <summary>
            /// 저장 데이터를 플러시하는 함수입니다.
            /// 현재 구현은 비어있지만, 필요한 경우 저장 전 추가 처리를 정의할 수 있습니다.
            /// </summary>
            public void Flush()
            {
                // 필요에 따라 저장 전 로직 추가
            }
        }

        // FloatingCloudCase 클래스는 화폐 획득 시 플로팅 클라우드 효과 관련 설정을 담는 내부 클래스입니다.
        [System.Serializable]
        public class FloatingCloudCase
        {
            // addToCloud: 화폐 획득 시 플로팅 클라우드 효과를 사용할지 여부입니다.
            [SerializeField]
            [Tooltip("화폐 획득 시 플로팅 클라우드 효과 사용 여부")]
            bool addToCloud;
            // AddToCloud 속성: addToCloud 변수의 값을 읽기 전용으로 제공합니다.
            public bool AddToCloud => addToCloud;

            // radius: 플로팅 클라우드가 생성될 때의 반경입니다.
            [SerializeField]
            [Tooltip("플로팅 클라우드 효과 반경")]
            float radius = 200;
            // Radius 속성: radius 변수의 값을 읽기 전용으로 제공합니다.
            public float Radius => radius;

            // specialPrefab: 플로팅 클라우드 효과에 사용될 특별한 프리팹입니다. (선택 사항)
            [SerializeField]
            [Tooltip("플로팅 클라우드 효과에 사용될 특별 프리팹")]
            GameObject specialPrefab;
            // SpecialPrefab 속성: specialPrefab 변수의 값을 읽기 전용으로 제공합니다.
            public GameObject SpecialPrefab => specialPrefab;

            // appearAudioClip: 플로팅 클라우드가 나타날 때 재생될 오디오 클립입니다.
            [SerializeField]
            [Tooltip("플로팅 클라우드 나타날 때 재생될 오디오 클립")]
            AudioClip appearAudioClip;
            // AppearAudioClip 속성: appearAudioClip 변수의 값을 읽기 전용으로 제공합니다.
            public AudioClip AppearAudioClip => appearAudioClip;

            // collectAudioClip: 플로팅 클라우드가 수집될 때 재생될 오디오 클립입니다.
            [SerializeField]
            [Tooltip("플로팅 클라우드 수집될 때 재생될 오디오 클립")]
            AudioClip collectAudioClip;
            // CollectAudioClip 속성: collectAudioClip 변수의 값을 읽기 전용으로 제공합니다.
            public AudioClip CollectAudioClip => collectAudioClip;
        }
    }
}