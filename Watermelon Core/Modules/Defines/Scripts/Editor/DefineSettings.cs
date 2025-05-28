// 스크립트 기능 요약:
// 이 스크립트는 프로젝트의 스크립팅 정의 심볼(Scripting Define Symbols)과 관련된 설정 및 유틸리티 함수를 제공합니다.
// 미리 정의된 정적 정의 심볼 목록과, 코드의 DefineAttribute를 통해 동적으로 등록되는 정의 심볼 목록을 관리합니다.
// 현재 빌드 타겟에 설정된 정의 심볼들을 가져오고, 프로젝트의 코드에서 DefineAttribute가 적용된 타입을 찾아 동적 정의 심볼 목록을 구성하는 기능을 포함합니다.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor; // PlayerSettings 및 BuildPipeline 접근을 위해 추가

namespace Watermelon
{
    // DefineSettings 클래스는 정적 유틸리티 클래스로 정의 심볼 관련 설정 및 기능을 제공합니다.
    public static class DefineSettings
    {
        // STATIC_DEFINES: 프로젝트에서 항상 사용되거나 특정 조건에서 활성화되는 미리 정의된 정적 정의 심볼 목록입니다.
        [Tooltip("프로젝트에서 사용되는 미리 정의된 정적 정의 심볼 목록")]
        public static readonly string[] STATIC_DEFINES = new string[]
        {
            "UNITY_POST_PROCESSING_STACK_V2",
            "PHOTON_UNITY_NETWORKING",
            "PUN_2_0_OR_NEWER",
            "PUN_2_OR_NEWER",
        };

        // STATIC_REGISTERED_DEFINES: 코드를 통해 DefineAttribute로 등록되는 정의 심볼 중
        // 항상 존재한다고 간주되는 정적 등록 정의 심볼 목록입니다.
        // 주로 핵심 모듈이나 필수적인 기능과 관련된 정의 심볼이 포함됩니다.
        [Tooltip("코드를 통해 DefineAttribute로 등록되는 정적 등록 정의 심볼 목록")]
        public static readonly RegisteredDefine[] STATIC_REGISTERED_DEFINES = new RegisteredDefine[]
        {
            // 시스템 관련 모듈 정의 심볼
            new RegisteredDefine("MODULE_INPUT_SYSTEM", "UnityEngine.InputSystem.InputManager"),
            new RegisteredDefine("MODULE_TMP", "TMPro.TMP_Text"),
            new RegisteredDefine("MODULE_CINEMACHINE", "Cinemachine.CinemachineBrain"),
            new RegisteredDefine("MODULE_IDFA", "Unity.Advertisement.IosSupport.ATTrackingStatusBinding"),

            // 코어 시스템 관련 모듈 정의 심볼
            new RegisteredDefine("MODULE_MONETIZATION", "Watermelon.Monetization"),
            new RegisteredDefine("MODULE_IAP", "UnityEngine.Purchasing.UnityPurchasing"),
            new RegisteredDefine("MODULE_POWERUPS", "Watermelon.PUController"),
            new RegisteredDefine("MODULE_HAPTIC", "Watermelon.Haptic"),
            new RegisteredDefine("MODULE_CURVE", "Watermelon.CurvatureManager"),

            new RegisteredDefine("TEST", "NewBehaviourScript"), // 테스트용 정의 심볼
        };

        /// <summary>
        /// 프로젝트 내의 코드를 검사하여 DefineAttribute가 적용된 타입으로부터 동적으로 등록되는 정의 심볼 목록을 가져옵니다.
        /// STATIC_REGISTERED_DEFINES에 정의된 심볼과 코드에서 찾은 심볼을 합쳐 최종 목록을 반환합니다.
        /// </summary>
        /// <returns>동적으로 등록된 정의 심볼 목록을 포함하는 RegisteredDefine 리스트</returns>
        public static List<RegisteredDefine> GetDynamicDefines()
        {
            // DefineAttribute가 적용된 타입을 찾기 위해 현재 AppDomain의 모든 어셈블리를 가져옵니다.
            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    try
                    {
                        // 어셈블리에서 모든 타입을 가져옵니다.
                        Type[] tempTypes = assembly.GetTypes();

                        // DefineAttribute가 정의된 타입만 필터링합니다.
                        tempTypes = tempTypes.Where(m => m.IsDefined(typeof(DefineAttribute), true)).ToArray();

                        // 필터링된 타입이 있으면 gameTypes 목록에 추가합니다.
                        if (!tempTypes.IsNullOrEmpty())
                            gameTypes.AddRange(tempTypes);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        // 리플렉션 타입 로딩 중 예외가 발생하면 로그를 출력합니다.
                        Debug.LogException(e);
                    }
                }
            }

            // 동적으로 등록된 정의 심볼 목록을 저장할 리스트를 초기화하고 정적 등록 정의 심볼을 추가합니다.
            List<RegisteredDefine> registeredDefines = new List<RegisteredDefine>();
            registeredDefines.AddRange(STATIC_REGISTERED_DEFINES);

            // DefineAttribute가 적용된 각 타입을 순회하며 동적 정의 심볼을 추출합니다.
            foreach (Type type in gameTypes)
            {
                // 타입에 적용된 DefineAttribute를 가져옵니다.
                DefineAttribute[] defineAttributes = (DefineAttribute[])Attribute.GetCustomAttributes(type, typeof(DefineAttribute));

                // 각 DefineAttribute를 순회합니다.
                for (int i = 0; i < defineAttributes.Length; i++)
                {
                    // AssemblyType이 비어있지 않은 경우 동적 정의 심볼로 처리합니다.
                    if (!string.IsNullOrEmpty(defineAttributes[i].AssemblyType))
                    {
                        // 이미 목록에 추가된 정의 심볼인지 확인합니다.
                        int methodId = registeredDefines.FindIndex(x => x.Define == defineAttributes[i].Define);
                        // 목록에 없으면 새로 추가합니다.
                        if (methodId == -1)
                        {
                            registeredDefines.Add(new RegisteredDefine(defineAttributes[i]));
                        }
                    }
                }
            }

            // 최종 동적 등록 정의 심볼 목록을 반환합니다.
            return registeredDefines;
        }
    }
}