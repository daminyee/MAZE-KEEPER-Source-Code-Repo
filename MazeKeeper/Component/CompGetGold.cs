using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompGetGold : MonoBehaviour
    {
        [SerializeField] float _delayTime;

        float m_ElapsedTime;

        int m_Gold;

        bool m_IsSetValue;


        void Update()
        {
            if (!m_IsSetValue) return;

            m_ElapsedTime += Time.deltaTime;
            if (m_ElapsedTime >= _delayTime && m_Gold > 0)
            {
                PlayerDataManager.Instance.AddGold(m_Gold);
                m_ElapsedTime = 0;
                m_IsSetValue  = false;
            }
        }


        public void SetGold(int gold)
        {
            m_Gold       = gold;
            m_IsSetValue = true;
        }
    }
}