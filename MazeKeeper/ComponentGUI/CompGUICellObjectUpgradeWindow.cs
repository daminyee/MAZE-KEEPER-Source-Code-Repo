using System;
using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUICellObjectUpgradeWindow : MonoBehaviour
    {
        public event Action OnClose;

        [SerializeField] SoCellObjectDataHolder _soCellObjectDataHolder;

        [FormerlySerializedAs("_remainGemText")]
        [SerializeField] TMP_Text _remainGoldText;

        [SerializeField] Button _closeButton;

        [SerializeField] GameObject _cellObjectDescriptionPanel;
        [SerializeField] GameObject _guideText;
        [SerializeField] GameObject _selectBorder;

        [SerializeField] List<CompGUICellObjectElement> _compGUICellObjectElementList;

        CompGUICellObjectElement _selectedCompGUICellObjectElement;


        void Start()
        {
            _closeButton.onClick.AddListener(CloseCharacterDetailWindow);
            PlayerDataManager.Instance.OnCellObjectLevelChanged += RefreshCellObjectUnlockElement;
            PlayerDataManager.Instance.OnCellObjectLevelChanged += RefreshGold;
            PlayerDataManager.Instance.OnGoldChanged            += RefreshGold;

            for (var i = 0; i <= (int)CellObjectType.TrapFire; i++)
            {
                for (var j = 0; j <= SoCellObjectData.MaxUpgradeLevel; j++)
                {
                    var cellObject       = _compGUICellObjectElementList[i * (SoCellObjectData.MaxUpgradeLevel + 1) + j];
                    var soCellObjectData = _soCellObjectDataHolder.GetSoCellObjectData((CellObjectType)i, j);
                    cellObject.Init(soCellObjectData);
                    cellObject.OnSelectButtonClicked += OnCellObjectElementSelected;
                }
            }

            RefreshCellObjectUnlockElement();
            RefreshGold();
        }


        public void RefreshGold()
        {
            _remainGoldText.text = PlayerDataManager.Instance.PlayerData.Gold.ToString();
            RefreshCellObjectUnlockElement();
        }


        public void CloseCharacterDetailWindow()
        {
            OnClose?.Invoke();
            gameObject.SetActive(false);
            _cellObjectDescriptionPanel.SetActive(false);
            _selectBorder.SetActive(false);
        }


        public void SelectStateInit()
        {
            _cellObjectDescriptionPanel.SetActive(false);
            _selectBorder.SetActive(false);
            _guideText.gameObject.SetActive(true);
        }


        void OnCellObjectElementSelected(CompGUICellObjectElement selectedCompGUICellObjectElement)
        {
            _guideText.gameObject.SetActive(false);
            _selectedCompGUICellObjectElement = selectedCompGUICellObjectElement;
            RefreshCellObjectUnlockElement();
        }


        void RefreshCellObjectUnlockElement()
        {
            foreach (var cellObjectElement in _compGUICellObjectElementList)
            {
                cellObjectElement.Refresh(_selectedCompGUICellObjectElement);
            }
            BattleUIManager.Instance.RefreshCellObjectBuyInfo();
        }


        void OnDestroy()
        {
            PlayerDataManager.Instance.OnCellObjectLevelChanged -= RefreshCellObjectUnlockElement;
            PlayerDataManager.Instance.OnCellObjectLevelChanged -= RefreshGold;
            PlayerDataManager.Instance.OnGoldChanged            -= RefreshGold;
        }
    }
}