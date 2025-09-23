using System;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Component
{
    [RequireComponent(typeof(CompAttackerStat))]
    public class CompAttacker : MonoBehaviour
    {
        public CompAttackerStat CompAttackerStat => m_CompAttackerStat;

        public CompEnemy Target { get; private set; }

        public bool IsAttack => m_IsAttack;

        /// <summary>
        /// 공격을 멈추고 있을지
        /// </summary>
        public bool IsHolding { get; set; }

        /// <summary>
        /// 자동 공격.
        /// Attack를 수동으로 호출하지 않아도, Update를 통해 자동으로 Attack
        /// </summary>
        [SerializeField] bool _useAutoAttack = true;

        [SerializeField] SoAudioClip _soAttackAudioClip;

        [SerializeField] Option<CompLookAtTarget> _lookAtTargetOption;

        protected float m_AttackElapsedTime;
        protected bool  m_IsAttack;
        protected bool  HasTarget { get; private set; }

        CompAttackerStat    m_CompAttackerStat;
        SoEnemyStatusEffect m_SoEnemyStatusEffect;


        bool m_IsInitialized;


        public void Init(SoAttackerStatData soAttackerStatData, SoEnemyStatusEffect soEnemyStatusEffect)
        {
            m_CompAttackerStat = GetComponent<CompAttackerStat>();
            m_CompAttackerStat.SetBaseStat(AttackerStatType.AttackDPS, soAttackerStatData.AttackDPS);
            m_CompAttackerStat.SetBaseStat(AttackerStatType.AttackInterval, soAttackerStatData.AttackInterval);
            m_CompAttackerStat.SetBaseStat(AttackerStatType.AttackRange, soAttackerStatData.AttackRange);
            m_CompAttackerStat.SetBaseStat(AttackerStatType.AttackBuffSpeed, 1f);
            m_CompAttackerStat.RefreshStat();

            m_SoEnemyStatusEffect = soEnemyStatusEffect;

            m_IsInitialized = true;
        }


        protected virtual void Update()
        {
            if (m_IsInitialized == false) return;
            if (IsHolding) return;

            m_AttackElapsedTime += Time.deltaTime;


            if (_useAutoAttack)
            {
                // 타겟이 유효하지않다면, 새로 찾는다.
                var isValidTarget = IsValidTarget();
                if (isValidTarget == false)
                {
                    var enemy = GameManager.Instance.GameEnemyController.FindEnemy(FindEnemyMethod.Closest, transform.position, GetAttackRange());
                    if (enemy != null)
                    {
                        Target = enemy.transform.GetComponent<CompEnemy>();
                        if (_lookAtTargetOption.Enabled)
                        {
                            _lookAtTargetOption.Value.LookTarget = Target.TargetTransform;
                        }
                        HasTarget = true;
                    }
                    else
                    {
                        Target    = null;
                        HasTarget = false;
                        return;
                    }
                }

                // 쿨타임이 다 돌았다면 공겨
                if (IsCoolDownToAttack()) Attack();
            }
        }


        public void SetBaseStat(AttackerStatType baseStatType, float statValue)
        {
            m_CompAttackerStat.SetBaseStat(baseStatType, statValue);
            m_CompAttackerStat.RefreshStat();
        }


        public void SetTarget(CompEnemy target)
        {
            Target    = target;
            HasTarget = target != null;
        }


        public virtual float GetAttackDamage()   => m_CompAttackerStat.GetAttackDamage();
        public         float GetAttackInterval() => m_CompAttackerStat.GetAttackInterval();
        public         float GetAttackRange()    => m_CompAttackerStat.GetAttackRangeStat();

        public bool IsCoolDownToAttack() => IsAttack == false && m_AttackElapsedTime >= GetAttackInterval();


        /// <summary>
        /// Target이 살아있고, 공격범위 안에 있다면 true
        /// </summary>
        public bool IsValidTarget()
        {
            if (Target == null) return false;
            if (Target.IsValid() == false) return false;

            var     range   = GetAttackRange();
            Vector3 diff    = Target.TargetTransform.position - transform.position;
            var     sqrDist = diff.sqrMagnitude;
            return range * range >= sqrDist;
        }


        public virtual void Attack()
        {
            m_AttackElapsedTime = 0f;
            AudioManager.Instance.Play(_soAttackAudioClip);
        }


        public void ApplyEnemyStatusEffect(CompEnemy target)
        {
            if (m_SoEnemyStatusEffect == null) return;
            if (UnityEngine.Random.Range(0f, 1f) <= m_SoEnemyStatusEffect.ApplyProbability)
            {
                target.EnemyStatusEffect.Add(m_SoEnemyStatusEffect.EnemyStatusEffect);
            }
        }
    }
}
