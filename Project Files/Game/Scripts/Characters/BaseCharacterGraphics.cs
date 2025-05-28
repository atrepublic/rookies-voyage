// ==============================================
// BaseCharacterGraphics.cs
// ==============================================
// 캐릭터 외형, 애니메이션, 리그(Rig), 무기 장착 위치, 히트 애니메이션 등을 관리하는
// 모든 캐릭터 그래픽 클래스의 베이스 클래스입니다.
// 애니메이터 오버라이드, 래그돌, 파티클, 이동/사격 애니메이션 등을 처리합니다.

using UnityEngine;
using UnityEngine.Animations.Rigging;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public abstract class BaseCharacterGraphics : MonoBehaviour
    {
        private static readonly int PARTICLE_UPGRADE = "Upgrade".GetHashCode();

        private readonly int ANIMATION_SHOT_HASH = Animator.StringToHash("Shot");
        private readonly int ANIMATION_HIT_HASH = Animator.StringToHash("Hit");
        private readonly int JUMP_ANIMATION_HASH = Animator.StringToHash("Jump");
        private readonly int GRUNT_ANIMATION_HASH = Animator.StringToHash("Grunt");

        [Header("애니메이터")]
        [Tooltip("캐릭터 애니메이터")]
        [SerializeField] protected Animator characterAnimator;
        public Animator CharacterAnimator => characterAnimator;

        [Header("스킨드 메쉬 렌더러")]
        [Tooltip("캐릭터 모델의 메쉬 렌더러")]
        [SerializeField] protected SkinnedMeshRenderer meshRenderer;
        public SkinnedMeshRenderer MeshRenderer => meshRenderer;

        [Header("이동 설정")]
        [Tooltip("일반 이동 설정")]
        [SerializeField] protected MovementSettings movementSettings;
        public MovementSettings MovementSettings => movementSettings;

        [Tooltip("조준 상태 이동 설정")]
        [SerializeField] protected MovementSettings movementAimingSettings;
        public MovementSettings MovementAimingSettings => movementAimingSettings;

        [Header("양손 리그 설정")]
        [SerializeField] protected TwoBoneIKConstraint leftHandRig;
        public TwoBoneIKConstraint LeftHandRig => leftHandRig;

        [Tooltip("왼손 회전 보정값")]
        [SerializeField] protected Vector3 leftHandExtraRotation;
        public Vector3 LeftHandExtraRotation => leftHandExtraRotation;

        [SerializeField] protected TwoBoneIKConstraint rightHandRig;
        public TwoBoneIKConstraint RightHandRig => rightHandRig;

        [Tooltip("오른손 회전 보정값")]
        [SerializeField] protected Vector3 rightHandExtraRotation;
        public Vector3 RightHandExtraRotation => rightHandExtraRotation;

        [Header("무기 관련 트랜스폼")]
        [SerializeField] protected Transform weaponsTransform;

        [Tooltip("미니건 위치 홀더")]
        [SerializeField] protected Transform minigunHolderTransform;
        public Transform MinigunHolderTransform => minigunHolderTransform;

        [Tooltip("샷건 위치 홀더")]
        [SerializeField] protected Transform shootGunHolderTransform;
        public Transform ShootGunHolderTransform => shootGunHolderTransform;

        [Tooltip("로켓 위치 홀더")]
        [SerializeField] protected Transform rocketHolderTransform;
        public Transform RocketHolderTransform => rocketHolderTransform;

        [Tooltip("테슬라 위치 홀더")]
        [SerializeField] protected Transform teslaHolderTransform;
        public Transform TeslaHolderTransform => teslaHolderTransform;

        [Space]
        [SerializeField] protected Rig mainRig;
        [SerializeField] protected Transform leftHandController;
        [SerializeField] protected Transform rightHandController;

        protected CharacterBehaviour characterBehaviour;
        protected CharacterAnimationHandler animationHandler;

        protected Material characterMaterial;
        public Material CharacterMaterial => characterMaterial;

        private AnimatorOverrideController animatorOverrideController;
        private int animatorShootingLayerIndex;

        protected RagdollBehavior ragdoll;

        private TweenCase rigWeightCase;
        private TweenCase weaponsTransformCase;
        private TweenCase leftHandCase;
        private TweenCase rightHandCase;

        private Vector3 saveWeaponsPosition;
        private Vector3 saveLeftHandPosition;
        private Vector3 saveRightHandPosition;

        // 캐릭터 그래픽 초기화
        public virtual void Init(CharacterBehaviour characterBehaviour)
        {
            this.characterBehaviour = characterBehaviour;

            ragdoll = new RagdollBehavior();
            ragdoll.Init(characterAnimator.transform);

            animationHandler = characterAnimator.GetComponent<CharacterAnimationHandler>();
            animationHandler.Inititalise(characterBehaviour);

            animatorOverrideController = new AnimatorOverrideController(characterAnimator.runtimeAnimatorController);
            characterAnimator.runtimeAnimatorController = animatorOverrideController;

            characterMaterial = meshRenderer.sharedMaterial;

            animatorShootingLayerIndex = characterAnimator.GetLayerIndex("Shooting");
        }

        // 사격 애니메이션 오버라이드 지정
        public void SetShootingAnimation(AnimationClip animationClip)
        {
            animatorOverrideController["Shot"] = animationClip;
        }

        // 사격 시 호출되는 애니메이션 재생
        public void OnShoot()
        {
            characterAnimator.Play(ANIMATION_SHOT_HASH, animatorShootingLayerIndex, 0);
        }

        // 피격 시 히트 애니메이션 트리거
        public void PlayHitAnimation()
        {
            characterAnimator.SetTrigger(ANIMATION_HIT_HASH);
        }

        // 캐릭터 업그레이드 시 파티클 연출
        public void PlayUpgradeParticle()
        {
            ParticlesController.PlayParticle(PARTICLE_UPGRADE)
                .SetPosition(transform.position + new Vector3(0, -0.25f, -0.25f))
                .SetScale(Vector3.one * 5);
        }

        // 캐릭터 점프 애니메이션
        public void Jump()
        {
            characterAnimator.SetTrigger(JUMP_ANIMATION_HASH);
            rigWeightCase.KillActive();
            mainRig.weight = 0f;
        }

        // 캐릭터 움찔(grunt) 애니메이션 및 손 위치 흔들림
        public void Grunt()
        {
            characterAnimator.SetTrigger(GRUNT_ANIMATION_HASH);

            var strength = 0.1f;
            var durationIn = 0.1f;
            var durationOut = 0.15f;

            if (weaponsTransformCase.KillActive()) weaponsTransform.localPosition = saveWeaponsPosition;
            if (leftHandCase.KillActive()) leftHandController.localPosition = saveLeftHandPosition;
            if (rightHandCase.KillActive()) rightHandController.localPosition = saveRightHandPosition;

            saveWeaponsPosition = weaponsTransform.localPosition;
            saveLeftHandPosition = leftHandController.localPosition;
            saveRightHandPosition = rightHandController.localPosition;

            weaponsTransformCase = weaponsTransform.DOMoveY(weaponsTransform.position.y - strength, durationIn)
                .SetEasing(Ease.Type.SineOut)
                .OnComplete(() =>
                {
                    weaponsTransformCase = weaponsTransform.DOMoveY(weaponsTransform.position.y + strength, durationOut)
                        .SetEasing(Ease.Type.SineInOut);
                });

            leftHandCase = leftHandController.DOMoveY(leftHandController.position.y - strength, durationIn)
                .SetEasing(Ease.Type.SineOut)
                .OnComplete(() =>
                {
                    leftHandCase = leftHandController.DOMoveY(leftHandController.position.y + strength, durationOut)
                        .SetEasing(Ease.Type.SineInOut);
                });

            rightHandCase = rightHandController.DOMoveY(rightHandController.position.y - strength, durationIn)
                .SetEasing(Ease.Type.SineOut)
                .OnComplete(() =>
                {
                    rightHandCase = rightHandController.DOMoveY(rightHandController.position.y + strength, durationOut)
                        .SetEasing(Ease.Type.SineInOut);
                });
        }

        // 래그돌 활성화
        public void EnableRagdoll()
        {
            mainRig.weight = 0.0f;
            characterAnimator.enabled = false;

            if (characterBehaviour?.Weapon != null)
                characterBehaviour.Weapon.gameObject.SetActive(false);

            ragdoll?.ActivateWithForce(transform.position + transform.forward, 700, 100);
        }

        // 래그돌 비활성화 후 리그 복원
        public void DisableRagdoll()
        {
            ragdoll?.Disable();
            mainRig.weight = 1.0f;

            if (characterBehaviour?.Weapon != null)
                characterBehaviour.Weapon.gameObject.SetActive(true);

            characterAnimator.enabled = true;
        }

        // 리그 천천히 활성화 (부드럽게 연결)
        public void EnableRig()
        {
            rigWeightCase = Tween.DoFloat(0, 1, 0.2f, value => mainRig.weight = value);
        }

        // 캐릭터 등장 시 스케일 애니메이션 (Bounce)
        public void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        // 추상 함수 - 이동 시작 시 처리
        public abstract void OnMovingStarted();

        // 추상 함수 - 이동 멈춤 처리
        public abstract void OnMovingStoped();

        // 추상 함수 - 이동 시 애니메이션 동기화
        public abstract void OnMoving(float speedPercent, Vector3 direction, bool isTargetFound);

        // 추상 함수 - 캐릭터 사망 처리
        public virtual void OnDeath() { }

        // 추상 함수 - FixedUpdate 커스텀 처리
        public abstract void CustomFixedUpdate();

        // 추상 함수 - 캐릭터 리소스 해제
        public abstract void Unload();

        // 추상 함수 - 캐릭터 리소스 재초기화
        public abstract void Reload();

        // 추상 함수 - 캐릭터 비활성화
        public abstract void Disable();

        // 추상 함수 - 캐릭터 활성화
        public abstract void Activate();
    


    //애니메이션 모델을 씬이나 프로젝트에 배치했을 때,
    //일일이 손으로 리그 구성하거나 손 위치 조절용 오브젝트를 만드는 번거로움을 줄이기 위한 자동 세팅용 버튼.
    //이 코드는 런타임 시에는 아예 들어가지 않음 (UNITY_EDITOR 때문)
    //실질적으로는 모델 임포트 직후 한 번만 누르면 되는 초기화 툴.
    //이런 툴은 "리깅 템플릿 자동화", "IK 커스터마이즈" 등이 필요한 프로젝트에서 아주 유용하게 사용.

#if UNITY_EDITOR
[Button("Prepare Model")]
public void PrepareModel()
{
    // 1. 애니메이터 가져오기
    Animator tempAnimator = characterAnimator;

    if (tempAnimator != null)
    {
        // 2. 아바타가 휴머노이드 타입인지 확인
        if (tempAnimator.avatar != null && tempAnimator.avatar.isHuman)
        {
            // 3. RigBuilder 없으면 생성
            RigBuilder rigBuilder = tempAnimator.GetComponent<RigBuilder>();
            if (rigBuilder == null)
            {
                rigBuilder = tempAnimator.gameObject.AddComponent<RigBuilder>();

                // 3-1. 메인 리그 오브젝트 생성
                GameObject rigObject = new GameObject("Main Rig");
                rigObject.transform.SetParent(tempAnimator.transform);
                rigObject.transform.ResetLocal();

                Rig rig = rigObject.AddComponent<Rig>();
                mainRig = rig;

                // 리그 레이어 추가
                rigBuilder.layers.Add(new RigLayer(rig, true));

                // 4. 왼손 리그 구성
                GameObject leftHandRigObject = new GameObject("Left Hand Rig");
                leftHandRigObject.transform.SetParent(rigObject.transform);
                leftHandRigObject.transform.ResetLocal();

                GameObject leftHandControllerObject = new GameObject("Controller");
                leftHandControllerObject.transform.SetParent(leftHandRigObject.transform);
                leftHandControllerObject.transform.ResetLocal();

                leftHandController = leftHandControllerObject.transform;

                // 왼손 본 위치에 컨트롤러 위치 맞추기
                Transform leftHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                leftHandControllerObject.transform.position = leftHandBone.position;
                leftHandControllerObject.transform.rotation = leftHandBone.rotation;

                // 두본 IK 컴포넌트 추가
                TwoBoneIKConstraint leftHandRig = leftHandRigObject.AddComponent<TwoBoneIKConstraint>();
                leftHandRig.data.target = leftHandControllerObject.transform;
                leftHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                leftHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                leftHandRig.data.tip = leftHandBone;

                // 5. 오른손 리그 구성 (왼손과 동일 방식)
                GameObject rightHandRigObject = new GameObject("Right Hand Rig");
                rightHandRigObject.transform.SetParent(rigObject.transform);
                rightHandRigObject.transform.ResetLocal();

                GameObject rightHandControllerObject = new GameObject("Controller");
                rightHandControllerObject.transform.SetParent(rightHandRigObject.transform);
                rightHandControllerObject.transform.ResetLocal();

                rightHandController = rightHandControllerObject.transform;

                Transform rightHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.RightHand);
                rightHandControllerObject.transform.position = rightHandBone.position;
                rightHandControllerObject.transform.rotation = rightHandBone.rotation;

                TwoBoneIKConstraint rightHandRig = rightHandRigObject.AddComponent<TwoBoneIKConstraint>();
                rightHandRig.data.target = rightHandControllerObject.transform;
                rightHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                rightHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                rightHandRig.data.tip = rightHandBone;

                this.leftHandRig = leftHandRig;
                this.rightHandRig = rightHandRig;
            }

            // 6. 래그돌 자동 생성
            RagdollHelper.CreateRagdoll(tempAnimator, 60, 1, LayerMask.NameToLayer("Ragdoll"));

            // 7. 이동 관련 설정 초기값 지정
            movementSettings.RotationSpeed = 8;
            movementSettings.MoveSpeed = 5;
            movementSettings.Acceleration = 781.25f;
            movementSettings.AnimationMultiplier = new DuoFloat(0, 1.4f);

            movementAimingSettings.RotationSpeed = 8;
            movementAimingSettings.MoveSpeed = 4.375f;
            movementAimingSettings.Acceleration = 781.25f;
            movementAimingSettings.AnimationMultiplier = new DuoFloat(0, 1.2f);

            // 8. 애니메이션 핸들러가 없으면 자동 추가
            CharacterAnimationHandler tempAnimationHandler = tempAnimator.GetComponent<CharacterAnimationHandler>();
            if (tempAnimationHandler == null)
                tempAnimator.gameObject.AddComponent<CharacterAnimationHandler>();

            // 9. 무기 위치 홀더 생성
            GameObject weaponHolderObject = new GameObject("Weapons");
            weaponHolderObject.transform.SetParent(tempAnimator.transform);
            weaponHolderObject.transform.ResetLocal();
            weaponsTransform = weaponHolderObject.transform;

            // 각각의 무기 종류 위치 설정
            GameObject miniGunHolderObject = new GameObject("Minigun Holder");
            miniGunHolderObject.transform.SetParent(weaponsTransform);
            miniGunHolderObject.transform.ResetLocal();
            miniGunHolderObject.transform.localPosition = new Vector3(0.204f, 0.7f, 0.375f);
            minigunHolderTransform = miniGunHolderObject.transform;

            GameObject shotgunHolderObject = new GameObject("Shotgun Holder");
            shotgunHolderObject.transform.SetParent(weaponsTransform);
            shotgunHolderObject.transform.ResetLocal();
            shotgunHolderObject.transform.localPosition = new Vector3(0.22f, 0.6735f, 0.23f);
            shootGunHolderTransform = shotgunHolderObject.transform;

            GameObject rocketHolderObject = new GameObject("Rocket Holder");
            rocketHolderObject.transform.SetParent(weaponsTransform);
            rocketHolderObject.transform.ResetLocal();
            rocketHolderObject.transform.localPosition = new Vector3(0.234f, 0.726f, 0.369f);
            rocketHolderObject.transform.localRotation = Quaternion.Euler(-23.68f, -4.74f, 7.92f);
            rocketHolderTransform = rocketHolderObject.transform;

            GameObject teslaHolderObject = new GameObject("Tesla Holder");
            teslaHolderObject.transform.SetParent(weaponsTransform);
            teslaHolderObject.transform.ResetLocal();
            teslaHolderObject.transform.localPosition = new Vector3(0.213f, 0.783f, 0.357f);
            teslaHolderTransform = teslaHolderObject.transform;

            // 10. 스킨드 메쉬 렌더러 초기화
            meshRenderer = tempAnimator.transform.GetComponentInChildren<SkinnedMeshRenderer>();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogError("Avatar is missing or type isn't humanoid!");
        }
    }
    else
    {
        Debug.LogWarning("Animator component can't be found!");
    }
}
#endif

    }
}