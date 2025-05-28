// 스크립트 기능 요약:
// 이 스크립트는 GameObject 및 IPool 인터페이스에 대한 확장 메서드를 제공하여
// 오브젝트 풀링 관련 작업을 더 편리하게 수행할 수 있도록 돕습니다.
// GameObject로부터 풀을 가져오거나, IPool 객체를 파괴하거나, 풀링된 GameObject의 위치, 회전, 스케일, 부모 등을 설정하는 확장 함수들을 포함합니다.

using UnityEngine;

namespace Watermelon
{
    // PoolExtensions 클래스는 확장 메서드를 포함하는 정적 클래스입니다.
    public static class PoolExtensions
    {
        /// <summary>
        /// 지정된 GameObject에 해당하는 풀을 가져옵니다.
        /// PoolManager에 해당 이름의 풀이 이미 존재하면 그 풀을 반환하고, 없으면 새로운 Pool 객체를 생성하여 반환합니다.
        /// 이 메서드는 PoolManager에 풀을 자동으로 등록하지는 않습니다.
        /// </summary>
        /// <param name="gameObject">풀을 가져올 기준이 되는 GameObject (주로 프리팹)</param>
        /// <returns>해당 GameObject와 연결된 IPool 객체</returns>
        public static IPool GetPool(this GameObject gameObject)
        {
            // PoolManager에 해당 GameObject의 이름으로 등록된 풀이 있는지 확인합니다.
            if (PoolManager.HasPool(gameObject.name))
                return PoolManager.GetPoolByName(gameObject.name); // 있으면 해당 풀 반환

            // 등록된 풀이 없으면 새로운 Pool 객체를 생성하여 반환합니다.
            return new Pool(gameObject);
        }

        /// <summary>
        /// 지정된 IPool 객체를 PoolManager에서 파괴합니다.
        /// 풀에 의해 관리되는 모든 오브젝트도 함께 파괴됩니다.
        /// </summary>
        /// <param name="pool">파괴할 IPool 객체</param>
        public static void Destroy(this IPool pool)
        {
            // 풀 객체가 null이 아닌 경우에만 PoolManager의 DestroyPool 함수를 호출하여 풀을 파괴합니다.
            if (pool != null)
                PoolManager.DestroyPool(pool);
        }

        /// <summary>
        /// GameObject의 위치(position)를 설정하는 확장 메서드입니다.
        /// GameObject.transform.position을 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">위치를 설정할 GameObject</param>
        /// <param name="position">설정할 새로운 위치</param>
        /// <returns>위치가 설정된 GameObject</returns>
        public static GameObject SetPosition(this GameObject gameObject, Vector3 position)
        {
            gameObject.transform.position = position; // 위치 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 위치(position)와 회전(rotation)을 동시에 설정하는 확장 메서드입니다.
        /// GameObject.transform.SetPositionAndRotation을 사용하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">위치와 회전을 설정할 GameObject</param>
        /// <param name="position">설정할 새로운 위치</param>
        /// <param name="rotation">설정할 새로운 회전 (Quaternion)</param>
        /// <returns>위치와 회전이 설정된 GameObject</returns>
        public static GameObject SetPositionAndRotation(this GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            gameObject.transform.SetPositionAndRotation(position, rotation); // 위치 및 회전 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 로컬 위치(localPosition)를 설정하는 확장 메서드입니다.
        /// GameObject.transform.localPosition을 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">로컬 위치를 설정할 GameObject</param>
        /// <param name="localPosition">설정할 새로운 로컬 위치</param>
        /// <returns>로컬 위치가 설정된 GameObject</returns>
        public static GameObject SetLocalPosition(this GameObject gameObject, Vector3 localPosition)
        {
            gameObject.transform.localPosition = localPosition; // 로컬 위치 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 스케일(localScale)을 설정하는 확장 메서드입니다.
        /// GameObject.transform.localScale을 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">스케일을 설정할 GameObject</param>
        /// <param name="scale">설정할 새로운 스케일</param>
        /// <returns>스케일이 설정된 GameObject</returns>
        public static GameObject SetScale(this GameObject gameObject, Vector3 scale)
        {
            gameObject.transform.localScale = scale; // 스케일 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 오일러 각(eulerAngles) 회전을 설정하는 확장 메서드입니다.
        /// GameObject.transform.eulerAngles를 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">오일러 각 회전을 설정할 GameObject</param>
        /// <param name="eulerAngles">설정할 새로운 오일러 각 (Vector3)</param>
        /// <returns>오일러 각 회전이 설정된 GameObject</returns>
        public static GameObject SetEulerAngles(this GameObject gameObject, Vector3 eulerAngles)
        {
            gameObject.transform.eulerAngles = eulerAngles; // 오일러 각 회전 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 로컬 오일러 각(localEulerAngles) 회전을 설정하는 확장 메서드입니다.
        /// GameObject.transform.localEulerAngles를 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">로컬 오일러 각 회전을 설정할 GameObject</param>
        /// <param name="localEulerAngles">설정할 새로운 로컬 오일러 각 (Vector3)</param>
        /// <returns>로컬 오일러 각 회전이 설정된 GameObject</returns>
        public static GameObject SetLocalEulerAngles(this GameObject gameObject, Vector3 localEulerAngles)
        {
            gameObject.transform.localEulerAngles = localEulerAngles; // 로컬 오일러 각 회전 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 회전(rotation)을 설정하는 확장 메서드입니다.
        /// GameObject.transform.rotation을 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">회전을 설정할 GameObject</param>
        /// <param name="rotation">설정할 새로운 회전 (Quaternion)</param>
        /// <returns>회전이 설정된 GameObject</returns>
        public static GameObject SetRotation(this GameObject gameObject, Quaternion rotation)
        {
            gameObject.transform.rotation = rotation; // 회전 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 로컬 회전(localRotation)을 설정하는 확장 메서드입니다.
        /// GameObject.transform.localRotation을 설정하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">로컬 회전을 설정할 GameObject</param>
        /// <param name="localRotation">설정할 새로운 로컬 회전 (Quaternion)</param>
        /// <returns>로컬 회전이 설정된 GameObject</returns>
        public static GameObject SetLocalRotation(this GameObject gameObject, Quaternion localRotation)
        {
            gameObject.transform.localRotation = localRotation; // 로컬 회전 설정
            return gameObject; // GameObject 반환
        }

        /// <summary>
        /// GameObject의 부모(parent)를 설정하는 확장 메서드입니다.
        /// GameObject.transform.SetParent를 사용하고 GameObject 자체를 반환하여 체이닝을 가능하게 합니다.
        /// </summary>
        /// <param name="gameObject">부모를 설정할 GameObject</param>
        /// <param name="parent">설정할 새로운 부모 Transform</param>
        /// <returns>부모가 설정된 GameObject</returns>
        public static GameObject SetParent(this GameObject gameObject, Transform parent)
        {
            gameObject.transform.SetParent(parent); // 부모 설정
            return gameObject; // GameObject 반환
        }
    }
}