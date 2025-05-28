// 이 스크립트는 수류탄 투사체의 동작을 정의합니다.
// 던지기, 포물선 궤적 이동, 폭발 및 주변 대상에게 피해를 입히는 기능을 구현합니다.
using System.Collections; // 코루틴 사용을 위해 필요합니다.
using UnityEngine;
using Watermelon; // Watermelon 네임스페이스의 다른 기능(예: ParticlesController, Tween, AudioController)을 사용하기 위해 필요합니다.
using Watermelon.LevelSystem; // ActiveRoom 사용을 위해 필요합니다.
// using DG.Tweening; 네임스페이스는 DOTween 플러그인 사용에 필요하지만, 현재 코드에 명시적으로 포함되어 있지 않습니다.
// Tweening 관련 기능(DOColor, DOScale, SetEasing 등)은 해당 플러그인이 프로젝트에 추가되어 있음을 가정합니다.

namespace Watermelon.SquadShooter
{
    public class GrenadeBehavior : MonoBehaviour
    {
        [Tooltip("수류탄 투척 시의 각도 (지면으로부터의 각도)입니다.")]
        public float angle = 45f;
        [Tooltip("수류탄 이동에 적용될 중력 값입니다.")]
        public float gravity = 150f;

        [Tooltip("수류탄의 회전 속도 범위 (최소/최대 값)입니다.")]
        public DuoVector3 angularVelocityDuo; // DuoVector3는 두 개의 Vector3 값을 저장하는 사용자 정의 구조체일 수 있습니다.
        // 현재 적용된 수류탄의 회전 속도입니다.
        private Vector3 angularVelocity;

        [Tooltip("수류탄의 물리적 시뮬레이션을 담당하는 Rigidbody 컴포넌트입니다.")]
        [SerializeField] Rigidbody rb;
        [Tooltip("수류탄의 시각적 메시를 나타내는 MeshRenderer 컴포넌트입니다.")]
        [SerializeField] MeshRenderer sphereRenderer;
        [Tooltip("수류탄 폭발 시 피해를 입히는 반경입니다.")]
        [SerializeField] float explosionRadius;

        [Tooltip("수류탄 메쉬의 시작 색상입니다 (투척 시).")]
        [SerializeField] Color startColor;
        [Tooltip("수류탄 메쉬의 끝 색상입니다 (폭발 전).")]
        [SerializeField] Color endColor;
        [Tooltip("색상 및 크기 애니메이션에 사용될 이징(Easing) 타입입니다.")]
        [SerializeField] Ease.Type easing; // Ease.Type은 DOTween 플러그인에 정의된 열거형일 수 있습니다.

        // 폭발 파티클 시스템의 해시 값입니다.
        private int explosionParticleHash;
        // 폭발 데칼 파티클 시스템의 해시 값입니다.
        private int explosionDecalParticleHash;
        // 수류탄이 목표 지점에 도달하는 데 걸리는 시간입니다.
        float duration;

        /// <summary>
        /// 이 오브젝트가 로드될 때 호출됩니다.
        /// </summary>
        private void Awake()
        {
            // 폭발 파티클 및 데칼 파티클의 해시 값을 미리 계산하여 저장합니다.
            explosionParticleHash = "Bomber Explosion".GetHashCode();
            explosionDecalParticleHash = "Bomber Explosion Decal".GetHashCode();
        }

        /// <summary>
        /// 수류탄을 특정 위치로 던지도록 초기화하고 코루틴을 시작합니다.
        /// </summary>
        /// <param name="startPosition">수류탄이 시작될 위치</param>
        /// <param name="targetPosition">수류탄이 목표로 할 위치</param>
        /// <param name="damage">수류탄 폭발 시 적용될 데미지</param>
        public void Throw(Vector3 startPosition, Vector3 targetPosition, float damage)
        {
            // 수류탄 게임 오브젝트를 활성화합니다.
            gameObject.SetActive(true);
            // 설정된 범위 내에서 무작위 회전 속도를 선택합니다.
            angularVelocity = angularVelocityDuo.Random();

            // 수류탄의 시작 위치를 설정합니다.
            transform.position = startPosition;

            // 코루틴 시작 전에는 물리 시뮬레이션을 사용하지 않습니다.
            rb.isKinematic = true;
            rb.useGravity = false;

            // 수류탄 투척 궤적 계산 및 이동을 처리하는 코루틴을 시작합니다.
            StartCoroutine(ThrowCoroutine(startPosition, targetPosition, angle, gravity, damage));

            // 수류탄 메쉬 렌더러 오브젝트를 활성화하고 초기 색상 및 크기를 설정합니다.
            sphereRenderer.gameObject.SetActive(true);
            sphereRenderer.material.SetColor("_BaseColor", startColor); // '_BaseColor'는 URP 또는 HDRP 셰이더의 기본 색상 속성 이름일 수 있습니다.
            sphereRenderer.transform.localScale = Vector3.zero;

            // 수류탄 메쉬의 색상과 크기를 시간에 따라 애니메이션합니다.
            sphereRenderer.material.DOColor(Shader.PropertyToID("_BaseColor"), endColor, duration + 0.5f).SetEasing(easing);
            sphereRenderer.DOScale(explosionRadius * 2, duration + 0.25f).SetEasing(easing);
        }

