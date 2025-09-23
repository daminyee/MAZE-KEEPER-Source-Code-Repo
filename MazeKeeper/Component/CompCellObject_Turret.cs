using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompCellObject_Turret : CompCellObject
    {
        public SoCellObjectData_Attacker SoCellObjectData_Attacker => (SoCellObjectData_Attacker)SoCellObjectData;

        [SerializeField] CompRangeIndicator _rangeIndicator;

        CompAttacker m_Attacker;


        public override void Init(SoCellObjectData soCellObjectData_Attacker)
        {
            base.Init(soCellObjectData_Attacker);

            _rangeIndicator.SetVisible(true);

            m_Attacker = GetComponent<CompAttacker>();
            // Attacker에 포탑의 스탯, 부여하는 상태이상 효과 적용
            m_Attacker.Init(SoCellObjectData_Attacker.SoAttackerStatData, SoCellObjectData_Attacker.SoEnemyStatusEffect);
        }


        void OnDisable()
        {
            SetActiveNavMeshModifiers(false, false);
            GameManager.Instance.BakeNavSurface(true);
        }


        public override void SetPlacingMode(bool isPlacing)
        {
            base.SetPlacingMode(isPlacing);

            SetRangeShow(isPlacing);
            m_Attacker.IsHolding = isPlacing;
        }


        public void SetRangeShow(bool isShow)
        {
            _rangeIndicator.SetRange(m_Attacker.GetAttackRange());
            _rangeIndicator.SetVisible(isShow);
        }
    }
}
