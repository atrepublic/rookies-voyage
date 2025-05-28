using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextController : MonoBehaviour
    {
        private static FloatingTextController floatingTextController;

        [SerializeField] FloatingTextCase[] floatingTextCases;
        private Dictionary<int, FloatingTextCase> floatingTextLink;

        public void Init()
        {
            floatingTextController = this;

            floatingTextLink = new Dictionary<int, FloatingTextCase>();
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            Debug.Log($"[FloatingTextController] Init 시작. 설정된 floatingTextCases 개수: {(floatingTextCases != null ? floatingTextCases.Length.ToString() : "null")}");
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            for (int i = 0; i < floatingTextCases.Length; i++)
            {
                FloatingTextCase floatingText = floatingTextCases[i];
                if (string.IsNullOrEmpty(floatingText.Name))
                {
                    Debug.LogError("[Floating Text]: Floating Text initialization failed. A unique name (ID) must be provided.", this);
                    continue;
                }

                if (floatingText.FloatingTextBehavior == null)
                {
                    Debug.LogError($"Floating Text ({floatingText.Name}) initialization failed. No Floating Text Behavior linked.", this);
                    continue;
                }

                floatingText.Init();
                int nameHash = floatingText.Name.GetHashCode(); // 해시값 미리 계산
                if (!floatingTextLink.ContainsKey(nameHash)) // 중복 추가 방지
                {
                    floatingTextLink.Add(nameHash, floatingText);
                    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                    Debug.Log($"[FloatingTextController] Init: '{floatingText.Name}' (Hash: {nameHash}) 케이스가 floatingTextLink에 추가됨.");
                    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                }
                else
                {
                    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                    Debug.LogWarning($"[FloatingTextController] Init: '{floatingText.Name}' (Hash: {nameHash}) 케이스는 이미 floatingTextLink에 존재하여 중복 추가하지 않음.");
                    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                }
            }
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            Debug.Log($"[FloatingTextController] Init 완료. 최종 floatingTextLink 개수: {(floatingTextLink != null ? floatingTextLink.Count.ToString() : "null")}");
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }

        private void OnDestroy()
        {
            if (!floatingTextCases.IsNullOrEmpty())
            {
                for (int i = 0; i < floatingTextCases.Length; i++)
                {
                    PoolManager.DestroyPool(floatingTextCases[i].FloatingTextPool);
                }
            }
        }

        // ===== 오버로드: 기본 Spawn 메서드 =====
        // (기존 오버로드 메서드들은 변경 없이 그대로 둡니다)
        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, Vector3 position)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), string.Empty, position, Quaternion.identity, 1.0f, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, Vector3 position)
        {
            return SpawnFloatingText(floatingTextNameHash, string.Empty, position, Quaternion.identity, 1.0f, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, Quaternion.identity, 1.0f, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position)
        {
            return SpawnFloatingText(floatingTextNameHash, text, position, Quaternion.identity, 1.0f, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, 1.0f, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation)
        {
            return SpawnFloatingText(floatingTextNameHash, text, position, rotation, 1.0f, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, scaleMultiplier, Color.white);
        }

        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier)
        {
            return SpawnFloatingText(floatingTextNameHash, text, position, rotation, scaleMultiplier, Color.white);
        }

        // 이 메서드는 bool isCritical 인자가 없으므로, 그대로 둡니다.
        public static FloatingTextBaseBehavior SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, scaleMultiplier, color);
        }

        // 이 메서드는 bool isCritical 인자가 없으므로, 그대로 둡니다.
        // 만약 이 메서드가 isCritical=false를 가정하고 Activate(..., color)만 호출한다면, 
        // 그리고 MiniGunBehavior가 이 메서드를 호출한다면 isCritical 정보가 유실될 수 있습니다.
        // 하지만 MiniGunBehavior는 isCritical을 받는 오버로드를 호출하므로 괜찮습니다.
        public static FloatingTextBaseBehavior SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color)
        {
            if (floatingTextController.floatingTextLink.ContainsKey(floatingTextNameHash))
            {
                FloatingTextCase floatingTextCase = floatingTextController.floatingTextLink[floatingTextNameHash];

                GameObject floatingTextObject = floatingTextCase.FloatingTextPool.GetPooledObject();
                floatingTextObject.transform.position = position;
                floatingTextObject.transform.rotation = rotation;
                floatingTextObject.SetActive(true);

                FloatingTextBaseBehavior floatingTextBehavior = floatingTextObject.GetComponent<FloatingTextBaseBehavior>();
                floatingTextBehavior.Activate(text, scaleMultiplier, color); // bool isCritical 인자 없는 Activate 호출

                return floatingTextBehavior;
            }

            return null;
        }


        // ===== [추가] 치명타 여부까지 포함하는 오버로드 =====

        /// <summary>
        /// 문자열 이름 기반, 치명타 여부 포함 텍스트 생성
        /// </summary>
        public static FloatingTextBaseBehavior SpawnFloatingText(
            string floatingTextName, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color, bool isCritical)
        {
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // 이 메서드는 단순히 아래 해시코드 기반 메서드를 호출하므로, 핵심 로그는 아래 메서드에 집중합니다.
            // Debug.Log($"[FloatingTextController] Spawn (string name) 호출됨 - Name: '{floatingTextName}', isCritical: {isCritical}");
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, scaleMultiplier, color, isCritical);
        }

        /// <summary>
        /// 해시값 기반, 치명타 여부 포함 텍스트 생성
        /// </summary>
        public static FloatingTextBaseBehavior SpawnFloatingText(
            int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scaleMultiplier, Color color, bool isCritical)
        {
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            if (floatingTextController == null)
            {
     //           Debug.LogError("[FloatingTextController] Spawn (hash name) 시도: floatingTextController가 null. Init() 호출 확인 필요.");
                return null;
            }
            if (floatingTextController.floatingTextLink == null)
            {
      //          Debug.LogError("[FloatingTextController] Spawn (hash name) 시도: floatingTextLink가 null. Init() 완료 확인 필요.");
                return null;
            }
      //      Debug.Log($"[FloatingTextController] Spawn (hash name) 요청 받음 - Hash: {floatingTextNameHash}, Text: '{text}', isCritical: {isCritical}");
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            if (floatingTextController.floatingTextLink.ContainsKey(floatingTextNameHash))
            {
                FloatingTextCase floatingTextCase = floatingTextController.floatingTextLink[floatingTextNameHash];

                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
      //          Debug.Log($"[FloatingTextController] '{floatingTextCase.Name}' (Hash: {floatingTextNameHash}) 케이스 찾음. Activate 호출 직전 -> isCritical 값: {isCritical}, 텍스트: '{text}', 전달될 색상: {color}");
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                
                GameObject floatingTextObject = floatingTextCase.FloatingTextPool.GetPooledObject();
                floatingTextObject.transform.position = position;
                floatingTextObject.transform.rotation = rotation;
                floatingTextObject.SetActive(true);

                FloatingTextBaseBehavior floatingTextBehavior = floatingTextObject.GetComponent<FloatingTextBaseBehavior>();
                floatingTextBehavior.Activate(text, scaleMultiplier, color, isCritical); // [핵심] 이 부분에서 Activate가 호출됩니다.

                return floatingTextBehavior;
            }
            else // 키를 찾지 못한 경우
            {
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [ 로그 추가 ] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
     //          Debug.LogError($"[FloatingTextController] Spawn 실패: Hash '{floatingTextNameHash}'에 해당하는 FloatingTextCase를 찾을 수 없습니다. (요청된 텍스트: '{text}')");
                if (floatingTextController.floatingTextLink != null && floatingTextController.floatingTextLink.Count > 0)
                {
      //              Debug.LogWarning("[FloatingTextController] 현재 등록된 케이스 목록:");
                    foreach (var pair in floatingTextController.floatingTextLink)
                    {
     //                   Debug.LogWarning($"  - 이름: '{pair.Value.Name}', 등록된 Hash: {pair.Key}");
                    }
                } else {
      //              Debug.LogWarning("[FloatingTextController] 현재 floatingTextLink에 등록된 케이스가 없습니다.");
                }
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ [ 로그 추가 완료 ] ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                return null;
            }
        }

        public static void Unload()
        {
            // floatingTextController가 null일 경우를 대비한 방어 코드 추가
            if (floatingTextController == null || floatingTextController.floatingTextCases.IsNullOrEmpty()) return;

            FloatingTextCase[] floatingTextCases = floatingTextController.floatingTextCases;
            for (int i = 0; i < floatingTextCases.Length; i++)
            {
                // floatingTextCases[i].FloatingTextPool이 null일 수도 있으므로 확인
                if (floatingTextCases[i] != null && floatingTextCases[i].FloatingTextPool != null)
                {
                    PoolManager.DestroyPool(floatingTextCases[i].FloatingTextPool);
                }
            }
        }
    }
}