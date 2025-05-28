/*
📄 SettingsElementsGroup.cs 요약 정리
이 스크립트는 설정 UI 내에서 여러 관련 UI 요소(예: 버튼, 슬라이더, 토글 세트)들을
하나의 그룹으로 묶는 역할을 하는 컴포넌트입니다.
주요 기능은 그룹 내에 활성화된 자식 요소가 하나라도 있는지 확인하는 것입니다.

⭐ 주요 기능
- IsGroupActive() 메서드를 통해 그룹 내에 표시되고 있는 (활성화된) UI 요소가 있는지 여부를 반환합니다.
  이는 예를 들어, 특정 플랫폼에서만 보여야 하는 설정 그룹 전체를 동적으로 표시하거나 숨길 때,
  또는 그룹의 높이를 계산할 때 사용될 수 있습니다.

🛠️ 사용 용도
- UISettings와 같은 설정 UI 페이지에서 여러 설정 항목들을 시각적 또는 기능적으로 그룹화할 때 사용됩니다.
- GamepadUISettings와 같은 스크립트에서 이 그룹 단위로 게임패드 탐색 로직을 구성할 수 있습니다.
- UI 레이아웃 계산 시, 비활성 그룹은 크기 계산에서 제외하는 등의 용도로 활용될 수 있습니다.
*/

using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 설정 UI 내에서 여러 관련 요소들을 그룹화하는 컴포넌트입니다.
    /// 이 그룹 자체가 활성화 상태인지 (즉, 내부에 활성화된 자식 요소가 있는지) 확인할 수 있는 기능을 제공합니다.
    /// </summary>
    public class SettingsElementsGroup : MonoBehaviour
    {
        /// <summary>
        /// 이 설정 요소 그룹 내에 활성화된 자식 게임 오브젝트가 하나라도 있는지 확인합니다.
        /// 그룹 전체가 실질적으로 사용자에게 보여지거나 상호작용 가능한 상태인지를 판단하는 데 사용될 수 있습니다.
        /// </summary>
        /// <returns>활성화된 자식 요소가 하나 이상 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.</returns>
        public bool IsGroupActive()
        {
            int childCount = transform.childCount; // 그룹의 직접적인 자식 요소 수를 가져옵니다.
            for(int i = 0; i < childCount; i++)
            {
                // 각 자식 요소의 게임 오브젝트가 활성화(activeSelf) 상태인지 확인합니다.
                if(transform.GetChild(i).gameObject.activeSelf)
                {
                    // 활성화된 자식 요소를 하나라도 찾으면 즉시 true를 반환합니다.
                    return true;
                }
            }

            // 모든 자식 요소를 확인했지만 활성화된 것이 하나도 없으면 false를 반환합니다.
            return false;
        }
    }
}