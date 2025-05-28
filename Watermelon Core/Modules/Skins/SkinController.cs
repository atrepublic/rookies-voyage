// SkinController.cs
// 이 스크립트는 게임 내 여러 스킨 데이터베이스를 관리하고,
// 스킨 선택, 잠금 해제, 저장 및 불러오기 기능을 제공하는 중앙 컨트롤러입니다.
// 싱글톤 패턴으로 구현되어 어디서든 접근이 가능합니다.

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SkinController : MonoBehaviour, ISkinsProvider
    {
        /// <summary>
        /// SkinController의 전역 접근 인스턴스입니다.
        /// </summary>
        public static SkinController Instance { get; private set; }

        [UnpackNested]
        [SerializeField, Tooltip("게임 내 모든 스킨 데이터베이스를 참조하는 핸들러")]
        private SkinsHandler handler;

        public SkinsHandler Handler => handler;

        /// <summary>
        /// 선택된 스킨 정보를 저장하는 객체입니다.
        /// </summary>
        private SkinControllerSave save;

        /// <summary>
        /// 각 스킨 데이터베이스별로 현재 선택된 스킨을 저장하는 딕셔너리입니다.
        /// </summary>
        private Dictionary<AbstractSkinDatabase, ISkinData> selectedSkins;

        /// <summary>
        /// 스킨 잠금 해제 시 호출되는 이벤트입니다.
        /// </summary>
        public static event SkinCallback SkinUnlocked;

        /// <summary>
        /// 스킨 선택 시 호출되는 이벤트입니다.
        /// </summary>
        public static event SkinCallback SkinSelected;

        public delegate void SkinCallback(ISkinData skinData);

        // SkinController 초기화 함수
        public void Init()
        {
            Instance = this;
            save = SaveController.GetSaveObject<SkinControllerSave>("Skin Controller Save");
            selectedSkins = new Dictionary<AbstractSkinDatabase, ISkinData>();

            for (int i = 0; i < handler.ProvidersCount; i++)
            {
                AbstractSkinDatabase provider = handler.GetSkinsProvider(i);
                InitProvider(provider);
            }

            UpdateSave();
        }

        // 각 스킨 데이터베이스 초기화 및 저장된 스킨 복원 또는 기본 스킨 설정
        private void InitProvider(AbstractSkinDatabase provider)
        {
            provider.Init();

            for (int i = 0; i < provider.SkinsCount; i++)
            {
                ISkinData skinData = provider.GetSkinData(i);

                for (int j = 0; j < save.SelectedSkinsCount; j++)
                {
                    int selectedSkinHash = save.GetSelectedSkin(j);
                    if (skinData.Hash == selectedSkinHash)
                    {
                        selectedSkins.Add(provider, skinData);
                        return;
                    }
                }
            }

            UnlockAndSelectDefaultSkin(provider);
        }

        public ISkinData GetSelectedSkin<T>() where T : AbstractSkinDatabase
        {
            AbstractSkinDatabase provider = GetProvider<T>();
            return GetSelectedSkin(provider);
        }

        public List<ISkinData> GetUnlockedSkins<T>() where T : AbstractSkinDatabase
        {
            AbstractSkinDatabase provider = GetProvider<T>();

            List<ISkinData> unlockedSkins = new List<ISkinData>();
            if (provider != null)
            {
                for (int i = 0; i < provider.SkinsCount; i++)
                {
                    ISkinData skin = provider.GetSkinData(i);
                    if (skin.IsUnlocked)
                        unlockedSkins.Add(skin);
                }
            }

            return unlockedSkins;
        }

        public ISkinData GetRandomSkin<T>() where T : AbstractSkinDatabase
        {
            AbstractSkinDatabase provider = GetProvider<T>();

            if (provider != null)
                return provider.GetSkinData(Random.Range(0, provider.SkinsCount));

            return null;
        }

        private ISkinData GetSelectedSkin(AbstractSkinDatabase provider)
        {
            if (provider == null || !selectedSkins.ContainsKey(provider)) return null;
            return selectedSkins[provider];
        }

        public bool IsSkinSelected(string skinId)
        {
            ISkinData skinData = GetSkinData(skinId);
            if (skinData == null) return false;
            return IsSkinSelected(skinData);
        }

        public bool IsSkinSelected(ISkinData skinData)
        {
            foreach (ISkinData selectedSkin in selectedSkins.Values)
            {
                if (skinData == selectedSkin) return true;
            }
            return false;
        }

        private ISkinData UnlockAndSelect<T>() where T : AbstractSkinDatabase
        {
            AbstractSkinDatabase provider = GetProvider<T>();
            if (provider == null) return null;
            return UnlockAndSelectDefaultSkin(provider);
        }

        // 기본 스킨 잠금 해제 및 선택
        private ISkinData UnlockAndSelectDefaultSkin(AbstractSkinDatabase provider)
        {
            if (provider.SkinsCount == 0) return null;

            ISkinData defaultSkin = provider.GetSkinData(0);
            defaultSkin.Unlock();

            if (selectedSkins.ContainsKey(provider))
                selectedSkins[provider] = defaultSkin;
            else
                selectedSkins.Add(provider, defaultSkin);

            return defaultSkin;
        }

        public void SelectSkin(string skinId)
        {
            ISkinData skinData = GetSkinData(skinId);
            if (skinData != null)
                SelectSkin(skinData);
        }

        public void SelectSkin(ISkinData data)
        {
            AbstractSkinDatabase provider = data.SkinsProvider;

            if (selectedSkins.ContainsKey(provider))
                selectedSkins[provider] = data;
            else
                selectedSkins.Add(provider, data);

            UpdateSave();
            SkinSelected?.Invoke(data);
        }

        public void UnlockSkin(string skinId, bool select = false)
        {
            ISkinData skinData = GetSkinData(skinId);
            if (skinData != null)
                UnlockSkin(skinData, select);
        }

        public void UnlockSkin(ISkinData skinData, bool select = false)
        {
            skinData.Unlock();
            SkinUnlocked?.Invoke(skinData);

            if (select)
                SelectSkin(skinData);
        }

        public ISkinData GetSkinData(string skinId)
        {
            int hash = skinId.GetHashCode();

            for (int i = 0; i < handler.ProvidersCount; i++)
            {
                AbstractSkinDatabase provider = handler.GetSkinsProvider(i);
                for (int j = 0; j < provider.SkinsCount; j++)
                {
                    ISkinData skinData = provider.GetSkinData(j);
                    if (skinData.Hash == hash)
                        return skinData;
                }
            }

            Debug.LogError($"[Skin Controller] 해당 ID('{skinId}')에 해당하는 스킨을 찾을 수 없습니다.");
            return null;
        }

        public bool IsSkinUnlocked(string skinId)
        {
            int hash = skinId.GetHashCode();

            for (int i = 0; i < handler.ProvidersCount; i++)
            {
                AbstractSkinDatabase provider = handler.GetSkinsProvider(i);
                for (int j = 0; j < provider.SkinsCount; j++)
                {
                    ISkinData skin = provider.GetSkinData(j);
                    if (skin.Hash == hash)
                        return skin.IsUnlocked;
                }
            }

            return false;
        }

        private AbstractSkinDatabase GetProvider<T>() where T : AbstractSkinDatabase
        {
            for (int i = 0; i < handler.ProvidersCount; i++)
            {
                AbstractSkinDatabase provider = handler.GetSkinsProvider(i);
                if (provider is T)
                    return provider;
            }
            return null;
        }

        // 선택된 스킨 정보를 save 객체에 저장
        private void UpdateSave()
        {
            int[] selectedHashes = new int[selectedSkins.Count];
            int i = 0;
            foreach (ISkinData data in selectedSkins.Values)
            {
                selectedHashes[i++] = data.Hash;
            }
            save.Update(selectedHashes);
        }
    }

    /// <summary>
    /// SkinController의 선택된 스킨 정보를 저장하기 위한 데이터 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class SkinControllerSave : ISaveObject
    {
        [SerializeField, Tooltip("선택된 스킨의 해시 배열")] 
        private int[] selectedSkins;

        public int SelectedSkinsCount => selectedSkins != null ? selectedSkins.Length : 0;

        public int GetSelectedSkin(int index)
        {
            return selectedSkins[index];
        }

        public void Update(int[] newSelectedSkins)
        {
            selectedSkins = newSelectedSkins;
        }

        public void Flush()
        {
            // 필요시 즉시 저장 로직을 추가할 수 있습니다.
        }
    }
}
