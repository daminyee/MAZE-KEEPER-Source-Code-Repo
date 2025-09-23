using System.Collections.Generic;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIStatUpgrade : MonoBehaviour
    {
        [SerializeField] TMP_Text         _priceText;
        [SerializeField] List<GameObject> _upgradeGaugeList;
        [SerializeField] Button           _upgradeButton;

        [SerializeField] CharacterUpgradableStatType _characterUpgradableStatType;

        SoCharacterData m_SoCharacterData;

        int m_CurrentStatLevel;
        int m_CurrentGemPrice;


        void Start()
        {
            _upgradeButton.onClick.AddListener(Upgrade);
        }


        public void Init(SoCharacterData soCharacterData)
        {
            m_SoCharacterData = soCharacterData;

            RefreshUI();
        }


        public void RefreshUI()
        {
            m_CurrentStatLevel = PlayerDataManager.Instance.PlayerData.CharacterDataList[(int)m_SoCharacterData.CharacterType].CharacterStatLevelList[(int)_characterUpgradableStatType];
            Debug.Log(m_CurrentStatLevel);

            if (_characterUpgradableStatType == CharacterUpgradableStatType.AttackPower)
            {
                m_CurrentGemPrice = m_SoCharacterData.AttackDamageStatList[m_CurrentStatLevel].Price;
            }
            else if (_characterUpgradableStatType == CharacterUpgradableStatType.MoveSpeed)
            {
                m_CurrentGemPrice = m_SoCharacterData.MoveSpeedStatList[m_CurrentStatLevel].Price;
            }
            _priceText.text = m_CurrentGemPrice.ToString();

            // UI적인 디자인을 위해서 Slider를 사용하지 않고, gameObject를 사용
            switch (m_CurrentStatLevel)
            {
                case 0:
                    _upgradeGaugeList[0].SetActive(true);
                    _upgradeGaugeList[1].SetActive(false);
                    _upgradeGaugeList[2].SetActive(false);
                    _upgradeButton.gameObject.SetActive(true);
                    _priceText.gameObject.SetActive(true);
                    break;
                case 1:
                    _upgradeGaugeList[0].SetActive(true);
                    _upgradeGaugeList[1].SetActive(true);
                    _upgradeGaugeList[2].SetActive(false);
                    _upgradeButton.gameObject.SetActive(true);
                    _priceText.gameObject.SetActive(true);
                    break;
                case 2:
                    _upgradeGaugeList[0].SetActive(true);
                    _upgradeGaugeList[1].SetActive(true);
                    _upgradeGaugeList[2].SetActive(true);
                    _upgradeButton.gameObject.SetActive(false);
                    _priceText.gameObject.SetActive(false);
                    break;
            }
        }


        void Upgrade()
        {
            if (_characterUpgradableStatType == CharacterUpgradableStatType.AttackPower)
            {
                if (!PlayerDataManager.Instance.TryConsumeGem(m_CurrentGemPrice)) return;
            }
            else if (_characterUpgradableStatType == CharacterUpgradableStatType.MoveSpeed)
            {
                if (!PlayerDataManager.Instance.TryConsumeGem(m_CurrentGemPrice)) return;
            }

            // 캐릭터 Stat레벨에 반영
            PlayerDataManager.Instance.LevelUpStatLevel(m_SoCharacterData.CharacterType, _characterUpgradableStatType);

            RefreshUI();
        }
    }
}