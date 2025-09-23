using System;
using System.Collections.Generic;


namespace MazeKeeper.Class
{
    [Serializable]
    public class EnemyWaveData
    {
        public List<EnemySpawnInfo> EnemySpawnInfoList = new List<EnemySpawnInfo>();
    }
}