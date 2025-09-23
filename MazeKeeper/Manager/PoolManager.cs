using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Component;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class PoolManager : ManagerBase<PoolManager>
    {
        /// <summary>
        /// 사용가능한 Pool Stack
        /// </summary>
        readonly Dictionary<string, Stack<CompPoolItem>> poolDic = new();
        /// <summary>
        /// 현재 씬에 Spawn된 CompPoolItem들의 List
        /// </summary>
        readonly Dictionary<string, List<CompPoolItem>> spawnedDic = new();


        /// <summary>
        /// Pool에서 오브젝트 가져오기
        /// </summary>
        public CompPoolItem Spawn(CompPoolItem compPoolItemFromAsset, Vector3 position, Quaternion rotation, bool activate, Transform parent = null)
        {
            var poolId = compPoolItemFromAsset.PoolId;

            // poolDic에 존재하지 않기때문에 Pool을 새로 생성한다.
            if (!poolDic.ContainsKey(poolId))
            {
                poolDic.Add(poolId, new());
                spawnedDic.Add(poolId, new());
                AddNewPoolItem(compPoolItemFromAsset.InitialPoolItemCount);
            }

            // 부족하면 1개 추가
            if (poolDic[poolId].Count == 0)
            {
                AddNewPoolItem(1);
            }

            var resultCompPoolItem = poolDic[poolId].Pop();
            resultCompPoolItem.transform.SetPositionAndRotation(position, rotation);

            if (activate)
            {
                resultCompPoolItem.gameObject.SetActive(true);
            }

            spawnedDic[poolId].Add(resultCompPoolItem);

            return resultCompPoolItem;

            // compPoolItemFromAsset 과 같은 인자를 바로 사용해야하고, Spawn함수 내부에서만 사용하므로, Local함수로 정의
            void AddNewPoolItem(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    GameObject newPoolObject;
                    if (parent != null)
                    {
                        newPoolObject = Instantiate(compPoolItemFromAsset.gameObject, parent);
                    }
                    else
                    {
                        newPoolObject = Instantiate(compPoolItemFromAsset.gameObject);
                    }
                    newPoolObject.SetActive(false);

                    // 새로 생성한 오브젝트의 이름이 뒤에 (Clone)이 붙기 때문에, 원래 이름으로 덮어쓴다.
                    newPoolObject.gameObject.name = compPoolItemFromAsset.gameObject.name;
                    var createdCompPoolItem = newPoolObject.GetComponent<CompPoolItem>();

                    poolDic[poolId].Push(createdCompPoolItem);
                }
            }
        }


        /// <summary>
        /// Pool에 오브젝트 반환
        /// </summary>
        public void Despawn(CompPoolItem spawnedCompPoolItem)
        {
            var poolId = spawnedCompPoolItem.PoolId;
            // spawnedDic의 List에서 삭제
            var result = spawnedDic[poolId].Remove(spawnedCompPoolItem);
            Debug.Assert(result, spawnedCompPoolItem);

            // poolDic의 Stack에 Push
            poolDic[poolId].Push(spawnedCompPoolItem);

            spawnedCompPoolItem.gameObject.SetActive(false);
        }


        public void DespawnAll()
        {
            foreach (var poolList in spawnedDic.Values)
            {
                foreach (var poolItem in poolList)
                {
                    Despawn(poolItem);
                }
            }
        }


        /// <summary>
        /// Pool 안의 오브젝트 완전 삭제
        /// </summary>
        public void DeletePool(string poolId)
        {
            for (int i = spawnedDic[poolId].Count - 1; i >= 0; i--)
            {
                var poolItem = spawnedDic[poolId][i];
                Despawn(poolItem);
            }
            foreach (var poolItem in poolDic[poolId])
            {
                Destroy(poolItem.gameObject);
            }
            poolDic.Remove(poolId);
            spawnedDic.Remove(poolId);
        }


        /// <summary>
        /// 모든 Pool 삭제
        /// </summary>
        public void DeletePoolAll()
        {
            Debug.Log($"Previous poolDic.Count {poolDic.Count}");
            Debug.Log($"Previous spawnedDic.Count {spawnedDic.Count}");
            var keyList = poolDic.Keys.ToList();

            foreach (var key in keyList)
            {
                Debug.Log($"Previous poolDic.Count {key}");
                DeletePool(key);
            }
            Debug.Log($"Current poolDic.Count {poolDic.Count}");
            Debug.Log($"Current spawnedDic.Count {spawnedDic.Count}");
        }
    }
}
