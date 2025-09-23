using System;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class PlayerDataManager : ManagerBase<PlayerDataManager>
    {
        const string PlayerDataKey = "PlayerData";

        public PlayerData PlayerData => m_PlayerData;

        public event Action OnSkillLevelChanged;
        public event Action OnCellObjectLevelChanged;
        public event Action OnGoldChanged;
        public event Action OnGemChanged;

        PlayerData m_PlayerData;


        void Start()
        {
            Load();
        }


        /// <summary>
        /// json파일에 저장된 데이터 불러오기
        /// </summary>
        public void Load()
        {
            string json = PlayerPrefs.GetString(PlayerDataKey, null);
            if (null != json)
            {
                Debug.Log(json);
                PlayerData loadedPlayer = JsonUtility.FromJson<PlayerData>(json);
                if (loadedPlayer == null)
                {
                    Debug.LogError("오류발생으로 초기화");
                    loadedPlayer = new();
                }
                m_PlayerData = loadedPlayer;
                PrintPlayerData(m_PlayerData);

                Save();
            }
            else
            {
                m_PlayerData = new();
            }
        }


        /// <summary>
        /// json파일에 데이터 저장하기
        /// </summary>
        public void Save()
        {
            string json = JsonUtility.ToJson(m_PlayerData, true);

            PlayerPrefs.SetString(PlayerDataKey, json);
            PlayerPrefs.Save();

            Debug.Log("저장완료");
            Debug.Log(json);
        }


        /// <summary>
        /// PlayerData를 초기값으로 초기화
        /// </summary>
        public void Reset()
        {
            PlayerData.Gold = 1500;

            PlayerData.CellObjectLevelList.Clear();
            PlayerData.CellObjectLevelList = Enumerable.Repeat(0, (int)CellObjectType.Length).ToList();
            PlayerData.CellObjectUnlockCheckList.Clear();
            PlayerData.CellObjectUnlockCheckList = Enumerable.Repeat(false, (int)CellObjectType.Length).ToList();

            PlayerData.CellObjectUnlockCheckList[(int)CellObjectType.TurretBasic] = true;
            PlayerData.CellObjectUnlockCheckList[(int)CellObjectType.Obstacle]    = true;

            Save();
        }


        public void PrintPlayerData(PlayerData playerData)
        {
            Debug.Log("Gold: " + playerData.Gold);
            Debug.Log("Gem: " + playerData.Gem);
            Debug.Log("CurrentStageIndex: " + playerData.CurrentStageIndex);

            Debug.Log("TurretBasic: " + playerData.CellObjectLevelList[(int)CellObjectType.TurretBasic]);
            Debug.Log("TurretLaser: " + playerData.CellObjectLevelList[(int)CellObjectType.TurretLaser]);
            Debug.Log("TurretGatling: " + playerData.CellObjectLevelList[(int)CellObjectType.TurretGatling]);
            Debug.Log("TurretMissile: " + playerData.CellObjectLevelList[(int)CellObjectType.TurretMissile]);
            Debug.Log("TurretThrow: " + playerData.CellObjectLevelList[(int)CellObjectType.TurretThrow]);
            Debug.Log("TrapSpike: " + playerData.CellObjectLevelList[(int)CellObjectType.TrapSpike]);
            Debug.Log("TrapFire: " + playerData.CellObjectLevelList[(int)CellObjectType.TrapFire]);
            Debug.Log("Obstacle: " + playerData.CellObjectLevelList[(int)CellObjectType.Obstacle]);

            // Debug.Log("AttackPower: " + playerData.CharacterSharedStatLevelList[(int)CharacterSharedStatType.AttackPower]);
            // Debug.Log("MoveSpeed: " + playerData.CharacterSharedStatLevelList[(int)CharacterSharedStatType.MoveSpeed]);
            // Debug.Log("MaxHealth: " + playerData.CharacterSharedStatLevelList[(int)CharacterSharedStatType.MaxHealth]);

            Debug.Log("UniqueSkillOfCharacterA: " + playerData.CharacterDataList[(int)CharacterType.Character0].CharacterSkillLevelList[(int)CharacterSkillType.BuffSkill]);
            Debug.Log("AttackSkill1OfCharacterA: " + playerData.CharacterDataList[(int)CharacterType.Character0].CharacterSkillLevelList[(int)CharacterSkillType.AttackSkill1]);
            Debug.Log("AttackSkill2OfCharacterA: " + playerData.CharacterDataList[(int)CharacterType.Character0].CharacterSkillLevelList[(int)CharacterSkillType.AttackSkill2]);
            Debug.Log("UniqueSkillOfCharacterB: " + playerData.CharacterDataList[(int)CharacterType.Character1].CharacterSkillLevelList[(int)CharacterSkillType.BuffSkill]);
            Debug.Log("AttackSkill1OfCharacterB: " + playerData.CharacterDataList[(int)CharacterType.Character1].CharacterSkillLevelList[(int)CharacterSkillType.AttackSkill1]);
            Debug.Log("AttackSkill2OfCharacterB: " + playerData.CharacterDataList[(int)CharacterType.Character1].CharacterSkillLevelList[(int)CharacterSkillType.AttackSkill2]);
            Debug.Log("UniqueSkillOfCharacterC: " + playerData.CharacterDataList[(int)CharacterType.Character2].CharacterSkillLevelList[(int)CharacterSkillType.BuffSkill]);
            Debug.Log("AttackSkill1OfCharacterC: " + playerData.CharacterDataList[(int)CharacterType.Character2].CharacterSkillLevelList[(int)CharacterSkillType.AttackSkill1]);
            Debug.Log("AttackSkill2OfCharacterC: " + playerData.CharacterDataList[(int)CharacterType.Character2].CharacterSkillLevelList[(int)CharacterSkillType.AttackSkill2]);
        }


        /// <summary>
        /// 골드 획득
        /// </summary>
        public void AddGold(int gold)
        {
            m_PlayerData.Gold += gold;
            OnGoldChanged?.Invoke();
        }


        /// <summary>
        /// 골드 소비
        /// </summary>
        public bool TryConsumeGold(int gold)
        {
            if (m_PlayerData.Gold < gold)
            {
                ToastManager.Instance.ToastWarning("골드가 부족합니다.");
                return false;
            }
            m_PlayerData.Gold -= gold;
            OnGoldChanged?.Invoke();
            return true;
        }


        /// <summary>
        /// 보석 획득
        /// </summary>
        public void AddGem(int gem)
        {
            m_PlayerData.Gem += gem;
            OnGemChanged?.Invoke();
        }


        /// <summary>
        /// 보석 소비
        /// </summary>
        public bool TryConsumeGem(int gem)
        {
            if (m_PlayerData.Gem < gem)
            {
                ToastManager.Instance.ToastWarning("보석이 부족합니다.");
                return false;
            }
            m_PlayerData.Gem -= gem;
            OnGemChanged?.Invoke();
            return true;
        }


        /// <summary>
        /// 캐릭터 스킬 레벨업
        /// </summary>
        public void LevelUpSkillLevel(CharacterType characterType, SoCharacterSkillData skillData)
        {
            m_PlayerData.CharacterDataList[(int)characterType].CharacterSkillLevelList[(int)skillData.CharacterSkillType]++;
            ToastManager.Instance.ToastMessage("Skill Unlocked!");
            OnSkillLevelChanged?.Invoke();
        }


        /// <summary>
        /// 캐릭터 스탯 레벨업
        /// </summary>
        public void LevelUpStatLevel(CharacterType characterType, CharacterUpgradableStatType upgradableStatType)
        {
            m_PlayerData.CharacterDataList[(int)characterType].CharacterStatLevelList[(int)upgradableStatType]++;
            ToastManager.Instance.ToastMessage("Stat Level Up!");
        }


        /// <summary>
        /// 배치물 레벨업
        /// </summary>
        public void LevelUpCellObjectLevel(SoCellObjectData cellObjectData)
        {
            m_PlayerData.CellObjectLevelList[(int)cellObjectData.CellObjectType]++;
            ToastManager.Instance.ToastMessage($"{cellObjectData.ObjectName} Unlocked!");
            OnCellObjectLevelChanged?.Invoke();
        }


        /// <summary>
        /// 배치물 잠금해제
        /// </summary>
        public void UnlockCellObject(SoCellObjectData cellObjectData)
        {
            PlayerData.CellObjectUnlockCheckList[(int)cellObjectData.CellObjectType] = true;
            ToastManager.Instance.ToastMessage($"{cellObjectData.ObjectName} Unlocked!");
            OnCellObjectLevelChanged?.Invoke();
        }


        /// <summary>
        /// 스테이지 증가
        /// </summary>
        public void SetNextStage()
        {
            m_PlayerData.CurrentStageIndex++;
        }
    }
}
