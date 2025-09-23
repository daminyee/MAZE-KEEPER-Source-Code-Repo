using System;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;


namespace MazeKeeper.Class
{
    [Serializable]
    public class CharacterData
    {
        public CharacterType CharacterType;

        public bool Unlocked;

        public List<int> CharacterStatLevelList  = Enumerable.Repeat(0, (int)CharacterUpgradableStatType.Length).ToList();
        public List<int> CharacterSkillLevelList = Enumerable.Repeat(0, (int)CharacterSkillType.Length).ToList();
    }
}