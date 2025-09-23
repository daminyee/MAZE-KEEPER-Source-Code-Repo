using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "ScoGateData", menuName = "SoData/SoGateData")]
    public class SoGateData : UnityEngine.ScriptableObject
    {
        public int           GateIndex;
        public float         GateStartDelay;
        public GateType      GateType;
        public EnemyWaveData EnemyWaveData;
    }
}