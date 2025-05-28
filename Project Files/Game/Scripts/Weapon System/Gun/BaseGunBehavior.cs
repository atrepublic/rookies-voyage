// 이 스크립트는 게임 내 모든 총기류의 기본 동작을 정의하는 추상 클래스입니다.
// 총기의 초기화, 캐릭터 장착, 애니메이션, 데미지 계산, 업그레이드 파티클 효과 등을 처리합니다.
// 구체적인 총기 타입(샷건, 미니건 등)은 이 클래스를 상속받아 고유한 발사/재장전 로직을 구현합니다.
using UnityEngine;
using Watermelon; // Watermelon 네임스페이스의 다른 기능(예: ParticlesController, Tween)을 사용하기 위해 필요합니다.
// using DG.Tweening; 네임스페이스는 DOTween 플러그인 사용에 필요하지만, 현재 코드에 명시적으로 포함되어 있지 않습니다.
// Tweening 관련 기능(DOScale, SetEasing 등)은 해당 플러그인이 프로젝트에 추가되어 있음을 가정합니다.
// using UnityEditor; 와 Button 속성은 에디터 스크립팅에 사용되며, 게임 빌드 시에는 포함되지 않습니다. 개발 편의를 위한 기능입니다.


namespace Watermelon.SquadShooter
{

    
    // BaseGunBehavior는 총기류의 기본 동작을 위한 추상 클래스입니다.
    public abstract class BaseGunBehavior : MonoBehaviour
    {
        // 총기 업그레이드 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_UPGRADE = "Gun Upgrade".GetHashCode();

        [Header("애니메이션")]
        [Tooltip("이 총기를 장착한 캐릭터가 발사할 때 재생될 애니메이션 클립입니다.")]
        [SerializeField] AnimationClip characterShootAnimation;

        [Space]
        [Tooltip("캐릭터의 손이 총기를 잡을 위치 정보를 담고 있는 GunHolder 객체입니다.")]
        [SerializeField] GunHolder gunHolder;

        [Space]
        [Tooltip("투사체(총알)가 발사될 위치를 나타내는 트랜스폼입니다.")]
        [SerializeField]
        protected Transform shootPoint; // 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.

        [Header("업그레이드")]
        [Tooltip("총기 업그레이드 파티클이 재생될 위치의 오프셋입니다.")]
        [SerializeField] Vector3 upgradeParticleOffset;
        [Tooltip("총기 업그레이드 파티클의 크기입니다.")]
        [SerializeField] float upgradeParticleSize = 1.0f;

        // 이 총기를 장착한 캐릭터 행동 컴포넌트입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected CharacterBehaviour characterBehaviour;
        // 이 총기의 무기 데이터 ScriptableObject입니다. 상속받는 클래스에서 접근 가능하도록 protected로 선언되었습니다.
        protected WeaponData weapon;

        // 이 총기의 현재 데미지 값 (최소/최대 값)입니다.
        protected DuoInt damage; // DuoInt는 두 개의 정수 값을 저장하는 사용자 정의 구조체일 수 있습니다.
        // 현재 데미지 값에 대한 읽기 전용 접근을 제공하는 프로퍼티입니다.
        public DuoInt Damage => damage;

        // 캐릭터의 왼손 리그(Rig) 컨트롤러 트랜스폼입니다.
        private Transform leftHandRigController;
        // 캐릭터의 왼손에 적용될 추가 회전 값입니다.
        private Vector3 leftHandExtraRotation;

        // 캐릭터의 오른손 리그(Rig) 컨트롤러 트랜스폼입니다.
        private Transform rightHandRigController;
        // 캐릭터의 오른손에 적용될 추가 회전 값입니다.
        private Vector3 rightHandExtraRotation;

        // 현재 캐릭터에 적용될 총기 잡는 위치 정보 데이터입니다.
        private GunHolder.HolderData activeHolderData;

        /// <summary>
        /// 총기 동작을 초기화합니다.
        /// 캐릭터 행동 및 무기 데이터를 할당합니다.
        /// </summary>
        /// <param name="characterBehaviour">총기를 장착할 캐릭터 행동 컴포넌트</param>
        /// <param name="data">이 총기의 무기 데이터</param>
        public virtual void Init(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            // 전달받은 캐릭터 행동 및 무기 데이터를 할당합니다.
            this.characterBehaviour = characterBehaviour;
            this.weapon = data;
        }

