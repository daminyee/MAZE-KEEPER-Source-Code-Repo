using System;
using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompCharacterSkillShooter : MonoBehaviour
    {
        public List<CompCharacterSkillAttacker> CharacterSkillAttackerList => m_CharacterSkillAttackerList;

        /// <summary>
        /// AttackSkill 사용시에 바닥에 그려지는 3D 인디케이터.
        /// </summary>
        [SerializeField] GameObject _attackSkillAimIndicatorGO;
        [SerializeField] GameObject _uniqueSkillAimIndicatorGO;

        readonly List<CompCharacterSkillAttacker> m_CharacterSkillAttackerList = new();

        Camera m_MainCam;

        bool m_IsInitialized;
        bool m_IsPlayingAttackSkill1;
        bool m_IsPlayingAttackSkill2;

        bool IsPlayingAnyAttackSkill => m_IsPlayingAttackSkill1 || m_IsPlayingAttackSkill2;


        /// <summary>
        /// 하위 Model에 설정된 SkillAttacker들을 연결한다.
        /// Start에서 초기화 하지 않는 이유는, Start시점에는 Model이 존재하지 않기 때문이다.
        /// </summary>
        public void Init()
        {
            m_CharacterSkillAttackerList.Clear();
            for (int i = 0; i < (int)CharacterSkillType.Length; i++)
            {
                m_CharacterSkillAttackerList.Add(null);
            }

            m_CharacterSkillAttackerList[(int)CharacterSkillType.BuffSkill] = GetComponentInChildren<CompCharacterSkillAttacker_Buff>();

            foreach (var characterAttackSkillAttacker in GetComponentsInChildren<CompCharacterSkillAttacker_Attack>())
            {
                if (characterAttackSkillAttacker.SkillType == CharacterSkillType.AttackSkill1)
                {
                    m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill1] = characterAttackSkillAttacker;
                }
                if (characterAttackSkillAttacker.SkillType == CharacterSkillType.AttackSkill2)
                {
                    m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill2] = characterAttackSkillAttacker;
                }
            }
            m_MainCam       = Camera.main;
            m_IsInitialized = true;
        }


        void Update()
        {
            if (m_IsInitialized == false) return;

            switch (GameManager.Instance.CurrentGameMode)
            {
                case GameModeType.PlacingMode:
                case GameModeType.BattleMode:
                case GameModeType.BattleAimMode:
                    if (_uniqueSkillAimIndicatorGO.activeSelf)
                    {
                        _uniqueSkillAimIndicatorGO.SetActive(false);
                    }
                    if (_attackSkillAimIndicatorGO.activeSelf)
                    {
                        _attackSkillAimIndicatorGO.SetActive(false);
                    }
                    break;
                case GameModeType.BuffSkillAimMode:
                {
                    // 마우스 포지션으로 부터 Ray를 쏴서 RayHit 위치에 _attackSkillAimIndicatorGO를 위치시킨다.
                    var mousePosition = Input.mousePosition;
                    var ray           = m_MainCam.ScreenPointToRay(mousePosition);
                    var layer         = 1 << GameMazeController.PlacingRayHit;

                    if (Physics.Raycast(ray, out var hit, 1000f, layer))
                    {
                        var targetPosition = hit.collider.gameObject;
                        if (targetPosition != null)
                        {
                            if (_uniqueSkillAimIndicatorGO.activeSelf == false)
                            {
                                _uniqueSkillAimIndicatorGO.SetActive(true);
                                var indicatorScale = Vector3.one * m_CharacterSkillAttackerList[(int)CharacterSkillType.BuffSkill].SplashRadius * 2f;
                                _uniqueSkillAimIndicatorGO.transform.localScale = indicatorScale;
                            }
                            _uniqueSkillAimIndicatorGO.transform.position = hit.point;
                        }
                    }
                }
                    break;
                case GameModeType.AttackSkill1AimMode:
                case GameModeType.AttackSkill2AimMode:
                {
                    // 마우스 포지션으로 부터 Ray를 쏴서 RayHit 위치에 _attackSkillAimIndicatorGO를 위치시킨다.
                    var mousePosition = Input.mousePosition;
                    var ray           = m_MainCam.ScreenPointToRay(mousePosition);
                    var layer         = 1 << GameMazeController.PlacingRayHit;

                    if (Physics.Raycast(ray, out var hit, 1000f, layer))
                    {
                        var targetPosition = hit.collider.gameObject;
                        if (targetPosition != null)
                        {
                            if (_attackSkillAimIndicatorGO.activeSelf == false)
                            {
                                _attackSkillAimIndicatorGO.SetActive(true);

                                if (GameManager.Instance.CurrentGameMode == GameModeType.AttackSkill1AimMode)
                                {
                                    var indicatorScale = Vector3.one * m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill1].SplashRadius * 2f;
                                    _attackSkillAimIndicatorGO.transform.localScale = indicatorScale;
                                }
                                if (GameManager.Instance.CurrentGameMode == GameModeType.AttackSkill2AimMode)
                                {
                                    var indicatorScale = Vector3.one * m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill2].SplashRadius * 2f;
                                    _attackSkillAimIndicatorGO.transform.localScale = indicatorScale;
                                }
                            }
                            _attackSkillAimIndicatorGO.transform.position = hit.point;
                        }
                    }
                }
                    break;
            }

            m_IsPlayingAttackSkill1 = ((CompCharacterSkillAttacker_Attack)m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill1]).IsAttacking;
            m_IsPlayingAttackSkill2 = ((CompCharacterSkillAttacker_Attack)m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill2]).IsAttacking;
        }


        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (IsPlayingAnyAttackSkill) return;

                switch (GameManager.Instance.CurrentGameMode)
                {
                    case GameModeType.BuffSkillAimMode:
                        if (!m_CharacterSkillAttackerList[(int)CharacterSkillType.BuffSkill].IsCoolDownToUseSkill()) return;
                        m_CharacterSkillAttackerList[(int)CharacterSkillType.BuffSkill].UseSkill(_uniqueSkillAimIndicatorGO.transform.position);
                        GameManager.Instance.ChangeMode(GameModeType.BattleMode);
                        break;
                    case GameModeType.AttackSkill1AimMode:
                        if (!m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill1].IsCoolDownToUseSkill()) return;
                        m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill1].UseSkill(_attackSkillAimIndicatorGO.transform.position);
                        GameManager.Instance.ChangeMode(GameModeType.BattleMode);
                        break;
                    case GameModeType.AttackSkill2AimMode:
                        if (!m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill2].IsCoolDownToUseSkill()) return;
                        m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill2].UseSkill(_attackSkillAimIndicatorGO.transform.position);
                        GameManager.Instance.ChangeMode(GameModeType.BattleMode);
                        break;
                }
            }
        }


        public void OnBuffSkillAim(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.CurrentGameMode != GameModeType.BattleMode && GameManager.Instance.CurrentGameMode != GameModeType.BattleAimMode) return;

            if (context.performed)
            {
                if (!m_CharacterSkillAttackerList[(int)CharacterSkillType.BuffSkill].IsCoolDownToUseSkill())
                {
                    ToastManager.Instance.ToastWarning("아직 해당 스킬을 사용할 수 없습니다!");
                    return;
                }
                GameManager.Instance.ChangeMode(GameModeType.BuffSkillAimMode);
            }
        }


        public void OnSkill1Aim(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.CurrentGameMode != GameModeType.BattleMode && GameManager.Instance.CurrentGameMode != GameModeType.BattleAimMode) return;

            if (context.performed)
            {
                if (!m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill1].IsCoolDownToUseSkill())
                {
                    ToastManager.Instance.ToastWarning("아직 해당 스킬을 사용할 수 없습니다!");
                    return;
                }
                GameManager.Instance.ChangeMode(GameModeType.AttackSkill1AimMode);
            }
        }


        public void OnSkill2Aim(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.CurrentGameMode != GameModeType.BattleMode && GameManager.Instance.CurrentGameMode != GameModeType.BattleAimMode) return;

            if (context.performed)
            {
                if (!m_CharacterSkillAttackerList[(int)CharacterSkillType.AttackSkill2].IsCoolDownToUseSkill())
                {
                    ToastManager.Instance.ToastWarning("아직 해당 스킬을 사용할 수 없습니다!");
                    return;
                }
                GameManager.Instance.ChangeMode(GameModeType.AttackSkill2AimMode);
            }
        }
    }
}
