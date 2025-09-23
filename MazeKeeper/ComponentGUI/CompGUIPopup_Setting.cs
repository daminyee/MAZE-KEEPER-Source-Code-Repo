using MazeKeeper.Define;
using MazeKeeper.Manager;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIPopup_Setting : CompGUIPopup
    {
        [SerializeField] Slider _masterVolumeSlider;
        [SerializeField] Slider _bgmVolumeSlider;
        [SerializeField] Slider _fxVolumeSlider;
        [SerializeField] Slider _uiVolumeSlider;
        [SerializeField] Button _closeButton;


        void Awake()
        {
            var audioManager = AudioManager.Instance;
            _masterVolumeSlider.onValueChanged.AddListener((volume) =>
                                                           {
                                                               audioManager.SetVolume(AudioManager.GetVolumeKey("Master"), volume);
                                                           });
            _bgmVolumeSlider.onValueChanged.AddListener((volume) =>
                                                        {
                                                            audioManager.SetVolume(AudioManager.GetVolumeKey(nameof(AudioChannelType.BGM)), volume);
                                                        });
            _fxVolumeSlider.onValueChanged.AddListener((volume) =>
                                                       {
                                                           audioManager.SetVolume(AudioManager.GetVolumeKey(nameof(AudioChannelType.FX)), volume);
                                                       });
            _uiVolumeSlider.onValueChanged.AddListener((volume) =>
                                                       {
                                                           audioManager.SetVolume(AudioManager.GetVolumeKey(nameof(AudioChannelType.UI)), volume);
                                                       });

            _closeButton.onClick.AddListener(Hide);
        }


        public override void Show()
        {
            base.Show();

            var audioManager = AudioManager.Instance;
            _masterVolumeSlider.value = audioManager.GetVolume(AudioManager.GetVolumeKey("Master"));
            _bgmVolumeSlider.value    = audioManager.GetVolume(AudioManager.GetVolumeKey(nameof(AudioChannelType.BGM)));
            _fxVolumeSlider.value     = audioManager.GetVolume(AudioManager.GetVolumeKey(nameof(AudioChannelType.FX)));
            _uiVolumeSlider.value     = audioManager.GetVolume(AudioManager.GetVolumeKey(nameof(AudioChannelType.UI)));
        }
    }
}