        /// <summary>
        /// 수류탄의 포물선 투척 궤적을 계산하고 이동을 시뮬레이션하는 코루틴입니다.
        /// </summary>
        /// <param name="startPosition">시작 위치</param>
        /// <param name="targetPosition">목표 위치</param>
        /// <param name="angle">투척 각도</param>
        /// <param name="gravity">적용될 중력</param>
        /// <param name="damage">폭발 데미지</param>
        IEnumerator ThrowCoroutine(Vector3 startPosition, Vector3 targetPosition, float angle, float gravity, float damage)
        {
            // 시작 위치와 목표 위치 사이의 거리를 계산합니다.
            var distance = Vector3.Distance(startPosition, targetPosition);
            // 시작 위치에서 목표 위치를 향하는 방향 벡터를 계산합니다.
            var direction = (targetPosition - startPosition).normalized;

            // 포물선 궤적에 필요한 초기 속도를 계산합니다.
            var velocity = distance / (Mathf.Sin(2 * angle * Mathf.Deg2Rad) / gravity);

            // 계산된 속도를 수평(Vx) 및 수직(Vy) 성분으로 분해합니다.
            var Vx = Mathf.Sqrt(velocity) * Mathf.Cos(angle * Mathf.Deg2Rad);
            var Vy = Mathf.Sqrt(velocity) * Mathf.Sin(angle * Mathf.Deg2Rad); // Mathf.Deg2Rad로 수정

            // 목표 지점까지 도달하는 예상 시간을 계산합니다.
            duration = distance / Vx;

            var time = 0f;
            var prevPos = transform.position; // 이전 프레임 위치를 저장할 변수

            // 예상 시간 동안 반복하여 수류탄의 위치를 업데이트합니다.
            while (time < duration)
            {
                prevPos = transform.position; // 현재 위치를 이전 위치로 저장

                // 포물선 궤적 공식에 따라 수직 및 수평 이동을 계산하여 위치를 업데이트합니다.
                transform.position += Vector3.up * (Vy - gravity * time) * Time.deltaTime + direction * Vx * Time.deltaTime;
                // 설정된 각속도로 회전을 업데이트합니다.
                transform.eulerAngles += angularVelocity * Time.deltaTime;

                // 시간 경과를 업데이트합니다.
                time += Time.deltaTime;

                // 다음 프레임까지 대기합니다.
                yield return null;
            }

            // 목표 지점에 도달하면 물리 시뮬레이션을 다시 활성화합니다.
            rb.isKinematic = false;
            rb.useGravity = true;

            // 마지막 두 프레임의 위치 차이를 사용하여 예상 속도를 계산하고, 제한된 값으로 Rigidbody의 선형 속도를 설정합니다.
            Vector3 calculatedVelocity = (transform.position - prevPos) / Time.deltaTime;
            Vector3 clampedVelocity = new Vector3(Mathf.Clamp(calculatedVelocity.x, -100f, 100f), Mathf.Clamp(calculatedVelocity.y, -100f, 100f), Mathf.Clamp(calculatedVelocity.z, -100f, 100f));
            rb.linearVelocity = clampedVelocity;
            // Rigidbody의 각속도를 설정합니다.
            rb.angularVelocity = angularVelocity;

            // 0.5초 대기 후 폭발 처리를 시작합니다.
            yield return new WaitForSeconds(0.5f);

            // 폭발 파티클을 재생합니다.
            var explosionCase = ParticlesController.PlayParticle(explosionParticleHash);
            explosionCase.SetPosition(transform.position);

            // 폭발 데칼 파티클을 재생하고 크기 및 회전을 설정합니다.
            var explosionDecalCase = ParticlesController.PlayParticle(explosionDecalParticleHash);
            explosionDecalCase.SetPosition(transform.position).SetScale(Vector3.one * 3f).SetRotation(Quaternion.Euler(-90f, 0f, 0f));
            // 폭발 사운드를 재생합니다.
            AudioController.PlaySound(AudioController.AudioClips.explode);

            // 수류탄 게임 오브젝트를 파괴합니다.
            Destroy(gameObject);

            // 플레이어 캐릭터 행동 컴포넌트를 가져옵니다.
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();

            // 플레이어가 폭발 반경 내에 있는지 확인하고, 있다면 피해를 입힙니다.
            if (Vector3.Distance(transform.position, characterBehaviour.transform.position) <= explosionRadius)
            {
                characterBehaviour.TakeDamage(damage);
            }

            // 현재 방에 있는 살아있는 모든 적을 가져옵니다.
            var aliveEnemies = ActiveRoom.GetAliveEnemies();

            // 살아있는 적들을 순회하며 폭발 반경 내에 있는 적에게 피해를 입힙니다.
            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                var enemy = aliveEnemies[i];

                // 자기 자신(수류탄)은 건너뜁니다. (사실 수류탄은 적 리스트에 없겠지만 안전을 위해)
                if (enemy == this)
                    continue;

                // 적이 폭발 반경 내에 있는지 확인합니다.
                if (Vector3.Distance(transform.position, enemy.transform.position) <= explosionRadius)
                {
                    // 적을 향하는 방향 벡터를 계산합니다 (Y축은 무시하고 수평 방향만 사용).
                    var directionToEnemy = (enemy.transform.position.SetY(0) - transform.position.SetY(0)).normalized;
                    // 적에게 데미지를 입힙니다. (피해 위치와 방향도 함께 전달)
                    enemy.TakeDamage(damage, transform.position, directionToEnemy);
                }
            }
        }
    }
}