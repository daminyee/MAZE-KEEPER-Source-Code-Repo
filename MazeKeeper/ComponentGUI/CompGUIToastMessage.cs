using MazeKeeper.Class;
using MazeKeeper.Component;
using MazeKeeper.Interface;
using TMPro;
using UnityEngine;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIToastMessage : MonoBehaviour
    {
        public const float DefaultLifeTime = 3f;

        [SerializeField] TMP_Text                  _messageText;
        [SerializeField] CompTransCanvasGroupAlpha _compTransCanvasGroupAlpha;

        PlayablePlayer m_PlayablePlayer;

        float m_ElapsedTime;
        bool  m_IsShow;
        float m_LifeTime;


        public void Show(string message, float lifeTime = DefaultLifeTime)
        {
            _messageText.text = message;
            m_LifeTime        = lifeTime;
            m_ElapsedTime     = 0f;
            m_IsShow          = true;

            if (gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
            }

            _compTransCanvasGroupAlpha.SetTransDuration(lifeTime);
            m_PlayablePlayer = new(gameObject);
            m_PlayablePlayer.Play();
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