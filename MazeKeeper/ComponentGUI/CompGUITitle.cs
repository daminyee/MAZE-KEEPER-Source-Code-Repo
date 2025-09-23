using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUITitle : MonoBehaviour
    {
        [SerializeField] Button      _gameStartButton;
        [SerializeField] Button      _settingButton;
        [SerializeField] SoAudioClip _bgmSoAudioClip;

        bool m_IsStarted;


        void Start()
        {
            Cursor.lockState = CursorLockMode.None;

            _gameStartButton.onClick.AddListener(StartGame);

            _settingButton.onClick.AddListener(() =>
                                               {
                                                   PopupManager.Instance.ShowSettingPop();
                                               });

            AudioManager.Instance.Play(_bgmSoAudioClip);
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
        }


        void StartGame()
        {
            if (m_IsStarted) return;

            SceneLoadManager.Instance.ChangeScene(SceneName.CharacterSelectScene);
            m_IsStarted = true;
        }
    }
}