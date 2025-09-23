using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class GameEnemyController : MonoBehaviour
    {
        public List<CompEnemy> EnemyList => m_EnemyList;

        [SerializeField] CompPoolItem      _compPoolItemGetGold;
        [SerializeField] CompPoolItem      _compPoolItemGetGem;
        [SerializeField] CompPoolItem      _compPoolItemDeadEffect;
        [SerializeField] List<SoAudioClip> _deadSoAudioClipList;

        readonly List<CompEnemy> m_EnemyList = new();


        /// <summary>
        /// Damage에 의해서 사망했을 경우 각종 처리
        /// </summary>
        public void DeadByDamage(CompEnemy enemy)
        {
            RemoveEnemy(enemy);

            if (enemy.DropGem > 0)
            {
                var gem = PoolManager.Instance.Spawn(_compPoolItemGetGem, enemy.transform.position, Quaternion.identity, true);
                gem.GetComponent<CompGetGem>().SetGem(enemy.DropGem);
            }
            if (enemy.DropGold > 0)
            {
                var gold = PoolManager.Instance.Spawn(_compPoolItemGetGold, enemy.transform.position, Quaternion.identity, true);
                gold.GetComponent<CompGetGold>().SetGold(enemy.DropGold);
            }

            PoolManager.Instance.Spawn(_compPoolItemDeadEffect, enemy.transform.position, Quaternion.identity, true);

            foreach (var audioClip in _deadSoAudioClipList)
            {
                AudioManager.Instance.Play(audioClip);
            }
        }


        /// <summary>
        /// Goal에 도달했을 때
        /// </summary>
        public void EnterGoal(CompEnemy enemy)
        {
            GameManager.Instance.GetDamage(enemy.AttackPower);

            RemoveEnemy(enemy);
        }


        /// <summary>
        /// 사거리 내의 피격 가능한 대상 찾기
        /// </summary>
        public CompEnemy FindEnemy(FindEnemyMethod findMethod, Vector3 position, float range, List<CompEnemy> exceptEnemyList = null)
        {
            switch (findMethod)
            {
                case FindEnemyMethod.Closest:
                {
                    return FindClosestEnemy(position, range, exceptEnemyList);
                }
                default:
                {
                    Debug.LogError($"{findMethod}는 유효하지 않은 타입입니다.");
                    return FindClosestEnemy(position, range, exceptEnemyList);
                }
            }
        }


        /// <summary>
        /// 가장 가까운 피격 대상 찾기
        /// </summary>
        public CompEnemy FindClosestEnemy(Vector3 position, float range, List<CompEnemy> exceptEnemyList = null)
        {
            CompEnemy result = null;
            position.y = 0;

            float min = float.MaxValue;

            //피격 대상들의 거리 비교
            foreach (var enemy in m_EnemyList)
            {
                var enemyPos = enemy.transform.position;
                enemyPos.y = 0;

                if (exceptEnemyList != null && exceptEnemyList.Contains(enemy)) continue;

                if (enemy.IsValid() == false) continue;

                var sqrMagnitude = Vector3.SqrMagnitude(enemyPos - position);
                if (sqrMagnitude > range * range) continue;
                if (sqrMagnitude < min)
                {
                    min    = sqrMagnitude;
                    result = enemy;
                }
            }

            return result;
        }


        public void AddEnemy(CompEnemy enemy)
        {
            m_EnemyList.Add(enemy);
        }


        void RemoveEnemy(CompEnemy enemy)
        {
            m_EnemyList.Remove(enemy);
            GameManager.Instance.CheckWinGame();
        }
    }
}
