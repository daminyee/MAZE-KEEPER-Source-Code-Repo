using System;
using MazeKeeper.Define;


namespace MazeKeeper.Class
{
    [Serializable]
    public class EnemyStatusEffectItem
    {
        public EnemyStatusEffectType EnemyStatusEffectType;

        public float RemainingTime;


        public EnemyStatusEffectItem Clone()
        {
            EnemyStatusEffectItem newEnemyStatusEffectItem = new();

            newEnemyStatusEffectItem.EnemyStatusEffectType = EnemyStatusEffectType;
            newEnemyStatusEffectItem.RemainingTime         = RemainingTime;

            return newEnemyStatusEffectItem;
        }
    }
}

