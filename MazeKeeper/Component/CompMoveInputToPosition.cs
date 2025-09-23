using UnityEngine;
using UnityEngine.InputSystem;


namespace MazeKeeper.Component
{
    public class CompMoveInputToPosition : MonoBehaviour
    {
        [SerializeField] float _moveSpeed;

        float m_MoveX;
        float m_MoveZ;


        void Update()
        {
            var forward = transform.forward;
            var right   = transform.right;
            var pos     = transform.position;

            forward *= m_MoveZ;
            right   *= m_MoveX;

            var dir = forward + right;

            dir.y = 0;

            dir.Normalize();

            pos += dir * (_moveSpeed * Time.deltaTime);

            transform.position = pos;
        }


        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 rawInput = context.ReadValue<Vector2>();
            m_MoveX = rawInput.x;
            m_MoveZ = rawInput.y;
        }
    }
}