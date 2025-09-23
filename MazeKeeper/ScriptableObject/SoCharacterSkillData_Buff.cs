using MazeKeeper.Class;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "ScoCharacterSkillData_Unique", menuName = "SoData/SoCharacterSkillData_Unique")]
    public class SoCharacterSkillData_Buff : SoCharacterSkillData
    {
        public BuffStatItem AttackSpeedBuffItem;
        public BuffStatItem AttackMultiplierBuffItem;
    }
}