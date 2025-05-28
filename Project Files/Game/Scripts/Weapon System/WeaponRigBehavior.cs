// 이 스크립트는 무기가 캐릭터의 특정 본(bone)에 부착되어 위치 및 회전을 따라가도록 하는 동작을 제어합니다.
// 한 손 또는 양 손 무기에 따라 다른 리그(rig) 방식을 적용하며, 에디터에서의 디버깅 기능도 포함합니다.
using UnityEngine;
using UnityEngine.UIElements; // ShowIf, Button 등의 UIElements 관련 속성을 사용하기 위해 필요할 수 있습니다. 실제 작동은 별개의 패키지나 커스텀 에디터 스크립트에 따라 다를 수 있습니다.

namespace Watermelon.SquadShooter
{
    // ExecuteAlways 속성은 플레이 모드 또는 에디터 모드에서 스크립트가 실행되도록 합니다.
    [ExecuteAlways]
    public class WeaponRigBehavior : MonoBehaviour
    {
        [Tooltip("무기 리그의 타입입니다 (한 손 또는 양 손).")]
        [SerializeField] WeaponRigType rigType;
        [Tooltip("무기를 잡는 주 손의 타입입니다 (왼손 또는 오른손).")]
        [SerializeField] PrimaryHandType primaryHandType;

        [Tooltip("주 손에 부착될 무기 내의 앵커(Anchor) 트랜스폼입니다.")]
        [SerializeField] Transform primaryHandAnchor;
        [Tooltip("보조 손에 부착될 무기 내의 앵커(Anchor) 트랜스폼입니다 (양 손 무기일 때만 사용).")]
        [SerializeField, ShowIf("NeedsOffHand")] Transform offHandAnchor;

        [Space]
        [Tooltip("양 손 무기일 때, 보조 손의 위치를 무시하고 주 손에만 무기를 고정할지 여부입니다.")]
        [SerializeField] bool stickToPrimaryHand;

        [Tooltip("주 손에 해당하는 캐릭터의 본(Bone) 트랜스폼입니다. 에디터에서 숨겨집니다.")]
        [SerializeField, HideInInspector] Transform primaryHandBone;
        [Tooltip("보조 손에 해당하는 캐릭터의 본(Bone) 트랜스폼입니다. 에디터에서 숨겨집니다.")]
        [SerializeField, HideInInspector] Transform offHandBone;

        [Header("개발")]
        [Tooltip("에디터에서 플레이 모드 없이 무기 리그 동작을 활성화할지 여부입니다.")]
        [SerializeField] bool enableRigWeaponInEditor = true;

        [Tooltip("무기가 장착된 적(Enemy) 행동 컴포넌트입니다. 에디터에서 숨겨집니다.")]
        [SerializeField, HideInInspector] BaseEnemyBehavior enemy;

        /// <summary>
        /// 무기 리그 동작을 초기화하고 캐릭터의 손 본을 설정합니다.
        /// </summary>
        /// <param name="enemy">무기가 장착될 적(Enemy) 객체</param>
        public void Init(BaseEnemyBehavior enemy)
        {
            this.enemy = enemy;

            // 주 손 타입에 따라 주 손과 보조 손 본을 설정합니다.
            if (primaryHandType == PrimaryHandType.Right)
            {
                primaryHandBone = enemy.RightHandBone;
                offHandBone = enemy.LeftHandBone;
            }
            else
            {
                primaryHandBone = enemy.LeftHandBone;
                offHandBone = enemy.RightHandBone;
            }
        }

        // 매 프레임 업데이트 동안 호출됩니다.
        private void Update()
        {
            // 플레이 모드가 아니거나 에디터에서의 리그 활성화가 꺼져 있으면 업데이트를 중단합니다.
            if (!(Application.isPlaying || enableRigWeaponInEditor)) return;

            // 필수 트랜스폼이 할당되지 않았으면 업데이트를 중단합니다.
            if (primaryHandBone == null || primaryHandAnchor == null) return;
            // 양 손 무기이고 보조 손 관련 트랜스폼이 할당되지 않았으면 업데이트를 중단합니다.
            if (rigType == WeaponRigType.TwoHanded && (offHandBone == null || offHandAnchor == null)) return;

            // 무기 리그 타입에 따라 적절한 업데이트 함수를 호출합니다.
            if (rigType == WeaponRigType.OneHanded)
            {
                OneHandedUpdate();
            }
            else
            {
                TwoHandedUpdate();
            }
        }

        /// <summary>
        /// 한 손 무기의 위치 및 회전을 업데이트합니다.
        /// 주 손 본의 회전을 따라가도록 무기의 회전을 조정하고, 주 손 본의 위치에 맞게 무기를 이동시킵니다.
        /// </summary>
        private void OneHandedUpdate()
        {
            // 주 손 본의 목표 회전을 가져옵니다.
            Quaternion desiredRotation = primaryHandBone.rotation;

            // 목표 회전과 주 손 앵커의 현재 로컬 회전 차이를 계산하여 무기의 회전 보정값을 얻습니다.
            Quaternion rotationCorrection = desiredRotation * Quaternion.Inverse(primaryHandAnchor.localRotation);

            // 무기의 회전을 보정된 회전으로 설정합니다.
            transform.rotation = rotationCorrection;

            // 주 손 본과 주 손 앵커의 위치 차이를 계산하여 무기의 위치 보정값을 얻습니다.
            Vector3 positionCorrection = primaryHandBone.position - primaryHandAnchor.position;

            // 무기의 현재 위치에 위치 보정값을 더하여 최종 위치를 설정합니다.
            transform.position = transform.position + positionCorrection;
        }

