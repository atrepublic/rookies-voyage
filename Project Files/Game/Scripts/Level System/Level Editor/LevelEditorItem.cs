// LevelEditorItem.cs
// 레벨 에디터에서 오브젝트를 배치하고, X/Z 축으로 미러링할 수 있도록 지원하는 스크립트입니다.
// 주로 아이템, 오브젝트 등을 배치할 때 좌우/앞뒤 반전을 쉽게 하기 위해 사용됩니다.

#pragma warning disable 649
using UnityEngine;

namespace Watermelon.LevelSystem
{
    public class LevelEditorItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [Tooltip("오브젝트를 식별하기 위한 고유 해시값입니다.")]
        [HideInInspector] 
        public int hash;

        /// <summary>
        /// X축을 기준으로 오브젝트를 미러링(좌우 반전)합니다.
        /// </summary>
        [Button]
        public void MirrorX()
        {
            GameObject spawnedObject = Instantiate(gameObject, transform.parent);
            spawnedObject.transform.localPosition = new Vector3(
                -transform.localPosition.x,
                 transform.localPosition.y,
                 transform.localPosition.z
            );
        }

        /// <summary>
        /// Z축을 기준으로 오브젝트를 미러링(앞뒤 반전)합니다.
        /// </summary>
        [Button]
        public void MirrorZ()
        {
            GameObject spawnedObject = Instantiate(gameObject, transform.parent);
            spawnedObject.transform.localPosition = new Vector3(
                 transform.localPosition.x,
                 transform.localPosition.y,
                -transform.localPosition.z
            );
        }
    }
}
