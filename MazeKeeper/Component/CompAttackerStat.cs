using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompAttackerStat : MonoBehaviour
    {
        /// <summary>
        /// 기본 스탯
        /// [주의] InGame에서 확인하기위해 SerializeField로 사용
        /// </summary>
        [Header("확인용")]
        [SerializeField] List<StatItem> _baseStatItemList;
        /// <summary>
        /// 누적된 BuffStat
        /// [주의] InGame에서 확인하기위해 SerializeField로 사용
        /// </summary>
        [SerializeField] List<BuffStatItem> _buffStatItemList;

        [SerializeField] GameObject _attackSpeedBuffEffect;
        [SerializeField] GameObject _attackDamageBuffEffect;


        /// <summary>
        /// 최종 스탯
        /// </summary>
        List<StatItem> m_FinalStatItemList;


        /// <summary>
        /// Interval까지 계산된 한번의 Attack(혹은 Attack묶음)에 적용되는 Damage 
        /// </summary>
        float m_AttackDamage;
        /// <summary>
        /// AttackSpeed Buff가 적용된, 실제 Attack(혹은 Attack묶음)에 적용되는 Interval
        /// </summary>
        float m_AttackInterval;

        /// <summary>
        /// Buff 적용, 혹은 RemainingTime만료 등으로 변화가 있었다면,
        /// 스탯들을 Refresh해야한다.
        /// </summary>
        bool m_IsChanged;


        /// <summary>
        /// 인스펙터 상에서 base 스탯의 입력 오류를 방지하기 위해
        /// 값을 강제로 확인/수정 한다.
        /// </summary>
        void OnValidate()
        {
            if (_baseStatItemList == null) _baseStatItemList = new();
            while (_baseStatItemList.Count < (int)AttackerStatType.Length)
            {
                _baseStatItemList.Add(new());
            }
            _baseStatItemList[(int)AttackerStatType.AttackDPS].StatType       = AttackerStatType.AttackDPS;
            _baseStatItemList[(int)AttackerStatType.AttackDPS].Value          = 0;
            _baseStatItemList[(int)AttackerStatType.AttackInterval].StatType  = AttackerStatType.AttackInterval;
            _baseStatItemList[(int)AttackerStatType.AttackInterval].Value     = 0;
            _baseStatItemList[(int)AttackerStatType.AttackRange].StatType     = AttackerStatType.AttackRange;
            _baseStatItemList[(int)AttackerStatType.AttackRange].Value        = 0;
            _baseStatItemList[(int)AttackerStatType.AttackBuffSpeed].StatType = AttackerStatType.AttackBuffSpeed;
            _baseStatItemList[(int)AttackerStatType.AttackBuffSpeed].Value    = 0;
        }


        void Update()
        {
            // _buffStatItemList 안에 RemainingTime을 deltaTime으로 깎으면서 0보다 작으면 삭제
            for (int i = _buffStatItemList.Count - 1; i >= 0; i--)
            {
                var buffStatItem = _buffStatItemList[i];
                buffStatItem.RemainingTime -= Time.deltaTime;
                if (buffStatItem.RemainingTime < 0)
                {
                    _buffStatItemList.Remove(buffStatItem);
                    m_IsChanged = true;
                }
            }
            if (m_IsChanged) RefreshStat();
        }


        /// <summary>
        /// AttackDPS의 Stat 값을 가져온다.
        /// </summary>
        public float GetAttackDPSStat() => m_FinalStatItemList[(int)AttackerStatType.AttackDPS].Value;


        /// <summary>
        /// AttackRange의 Stat 값을 가져온다.
        /// [주의] 실제 Range계산에도 사용 가능하다.
        /// </summary>
        public float GetAttackRangeStat() => m_FinalStatItemList[(int)AttackerStatType.AttackRange].Value;


        /// <summary>
        /// AttackInterval의 Stat 값을 가져온다.
        /// </summary>
        public float GetAttackIntervalStat() => m_FinalStatItemList[(int)AttackerStatType.AttackInterval].Value;


        /// <summary>
        /// AttackInterval의 Stat 값을 가져온다.
        /// </summary>
        public float GetAttackSpeedStat() => m_FinalStatItemList[(int)AttackerStatType.AttackBuffSpeed].Value;


        /// <summary>
        /// Interval까지 계산된 한번의 Attack(혹은 Attack묶음)에 적용되는 Damage 
        /// </summary>
        public float GetAttackDamage() => m_AttackDamage;


        /// <summary>
        /// AttackSpeed Buff가 적용된, 실제 Attack(혹은 Attack묶음)에 적용되는 Interval
        /// </summary>
        public float GetAttackInterval() => m_AttackInterval;


        /// <summary>
        /// 스탯 갱신
        /// </summary>
        public void RefreshStat()
        {
            // m_FinalStatItemList 초기화.
            if (m_FinalStatItemList == null)
            {
                m_FinalStatItemList = new();
                while (m_FinalStatItemList.Count < (int)AttackerStatType.Length)
                {
                    m_FinalStatItemList.Add(new());
                }
                m_FinalStatItemList[(int)AttackerStatType.AttackDPS].StatType       = AttackerStatType.AttackDPS;
                m_FinalStatItemList[(int)AttackerStatType.AttackInterval].StatType  = AttackerStatType.AttackInterval;
                m_FinalStatItemList[(int)AttackerStatType.AttackRange].StatType     = AttackerStatType.AttackRange;
                m_FinalStatItemList[(int)AttackerStatType.AttackBuffSpeed].StatType = AttackerStatType.AttackBuffSpeed;
                m_FinalStatItemList[(int)AttackerStatType.AttackBuffSpeed].Value    = 1f;
            }

            var totalBuffAttackDPS      = 1f;
            var totalBuffAttackRange    = 1f;
            var totalBuffAttackSpeed    = 1f;
            var totalBuffAttackInterval = 1f;

            var isBuffedAttackSpeed  = false;
            var isBuffedAttackDamage = false;

            foreach (var buffStatItem in _buffStatItemList)
            {
                switch (buffStatItem.StatItem.StatType)
                {
                    case AttackerStatType.AttackDPS:
                        totalBuffAttackDPS   += buffStatItem.StatItem.Value;
                        isBuffedAttackDamage =  true;
                        break;
                    case AttackerStatType.AttackRange:
                        totalBuffAttackRange += buffStatItem.StatItem.Value;
                        break;
                    case AttackerStatType.AttackBuffSpeed:
                        totalBuffAttackSpeed += buffStatItem.StatItem.Value;
                        isBuffedAttackSpeed  =  true;
                        break;
                    case AttackerStatType.AttackInterval:
                        totalBuffAttackInterval += buffStatItem.StatItem.Value;
                        break;
                }
            }
            
            //가진 버프에 따른 최종 스탯 계산

            m_FinalStatItemList[(int)AttackerStatType.AttackDPS].Value       = _baseStatItemList[(int)AttackerStatType.AttackDPS].Value * totalBuffAttackDPS;
            m_FinalStatItemList[(int)AttackerStatType.AttackInterval].Value  = _baseStatItemList[(int)AttackerStatType.AttackInterval].Value * totalBuffAttackInterval;
            m_FinalStatItemList[(int)AttackerStatType.AttackRange].Value     = _baseStatItemList[(int)AttackerStatType.AttackRange].Value * totalBuffAttackRange;
            m_FinalStatItemList[(int)AttackerStatType.AttackBuffSpeed].Value = _baseStatItemList[(int)AttackerStatType.AttackBuffSpeed].Value * totalBuffAttackSpeed;
            m_AttackInterval                                                 = m_FinalStatItemList[(int)AttackerStatType.AttackInterval].Value / m_FinalStatItemList[(int)AttackerStatType.AttackBuffSpeed].Value;
            m_AttackDamage                                                   = m_FinalStatItemList[(int)AttackerStatType.AttackDPS].Value * m_FinalStatItemList[(int)AttackerStatType.AttackInterval].Value;

            if (_attackDamageBuffEffect != null) _attackDamageBuffEffect.SetActive(isBuffedAttackDamage && totalBuffAttackDPS != 1f);
            if (_attackSpeedBuffEffect != null) _attackSpeedBuffEffect.SetActive(isBuffedAttackSpeed && totalBuffAttackSpeed != 1f);

            m_IsChanged = false;
        }

        
        /// <summary>
        /// buffStatItem 추가
        /// </summary>
        public void AddBuffStat(BuffStatItem buffStatItem)
        {
            _buffStatItemList.Add(buffStatItem);
            RefreshStat();
        }

        /// <summary>
        /// 기초 스탯 셋
        /// </summary>
        public void SetBaseStat(AttackerStatType baseStatType, float statValue)
        {
            _baseStatItemList[(int)baseStatType].Value = statValue;
        }
    }
}
