// 스크립트 설명: 게임 내 드롭 시스템을 관리하는 정적 클래스입니다.
// 드롭 아이템 등록, 초기화, 생성, 제거, 자동 획득 등의 기능을 제공합니다.
using System;
using System.Collections.Generic;
using System.Linq; // LINQ 네임스페이스 추가
using UnityEngine;
using Watermelon.SquadShooter;
using Random = UnityEngine.Random; // UnityEngine.Random 사용 명시

namespace Watermelon.LevelSystem
{
    public static class Drop
    {
        // 등록된 모든 드롭 아이템 목록
        private static List<IDropItem> dropItems = new List<IDropItem>();

        // 드롭 애니메이션 설정 배열
        private static DropAnimation[] dropAnimations;

        // 현재 활성화 상태인 드롭 오브젝트 목록
        private static List<BaseDropBehavior> activeObjects = new List<BaseDropBehavior>();

        /// <summary>
        /// 드롭 시스템을 지정된 설정으로 초기화합니다.
        /// </summary>
        /// <param name="dropSettings">드롭 시스템을 위한 설정 데이터.</param>
        public static void Init(DropableItemSettings dropSettings)
        {
            if (dropSettings == null)
            {
                Debug.LogError("DropableItemSettings는 Null이 될 수 없습니다."); // 한글 로그 메시지

                return;
            }

            // 활성화된 드롭 오브젝트 목록 초기화
            activeObjects = new List<BaseDropBehavior>();

            // 드롭 애니메이션 설정 가져오기
            dropAnimations = dropSettings.DropAnimations;

            // 사용자 정의 드롭 아이템 등록
            foreach (CustomDropItem customDropItem in dropSettings.CustomDropItems)
            {
                RegisterDropItem(customDropItem);
            }

            // 화폐 드롭 아이템 등록
            CurrencyDropItem currencyDropItem = new CurrencyDropItem();
            // CurrencyController에서 화폐 목록을 가져와 설정 (CurrencyController에 정의된 것으로 가정)
            currencyDropItem.SetCurrencies(CurrencyController.Currencies);
            RegisterDropItem(currencyDropItem);

            // 무기 드롭 아이템 등록 (WeaponDropItem에 정의된 것으로 가정)
            RegisterDropItem(new WeaponDropItem());

            // 캐릭터 드롭 아이템 등록 (CharacterDropItem에 정의된 것으로 가정)
            RegisterDropItem(new CharacterDropItem());
        }

        /// <summary>
        /// 자동 획득 가능한 모든 아이템을 자동으로 수집합니다.
        /// </summary>
        public static void AutoCollect()
        {
            // 활성화된 오브젝트 목록이 비어있지 않다면
            if (!activeObjects.IsNullOrEmpty()) // IsNullOrEmpty 확장 메서드 사용 (Watermelon 네임스페이스에 정의된 것으로 가정)
            {
                // 각 아이템에 대해
                foreach (BaseDropBehavior item in activeObjects)
                {
                    // 아이템이 아직 획득되지 않았고 자동 획득 가능하다면
                    if (!item.IsPicked && item.IsAutoPickable)
                    {
                        item.ApplyReward(true); // 자동 보상 적용
                    }
                }
            }
        }

        /// <summary>
        /// 등록된 모든 드롭 아이템을 언로드하고 드롭 시스템을 초기화합니다.
        /// </summary>
        public static void Unload()
        {
            // 등록된 드롭 아이템 목록이 비어있지 않다면
            if (!dropItems.IsNullOrEmpty())
            {
                // 각 드롭 아이템을 언로드
                foreach (IDropItem item in dropItems)
                {
                    item.Unload();
                }

                dropItems.Clear(); // 드롭 아이템 목록 비우기
            }

            dropAnimations = null; // 드롭 애니메이션 초기화
        }

        /// <summary>
        /// 룸이 로드될 때 호출되며, 모든 활성화된 드롭 오브젝트를 파괴합니다.
        /// </summary>
        public static void OnRoomLoaded()
        {
            // 활성화된 오브젝트가 있다면 파괴
            if (activeObjects.Count > 0)
                DestroyActiveObjects();
        }

