// 이 스크립트는 Unity 에디터에서 레벨을 편집하기 위한 컨트롤러입니다.
// 레벨 내의 아이템, 적, 상자, 출구 지점 및 커스텀 오브젝트를 관리하고
// 씬에 배치하거나 수집하는 기능을 제공합니다.
// 또한 현재 방의 상태를 기록하고 변경 사항을 감지하는 기능을 포함합니다.
#pragma warning disable 649

using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public class EditorSceneController : MonoBehaviour
    {
#if UNITY_EDITOR
        private static EditorSceneController instance;
        // EditorSceneController의 싱글톤 인스턴스를 가져옵니다.
        public static EditorSceneController Instance { get => instance; }

        // 씬의 모든 편집 가능한 오브젝트를 포함하는 컨테이너 오브젝트입니다.
        [Tooltip("씬의 모든 편집 가능한 오브젝트를 포함하는 컨테이너 오브젝트입니다.")]
        [SerializeField] private GameObject container;
        // 현재 방에 특화된 커스텀 오브젝트를 포함하는 컨테이너 오브젝트입니다.
        [Tooltip("현재 방에 특화된 커스텀 오브젝트를 포함하는 컨테이너 오브젝트입니다.")]
        [SerializeField] private GameObject roomCustomObjectsContainer;
        // 월드 전체에 걸쳐 있는 커스텀 오브젝트를 포함하는 컨테이너 오브젝트입니다.
        [Tooltip("월드 전체에 걸쳐 있는 커스텀 오브젝트를 포함하는 컨테이너 오브젝트입니다.")]
        [SerializeField] private GameObject worldCustomObjectsContainer;
        // 플레이어가 스폰될 위치입니다.
        [Tooltip("플레이어가 스폰될 위치입니다.")]
        [SerializeField] Vector3 spawnPoint;
        // 스폰 지점을 나타내는 기즈모 구체의 크기입니다.
        [Tooltip("스폰 지점을 나타내는 기즈모 구체의 크기입니다.")]
        [SerializeField] float spawnPointSphereSize;
        // 출구 지점을 나타내는 기즈모 구체의 크기입니다. (현재 코드에서는 사용되지 않음)
        [Tooltip("출구 지점을 나타내는 기즈모 구체의 크기입니다. (현재 코드에서는 사용되지 않음)")]
        [SerializeField] float exitPointSphereSize;
        // 스폰 지점 기즈모의 색상입니다.
        [Tooltip("스폰 지점 기즈모의 색상입니다.")]
        [SerializeField] Color spawnPointColor;
        // 기즈모를 그리기 전의 원래 색상을 저장합니다.
        private Color backupColor;
        // 스폰 지점 기즈모를 표시할지 여부를 나타냅니다.
        private bool showGizmo;

        // 새로운 저장 시스템을 위해 사용되는 데이터 필드입니다.
        // 현재 방의 내용이 변경되었는지 여부를 나타냅니다.
        private bool roomChanged;
        // 현재 방의 아이템 엔티티 데이터 배열입니다.
        private ItemEntityData[] roomItems;
        // 현재 방의 상자 엔티티 데이터 배열입니다.
        private ChestEntityData[] roomChests;
        // 현재 방의 적 엔티티 데이터 배열입니다.
        private EnemyEntityData[] roomEnemies;
        // 현재 방의 출구 지점 위치 벡터입니다.
        private Vector3 roomExitPointVector;
        // 현재 방에 출구 지점이 존재하는지 여부를 나타냅니다.
        private bool roomExitPoint;
        // 현재 방의 커스텀 오브젝트 데이터 목록입니다.
        private List<CustomObjectData> roomCustomObjects;
        // 월드의 커스텀 오브젝트 데이터 목록입니다.
        private List<CustomObjectData> worldCustomObjects;
        // 임시 출구 지점 위치 벡터입니다.
        private Vector3 tempExitPoint;

        // 컨테이너 오브젝트를 설정합니다.
        public GameObject Container { set => container = value; }
        // 스폰 지점 위치를 가져오거나 설정합니다.
        public Vector3 SpawnPoint { get => spawnPoint; set => spawnPoint = value; }
        // 스폰 지점 기즈모 색상을 가져오거나 설정합니다.
        public Color SpawnPointColor { get => spawnPointColor; set => spawnPointColor = value; }

        // EditorSceneController 클래스의 생성자입니다.
        // 싱글톤 인스턴스를 초기화합니다.
        public EditorSceneController()
        {
            instance = this;
        }

        // 지정된 프리팹을 사용하여 아이템을 씬에 스폰합니다.
        // position, rotation, scale, hash 값을 설정하고 LevelEditorItem 컴포넌트를 추가합니다.
        // 필요에 따라 스폰된 아이템을 선택할 수 있습니다.
        public void SpawnItem(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, int hash, bool selectSpawnedItem = false)
        {
            // 프리팹을 인스턴스화합니다.
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            // 컨테이너의 자식으로 설정합니다.
            gameObject.transform.SetParent(container.transform);
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            gameObject.hideFlags = HideFlags.DontSave;

            // 위치, 회전, 스케일을 설정합니다.
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;

            // LevelEditorItem 컴포넌트를 추가하고 해시 값을 설정합니다.
            LevelEditorItem levelEditorItem = gameObject.AddComponent<LevelEditorItem>();
            levelEditorItem.hash = hash;
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            levelEditorItem.hideFlags = HideFlags.DontSave;

            // 스폰된 아이템을 선택하도록 설정된 경우 해당 오브젝트를 선택합니다.
            if (selectSpawnedItem)
            {
                Selection.activeGameObject = gameObject;
            }
        }


        // 지정된 프리팹을 사용하여 적을 씬에 스폰합니다.
        // position, rotation, scale, type, isElite 값을 설정하고 LevelEditorEnemy 컴포넌트를 추가합니다.
        // 적의 이동 경로를 위한 경로 지점을 설정합니다.
        public void SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, EnemyType type, bool isElite, Vector3[] pathPoints)
        {
            // 프리팹을 인스턴스화합니다.
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            // 컨테이너의 자식으로 설정합니다.
            gameObject.transform.SetParent(container.transform);
            // 위치, 회전, 스케일을 설정합니다.
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            gameObject.hideFlags = HideFlags.DontSave;

            // LevelEditorEnemy 컴포넌트를 추가하고 타입 및 엘리트 여부를 설정합니다.
            LevelEditorEnemy levelEditorEnemy = gameObject.AddComponent<LevelEditorEnemy>();
            levelEditorEnemy.type = type;
            levelEditorEnemy.isElite = isElite;
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            levelEditorEnemy.hideFlags = HideFlags.DontSave;

            // 경로 지점을 담을 컨테이너 오브젝트를 생성합니다.
            GameObject pointsContainer = new GameObject("PathPointsContainer");
            // 적 오브젝트의 자식으로 설정합니다.
            pointsContainer.transform.SetParent(gameObject.transform);
            // LevelEditorEnemy 컴포넌트에 경로 지점 컨테이너 트랜스폼을 할당합니다.
            levelEditorEnemy.pathPointsContainer = pointsContainer.transform;
            // 경로 지점 컨테이너의 로컬 위치를 0으로 설정합니다.
            pointsContainer.transform.localPosition = Vector3.zero;

            GameObject sphere;

            // 경로 지점 배열을 순회하며 각 지점에 구체 오브젝트를 생성하고 설정합니다.
            for (int i = 0; i < pathPoints.Length; i++)
            {
                // 구체 프리미티브를 생성합니다.
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // 경로 지점 컨테이너의 자식으로 설정합니다.
                sphere.transform.SetParent(levelEditorEnemy.pathPointsContainer);
                // 적 오브젝트의 로컬 위치를 기준으로 경로 지점의 로컬 위치를 설정합니다.
                sphere.transform.localPosition = pathPoints[i] - gameObject.transform.localPosition;
                // 구체의 로컬 스케일을 설정합니다.
                sphere.transform.localScale = Vector3.one * 0.78125f;
                // LevelEditorEnemy 컴포넌트의 경로 지점 목록에 추가합니다.
                levelEditorEnemy.pathPoints.Add(sphere.transform);
            }

            // 경로 지점에 재질을 적용합니다.
            levelEditorEnemy.ApplyMaterialToPathPoints();
            // 스폰된 적 오브젝트를 선택합니다.
            Selection.activeGameObject = gameObject;
        }

        // 지정된 프리팹을 사용하여 상자를 씬에 스폰합니다.
        // position, rotation, scale, type, rewardCurrency, rewardValue, droppedCurrencyItemsAmount 값을 설정하고 LevelEditorChest 컴포넌트를 추가합니다.
        public void SpawnChest(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, LevelChestType type, CurrencyType rewardCurrency, int rewardValue, int droppedCurrencyItemsAmount)
        {
            // 프리팹을 인스턴스화합니다.
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            // 컨테이너의 자식으로 설정합니다.
            gameObject.transform.SetParent(container.transform);
            // 위치, 회전, 스케일을 설정합니다.
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            gameObject.hideFlags = HideFlags.DontSave;

            // LevelEditorChest 컴포넌트를 추가하고 관련 속성을 설정합니다.
            LevelEditorChest levelEditorChest = gameObject.AddComponent<LevelEditorChest>();
            levelEditorChest.type = type;
            levelEditorChest.rewardCurrency = rewardCurrency;
            levelEditorChest.rewardValue = rewardValue;
            levelEditorChest.droppedCurrencyItemsAmount = droppedCurrencyItemsAmount;
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            levelEditorChest.hideFlags = HideFlags.DontSave;

            // 스폰된 상자 오브젝트를 선택합니다.
            Selection.activeGameObject = gameObject;
        }

        // 컨테이너 오브젝트의 이름을 업데이트하고 기즈모 표시 여부를 설정합니다.
        // index가 -1이면 컨테이너를 숨기고 기즈모를 비활성화합니다.
        // 그렇지 않으면 방 번호를 포함하여 컨테이너 이름을 설정하고 기즈모를 활성화합니다.
        public void UpdateContainerLabel(int index)
        {
            if(index == -1)
            {
                // 인덱스가 -1이면 기본 이름으로 설정하고 기즈모를 숨깁니다.
                container.name = "Container";
                showGizmo = false;
                // 계층 구조에서 컨테이너 오브젝트들을 숨깁니다.
                container.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                roomCustomObjectsContainer.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                worldCustomObjectsContainer.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
            }
            else
            {
                // 인덱스가 유효하면 방 번호를 포함하여 이름을 설정하고 기즈모를 표시합니다.
                container.name = $"Container( Room #{index + 1})";
                showGizmo = true;
                // 계층 구조에서 컨테이너 오브젝트들을 표시합니다.
                container.hideFlags = UnityEngine.HideFlags.None;
                roomCustomObjectsContainer.hideFlags = UnityEngine.HideFlags.None;
                worldCustomObjectsContainer.hideFlags = UnityEngine.HideFlags.None;
            }
        }
        // 현재 방 컨테이너에 있는 모든 아이템 오브젝트에서 ItemEntityData를 수집합니다.
        // LevelEditorItem 컴포넌트를 찾아 해당 데이터를 ItemEntityData 배열로 반환합니다.
        public ItemEntityData[] CollectItemsFromRoom()
        {
            // 컨테이너 내의 LevelEditorItem 컴포넌트들을 모두 가져옵니다.
            LevelEditorItem[] editorData = container.GetComponentsInChildren<LevelEditorItem>();
            // ItemEntityData 배열을 생성합니다.
            ItemEntityData[] result = new ItemEntityData[editorData.Length];

            // 각 LevelEditorItem에서 필요한 데이터를 추출하여 ItemEntityData를 생성합니다.
            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new ItemEntityData(editorData[i].hash, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale);
            }

            // 수집된 ItemEntityData 배열을 반환합니다.
            return result;
        }

        // 현재 방 컨테이너에 있는 모든 적 오브젝트에서 EnemyEntityData를 수집합니다.
        // LevelEditorEnemy 컴포넌트를 찾아 해당 데이터를 EnemyEntityData 배열로 반환합니다.
        public EnemyEntityData[] CollectEnemiesFromRoom()
        {
            // 컨테이너 내의 LevelEditorEnemy 컴포넌트들을 모두 가져옵니다.
            LevelEditorEnemy[] editorData = container.GetComponentsInChildren<LevelEditorEnemy>();
            // EnemyEntityData 배열을 생성합니다.
            EnemyEntityData[] result = new EnemyEntityData[editorData.Length];

            // 각 LevelEditorEnemy에서 필요한 데이터를 추출하여 EnemyEntityData를 생성합니다.
            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new EnemyEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale, editorData[i].isElite,editorData[i].GetPathPoints());
            }

            // 수집된 EnemyEntityData 배열을 반환합니다.
            return result;
        }

        // 현재 방 컨테이너에서 출구 지점 오브젝트의 위치를 수집합니다.
        // LevelEditorExitPoint 컴포넌트를 찾아 위치를 out 매개변수로 반환하고, 출구 지점 존재 여부를 bool 값으로 반환합니다.
        public bool CollectExitPointFromRoom(out Vector3 position)
        {
            // 컨테이너 내의 LevelEditorExitPoint 컴포넌트를 가져옵니다.
            LevelEditorExitPoint editorData = container.GetComponentInChildren<LevelEditorExitPoint>();

            // 출구 지점이 없는 경우
            if(editorData == null)
            {
                position = Vector3.zero;
                // 출구 지점이 없음을 반환합니다.
                return false;
            }
            // 출구 지점이 있는 경우
            else
            {
                // 출구 지점의 로컬 위치를 반환합니다.
                position = editorData.transform.localPosition;
                // 출구 지점이 있음을 반환합니다.
                return true;
            }
        }

        // 현재 방 컨테이너에 있는 모든 상자 오브젝트에서 ChestEntityData를 수집합니다.
        // LevelEditorChest 컴포넌트를 찾아 해당 데이터를 ChestEntityData 배열로 반환합니다.
        public ChestEntityData[] CollectChestFromRoom()
        {
            // 컨테이너 내의 LevelEditorChest 컴포넌트들을 모두 가져옵니다.
            LevelEditorChest[] editorData = container.GetComponentsInChildren<LevelEditorChest>();
            // ChestEntityData 배열을 생성합니다.
            ChestEntityData[] result = new ChestEntityData[editorData.Length];

            // 각 LevelEditorChest에서 필요한 데이터를 추출하여 ChestEntityData를 생성합니다.
            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new ChestEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale, editorData[i].rewardCurrency, editorData[i].rewardValue, editorData[i].droppedCurrencyItemsAmount);
            }

            // 수집된 ChestEntityData 배열을 반환합니다.
            return result;
        }

        // 지정된 프리팹을 사용하여 방 커스텀 오브젝트를 씬에 스폰합니다.
        // position, rotation, scale 값을 설정하고 roomCustomObjectsContainer의 자식으로 추가합니다.
        public void SpawnRoomCustomObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // 프리팹을 인스턴스화합니다.
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            // 방 커스텀 오브젝트 컨테이너의 자식으로 설정합니다.
            gameObject.transform.SetParent(roomCustomObjectsContainer.transform);
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            gameObject.hideFlags = HideFlags.DontSave;

            // 위치, 회전, 스케일을 설정합니다.
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
        }

        // 지정된 프리팹을 사용하여 월드 커스텀 오브젝트를 씬에 스폰합니다.
        // position, rotation, scale 값을 설정하고 worldCustomObjectsContainer의 자식으로 추가합니다.
        public void SpawnWorldCustomObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // 프리팹을 인스턴스화합니다.
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            // 월드 커스텀 오브젝트 컨테이너의 자식으로 설정합니다.
            gameObject.transform.SetParent(worldCustomObjectsContainer.transform);
            // 씬 저장 시 제외되도록 hideFlags를 설정합니다.
            gameObject.hideFlags = HideFlags.DontSave;

            // 위치, 회전, 스케일을 설정합니다.
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
        }

        // roomCustomObjectsContainer에 있는 모든 커스텀 오브젝트의 데이터를 수집합니다.
        // PrefabUtility를 사용하여 프리팹 원본과 트랜스폼 데이터를 CustomObjectData 목록으로 반환합니다.
        public List<CustomObjectData> CollectRoomCustomObjects()
        {
            // CustomObjectData 목록을 생성합니다.
            List<CustomObjectData> result = new List<CustomObjectData>();
            Transform temp;

            // roomCustomObjectsContainer의 모든 자식을 순회합니다.
            for (int i = 0; i < roomCustomObjectsContainer.transform.childCount; i++)
            {
                temp = roomCustomObjectsContainer.transform.GetChild(i);

                // 오브젝트가 프리팹의 일부인 경우에만 데이터를 수집합니다.
                if (PrefabUtility.IsPartOfAnyPrefab(temp))
                {
                    // 프리팹 원본과 트랜스폼 데이터를 포함하는 CustomObjectData를 생성하여 목록에 추가합니다.
                    result.Add(new CustomObjectData(PrefabUtility.GetCorrespondingObjectFromSource(temp.gameObject), temp.localPosition, temp.localRotation, temp.localScale));
                }

            }

            // 수집된 CustomObjectData 목록을 반환합니다.
            return result;
        }

        // worldCustomObjectsContainer에 있는 모든 커스텀 오브젝트의 데이터를 수집합니다.
        // PrefabUtility를 사용하여 프리팹 원본과 트랜스폼 데이터를 CustomObjectData 목록으로 반환합니다.
        public List<CustomObjectData> CollectWorldCustomObjects()
        {
            // CustomObjectData 목록을 생성합니다.
            List<CustomObjectData> result = new List<CustomObjectData>();
            Transform temp;

            // worldCustomObjectsContainer의 모든 자식을 순회합니다.
            for (int i = 0; i < worldCustomObjectsContainer.transform.childCount; i++)
            {
                temp = worldCustomObjectsContainer.transform.GetChild(i);

                // 오브젝트가 프리팹의 일부인 경우에만 데이터를 수집합니다.
                if (PrefabUtility.IsPartOfAnyPrefab(temp))
                {
                    // 프리팹 원본과 트랜스폼 데이터를 포함하는 CustomObjectData를 생성하여 목록에 추가합니다.
                    result.Add(new CustomObjectData(PrefabUtility.GetCorrespondingObjectFromSource(temp.gameObject), temp.localPosition, temp.localRotation, temp.localScale));
                }
            }

            // 수집된 CustomObjectData 목록을 반환합니다.
            return result;
        }

        // 현재 방 컨테이너의 모든 자식 오브젝트를 삭제하고 컨테이너의 트랜스폼을 초기화합니다.
        public void Clear()
        {
            // 컨테이너가 null이면 함수를 종료합니다.
            if(container == null)
            {
                return;
            }

            // 컨테이너의 모든 자식 오브젝트를 역순으로 순회하며 즉시 삭제합니다.
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }

            // 컨테이너의 글로벌 트랜스폼을 초기화합니다. (위치: 0,0,0, 회전: 0,0,0, 스케일: 1,1,1)
            container.transform.ResetGlobal();
        }

        // roomCustomObjectsContainer의 모든 자식 오브젝트를 삭제하고 컨테이너의 트랜스폼을 초기화합니다.
        public void ClearRoomCustomObjectsContainer()
        {
            // roomCustomObjectsContainer의 모든 자식 오브젝트를 역순으로 순회하며 즉시 삭제합니다.
            for (int i = roomCustomObjectsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roomCustomObjectsContainer.transform.GetChild(i).gameObject);
            }

            // roomCustomObjectsContainer의 글로벌 트랜스폼을 초기화합니다.
            roomCustomObjectsContainer.transform.ResetGlobal();
        }

        // worldCustomObjectsContainer의 모든 자식 오브젝트를 삭제하고 컨테이너의 트랜스폼을 초기화합니다.
        public void ClearWorldCustomObjectsContainer()
        {
            // worldCustomObjectsContainer의 모든 자식 오브젝트를 역순으로 순회하며 즉시 삭제합니다.
            for (int i = worldCustomObjectsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(worldCustomObjectsContainer.transform.GetChild(i).gameObject);
            }

            // worldCustomObjectsContainer의 글로벌 트랜스폼을 초기화합니다.
            worldCustomObjectsContainer.transform.ResetGlobal();
        }

        // Unity 에디터 씬 뷰에 기즈모를 그립니다.
        // showGizmo가 true이고 컨테이너가 존재하면 스폰 지점을 나타내는 와이어 구체를 그립니다.
        public void OnDrawGizmos()
        {
            // 기즈모를 표시하도록 설정된 경우에만 그립니다.
            if(showGizmo)
            {
                // 현재 기즈모 색상을 백업합니다.
                backupColor = Gizmos.color;

                // 스폰 지점 색상으로 설정하고 스폰 지점에 와이어 구체를 그립니다.
                Gizmos.color = spawnPointColor;
                Gizmos.DrawWireSphere(container.transform.position + spawnPoint, spawnPointSphereSize);

                // 백업된 색상으로 복원합니다.
                Gizmos.color = backupColor;
            }

        }

        // 현재 방의 상태 (아이템, 상자, 적, 출구 지점, 커스텀 오브젝트)를 기록합니다.
        // 이는 방의 변경 사항을 감지하는 데 사용됩니다.
        public void RegisterRoomState()
        {
            // 방 변경 플래그를 초기화합니다.
            roomChanged = false;

            // 현재 방의 아이템, 상자, 적, 출구 지점, 커스텀 오브젝트 데이터를 수집하여 저장합니다.
            roomItems =  CollectItemsFromRoom();
            roomChests = CollectChestFromRoom();
            roomEnemies = CollectEnemiesFromRoom();
            roomExitPointVector = Vector3.zero;
            roomExitPoint = CollectExitPointFromRoom(out roomExitPointVector);
            roomCustomObjects = CollectRoomCustomObjects();
            worldCustomObjects = CollectWorldCustomObjects();
        }

        // 현재 방의 상태가 기록된 상태와 비교하여 변경되었는지 여부를 확인합니다.
        // 아이템, 상자, 적, 출구 지점, 커스텀 오브젝트의 변경 사항을 감지합니다.
        public bool IsRoomChanged()
        {
            // 이미 변경된 것으로 표시되어 있으면 즉시 반환합니다.
            if (roomChanged)
            {
                return roomChanged;
            }

            // 컨테이너가 null이면 변경되지 않은 것으로 간주합니다.
            if(container == null)
            {
                return false;
            }

            // 임시 출구 지점 벡터를 초기화합니다.
            tempExitPoint = Vector3.zero;

            // 출구 지점의 존재 여부가 변경되었는지 확인합니다.
            if(roomExitPoint != CollectExitPointFromRoom(out tempExitPoint))
            {
                roomChanged = true;
                return roomChanged;
            }

            // 출구 지점의 위치가 변경되었는지 확인합니다.
            if (roomExitPointVector != tempExitPoint)
            {
                roomChanged = true;
                return roomChanged;
            }

            // 아이템 목록이 변경되었는지 확인합니다.
            if (!roomItems.SequenceEqual(CollectItemsFromRoom()))
            {
                roomChanged = true;
                return roomChanged;
            }

            // 상자 목록이 변경되었는지 확인합니다.
            if (!roomChests.SequenceEqual(CollectChestFromRoom()))
            {
                roomChanged = true;
                return roomChanged;
            }

            // 적 목록이 변경되었는지 확인합니다.
            if (!roomEnemies.SequenceEqual(CollectEnemiesFromRoom()))
            {
                roomChanged = true;
                return roomChanged;
            }

            if (!roomCustomObjects.SequenceEqual(CollectRoomCustomObjects()))
            {
                roomChanged = true;
                return roomChanged;
            }

            if (!worldCustomObjects.SequenceEqual(CollectWorldCustomObjects()))
            {
                roomChanged = true;
                return roomChanged;
            }

            return roomChanged;
        }
#endif
    }
}