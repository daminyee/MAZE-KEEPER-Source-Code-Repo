using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompCharacterShooter : MonoBehaviour
    {
        public const int EnemyLayer = 8;

        [SerializeField] GameObject _targetReticleGO;
        [SerializeField] Canvas     _reticleUICanvas;

        [SerializeField] float _attackAnimationStartDelay; // Interval보단 짧아야함

        PlayablePlayer m_PlayablePlayer_TargetReticle;

        CompAttacker_Projectile m_CompAttacker_Projectile;
        Camera                  m_MainCam;
        GameObject              m_Target;

        float    m_AttackElapsedTime;
        Animator m_Animator;
        int      m_AttackAnimationStack;
        bool     m_IsAttacking;

        bool m_IsInitialized;
        bool m_IsActive;

        bool IsFireButtonHolding;

        CompCharacterController m_CompCharacterController;


        void Start()
        {
            GameManager.Instance.OnModeChanged += (previousMode, currentMode) =>
                                                  {
                                                      switch (currentMode)
                                                      {
                                                          case GameModeType.BattleAimMode:
                                                              Activate();
                                                              break;
                                                          default:
                                                              Deactivate();
                                                              break;
                                                      }
                                                  };
        }


        public void Init()
        {
            m_CompCharacterController = GetComponent<CompCharacterController>();
            m_Animator                = GetComponentInChildren<Animator>();
            m_MainCam                 = Camera.main;

            m_CompAttacker_Projectile = GetComponentInChildren<CompAttacker_Projectile>();
            m_CompAttacker_Projectile.Init(PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData.AttackerStat, null);
            m_CompAttacker_Projectile.SetBaseStat(AttackerStatType.AttackDPS, PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData.GetCurrentScoCharacterStatData(CharacterUpgradableStatType.AttackPower).StatValue);

            m_PlayablePlayer_TargetReticle = new(_targetReticleGO);

            m_IsInitialized = true;
        }


        void Update()
        {
            if (m_IsInitialized == false) return;
            if (m_IsActive == false) return;


            bool isCooldown = m_CompAttacker_Projectile.IsCoolDownToAttack();

            // 레이를 쏴서 Enemy Layer에 Hit된 Enemy를 Target으로 가져온다.
            var screenCenter   = new Vector3(Screen.width * 0.5f, Screen.height * 0.65f, 0);
            var ray            = m_MainCam.ScreenPointToRay(screenCenter);
            var layer          = 1 << EnemyLayer;
            var previousTarget = m_Target;

            if (Physics.Raycast(ray, out var hit, 1000f, layer))
            {
                m_Target = hit.collider.gameObject;
                // 만약에 Target이 있다면
                if (m_Target != null)
                {
                    var target = m_Target.GetComponent<CompEnemy>();
                    Debug.Assert(target != null, $"target == null  {m_Target.gameObject.name}");

                    m_CompAttacker_Projectile.SetTarget(target); // 타켓 넘겨주기.

                    _targetReticleGO.SetActive(true);
                    if (previousTarget != m_Target)
                    {
                        _targetReticleGO.GetComponent<CompWorldToUI>().Init(_reticleUICanvas.transform, target.TargetTransform);
                        m_PlayablePlayer_TargetReticle.Play();
                    }
                    if (isCooldown && IsFireButtonHolding && m_IsAttacking == false)
                    {
                        StartCoroutine(CR_Attack());
                    }
                }
                else
                {
                    m_CompAttacker_Projectile.SetTarget(null);
                    _targetReticleGO.SetActive(false);
                    m_Target = null;
                }
            }
            else
            {
                _targetReticleGO.SetActive(false);
                m_Target = null;
            }

            if (m_IsAttacking)
            {
                m_AttackElapsedTime += Time.deltaTime;
            }
        }


        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.started) IsFireButtonHolding  = true;
            if (context.canceled) IsFireButtonHolding = false;
        }


        void Deactivate()
        {
            m_IsActive = false;
            _targetReticleGO.SetActive(false);
        }


        void Activate()
        {
            m_IsActive = true;
        }


        IEnumerator CR_Attack()
        {
            m_CompCharacterController.SetSlowSpeed(true);
            m_IsAttacking = true;
            
            if (m_AttackAnimationStack == 0)
            {
                m_Animator.SetTrigger("Attack1");
                m_AttackAnimationStack++;
            }
            else if (m_AttackAnimationStack == 1)
            {
                m_Animator.SetTrigger("Attack2");
                m_AttackAnimationStack = 0;
            }

            yield return new WaitForSeconds(_attackAnimationStartDelay);

            m_AttackElapsedTime = 0f;
            m_IsAttacking       = false;
            m_CompAttacker_Projectile.Attack();

            yield return new WaitForSeconds(0.25f);

            m_CompCharacterController.SetSlowSpeed(false);
        }
    }
}