        /// <summary>
        /// 모든 활성화된 드롭 오브젝트를 파괴합니다.
        /// </summary>
        public static void DestroyActiveObjects()
        {
            // 활성화된 오브젝트 목록이 비어있거나 null이면 종료
            if (activeObjects.IsNullOrEmpty()) return;

            // 각 활성화된 오브젝트를 언로드 (파괴 포함)
            foreach (BaseDropBehavior activeObject in activeObjects)
            {
                activeObject?.Unload(); // null 체크 후 언로드 호출 (Unity 2023+ 문법)
            }

            activeObjects.Clear(); // 활성화된 오브젝트 목록 비우기
        }

        /// <summary>
        /// 활성화된 오브젝트 목록에서 드롭 오브젝트를 제거합니다.
        /// </summary>
        /// <param name="dropObject">제거할 드롭 오브젝트.</param>
        public static void RemoveObject(BaseDropBehavior dropObject)
        {
            activeObjects.Remove(dropObject); // 목록에서 드롭 오브젝트 제거
        }

        /// <summary>
        /// 새로운 드롭 아이템을 등록합니다.
        /// </summary>
        /// <param name="dropItem">등록할 드롭 아이템.</param>
        public static void RegisterDropItem(IDropItem dropItem)
        {
            if (dropItem == null)
            {
                Debug.LogError("드롭 아이템은 Null이 될 수 없습니다."); // 한글 로그 메시지

                return;
            }

#if UNITY_EDITOR // 유니티 에디터 환경에서만 실행
            // 이미 같은 타입의 드롭 아이템이 등록되어 있는지 확인
            if (dropItems.Exists(item => item.DropItemType == dropItem.DropItemType))
            {
                Debug.LogError($"타입 {dropItem.DropItemType}의 드롭 아이템이 이미 등록되어 있습니다!"); // 한글 로그 메시지

                return;
            }
#endif

            dropItems.Add(dropItem); // 드롭 아이템 목록에 추가

            dropItem.Init(); // 드롭 아이템 초기화
        }

        /// <summary>
        /// 드롭 아이템 타입을 기준으로 드롭 아이템을 가져옵니다.
        /// </summary>
        /// <param name="dropableItemType">가져올 드롭 아이템의 타입.</param>
        /// <returns>찾은 드롭 아이템 또는 null.</returns>
        public static IDropItem GetDropItem(DropableItemType dropableItemType)
        {
            // LINQ의 Find 메서드를 사용하여 해당 타입의 드롭 아이템 검색
            return dropItems.Find(item => item.DropItemType == dropableItemType);
        }

        /// <summary>
        /// 낙하 스타일에 따라 드롭 애니메이션을 가져옵니다.
        /// </summary>
        /// <param name="dropFallingStyle">드롭 애니메이션의 낙하 스타일.</param>
        /// <returns>찾은 드롭 애니메이션 또는 null.</returns>
        public static DropAnimation GetAnimation(DropFallingStyle dropFallingStyle)
        {
            // 드롭 애니메이션 배열에서 해당 낙하 스타일과 일치하는 애니메이션 검색 (LINQ 사용)
            return dropAnimations?.FirstOrDefault(animation => animation.FallStyle == dropFallingStyle); // null 조건부 연산자 사용 (Unity 2023+ 문법)
        }

        /// <summary>
        /// 지정된 위치에 파라미터에 따라 아이템을 드롭합니다.
        /// </summary>
        /// <param name="dropData">드롭 아이템 데이터.</param>
        /// <param name="spawnPosition">아이템이 생성될 위치.</param>
        /// <param name="rotation">아이템의 회전.</param>
        /// <param name="fallingStyle">아이템의 낙하 스타일.</param>
        /// <param name="availableToPickDelay">아이템을 주울 수 있게 되기까지의 지연 시간.</param>
        /// <param name="autoPickDelay">아이템이 자동으로 주어지기까지의 지연 시간.</param>
        /// <param name="rewarded">드롭이 보상 아이템인지 여부.</param>
        /// <returns>드롭된 아이템의 게임 오브젝트.</returns>
        private static GameObject DropItem(DropData dropData, Vector3 spawnPosition, Vector3 rotation, DropFallingStyle fallingStyle, float availableToPickDelay = -1f, float autoPickDelay = -1f, bool rewarded = false)
        {
            IDropItem dropItem = GetDropItem(dropData.DropType); // 드롭 타입에 맞는 드롭 아이템 가져오기
            if (dropItem == null) return null; // 없으면 null 반환

            GameObject dropPrefab = dropItem.GetDropObject(dropData); // 드롭 아이템에 해당하는 프리팹/모델 가져오기
            if (dropPrefab == null) return null; // 없으면 null 반환

            // 프리팹 인스턴스 생성
            GameObject itemGameObject = GameObject.Instantiate(dropItem.GetDropObject(dropData));

            // BaseDropBehavior 컴포넌트 가져오기
            BaseDropBehavior item = itemGameObject.GetComponent<BaseDropBehavior>();
            item.Init(dropData, availableToPickDelay, autoPickDelay); // 드롭 동작 초기화
            item.IsRewarded = rewarded; // 보상 여부 설정

            // 아이템 위치, 스케일, 회전 설정
            itemGameObject.transform.position = spawnPosition + (Random.insideUnitSphere * 0.05f); // 생성 위치에 약간의 랜덤 오프셋 추가
            itemGameObject.transform.localScale = Vector3.one;
            itemGameObject.transform.eulerAngles = rotation;
            itemGameObject.SetActive(true); // 오브젝트 활성화

            activeObjects.Add(item); // 활성화된 오브젝트 목록에 추가

            return itemGameObject; // 생성된 게임 오브젝트 반환
        }