        /// <summary>
        /// 총기를 장착할 캐릭터 그래픽스 컴포넌트를 사용하여 총기와 캐릭터 손 리그를 설정합니다.
        /// </summary>
        /// <param name="characterGraphics">총기를 장착할 캐릭터 그래픽스 컴포넌트</param>
        public void InitCharacter(BaseCharacterGraphics characterGraphics)
        {
            // 캐릭터 그래픽스에서 왼손 및 오른손 리그 컨트롤러 트랜스폼을 가져옵니다.
            leftHandRigController = characterGraphics.LeftHandRig.data.target;
            rightHandRigController = characterGraphics.RightHandRig.data.target;

            // 캐릭터 그래픽스에서 왼손 및 오른손의 추가 회전 값을 가져옵니다.
            leftHandExtraRotation = characterGraphics.LeftHandExtraRotation;
            rightHandExtraRotation = characterGraphics.RightHandExtraRotation;

            // 캐릭터 그래픽스에 총기 발사 애니메이션 클립을 설정합니다.
            characterGraphics.SetShootingAnimation(characterShootAnimation);

            // 현재 선택된 캐릭터 데이터를 가져와서 해당 캐릭터에 맞는 총기 잡는 위치 데이터를 가져옵니다.
            CharacterData character = CharactersController.SelectedCharacter;
            activeHolderData = gunHolder.GetHolderData(character);
        }

        /// <summary>
        /// 레벨이 로드될 때 호출될 수 있는 가상 함수입니다.
        /// 기본적으로 데미지 값을 다시 계산합니다.
        /// </summary>
        public virtual void OnLevelLoaded()
        {
            // 현재 무기 데이터의 강화 상태에 따라 데미지 값을 다시 계산합니다.
            RecalculateDamage();
        }

        /// <summary>
        /// 매 프레임 총기 동작을 업데이트하는 가상 함수입니다.
        /// 구체적인 총기 타입에서 발사 로직 등을 구현하는 데 사용됩니다.
        /// </summary>
        public virtual void GunUpdate()
        {
            // 기본 구현은 비어 있습니다.
        }

        /// <summary>
        /// 캐릭터의 손 리그(Rig) 위치 및 회전을 총기의 잡는 위치에 맞게 업데이트합니다.
        /// </summary>
        public void UpdateHandRig()
        {
            // 캐릭터의 손 리그 컨트롤러 위치를 총기의 잡는 위치와 동일하게 설정합니다.
            leftHandRigController.position = activeHolderData.LeftHandHolder.position;
            rightHandRigController.position = activeHolderData.RightHandHolder.position;

#if UNITY_EDITOR // 에디터에서만 실행되는 코드 블록입니다.
            // 에디터에서 캐릭터 그래픽스가 변경될 경우 추가 회전 값을 다시 가져옵니다.
            if (characterBehaviour != null && characterBehaviour.Graphics != null)
            {
                leftHandExtraRotation = characterBehaviour.Graphics.LeftHandExtraRotation;
                rightHandExtraRotation = characterBehaviour.Graphics.RightHandExtraRotation;
            }
#endif

            // 캐릭터의 손 리그 컨트롤러 회전을 총기의 잡는 위치 회전에 추가 회전 값을 더하여 설정합니다.
            leftHandRigController.rotation = Quaternion.Euler(activeHolderData.LeftHandHolder.eulerAngles + leftHandExtraRotation);
            rightHandRigController.rotation = Quaternion.Euler(activeHolderData.RightHandHolder.eulerAngles + rightHandExtraRotation);
        }

        /// <summary>
        /// 총기를 재장전하는 추상 함수입니다.
        /// 구체적인 총기 동작 클래스에서 반드시 구현해야 합니다.
        /// </summary>
        public abstract void Reload();
        /// <summary>
        /// 총기가 해제될 때 호출되는 추상 함수입니다.
        /// 구체적인 총기 동작 클래스에서 반드시 구현해야 합니다. (예: 투사체 풀 정리)
        /// </summary>
        public abstract void OnGunUnloaded();
        /// <summary>
        /// 캐릭터 그래픽스에 총기를 물리적으로 장착시키는 추상 함수입니다.
        /// 구체적인 총기 동작 클래스에서 반드시 구현해야 합니다. (예: 특정 트랜스폼의 자식으로 설정)
        /// </summary>
        /// <param name="characterGraphics">총기를 장착할 캐릭터 그래픽스 컴포넌트</param>
        public abstract void PlaceGun(BaseCharacterGraphics characterGraphics);

