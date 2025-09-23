using System;
using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.ComponentGUI;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompGate : MonoBehaviour
    {
        public bool IsNowSpawning   => m_NowSpawning;
        public int  TotalEnemyCount => m_TotalEnemyCount;

        public event Action OnEnemySpawned;

        [SerializeField] Transform _guiProgressBarHudPos;
        [SerializeField] Transform _guiProgressTextHudPos;

        [SerializeField] int _gateIndex;


        CompGUIHudGateProgressBar  m_GateProgressBar;
        CompGUIHudGateProgressText m_SpawnProgressText;

        SoGateData m_GateData;

        PlayablePlayer m_PlayablePlayer;


        GateType m_GateType;

        bool m_NowSpawning;
        bool m_IsWaiting;

        float m_TimerOfRemainTime;
        int   m_CurrentWaveEnemySpawnCount;
        int   m_CurrentSpawnInfoIndex;
        int   m_TotalEnemyCount;

        int m_TotalGold;
        int m_TotalGem;

        float m_EndOfSpawnTime;


        void Start()
        {
            m_PlayablePlayer = new(gameObject, "Spawn");

            m_TotalEnemyCount = 0;
            m_TotalGem        = 0;
            m_EndOfSpawnTime  = 0;
        }


        public void Init(SoStageData soStageData)
        {
            m_GateData = soStageData.SoGateDataList[_gateIndex];

            m_GateType          = m_GateData.GateType;
            m_TimerOfRemainTime = m_GateData.GateStartDelay;
            m_NowSpawning       = false;
            m_IsWaiting         = true;

            m_GateProgressBar   = BattleUIManager.Instance.SpawnGUIHudGateProcessBar(_guiProgressBarHudPos);
            m_SpawnProgressText = BattleUIManager.Instance.SpawnGUIHudSpawnRemainTimer(_guiProgressTextHudPos);

            transform.position = GameManager.Instance.GameMazeController.GetGatePosition(m_GateType);
            m_GateProgressBar.SetProgressPercent(0);
            m_GateProgressBar.gameObject.SetActive(false);
            m_SpawnProgressText.gameObject.SetActive(true);

            var waveData = m_GateData.EnemyWaveData;
            foreach (EnemySpawnInfo info in waveData.EnemySpawnInfoList)
            {
                m_EndOfSpawnTime  += info.SpawnStartDelay + (info.SpawnCount * info.SpawnInterval);
                m_TotalEnemyCount += info.SpawnCount;
                m_TotalGold       += info.EnemyTemplate.DropGold * info.SpawnCount;
                m_TotalGem        += info.EnemyTemplate.DropGem * info.SpawnCount;
            }
        }


        void Update()
        {
            if (m_NowSpawning == false) return;

            m_TimerOfRemainTime -= Time.deltaTime;

            if (m_IsWaiting && m_TimerOfRemainTime <= 0)
            {
                m_TimerOfRemainTime = 0;
                m_NowSpawning       = true;
                m_IsWaiting         = false;

                StartCoroutine(CR_SpawnEnemy());
            }
        }


        public void WaveStart()
        {
            m_IsWaiting   = true;
            m_NowSpawning = true;

            m_GateProgressBar.gameObject.SetActive(true);
            m_SpawnProgressText.gameObject.SetActive(true);

            m_GateProgressBar.SetProgressPercent(0f / m_TotalEnemyCount);
            m_SpawnProgressText.SetProgressPercent(0f, m_TotalEnemyCount);
        }


        public void WaveStop()
        {
            StopAllCoroutines();
        }


        public CompEnemy SpawnEnemy(CompEnemy enemyTemplate, Vector3 startPos)
        {
            var directionToCenter = Vector3.zero - startPos;
            var enemyPoolItem     = PoolManager.Instance.Spawn(enemyTemplate.GetComponent<CompPoolItem>(), startPos, Quaternion.LookRotation(new(directionToCenter.x, 0f, directionToCenter.z)), true);
            var enemy             = enemyPoolItem.GetComponent<CompEnemy>();
            enemy.Init(startPos, GameManager.Instance.GameMazeController.GetGoalPosition());

            // 남은 Enemy 카운트 표시
            m_GateProgressBar.SetProgressPercent((float)(m_CurrentWaveEnemySpawnCount + 1) / m_TotalEnemyCount);
            m_SpawnProgressText.SetProgressPercent(m_CurrentWaveEnemySpawnCount + 1, m_TotalEnemyCount);

            // Spawn 연출
            m_PlayablePlayer.Play();

            OnEnemySpawned?.Invoke();

            return enemy;
        }


        IEnumerator CR_SpawnEnemy()
        {
            m_CurrentWaveEnemySpawnCount = 0;
            m_CurrentSpawnInfoIndex      = 0;

            var startGatePos = GameManager.Instance.GameMazeController.GetGatePosition(m_GateType);
            var waveData     = m_GateData.EnemyWaveData;
            
            // waveData내의 적들을 소환
            while (m_CurrentSpawnInfoIndex < waveData.EnemySpawnInfoList.Count)
            {
                var currentWave = waveData.EnemySpawnInfoList[m_CurrentSpawnInfoIndex];
                yield return new WaitForSeconds(currentWave.SpawnStartDelay);

                for (int i = 0; i < currentWave.SpawnCount; i++)
                {
                    if (GameManager.Instance.CurrentGameMode == GameModeType.GoingToNextStageMode) yield break;

                    SpawnEnemy(currentWave.EnemyTemplate, startGatePos);
                    m_CurrentWaveEnemySpawnCount++;

                    if (currentWave.EnemyTemplate.IsBossEnemy)
                    {
                        ToastManager.Instance.ToastBoss("BOSS 출현!");
                    }

                    yield return new WaitForSeconds(currentWave.SpawnInterval);
                }

                m_CurrentSpawnInfoIndex++;
            }

            m_NowSpawning = false;
        }
    }
}
