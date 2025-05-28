/*
 * MovementSettings.cs
 * ---------------------
 * 이 스크립트는 캐릭터 또는 다른 개체의 이동 관련 설정을 정의하는 데이터 클래스입니다.
 * 회전 속도, 이동 속도, 가속도, 그리고 이동 애니메이션 속도 배율 값을 포함합니다.
 * 주로 캐릭터 그래픽 설정(BaseCharacterGraphics) 등에서 참조하여 사용됩니다.
 */

using UnityEngine;
using Watermelon; // Watermelon 프레임워크 네임스페이스 (DuoFloat 등)

namespace Watermelon.SquadShooter
{
    // 직렬화 가능한 클래스로 선언하여 Inspector에서 편집하고 저장할 수 있도록 함
    [System.Serializable]
    public class MovementSettings
    {
        [Tooltip("캐릭터 또는 개체의 초당 회전 속도")]
        public float RotationSpeed;

        [Space] // 인스펙터 공백
        [Tooltip("캐릭터 또는 개체의 최대 이동 속도")]
        public float MoveSpeed;
        [Tooltip("캐릭터 또는 개체가 최대 이동 속도에 도달하기까지의 가속도")]
        public float Acceleration;

        [Space] // 인스펙터 공백
        [Tooltip("이동 속도에 따른 애니메이션 재생 속도 배율 (최소/최대값)")]
        public DuoFloat AnimationMultiplier; // DuoFloat는 최소/최대 float 값을 가지는 Watermelon 프레임워크의 커스텀 타입일 수 있음
    }
}

/*
    // Watermelon 프레임워크에 포함된 것으로 추정되는 커스텀 구조체 (예시)
    // 실제 구현은 Watermelon 프레임워크 소스 코드에 있음
    [System.Serializable]
    public struct DuoFloat
    {
        public float Min;
        public float Max;
    }
*/
