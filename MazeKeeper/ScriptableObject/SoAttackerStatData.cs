using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoAttackerStatData", menuName = "SoData/SoAttackerStatData")]
    public class SoAttackerStatData : UnityEngine.ScriptableObject
    {
        public float AttackDPS;
        public float AttackInterval;
        public float AttackRange;
    }
}