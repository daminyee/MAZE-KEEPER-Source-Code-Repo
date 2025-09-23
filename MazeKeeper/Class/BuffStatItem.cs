using System;
using UnityEngine;


namespace MazeKeeper.Class
{
    [Serializable]
    public class BuffStatItem
    {
        /// <summary>
        /// 버프 지속 시간
        /// </summary>
        public float RemainingTime;

        public StatItem StatItem;


        public BuffStatItem Clone()
        {
            BuffStatItem newBuffStatItem = new();
            newBuffStatItem.StatItem          = new();
            newBuffStatItem.RemainingTime     = RemainingTime;
            newBuffStatItem.StatItem.StatType = StatItem.StatType;
            newBuffStatItem.StatItem.Value    = StatItem.Value;
            return newBuffStatItem;
        }
    }
}