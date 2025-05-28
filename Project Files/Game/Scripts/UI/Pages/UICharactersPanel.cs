//====================================================================================================
// 해당 스크립트: UICharactersPanel.cs
// 기능: 캐릭터 업그레이드 패널 UI를 관리하고 표시합니다.
// 용도: 캐릭터 목록을 보여주고, 각 캐릭터의 상태(잠금 해제, 업그레이드 가능 여부)를 표시하며,
//      캐릭터 선택 및 업그레이드 기능을 제공합니다. 또한 캐릭터 등장 애니메이션을 관리합니다.
//====================================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIUpgradesAbstractPage<CharacterPanelUI, CharacterData>
    {
        [Space]
        [Tooltip("스테이지 별 프리팹입니다. 캐릭터 업그레이드 단계에 따라 표시됩니다.")]
        [SerializeField] private GameObject stageStarPrefab;

        private CharactersDatabase charactersDatabase; // 캐릭터 데이터베이스

        private Pool stageStarPool; // 스테이지 별 오브젝트 풀

        /// <summary>
        /// 현재 선택된 캐릭터의 인덱스를 가져오는 프로퍼티입니다.
        /// 캐릭터 인덱스를 안전하게 반환합니다.
        /// </summary>
        protected override int SelectedIndex => Mathf.Clamp(CharactersController.GetCharacterIndex(CharactersController.SelectedCharacter), 0, int.MaxValue);

        /// <summary>
        /// 스테이지 별 오브젝트 풀에서 오브젝트를 가져오는 함수입니다.
        /// </summary>
        /// <returns>풀링된 스테이지 별 게임 오브젝트</returns>
        public GameObject GetStageStarObject()
        {
            return stageStarPool.GetPooledObject();
        }

        /// <summary>
        /// 현재 캐릭터 패널들 중에 플레이어가 수행할 수 있는 액션(새로운 캐릭터 해제 또는 업그레이드)이 있는지 확인하는 함수입니다.
        /// </summary>
        /// <returns>수행 가능한 액션이 하나라도 있으면 true, 없으면 false를 반환합니다.</returns>
        public bool IsAnyActionAvailable()
        {
            // 모든 아이템 패널을 순회하며 새로운 캐릭터 해제 또는 다음 업그레이드 가능 여부 확인
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].IsNewCharacterOpened())
                    return true;

                if (itemPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false; // 수행 가능한 액션이 없으면 false 반환
        }

        /// <summary>
        /// 게임패드 버튼 태그를 활성화하는 함수입니다.
        /// 캐릭터 패널에서 사용되는 게임패드 버튼 태그를 설정합니다.
        /// </summary>
        protected override void EnableGamepadButtonTag()
        {
            // UI 게임패드 버튼의 Characters 태그 활성화
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Characters);
        }

        #region Animation

        private bool isAnimationPlaying; // 현재 애니메이션이 재생 중인지 나타내는 플래그
        private Coroutine animationCoroutine; // 현재 실행 중인 애니메이션 코루틴

        private static bool isControlBlocked = false; // 애니메이션 재생 중 컨트롤이 차단되었는지 나타내는 플래그
        /// <summary>
        /// 애니메이션 재생으로 인해 컨트롤이 차단되었는지 확인하는 프로퍼티입니다.
        /// </summary>
        public static bool IsControlBlocked => isControlBlocked;

        private static List<CharacterDynamicAnimation> characterDynamicAnimations = new List<CharacterDynamicAnimation>(); // 캐릭터 동적 애니메이션 목록

        /// <summary>
        /// 현재 실행 중인 애니메이션을 리셋하는 함수입니다.
        /// 코루틴을 중지하고 애니메이션 관련 변수를 초기화합니다.
        /// </summary>
        private void ResetAnimations()
        {
            // 애니메이션이 재생 중이면 중지
            if (isAnimationPlaying)
            {
                StopCoroutine(animationCoroutine);

                isAnimationPlaying = false;
                animationCoroutine = null;
            }

            // 캐릭터 동적 애니메이션 목록 초기화
            characterDynamicAnimations = new List<CharacterDynamicAnimation>();
        }

        /// <summary>
        /// 캐릭터 동적 애니메이션을 시작하는 함수입니다.
        /// 애니메이션이 목록에 있으면 코루틴을 시작합니다.
        /// </summary>
        private void StartAnimations()
        {
            // 애니메이션이 이미 재생 중이면 리턴
            if (isAnimationPlaying)
                return;

            // 캐릭터 동적 애니메이션 목록이 비어있지 않으면 애니메이션 시작
            if (!characterDynamicAnimations.IsNullOrEmpty())
            {
                isControlBlocked = true; // 컨트롤 차단
                scrollView.enabled = false; // 스크롤 뷰 비활성화

                isAnimationPlaying = true; // 애니메이션 재생 플래그 설정

                // 동적 애니메이션 코루틴 시작
                animationCoroutine = StartCoroutine(DynamicAnimationCoroutine());
            }
        }

        /// <summary>
        /// 특정 캐릭터 패널로 스크롤하는 코루틴 함수입니다.
        /// 대상 패널이 보이도록 스크롤 뷰를 이동합니다.
        /// </summary>
        /// <param name="characterPanelUI">스크롤할 대상 캐릭터 패널 UI</param>
        private IEnumerator ScrollCoroutine(CharacterPanelUI characterPanelUI)
        {
            // 대상 패널이 보이도록 스크롤 뷰의 콘텐츠 위치 계산
            float scrollOffsetX = -(characterPanelUI.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);

            // 현재 스크롤 위치와 목표 스크롤 위치의 차이 계산
            float positionDiff = Mathf.Abs(scrollView.content.anchoredPosition.x - scrollOffsetX);

            // 위치 차이가 일정 값보다 크면 스크롤 애니메이션 실행
            if (positionDiff > 80)
            {
                // CubicOut 보간 함수 가져오기
                Ease.IEasingFunction easeFunctionCubicIn = Ease.GetFunction(Ease.Type.CubicOut);

                Vector2 currentPosition = scrollView.content.anchoredPosition; // 현재 스크롤 위치
                Vector2 targetPosition = new Vector2(scrollOffsetX, 0); // 목표 스크롤 위치

                float speed = positionDiff / 2500; // 스크롤 속도 계산

                // Lerp를 사용하여 부드럽게 스크롤
                for (float s = 0; s < 1.0f; s += Time.deltaTime / speed)
                {
                    scrollView.content.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, easeFunctionCubicIn.Interpolate(s));

                    yield return null; // 다음 프레임까지 대기
                }
            }
        }

        /// <summary>
        /// 캐릭터 동적 애니메이션을 순차적으로 실행하는 코루틴 함수입니다.
        /// 각 애니메이션 항목에 대해 스크롤 및 지정된 딜레이 후 애니메이션을 시작합니다.
        /// </summary>
        private IEnumerator DynamicAnimationCoroutine()
        {
            int currentAnimationIndex = 0; // 현재 재생 중인 애니메이션 인덱스
            CharacterDynamicAnimation tempAnimation; // 현재 애니메이션 데이터
            WaitForSeconds delayWait = new WaitForSeconds(0.4f); // 초기 딜레이 대기 시간

            yield return delayWait; // 초기 딜레이 대기

            // 모든 동적 애니메이션 순회 및 실행
            while (currentAnimationIndex < characterDynamicAnimations.Count)
            {
                tempAnimation = characterDynamicAnimations[currentAnimationIndex]; // 현재 애니메이션 데이터 가져오기

                delayWait = new WaitForSeconds(tempAnimation.Delay); // 현재 애니메이션의 딜레이 설정

                yield return StartCoroutine(ScrollCoroutine(tempAnimation.CharacterPanel)); // 해당 캐릭터 패널로 스크롤

                tempAnimation.OnAnimationStarted?.Invoke(); // 애니메이션 시작 시 콜백 함수 호출

                yield return delayWait; // 애니메이션 딜레이 대기

                currentAnimationIndex++; // 다음 애니메이션으로 이동
            }

            yield return null; // 코루틴 종료 전 대기

            isAnimationPlaying = false; // 애니메이션 재생 플래그 해제
            isControlBlocked = false; // 컨트롤 차단 해제
            scrollView.enabled = true; // 스크롤 뷰 활성화
        }

        /// <summary>
        /// 캐릭터 동적 애니메이션 목록에 새로운 애니메이션을 추가하는 함수입니다.
        /// 우선 순위에 따라 목록의 시작 또는 끝에 추가할 수 있습니다.
        /// </summary>
        /// <param name="characterDynamicAnimation">추가할 캐릭터 동적 애니메이션 목록</param>
        /// <param name="isPrioritize">true이면 목록 시작에 추가 (우선 순위 높음), false이면 목록 끝에 추가</param>
        public void AddAnimations(List<CharacterDynamicAnimation> characterDynamicAnimation, bool isPrioritize = false)
        {
            // 우선 순위에 따라 애니메이션 목록에 추가
            if (!isPrioritize)
            {
                characterDynamicAnimations.AddRange(characterDynamicAnimation);
            }
            else
            {
                characterDynamicAnimations.InsertRange(0, characterDynamicAnimation);
            }
        }

        #endregion

        #region UI Page

        /// <summary>
        /// UI 페이지 초기화 함수입니다.
        /// 기본 초기화 후 캐릭터 데이터베이스를 가져오고 스테이지 별 풀을 생성하며,
        /// 각 캐릭터에 대한 패널을 생성하고 초기화합니다.
        /// </summary>
        public override void Init()
        {
            base.Init(); // 부모 클래스의 Init 호출

            // 캐릭터 데이터베이스 가져오기
            charactersDatabase = CharactersController.GetDatabase();

            // 스테이지 별 프리팹을 사용하여 오브젝트 풀 생성
            stageStarPool = new Pool(stageStarPrefab, stageStarPrefab.name);

            // 모든 캐릭터에 대해 캐릭터 패널 생성 및 초기화
            for (int i = 0; i < charactersDatabase.Characters.Length; i++)
            {
                var newPanel = AddNewPanel(); // 새로운 캐릭터 패널 추가
                newPanel.Init(charactersDatabase.Characters[i], this); // 캐릭터 데이터 및 UICharactersPanel 참조와 함께 패널 초기화
            }
        }

        /// <summary>
        /// 오브젝트가 파괴될 때 호출되는 함수입니다.
        /// 생성된 오브젝트 풀을 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 스테이지 별 오브젝트 풀이 존재하면 파괴
            if(stageStarPool != null)
            {
                PoolManager.DestroyPool(stageStarPool);
            }
        }

        /// <summary>
        /// UI 페이지 표시 애니메이션을 실행하는 함수입니다.
        /// 애니메이션을 리셋하고 기본 표시 애니메이션 및 캐릭터 동적 애니메이션을 시작합니다.
        /// </summary>
        public override void PlayShowAnimation()
        {
            ResetAnimations(); // 애니메이션 리셋

            base.PlayShowAnimation(); // 부모 클래스의 표시 애니메이션 호출

            StartAnimations(); // 캐릭터 동적 애니메이션 시작
        }

        /// <summary>
        /// UI 페이지 숨김 애니메이션을 실행하는 함수입니다.
        /// 배경 패널을 아래로 이동시키는 애니메이션을 실행하고 완료 시 페이지 닫힘 이벤트를 호출합니다.
        /// </summary>
        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation(); // 부모 클래스의 숨김 애니메이션 호출

            // 배경 패널을 아래로 이동시키는 애니메이션 시작 (CubicIn 보간)
            backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                // UI 컨트롤러에 페이지 닫힘 이벤트 알림
                UIController.OnPageClosed(this);
            });
        }

        /// <summary>
        /// UI 페이지를 숨기는 함수입니다.
        /// UIController를 통해 이 페이지를 숨깁니다.
        /// </summary>
        /// <param name="onFinish">숨김 애니메이션 완료 시 호출될 콜백 함수</param>
        protected override void HidePage(SimpleCallback onFinish)
        {
            // UIController를 사용하여 UICharactersPanel 페이지 숨김
            UIController.HidePage<UICharactersPanel>(onFinish);
        }

        /// <summary>
        /// 특정 캐릭터 데이터에 해당하는 캐릭터 패널 UI를 가져오는 함수입니다.
        /// </summary>
        /// <param name="character">찾으려는 캐릭터 데이터</param>
        /// <returns>해당 캐릭터 데이터와 일치하는 CharacterPanelUI 객체, 없으면 null 반환</returns>
        public override CharacterPanelUI GetPanel(CharacterData character)
        {
            // 모든 아이템 패널을 순회하며 해당 캐릭터 데이터와 일치하는 패널 찾기
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].Character == character)
                    return itemPanels[i]; // 일치하는 패널 찾으면 반환
            }

            return null; // 일치하는 패널이 없으면 null 반환
        }

        #endregion
    }
}