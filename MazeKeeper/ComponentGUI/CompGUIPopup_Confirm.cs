using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIPopup_Confirm : CompGUIPopup
    {
        public bool IsConfirmed { get; private set; }

        public bool IsCanceled { get; private set; }

        [SerializeField] Button   _confirmButton;
        [SerializeField] Button   _cancelButton;
        [SerializeField] TMP_Text _messageText;


        void Awake()
        {
            //각 버튼에 람다함수 등록
            _confirmButton.onClick.AddListener(() => IsConfirmed = true);
            _cancelButton.onClick.AddListener(() => IsCanceled   = true);
        }


        public void Show(string message)
        {
            _messageText.text = message;
            IsConfirmed       = false;
            IsCanceled        = false;
            Show();
        }


        public override void Hide()
        {
            base.Hide();
            IsConfirmed = false;
            IsCanceled  = true;
        }
    }
}