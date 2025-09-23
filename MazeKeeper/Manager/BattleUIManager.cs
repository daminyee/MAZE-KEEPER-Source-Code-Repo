using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Component;
using MazeKeeper.ComponentGUI;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.Manager
{
    public class BattleUIManager : ManagerBase<BattleUIManager>
    {
        public const float MiniMapUISize = 300f;

        public Transform MiniMapTransform => _miniMap.transform;

        [SerializeField] Button _pauseButton;
        [SerializeField] Canvas _worldUICanvas;
        [SerializeField] Button _waveStartButton;

        [Header("Pool Template")]
        [SerializeField] CompGUIHudHpBar _hudHpBarTemplate;
        [SerializeField] CompGUIHudDamageText      _hudDamageTextTemplate;
        [SerializeField] CompGUIHudGateProgressBar _gateProgressBarTemplate;
        [FormerlySerializedAs("_spawnTimerTextTemplate")]
        [SerializeField] CompGUIHudGateProgressText _gateProgressTextTemplate;
        [SerializeField] CompGUIHudMiniMapMover      _compGUIHudMiniMapMover;
        [SerializeField] CompGUIHudEnemyStatusEffect _compGUIHudStatusEffectTemplate;

        [Header("cellObject Buy Info")]
        [SerializeField] CompGUICellObjectBuyInfo _cellObjectBuyInfoTemplate;
        [SerializeField] Transform                      _cellObjectBuyInfoParent;
        [SerializeField] SoCellObjectDataHolder         _soCellObjectDataHolder;
        [SerializeField] CompGUICellObjectUpgradeWindow _compGUICellObjectUpgradeWindow;

        [Header("자원 Hp 정보")]
        [SerializeField] TMP_Text _goldText;
        [SerializeField] TMP_Text _gemText;
        [SerializeField] TMP_Text _hpText;
        [SerializeField] Slider   _hpBar;

        [Header("Enemy Count")]
        [SerializeField] GameObject _enemyCountPanel;
        [SerializeField] TMP_Text _enemyCountText;
        [FormerlySerializedAs("_enemyCountImage")]
        [SerializeField] Image _enemyCountBarImage;

        [Header("Mini Map")]
        [SerializeField] GameObject _miniMap;
        [SerializeField] GameObject _miniMapWallPointTemplate;
        [SerializeField] GameObject _miniMapTurretPointTemplate;
        [SerializeField] GameObject _miniMapCellObjectPointTemplate;

        [Header("Character Skill")]
        [SerializeField] GameObject _characterSkillPanel;
        [SerializeField] List<CompGUIHudCharacterSkillIcon> _compGUICharacterSkillIconList = new();
        [SerializeField] CompCharacterSkillShooter          _characterSkillShooter;

        [SerializeField] TMP_Text _currentStageText;


        readonly List<CompGUICellObjectBuyInfo> CompGUICellObjectBuyInfoList = new();

        GameObject[,]  m_MiniMapCellObjects;
        PlayablePlayer m_PlayablePlayer;


        int m_currentTotalEnemy;

        event Action OnConfirmStart;
        event Action OnCancelStart;


        IEnumerator Start()
        {
            foreach (var gate in GameManager.Instance.CompGateList)
            {
                gate.OnEnemySpawned += RefreshEnemyCount;
            }
            PlayerDataManager.Instance.OnGoldChanged += RefreshGold;
            PlayerDataManager.Instance.OnGemChanged  += RefreshGem;
            GameManager.Instance.OnPlayerHpChanged   += RefreshHp;
            GameManager.Instance.OnPlayerHpChanged   += () => m_PlayablePlayer.Play();
            GameManager.Instance.OnModeChanged       += RefreshOnModeChanged;
            _waveStartButton.onClick.AddListener(OnWaveStartButtonClicked);
            _pauseButton.onClick.AddListener(PopupManager.Instance.ShowPauseMenu);
            OnConfirmStart -= ConfirmStart;
            OnConfirmStart += ConfirmStart;
            OnCancelStart  -= CancelStart;
            OnCancelStart  += CancelStart;

            m_PlayablePlayer     = new(gameObject, "GetDamage");
            m_MiniMapCellObjects = new GameObject[(int)BattleUIManager.MiniMapUISize, (int)BattleUIManager.MiniMapUISize];

            for (int i = 0; i < (int)CellObjectType.Length; i++)
            {
                var compBuyInfo = Instantiate(_cellObjectBuyInfoTemplate, _cellObjectBuyInfoParent);
                compBuyInfo.Init(_soCellObjectDataHolder.GetCurrentSoCellObjectData((CellObjectType)i));
                CompGUICellObjectBuyInfoList.Add(compBuyInfo);
            }
            _cellObjectBuyInfoTemplate.gameObject.SetActive(false);
            ShowWaveStartButton(false);


            // 미니맵 생성
            var stageManager = GameManager.Instance;
            var map          = stageManager.GameMazeController.Cell_Array;
            var PointSize    = MiniMapUISize / (stageManager.GameMazeController.CellCountWithBorder - 1);
            for (int j = 0; j < stageManager.GameMazeController.CellCountWithBorder; j++)
            {
                for (int i = 0; i < stageManager.GameMazeController.CellCountWithBorder; i++)
                {
                    // 벽이 있는 위치에만 생성
                    if (map[j, i])
                    {
                        var wallPoint = Instantiate(_miniMapWallPointTemplate, _miniMap.transform);
                        wallPoint.GetComponent<RectTransform>().anchoredPosition
                            = new Vector3(
                                i * PointSize - (MiniMapUISize * 0.5f),
                                ((stageManager.GameMazeController.CellCountWithBorder - 1) - j) * PointSize - (MiniMapUISize * 0.5f)
                            );
                    }
                }
            }

            RefreshGold();
            RefreshGem();
            RefreshHp();
            RefreshCurrentStageGUI();
            RefreshOnModeChanged(GameModeType.PlacingMode, GameModeType.PlacingMode);

            yield return new WaitForSeconds(3f);

            ShowWaveStartButton(true);
        }


        void OnDestroy()
        {
            PlayerDataManager.Instance.OnGoldChanged -= RefreshGold;
            PlayerDataManager.Instance.OnGemChanged  -= RefreshGem;
        }


        void Update()
        {
            if (GameManager.Instance.CurrentGameMode != GameModeType.PlacingMode) return;

            // 배치물 업그레이드 창 열기
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_compGUICellObjectUpgradeWindow.gameObject.activeSelf)
                {
                    _compGUICellObjectUpgradeWindow.SelectStateInit();
                }
                else
                {
                    GameManager.Instance.GamePlacingController.ChangePlaceMode(GamePlacingController.PlaceMode.UpgradeMode);
                }
                _compGUICellObjectUpgradeWindow.gameObject.SetActive(!_compGUICellObjectUpgradeWindow.gameObject.activeSelf);
            }
        }


        /// <summary>
        /// 미니맵 위에 새로운 오브젝트 생성
        /// </summary>
        public void CreateCellObjectPointToMiniMap(CompCellObject compCellObject, Vector2Int cellPosition)
        {
            GameObject minimapTemplate;
            if (compCellObject is CompCellObject_Turret turret)
            {
                minimapTemplate = _miniMapTurretPointTemplate;
            }
            else minimapTemplate = _miniMapCellObjectPointTemplate;

            var stageManager = GameManager.Instance;
            var PointSize    = MiniMapUISize / (stageManager.GameMazeController.CellCountWithBorder - 1);
            var wallPoint    = Instantiate(minimapTemplate, MiniMapTransform);
            m_MiniMapCellObjects[cellPosition.y, cellPosition.x] = wallPoint;
            wallPoint.GetComponent<RectTransform>().anchoredPosition
                = new Vector3(
                    cellPosition.x * PointSize - (MiniMapUISize * 0.5f),
                    ((stageManager.GameMazeController.CellCountWithBorder - 1) - cellPosition.y) * PointSize - (MiniMapUISize * 0.5f)
                );
        }


        /// <summary>
        /// 미니맵 위의 오브젝트 제거
        /// </summary>
        public void DeleteCellObjectPointToMiniMap(Vector2Int cellPosition)
        {
            Destroy(m_MiniMapCellObjects[cellPosition.y, cellPosition.x].gameObject);
        }


        /// <summary>
        /// 배치물 구매 창 갱신(해금 여부, 업그레이드 레벨)
        /// </summary>
        public void RefreshCellObjectBuyInfo()
        {
            for (int i = 0; i < (int)CellObjectType.Length; i++)
            {
                var compBuyInfo = CompGUICellObjectBuyInfoList[i];
                compBuyInfo.Init(_soCellObjectDataHolder.GetCurrentSoCellObjectData((CellObjectType)i));
            }
        }


        /// <summary>
        /// 바뀔 GameMode에 따른 처리
        /// </summary>
        void RefreshOnModeChanged(GameModeType previousMode, GameModeType currentMode)
        {
            switch (currentMode)
            {
                case GameModeType.PlacingMode:
                    _enemyCountPanel.SetActive(false);
                    break;
                case GameModeType.BattleMode:
                    _cellObjectBuyInfoParent.gameObject.SetActive(false);
                    _enemyCountPanel.SetActive(true);
                    break;
                case GameModeType.BattleAimMode:
                    break;
                case GameModeType.BuffSkillAimMode:
                    break;
                case GameModeType.AttackSkill1AimMode:
                    break;
                case GameModeType.AttackSkill2AimMode:
                    break;
                case GameModeType.UpgradeCellObjectMode:
                    _enemyCountPanel.SetActive(false);
                    break;
                case GameModeType.GoingToNextStageMode:
                    _enemyCountPanel.SetActive(false);
                    break;
                case GameModeType.GameClear:
                    _enemyCountPanel.SetActive(false);
                    break;
                case GameModeType.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentMode), currentMode, null);
            }
        }


        public void InitCharacterSkillIcon()
        {
            for (int i = 0; i < _characterSkillShooter.CharacterSkillAttackerList.Count; i++)
            {
                _compGUICharacterSkillIconList[i].Init(_characterSkillShooter.CharacterSkillAttackerList[i]);
            }
        }


        public void ShowWaveStartButton(bool show)
        {
            if (show && GameManager.Instance.CurrentGameMode == GameModeType.PlacingMode)
            {
                _waveStartButton.gameObject.SetActive(true);
            }
            else
            {
                _waveStartButton.gameObject.SetActive(false);
            }
        }


        // 각종 UI들 Spawn함수


        public CompGUIHudHpBar SpawnGUIHudHpBar(Transform enemyTransform)
        {
            var spawned = PoolManager.Instance.Spawn(_hudHpBarTemplate.gameObject.GetComponent<CompPoolItem>(), Vector3.zero, Quaternion.identity, true);
            var hpBar   = spawned.GetComponent<CompGUIHudHpBar>();
            hpBar.Init(_worldUICanvas.transform, enemyTransform);

            return hpBar;
        }


        public CompGUIHudEnemyStatusEffect SpawnGuiHudEnemyStatusEffect(Transform enemyTransform, CompEnemy enemy)
        {
            var spawned      = PoolManager.Instance.Spawn(_compGUIHudStatusEffectTemplate.gameObject.GetComponent<CompPoolItem>(), Vector3.zero, Quaternion.identity, true);
            var statusEffect = spawned.GetComponent<CompGUIHudEnemyStatusEffect>();
            statusEffect.Init(enemy, _worldUICanvas.transform, enemyTransform);

            return statusEffect;
        }


        public CompGUIHudGateProgressBar SpawnGUIHudGateProcessBar(Transform gateTransform)
        {
            var spawned    = PoolManager.Instance.Spawn(_gateProgressBarTemplate.gameObject.GetComponent<CompPoolItem>(), Vector3.zero, Quaternion.identity, true);
            var processBar = spawned.GetComponent<CompGUIHudGateProgressBar>();
            processBar.Init(_worldUICanvas.transform, gateTransform);
            return processBar;
        }


        public CompGUIHudGateProgressText SpawnGUIHudSpawnRemainTimer(Transform gateTransform)
        {
            var spawned              = PoolManager.Instance.Spawn(_gateProgressTextTemplate.gameObject.GetComponent<CompPoolItem>(), Vector3.zero, Quaternion.identity, true);
            var spawnRemainTimerText = spawned.GetComponent<CompGUIHudGateProgressText>();
            spawnRemainTimerText.Init(_worldUICanvas.transform, gateTransform);
            return spawnRemainTimerText;
        }


        public void SpawnGUIDamageText(Transform enemyTransform, float damage, Vector3? direction = null)
        {
            var pool              = _hudDamageTextTemplate.gameObject.GetComponent<CompPoolItem>();
            var spawned           = PoolManager.Instance.Spawn(pool, Vector3.zero, Quaternion.identity, true);
            var compGUIDamageText = spawned.GetComponent<CompGUIHudDamageText>();
            compGUIDamageText.Init(_worldUICanvas.transform, enemyTransform, damage, direction);
        }


        public CompGUIHudMiniMapMover SpawnGUIEnemyMover(Transform enemyTransform)
        {
            var pool                = _compGUIHudMiniMapMover.gameObject.GetComponent<CompPoolItem>();
            var spawned             = PoolManager.Instance.Spawn(pool, Vector3.zero, Quaternion.identity, true, _miniMap.transform);
            var compGUIMiniMapMover = spawned.GetComponent<CompGUIHudMiniMapMover>();
            compGUIMiniMapMover.SetTarget(enemyTransform);
            return compGUIMiniMapMover;
        }


        public void ShowCharacterSkillPanel(bool show)
        {
            _characterSkillPanel.SetActive(show);
        }


        void OnWaveStartButtonClicked()
        {
            if (PlayerDataManager.Instance.PlayerData.Gold >= 50)
            {
                PopupManager.Instance.ShowPopup("현재 사용되지 않은 골드가 있습니다.\n정말 시작하시겠습니까?", OnConfirmStart, OnCancelStart);
            }
            else
            {
                ConfirmStart();
            }
        }


        void ConfirmStart()
        {
            GameManager.Instance.WaveStart();
            ShowWaveStartButton(false);
        }


        void CancelStart()
        {
            PopupManager.Instance.HideAllPopup();
        }


        void RefreshGold()
        {
            _goldText.text = PlayerDataManager.Instance.PlayerData.Gold.ToString();
        }


        void RefreshGem()
        {
            _gemText.text = PlayerDataManager.Instance.PlayerData.Gem.ToString();
        }


        void RefreshHp()
        {
            var currentHp = GameManager.Instance.Hp;
            var maxHp     = GameManager.Instance.MaxHp;
            _hpText.text = $"{currentHp}/{maxHp}";
            _hpBar.value = currentHp / maxHp;
        }


        void RefreshEnemyCount()
        {
            var totalEnemy = 0f;
            m_currentTotalEnemy += 1;

            foreach (var gate in GameManager.Instance.CompGateList)
            {
                totalEnemy += gate.TotalEnemyCount;
            }
            _enemyCountText.text           = $"{m_currentTotalEnemy} / {totalEnemy}";
            _enemyCountBarImage.fillAmount = (m_currentTotalEnemy / totalEnemy);
        }


        void RefreshCurrentStageGUI()
        {
            _currentStageText.text = $"STAGE {PlayerDataManager.Instance.PlayerData.CurrentStageIndex + 1}";
        }
    }
}
