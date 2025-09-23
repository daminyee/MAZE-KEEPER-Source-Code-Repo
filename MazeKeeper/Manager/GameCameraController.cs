using MazeKeeper.Component;
using MazeKeeper.Define;
using Unity.Cinemachine;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class GameCameraController : MonoBehaviour
    {
        const int CinemachineCameraPriorityDefault = 10;
        const int CinemachineCameraPriorityUsing   = 11;

        [SerializeField] CinemachineCamera _placingCamera;
        [SerializeField] CinemachineCamera _freeLookCamera;
        [SerializeField] CinemachineCamera _aimCamera;
        [SerializeField] CinemachineCamera _skillAimCamera;

        [SerializeField] GameObject               _aimCore;
        [SerializeField] GameObject               _skillAimCore;
        [SerializeField] CinemachineOrbitalFollow _freeLookCinemachineOrbitalFollow;

        CompLookInputToRotation m_AimCoreCompLookInputToRotation;


        void Start()
        {
            GameManager.Instance.OnModeChanged += OnModeChanged;

            m_AimCoreCompLookInputToRotation = _aimCore.GetComponent<CompLookInputToRotation>();
        }


        /// <summary>
        /// 게임 상태 변경에 따른 카메라Priority 변경, 필요한 처리 수행
        /// </summary>
        void OnModeChanged(GameModeType previousGameMode, GameModeType currentGameMode)
        {
            _placingCamera.Priority.Value  = CinemachineCameraPriorityDefault;
            _freeLookCamera.Priority.Value = CinemachineCameraPriorityDefault;
            _aimCamera.Priority.Value      = CinemachineCameraPriorityDefault;
            _skillAimCamera.Priority.Value = CinemachineCameraPriorityDefault;

            switch (currentGameMode)
            {
                case GameModeType.PlacingMode:
                    _placingCamera.Priority.Value = CinemachineCameraPriorityUsing;
                    break;
                case GameModeType.BattleMode:
                    _freeLookCamera.Priority.Value = CinemachineCameraPriorityUsing;
                    break;
                case GameModeType.BattleAimMode:
                    _aimCamera.Priority.Value = CinemachineCameraPriorityUsing;
                    break;
                case GameModeType.BuffSkillAimMode:
                    _skillAimCamera.Priority.Value = CinemachineCameraPriorityUsing;
                    break;
                case GameModeType.AttackSkill1AimMode:
                    _skillAimCamera.Priority.Value = CinemachineCameraPriorityUsing;
                    break;
                case GameModeType.AttackSkill2AimMode:
                    _skillAimCamera.Priority.Value = CinemachineCameraPriorityUsing;
                    break;
            }

            switch (currentGameMode)
            {
                case GameModeType.BattleAimMode:
                    var eulerAngles = Camera.main.transform.eulerAngles;
                    m_AimCoreCompLookInputToRotation.ResetAngle(eulerAngles.y, eulerAngles.x);
                    break;
                case GameModeType.BuffSkillAimMode:
                case GameModeType.AttackSkill1AimMode:
                case GameModeType.AttackSkill2AimMode:
                    _skillAimCore.GetComponent<CompFollower>().FollowRotationY = false;
                    _skillAimCore.transform.rotation                           = Quaternion.Euler(60f, Camera.main.transform.rotation.eulerAngles.y, 0f);
                    break;
                default:
                case GameModeType.BattleMode:
                    _skillAimCore.GetComponent<CompFollower>().FollowRotationY = true;
                    _freeLookCinemachineOrbitalFollow.HorizontalAxis.Value     = Camera.main.transform.rotation.eulerAngles.y;
                    _freeLookCinemachineOrbitalFollow.VerticalAxis.Value       = m_AimCoreCompLookInputToRotation.transform.rotation.eulerAngles.x;
                    break;
            }
        }
    }
}
