using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class PauseManager : ManagerBase<PauseManager>
    {
        public bool IsPaused => m_PauseStackCount > 0;

        int m_PauseStackCount;


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Time.timeScale = 1f;
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Time.timeScale++;
            }
        }


        public void Pause()
        {
            m_PauseStackCount++;
            Time.timeScale = 0;
            AudioManager.Instance.Stop(AudioChannelType.FX);
        }


        public void UnPause()
        {
            if (IsPaused == false) return;

            m_PauseStackCount--;
            if (m_PauseStackCount <= 0)
            {
                Time.timeScale = 1;
            }
        }
    }
}