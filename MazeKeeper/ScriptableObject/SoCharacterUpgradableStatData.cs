using MazeKeeper.Define;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "ScoCharacterUpgradableStatData", menuName = "SoData/SoCharacterUpgradableStatData")]
    public class SoCharacterUpgradableStatData : UnityEngine.ScriptableObject
    {
        public CharacterUpgradableStatType UpgradableStatType;

        public float StatValue;

        public int Price;
    }
}