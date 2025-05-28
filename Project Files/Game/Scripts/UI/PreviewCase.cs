//====================================================================================================
// 해당 스크립트: PreviewCase.cs
// 기능: 개별 레벨 미리보기 오브젝트와 해당 동작을 관리하는 클래스입니다.
// 용도: 레벨 진행 패널에서 각 레벨을 나타내는 미리보기 오브젝트의 게임 오브젝트, RectTransform,
//      그리고 레벨 미리보기 동작 스크립트를 캡슐화합니다.
//====================================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class PreviewCase
    {
        private GameObject gameObject; // 레벨 미리보기 게임 오브젝트
        private LevelTypeSettings levelTypeSettings; // 레벨 타입 관련 설정

        private RectTransform rectTransform; // 레벨 미리보기 오브젝트의 RectTransform
        /// <summary>
        /// 레벨 미리보기 오브젝트의 RectTransform에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public RectTransform RectTransform => rectTransform;

        private LevelPreviewBaseBehaviour previewBehaviour; // 레벨 미리보기 동작 스크립트
        /// <summary>
        /// 레벨 미리보기 오브젝트에 연결된 LevelPreviewBaseBehaviour 스크립트에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public LevelPreviewBaseBehaviour PreviewBehaviour => previewBehaviour;

        /// <summary>
        /// PreviewCase 클래스의 생성자입니다.
        /// 레벨 미리보기 게임 오브젝트와 레벨 타입 설정을 받아 초기화합니다.
        /// </summary>
        /// <param name="gameObject">레벨 미리보기로 사용할 게임 오브젝트</param>
        /// <param name="levelTypeSettings">해당 레벨의 타입 설정</param>
        public PreviewCase(GameObject gameObject, LevelTypeSettings levelTypeSettings)
        {
            this.gameObject = gameObject; // 게임 오브젝트 설정
            this.levelTypeSettings = levelTypeSettings; // 레벨 타입 설정

            // 게임 오브젝트의 RectTransform 가져오기
            rectTransform = (RectTransform)gameObject.transform;

            // 게임 오브젝트에서 LevelPreviewBaseBehaviour 컴포넌트 가져오기 및 초기화
            previewBehaviour = gameObject.GetComponent<LevelPreviewBaseBehaviour>();
            previewBehaviour.Init();
        }

        /// <summary>
        /// 미리보기 케이스를 리셋하는 함수입니다.
        /// 일반적으로 오브젝트 풀링을 위해 비활성화할 때 사용됩니다.
        /// </summary>
        public void Reset()
        {
            // 게임 오브젝트 비활성화
            gameObject.SetActive(false);
        }
    }
}