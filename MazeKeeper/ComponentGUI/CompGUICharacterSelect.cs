using System.Collections;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUICharacterSelect : MonoBehaviour
    {
        const int CinemachineCameraPriorityDefault = 10;
        const int CinemachineCameraPriorityFocus   = 11;

        [SerializeField] SoCharacterData _characterData0;
        [SerializeField] SoCharacterData _characterData1;
        [SerializeField] SoCharacterData _characterData2;

        [SerializeField] Button _character0;
        [SerializeField] Button _character1;
        [SerializeField] Button _character2;

        [SerializeField] CinemachineCamera _cameraForFullShot;
        [SerializeField] CinemachineCamera _cameraForCharacter0;
        [SerializeField] CinemachineCamera _cameraForCharacter1;
        [SerializeField] CinemachineCamera _cameraForCharacter2;

        [SerializeField] Button      _gameStartButton;
        [SerializeField] Button      _settingButton;
        [SerializeField] SoAudioClip _bgmSoAudioClip;

        [SerializeField] CompGUICharacterDetailWindow _characterDetailWindow;

        [SerializeField] TMP_Text _remainGemText;
        [SerializeField] TMP_Text _stageText;

        bool m_IsStarted;


        IEnumerator Start()
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            _gameStartButton.onClick.AddListener(StartGame);
            _settingButton.onClick.AddListener(PopupManager.Instance.ShowSettingPop);

            AudioManager.Instance.Play(_bgmSoAudioClip);

            _character0.onClick.AddListener(() => { SelectCharacter(0); });
            _character1.onClick.AddListener(() => { SelectCharacter(1); });
            _character2.onClick.AddListener(() => { SelectCharacter(2); });

            _characterDetailWindow.gameObject.SetActive(false);
            _characterDetailWindow.OnClose += OnCloseCharacterDetailWindow;

            RefreshGemText();
            PlayerDataManager.Instance.OnGemChanged += RefreshGemText;

            _stageText.text = $"STAGE {PlayerDataManager.Instance.PlayerData.CurrentStageIndex + 1}";

            yield return new WaitForSeconds(0.5f);
            ToastManager.Instance.ToastTitle("Character\nSelect & Level Up");
        }


        void OnDestroy()
        {
            PlayerDataManager.Instance.OnGemChanged -= RefreshGemText;
        }


        public void RefreshGemText()
        {
            _remainGemText.text = PlayerDataManager.Instance.PlayerData.Gem.ToString();
        }


        void StartGame()
        {
            if (m_IsStarted) return;
            m_IsStarted = true;

            SceneLoadManager.Instance.ChangeScene(SceneName.MazeScene);
        }


        void SelectCharacter(int characterIndex)
        {
            ResetCameraPriority();
            SetActiveCharacterButtons(false);

            switch (characterIndex)
            {
                case 0:
                    _cameraForCharacter0.Priority.Value                          = CinemachineCameraPriorityFocus;
                    PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData = _characterData0;
                    _characterDetailWindow.Init(_characterData0);
                    _gameStartButton.gameObject.SetActive(PlayerDataManager.Instance.PlayerData.CharacterDataList[(int)CharacterType.Character0].Unlocked);
                    return;
                case 1:
                    _cameraForCharacter1.Priority.Value                          = CinemachineCameraPriorityFocus;
                    PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData = _characterData1;
                    _characterDetailWindow.Init(_characterData1);
                    _gameStartButton.gameObject.SetActive(PlayerDataManager.Instance.PlayerData.CharacterDataList[(int)CharacterType.Character1].Unlocked);
                    return;
                case 2:
                    _cameraForCharacter2.Priority.Value                          = CinemachineCameraPriorityFocus;
                    PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData = _characterData2;
                    _characterDetailWindow.Init(_characterData2);
                    _gameStartButton.gameObject.SetActive(PlayerDataManager.Instance.PlayerData.CharacterDataList[(int)CharacterType.Character2].Unlocked);
                    return;
                default:
                    ReturnFullShotCamera();
                    return;
            }
        }


        void ReturnFullShotCamera()
        {
            ResetCameraPriority();
            _cameraForFullShot.Priority.Value = CinemachineCameraPriorityFocus;
        }


        void ResetCameraPriority()
        {
            _cameraForFullShot.Priority.Value   = CinemachineCameraPriorityDefault;
            _cameraForCharacter0.Priority.Value = CinemachineCameraPriorityDefault;
            _cameraForCharacter1.Priority.Value = CinemachineCameraPriorityDefault;
            _cameraForCharacter2.Priority.Value = CinemachineCameraPriorityDefault;
        }


        void SetActiveCharacterButtons(bool active)
        {
            _character0.gameObject.SetActive(active);
            _character1.gameObject.SetActive(active);
            _character2.gameObject.SetActive(active);
        }


        void OnCloseCharacterDetailWindow()
        {
            SetActiveCharacterButtons(true);
            ReturnFullShotCamera();
        }
    }
}