        /// <summary>
        /// 드롭 오브젝트를 지정된 낙하 스타일로 던집니다.
        /// </summary>
        /// <param name="baseDropBehavior">던질 드롭 오브젝트의 BaseDropBehavior 컴포넌트.</param>
        /// <param name="fallingStyle">아이템의 낙하 스타일.</param>
        public static void ThrowItem(BaseDropBehavior baseDropBehavior, DropFallingStyle fallingStyle)
        {
            DropAnimation dropAnimation = GetAnimation(fallingStyle); // 낙하 스타일에 맞는 애니메이션 설정 가져오기

            Transform itemTransform = baseDropBehavior.transform;
            Vector3 spawnPosition = itemTransform.position; // 현재 위치

            // 애니메이션 오프셋 적용
            itemTransform.position = itemTransform.position + new Vector3(0, dropAnimation.OffsetY, 0);

            // 랜덤한 목표 위치 계산
            // GetRandomPositionAroundObject 및 AddToY 확장 메서드는 Watermelon 네임스페이스에 정의된 것으로 가정
            Vector3 targetPosition = spawnPosition.GetRandomPositionAroundObject(dropAnimation.Radius * 0.9f, dropAnimation.Radius * 1.2f).AddToY(0.1f);

            // 드롭 오브젝트를 목표 위치로 던지는 애니메이션 실행
            baseDropBehavior.Throw(targetPosition, dropAnimation, dropAnimation.FallTime);
        }