        /// <summary>
        /// 현재 무기 강화 상태에 따라 총기의 데미지 및 관련 스탯을 다시 계산하는 추상 함수입니다.
        /// 구체적인 총기 동작 클래스에서 반드시 구현해야 합니다.
        /// </summary>
        public abstract void RecalculateDamage();

        /// <summary>
        /// 총기 발사 애니메이션 클립을 가져옵니다.
        /// </summary>
        /// <returns>캐릭터 발사 애니메이션 클립</returns>
        public AnimationClip GetShootAnimationClip()
        {
            return characterShootAnimation;
        }

        /// <summary>
        /// 총기 오브젝트에 간단한 크기 바운스 애니메이션을 재생합니다.
        /// </summary>
        public virtual void PlayBounceAnimation()
        {
            // 총기의 스케일을 작게 만들고, 짧은 시간 동안 원래 크기로 돌아오는 바운스 애니메이션을 실행합니다.
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut); // BackOut 이징 사용
        }

        /// <summary>
        /// 총기의 데미지 값을 설정합니다.
        /// </summary>
        /// <param name="damage">설정할 데미지 값 (최소/최대)</param>
        public void SetDamage(DuoInt damage)
        {
            this.damage = damage;
        }

        /// <summary>
        /// 총기의 데미지 값을 최소/최대 값으로 설정합니다.
        /// </summary>
        /// <param name="minDamage">최소 데미지</param>
        /// <param name="maxDamage">최대 데미지</param>
        public void SetDamage(int minDamage, int maxDamage)
        {
            damage = new DuoInt(minDamage, maxDamage);
        }

        /// <summary>
        /// 총기 업그레이드 파티클 효과를 재생합니다.
        /// </summary>
        public void PlayUpgradeParticle()
        {
            // 설정된 위치 오프셋과 크기로 업그레이드 파티클을 재생합니다.
            ParticleCase particleCase = ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position + upgradeParticleOffset).SetScale(upgradeParticleSize.ToVector3()); // ToVector3()는 사용자 정의 확장 함수일 수 있습니다.
        }

        /// <summary>
        /// 에디터에서 기즈모를 그릴 때 호출됩니다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 업그레이드 파티클 위치에 와이어 큐브 기즈모를 그려 시각적으로 확인합니다.
            Gizmos.DrawWireCube(transform.position + upgradeParticleOffset, upgradeParticleSize.ToVector3()); // ToVector3()는 사용자 정의 확장 함수일 수 있습니다.
        }

        /// <summary>
        /// 캐릭터 능력치를 고려하여 실제 데미지를 계산합니다.
        /// 치명타 확률 및 배수를 반영합니다.
        /// </summary>
        /// <returns>계산된 데미지 값</returns>
        protected int CalculateFinalDamage()
        {
            // [변경] DuoInt의 Random() 메서드를 활용하여 데미지 계산
            int baseDamage = damage.Random();
            float critChance = characterBehaviour.Stats.CritChance;
            float critMultiplier = characterBehaviour.Stats.CritMultiplier;

            // [추가] 치명타 판정 로직
            bool isCritical = Random.value < critChance;
            int finalDamage = isCritical ? Mathf.RoundToInt(baseDamage * critMultiplier) : baseDamage;

            // [추가] 치명타 시 시각 효과 처리 (색상 등은 무기 발사 측에서 처리)
            if (isCritical)
            {
                // 예시: 사운드 또는 별도 이펙트 실행
                // AudioController.PlaySound("critical_hit");
                // ParticlesController.Play("CriticalImpact", shootPoint.position);
            }

            return finalDamage;
        }

                // [추가] 치명타 여부 및 데미지를 함께 반환하는 구조체 또는 튜플 리턴 방식
        /// <summary>
        /// 캐릭터 능력치를 고려하여 실제 데미지를 계산합니다.
        /// 치명타 확률 및 배수를 반영하며, 치명타 여부를 함께 반환합니다.
        /// </summary>
        /// <returns>(계산된 데미지 값, 치명타 여부)</returns>
        protected (int damage, bool isCritical) CalculateFinalDamageWithCrit()
        {
            int baseDamage = damage.Random();
            float critChance = characterBehaviour.Stats.CritChance;
            float critMultiplier = characterBehaviour.Stats.CritMultiplier;

            bool isCritical = Random.value < critChance;
            //bool isCritical = false; // <--- 항상 false로 설정하여 치명타가 발생하지 않도록 함
            int finalDamage = isCritical ? Mathf.RoundToInt(baseDamage * critMultiplier) : baseDamage;
            //int finalDamage = baseDamage; // <--- 치명타 배율 적용하지 않고 기본 데미지만 사용하도록 수정

            return (finalDamage, isCritical);
        }

