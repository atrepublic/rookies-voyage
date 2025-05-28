// 스크립트 기능 요약:
// 이 스크립트는 프로젝트에서 사용되는 모든 화폐(통화) 타입의 목록을 저장하는 ScriptableObject입니다.
// 게임 디자인 단계에서 사용될 화폐들을 정의하고 관리하는 데이터 컨테이너 역할을 합니다.
// Unity 에디터의 CreateAssetMenu 속성을 통해 에디터 내에서 에셋 파일로 쉽게 생성할 수 있습니다.

using UnityEngine; // ScriptableObject, SerializeField, CreateAssetMenu 속성 사용을 위해 필요

namespace Watermelon
{
    // CreateAssetMenu 속성은 Unity 에디터에서 이 클래스의 에셋 파일을 생성할 수 있도록 메뉴 항목을 추가합니다.
    // fileName은 기본 파일 이름, menuName은 에디터 메뉴 경로를 지정합니다.
    [CreateAssetMenu(fileName = "Currency Database", menuName = "Data/Core/Currency Database")]
    // CurrencyDatabase 클래스는 모든 화폐 정보를 담는 ScriptableObject입니다.
    public class CurrencyDatabase : ScriptableObject
    {
        // currencies: 이 데이터베이스에 포함된 모든 Currency 객체들의 배열입니다.
        // Inspector에서 직접 각 화폐의 설정을 지정할 수 있습니다.
        [SerializeField]
        [Tooltip("이 데이터베이스에 포함된 모든 화폐 객체 목록")]
        Currency[] currencies;
        // Currencies 속성: currencies 배열을 읽기 전용으로 제공합니다.
        public Currency[] Currencies => currencies;
    }
}