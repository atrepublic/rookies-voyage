// 스크립트 설명: 무기 카드 드롭 아이템의 동작을 관리하는 클래스입니다.
// 카드 데이터 설정, 보상 적용, 시각적 표시 및 파티클 효과 처리를 포함합니다.
using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스
using TMPro; // TextMeshPro 사용을 위한 네임스페이스
using System.Collections.Generic; // List 사용을 위한 네임스페이스
using Watermelon; // Tween 관련 네임스페이스 (BaseDropBehavior에서 상속받았거나 필요하다고 가정)

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 무기 카드 드롭의 동작을 관리합니다. 카드 데이터 설정 및 보상 적용 기능을 포함합니다.
    /// </summary>
    public class WeaponCardDropBehavior : BaseDropBehavior // BaseDropBehavior 상속 (이전 파일에서 정의된 것으로 가정)
    {
        [SerializeField]
        [Tooltip("무기 카드의 아이템 이미지를 표시하는 Image 컴포넌트")] // 주요 변수 한글 툴팁
        Image itemImage; // 아이템 이미지

        [SerializeField]
        [Tooltip("무기 카드의 배경 이미지를 표시하는 Image 컴포넌트 (희귀도 색상 적용)")] // 주요 변수 한글 툴팁
        Image backImage; // 카드 배경 이미지

        [SerializeField]
        [Tooltip("무기 카드의 이름을 표시하는 TextMeshProUGUI 컴포넌트")] // 주요 변수 한글 툴팁
        TextMeshProUGUI titleText; // 무기 이름 텍스트

        [SerializeField]
        [Tooltip("착지 시 스케일 애니메이션이 적용될 파티클 오브젝트")] // 주요 변수 한글 툴팁
        GameObject particleObject; // 파티클 효과를 담는 오브젝트

        [SerializeField]
        [Tooltip("희귀도에 따라 색상이 변경될 파티클 시스템 목록")] // 주요 변수 한글 툴팁
        List<ParticleSystem> rarityParticles = new List<ParticleSystem>(); // 희귀도별 파티클 시스템 목록

        private TweenCase scaleTweenCase; // 스케일 애니메이션을 위한 트윈 케이스

        /// <summary>
        /// 무기 카드 드롭에 무기 데이터를 설정합니다. 시각적 요소 및 파티클 효과를 업데이트합니다.
        /// </summary>
        /// <param name="weapon">설정할 무기 데이터.</param>
        public void SetCardData(WeaponData weapon)
        {
            if (weapon == null)
            {
                Debug.LogError("무기 데이터가 Null입니다!"); // 한글 로그 메시지

                return;
            }

            // 무기 데이터에 기반하여 시각적 요소 업데이트
            itemImage.sprite = weapon.Icon; // 무기 아이콘 설정
            backImage.color = weapon.RarityData.MainColor; // 희귀도 주 색상으로 배경색 설정 (WeaponData에 RarityData가 정의된 것으로 가정)
            titleText.text = weapon.WeaponName; // 무기 이름 텍스트 설정

            // 희귀도에 따라 파티클 색상 변경
            if (rarityParticles != null && rarityParticles.Count > 0)
            {
                foreach (ParticleSystem particle in rarityParticles)
                {
                    if (particle != null)
                    {
                        ParticleSystem.MainModule main = particle.main;
                        // 파티클 시작 색상의 알파값은 유지하고 RGB만 희귀도 색상으로 변경
                        main.startColor = weapon.RarityData.MainColor.SetAlpha(main.startColor.color.a); // SetAlpha 확장 메서드는 Watermelon 네임스페이스에 정의된 것으로 가정
                    }
                }
            }
        }

        /// <summary>
        /// 무기 카드 드롭 아이템 획득에 대한 보상을 적용합니다.
        /// (현재 이 스크립트에서는 별도의 보상 로직이 구현되어 있지 않습니다.)
        /// </summary>
        /// <param name="autoReward">자동 보상 적용 여부.</param>
        public override void ApplyReward(bool autoReward = false)
        {
            // 무기 카드 획득 시 보상 로직 (필요하다면 여기에 추가)
            // 예: 인벤토리에 카드 추가 등
        }

        /// <summary>
        /// 무기 카드 드롭을 언로드하고, 활성화된 트윈을 중지하며 게임 오브젝트를 파괴합니다.
        /// </summary>
        public override void Unload()
        {
            scaleTweenCase.KillActive(); // 활성화된 스케일 트윈 중지

            // 기본 언로드 로직 호출 (게임 오브젝트 파괴 등)
            base.Unload();
        }

        /// <summary>
        /// 무기 카드 드롭이 착지했을 때 호출되며, 파티클 효과의 스케일 애니메이션을 트리거합니다.
        /// </summary>
        public override void OnItemLanded()
        {
            // 착지 시 파티클 오브젝트에 스케일 애니메이션 적용
            if (particleObject != null)
            {
                // DOScale 트윈 애니메이션 실행 (Watermelon 네임스페이스에 정의된 것으로 가정)
                scaleTweenCase = particleObject.transform.DOScale(7f, 0.2f).SetEasing(Ease.Type.SineOut);
            }
        }
    }
}