using MazeKeeper.Class;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoEnemyStatusEffect", menuName = "SoData/SoEnemyStatusEffect")]
    public class SoEnemyStatusEffect : UnityEngine.ScriptableObject
    {
        public EnemyStatusEffectItem EnemyStatusEffect;
        public float                 ApplyProbability = 1f;
    }
}