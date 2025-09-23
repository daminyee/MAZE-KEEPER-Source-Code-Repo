using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Component;
using MazeKeeper.ComponentGUI;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace MazeKeeper.Manager
{
    public class GameManager : ManagerBase<GameManager>
    {
        public GamePathController    GamePathController    => _gamePathController;
        public GameMazeController    GameMazeController    => _gameMazeController;
        public GameEnemyController   GameEnemyController   => _gameEnemyController;
        public GameCameraController  GameCameraController  => _gameCameraController;
        public GamePlacingController GamePlacingController => _gamePlacingController;
        public GameModeType          CurrentGameMode       => m_CurrentGameMode;
        public List<CompGate>        CompGateList          => _compGateList;

        public float Hp    => _hp;
        public float MaxHp => _maxHp;


        public event Action<GameModeType, GameModeType> OnModeChanged;
        public event Action                             OnPlayerHpChanged;

        [SerializeField] GamePlacingController _gamePlacingController;
        [SerializeField] GamePathController    _gamePathController;
        [SerializeField] GameMazeController    _gameMazeController;
        [SerializeField] GameEnemyController   _gameEnemyController;
        [SerializeField] GameCameraController  _gameCameraController;

        [SerializeField] GameObject _characterGo;
        [SerializeField] GameObject _aimReticleGO;

        [FormerlySerializedAs("_gateList")]
        [SerializeField] List<CompGate> _compGateList;
        [SerializeField] List<SoStageData> _soStageDataList;
        [FormerlySerializedAs("_goal")]
        [SerializeField] CompGoal _compGoal;

        [SerializeField] SoAudioClip _bgmSoAudioClipInPlacingMode;
        [SerializeField] SoAudioClip _bgmSoAudioClipInBattleMode;
        [SerializeField] SoAudioClip _bgmSoAudioClipWinGameMode;
        [SerializeField] SoAudioClip _fxSoAudioClipRandomMaze;

        [SerializeField] CinemachineImpulseSource _cinemachineImpulseSource;


        [SerializeField] float _maxHp;
        [SerializeField] float _hp;

        GameModeType m_CurrentGameMode;
        GameModeType m_PreviousMode;


        IEnumerator Start()
        {
#if UNITY_EDITOR
            // 에디터에서 테스트시에 일시정지 했을때 락된 커서를 풀어줌.
            UnityEditor.EditorApplication.pauseStateChanged += (state) => { Cursor.lockState = CursorLockMode.None; };
#endif

            _hp = _maxHp;
            _gamePathController.HideEnemyPath();
            BattleUIManager.Instance.gameObject.SetActive(false);

            _gameMazeController.Init();
            _gamePathController.Init();
            foreach (var gate in _compGateList)
            {
                gate.Init(_soStageDataList[PlayerDataManager.Instance.PlayerData.CurrentStageIndex]);
            }
            _compGoal.Init();

            AudioManager.Instance.Play(_fxSoAudioClipRandomMaze);

            yield return new WaitForSeconds(1.5f);

            _gamePathController.ShowEnemyPath();

            ToastManager.Instance.ToastTitle($"STAGE {PlayerDataManager.Instance.PlayerData.CurrentStageIndex + 1}", "랜덤 미로 생성");

            BattleUIManager.Instance.gameObject.SetActive(true);
            BattleUIManager.Instance.ShowCharacterSkillPanel(false);

            yield return new WaitForSeconds(1f);

            AudioManager.Instance.Play(_bgmSoAudioClipInPlacingMode);
        }


        void LateUpdate()
        {
            UpdateCursorLockState();
        }


        /// <summary>
        /// 현재 GameMode에 따른 마우스 잠금 상태 변경
        /// </summary>
        void UpdateCursorLockState()
        {
            switch (m_CurrentGameMode)
            {
                case GameModeType.PlacingMode:
                    if (Cursor.lockState != CursorLockMode.None)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    break;
                case GameModeType.GoingToNextStageMode:
                case GameModeType.BattleMode:
                case GameModeType.BattleAimMode:
                case GameModeType.GameClear:
                case GameModeType.GameOver:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Normal);
                    if (PauseManager.Instance.IsPaused)
                    {
                        if (Cursor.lockState != CursorLockMode.None)
                        {
                            Cursor.lockState = CursorLockMode.None;
                        }
                    }
                    else
                    {
                        if (Cursor.lockState != CursorLockMode.Locked)
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                        }
                    }
                    break;
                case GameModeType.BuffSkillAimMode:
                case GameModeType.AttackSkill1AimMode:
                case GameModeType.AttackSkill2AimMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Attack);
                    if (Cursor.lockState != CursorLockMode.None)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    break;
                case GameModeType.UpgradeCellObjectMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Normal);
                    if (Cursor.lockState != CursorLockMode.None)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// 현재 GameMode 변경
        /// </summary>
        public void ChangeMode(GameModeType gameMode)
        {
            m_PreviousMode    = m_CurrentGameMode;
            m_CurrentGameMode = gameMode;
            OnModeChanged?.Invoke(m_PreviousMode, m_CurrentGameMode);

            switch (m_CurrentGameMode)
            {
                case GameModeType.BattleAimMode:
                    _aimReticleGO.gameObject.SetActive(true);
                    break;
                case GameModeType.BattleMode:
                    _aimReticleGO.gameObject.SetActive(false);
                    break;
                default:
                    _aimReticleGO.gameObject.SetActive(false);
                    break;
            }
        }


        // 조준(New Input System 이용)
        public void OnAim(InputAction.CallbackContext context)
        {
            switch (m_CurrentGameMode)
            {
                case GameModeType.PlacingMode:
                    break;
                case GameModeType.BattleMode:
                    if (context.started) ChangeMode(GameModeType.BattleAimMode);
                    break;
                case GameModeType.BattleAimMode:
                    if (context.canceled) ChangeMode(GameModeType.BattleMode);
                    break;
                case GameModeType.BuffSkillAimMode:
                case GameModeType.AttackSkill1AimMode:
                case GameModeType.AttackSkill2AimMode:
                    if (context.performed) ChangeMode(GameModeType.BattleMode);
                    break;
            }
        }


        public void BakeNavSurface(bool bakeWithEnemySurface)
        {
            _gamePathController.BakeNavSurface(bakeWithEnemySurface);
        }


        /// <summary>
        /// 게임 시작, gate들의 시작 함수 호출
        /// </summary>
        public void WaveStart()
        {
            AudioManager.Instance.Play(_bgmSoAudioClipInBattleMode);
            ChangeMode(GameModeType.BattleMode);
            ToastManager.Instance.ToastTitle($"STAGE {PlayerDataManager.Instance.PlayerData.CurrentStageIndex + 1}", "START!");

            _characterGo.GetComponent<CompCharacterController>().Init();

            BattleUIManager.Instance.InitCharacterSkillIcon();
            BattleUIManager.Instance.ShowCharacterSkillPanel(true);

            foreach (var gate in _compGateList)
            {
                gate.WaveStart();
            }

            _gamePathController.ChangeInGamePathMaterial();
        }


        /// <summary>
        /// 게임 승리
        /// </summary>
        public void CheckWinGame()
        {
            if (CurrentGameMode == GameModeType.GameClear) return;

            foreach (var gate in _compGateList)
            {
                if (gate.IsNowSpawning) return;
            }
            if (GameEnemyController.EnemyList.Count > 0) return;

            if (_hp <= 0) return;

            ChangeMode(GameModeType.GameClear);
            AudioManager.Instance.Play(_bgmSoAudioClipWinGameMode);

            // 마지막 스테이지가 아닐 시, 다음 스테이지로. 마지막 스테이지일 시, 게임 클리어 씬으로
            if (PlayerDataManager.Instance.PlayerData.CurrentStageIndex == _soStageDataList.Count - 1f)
            {
                StartCoroutine(GameClear());
            }
            else
            {
                if (CurrentGameMode != GameModeType.GoingToNextStageMode)
                {
                    StartCoroutine(GotoNextStage());
                }
            }
        }


        /// <summary>
        /// 크리스탈 체력 까임
        /// </summary>
        public void GetDamage(float damage)
        {
            _hp = Mathf.Clamp(_hp -= damage, 0, _maxHp);

            OnPlayerHpChanged?.Invoke();
            _cinemachineImpulseSource.GenerateImpulse();
            _compGoal.Goal();

            if (_hp <= 0)
            {
                if (CurrentGameMode != GameModeType.GameOver)
                {
                    StartCoroutine(GameOver());
                }
            }
        }


        void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"OnApplicationPause {pauseStatus}");
            Cursor.lockState = CursorLockMode.None;
        }


        IEnumerator GotoNextStage()
        {
            ChangeMode(GameModeType.GoingToNextStageMode);
            PlayerDataManager.Instance.SetNextStage();
            ToastManager.Instance.ToastTitle("VICTORY!", $"STAGE {PlayerDataManager.Instance.PlayerData.CurrentStageIndex}에서 모든 적을 섬멸하였습니다!");
            yield return new WaitForSeconds(5.0f);
            SceneLoadManager.Instance.ChangeScene(SceneName.CharacterSelectScene);
        }


        IEnumerator GameOver()
        {
            ChangeMode(GameModeType.GameOver);
            _compGoal.Explode();
            foreach (var gate in _compGateList)
            {
                gate.WaveStop();
            }
            ToastManager.Instance.ToastTitle($"Game Over", "Crystal Destroyed");
            yield return new WaitForSeconds(5.0f);
            PlayerDataManager.Instance.Reset();
            SceneLoadManager.Instance.ChangeScene(SceneName.TitleScene);
        }


        IEnumerator GameClear()
        {
            ChangeMode(GameModeType.GameClear);
            ToastManager.Instance.ToastTitle($"Game Clear", "You Finished Last Enemy");
            yield return new WaitForSeconds(5.0f);
            PlayerDataManager.Instance.Reset();
            SceneLoadManager.Instance.ChangeScene(SceneName.ClearScene);
        }
    }
}
