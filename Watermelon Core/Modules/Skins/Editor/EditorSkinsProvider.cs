// EditorSkinsProvider.cs
// Unity 에디터 초기화 시 프로젝트에 존재하는 모든 AbstractSkinDatabase 에셋을 자동으로 탐색하여 등록합니다.
// 커스텀 에디터 및 스킨 피커 창 등에서 스킨 데이터베이스에 접근할 수 있도록 합니다.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class EditorSkinsProvider
    {
        /// <summary>
        /// 프로젝트 내 자동 등록된 모든 스킨 데이터베이스
        /// </summary>
        private static List<AbstractSkinDatabase> skinsDatabases;
        public static List<AbstractSkinDatabase> SkinsDatabases => skinsDatabases;

        /// <summary>
        /// AbstractSkinDatabase를 상속받은 모든 타입 정보
        /// </summary>
        private static IEnumerable<Type> registeredTypes;

        // 클래스가 처음 로드될 때 자동 실행되는 정적 생성자입니다.
        static EditorSkinsProvider()
        {
            skinsDatabases = new List<AbstractSkinDatabase>();

            // 모든 AbstractSkinDatabase 하위 타입을 검색
            registeredTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(AbstractSkinDatabase)));

            // 각 타입에 해당하는 에셋을 찾아 등록
            foreach (Type type in registeredTypes)
            {
                Object database = EditorUtils.GetAsset(type);
                if (database != null)
                {
                    skinsDatabases.Add((AbstractSkinDatabase)database);
                }
            }
        }

        /// <summary>
        /// 외부에서 수동으로 스킨 데이터베이스를 등록할 수 있습니다.
        /// </summary>
        public static void AddDatabase(AbstractSkinDatabase database)
        {
            if (HasSkinsProvider(database)) return;
            skinsDatabases.Add(database);
        }

        /// <summary>
        /// 특정 타입에 해당하는 스킨 데이터베이스를 가져옵니다.
        /// </summary>
        public static AbstractSkinDatabase GetSkinsProvider(Type providerType)
        {
            if (!skinsDatabases.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase database in skinsDatabases)
                {
                    if (database.GetType() == providerType)
                        return database;
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 스킨 데이터베이스가 등록되어 있는지 확인합니다.
        /// </summary>
        public static bool HasSkinsProvider(AbstractSkinDatabase provider)
        {
            if (!skinsDatabases.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase database in skinsDatabases)
                {
                    if (database == provider)
                        return true;
                }
            }
            return false;
        }
    }
}
