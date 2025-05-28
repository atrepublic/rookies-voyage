// 이 스크립트는 테슬라 투사체의 동작을 정의합니다.
// 플레이어 투사체 기본 동작을 상속받으며, 여러 적에게 연쇄적으로 피해를 입히는 기능을 추가합니다.
using System.Collections.Generic; // List 사용을 위해 필요합니다.
using System.Linq; // Linq (OrderBy, ToList) 사용을 위해 필요합니다.
using UnityEngine;
using Watermelon.LevelSystem; // ActiveRoom 사용을 위해 필요합니다.
// DOTween 등 필요한 네임스페이스가 있다면 여기에 추가하세요. (예: using DG.Tweening;)

namespace Watermelon.SquadShooter
{
    // PlayerBulletBehavior를 상속받아 플레이어 투사체의 기본 기능을 활용합니다.
    public class TeslaBulletBehavior : PlayerBulletBehavior
    {
        // 적 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_HIT_HASH = "Tesla Hit".GetHashCode();
        // 벽 명중 시 재생할 파티클 시스템 해시 값입니다.
        private static readonly int PARTICLE_WALL_HIT_HASH = "Tesla Wall Hit".GetHashCode();

        [Space(5f)] // 인스펙터에 5f 간격을 추가합니다.
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
        /// 기본 투사체 정보 설정 후 트레일 렌더러를 초기화하고 크기 애니메이션을 시작하며, 공격할 적 대상 목록을 설정합니다.
        /// </summary>
        /// <param name="damage">투사체의 초기 데미지 값</param>
        /// <param name="speed">투사체의 이동 속도</param>
        /// <param name="currentTarget">투사체의 초기 목표 적 (주로 첫 번째 대상)</param>
        /// <param name="autoDisableTime">자동 비활성화 시간</param>
        /// <param name="autoDisableOnHit">충돌 시 자동 비활성화 여부</param>
        public override void Init(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit)
        {
            // 상위 클래스의 Init 함수를 호출하여 기본 투사체 속성을 설정합니다.
            base.Init(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            if (trailRenderer == null) {
                Debug.LogError($"[테슬라 총알 초기화] ID: {this.gameObject.GetInstanceID()} - 트레일 렌더러가 할당되지 않았습니다!");
            } else {
                 trailRenderer.Clear();
            }

            // 투사체의 초기 스케일을 작게 설정하고, 짧은 시간 동안 원래 크기로 커지는 애니메이션을 실행합니다.
            transform.localScale = Vector3.one * 0.1f;
            // DOTween 사용 시, 해당 네임스페이스(DG.Tweening)를 using 해야 합니다.
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn);

            // 명중 횟수 카운트를 0으로 초기화합니다.
            hitsPerformed = 0;

            // CharacterBehaviour.GetBehaviour() 및 그 transform이 null이 아닌지 확인합니다.
            CharacterBehaviour currentCharacterBehaviour = CharacterBehaviour.GetBehaviour();
            if (currentCharacterBehaviour != null && currentCharacterBehaviour.transform != null) {
                targets = ActiveRoom.GetAliveEnemies().OrderBy(e => Vector3.SqrMagnitude(e.transform.position - currentCharacterBehaviour.transform.position)).ToList();
                Debug.Log($"[테슬라 총알 초기화] ID: {this.gameObject.GetInstanceID()}, 타겟 목록 개수: {(targets != null ? targets.Count : 0)}");
            } else {
                Debug.LogError($"[테슬라 총알 초기화] ID: {this.gameObject.GetInstanceID()} - CharacterBehaviour 또는 그 Transform을 찾을 수 없어 타겟 목록을 초기화할 수 없습니다.");
                targets = new List<BaseEnemyBehavior>(); // 오류 방지를 위해 빈 리스트로 초기화
            }
        }

        /// <summary>
        /// 투사체가 명중시켜야 할 총 적 대상 목표치를 설정합니다.
        /// </summary>
        /// <param name="goal">명중 목표 수</param>
        public void SetTargetsHitGoal(int goal)
        {
            targetsHitGoal = goal;
        }

