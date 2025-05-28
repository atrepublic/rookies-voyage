// 스크립트 설명: 게임 내 카메라 시스템을 관리하는 컨트롤러 클래스입니다.
// 메인 카메라, 가상 카메라 전환, 타겟 추적 및 이동 오프셋 계산 등의 기능을 담당합니다.
using System.Collections.Generic; // Dictionary, List 사용을 위한 네임스페이스
using Unity.Cinemachine; // Cinemachine 관련 네임스페이스
using UnityEngine;
using Watermelon.SquadShooter; // BaseEnemyBehavior 사용을 위한 네임스페이스
using Watermelon; // 네임스페이스 (추가적인 유틸리티 함수 포함 가능)

namespace Watermelon
{
    public class CameraController : MonoBehaviour
    {
        // 활성 가상 카메라의 우선순위 (높을수록 우선)
        private const int ACTIVE_CAMERA_PRIORITY = 100;
        // 비활성 가상 카메라의 우선순위
        private const int UNACTIVE_CAMERA_PRIORITY = 0;

        // CameraController 싱글톤 인스턴스
        private static CameraController cameraController;

        [SerializeField]
        [Tooltip("씬의 Cinemachine Brain 컴포넌트")] // 주요 변수 한글 툴팁
        CinemachineBrain cameraBrain; // 시네머신 브레인

        [SerializeField]
        [Tooltip("초기 활성화될 카메라 타입")] // 주요 변수 한글 툴팁
        CameraType firstCamera; // 게임 시작 시 첫 카메라 타입

        [Space]
        [SerializeField]
        [Tooltip("관리할 가상 카메라 설정 목록")] // 주요 변수 한글 툴팁
        VirtualCameraCase[] virtualCameras; // 가상 카메라 설정 배열

        [Header("플레이어 전방 이동에 따른 카메라 시프트 설정")] // 헤더 추가
        [SerializeField]
        [Tooltip("플레이어 전방 이동 시 X축 카메라 오프셋 계수")] // 주요 변수 한글 툴팁
        float forwardX = 4f; // 전방 이동 X 오프셋 계수

        [SerializeField]
        [Tooltip("플레이어 전방 이동 시 Z축 카메라 오프셋 계수")] // 주요 변수 한글 툴팁
        float forwardZ = 1f; // 전방 이동 Z 오프셋 계수

        [SerializeField]
        [Tooltip("플레이어 전방 오프셋 보간(Lerp) 속도 계수")] // 주요 변수 한글 툴팁
        float forwardLerpMultiplier = 4f; // 전방 오프셋 보간 속도

        [Header("적 타겟에 따른 카메라 시프트 설정")] // 헤더 추가
        [SerializeField]
        [Tooltip("적 타겟 방향 시 X축 카메라 오프셋 계수")] // 주요 변수 한글 툴팁
        float enemyShiftX = 4f; // 적 타겟 X 오프셋 계수

        [SerializeField]
        [Tooltip("적 타겟 방향 시 Z축 카메라 오프셋 계수")] // 주요 변수 한글 툴팁
        float enemyShiftZ = 1f; // 적 타겟 Z 오프셋 계수

        [SerializeField]
        [Tooltip("적 타겟 오프셋 보간(Lerp) 속도 계수")] // 주요 변수 한글 툴팁
        float enemyShiftLerpMultiplier = 4f; // 적 타겟 오프셋 보간 속도

        // 가상 카메라 타입과 인덱스를 연결하는 딕셔너리
        private static Dictionary<CameraType, int> virtualCamerasLink;

        // 메인 카메라 컴포넌트
        private static Camera mainCamera;
        // 외부에서 메인 카메라에 접근하기 위한 프로퍼티
        public static Camera MainCamera => mainCamera;

        // 카메라가 추적하는 주 타겟 트랜스폼
        private static Transform mainTarget;
        // 외부에서 메인 타겟에 접근하기 위한 프로퍼티
        public static Transform MainTarget => mainTarget;

        // 현재 활성화된 가상 카메라 설정
        private static VirtualCameraCase activeVirtualCamera;
        // 외부에서 활성화된 가상 카메라에 접근하기 위한 프로퍼티
        public static VirtualCameraCase ActiveVirtualCamera => activeVirtualCamera;

        // 카메라가 실제로 따라가는 내부 타겟 트랜스폼 (메인 타겟 + 오프셋)
        private Transform internalTarget;

        // 플레이어 전방 이동에 따른 카메라 오프셋
        private Vector3 forward = Vector3.zero;
        // 적 타겟 방향에 따른 카메라 오프셋 (정적 변수)
        private static Vector3 enemyDirection = Vector3.zero;
        // 현재 타겟으로 설정된 적 (정적 변수)
        private static BaseEnemyBehavior targetEnemy; // BaseEnemyBehavior는 Watermelon.SquadShooter에 정의된 것으로 가정

