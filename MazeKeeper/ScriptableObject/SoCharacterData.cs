using System;
using System.Collections.Generic;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoCharacterData", menuName = "SoData/SoCharacterData")]
    public class SoCharacterData : UnityEngine.ScriptableObject
    {
        public const int MaxStatLevel  = 2; // 0부터 시작
        public const int MaxSkillLevel = 2; // 0부터 시작

        public CharacterType CharacterType;
        public string        CharacterName;
        public string        CharacterDescription;

        [FormerlySerializedAs("UnlockPrice")]
        public int CharacterUnlockPrice;

        [FormerlySerializedAs("UniqueScoCharacterSkillList")]
        public List<SoCharacterSkillData_Buff> BuffScoCharacterSkillList;
        public List<SoCharacterSkillData_Attack> Attack1ScoCharacterSkillList;
        public List<SoCharacterSkillData_Attack> Attack2ScoCharacterSkillList;

        public List<SoCharacterUpgradableStatData> AttackDamageStatList;
        public List<SoCharacterUpgradableStatData> MoveSpeedStatList;

        public SoAttackerStatData AttackerStat;


        public SoCharacterSkillData GetCurrentScoCharacterSkillData(CharacterSkillType characterSkillType)
        {
            var playerData = PlayerDataManager.Instance.PlayerData;
            switch (characterSkillType)
            {
                case CharacterSkillType.BuffSkill:
                    return BuffScoCharacterSkillList[playerData.CurrentCharacterData.CharacterSkillLevelList[(int)characterSkillType]];
                case CharacterSkillType.AttackSkill1:
                    return Attack1ScoCharacterSkillList[playerData.CurrentCharacterData.CharacterSkillLevelList[(int)characterSkillType]];
                case CharacterSkillType.AttackSkill2:
                    return Attack2ScoCharacterSkillList[playerData.CurrentCharacterData.CharacterSkillLevelList[(int)characterSkillType]];
                default:
                    throw new ArgumentOutOfRangeException(nameof(characterSkillType), characterSkillType, null);
            }
        }


        public SoCharacterSkillData GetScoCharacterSkillData(CharacterSkillType characterSkillType, int level)
        {
            switch (characterSkillType)
            {
                case CharacterSkillType.BuffSkill:
                    return BuffScoCharacterSkillList[level];
                case CharacterSkillType.AttackSkill1:
                    return Attack1ScoCharacterSkillList[level];
                case CharacterSkillType.AttackSkill2:
                    return Attack2ScoCharacterSkillList[level];
                default:
                    throw new ArgumentOutOfRangeException(nameof(characterSkillType), characterSkillType, null);
            }
        }


        public SoCharacterUpgradableStatData GetCurrentScoCharacterStatData(CharacterUpgradableStatType characterUpgradableStatType)
        {
            var playerData = PlayerDataManager.Instance.PlayerData;
            switch (characterUpgradableStatType)
            {
                case CharacterUpgradableStatType.AttackPower:
                    return AttackDamageStatList[playerData.CurrentCharacterData.CharacterStatLevelList[(int)characterUpgradableStatType]];
                case CharacterUpgradableStatType.MoveSpeed:
                    return MoveSpeedStatList[playerData.CurrentCharacterData.CharacterStatLevelList[(int)characterUpgradableStatType]];
                default:
                    throw new ArgumentOutOfRangeException(nameof(characterUpgradableStatType), characterUpgradableStatType, null);
            }
        }
    }
}