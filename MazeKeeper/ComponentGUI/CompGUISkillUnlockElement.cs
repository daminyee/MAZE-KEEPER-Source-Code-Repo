using System;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUISkillUnlockElement : MonoBehaviour
    {
        public event Action<CompGUISkillUnlockElement> OnSelectButtonClicked;

        // 스킬 아이콘
        [SerializeField] GameObject _lockIcon;
        [SerializeField] GameObject _selectBorder;
        [SerializeField] Image      _skillIcon;
        [SerializeField] int        _level;

        // 공용 Panel
        [SerializeField] TMP_Text _skillName;
        [SerializeField] TMP_Text _skillDescription;
        [SerializeField] TMP_Text _skillUnlockPrice;


        [SerializeField] GameObject _unlockPanel;
        [SerializeField] GameObject _skillDescriptionPanel;

        [SerializeField] Button m_SelectButton;
        [SerializeField] Button m_UnlockButton;


        SoCharacterData      m_SoCharacterData;
        SoCharacterSkillData m_SoCharacterSkillData;
        bool                 m_IsSelected;

        /// <summary>
        /// 스킬을 사용가능한 상태
        /// </summary>
        bool m_IsOpen;

        /// <summary>
        /// Unlock을 위한 Panel을 보여줄지
        /// </summary>
        bool m_IsShowUnlockPanel;


        void Start()
        {
            m_SelectButton.onClick.AddListener(() => Select(true));
            m_UnlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }


        public void Init(SoCharacterData characterData, CharacterSkillType characterSkillType)
        {
            m_SoCharacterData      = characterData;
            m_SoCharacterSkillData = characterData.GetScoCharacterSkillData(characterSkillType, _level);
            _skillIcon.sprite      = m_SoCharacterSkillData.SkillIcon;
        }


        public void Select(bool isSelected)
        {
            m_IsSelected = isSelected;
            _skillDescriptionPanel.SetActive(true);
            _selectBorder.SetActive(true);
            _selectBorder.transform.position = transform.position;

            if (isSelected)
            {
                _skillName.text        = m_SoCharacterSkillData.Name;
                _skillDescription.text = m_SoCharacterSkillData.Description;
                _skillUnlockPrice.text = m_SoCharacterSkillData.Price.ToString();
                if (m_IsShowUnlockPanel)
                {
                    // Unlock 패널 On,
                    _unlockPanel.SetActive(true);
                    OnSelectButtonClicked?.Invoke(this);
                }
                else
                {
                    _unlockPanel.SetActive(false);
                }
            }
        }


        public void OnUnlockButtonClicked()
        {
            if (m_IsSelected == false || m_IsShowUnlockPanel == false || m_IsOpen) return;

            if (!PlayerDataManager.Instance.TryConsumeGem(m_SoCharacterSkillData.Price)) return;
            PlayerDataManager.Instance.LevelUpSkillLevel(m_SoCharacterData.CharacterType, m_SoCharacterSkillData);
            _unlockPanel.SetActive(false);
        }


        public void Refresh(CompGUISkillUnlockElement selectedCompGUISkillUnlockElement)
        {
            m_IsSelected = selectedCompGUISkillUnlockElement == this;

            var skillLevel = PlayerDataManager.Instance.PlayerData.CharacterDataList[(int)m_SoCharacterData.CharacterType].CharacterSkillLevelList[(int)m_SoCharacterSkillData.CharacterSkillType];
            switch (skillLevel)
            {
                case 0:
                    if (_level == 0) SetUnlockPanel(false);
                    if (_level == 1) SetUnlockPanel(true);
                    if (_level == 2) SetUnlockPanel(false);

                    if (_level == 0) SetOpen(true);
                    if (_level == 1) SetOpen(false);
                    if (_level == 2) SetOpen(false);
                    break;
                case 1:
                    if (_level == 0) SetUnlockPanel(false);
                    if (_level == 1) SetUnlockPanel(false);
                    if (_level == 2) SetUnlockPanel(true);

                    if (_level == 0) SetOpen(true);
                    if (_level == 1) SetOpen(true);
                    if (_level == 2) SetOpen(false);
                    break;
                case 2:
                    if (_level == 0) SetUnlockPanel(false);
                    if (_level == 1) SetUnlockPanel(false);
                    if (_level == 2) SetUnlockPanel(false);

                    if (_level == 0) SetOpen(true);
                    if (_level == 1) SetOpen(true);
                    if (_level == 2) SetOpen(true);
                    break;
            }
        }


        void SetUnlockPanel(bool show)
        {
            m_IsShowUnlockPanel = show;
        }


        void SetOpen(bool open)
        {
            _lockIcon.SetActive(!open);
            m_IsOpen = open;
        }
    }
}