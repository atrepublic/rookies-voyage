// ExitPointBehaviour.cs
// 이 스크립트는 레벨 출구 지점의 동작을 정의하는 추상 클래스입니다.
// 플레이어가 출구 영역에 진입했을 때의 로직을 처리하며, 상속받는 클래스에서 초기화, 활성화, 플레이어 진입 및 언로드 동작을 구현합니다.
using UnityEngine;

namespace Watermelon.LevelSystem
{
    // 이 스크립트가 첨부된 게임 오브젝트에 BoxCollider 컴포넌트가 필요함을 명시합니다.
    [RequireComponent(typeof(BoxCollider))]
    public abstract class ExitPointBehaviour : MonoBehaviour
    {
        // 출구 지점이 활성화되었는지 여부를 나타내는 변수
        [Tooltip("출구 지점이 현재 활성화 상태인지 나타냅니다.")] // isExitActivated 변수에 대한 툴팁
        protected bool isExitActivated;

        /// <summary>
        /// 오브젝트가 활성화될 때 호출됩니다.
        /// 현재 출구 지점을 ActiveRoom에 등록합니다.
        /// </summary>
        private void OnEnable()
        {
            // ActiveRoom 클래스에 현재 출구 지점을 등록하는 메서드 (ActiveRoom 클래스는 현재 코드에 포함되어 있지 않으므로 가정합니다.)
            ActiveRoom.RegisterExitPoint(this);
        }

        /// <summary>
        /// 출구 지점을 초기화합니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 출구 지점이 활성화될 때 호출됩니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        public abstract void OnExitActivated();

        /// <summary>
        /// 플레이어가 출구 지점 트리거 영역에 진입했을 때 호출됩니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        public abstract void OnPlayerEnteredExit();

        /// <summary>
        /// 출구 지점을 언로드하거나 정리할 때 호출됩니다. (상속받는 클래스에서 구현해야 합니다.)
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// 다른 콜라이더가 트리거 영역에 진입했을 때 호출됩니다.
        /// 출구 지점이 활성화 상태이고, 진입한 오브젝트가 플레이어 레이어에 속하는 경우 OnPlayerEnteredExit 메서드를 호출합니다.
        /// </summary>
        /// <param name="other">트리거 영역에 진입한 다른 콜라이더</param>
        private void OnTriggerEnter(Collider other)
        {
            // 출구가 활성화되지 않았다면 처리를 중단합니다.
            if (!isExitActivated)
                return;

            // 진입한 오브젝트의 레이어가 PhysicsHelper.LAYER_PLAYER와 같은지 확인합니다.
            // (PhysicsHelper 클래스와 LAYER_PLAYER 상수는 현재 코드에 포함되어 있지 않으므로 가정합니다.)
            if (other.gameObject.layer.Equals(PhysicsHelper.LAYER_PLAYER))
            {
                // 플레이어가 출구에 진입했을 때의 로직을 실행합니다.
                OnPlayerEnteredExit();
            }
        }

        /// <summary>
        /// 다른 콜라이더가 트리거 영역 안에 머물러 있는 동안 호출됩니다.
        /// 출구 지점이 활성화 상태이고, 영역 안에 있는 오브젝트가 플레이어 레이어에 속하는 경우 OnPlayerEnteredExit 메서드를 호출합니다.
        /// </summary>
        /// <param name="other">트리거 영역 안에 머물러 있는 다른 콜라이더</param>
        private void OnTriggerStay(Collider other)
        {
            // 출구가 활성화되지 않았다면 처리를 중단합니다.
            if (!isExitActivated)
                return;

            // 영역 안에 있는 오브젝트의 레이어가 PhysicsHelper.LAYER_PLAYER와 같은지 확인합니다.
            // (PhysicsHelper 클래스와 LAYER_PLAYER 상수는 현재 코드에 포함되어 있지 않으므로 가정합니다.)
            if (other.gameObject.layer.Equals(PhysicsHelper.LAYER_PLAYER))
            {
                // 플레이어가 출구에 머물러 있는 동안의 로직을 실행합니다. (필요에 따라 OnTriggerEnter와 다르게 구현될 수 있습니다.)
                OnPlayerEnteredExit();
            }
        }
    }
}