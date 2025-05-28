/*
 *---------------------------------------------------------------
 * TutorialLabelBehaviour.cs
 *---------------------------------------------------------------
 * 설명
 * 이 스크립트는 튜토리얼 단계에서 플레이어 머리 위 등 특정 위치에
 * 안내 문구(Label)를 따라다니도록 표시하는 기능을 담당합니다. 
 * - 지정한 부모 Transform 의 월드 좌표 + 오프셋 위치로 항상 이동합니다.
 * - Activate() 로 문구, 부모 트랜스폼, 오프셋을 받아 활성화하며
 *   Disable() 로 비활성화합니다.
 * - Unity 2023 기준 베스트 프랙티스를 반영하여 작성되었습니다.
 *---------------------------------------------------------------*/

using TMPro;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// 튜토리얼 안내 라벨 UI 를 부모 오브젝트 위치에 맞춰 따라다니게 하는 컴포넌트
    /// </summary>
    [DisallowMultipleComponent]                 // 하나의 게임 오브젝트에 중복 부착 방지
    [RequireComponent(typeof(Animation))]       // Animation 컴포넌트 필수
    public class TutorialLabelBehaviour : MonoBehaviour
    {
        #region Inspector Fields ------------------------------------------------

        [SerializeField, Tooltip("라벨에 표시될 텍스트를 관리하는 TextMeshProUGUI 컴포넌트")]
        private TextMeshProUGUI _label;

        #endregion

        #region Private Fields ---------------------------------------------------

        private Animation _labelAnimation;   // 애니메이션 재생용 컴포넌트 (깜빡임 등)
        private Transform _parentTransform;  // 위치를 따라갈 부모 Transform
        private Vector3 _offset;             // 부모 위치에서의 상대 오프셋

        #endregion

        #region Unity Event Functions -------------------------------------------

        /// <summary>
        /// 컴포넌트 초기화 – Animation 캐시
        /// </summary>
        private void Awake()
        {
            _labelAnimation = GetComponent<Animation>();
        }

        /// <summary>
        /// 매 프레임 부모 위치 + 오프셋만큼 이동
        /// </summary>
        private void Update()
        {
            if (_parentTransform == null) return;
            transform.position = _parentTransform.position + _offset;
        }

        #endregion

        #region Public API -------------------------------------------------------

        /// <summary>
        /// 라벨을 활성화하며 표시할 텍스트, 부모 Transform, 오프셋을 설정합니다.
        /// </summary>
        /// <param name="text">Label 에 표시할 문자열</param>
        /// <param name="parentTransform">따라다닐 부모 Transform</param>
        /// <param name="offset">부모 기준 오프셋</param>
        public void Activate(string text, Transform parentTransform, Vector3 offset)
        {
            _parentTransform = parentTransform;
            _offset          = offset;

            _label.text      = text;

            gameObject.SetActive(true);
            _labelAnimation.enabled = true;
        }

        /// <summary>
        /// 라벨을 비활성화합니다.
        /// </summary>
        public void Disable()
        {
            gameObject.SetActive(false);
            _labelAnimation.enabled = false;
        }

        #endregion
    }
}
