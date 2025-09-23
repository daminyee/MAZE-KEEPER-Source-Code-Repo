using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoCellObjectData_Attacker", menuName = "SoData/SoCellObjectData_Attacker")]
    public class SoCellObjectData_Attacker : SoCellObjectData
    {
        public SoAttackerStatData  SoAttackerStatData;
        public SoEnemyStatusEffect SoEnemyStatusEffect;
    }
}