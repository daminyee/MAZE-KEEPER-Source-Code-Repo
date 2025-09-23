using System;
using MazeKeeper.Component;
using UnityEngine.Serialization;


namespace MazeKeeper.Class
{
    [Serializable]
    public class EnemySpawnInfo
    {
        [FormerlySerializedAs("enemyTemplate")]
        public CompEnemy EnemyTemplate;

        public float SpawnStartDelay;
        public float SpawnInterval;
        public int   SpawnCount;
    }
}