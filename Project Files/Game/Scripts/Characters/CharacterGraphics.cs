// CharacterGraphics.cs
// 이 스크립트는 캐릭터의 시각적인 표현 및 애니메이션을 관리하는 MonoBehaviour입니다.
// BaseCharacterGraphics를 상속받아 캐릭터 이동 및 타겟팅 상태에 따른 애니메이션을 제어하고,
// 애니메이터 파라미터를 업데이트하는 기능을 수행합니다.

using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 캐릭터 그래픽스 및 애니메이션 관리를 위한 클래스입니다.
    public class CharacterGraphics : BaseCharacterGraphics
    {
        // 애니메이터 "Speed" 파라미터의 해시 값입니다.
        private static readonly int ANIMATOR_MOVEMENT_SPEED = Animator.StringToHash("Speed");

        // 애니메이터 "IsRunning" 파라미터의 해시 값입니다.
        private static readonly int ANIMATOR_RUNNING_HASH = Animator.StringToHash("IsRunning");
        // 애니메이터 "MovementX" 파라미터의 해시 값입니다.
        private static readonly int ANIMATOR_MOVEMENT_X_HASH = Animator.StringToHash("MovementX");
        // 애니메이터 "MovementY" 파라미터의 해시 값입니다.
        private static readonly int ANIMATOR_MOVEMENT_Y_HASH = Animator.StringToHash("MovementY");

        // 적 캐릭터의 현재 위치입니다.
        private Vector3 enemyPosition;
        // 캐릭터 회전 각도입니다.
        private float angle;
        // 입력 방향이 캐릭터 회전에 맞춰 변환된 벡터입니다.
        private Vector2 rotatedInput;

        /// <summary>
        /// MonoBehaviour 인스턴스가 로드될 때 호출됩니다.
        /// 현재 구현에서는 특별한 초기화 로직이 없습니다.
        /// </summary>
        private void Awake()
        {
            // 이 함수는 MonoBehaviour 초기화 시 호출됩니다.
        }

        /// <summary>
        /// 캐릭터 이동이 시작될 때 호출되는 오버라이드 함수입니다.
        /// 달리기 애니메이션을 활성화합니다.
        /// </summary>
        public override void OnMovingStarted()
        {
            // 애니메이터의 "IsRunning" 불리언 파라미터를 true로 설정하여 달리기 애니메이션을 시작합니다.
            characterAnimator.SetBool(ANIMATOR_RUNNING_HASH, true);
        }

        /// <summary>
        /// 캐릭터 이동이 멈출 때 호출되는 오버라이드 함수입니다.
        /// 달리기 애니메이션을 비활성화합니다.
        /// </summary>
        public override void OnMovingStoped()
        {
            // 애니메이터의 "IsRunning" 불리언 파라미터를 false로 설정하여 달리기 애니메이션을 멈춥니다.
            characterAnimator.SetBool(ANIMATOR_RUNNING_HASH, false);
        }

        /// <summary>
        /// 캐릭터가 이동 중일 때 매 프레임 호출되는 오버라이드 함수입니다.
        /// 이동 속도 및 방향에 따라 애니메이터 파라미터를 업데이트하고,
        /// 타겟이 있는 경우 타겟 방향에 맞춰 이동 애니메이션을 제어합니다.
        /// </summary>
        /// <param name="speedPercent">최대 이동 속도에 대한 현재 속도의 백분율 (0.0f ~ 1.0f)</param>
        /// <param name="direction">캐릭터의 현재 이동 방향 벡터</param>
        /// <param name="isTargetFound">현재 공격 타겟을 찾았는지 여부</param>
        public override void OnMoving(float speedPercent, Vector3 direction, bool isTargetFound)
        {
            // 애니메이터의 "Speed" 플로트 파라미터를 현재 이동 속도 백분율에 맞춰 업데이트합니다.
            // characterBehaviour.MovementSettings.AnimationMultiplier를 사용하여 속도에 가중치를 적용할 수 있습니다.
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_SPEED, characterBehaviour.MovementSettings.AnimationMultiplier.Lerp(speedPercent));

            // 타겟을 찾았으면
            if (isTargetFound)
            {
                // 가장 가까운 적의 위치를 가져옵니다.
                enemyPosition = characterBehaviour.ClosestEnemyBehaviour.transform.position;

                // 캐릭터와 적 사이의 각도를 계산합니다. (Y축 기준)
                angle = Mathf.Atan2(enemyPosition.x - transform.position.x, enemyPosition.z - transform.position.z) * 180 / Mathf.PI;

                // 현재 이동 방향을 캐릭터의 전방 방향(적을 바라보는 방향)에 맞춰 회전시킵니다.
                rotatedInput = Quaternion.Euler(0, 0, angle) * new Vector2(direction.x, direction.z);

                // 회전된 입력 방향을 애니메이터의 "MovementX", "MovementY" 파라미터에 설정하여 이동 애니메이션을 제어합니다.
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_X_HASH, rotatedInput.x);
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_Y_HASH, rotatedInput.y);
            }
            else // 타겟을 찾지 못했으면
            {
                // 타겟이 없으면 제자리 걸음 애니메이션(MovementX=0, MovementY=1)을 재생합니다.
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_X_HASH, 0);
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_Y_HASH, 1);
            }
        }

        /// <summary>
        /// FixedUpdate에서 호출되는 커스텀 함수입니다.
        /// 현재 구현에서는 특별한 로직이 없습니다.
        /// </summary>
        public override void CustomFixedUpdate()
        {
            // 물리 업데이트 주기에 맞춰 호출되는 함수입니다.
        }

        /// <summary>
        /// 캐릭터 그래픽 리소스가 언로드될 때 호출되는 오버라이드 함수입니다.
        /// 현재 구현에서는 특별한 로직이 없습니다.
        /// </summary>
        public override void Unload()
        {
            // 캐릭터 리소스 언로드 시 필요한 정리 로직을 구현할 수 있습니다.
        }

        /// <summary>
        /// 캐릭터 그래픽 리소스가 다시 로드될 때 호출되는 오버라이드 함수입니다.
        /// 이동 애니메이션을 멈추도록 설정합니다.
        /// </summary>
        public override void Reload()
        {
            // 캐릭터 리소스 로드 완료 후 이동 애니메이션을 멈춥니다.
            StopMovementAnimation();
        }

        /// <summary>
        /// 캐릭터 그래픽이 활성화될 때 호출되는 오버라이드 함수입니다.
        /// 이동 애니메이션을 멈추도록 설정합니다.
        /// </summary>
        public override void Activate()
        {
            // 캐릭터 활성화 후 이동 애니메이션을 멈춥니다.
            StopMovementAnimation();
        }

        /// <summary>
        /// 캐릭터 그래픽이 비활성화될 때 호출되는 오버라이드 함수입니다.
        /// 이동 애니메이션을 멈추도록 설정합니다.
        /// </summary>
        public override void Disable()
        {
            // 캐릭터 비활성화 후 이동 애니메이션을 멈춥니다.
            StopMovementAnimation();
        }

        /// <summary>
        /// 캐릭터의 이동 관련 애니메이션을 멈추고 기본 상태로 되돌리는 함수입니다.
        /// 애니메이터 파라미터 "Speed", "MovementX", "MovementY"를 기본값으로 설정합니다.
        /// </summary>
        private void StopMovementAnimation()
        {
            // 애니메이터 "Speed" 파라미터를 1.0으로 설정합니다. (기본 속도 상태)
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_SPEED, 1.0f);

            // 애니메이터 "MovementX", "MovementY" 파라미터를 0으로 설정하여 이동 애니메이션을 멈춥니다.
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_X_HASH, 0);
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_Y_HASH, 0);
        }
    }
}