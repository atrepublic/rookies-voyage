// TeslaBulletBehavior.cs
// 이 스크립트는 테슬라 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 여러 적에게 연쇄적으로 피해를 입히는 기능을 추가합니다.
// PlayerBulletBehavior의 변경된 Init 시그니처를 따르도록 수정되었습니다.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon; // Tween, ParticlesController 등 Watermelon 프레임워크 사용
using Watermelon.LevelSystem; // ActiveRoom 사용

namespace Watermelon.SquadShooter
{
    public class TeslaBulletBehavior : PlayerBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = "Tesla Hit".GetHashCode();
        private static readonly int PARTICLE_WALL_HIT_HASH = "Tesla Wall Hit".GetHashCode();

        [Space(5f)]
        [Tooltip("투사체 이동 경로를 시각적으로 표시하는 트레일 렌더러 컴포넌트입니다.")]
        [SerializeField] TrailRenderer trailRenderer;

        // 투사체가 연쇄적으로 공격할 적 대상 목록입니다.
        private List<BaseEnemyBehavior> targets;
        // 투사체가 총 몇 명의 적을 명중시켜야 하는지의 목표치입니다.
        private int targetsHitGoal;
        // 투사체가 현재까지 몇 명의 적을 명중시켰는지의 카운트입니다.
        private int hitsPerformed;

        /// <summary>
        /// 테슬라 투사체를 초기화합니다.
        /// </summary>
        /// <param name="baseDamageFromGun">총기에서 계산된 초기 데미지 값</param>
        /// <param name="bulletSpeed">투사체의 이동 속도</param>
        /// <param name="initialTargetForProjectile">투사체의 초기 목표 적</param>
        /// <param name="projectileAutoDisableTime">투사체가 자동으로 비활성화될 시간</param>
        /// <param name="projectileDisableOnHit">직접 충돌 시 투사체를 비활성화할지 여부</param>
        /// <param name="gunShotWasCritical">총구 발사 시점의 치명타 여부</param>
        /// <param name="projectileOwner">이 투사체를 발사한 캐릭터의 CharacterBehaviour</param>
        public override void Init(float baseDamageFromGun, float bulletSpeed, BaseEnemyBehavior initialTargetForProjectile, float projectileAutoDisableTime, bool projectileDisableOnHit, bool gunShotWasCritical, CharacterBehaviour projectileOwner)
        {
            // PlayerBulletBehavior의 Init 호출 (변경된 시그니처에 맞게 모든 인자 전달)
            base.Init(baseDamageFromGun, bulletSpeed, initialTargetForProjectile, projectileAutoDisableTime, projectileDisableOnHit, gunShotWasCritical, projectileOwner);

            if (trailRenderer == null) {
                Debug.LogError($"[TeslaBulletBehavior] Init ({this.gameObject.name}): 트레일 렌더러가 할당되지 않았습니다!");
            } else {
                 trailRenderer.Clear(); // 재사용 시 이전 트레일 제거
            }

            // 투사체 크기 애니메이션 (DOTween 사용 가정)
            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn); // DOTween 확장 메서드

            hitsPerformed = 0; // 명중 횟수 초기화

