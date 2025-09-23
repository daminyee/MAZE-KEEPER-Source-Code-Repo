using System;
using MazeKeeper.Define;
using UnityEngine.Serialization;


namespace MazeKeeper.Class
{
    [Serializable]
    public class StatItem
    {
        public AttackerStatType StatType;
        public float            Value;
    }
}
