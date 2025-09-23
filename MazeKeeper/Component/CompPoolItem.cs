using System.Collections;
using MazeKeeper.Class;
using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompPoolItem : MonoBehaviour
    {
        public string PoolId               => gameObject.name;
        public int    InitialPoolItemCount => _initialPoolItemCount;


        /// <summary>
        /// autoDestroyOption을 사용해서 일정 시간이 지나면 자동으로 despawn
        /// </summary>
        [SerializeField] Option<float> _autoDestroyOption;
        /// <summary>
        /// Pool이 최초 생성될때 몇개를 기본으로 생성할 것인지
        /// </summary>
        [SerializeField] int _initialPoolItemCount = 5;


        void OnEnable()
        {
            if (_autoDestroyOption.Enabled)
            {
                StartCoroutine(CR_Despawn());
            }
        }


        IEnumerator CR_Despawn()
        {
            yield return new WaitForSeconds(_autoDestroyOption.Value);

            var poolManager = PoolManager.Instance;
            poolManager.Despawn(this);
        }
    }
}