        /// <summary>
        /// 물리 업데이트 동안 호출되며 투사체의 이동 및 대상 추적을 처리합니다.
        /// </summary>
        protected override void FixedUpdate()
        {
            // 공격할 대상 목록이 null이거나 비어있으면 투사체를 비활성화합니다.
            if (targets == null || targets.Count == 0)
            {
                DisableBullet();
                return;
            }

            // 목표 명중 횟수를 달성했으면 투사체를 비활성화합니다.
            if (hitsPerformed >= targetsHitGoal)
            {
                DisableBullet();
                return;
            }
            
            // 현재 목표 대상(targets[0])이 null이거나 게임 오브젝트가 비활성화된 경우 처리합니다.
            if (targets[0] == null || !targets[0].gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[테슬라 총알 FixedUpdate] ID: {this.gameObject.GetInstanceID()} - 현재 타겟(targets[0])이 null이거나 비활성화되어 리스트에서 제거합니다.");
                targets.RemoveAt(0);
                // 제거 후 targets 리스트가 비었는지 다시 확인하고 비었으면 투사체를 비활성화합니다.
                if (targets.Count == 0)
                {
                    DisableBullet();
                }
                return; // 다음 FixedUpdate에서 새로운 targets[0]으로 처리하도록 합니다.
            }

            // 현재 목표 대상의 위치를 향하는 방향 벡터를 계산합니다 (Y축 위치는 1f로 고정).
            Vector3 targetPosition = targets[0].transform.position;
            Vector3 targetDirection = targetPosition.SetY(1f) - transform.position;

            // 목표 방향 벡터의 크기가 매우 작지 않은 경우 (오류 방지), 투사체를 회전시킵니다.
            if (targetDirection.sqrMagnitude > 0.001f) 
            {
                // Squad Shooter 원본 방식 (거의 즉시 회전)
                Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 0f); 
                transform.rotation = Quaternion.LookRotation(rotationDirection);
            }

            // 상위 클래스의 FixedUpdate 함수를 호출하여 속도에 따라 투사체를 앞 방향으로 이동시킵니다.
            base.FixedUpdate();

            // 현재 목표 대상이 죽었는지 확인하고, 죽었다면 대상 목록에서 제거합니다.
            // targets 리스트가 비어있지 않고, targets[0]이 null이 아닌지 먼저 확인합니다.
            if (targets.Count > 0 && targets[0] != null && targets[0].IsDead)
            {
                Debug.Log($"[테슬라 총알 FixedUpdate] ID: {this.gameObject.GetInstanceID()} - 현재 타겟 {targets[0].gameObject.name}이(가) 사망하여 리스트에서 제거합니다.");
                targets.RemoveAt(0);
                // 제거 후 targets 리스트가 비었는지 다시 확인하고 비었으면 투사체를 비활성화합니다.
                if (targets.Count == 0)
                {
                    DisableBullet(); 
                }
            }
        }

        /// <summary>
        /// 적에게 명중했을 때 호출됩니다.
        /// 특정 명중 파티클을 재생하고 트레일 렌더러를 초기화하며, 대상 목록을 업데이트하고 명중 횟수 및 데미지를 조정합니다.
        /// </summary>
        /// <param name="baseEnemyBehavior">명중한 적 객체</param>
        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            string bulletInstanceID = this.gameObject.GetInstanceID().ToString();
            Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 대상: {baseEnemyBehavior.gameObject.name} (ID: {baseEnemyBehavior.GetInstanceID()}), 현재 명중 횟수: {hitsPerformed}");
            
            if (targets == null) {
                 Debug.LogError($"[테슬라 총알 명중] ID: {bulletInstanceID} - Targets 리스트가 null입니다! 비정상 상황. 투사체를 비활성화합니다.");
                 DisableBullet();
                 return;
            }

            Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 타겟 제거 전 리스트 (개수: {targets.Count}):");
            for(int j=0; j < targets.Count; j++) // 필요시 타겟 상세 로깅
            {
                if (targets[j] != null)
                    Debug.Log($"  - 타겟[{j}]: {targets[j].gameObject.name} (ID: {targets[j].GetInstanceID()}), 사망 여부: {targets[j].IsDead}");
                else
                    Debug.Log($"  - 타겟[{j}]: NULL");
            }

            // ParticlesController.IsInitialised 체크 제거
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            if (trailRenderer != null) trailRenderer.Clear();

