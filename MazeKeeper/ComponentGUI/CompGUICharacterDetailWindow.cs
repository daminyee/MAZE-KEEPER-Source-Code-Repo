using System;
using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUICharacterDetailWindow : MonoBehaviour
    {
        public event Action OnClose;

        [SerializeField] TMP_Text _characterName;
        [SerializeField] TMP_Text _characterDescription;

        [SerializeField] Button _closeButton;
        [SerializeField] Button _closeButtonBackground;

        [SerializeField] Button     _characterUnlockButton;
        [SerializeField] TMP_Text   _characterUnlockPrice;
        [SerializeField] GameObject _unlockGuideText;

        [SerializeField] GameObject _startButton;

        [SerializeField] List<CompGUISkillUnlockElement> _compGUISkillUnlockElementList;
        [SerializeField] GameObject                      _skillDescriptionPanel;
        [SerializeField] GameObject                      _skillUnlockPanel;
        [SerializeField] GameObject                      _statUpgradePanel;

        [SerializeField] List<CompGUIStatUpgrade> _statUpgradeList;


        [SerializeField] GameObject _selectBorder;

        SoCharacterData           m_SoCharacterData;
        CompGUISkillUnlockElement _selectedCompGUISkillUnlockElement;


        void Start()
        {
            _closeButton.onClick.AddListener(CloseCharacterDetailWindow);
            _closeButtonBackground.onClick.AddListener(CloseCharacterDetailWindow);
            _characterUnlockButton.onClick.AddListener(UnlockCharacter);
            PlayerDataManager.Instance.OnSkillLevelChanged += RefreshSkillUnlockElement;
        }


        void OnDestroy()
        {
            PlayerDataManager.Instance.OnSkillLevelChanged -= RefreshSkillUnlockElement;
        }


        public void Init(SoCharacterData characterData)
        {
            m_SoCharacterData          = characterData;
            _characterName.text        = characterData.CharacterName;
            _characterDescription.text = characterData.CharacterDescription;
            _characterUnlockPrice.text = characterData.CharacterUnlockPrice.ToString();

            for (var i = 0; i < _statUpgradeList.Count; i++)
            {
                _statUpgradeList[i].Init(m_SoCharacterData);
            }

            for (var i = 0; i < (int)CharacterSkillType.Length; i++)
            {
                for (var j = 0; j <= SoCharacterData.MaxSkillLevel; j++)
                {
                    var skill = _compGUISkillUnlockElementList[i * (SoCharacterData.MaxSkillLevel + 1) + j];
                    skill.Init(m_SoCharacterData, (CharacterSkillType)i);
                    skill.OnSelectButtonClicked += OnSkillElementSelected;
                }
            }

            OpenCharacterDetailWindow();
            RefreshSkillUnlockElement();

            var isUnlocked = PlayerDataManager.Instance.PlayerData.CharacterDataList[(int)characterData.CharacterType].Unlocked;

            _characterUnlockButton.gameObject.SetActive(!isUnlocked);
            _unlockGuideText.SetActive(!isUnlocked);
            _skillUnlockPanel.SetActive(isUnlocked);
            _statUpgradePanel.SetActive(isUnlocked);
        }


        public void OpenCharacterDetailWindow()
        {
            gameObject.SetActive(true);
            foreach (var stat in _statUpgradeList)
            {
                stat.RefreshUI();
            }
        }


        public void CloseCharacterDetailWindow()
        {
            OnClose?.Invoke();
            gameObject.SetActive(false);
            _skillDescriptionPanel.SetActive(false);
            _selectBorder.SetActive(false);
        }


        void UnlockCharacter()
        {
            var playerDataManager = PlayerDataManager.Instance;

            if (!playerDataManager.TryConsumeGem(playerDataManager.PlayerData.CurrentSoCharacterData.CharacterUnlockPrice)) return;

            ToastManager.Instance.ToastMessage("캐릭터의 잠금이 해제되었습니다.");
            playerDataManager.PlayerData.CurrentCharacterData.Unlocked = true;
            _characterUnlockButton.gameObject.SetActive(false);
            _startButton.SetActive(true);
            _unlockGuideText.SetActive(false);
            _skillUnlockPanel.SetActive(true);
            _statUpgradePanel.SetActive(true);
        }


        void OnSkillElementSelected(CompGUISkillUnlockElement selectedCompGUISkillUnlockElement)
        {
            _selectedCompGUISkillUnlockElement = selectedCompGUISkillUnlockElement;
            RefreshSkillUnlockElement();
        }


        void RefreshSkillUnlockElement()
        {
            foreach (var skillUnlockElement in _compGUISkillUnlockElementList)
            {
                skillUnlockElement.Refresh(_selectedCompGUISkillUnlockElement);
            }
        }
    }
}