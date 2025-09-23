using TMPro;
using UnityEngine;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIToastWarning : MonoBehaviour
    {
        public const float DefaultLifeTime = 1.2f;

        [SerializeField] TMP_Text _messageText;

        float m_ElapsedTime;
        bool  m_IsShow;
        float m_LifeTime;


        public void Show(string message, float lifeTime = DefaultLifeTime)
        {
            _messageText.text = message;
            m_LifeTime        = lifeTime;
            m_ElapsedTime     = 0f;
            m_IsShow          = true;
            gameObject.SetActive(true);
        }


        void Update()
        {
            if (m_IsShow == false) return;

            m_ElapsedTime += Time.unscaledDeltaTime;
            if (m_ElapsedTime >= m_LifeTime)
            {
                Hide();
            }
        }


        public void Hide()
        {
            m_IsShow = false;
            gameObject.SetActive(false);
        }
    }
}