            // 연쇄 공격 대상 목록 설정
            // ownerCharacterBehaviour는 base.Init에서 설정됨
            if (this.ownerCharacterBehaviour != null && this.ownerCharacterBehaviour.transform != null)
            {
                // 현재 활성화된 방의 살아있는 적들을 가져와 캐릭터로부터 가까운 순으로 정렬
                targets = ActiveRoom.GetAliveEnemies()
                    .Where(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead) // 유효한 타겟만 필터링
                    .OrderBy(e => Vector3.SqrMagnitude(e.transform.position - this.ownerCharacterBehaviour.transform.position))
                    .ToList();
                
                // 첫 번째 타겟(initialTargetForProjectile)은 이미 공격 대상이므로, targets 리스트에서 제외할 수 있음 (선택적)
                if (initialTargetForProjectile != null && targets.Contains(initialTargetForProjectile))
                {
                    targets.Remove(initialTargetForProjectile);
                }
                // Debug.Log($"[TeslaBulletBehavior] Init ({this.gameObject.name}): 초기 연쇄 타겟 목록 개수: {(targets != null ? targets.Count : 0)}");
            }
            else
            {
                Debug.LogError($"[TeslaBulletBehavior] Init ({this.gameObject.name}): CharacterBehaviourOwner를 찾을 수 없어 타겟 목록을 초기화할 수 없습니다.");
                targets = new List<BaseEnemyBehavior>(); // 오류 방지를 위해 빈 리스트로 초기화
            }
        }

        /// <summary>
        /// 투사체가 명중시켜야 할 총 적 대상 목표치(연쇄 횟수)를 설정합니다.
        /// </summary>
        /// <param name="goal">명중 목표 수</param>
        public void SetTargetsHitGoal(int goal)
        {
            targetsHitGoal = goal;
        }

        /// <summary>
        /// 물리 업데이트 동안 호출되며 투사체의 이동 및 다음 대상 추적을 처리합니다.
        /// </summary>
        protected override void FixedUpdate()
        {
            // 이미 비활성화되었거나, 발사자 정보가 없으면 아무것도 하지 않음
            if (!gameObject.activeSelf || ownerCharacterBehaviour == null)
            {
                return;
            }

            // Case 1: 첫 번째 타격이 아직 수행되지 않았고 (hitsPerformed == 0), 
            //         PlayerBulletBehavior에 의해 설정된 초기 목표(this.initialTarget)가 유효한 경우
            if (hitsPerformed == 0 && this.initialTarget != null && this.initialTarget.gameObject.activeInHierarchy && !this.initialTarget.IsDead)
            {
                // 목표를 향해 회전
                Vector3 targetPosition = this.initialTarget.transform.position;
                Vector3 targetDirection = targetPosition.SetY(transform.position.y) - transform.position;
                if (targetDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(targetDirection.normalized);
                }
                base.FixedUpdate(); // PlayerBulletBehavior의 이동 로직 사용
                return; // 첫 타격 발생 (OnTriggerEnter -> OnEnemyHitted 호출) 전까지는 아래 연쇄 로직 실행 안 함
            }

            // Case 2: 첫 번째 타격이 이미 발생했거나 (hitsPerformed > 0), 또는 어떤 이유로든 초기 목표가 없는 경우,
            //         이제 연쇄 공격 로직을 따릅니다.
            if (targets == null || targets.Count == 0 || hitsPerformed >= targetsHitGoal)
            {
                // 연쇄할 타겟이 없거나, 목표 연쇄 횟수를 달성했으면 비활성화
                DisableBullet();
                return;
            }

            // 다음 연쇄 타겟 유효성 검사
            BaseEnemyBehavior currentChainTarget = targets[0];
            if (currentChainTarget == null || !currentChainTarget.gameObject.activeInHierarchy || currentChainTarget.IsDead)
            {
                targets.RemoveAt(0);
                // FixedUpdate가 다음 프레임에 다시 이 부분을 평가하도록 여기서 return
                return;
            }

            // 다음 연쇄 대상을 PlayerBulletBehavior의 initialTarget 필드에도 반영하여
            // base.FixedUpdate()가 올바르게 해당 타겟을 향해 이동하도록 함
            this.initialTarget = currentChainTarget;
            Vector3 nextTargetPosition = currentChainTarget.transform.position;
            Vector3 nextTargetDirection = nextTargetPosition.SetY(transform.position.y) - transform.position;

            if (nextTargetDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(nextTargetDirection.normalized);
            }
            base.FixedUpdate(); // PlayerBulletBehavior의 이동 로직 사용
        }

        /// <summary>
        /// 적에게 명중했을 때 호출됩니다. (PlayerBulletBehavior.OnTriggerEnter 내부에서 호출됨)
        /// 연쇄 공격 로직을 처리합니다: 명중 횟수 증가, 데미지 감소, 다음 타겟 설정 등.
        /// 플로팅 텍스트는 PlayerBulletBehavior.OnTriggerEnter에서 각 타격 시 생성됩니다.
        /// </summary>
        /// <param name="enemyHitByThisBullet">이번에 명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior enemyHitByThisBullet)
        {
            // Debug.Log($"[TeslaBulletBehavior] OnEnemyHitted ({this.gameObject.name}) - 대상: {enemyHitByThisBullet.gameObject.name}, 현재 명중 횟수(증가 전): {hitsPerformed}");
            
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
            if (trailRenderer != null) trailRenderer.Clear(); // 다음 타겟으로 이동 전 트레일 초기화

            // 명중한 적(enemyHitByThisBullet)을 targets 리스트에서 제거 (이미 죽었거나, 방금 명중했으므로)
            // 그리고 이미 죽은 다른 타겟들도 정리
            if (targets != null)
            {
                targets.RemoveAll(t => t == null || t.IsDead || t == enemyHitByThisBullet);
            } else {
                targets = new List<BaseEnemyBehavior>(); // targets가 null인 예외 상황 방어
            }
            
            hitsPerformed++; // 실제 명중 횟수 증가
            // Debug.Log($"[TeslaBulletBehavior] OnEnemyHitted ({this.gameObject.name}) - 명중 횟수 증가 후: {hitsPerformed}");

            // 첫 번째 연쇄 공격(즉, 두 번째 타격부터) 데미지 감소 적용
            if (hitsPerformed == 1) // hitsPerformed는 0에서 시작하여 첫 타격 후 1이 됨. 즉, 이것이 첫 '연쇄' 시작점.
            {
                this.currentDamage *= 0.3f; // PlayerBulletBehavior의 currentDamage 필드를 직접 수정
                // Debug.Log($"[TeslaBulletBehavior] OnEnemyHitted ({this.gameObject.name}) - 첫 연쇄 명중. 데미지 감소 적용: {this.currentDamage}");
            }

            // 목표 연쇄 횟수를 달성했거나 더 이상 공격할 타겟이 없으면 총알 비활성화
            if (hitsPerformed >= targetsHitGoal || targets == null || targets.Count == 0)
            {
                // Debug.Log($"[TeslaBulletBehavior] OnEnemyHitted ({this.gameObject.name}) - 투사체 비활성화. 이유: 명중 횟수 달성({hitsPerformed}/{targetsHitGoal}) 또는 남은 타겟 없음({(targets != null ? targets.Count : 0)}).");
                DisableBullet();
            }
            else
            {
                // 다음 타겟이 있다면 FixedUpdate에서 해당 타겟으로 이동 및 공격 계속
                // Debug.Log($"[TeslaBulletBehavior] OnEnemyHitted ({this.gameObject.name}) - 연쇄 공격 계속. 다음 타겟 후보: {targets[0].gameObject.name}. 남은 타겟 수: {targets.Count}");
            }
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            // PlayerBulletBehavior의 OnObstacleHitted가 먼저 호출되어 기본 처리 (비활성화 등)
            base.OnObstacleHitted(); 

            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            
            // 추가적으로 확실히 비활성화
            if (gameObject.activeSelf) 
            {
                 DisableBullet(); 
            }
        }

        /// <summary>
        /// 투사체 게임 오브젝트를 비활성화하고 트레일 렌더러를 정리합니다.
        /// </summary>
        private void DisableBullet()
        {
            if (!gameObject.activeSelf) 
            {
                return; // 이미 비활성화된 경우 중복 호출 방지
            }
            // Debug.Log($"[TeslaBulletBehavior] DisableBullet ({this.gameObject.name}) - 비활성화 처리 시작.");
            if(trailRenderer != null) 
                 trailRenderer.Clear();
            gameObject.SetActive(false);
        }
    }
}