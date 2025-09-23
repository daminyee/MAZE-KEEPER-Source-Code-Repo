using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIPopup_PauseMenu : CompGUIPopup
    {
        [SerializeField] Button _resumeButton;
        [SerializeField] Button _settingButton;
        [SerializeField] Button _quitButton;
        [SerializeField] Button _closeButton;


        void Awake()
        {
            _resumeButton.onClick.AddListener(Hide);
            _settingButton.onClick.AddListener(() => PopupManager.Instance.ShowSettingPop());
            _quitButton.onClick.AddListener(() =>
                                            {
                                                Hide();
                                                SceneLoadManager.Instance.ChangeScene(SceneName.TitleScene);
                                            });
            _closeButton.onClick.AddListener(Hide);
        }


        public override void Show()
        {
            base.Show();
            PauseManager.Instance.Pause();
        }


        public override void Hide()
        {
            base.Hide();
            PauseManager.Instance.UnPause();
        }
    }
}