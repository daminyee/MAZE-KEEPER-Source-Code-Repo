using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUICellObjectBuyInfo : MonoBehaviour
    {
        public SoCellObjectData SoCellObjectData => m_SoCellObjectData;

        [SerializeField] Image      _turretIconImage;
        [SerializeField] TMP_Text   _nameText;
        [SerializeField] TMP_Text   _levelText;
        [SerializeField] TMP_Text   _priceText;
        [SerializeField] Button     _buyButton;
        [SerializeField] GameObject _lockImage;
        [SerializeField] GameObject _selectedIndicator;
        [SerializeField] TMP_Text   _placeCountText;


        SoCellObjectData m_SoCellObjectData;


        void Start()
        {
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
            GameManager.Instance.GamePlacingController.OnCellObjectUpdated += RefreshPlaceCountText;

            switch (m_SoCellObjectData.CellObjectType)
            {
                case CellObjectType.Obstacle:
                    _levelText.gameObject.SetActive(false);
                    _placeCountText.gameObject.SetActive(false);
                    break;
                case CellObjectType.TrapFire:
                case CellObjectType.TrapSpike:
                    _levelText.gameObject.SetActive(true);
                    _placeCountText.gameObject.SetActive(false);
                    break;
                default:
                    _levelText.gameObject.SetActive(true);
                    _placeCountText.gameObject.SetActive(true);
                    break;
            }
        }


        public void Init(SoCellObjectData soCellObjectData)
        {
            m_SoCellObjectData      = soCellObjectData;
            _turretIconImage.sprite = m_SoCellObjectData.ObjectSprite;
            _nameText.text          = m_SoCellObjectData.ObjectName;
            _levelText.text         = $"Lv.{(m_SoCellObjectData.Level + 1).ToString()}";
            _priceText.text         = m_SoCellObjectData.PlacePrice.ToString();

            _lockImage.SetActive(!PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[(int)m_SoCellObjectData.CellObjectType]);
            RefreshPlaceCountText();
        }


        public void OnBuyButtonClicked()
        {
            if (!PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[(int)m_SoCellObjectData.CellObjectType])
            {
                ToastManager.Instance.ToastWarning("잠겨있어서 배치할 수 없습니다. [TAB]키로 해금창을 열어보세요.");
                return;
            }

            var placingObject = GameManager.Instance.GamePlacingController.PlacingCellObject;
            if (placingObject != null)
            {
                Debug.Log(placingObject);

                if (placingObject.SoCellObjectData.CellObjectType == m_SoCellObjectData.CellObjectType)
                {
                    GameManager.Instance.GamePlacingController.ChangePlaceMode(GamePlacingController.PlaceMode.NormalMode);
                    return;
                }
            }
            GameManager.Instance.GamePlacingController.ChangePlaceMode(GamePlacingController.PlaceMode.PlacingMode, this);
        }


        public void ShowSelectedButtonIndicator(bool show)
        {
            if (_selectedIndicator.gameObject.activeSelf != show)
            {
                _selectedIndicator.gameObject.SetActive(show);
            }
        }


        void RefreshPlaceCountText()
        {
            _placeCountText.text = $"{GameManager.Instance.GameMazeController.PlacedCellObjectCountList[(int)m_SoCellObjectData.CellObjectType]} / {m_SoCellObjectData.PlaceLimit}";
        }
    }
}