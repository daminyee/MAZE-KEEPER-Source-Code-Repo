using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.ComponentGUI;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using UnityEngine;
using UnityEngine.AI;


namespace MazeKeeper.Component
{
    public class CompEnemy : MonoBehaviour
    {
        public static bool IsInvincible;

        public int  DropGold    => _dropGold;
        public int  DropGem     => _dropGem;
        public int  AttackPower => _attackPower;
        public bool IsBossEnemy => _isBossEnemy;

        public CompEnemyStatusEffect EnemyStatusEffect => m_EnemyStatusEffect;
        public Transform             TargetTransform   => _targetPosTransform;
        public Vector2Int            CurrentCellPos    => m_CurrentCellPos;


        [SerializeField] int          _maxHp       = 100;
        [SerializeField] int          _attackPower = 1;
        [SerializeField] int          _dropGold;
        [SerializeField] int          _dropGem;
        [SerializeField] Transform    _uiPosTransform;
        [SerializeField] Transform    _targetPosTransform;
        [SerializeField] NavMeshAgent _navMeshAgent;
        [SerializeField] bool         _isBossEnemy;


        float m_DefaultMoveSpeed;
        float m_DebuffMoveSpeed;

        float m_Hp;
        bool  m_IsDead;

        float m_BurnInterval = 1f;

        Vector3    m_StartPos;
        Vector3    m_TargetPos;
        Vector2Int m_CurrentCellPos;

        CompGUIHudHpBar             m_HUDHpBar;
        CompGUIHudEnemyStatusEffect m_HudStatusEffect;

        CompGUIHudMiniMapMover m_CompGUIHudMiniMapMover;

        CompEnemyStatusEffect m_EnemyStatusEffect;

        PlayablePlayer m_PlayablePlayer;


        void Awake()
        {
            m_EnemyStatusEffect = GetComponent<CompEnemyStatusEffect>();
            m_DefaultMoveSpeed  = _navMeshAgent.speed;
            m_PlayablePlayer    = new(gameObject, "GetDamage");

            if (!_isBossEnemy)
            {
                m_EnemyStatusEffect.OnStatusEffectTypeChanged += CheckAppliedStatusEffect;
            }
        }


        // Spawn될 때 초기화 
        public void Init(Vector3 startPos, Vector3 targetPos)
        {
            m_HUDHpBar               = BattleUIManager.Instance.SpawnGUIHudHpBar(_uiPosTransform);
            m_HudStatusEffect        = BattleUIManager.Instance.SpawnGuiHudEnemyStatusEffect(_uiPosTransform, this);
            m_CompGUIHudMiniMapMover = BattleUIManager.Instance.SpawnGUIEnemyMover(transform);
            m_Hp                     = _maxHp;
            m_DebuffMoveSpeed        = m_DefaultMoveSpeed * 0.5f;
            m_StartPos               = startPos;
            m_TargetPos              = targetPos;
            m_IsDead                 = false;

            _navMeshAgent.enabled = true;
            SetDestination();

            GameManager.Instance.GameEnemyController.AddEnemy(this);
        }


        void Update()
        {
            if (m_IsDead) return;


            var result = GameManager.Instance.GameMazeController.GetCellPosFromWorldPosition(transform.position);
            m_CurrentCellPos = result.IsValid ? result.CellPos : Vector2Int.zero;

            if (EnemyStatusEffect.IsAppliedStatusEffect(EnemyStatusEffectType.Burn))
            {
                m_BurnInterval -= Time.deltaTime;
                if (m_BurnInterval <= 0)
                {
                    // 남은 피의 1/8 정도 Damage
                    GetDamage(Mathf.Min(_maxHp * 0.125f, 100));
                    m_BurnInterval = 1f;
                }
            }

            // 적이 목표 지점에 골인하면 플레이어한테 피해 주고 사망 처리
            // 실제 SetDestination직후 이동이 시작되기 전까지 path계산이 끝나지 않은 상태일때에는 pathPending이 true
            if (_navMeshAgent.pathPending == false)
            {
                const float GoalThreshold = 0.5f;
                if (Vector3.Distance(transform.position, m_TargetPos) < GoalThreshold)
                {
                    GameManager.Instance.GameEnemyController.EnterGoal(this);
                    Despawn();
                }
            }
        }


        public bool IsValid()
        {
            return m_Hp > 0 && gameObject.activeInHierarchy;
        }


        public void GetDamage(float damage, Vector3? direction = null)
        {
            if (IsInvincible) return;
            if (m_IsDead) return;

            if (EnemyStatusEffect.IsAppliedStatusEffect(EnemyStatusEffectType.Weak))
            {
                damage *= _isBossEnemy ? 1.15f : 1.5f;
            }
            
            //체력이 최대체력을 넘거나 0 이하로 내려가지 않게끔
            m_Hp = Mathf.Clamp(m_Hp - damage, 0f, _maxHp);

            var remainingHpPercent = m_Hp / _maxHp;


            m_PlayablePlayer.Play(direction);


            m_HUDHpBar.SetHpPercent(remainingHpPercent, direction);

            BattleUIManager.Instance.SpawnGUIDamageText(_uiPosTransform, damage, direction);

            if (m_Hp <= 0)
            {
                m_IsDead                = true;
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled   = false;
                GameManager.Instance.GameEnemyController.DeadByDamage(this);
                Despawn();
            }
        }


        void SetDestination()
        {
            _navMeshAgent.Warp(m_StartPos);
            _navMeshAgent.SetDestination(m_TargetPos);
        }


        void Despawn()
        {
            var poolManager = PoolManager.Instance;
            poolManager.Despawn(GetComponent<CompPoolItem>());
            poolManager.Despawn(m_HUDHpBar.gameObject.GetComponent<CompPoolItem>());
            poolManager.Despawn(m_HudStatusEffect.gameObject.GetComponent<CompPoolItem>());
            poolManager.Despawn(m_CompGUIHudMiniMapMover.gameObject.GetComponent<CompPoolItem>());
        }


        void CheckAppliedStatusEffect(List<(EnemyStatusEffectType Type, bool CurrentState)> changedStatusEffectList)
        {
            foreach (var appliedStatusEffect in changedStatusEffectList)
            {
                switch (appliedStatusEffect.Type)
                {
                    // 둔화 상태이상이 걸려 있다면 이동속도 감소
                    case EnemyStatusEffectType.Slow:
                        if (appliedStatusEffect.CurrentState)
                        {
                            _navMeshAgent.speed = m_DebuffMoveSpeed;
                        }
                        else
                        {
                            _navMeshAgent.speed = m_DefaultMoveSpeed;
                        }
                        break;
                    // 둔화 상태이상이 걸려 있다면 이동 정지
                    case EnemyStatusEffectType.Stun:
                        if (appliedStatusEffect.CurrentState)
                        {
                            _navMeshAgent.velocity  = Vector3.zero;
                            _navMeshAgent.isStopped = true;
                        }
                        else
                        {
                            _navMeshAgent.isStopped = false;
                        }
                        break;
                }
            }
        }
    }
}