        /// <summary>
        /// 드롭 아이템을 생성하고 필요에 따라 던지는 애니메이션을 적용합니다.
        /// </summary>
        /// <param name="dropData">생성할 드롭 아이템 데이터.</param>
        /// <param name="spawnPosition">아이템이 생성될 위치.</param>
        /// <param name="rotation">아이템의 초기 회전.</param>
        /// <param name="isRewarded">생성되는 드롭이 보상 아이템인지 여부.</param>
        /// <param name="spawnCallback">아이템 생성 후 호출될 콜백 함수.</param>
        public static void SpawnDropItem(DropData dropData, Vector3 spawnPosition, Vector3 rotation, bool isRewarded, Action<BaseDropBehavior, DropFallingStyle> spawnCallback = null)
        {
            // 드롭 타입에 따라 생성 및 처리 로직 분기
            if (dropData.DropType == DropableItemType.Currency)
            {
                // 화폐 드롭의 경우 여러 개로 분산하여 드롭
                int itemsAmount = Mathf.Clamp(Random.Range(9, 11), 1, dropData.Amount); // 드롭할 아이템 개수 결정

                // 총 수량을 드롭할 아이템 개수로 분할
                List<int> itemValues = LevelController.SplitIntEqually(dropData.Amount, itemsAmount); // LevelController에 정의된 것으로 가정

                for (int j = 0; j < itemsAmount; j++)
                {
                    int tempIndex = j;
                    // 각 아이템 생성에 약간의 지연 시간 적용
                    Tween.DelayedCall(j * 0.01f, () => // Tween에 정의된 것으로 가정
                    {
                        DropData data = dropData.Clone(); // 드롭 데이터 복제
                        data.Amount = itemValues[tempIndex]; // 분할된 수량 설정

                        // 화폐 아이템 생성 및 초기화
                        GameObject dropObject = Drop.DropItem(data, spawnPosition, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Coin, itemValues[tempIndex], 0.5f, rewarded: isRewarded);
                        if (dropObject != null)
                        {
                            CurrencyDropBehavior dropBehavior = dropObject.GetComponent<CurrencyDropBehavior>(); // CurrencyDropBehavior 컴포넌트 가져오기
                            if (dropBehavior != null)
                            {
                                dropBehavior.SetCurrencyData(data.CurrencyType, itemValues[tempIndex]); // 화폐 데이터 설정

                                spawnCallback?.Invoke(dropBehavior, DropFallingStyle.Coin); // 콜백 호출
                            }
                        }
                    });
                }
            }
            else if (dropData.DropType == DropableItemType.WeaponCard)
            {
                // 무기 카드 드롭의 경우 개수만큼 생성
                for (int j = 0; j < dropData.Amount; j++)
                {
                    // 무기 카드 아이템 생성 및 초기화
                    GameObject dropObject = Drop.DropItem(dropData, spawnPosition, Vector3.zero, DropFallingStyle.Default, 1, 0.6f);
                    if (dropObject != null)
                    {
                        WeaponCardDropBehavior card = dropObject.GetComponent<WeaponCardDropBehavior>(); // WeaponCardDropBehavior 컴포넌트 가져오기 (정의된 것으로 가정)
                        if(card != null)
                        {
                            card.SetCardData(dropData.Weapon); // 카드 데이터 설정

                            spawnCallback?.Invoke(card, DropFallingStyle.Default); // 콜백 호출
                        }
                    }
                }
            }
            else if (dropData.DropType == DropableItemType.Character)
            {
                // 캐릭터 드롭의 경우 1개 생성
                GameObject dropObject = Drop.DropItem(dropData, spawnPosition, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Default, 1);
                if (dropObject != null)
                {
                    CharacterDropBehavior characterDropBehavior = dropObject.GetComponent<CharacterDropBehavior>(); // CharacterDropBehavior 컴포넌트 가져오기 (정의된 것으로 가정)
                    if(characterDropBehavior != null)
                    {
                        characterDropBehavior.SetCharacterData(dropData.Character, dropData.Level); // 캐릭터 데이터 설정

                        spawnCallback?.Invoke(characterDropBehavior, DropFallingStyle.Default); // 콜백 호출
                    }
                }
            }
            else if (dropData.DropType == DropableItemType.Weapon)
            {
                // 무기 드롭의 경우 1개 생성
                GameObject dropObject = Drop.DropItem(dropData, spawnPosition, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Default, 1);
                if (dropObject != null)
                {
                    WeaponDropBehavior weaponDropBehavior = dropObject.GetComponent<WeaponDropBehavior>(); // WeaponDropBehavior 컴포넌트 가져오기 (정의된 것으로 가정)
                    if(weaponDropBehavior != null)
                    {
                        weaponDropBehavior.SetWeaponData(dropData.Weapon, dropData.Level); // 무기 데이터 설정

                        spawnCallback?.Invoke(weaponDropBehavior, DropFallingStyle.Default); // 콜백 호출
                    }
                }
            }
            else if (dropData.DropType == DropableItemType.Heal)
            {
                // 회복 아이템 드롭의 경우 1개 생성
                GameObject dropObject = Drop.DropItem(dropData, spawnPosition, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Default, 1);
                if (dropObject != null)
                {
                    HealDropBehaviour healDropBehaviour = dropObject.GetComponent<HealDropBehaviour>(); // HealDropBehaviour 컴포넌트 가져오기 (정의된 것으로 가정)
                    if (healDropBehaviour != null)
                    {
                        healDropBehaviour.SetData(dropData.Amount); // 회복량 설정

                        spawnCallback?.Invoke(healDropBehaviour, DropFallingStyle.Default); // 콜백 호출
                    }
                }
            }
            else
            {
                // 그 외의 드롭 타입 처리
                GameObject dropObject = Drop.DropItem(dropData, spawnPosition, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Default, 1);
                if (dropObject != null)
                {
                    BaseDropBehavior dropBehavior = dropObject.GetComponent<BaseDropBehavior>(); // BaseDropBehavior 컴포넌트 가져오기
                    if (dropBehavior != null)
                    {
                        spawnCallback?.Invoke(dropBehavior, DropFallingStyle.Default); // 콜백 호출
                    }
                }
            }
        }
    }
}