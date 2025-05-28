// SkinsHandler.cs
// 이 클래스는 게임 내 존재하는 여러 스킨 데이터베이스(AbstractSkinDatabase)를 관리하는 핸들러입니다.
// 데이터베이스를 타입 또는 인덱스로 조회하거나, 포함 여부를 확인할 수 있습니다.

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SkinsHandler
    {
        [SerializeField, Tooltip("게임 내 등록된 스킨 데이터베이스 목록")]
        private AbstractSkinDatabase[] skinProviders;

        /// <summary>
        /// 등록된 모든 스킨 데이터베이스 배열을 반환합니다.
        /// </summary>
        public AbstractSkinDatabase[] SkinsProviders => skinProviders;

        /// <summary>
        /// 등록된 데이터베이스의 수를 반환합니다.
        /// </summary>
        public int ProvidersCount => skinProviders.Length;

        /// <summary>
        /// 인덱스를 통해 특정 스킨 데이터베이스를 가져옵니다.
        /// </summary>
        public AbstractSkinDatabase GetSkinsProvider(int index)
        {
            return skinProviders[index];
        }

        /// <summary>
        /// 타입을 기준으로 스킨 데이터베이스를 찾아 반환합니다.
        /// 해당 타입이 존재하지 않으면 null 반환.
        /// </summary>
        public AbstractSkinDatabase GetSkinsProvider(System.Type providerType)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase skinProvider in skinProviders)
                {
                    if (skinProvider.GetType() == providerType)
                        return skinProvider;
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 타입의 스킨 데이터베이스가 존재하는지 여부를 반환합니다.
        /// </summary>
        public bool HasSkinsProvider(System.Type providerType)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase skinProvider in skinProviders)
                {
                    if (skinProvider.GetType() == providerType)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 특정 인스턴스의 데이터베이스가 포함되어 있는지 확인합니다.
        /// </summary>
        public bool HasSkinsProvider(AbstractSkinDatabase provider)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase skinProvider in skinProviders)
                {
                    if (skinProvider == provider)
                        return true;
                }
            }
            return false;
        }
    }
}
