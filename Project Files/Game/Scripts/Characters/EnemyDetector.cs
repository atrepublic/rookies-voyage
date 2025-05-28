/*
 * EnemyDetector.cs
 * ---------------------
 * 이 스크립트는 캐릭터 또는 다른 개체 주변의 적을 감지하는 역할을 합니다.
 * SphereCollider 트리거를 사용하여 범위 내의 적들을 감지하고,
 * 가장 가까운 적을 찾아 주기적으로 업데이트하며, 감지 상태 변경 시
 * IEnemyDetector 인터페이스를 통해 관련 컴포넌트(예: CharacterBehaviour)에 알립니다.
 */

using System.Collections.Generic;
using UnityEngine;
using Watermelon; // Watermelon 프레임워크 네임스페이스 (BaseEnemyBehavior 등)

namespace Watermelon.SquadShooter
{
    // 적 감지 로직을 처리하는 클래스
    public class EnemyDetector : MonoBehaviour
    {
        [Tooltip("가장 가까운 적을 다시 확인하는 주기 (초). 적이 많을 때 성능 부하를 줄이기 위함.")]
        [SerializeField] float checkDelay = 1f;

        [Tooltip("적 감지에 사용되는 Sphere Collider 컴포넌트")]
        private SphereCollider detectorCollider;
        // 외부에서 감지 콜라이더에 접근하기 위한 프로퍼티
        public SphereCollider DetectorCollider => detectorCollider;

        [Tooltip("현재 감지 범위 내에 있는 적의 수")]
        private int detectedEnemiesCount;
        [Tooltip("현재 감지 범위 내에 있는 적들의 리스트")]
        private List<BaseEnemyBehavior> detectedEnemies;
        // 외부에서 감지된 적 리스트에 접근하기 위한 프로퍼티
        public List<BaseEnemyBehavior> DetectedEnemies => detectedEnemies;

        [Tooltip("현재 감지된 적들 중 가장 가까운 적")]
        private BaseEnemyBehavior closestEnemy;
        // 외부에서 가장 가까운 적에 접근하기 위한 프로퍼티
        public BaseEnemyBehavior ClosestEnemy => closestEnemy;

        // 현재 감지 범위의 반지름을 반환하는 프로퍼티
        public float DetectorRadius => detectorCollider.radius;

        [Tooltip("다음 번 가장 가까운 적 확인을 수행할 시간")]
        private float nextClosestCheckTime = 0.0f;

        [Tooltip("이 감지기의 이벤트를 수신할 대상 (CharacterBehaviour 등)")]
        private IEnemyDetector enemyDetector; // IEnemyDetector 인터페이스 타입

        /// <summary>
        /// EnemyDetector를 초기화합니다.
        /// </summary>
        /// <param name="enemyDetector">가장 가까운 적 변경 알림을 받을 객체</param>
        public void Init(IEnemyDetector enemyDetector)
        {
            this.enemyDetector = enemyDetector;

            // 감지 콜라이더 컴포넌트 가져오기
            detectorCollider = GetComponent<SphereCollider>();

            // 변수 초기화
            detectedEnemies = new List<BaseEnemyBehavior>();
            detectedEnemiesCount = 0;

            // 적 사망 이벤트 구독 (사망한 적을 리스트에서 제거하기 위함)
            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        /// <summary>
        /// 감지기의 반경을 설정합니다. (무기 사거리 변경 등에 사용)
        /// </summary>
        /// <param name="radius">새로운 반경 값</param>
        public void SetRadius(float radius)
        {
            detectorCollider.radius = radius;
        }

        /// <summary>
        /// 적 사망 이벤트 발생 시 호출되는 콜백 함수입니다.
        /// </summary>
        /// <param name="enemy">사망한 적</param>
        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            RemoveEnemy(enemy); // 감지 목록에서 해당 적 제거
        }

        /// <summary>
        /// 현재 감지된 적들 중에서 가장 가까운 적을 찾아 업데이트합니다.
        /// 가장 가까운 적이 변경되면 IEnemyDetector 인터페이스를 통해 알립니다.
        /// </summary>
        public void UpdateClosestEnemy()
        {
            // 감지된 적이 없으면
            if (detectedEnemiesCount == 0)
            {
                // 이전에 가장 가까운 적이 있었다면, 이제 없다고 알림
                if (closestEnemy != null)
                    enemyDetector.OnCloseEnemyChanged(null);

                closestEnemy = null; // 가장 가까운 적 정보 초기화
                return; // 처리 종료
            }

            // 가장 가까운 적을 찾기 위한 초기화
            float minDistanceSqr = float.MaxValue; // 최소 거리 제곱값 (거리 비교 시 제곱근 계산 방지)
            BaseEnemyBehavior tempEnemy = null; // 임시로 가장 가까운 적을 저장할 변수

            // 감지된 모든 적 순회
            for (int i = 0; i < detectedEnemiesCount; i++)
            {
                var enemy = detectedEnemies[i];

                // 적이 죽었으면 건너뜀 (리스트에서 즉시 제거되지 않았을 경우 대비)
                if (enemy.IsDead) continue;

                // 감지기 위치와 적 위치 사이의 거리 제곱 계산
                float distanceSqr = (transform.position - enemy.transform.position).sqrMagnitude;

                // 현재까지의 최소 거리보다 더 가깝고, 살아있는 적이면
                if (distanceSqr < minDistanceSqr)
                {
                    tempEnemy = enemy; // 가장 가까운 적 후보 업데이트
                    minDistanceSqr = distanceSqr; // 최소 거리 업데이트
                }
            }

            // 최종적으로 찾은 가장 가까운 적이 이전에 기록된 적과 다르면
            if (closestEnemy != tempEnemy)
                enemyDetector.OnCloseEnemyChanged(tempEnemy); // 변경 사항 알림

            closestEnemy = tempEnemy; // 가장 가까운 적 정보 업데이트
        }

