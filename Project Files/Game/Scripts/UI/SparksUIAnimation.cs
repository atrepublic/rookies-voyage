
/*
📄 SparksUIAnimation.cs 요약
UI에서 무작위로 반짝이는 불꽃(Spark) 애니메이션을 연출하는 스크립트야.

🧩 주요 기능
sparkPrefab을 UI 위치에 랜덤하게 생성 및 애니메이션 시켜주는 역할을 해.

sparkPositions 배열에 정의된 RectTransform 위치들 중에서 비활성화된 위치를 골라 spark를 생성하고,
커졌다가 → 작아지며 → 사라지는 Tween 애니메이션을 실행해.

Pool 시스템을 사용해서 오브젝트 풀링 최적화가 적용돼 있어.

⚙️ 사용 용도
게임 내 UI에서 경험치 획득, 업그레이드 완료, 버튼 효과 강조 등 시각적 피드백으로 사용 가능.

spark가 자동으로 계속 발생하는 효과를 주며, StartAnimation()과 StopAnimation()으로 제어 가능해.

*/

using System.Collections;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class SparksUIAnimation : MonoBehaviour
    {
        [SerializeField] GameObject sparkPrefab;
        [SerializeField] RectTransform[] sparkPositions;

        private Pool sparkPool;
        private Coroutine sparksCoroutine;

        private void Start()
        {
            sparkPool = new Pool(sparkPrefab, "UI Spark");

            for (int i = 0; i < sparkPositions.Length; i++)
            {
                sparkPositions[i].gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if(sparkPool != null)
            {
                PoolManager.DestroyPool(sparkPool);
            }
        }

        public void StartAnimation()
        {
            if (sparkPositions.Length > 0)
                sparksCoroutine = StartCoroutine(SparkAnimation());
        }

        public void StopAnimation()
        {
            if (sparksCoroutine != null)
                StopCoroutine(sparksCoroutine);
        }

        private IEnumerator SparkAnimation()
        {
            WaitForSeconds waitForSeconds;

            RectTransform[] tempSparkObjects;

            while (true)
            {
                waitForSeconds = new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));

                tempSparkObjects = sparkPositions.Where(x => !x.gameObject.activeSelf).ToArray();
                if (!tempSparkObjects.IsNullOrEmpty())
                {
                    RectTransform parentSpark = tempSparkObjects.GetRandomItem();
                    parentSpark.gameObject.SetActive(true);

                    GameObject sparkObject = sparkPool.GetPooledObject();
                    sparkObject.gameObject.SetActive(true);
                    sparkObject.transform.SetParent(parentSpark);
                    sparkObject.transform.localPosition = Vector3.zero;
                    sparkObject.transform.localScale = Vector3.zero;
                    sparkObject.transform.localRotation = Quaternion.identity;

                    sparkObject.transform.DOScale(UnityEngine.Random.Range(0.4f, 1.2f), 0.5f).SetEasing(Ease.Type.CircOut).OnComplete(delegate
                    {
                        sparkObject.transform.DOScale(0, 0.4f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
                        {
                            sparkObject.SetActive(false);
                            sparkObject.transform.SetParent(null);

                            parentSpark.gameObject.SetActive(false);
                        });
                    });
                }

                yield return waitForSeconds;
            }
        }
    }
}