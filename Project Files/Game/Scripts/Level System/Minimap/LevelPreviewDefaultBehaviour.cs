// 이 스크립트는 일반 레벨의 미리보기 동작을 정의합니다.
// LevelPreviewBaseBehaviour를 상속받아 UI 요소의 활성화/비활성화를 통해
// 레벨의 현재 상태(기본, 잠김, 선택됨)를 시각적으로 표현합니다.
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    // 일반 레벨 미리보기 동작을 담당하는 클래스입니다.
    // LevelPreviewBaseBehaviour의 추상 메소드들을 오버라이드하여 일반 레벨의 UI 상태를 관리합니다.
    public class LevelPreviewDefaultBehaviour : LevelPreviewBaseBehaviour
    {
        // 레벨이 기본 상태일 때 표시되는 원형 이미지입니다.
        [Tooltip("레벨이 기본 상태일 때 표시되는 원형 이미지입니다.")]
        [SerializeField] Image circleDefaultImage;
        // 레벨이 잠금 상태일 때 표시되는 원형 이미지입니다.
        [Tooltip("레벨이 잠금 상태일 때 표시되는 원형 이미지입니다.")]
        [SerializeField] Image circleLockedImage;
        // 레벨이 선택되었을 때 표시되는 이미지입니다.
        [Tooltip("레벨이 선택되었을 때 표시되는 이미지입니다.")]
        [SerializeField] Image selectedImage;

        // 일반 레벨 미리보기를 초기화하는 메소드입니다.
        // 현재 구현은 비어 있습니다. 필요에 따라 초기화 로직을 추가할 수 있습니다.
        public override void Init()
        {
            // 초기화 로직 (필요하다면 추가)
        }

        // 일반 레벨 미리보기를 활성화 (선택) 상태로 설정하는 메소드입니다.
        // 선택된 이미지를 활성화하고 기본 이미지를 표시하며, 잠금 이미지는 숨깁니다.
        // animate: 활성화 시 애니메이션을 적용할지 여부 (현재 미사용)
        public override void Activate(bool animate = false)
        {
            // 선택된 이미지를 활성화합니다.
            selectedImage.gameObject.SetActive(true);
            // 기본 상태 이미지를 활성화합니다.
            circleDefaultImage.gameObject.SetActive(true);

            // 잠금 상태 이미지를 비활성화합니다.
            circleLockedImage.gameObject.SetActive(false);
        }

        // 일반 레벨 미리보기를 완료 상태로 설정하는 메소드입니다.
        // 기본 이미지를 표시하고, 선택 및 잠금 이미지는 숨깁니다.
        public override void Complete()
        {
            // 기본 상태 이미지를 활성화합니다.
            circleDefaultImage.gameObject.SetActive(true);

            // 선택된 이미지와 잠금 상태 이미지를 비활성화합니다.
            selectedImage.gameObject.SetActive(false);
            circleLockedImage.gameObject.SetActive(false);
        }

        // 일반 레벨 미리보기를 잠금 상태로 설정하는 메소드입니다.
        // 잠금 이미지를 활성화하고, 기본 및 선택 이미지는 숨깁니다.
        public override void Lock()
        {
            // 잠금 상태 이미지를 활성화합니다.
            circleLockedImage.gameObject.SetActive(true);

            // 기본 상태 이미지와 선택된 이미지를 비활성화합니다.
            circleDefaultImage.gameObject.SetActive(false);
            selectedImage.gameObject.SetActive(false);
        }
    }
}