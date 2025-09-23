using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompCharacterAnimator : MonoBehaviour
    {
        const float IdleThreshold = 0.2f;

        [Tooltip("Tune this to the animation in the model: feet should not slide when walking at this speed")]
        public float NormalWalkSpeed = 1.7f;

        [Tooltip("Tune this to the animation in the model: feet should not slide when sprinting at this speed")]
        public float NormalSprintSpeed = 5;

        [Tooltip("Never speed up the sprint animation more than this, to avoid absurdly fast movement")]
        public float MaxSprintScale = 1.4f;

        [Tooltip("Scale factor for the overall speed of the jump animation")]
        public float JumpAnimationScale = 0.65f;


        protected struct AnimationParams
        {
            public bool    IsWalking;
            public bool    IsRunning;
            public bool    IsJumping;
            public bool    LandTriggered;
            public bool    JumpTriggered;
            public Vector3 Direction;   // normalized direction of motion
            public float   MotionScale; // scale factor for the animation speed
            public float   JumpScale;   // scale factor for the jump animation
        }


        CompCharacterController m_CompCharacterController;

        AnimationParams m_AnimationParams;
        Animator        m_Animator;

        bool m_IsInitialized;


        public void Init()
        {
            m_CompCharacterController            =  GetComponentInParent<CompCharacterController>();
            m_Animator                           =  GetComponentInChildren<Animator>();
            m_CompCharacterController.StartJump  += () => m_AnimationParams.JumpTriggered = true;
            m_CompCharacterController.EndJump    += () => m_AnimationParams.LandTriggered = true;
            m_CompCharacterController.PostUpdate += (vel, jumpAnimationScale) => UpdateAnimationState(vel, jumpAnimationScale);
            m_IsInitialized                      =  true;
        }


        void UpdateAnimationState(Vector3 vel, float jumpAnimationScale)
        {
            if (m_IsInitialized == false) return;

            vel.y = 0; // we don't consider vertical movement
            var speed = vel.magnitude;

            // Hysteresis reduction
            bool isRunning = speed > NormalWalkSpeed * 2 + (m_AnimationParams.IsRunning ? -0.15f : 0.15f);
            bool isWalking = !isRunning && speed > IdleThreshold + (m_AnimationParams.IsWalking ? -0.05f : 0.05f);
            m_AnimationParams.IsWalking = isWalking;
            m_AnimationParams.IsRunning = isRunning;

            // Set the normalized direction of motion and scale the animation speed to match motion speed
            m_AnimationParams.Direction   = speed > IdleThreshold ? vel / speed : Vector3.zero;
            m_AnimationParams.MotionScale = isWalking ? speed / NormalWalkSpeed : 1;
            m_AnimationParams.JumpScale   = JumpAnimationScale * jumpAnimationScale;

            // We scale the sprint animation speed to loosely match the actual speed, but we cheat
            // at the high end to avoid making the animation look ridiculous
            if (isRunning)
                m_AnimationParams.MotionScale = (speed < NormalSprintSpeed)
                    ? speed / NormalSprintSpeed
                    : Mathf.Min(MaxSprintScale, 1 + (speed - NormalSprintSpeed) / (3 * NormalSprintSpeed));

            UpdateAnimationParameter(m_AnimationParams);

            if (m_AnimationParams.JumpTriggered) m_AnimationParams.IsJumping = true;
            if (m_AnimationParams.LandTriggered) m_AnimationParams.IsJumping = false;

            m_AnimationParams.JumpTriggered = false;
            m_AnimationParams.LandTriggered = false;
        }


        void UpdateAnimationParameter(AnimationParams animationParams)
        {
            if (m_IsInitialized == false) return;

            m_Animator.SetFloat("DirX", animationParams.Direction.x);
            m_Animator.SetFloat("DirZ", animationParams.Direction.z);
            m_Animator.SetFloat("MotionScale", animationParams.MotionScale);
            m_Animator.SetBool("Walking", animationParams.IsWalking);
            m_Animator.SetBool("Running", animationParams.IsRunning);
            m_Animator.SetFloat("JumpScale", animationParams.JumpScale);

            if (m_AnimationParams.JumpTriggered) m_Animator.SetTrigger("Jump");
            if (m_AnimationParams.LandTriggered) m_Animator.SetTrigger("Land");
        }
    }
}