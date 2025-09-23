using System;
using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoCellObjectDataHolder", menuName = "SoData/SoCellObjectDataHolder")]
    public class SoCellObjectDataHolder : UnityEngine.ScriptableObject
    {
        public List<SoCellObjectData> SoCellObjectData_TurretBasicList;
        public List<SoCellObjectData> SoCellObjectData_TurretLaserList;
        public List<SoCellObjectData> SoCellObjectData_TurretGatlingList;
        public List<SoCellObjectData> SoCellObjectData_TurretThrowList;
        public List<SoCellObjectData> SoCellObjectData_TurretMissileList;
        public List<SoCellObjectData> SoCellObjectData_TrapSpikeList;
        public List<SoCellObjectData> SoCellObjectData_TrapFireList;
        public List<SoCellObjectData> SoCellObjectData_ObstacleList;


        public SoCellObjectData GetCurrentSoCellObjectData(CellObjectType cellObjectType)
        {
            var playerData = PlayerDataManager.Instance.PlayerData;
            switch (cellObjectType)
            {
                case CellObjectType.TurretBasic:
                    return SoCellObjectData_TurretBasicList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.TurretLaser:
                    return SoCellObjectData_TurretLaserList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.TurretGatling:
                    return SoCellObjectData_TurretGatlingList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.TurretMissile:
                    return SoCellObjectData_TurretMissileList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.TurretThrow:
                    return SoCellObjectData_TurretThrowList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.TrapSpike:
                    return SoCellObjectData_TrapSpikeList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.TrapFire:
                    return SoCellObjectData_TrapFireList[playerData.CellObjectLevelList[(int)cellObjectType]];
                case CellObjectType.Obstacle:
                    return SoCellObjectData_ObstacleList[playerData.CellObjectLevelList[(int)cellObjectType]];
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellObjectType), cellObjectType, null);
            }
        }


        public SoCellObjectData GetSoCellObjectData(CellObjectType cellObjectType, int level)
        {
            switch (cellObjectType)
            {
                case CellObjectType.TurretBasic:
                    return SoCellObjectData_TurretBasicList[level];
                case CellObjectType.TurretLaser:
                    return SoCellObjectData_TurretLaserList[level];
                case CellObjectType.TurretGatling:
                    return SoCellObjectData_TurretGatlingList[level];
                case CellObjectType.TurretMissile:
                    return SoCellObjectData_TurretMissileList[level];
                case CellObjectType.TurretThrow:
                    return SoCellObjectData_TurretThrowList[level];
                case CellObjectType.TrapSpike:
                    return SoCellObjectData_TrapSpikeList[level];
                case CellObjectType.TrapFire:
                    return SoCellObjectData_TrapFireList[level];
                case CellObjectType.Obstacle:
                    return SoCellObjectData_ObstacleList[level];
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellObjectType), cellObjectType, null);
            }
        }
    }
}