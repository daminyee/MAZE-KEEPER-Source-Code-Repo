using System;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUICellObjectElement : MonoBehaviour
    {
        public event Action<CompGUICellObjectElement> OnSelectButtonClicked;

        [SerializeField] GameObject _lockIcon;
        [SerializeField] Image      _cellObjectIcon;
        [SerializeField] GameObject _selectBorder;
        [SerializeField] int        _level;

        [SerializeField] TMP_Text _cellObjectName;
        [SerializeField] TMP_Text _cellObjectDescription;
        [SerializeField] TMP_Text _cellObjectUnlockPrice;

        [SerializeField] TMP_Text _attackDamageText;
        [SerializeField] TMP_Text _attackRangeText;

        [SerializeField] GameObject _unlockPanel;
        [SerializeField] GameObject _cellObjectDescriptionPanel;

        [SerializeField] Button m_SelectButton;
        [SerializeField] Button m_UnlockButton;


        SoCellObjectData m_SoCellObjectData;


        bool m_IsSelected;

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


        public void Init(SoCellObjectData cellObjectData)
        {
            m_SoCellObjectData     = cellObjectData;
            _cellObjectIcon.sprite = m_SoCellObjectData.ObjectSprite;
        }


        public void Select(bool isSelected)
        {
            m_IsSelected = isSelected;
            _cellObjectDescriptionPanel.SetActive(true);
            _selectBorder.SetActive(true);
            _selectBorder.transform.position = this.transform.position;

            if (isSelected)
            {
                _cellObjectName.text        = m_SoCellObjectData.ObjectName;
                _cellObjectDescription.text = m_SoCellObjectData.ObjectDescription;
                _cellObjectUnlockPrice.text = m_SoCellObjectData.UnlockPrice.ToString();

                if (m_SoCellObjectData is SoCellObjectData_Attacker)
                {
                    SoCellObjectData_Attacker soCellObjectData = (SoCellObjectData_Attacker)m_SoCellObjectData;

                    _attackDamageText.text = soCellObjectData.SoAttackerStatData.AttackDPS.ToString();
                    _attackRangeText.text  = soCellObjectData.SoAttackerStatData.AttackRange.ToString();
                }

                // Debug.Log(m_IsShowUnlockPanel);
                if (m_IsShowUnlockPanel)
                {
                    // Unlock 패널 On,
                    _unlockPanel.SetActive(true);
                }
                else
                {
                    _unlockPanel.SetActive(false);
                }

                OnSelectButtonClicked?.Invoke(this);
            }
        }


        public void OnUnlockButtonClicked()
        {
            if (m_IsSelected == false || m_IsShowUnlockPanel == false || m_IsOpen) return;

            if (!PlayerDataManager.Instance.TryConsumeGold(m_SoCellObjectData.UnlockPrice)) return;

            if (!PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[(int)m_SoCellObjectData.CellObjectType])
            {
                PlayerDataManager.Instance.UnlockCellObject(m_SoCellObjectData);
            }
            else
            {
                PlayerDataManager.Instance.LevelUpCellObjectLevel(m_SoCellObjectData);
            }
            _unlockPanel.SetActive(false);

            GameManager.Instance.GameMazeController.ChangeCellObjects(m_SoCellObjectData);
        }


        public void Refresh(CompGUICellObjectElement selectedCellObjectElement)
        {
            m_IsSelected = selectedCellObjectElement == this;

            var cellObjectLevel = PlayerDataManager.Instance.PlayerData.CellObjectLevelList[(int)m_SoCellObjectData.CellObjectType];

            if (!PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[(int)m_SoCellObjectData.CellObjectType])
            {
                if (_level == 0) SetUnlockPanel(true);
                if (_level == 1) SetUnlockPanel(false);
                if (_level == 2) SetUnlockPanel(false);

                if (_level == 0) SetOpen(false);
                if (_level == 1) SetOpen(false);
                if (_level == 2) SetOpen(false);

                return;
            }

            switch (cellObjectLevel)
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