        /// <summary>
        /// MonoBehaviour: 매 프레임 호출됩니다.
        /// 주기적으로 가장 가까운 적을 업데이트합니다. (성능 최적화)
        /// </summary>
        private void Update()
        {
            // 감지된 적이 1명 초과이고, 다음 확인 시간이 되었다면
            if (detectedEnemiesCount > 1 && Time.time > nextClosestCheckTime)
            {
                nextClosestCheckTime = Time.time + checkDelay; // 다음 확인 시간 설정
                UpdateClosestEnemy(); // 가장 가까운 적 업데이트 함수 호출
            }
            // 감지된 적이 0명 또는 1명일 때는 OnTriggerEnter/Exit에서 즉시 처리되므로 매 프레임 확인할 필요 없음
        }

        /// <summary>
        /// 감지 목록에서 특정 적을 제거합니다.
        /// </summary>
        /// <param name="enemy">제거할 적</param>
        private void RemoveEnemy(BaseEnemyBehavior enemy)
        {
            // 리스트에서 해당 적의 인덱스 찾기
            int enemyIndex = detectedEnemies.FindIndex(x => x == enemy);
            // 적이 리스트에 존재하면 (-1이 아니면)
            if (enemyIndex != -1)
            {
                detectedEnemies.RemoveAt(enemyIndex); // 리스트에서 제거
                detectedEnemiesCount--; // 감지된 적 수 감소

                // 적이 제거되었으므로 가장 가까운 적을 다시 찾아야 할 수 있음
                UpdateClosestEnemy();
            }
        }

        /// <summary>
        /// MonoBehaviour: 다른 Collider가 트리거 영역에 들어왔을 때 호출됩니다.
        /// </summary>
        /// <param name="other">충돌한 Collider</param>
        private void OnTriggerEnter(Collider other)
        {
            // 충돌한 객체의 태그가 "Enemy"인지 확인
            if (other.gameObject.CompareTag(PhysicsHelper.TAG_ENEMY))
            {
                // BaseEnemyBehavior 컴포넌트 가져오기
                BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
                if (enemy != null)
                {
                    // 리스트에 아직 포함되지 않은 적이면
                    if (!detectedEnemies.Contains(enemy))
                    {
                        detectedEnemies.Add(enemy); // 리스트에 추가
                        detectedEnemiesCount++; // 감지된 적 수 증가

                        // 새로운 적이 추가되었으므로 가장 가까운 적 업데이트
                        UpdateClosestEnemy();
                    }
                }
            }
        }

        /// <summary>
        /// 외부에서 특정 적을 감지 목록에 추가하도록 시도합니다.
        /// (예: 적이 스폰될 때 즉시 감지 목록에 반영하기 위해)
        /// </summary>
        /// <param name="enemy">추가 시도할 적</param>
        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            // 리스트에 아직 포함되지 않은 적이면
            if (!detectedEnemies.Contains(enemy))
            {
                // 적이 감지 범위 내에 있는지 확인
                if (Vector3.Distance(enemy.transform.position, transform.position) <= DetectorRadius)
                {
                    detectedEnemies.Add(enemy); // 리스트에 추가
                    detectedEnemiesCount++; // 감지된 적 수 증가
                    UpdateClosestEnemy(); // 가장 가까운 적 업데이트
                }
            }
            else // 이미 리스트에 있는 경우 (혹시 모를 상황 대비)
            {
                // 가장 가까운 적만 다시 확인
                UpdateClosestEnemy();
            }
        }

        /// <summary>
        /// MonoBehaviour: 다른 Collider가 트리거 영역에서 나갔을 때 호출됩니다.
        /// </summary>
        /// <param name="other">충돌이 끝난 Collider</param>
        private void OnTriggerExit(Collider other)
        {
            // 충돌이 끝난 객체의 태그가 "Enemy"인지 확인
            if (other.gameObject.CompareTag(PhysicsHelper.TAG_ENEMY))
            {
                // BaseEnemyBehavior 컴포넌트 가져오기
                BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
                if (enemy != null)
                {
                    RemoveEnemy(enemy); // 감지 목록에서 제거
                }
            }
        }

        /// <summary>
        /// 감지된 적 목록을 강제로 비웁니다. (현재 코드에서는 사용되지 않는 것으로 보임)
        /// </summary>
        public void ClearZombiesList()
        {
            detectedEnemies.Clear(); // 리스트 비우기
            detectedEnemiesCount = 0; // 카운트 초기화
            UpdateClosestEnemy(); // 가장 가까운 적 업데이트 (null로 설정됨)
        }

        /// <summary>
        /// MonoBehaviour: 오브젝트가 파괴될 때 호출됩니다.
        /// 이벤트 구독을 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 구독했던 적 사망 이벤트 해제
            BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;
        }

        /// <summary>
        /// 감지기 상태를 초기화(재설정)합니다. 레벨 재시작 등에 사용됩니다.
        /// </summary>
        public void Reload()
        {
            detectedEnemies.Clear(); // 감지 목록 비우기
            detectedEnemiesCount = 0; // 카운트 초기화
            closestEnemy = null; // 가장 가까운 적 초기화
            // 필요하다면 IEnemyDetector에게 null 전달
            // enemyDetector?.OnCloseEnemyChanged(null);
        }
    }
}