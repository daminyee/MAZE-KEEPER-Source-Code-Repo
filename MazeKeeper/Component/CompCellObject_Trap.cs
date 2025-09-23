using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompCellObject_Trap : CompCellObject
    {
        public SoCellObjectData_Attacker SoCellObjectData_Attacker => (SoCellObjectData_Attacker)SoCellObjectData;

        CompAttacker m_Attacker;

        float m_ElapsedTime;


        public override void Init(SoCellObjectData soCellObjectData_Attacker)
        {
            base.Init(soCellObjectData_Attacker);

            m_Attacker = GetComponent<CompAttacker>();
            m_Attacker.Init(SoCellObjectData_Attacker.SoAttackerStatData, SoCellObjectData_Attacker.SoEnemyStatusEffect);
        }


        void Update()
        {
            if (m_IsPlacing) return;

            m_ElapsedTime += Time.deltaTime;

            // 쿨타임이 되었는지 확인. 아직이면 return;
            if (m_ElapsedTime < m_Attacker.GetAttackInterval()) return;

            // 내 Cell안에 들어온 적들을 모두 찾아서 Damage를 준다.
            var enemies = GameManager.Instance.GameEnemyController.EnemyList;
            foreach (var compEnemy in enemies)
            {
                if (m_PlacedCellPos == compEnemy.CurrentCellPos)
                {
                    m_Attacker.ApplyEnemyStatusEffect(compEnemy);
                    compEnemy.GetDamage(m_Attacker.GetAttackDamage());
                }
            }

            m_ElapsedTime = 0.0f;
        }
    }
}