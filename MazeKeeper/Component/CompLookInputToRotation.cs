using UnityEngine;
using UnityEngine.InputSystem;


namespace MazeKeeper.Component
{
    public class CompLookInputToRotation : MonoBehaviour
    {
        const float DefaultVerticalAngle = 10f;

        [SerializeField] float _mouseSensitivity      = 300f;
        [SerializeField] float _clampVerticalAngleMin = -80f;
        [SerializeField] float _clampVerticalAngleMax = 80f;

        float m_RotationHorizontal;
        float m_RotationVertical = DefaultVerticalAngle;


        public void ResetAngle(float horizontalAngle, float verticalAngle)
        {
            m_RotationHorizontal    = NormalizeAngle(horizontalAngle);
            m_RotationVertical      = NormalizeAngle(verticalAngle);
            transform.localRotation = Quaternion.Euler(m_RotationVertical, m_RotationHorizontal, 0f);
        }


        public void OnLook(InputAction.CallbackContext context)
        {
            // 1. 원시 픽셀 델타 값 읽기
            Vector2 rawInput = context.ReadValue<Vector2>();

            // 2. 해상도로 나누어 정규화 (Normalization)
            // 화면 크기가 0인 경우를 방지하기 위한 예외 처리
            if (Screen.width == 0 || Screen.height == 0) return;

            Vector2 normalizedInput = new Vector2(rawInput.x / Screen.width, rawInput.y / Screen.height);

            // 3. 민감도와 시간을 곱하여 최종 회전값 계산
            float mouseX = normalizedInput.x * _mouseSensitivity * Time.deltaTime;
            float mouseY = normalizedInput.y * _mouseSensitivity * Time.deltaTime;

            m_RotationHorizontal += NormalizeAngle(mouseX);
            m_RotationVertical   += -NormalizeAngle(mouseY);
            m_RotationHorizontal =  NormalizeAngle(m_RotationHorizontal);
            m_RotationVertical   =  Mathf.Clamp(m_RotationVertical, _clampVerticalAngleMin, _clampVerticalAngleMax);

            transform.localRotation = Quaternion.Euler(m_RotationVertical, m_RotationHorizontal, 0f);
        }


        float NormalizeAngle(float angle)
        {
            while (angle > 180) angle  -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
    }
}