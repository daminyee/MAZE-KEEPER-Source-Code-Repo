using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompGetGem : MonoBehaviour
    {
        [SerializeField] float _delayTime;

        float m_ElapsedTime;

        int m_Gem;

        bool m_IsSetValue;


        void Update()
        {
            if (!m_IsSetValue) return;

            m_ElapsedTime += Time.deltaTime;
            if (m_ElapsedTime >= _delayTime && m_Gem > 0)
            {
                PlayerDataManager.Instance.AddGem(m_Gem);
                m_ElapsedTime = 0;
                m_IsSetValue  = false;
            }
        }


        public void SetGem(int gold)
        {
            m_Gem        = gold;
            m_IsSetValue = true;
        }
    }
}