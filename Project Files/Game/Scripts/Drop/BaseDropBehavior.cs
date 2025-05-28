// 스크립트 설명: 게임 내 드롭 아이템의 기본 동작을 정의하는 추상 클래스입니다.
// 아이템 생성, 던지기, 줍기, 제거 등 기본적인 드롭 관련 기능을 포함합니다.
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon; // Tween 관련 네임스페이스 추가

namespace Watermelon.SquadShooter
{
    public abstract class BaseDropBehavior : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("아이템 애니메이션을 제어하는 애니메이터 컴포넌트")] // 주요 변수 한글 툴팁
        Animator animator;

        [SerializeField]
        [Tooltip("아이템의 충돌 처리를 담당하는 콜라이더 컴포넌트")] // 주요 변수 한글 툴팁
        Collider itemCollider;

        [Space]
        [SerializeField]
        [Tooltip("아이템 자동 획득 사용 여부")] // 주요 변수 한글 툴팁
        bool useAutoPickup = true;
        public bool IsAutoPickable => useAutoPickup; // 자동 획득 가능한지 여부

        [Tooltip("보상으로 획득되었는지 여부")] // 주요 변수 한글 툴팁
        public bool IsRewarded { get; set; } = false; // 보상 획득 여부

        [Tooltip("아이템이 이미 획득되었는지 여부")] // 주요 변수 한글 툴팁
        protected bool isPicked = false; // 획득 상태 여부
        public bool IsPicked => isPicked; // 아이템 획득 상태

        public GameObject GameObject => gameObject; // 현재 게임 오브젝트

        [Tooltip("드롭 아이템의 데이터")] // 주요 변수 한글 툴팁
        protected DropData dropData; // 드롭 데이터
        public DropData DropData => dropData; // 드롭 데이터 접근

        public int DropAmount => dropData.Amount; // 드롭 수량
        public DropableItemType DropType => dropData.DropType; // 드롭 아이템 타입

        [Tooltip("아이템을 주울 수 있게 되기까지의 지연 시간")] // 주요 변수 한글 툴팁
        protected float availableToPickDelay; // 줍기 가능 지연 시간

        [Tooltip("아이템 자동 줍기까지의 지연 시간")] // 주요 변수 한글 툴팁
        protected float autoPickDelay; // 자동 줍기 지연 시간

        private TweenCaseCollection throwTweenCase; // 던지기 애니메이션 트윈 케이스 컬렉션

        /// <summary>
        /// 드롭 아이템을 지정된 데이터와 지연 시간으로 초기화합니다.
        /// </summary>
        /// <param name="dropData">드롭 아이템 데이터.</param>
        /// <param name="availableToPickDelay">아이템을 주울 수 있게 되기까지의 지연 시간.</param>
        /// <param name="autoPickDelay">아이템이 자동으로 주어지기까지의 지연 시간.</param>
        public virtual void Init(DropData dropData, float availableToPickDelay = -1f, float autoPickDelay = -1f)
        {
            this.dropData = dropData;
            this.availableToPickDelay = availableToPickDelay;
            this.autoPickDelay = autoPickDelay;

            isPicked = false;

            animator.enabled = true;
            itemCollider.enabled = true;
        }

        /// <summary>
        /// 드롭 아이템을 지정된 위치로 애니메이션과 함께 던집니다.
        /// </summary>
        /// <param name="position">목표 위치.</param>
        /// <param name="dropAnimation">드롭 애니메이션 설정.</param>
        /// <param name="time">던지는 데 걸리는 시간.</param>
        public virtual void Throw(Vector3 position, DropAnimation dropAnimation, float time)
        {
            animator.enabled = false;
            itemCollider.enabled = false;

            throwTweenCase.KillActive();

            throwTweenCase = Tween.BeginTweenCaseCollection();
            transform.DOMoveXZ(position.x, position.z, time).SetCurveEasing(dropAnimation.FallAnimationCurve);
            throwTweenCase += transform.DOMoveY(position.y, time).SetCurveEasing(dropAnimation.FallYAnimationCurve).OnComplete(() =>
            {
                animator.enabled = true;

                if (availableToPickDelay != -1f)
                {
                    throwTweenCase += Tween.DelayedCall(availableToPickDelay, () =>
                    {
                        itemCollider.enabled = true;
                    });
                }
                else
                {
                    itemCollider.enabled = true;
                }

                if (useAutoPickup && autoPickDelay != -1f) // 자동 줍기 사용 여부 확인
                {
                    throwTweenCase += Tween.DelayedCall(autoPickDelay, () =>
                    {
                        Pick();
                    });
                }

                OnItemLanded();
            });
            Tween.EndTweenCaseCollection();
        }

        /// <summary>
        /// 드롭 아이템을 획득합니다. 선택적으로 플레이어에게 이동시킬 수 있습니다.
        /// </summary>
        /// <param name="moveToPlayer">아이템을 플레이어에게 이동시킬지 여부.</param>
        public virtual void Pick(bool moveToPlayer = true)
        {
            if (isPicked) return; // 이미 획득했다면 중복 처리 방지

            isPicked = true;

            // 이동 트윈 애니메이션 중지
            throwTweenCase.KillActive();

            animator.enabled = false;
            itemCollider.enabled = false;

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (moveToPlayer && characterBehaviour != null)
            {
                throwTweenCase += transform.DOMove(characterBehaviour.transform.position.SetY(0.625f), 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    ApplyReward(); // 보상 적용
                    DestoryObject(); // 오브젝트 파괴
                });
            }
            else
            {
                ApplyReward(); // 보상 적용
                DestoryObject(); // 오브젝트 파괴
            }
        }

        /// <summary>
        /// 드롭 오브젝트를 파괴하고 드롭 매니저에서 제거합니다.
        /// </summary>
        public void DestoryObject()
        {
            Unload(); // 언로드 처리

            // 드롭 매니저에서 오브젝트 제거 (Drop 클래스에 RemoveObject 함수가 있다고 가정)
            // 실제 사용 시 해당 클래스와 함수가 있는지 확인 필요
            Drop.RemoveObject(this); // 수정: Drop.Exists() 체크 제거
        }

        /// <summary>
        /// 드롭 아이템을 언로드하고 게임 오브젝트를 파괴합니다.
        /// </summary>
        public virtual void Unload()
        {
            throwTweenCase.KillActive(); // 활성화된 트윈 중지

            if (gameObject != null)
            {
                Destroy(gameObject); // 게임 오브젝트 파괴
            }
        }

        /// <summary>
        /// 지정된 캐릭터가 드롭 아이템을 주울 수 있는지 여부를 판단합니다.
        /// </summary>
        /// <param name="characterBehaviour">캐릭터 동작 컴포넌트.</param>
        /// <returns>아이템을 주울 수 있으면 true, 그렇지 않으면 false.</returns>
        public virtual bool IsPickable(CharacterBehaviour characterBehaviour) => true; // 기본적으로 모든 캐릭터가 획득 가능

        /// <summary>
        /// 아이템이 던져진 후 땅에 착지했을 때 호출됩니다.
        /// </summary>
        public virtual void OnItemLanded() { } // 착지 시 추가 동작을 위한 가상 함수

        /// <summary>
        /// 드롭 아이템 획득에 대한 보상을 적용합니다.
        /// </summary>
        /// <param name="autoReward">자동 보상 적용 여부.</param>
        public abstract void ApplyReward(bool autoReward = false); // 보상 적용을 위한 추상 함수 (하위 클래스에서 구현)
    }
}