        /// <summary>
        /// 카메라 컨트롤러를 초기화하고 메인 타겟을 설정합니다.
        /// 가상 카메라들을 준비하고 첫 카메라를 활성화합니다.
        /// </summary>
        /// <param name="target">카메라가 추적할 메인 타겟 트랜스폼.</param>
        public void Init(Transform target)
        {
            cameraController = this; // 싱글톤 인스턴스 설정

            // 메인 카메라 컴포넌트 가져오기
            mainCamera = GetComponent<Camera>();

            // 내부 타겟 오브젝트 생성
            internalTarget = new GameObject("[Internal Camera Target]").transform;

            mainTarget = target; // 메인 타겟 설정
            internalTarget.position = target.position; // 내부 타겟 위치 초기화

            // 가상 카메라 타입과 인덱스를 연결하는 딕셔너리 초기화
            virtualCamerasLink = new Dictionary<CameraType, int>();
            for(int i = 0; i < virtualCameras.Length; i++)
            {
                VirtualCameraCase camera = virtualCameras[i];
                if(camera != null)
                {
                    camera.Init(); // 가상 카메라 설정 초기화

                    CinemachineCamera virtualCamera = camera.VirtualCamera;
                    virtualCamera.Follow = internalTarget; // 가상 카메라의 Follow 타겟을 내부 타겟으로 설정
                    virtualCamera.LookAt = internalTarget; // 가상 카메라의 LookAt 타겟을 내부 타겟으로 설정
                    virtualCamera.ForceCameraPosition(target.position, target.rotation); // 가상 카메라 위치 강제 설정

                    // 딕셔너리에 카메라 타입과 인덱스 추가
                    virtualCamerasLink.Add(camera.CameraType, i);
                }
            }

            // 첫 카메라 활성화
            EnableCamera(firstCamera);

            cameraBrain.enabled = true; // Cinemachine Brain 활성화

            // 모든 가상 카메라의 초기 위치를 타겟 위치로 강제 설정
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                VirtualCameraCase camera = virtualCameras[i];
                if (camera != null)
                {
                    CinemachineCamera virtualCamera = camera.VirtualCamera;
                    virtualCamera.ForceCameraPosition(target.position, target.rotation);
                }
            }
        }

        /// <summary>
        /// 카메라가 따라갈 적 타겟을 설정합니다.
        /// </summary>
        /// <param name="enemy">타겟으로 설정할 적 행동 컴포넌트.</param>
        public static void SetEnemyTarget(BaseEnemyBehavior enemy)
        {
            targetEnemy = enemy; // 적 타겟 설정
        }

        /// <summary>
        /// LateUpdate는 모든 Update 함수가 호출된 후 각 프레임마다 호출됩니다.
        /// 카메라의 내부 타겟 위치를 메인 타겟과 이동 오프셋을 기반으로 업데이트합니다.
        /// </summary>
        private void LateUpdate()
        {
            // 플레이어 전방 이동에 따른 오프셋 계산
            float z = mainTarget.forward.z * forwardZ;
            float x = mainTarget.forward.x * forwardX;
            forward = Vector3.Lerp(forward, new Vector3(x, 0, z), Time.deltaTime * forwardLerpMultiplier); // 보간 적용

            // 적 타겟 방향에 따른 오프셋 계산
            // targetEnemy가 null이 아니면 적 위치 - 메인 타겟 위치의 정규화된 벡터, null이면 Vector3.zero
            Vector3 currentEnemyDirection = targetEnemy ? (targetEnemy.transform.position - mainTarget.position).normalized : Vector3.zero; // 삼항 연산자 사용

            currentEnemyDirection.x *= enemyShiftX; // X축 오프셋 계수 적용
            currentEnemyDirection.z *= enemyShiftZ; // Z축 오프셋 계수 적용

            enemyDirection = Vector3.Lerp(enemyDirection, currentEnemyDirection, Time.deltaTime * enemyShiftLerpMultiplier); // 보간 적용

            // 내부 타겟 위치 업데이트 (메인 타겟 위치 + 전방 오프셋 + 적 타겟 오프셋)
            internalTarget.position = mainTarget.position + forward + enemyDirection;
        }

        /// <summary>
        /// 지정된 카메라 타입에 해당하는 VirtualCameraCase 설정을 가져옵니다.
        /// </summary>
        /// <param name="cameraType">가져올 가상 카메라의 타입.</param>
        /// <returns>해당 타입의 VirtualCameraCase 객체.</returns>
        public static VirtualCameraCase GetCamera(CameraType cameraType)
        {
            // 딕셔너리를 사용하여 카메라 타입에 해당하는 인덱스를 찾고 VirtualCameraCase 배열에서 가져옴
            return cameraController.virtualCameras[virtualCamerasLink[cameraType]];
        }

        /// <summary>
        /// 지정된 카메라 타입의 가상 카메라를 활성화합니다.
        /// 다른 가상 카메라는 비활성화됩니다.
        /// </summary>
        /// <param name="cameraType">활성화할 가상 카메라의 타입.</param>
        public static void EnableCamera(CameraType cameraType)
        {
            // 이미 활성화된 카메라와 같은 타입이라면 중복 처리 방지
            if (activeVirtualCamera != null && activeVirtualCamera.CameraType == cameraType)
                return;

            // 모든 가상 카메라의 우선순위를 비활성 상태로 설정
            for (int i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Priority = UNACTIVE_CAMERA_PRIORITY;
            }

            // 활성화할 가상 카메라를 찾고 우선순위를 활성 상태로 설정
            activeVirtualCamera = cameraController.virtualCameras[virtualCamerasLink[cameraType]];
            activeVirtualCamera.VirtualCamera.Priority = ACTIVE_CAMERA_PRIORITY;
        }
    }
}