            // Squad Shooter 버전과 유사한 방식의 제거 루프로 변경
            bool removedThisEnemy = false;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) { 
                    Debug.LogWarning($"[테슬라 총알 명중] ID: {bulletInstanceID} - 제거 루프 중 타겟[{i}]이 null입니다. 안전하게 제거합니다.");
                    targets.RemoveAt(i);
                    i--;
                    continue;
                }

                if (targets[i].IsDead || targets[i] == baseEnemyBehavior) // 참조(==) 비교 사용
                {
                    Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 제거 대상: {targets[i].gameObject.name} (사망: {targets[i].IsDead}, 명중타겟과 동일: {targets[i] == baseEnemyBehavior})");
                    if (targets[i] == baseEnemyBehavior) {
                        removedThisEnemy = true;
                    }
                    targets.RemoveAt(i);
                    i--; 
                }
            }

            if (removedThisEnemy) {
                Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 명중한 적 ({baseEnemyBehavior.gameObject.name})을 타겟 리스트에서 성공적으로 제거했습니다 (Squad Shooter 방식).");
            } else {
                Debug.LogWarning($"[테슬라 총알 명중] ID: {bulletInstanceID} - 경고: 명중한 적 ({baseEnemyBehavior.gameObject.name})이 타겟 리스트에 없었거나 제거되지 않았습니다 (Squad Shooter 방식). '누적' 현상 유발 가능.");
            }
            
            Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 타겟 최종 정리 후 리스트 (개수: {targets.Count}):");
            // 필요시 최종 타겟 상세 로깅

            hitsPerformed++;
            Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 명중 횟수 증가: {hitsPerformed}");

            if (hitsPerformed == 1) 
            {
                damage *= 0.3f; 
                Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 첫 연쇄 명중. 데미지 감소 적용: {damage}");
            }

            if (hitsPerformed >= targetsHitGoal || targets.Count == 0)
            {
                Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 투사체 비활성화. 이유: 명중 횟수 달성({hitsPerformed}/{targetsHitGoal}) 또는 남은 타겟 없음({targets.Count}).");
                DisableBullet();
            }
            else
            {
                if (targets.Count > 0 && targets[0] != null && targets[0].gameObject.activeInHierarchy && !targets[0].IsDead) {
                    Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - 연쇄 공격 계속. 다음 타겟: {targets[0].gameObject.name}. 투사체 활성 유지.");
                } else {
                     Debug.LogWarning($"[테슬라 총알 명중] ID: {bulletInstanceID} - 연쇄 공격 계속하려 했으나, 다음 타겟이 유효하지 않음. 투사체 비활성화.");
                     // 필요시 다음 타겟 상세 정보 로깅
                     DisableBullet(); 
                }
            }
            Debug.Log($"[테슬라 총알 명중] ID: {bulletInstanceID} - OnEnemyHitted 로직 종료. 명중 대상: {baseEnemyBehavior.gameObject.name}.");
        }

        /// <summary>
        /// 장애물에 명중했을 때 호출됩니다.
        /// 기본 장애물 충돌 처리와 함께 특정 벽 충돌 파티클을 재생하고 투사체를 비활성화합니다.
        /// </summary>
        protected override void OnObstacleHitted()
        {
            string bulletID = this.gameObject.GetInstanceID().ToString();
            Debug.LogWarning($"[테슬라 총알 장애물 명중] ID: {bulletID} - OnObstacleHitted 호출됨. 투사체 비활성화 예정.");
            
            base.OnObstacleHitted(); 

            // ParticlesController.IsInitialised 체크 제거
            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            
            if (gameObject.activeSelf) 
            {
                 DisableBullet(); 
            }
        }

        /// <summary>
        /// 투사체 게임 오브젝트를 비활성화하고 트레일 렌더러를 초기화합니다.
        /// </summary>
        private void DisableBullet()
        {
            if (!gameObject.activeSelf) 
            {
                return;
            }
            Debug.Log($"[테슬라 총알] ID: {this.gameObject.GetInstanceID()} - DisableBullet 호출됨. 비활성화 처리 시작.");
            if(trailRenderer != null) 
                 trailRenderer.Clear();
            gameObject.SetActive(false);
        }
    }
}