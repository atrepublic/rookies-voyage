// -----------------------------
// AnimatorParameters.cs
// -----------------------------
// Animator 파라미터들을 저장해두었다가
// 다른 Animator에 동일하게 적용할 수 있도록 복사 기능을 제공합니다.

using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class AnimatorParameters
    {
        private readonly Dictionary<string, float> floatParameters = new();
        private readonly Dictionary<string, int> intParameters = new();
        private readonly Dictionary<string, bool> boolParameters = new();
        private readonly List<string> triggerParameters = new();
        private readonly Dictionary<int, float> layerWeights = new();

        // 생성자: Animator에서 현재 설정된 모든 파라미터 값을 복사
        public AnimatorParameters(Animator animator)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        floatParameters[parameter.name] = animator.GetFloat(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        intParameters[parameter.name] = animator.GetInteger(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        boolParameters[parameter.name] = animator.GetBool(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (animator.GetBool(parameter.name))
                            triggerParameters.Add(parameter.name);
                        break;
                }
            }

            for (int i = 0; i < animator.layerCount; i++)
            {
                layerWeights[i] = animator.GetLayerWeight(i);
            }
        }

        // 복사된 파라미터들을 다른 Animator에 적용
        public void ApplyTo(Animator animator)
        {
            foreach (var parameter in floatParameters)
                animator.SetFloat(parameter.Key, parameter.Value);

            foreach (var parameter in intParameters)
                animator.SetInteger(parameter.Key, parameter.Value);

            foreach (var parameter in boolParameters)
                animator.SetBool(parameter.Key, parameter.Value);

            foreach (var parameter in triggerParameters)
                animator.SetTrigger(parameter);

            foreach (var layerWeight in layerWeights)
                animator.SetLayerWeight(layerWeight.Key, layerWeight.Value);
        }
    }
}
