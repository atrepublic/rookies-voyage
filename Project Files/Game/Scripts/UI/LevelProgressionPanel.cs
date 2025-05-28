//====================================================================================================
// 해당 스크립트: LevelProgressionPanel.cs
// 기능: 게임의 레벨 진행 상황 패널을 표시하고 관리합니다.
// 용도: 현재 월드와 다음 월드의 미리보기를 표시하며, 각 레벨의 상태(활성, 완료, 잠금)를 시각적으로 나타냅니다.
//====================================================================================================
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class LevelProgressionPanel : MonoBehaviour
    {
        [Tooltip("레벨 미리보기 오브젝트들이 배치될 컨테이너 Transform입니다.")]
        [SerializeField] private Transform levelPreviewContainer;

        [Space]
        [Tooltip("현재 월드를 나타내는 게임 오브젝트입니다.")]
        [SerializeField] private GameObject currentWorldObject;
        [Tooltip("현재 월드의 미리보기 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] private Image currentWorldImage;

        [Tooltip("다음 월드를 나타내는 게임 오브젝트입니다.")]
        [SerializeField] private GameObject nextWorldObject;
        [Tooltip("다음 월드의 미리보기 이미지를 표시하는 Image 컴포넌트입니다.")]
        [SerializeField] private Image nextWorldImage;

        [Space]
        [Tooltip("현재 레벨을 가리키는 화살표의 RectTransform입니다.")]
        [SerializeField] private RectTransform arrowRectTransform;

        private LevelsDatabase levelsDatabase; // 레벨 데이터베이스
        private GameSettings levelSettings; // 게임 설정 (레벨 관련)

        private PreviewCase[] previewCases; // 각 레벨 미리보기 케이스 배열

        private CanvasGroup canvasGroup; // 패널의 투명도 조정을 위한 CanvasGroup 컴포넌트
        private LevelSave levelSave; // 레벨 저장 데이터

        private TweenCase fadeTweenCase; // 패널 페이드 애니메이션 트윈 케이스

        /// <summary>
        /// 레벨 진행 패널을 초기화하는 함수입니다.
        /// 필요한 컴포넌트와 데이터를 가져옵니다.
        /// </summary>
        public void Init()
        {
            // CanvasGroup 컴포넌트 가져오기
            canvasGroup = GetComponent<CanvasGroup>();

            // 게임 설정 및 레벨 데이터베이스 가져오기
            levelSettings = GameSettings.GetSettings();
            levelsDatabase = levelSettings.LevelsDatabase;

            // 레벨 저장 데이터 가져오기
            levelSave = SaveController.GetSaveObject<LevelSave>("level");
        }

        /// <summary>
        /// 현재 레벨 진행 상태에 따라 패널을 로드하고 표시하는 함수입니다.
        /// 월드 및 레벨 미리보기 오브젝트들을 설정합니다.
        /// </summary>
        public void LoadPanel()
        {
            // 현재 월드 및 레벨 인덱스 가져오기
            int currentWorldIndex = levelSave.WorldIndex;
            int currentLevelIndex = levelSave.LevelIndex;

            // 현재 및 다음 월드 데이터 가져오기
            WorldData currentWorld = levelsDatabase.GetWorld(currentWorldIndex);
            WorldData nextWorld = levelsDatabase.GetWorld(currentWorldIndex + 1);

            // 기존 미리보기 오브젝트들 초기화 (풀링된 경우)
            if (previewCases != null)
            {
                for (int i = 0; i < previewCases.Length; i++)
                {
                    previewCases[i].Reset();
                }
            }

            // 화살표 오브젝트 부모 리셋
            arrowRectTransform.SetParent(transform);

            // 현재 월드 데이터가 있을 경우 패널 활성화 및 설정
            if (currentWorld != null)
            {
                // 패널 게임 오브젝트 활성화
                gameObject.SetActive(true);

                // 현재 월드 미리보기 이미지 설정 (스프라이트가 없으면 기본 스프라이트 사용)
                currentWorldImage.sprite = currentWorld.PreviewSprite != null ? currentWorld.PreviewSprite : levelSettings.DefaultWorldSprite;

                // 다음 월드 미리보기 이미지 설정 및 오브젝트 활성화/비활성화
                if (nextWorld != null)
                {
                    nextWorldImage.sprite = nextWorld.PreviewSprite != null ? nextWorld.PreviewSprite : levelSettings.DefaultWorldSprite;
                    nextWorldObject.SetActive(true);
                }
                else
                {
                    nextWorldObject.SetActive(false);
                }

                // 레벨 미리보기 케이스 배열 초기화
                previewCases = new PreviewCase[currentWorld.Levels.Length];
                for (int i = 0; i < previewCases.Length; i++)
                {
                    // 현재 레벨에 해당하는 레벨 타입 설정 가져오기
                    LevelTypeSettings levelTypeSettings = levelSettings.GetLevelSettings(currentWorld.Levels[i].Type);

                    // 레벨 미리보기 오브젝트 인스턴스화 및 부모 설정
                    GameObject previewObject = Instantiate(levelTypeSettings.PreviewObject);
                    previewObject.transform.SetParent(levelPreviewContainer);
                    previewObject.transform.ResetLocal(); // 로컬 위치, 회전, 스케일 리셋
                    previewObject.transform.localScale = Vector3.one; // 스케일 1로 설정
                    previewObject.transform.SetAsLastSibling(); // 마지막 자식으로 설정

                    // PreviewCase 인스턴스 생성 및 초기화
                    previewCases[i] = new PreviewCase(previewObject, levelTypeSettings);

                    // 현재 레벨 상태에 따라 미리보기 오브젝트 활성화/완료/잠금 상태 설정
                    if (currentLevelIndex == i)
                    {
                        previewCases[i].PreviewBehaviour.Activate(true); // 현재 레벨 활성화

                        // 화살표를 현재 레벨 미리보기 오브젝트의 부모로 설정 및 위치 리셋
                        arrowRectTransform.SetParent(previewCases[i].RectTransform);
                        arrowRectTransform.ResetLocal();
                    }
                    else if (currentLevelIndex > i)
                    {
                        previewCases[i].PreviewBehaviour.Complete(); // 이전 레벨 완료 상태 설정
                    }
                    else if (currentLevelIndex < i)
                    {
                        previewCases[i].PreviewBehaviour.Lock(); // 이후 레벨 잠금 상태 설정
                    }
                }

                // 다음 월드 오브젝트를 마지막 자식으로 설정
                nextWorldObject.transform.SetAsLastSibling();
            }
            else
            {
                // 현재 월드 데이터가 없으면 패널 비활성화
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 패널을 서서히 나타나게 하는 함수입니다.
        /// 페이드 인 애니메이션을 실행합니다.
        /// </summary>
        public void Show()
        {
            // 기존 페이드 트윈 케이스 중단
            fadeTweenCase.KillActive();

            // 투명도를 1.0으로 페이드 인 애니메이션 시작 (CircIn 보간)
            fadeTweenCase = canvasGroup.DOFade(1.0f, 0.3f).SetEasing(Ease.Type.CircIn);
        }

        /// <summary>
        /// 패널을 서서히 사라지게 하는 함수입니다.
        /// 페이드 아웃 애니메이션을 실행합니다.
        /// </summary>
        public void Hide()
        {
            // 기존 페이드 트윈 케이스 중단
            fadeTweenCase.KillActive();

            // 투명도를 0.0으로 페이드 아웃 애니메이션 시작 (CircIn 보간)
            fadeTweenCase = canvasGroup.DOFade(0.0f, 0.3f).SetEasing(Ease.Type.CircIn);
        }
    }
}