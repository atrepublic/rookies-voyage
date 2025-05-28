// 스크립트 설명: 게임 내 상자(Chest)의 기본 동작을 정의하는 추상 클래스입니다.
// 상자의 애니메이션 상태, 드롭 아이템 목록, 개봉 상태, 보상 여부 등 기본적인 상자 관련 기능을 포함합니다.
using System.Collections.Generic; // List 사용을 위한 네임스페이스
using UnityEngine;
using Watermelon.LevelSystem; // LevelController, Drop 관련 네임스페이스
using Watermelon; // DuoInt, IsNullOrEmpty, AudioController 관련 네임스페이스

namespace Watermelon.SquadShooter
{
    public abstract class AbstractChestBehavior : MonoBehaviour
    {
        // 애니메이터 상태 해시 값 (정적 상수)
        protected static readonly int IDLE_HASH = Animator.StringToHash("Idle"); // 대기 상태 해시
        protected static readonly int SHAKE_HASH = Animator.StringToHash("Shake"); // 흔들리는 상태 해시
        protected static readonly int OPEN_HASH = Animator.StringToHash("Open"); // 열리는 상태 해시

        [SerializeField]
        [Tooltip("상자 애니메이션을 제어하는 애니메이터 컴포넌트 참조")] // 주요 변수 한글 툴팁
        protected Animator animatorRef; // 애니메이터 컴포넌트 참조

        [SerializeField]
        [Tooltip("상자 개봉 시 활성화될 파티클 효과 오브젝트")] // 주요 변수 한글 툴팁
        protected GameObject particle; // 파티클 오브젝트

        // 상자가 열렸을 때 호출될 콜백 함수의 델리게이트 타입 정의
        public delegate void OnChestOpenedCallback(AbstractChestBehavior chest);

        [Tooltip("상자에서 드롭될 아이템 데이터 목록")] // 주요 변수 한글 툴팁
        protected List<DropData> dropData; // 드롭될 아이템 데이터 목록

        [Tooltip("상자 개봉 시 드롭될 아이템의 수량 범위 (최소, 최대)")] // 주요 변수 한글 툴팁
        protected DuoInt itemsAmountRange; // 드롭될 아이템 수량 범위

        // 상자가 열렸을 때 외부에 알리기 위한 이벤트
        public static event OnChestOpenedCallback OnChestOpenedEvent;

        [Tooltip("상자가 이미 개봉되었는지 여부")] // 주요 변수 한글 툴팁
        protected bool opened; // 상자 개봉 상태

        [Tooltip("상자가 보상 상자인지 여부 (광고 시청 등)")] // 주요 변수 한글 툴팁
        protected bool isRewarded; // 보상 상자 여부

        /// <summary>
        /// 상자 행동을 초기화합니다.
        /// </summary>
        /// <param name="drop">상자에서 드롭될 아이템 데이터 목록.</param>
        public virtual void Init(List<DropData> drop)
        {
            opened = false; // 초기에는 닫힌 상태
            dropData = drop; // 드롭 데이터 설정
            particle.SetActive(true); // 파티클 활성화

            animatorRef.SetTrigger(IDLE_HASH); // 대기 애니메이션 재생

            // 드롭될 아이템 수량 범위 설정 (예: 9개에서 11개 사이)
            itemsAmountRange = new DuoInt(9, 11); // DuoInt는 Watermelon 네임스페이스에 정의된 것으로 가정
        }

        /// <summary>
        /// 캐릭터가 상자에 접근했을 때 호출되는 추상 메서드입니다.
        /// 하위 클래스에서 상자 접근 시 특정 동작을 구현합니다.
        /// </summary>
        public abstract void ChestApproached(); // 상자 접근 시 동작 정의 (하위 클래스 구현 필수)

        /// <summary>
        /// 캐릭터가 상자에서 멀어졌을 때 호출되는 추상 메서드입니다.
        /// 하위 클래스에서 상자 이탈 시 특정 동작을 구현합니다.
        /// </summary>
        public abstract void ChestLeft(); // 상자 이탈 시 동작 정의 (하위 클래스 구현 필수)

        /// <summary>
        /// 상자에서 보상 아이템(자원)을 드롭합니다.
        /// </summary>
        protected void DropResources()
        {
            // 게임 플레이가 활성화 상태가 아니면 드롭하지 않음
            if (!LevelController.IsGameplayActive) // LevelController에 정의된 것으로 가정
                return;

            // 드롭될 위치의 중심점 계산 (상자 위치 기준 약간 앞에)
            Vector3 dropCenter = transform.position + Vector3.forward * -1f;

            // 드롭 데이터 목록이 비어있지 않다면
            if (!dropData.IsNullOrEmpty()) // IsNullOrEmpty 확장 메서드는 Watermelon 네임스페이스에 정의된 것으로 가정
            {
                // 드롭 데이터 목록의 각 아이템에 대해 처리
                for (int i = 0; i < dropData.Count; i++)
                {
                    // 드롭 아이템 생성 및 설정
                    Drop.SpawnDropItem(dropData[i], dropCenter, Vector3.zero, isRewarded, (drop, fallingStyle) => // Drop.SpawnDropItem에 정의된 것으로 가정
                    {
                        // 생성된 아이템을 지정된 스타일로 던짐
                        Drop.ThrowItem(drop, fallingStyle); // Drop.ThrowItem에 정의된 것으로 가정
                    });
                }

                // 상자 개봉 사운드 재생 (AudioController에 정의된 것으로 가정)
                AudioController.PlaySound(AudioController.AudioClips.chestOpen);
            }

            // 상자 개봉 이벤트를 발생시킵니다.
            OnChestOpenedEvent?.Invoke(this); // null 조건부 연산자 사용 (Unity 2023+ 문법)
        }
    }
}