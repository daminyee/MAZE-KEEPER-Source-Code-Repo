using System;
using System.Collections.Generic;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine.Samples;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompCharacterController : MonoBehaviour
    {
        [Tooltip("Force of gravity in the down direction (m/s^2)")]
        public float Gravity = 17;

        [Tooltip("Transition duration (in seconds) when the player changes velocity or rotation.")]
        public float Damping = 0.5f;


        [Header("Events")]
        [Tooltip("This event is sent when the player lands after a jump.")]
        public UnityEvent Landed = new();

        public event Action<Vector3, float> PostUpdate;
        public event Action                 StartJump;
        public event Action                 EndJump;

        [Header("이동 속도")]
        [SerializeField] float _walkSpeed = 3.5f;
        [SerializeField] float _sprintSpeed = 8;
        [Tooltip("공격중이거나 아주 천천히 움직여야하는 상황")]
        [SerializeField] float _slowSpeed = 1f;
        [SerializeField] float _jumpSpeed       = 6;
        [SerializeField] float _sprintJumpSpeed = 8;


        [SerializeField] List<GameObject> _modelList;


        CompFollower        m_CompFollower;
        CharacterController m_CharacterController;

        // 캐릭터 상태

        float   m_TimeLastGrounded;
        Vector3 m_CurrentVelocityXZ;
        Vector3 m_LastRawInputDirection;
        Vector3 m_LastInputDirection;
        float   m_CurrentVelocityY;
        bool    m_IsJumping;
        bool    m_Strafe;
        bool    m_IsInitialized;
        bool    m_Slow;

        // Input System에 의한 입력 상태

        bool  m_IsJumpButtonHolding;
        bool  m_IsSprintButtonHolding;
        float m_MoveX;
        float m_MoveZ;


        void Start()
        {
            m_CharacterController              =  GetComponent<CharacterController>();
            m_CompFollower                     =  GetComponent<CompFollower>();
            GameManager.Instance.OnModeChanged += OnModeChanged;
        }


        public float WalkSpeed       => _walkSpeed * MoveSpeedStatValue;
        public float SprintSpeed     => _sprintSpeed * MoveSpeedStatValue;
        public float JumpSpeed       => _jumpSpeed;
        public float SprintJumpSpeed => _sprintJumpSpeed;
        public float SlowSpeed       => _slowSpeed * MoveSpeedStatValue;

        float MoveSpeedStatValue => PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData.GetCurrentScoCharacterStatData(CharacterUpgradableStatType.MoveSpeed).StatValue;


        public void Init()
        {
            _modelList[(int)PlayerDataManager.Instance.PlayerData.CurrentCharacterData.CharacterType].SetActive(true);

            GetComponent<CompCharacterShooter>().Init();
            GetComponent<CompCharacterSkillShooter>().Init();
            GetComponent<CompCharacterAnimator>().Init();
            m_IsInitialized = true;
        }


        public void SetSlowSpeed(bool slow) => m_Slow = slow;


        void OnEnable()
        {
            m_IsInitialized         = false;
            m_CurrentVelocityY      = 0;
            m_Strafe                = false;
            m_IsSprintButtonHolding = false;
            m_IsJumping             = false;
            m_TimeLastGrounded      = Time.time;
        }


        void Update()
        {
            if (m_IsInitialized == false) return;

            // 점프의 시작, 점프후 낙하속도 계산, 착지 처리.
            ProcessJump();

            // WASD로 입력받은 방향에 카메라 방향을 곱해서, 항상 카메라 방향이 W키를 눌렀을때 움직이는 방향이 되도록 한다.
            m_LastRawInputDirection = new(m_MoveX, 0, m_MoveZ);
            var inputFrame = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
            m_LastInputDirection = inputFrame * m_LastRawInputDirection;
            m_LastInputDirection.Normalize();

            // 방향에 속도를 곱해서 원하는 Velocity를 구한다.
            Vector3 desiredVelocity;
            if (m_Slow)
            {
                desiredVelocity = m_LastInputDirection * SlowSpeed;
            }
            else
            {
                desiredVelocity = m_LastInputDirection * (m_IsSprintButtonHolding ? SprintSpeed : WalkSpeed);
            }

            // 원하는 Velocity와 현재 Velocity를 부드럽게 전환한다.
            // 즉, 이동 방향이 부드럽게 전환된다.
            if (Vector3.Angle(m_CurrentVelocityXZ, desiredVelocity) < 100)
            {
                // 부드럽게 회전
                m_CurrentVelocityXZ = Vector3.Slerp(m_CurrentVelocityXZ, desiredVelocity, Damper.Damp(1, Damping, Time.deltaTime));
            }
            else
            {
                // 100도가 넘으면 빠른속도로 회전하도록한다.
                m_CurrentVelocityXZ += Damper.Damp(desiredVelocity - m_CurrentVelocityXZ, Damping, Time.deltaTime);
            }

            // 캐릭터의 위치 이동
            m_CharacterController.Move((m_CurrentVelocityY * Vector3.up + m_CurrentVelocityXZ) * Time.deltaTime);

            // 캐릭터가 이동방향을 바라보도록 회전시킨다.
            if (!m_Strafe && m_CurrentVelocityXZ.sqrMagnitude > 0.001f)
            {
                var qA = transform.rotation;
                var qB = Quaternion.LookRotation(m_CurrentVelocityXZ, Vector3.up);
                transform.rotation = Quaternion.Slerp(qA, qB, Damper.Damp(1, Damping, Time.deltaTime));
            }

            if (PostUpdate != null)
            {
                // Get local-space velocity
                var vel = Quaternion.Inverse(transform.rotation) * m_CurrentVelocityXZ;
                vel.y = m_CurrentVelocityY;
                PostUpdate?.Invoke(vel, m_IsSprintButtonHolding ? JumpSpeed / SprintJumpSpeed : 1);
            }
        }


        public void OnMove(InputAction.CallbackContext context)
        {
            if (m_IsInitialized == false) return;

            Vector2 rawInput = context.ReadValue<Vector2>();
            m_MoveX = rawInput.x;
            m_MoveZ = rawInput.y;
        }


        public void OnJump(InputAction.CallbackContext context)
        {
            if (m_IsInitialized == false) return;

            if (context.started) m_IsJumpButtonHolding  = true;
            if (context.canceled) m_IsJumpButtonHolding = false;
        }


        public void OnSprint(InputAction.CallbackContext context)
        {
            if (m_IsInitialized == false) return;

            if (context.started) m_IsSprintButtonHolding  = true;
            if (context.canceled) m_IsSprintButtonHolding = false;
        }


        void OnModeChanged(GameModeType previousGameMode, GameModeType currentGameMode)
        {
            switch (currentGameMode)
            {
                case GameModeType.BattleAimMode:
                    m_Strafe               = true;
                    m_CompFollower.enabled = true;
                    break;
                case GameModeType.BuffSkillAimMode:
                case GameModeType.AttackSkill1AimMode:
                case GameModeType.AttackSkill2AimMode:
                    m_Strafe                = true;
                    transform.localRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
                    break;
                default:
                case GameModeType.BattleMode:
                    m_Strafe               = false;
                    m_CompFollower.enabled = false;
                    break;
            }
        }


        void ProcessJump()
        {
            const float DelayBeforeInferringJump = 0.2f;

            bool isGrounded = m_CharacterController.isGrounded;

            // 중력값만큼 -y 값을 적용한다.
            m_CurrentVelocityY -= Gravity * Time.deltaTime;

            // 점프가 아닌상태에서 특정 조건일 경우 점프 시작.
            if (!m_IsJumping)
            {
                if (isGrounded && m_IsJumpButtonHolding)
                {
                    m_IsJumping        = true;
                    m_CurrentVelocityY = m_IsSprintButtonHolding ? SprintJumpSpeed : JumpSpeed;
                }

                // [점프 시작 조건 2] 높은 곳에서 떨어질때 (특정 시간이 지나면)
                if (!isGrounded && Time.time - m_TimeLastGrounded > DelayBeforeInferringJump)
                {
                    m_IsJumping = true;
                }

                // 점프가 시작되었다면 Jump 시작 처리
                if (m_IsJumping)
                {
                    StartJump?.Invoke();
                    isGrounded = false;
                }
            }

            // 착지 처리
            if (isGrounded)
            {
                m_TimeLastGrounded = Time.time;
                m_CurrentVelocityY = 0;

                // 착지 처리
                if (m_IsJumping)
                {
                    m_IsJumping = false;
                    EndJump?.Invoke();
                    Landed.Invoke();
                }
            }
        }
    }
}
