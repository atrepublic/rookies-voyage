/*
 * CharacterStageData.cs
 * ---------------------
 * 이 스크립트는 캐릭터의 특정 '단계(Stage)'에 대한 데이터를 정의합니다.
 * 캐릭터가 진화(Evolve)하면 이 단계가 변경될 수 있으며,
 * 각 단계는 고유한 외형(스프라이트, 프리팹)과 체력 바 위치 등을 가집니다.
 */

using UnityEngine;

namespace Watermelon.SquadShooter
{
    // 직렬화 가능한 클래스로 선언하여 Inspector에서 편집하고 저장할 수 있도록 함
    [System.Serializable]
    public class CharacterStageData
    {
        [Tooltip("캐릭터 선택 창 등에서 보여줄 미리보기 스프라이트")]
        [SerializeField] Sprite previewSprite;
        // 외부에서 미리보기 스프라이트에 접근하기 위한 프로퍼티
        public Sprite PreviewSprite => previewSprite;

        [Tooltip("캐릭터가 잠겨있을 때 보여줄 스프라이트")]
        [SerializeField] Sprite lockedSprite;
        // 외부에서 잠금 상태 스프라이트에 접근하기 위한 프로퍼티
        public Sprite LockedSprite => lockedSprite;

        [Tooltip("이 단계의 캐릭터 외형을 나타내는 게임 오브젝트 프리팹")]
        [SerializeField] GameObject prefab;
        // 외부에서 캐릭터 프리팹에 접근하기 위한 프로퍼티
        public GameObject Prefab => prefab;

        [Tooltip("이 단계에서 캐릭터 머리 위에 표시될 체력 바의 상대적 위치 오프셋")]
        [SerializeField] Vector3 healthBarOffset;
        // 외부에서 체력 바 오프셋에 접근하기 위한 프로퍼티
        public Vector3 HealthBarOffset => healthBarOffset;
    }
}