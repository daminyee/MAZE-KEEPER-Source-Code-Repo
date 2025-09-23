using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoStageData", menuName = "SoData/SoStageData")]
    public class SoStageData : UnityEngine.ScriptableObject
    {
        public int StageIndex;

        public List<SoGateData> SoGateDataList;
    }
}