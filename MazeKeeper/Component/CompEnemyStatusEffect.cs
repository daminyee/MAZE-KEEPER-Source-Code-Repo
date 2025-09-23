using System;
using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompEnemyStatusEffect : MonoBehaviour
    {
        public event Action<List<(EnemyStatusEffectType Type, bool CurrentState)>> OnStatusEffectTypeChanged;

        readonly List<EnemyStatusEffectItem> m_StatusEffectItemList = new();
        /// <summary>
        /// OnStatusEffectTypeChanged 이벤트에 현재 변경된 상태를 전달해주기 위한 리스트.
        /// 변화가 일어나는 순간에 List Add, Invoke직후에는 Clear
        /// </summary>
        readonly List<(EnemyStatusEffectType Type, bool CurrentState)> m_ChangedStatusEffectList = new();


        bool m_IsChanged;


        void OnEnable()
        {
            m_StatusEffectItemList.Clear();
            m_ChangedStatusEffectList.Clear();
            m_IsChanged = false;
        }


        void Update()
        {
            for (int i = m_StatusEffectItemList.Count - 1; i >= 0; i--)
            {
                var StatusEffectItem = m_StatusEffectItemList[i];
                StatusEffectItem.RemainingTime -= Time.deltaTime;
                if (StatusEffectItem.RemainingTime < 0)
                {
                    m_StatusEffectItemList.Remove(StatusEffectItem);
                    m_ChangedStatusEffectList.Add((StatusEffectItem.EnemyStatusEffectType, false));
                    m_IsChanged = true;
                }
            }
            if (m_IsChanged) RefreshStatusEffectChange();
        }

        /// <summary>
        /// 상태이상 효과가 적용되어 있는지 확인용
        /// </summary>
        public bool IsAppliedStatusEffect(EnemyStatusEffectType statusEffectType)
        {
            foreach (EnemyStatusEffectItem t in m_StatusEffectItemList)
            {
                if (t.EnemyStatusEffectType == statusEffectType) return true;
            }
            return false;
        }

        /// <summary>
        /// 상태이상 추가
        /// </summary>
        public void Add(EnemyStatusEffectItem statusEffectItem)
        {
            foreach (EnemyStatusEffectItem t in m_StatusEffectItemList)
            {
                // 이미 걸려 있는 상태이상이고 지속시간이 더 길면 지속시간 갱신(중첩X)
                if (t.EnemyStatusEffectType == statusEffectItem.EnemyStatusEffectType)
                {
                    if (t.RemainingTime < statusEffectItem.RemainingTime)
                    {
                        t.RemainingTime = statusEffectItem.RemainingTime;
                    }
                    return;
                }
            }
            m_StatusEffectItemList.Add(statusEffectItem.Clone());
            m_ChangedStatusEffectList.Add((statusEffectItem.EnemyStatusEffectType, true));
            m_IsChanged = true;
        }


        void RefreshStatusEffectChange()
        {
            OnStatusEffectTypeChanged?.Invoke(m_ChangedStatusEffectList);
            m_ChangedStatusEffectList.Clear();
            m_IsChanged = false;
        }
    }
}
