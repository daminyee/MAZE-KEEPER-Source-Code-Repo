using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompWorldToUI : MonoBehaviour
    {
        [SerializeField] bool   _playOnEnable  = true;
        [SerializeField] string _tagOfUICamera = "UICamera";

        Camera        m_UICamera;
        RectTransform m_RectTransform;

        Transform     m_FollowTransform;
        RectTransform m_ParentRectTransform;

        bool m_IsPlaying;

        bool m_IsInit;


        void OnEnable()
        {
            if (_playOnEnable && m_IsInit)
            {
                m_IsPlaying = true;
            }
            else
            {
                m_IsPlaying = false;
            }
        }


        public void Init(Transform parentTransform, Transform followTransform)
        {
            var cameraGameObject = GameObject.FindWithTag(_tagOfUICamera);
            m_UICamera = cameraGameObject.GetComponent<Camera>();
            if (m_UICamera == null) Debug.LogError("Camera가 없습니다.", this);

            transform.SetParent(parentTransform);
            m_RectTransform       = (RectTransform)transform;
            m_ParentRectTransform = (RectTransform)parentTransform;
            m_FollowTransform     = followTransform;
            m_IsInit              = true;
            m_IsPlaying           = true;
        }


        void LateUpdate()
        {
            if (m_IsPlaying == false) return;

            var targetPosition = m_FollowTransform.position;
            var screenPosition = Camera.main.WorldToScreenPoint(targetPosition);

            Vector2 localPoint;
            // Main Camera가 바라보는 관점 기준으로 _followTransform의 Screen좌표를 가져온다.
            if (screenPosition.z > 0)
            {
                // UI Camera가 바라보는 관점 기준으로 좌표를 반환한다.
                // Main Camera는 실제 게임을 바라보고 있고, UI Camera는 0,0,0에서 정면을 바라보고 있기 때문에 두개를 분리해서 고려해야한다.
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_ParentRectTransform,
                    screenPosition,
                    m_UICamera,
                    out localPoint
                );

                m_RectTransform.anchoredPosition3D = new(localPoint.x, localPoint.y, 0);
                m_RectTransform.localRotation      = Quaternion.identity;
                m_RectTransform.localScale         = Vector3.one;
            }
            else
            {
                const float OutOfCanvas = -3000f;
                m_RectTransform.anchoredPosition3D = new(OutOfCanvas, OutOfCanvas, 0);
            }
        }
    }
}
