using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompGatlingBarrel : MonoBehaviour
    {
        [SerializeField] GameObject _barrelObject;
        [SerializeField] float      _barrelSpinSpeed;

        float m_RollSpeedOffset;

        CompAttacker m_Attacker;


        void Start()
        {
            m_Attacker        = GetComponent<CompAttacker>();
            m_RollSpeedOffset = 1;
        }


        void Update()
        {
            if (m_Attacker.IsValidTarget() == false)
            {
                m_RollSpeedOffset = Mathf.Clamp(m_RollSpeedOffset - Time.deltaTime, 0, 1);
            }
            else
            {
                m_RollSpeedOffset = 1;
            }

            //공격 속도 따라 빠르게 돌아감
            float rollSpeed = m_Attacker.CompAttackerStat.GetAttackSpeedStat() * Time.deltaTime * _barrelSpinSpeed * m_RollSpeedOffset;
            _barrelObject.transform.Rotate(0, 0, rollSpeed);
        }
    }
}