        /// <summary>
        /// 양 손 무기의 위치 및 회전을 업데이트합니다.
        /// 두 손 본의 위치를 기반으로 무기의 위치와 방향을 계산합니다.
        /// </summary>
        private void TwoHandedUpdate()
        {
            // 무기 자체의 스케일을 계산합니다.
            var parent = transform;
            var scale = Vector3.one;
            while (parent != null)
            {
                scale = scale.Mult(parent.localScale);
                parent = parent.parent;
            }

            // 주 손에 무기를 고정할지, 두 손의 중간에 위치시킬지 결정합니다.
            if (stickToPrimaryHand)
            {
                // 주 손 본의 위치에서 주 손 앵커의 로컬 위치를 무기의 회전과 스케일을 적용하여 뺀 위치로 무기를 이동시킵니다.
                transform.position = primaryHandBone.position - transform.rotation * primaryHandAnchor.localPosition.Mult(scale);
            }
            else
            {
                // 무기 내의 두 앵커의 중간 위치를 계산합니다.
                var middleBetweenAnchors = (primaryHandAnchor.localPosition + offHandAnchor.localPosition) / 2;
                // 캐릭터의 두 손 본의 중간 위치를 계산합니다.
                var middleBetweenHands = (offHandBone.position + primaryHandBone.position) / 2;

                // 두 손 본의 중간 위치에서 무기 내 두 앵커의 중간 위치를 무기의 회전과 스케일을 적용하여 뺀 위치로 무기를 이동시킵니다.
                transform.position = middleBetweenHands - transform.rotation * middleBetweenAnchors.Mult(scale);
            }

            // 두 손 본 사이의 방향 벡터를 계산합니다.
            var direction = (primaryHandBone.position - offHandBone.position).normalized;
            // 무기의 상향 벡터를 기본적으로 Vector3.up으로 설정합니다.
            var up = Vector3.up;
            // 두 손 본 사이의 방향과 상향 벡터를 사용하여 무기의 회전을 설정합니다.
            transform.rotation = Quaternion.LookRotation(direction, up);
        }

        /// <summary>
        /// 무기 내의 앵커 위치와 회전을 캐릭터의 해당 손 본의 위치 및 회전으로 리셋합니다.
        /// 에디터에서 앵커 위치를 설정하는 데 유용합니다.
        /// </summary>
        [Button("Reset Anchor Position")]
        public void ResetAnchorPosition()
        {
            // 적(Enemy)이 할당되지 않았으면 오류를 로깅합니다.
            if (enemy == null)
            {
                Debug.LogError("The weapon is not assigned to an enemy!");
                return;
            }

            // 주 손 본이 할당되지 않았으면 오류를 로깅합니다.
            if (primaryHandBone == null)
            {
                Debug.LogError($"The enemy does not have the {primaryHandType} bone assigned!");
                return;
            }

            // 주 손 앵커 트랜스폼이 할당되지 않았으면 오류를 로깅합니다.
            if (primaryHandAnchor == null)
            {
                Debug.LogError("The weapon does not have 'Primary Hand Anchor' transform assigned!");
                return;
            }

            // 양 손 무기인 경우 보조 손 본 및 앵커 할당 상태를 추가로 확인합니다.
            if (rigType == WeaponRigType.TwoHanded)
            {
                // 보조 손 본이 할당되지 않았으면 오류를 로깅합니다.
                if (offHandBone == null)
                {
                    var offHand = primaryHandType == PrimaryHandType.Right ? "Left" : "Right";
                    Debug.LogError($"The enemy does not have the {offHand} bone assigned!");

                    return;
                }

                // 보조 손 앵커 트랜스폼이 할당되지 않았으면 오류를 로깅합니다.
                if (offHandAnchor == null)
                {
                    Debug.LogError("The weapon does not have 'Off Hand Anchor' transform assigned!");
                    return;
                }
            }

            // 주 손 앵커의 위치와 회전을 주 손 본과 동일하게 설정합니다.
            primaryHandAnchor.position = primaryHandBone.position;
            primaryHandAnchor.rotation = primaryHandBone.rotation;

            // 양 손 무기인 경우 보조 손 앵커의 위치와 회전도 보조 손 본과 동일하게 설정합니다.
            if (rigType == WeaponRigType.TwoHanded)
            {
                offHandAnchor.position = offHandBone.position;
                offHandAnchor.rotation = offHandBone.rotation;
            }
        }

        // 에디터에서 스크립트의 인스턴스가 로드되거나 값이 변경될 때 호출됩니다.
        private void OnValidate()
        {
            // 적(Enemy)이 할당되어 있으면 Init 함수를 호출하여 초기화합니다.
            if (enemy != null) Init(enemy);
        }

        /// <summary>
        /// 보조 손 앵커가 필요한지 (양 손 무기인지) 여부를 반환합니다.
        /// </summary>
        /// <returns>양 손 무기이면 true, 아니면 false</returns>
        private bool NeedsOffHand()
        {
            return rigType == WeaponRigType.TwoHanded;
        }

        // 무기 리그의 타입을 정의하는 열거형입니다.
        public enum WeaponRigType
        {
            OneHanded, // 한 손 무기
            TwoHanded, // 양 손 무기
        }

        // 주 손의 타입을 정의하는 열거형입니다.
        public enum PrimaryHandType
        {
            Left, // 왼손
            Right, // 오른손
        }
    }
}