#if UNITY_EDITOR // 에디터에서만 실행되는 코드 블록입니다.
        /// <summary>
        /// 에디터 버튼을 통해 호출되는 함수로, 총기 설정을 위한 기본 트랜스폼(손 홀더, 발사 지점)을 준비합니다.
        /// </summary>
        [Button("Prepare Weapon")] // 에디터에 버튼으로 표시되도록 하는 속성입니다.
        private void PrepareWeapon()
        {
            // 왼손 홀더 트랜스폼이 할당되지 않았으면 새로운 게임 오브젝트를 생성하여 설정합니다.
            if (gunHolder.DefaultHolderData.LeftHandHolder == null)
            {
                GameObject leftHandHolderObject = new GameObject("Left Hand Holder");
                leftHandHolderObject.transform.SetParent(transform);
                leftHandHolderObject.transform.ResetLocal(); // ResetLocal()은 사용자 정의 확장 함수일 수 있습니다.
                leftHandHolderObject.transform.localPosition = new Vector3(-0.4f, 0, 0);

                // 에디터에서 아이콘을 설정합니다.
                GUIContent iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_3");
                UnityEditor.EditorGUIUtility.SetIconForObject(leftHandHolderObject, (Texture2D)iconContent.image);

                gunHolder.DefaultHolderData.LeftHandHolder = leftHandHolderObject.transform;
            }

            // 오른손 홀더 트랜스폼이 할당되지 않았으면 새로운 게임 오브젝트를 생성하여 설정합니다.
            if (gunHolder.DefaultHolderData.RightHandHolder == null)
            {
                GameObject rightHandHolderObject = new GameObject("Right Hand Holder");
                rightHandHolderObject.transform.SetParent(transform);
                rightHandHolderObject.transform.ResetLocal(); // ResetLocal()은 사용자 정의 확장 함수일 수 있습니다.
                rightHandHolderObject.transform.localPosition = new Vector3(0.4f, 0, 0);

                // 에디터에서 아이콘을 설정합니다.
                GUIContent iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_4");
                UnityEditor.EditorGUIUtility.SetIconForObject(rightHandHolderObject, (Texture2D)iconContent.image);

                gunHolder.DefaultHolderData.RightHandHolder = rightHandHolderObject.transform;
            }

            // 발사 지점 트랜스폼이 할당되지 않았으면 새로운 게임 오브젝트를 생성하여 설정합니다.
            if (shootPoint == null)
            {
                GameObject shootingPointObject = new GameObject("Shooting Point");
                shootingPointObject.transform.SetParent(transform);
                shootingPointObject.transform.ResetLocal(); // ResetLocal()은 사용자 정의 확장 함수일 수 있습니다.
                shootingPointObject.transform.localPosition = new Vector3(0, 0, 1);

                // 에디터에서 아이콘을 설정합니다.
                GUIContent iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_1");
                UnityEditor.EditorGUIUtility.SetIconForObject(shootingPointObject, (Texture2D)iconContent.image);

                shootPoint = shootingPointObject.transform;
            }

            // 캐릭터 발사 애니메이션 클립이 할당되지 않았으면 "Shot"이라는 이름의 애니메이션 클립을 에디터에서 로드합니다.
            if (characterShootAnimation == null)
            {
                characterShootAnimation = RuntimeEditorUtils.GetAssetByName<AnimationClip>("Shot"); // RuntimeEditorUtils는 사용자 정의 에디터 유틸리티 클래스일 수 있습니다.
            }
        }
#endif
    }
}