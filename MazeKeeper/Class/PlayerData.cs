using System;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;


namespace MazeKeeper.Class
{
    [Serializable]
    public class PlayerData
    {
        [NonSerialized]
        public SoCharacterData CurrentSoCharacterData;
        public CharacterData CurrentCharacterData => CharacterDataList[(int)CurrentSoCharacterData.CharacterType];

        public int Gold;
        public int Gem;

        public int CurrentStageIndex;

        public List<int>  CellObjectLevelList       = Enumerable.Repeat(0, (int)CellObjectType.Length).ToList();
        public List<bool> CellObjectUnlockCheckList = Enumerable.Repeat(false, (int)CellObjectType.Length).ToList();

        public List<CharacterData> CharacterDataList = new()
        {
            new(),
            new(),
            new(),
        };


        public PlayerData()
        {
            Gold = 1500;
            Gem  = 0;

            CharacterDataList[(int)CharacterType.Character0].Unlocked = true;
            for (int i = 0; i < (int)CharacterType.Length; i++)
            {
                CharacterDataList[i].CharacterType = (CharacterType)i;
            }

            CellObjectUnlockCheckList[(int)CellObjectType.TurretBasic] = true;
            CellObjectUnlockCheckList[(int)CellObjectType.Obstacle]    = true;
        }
    }
}