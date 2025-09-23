using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.ScriptableObject
{
    public abstract class SoCellObjectData : UnityEngine.ScriptableObject
    {
        public const int MaxUpgradeLevel = 2;

        /// <summary>
        /// 길을 막는 속성
        /// </summary>
        public bool BlockPath = true;
        /// <summary>
        /// 바닥에 배치 가능
        /// </summary>
        public bool CanPlaceFloor = true;
        /// <summary>
        /// 벽 위에 배치 가능
        /// </summary>
        public bool CanPlaceWall = true;

        public int Level;

        public CellObjectType CellObjectType;

        public Sprite ObjectSprite;
        public string ObjectName;
        public string ObjectDescription;

        public int PlacePrice;

        public int UnlockPrice;

        public CompCellObject CompCellObject;

        public int PlaceLimit;
    }
}