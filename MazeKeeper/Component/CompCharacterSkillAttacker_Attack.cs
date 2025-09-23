using System.Collections;
using System.Linq;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompCharacterSkillAttacker_Attack : CompCharacterSkillAttacker
    {
        public bool IsAttacking => m_IsAttacking;

        [Header("스탯 확인용")]
        [SerializeField] float _attackDamage;
        [SerializeField] float _attackInterval;
        [SerializeField] int   _attackCount;
        [SerializeField] float _startDelay;

        bool m_IsAttacking;

        SoCharacterSkillData_Attack m_SoCharacterSkillData_Attack;
        GameObject                  m_AttackSkillGO;


        protected override void Awake()
        {
            base.Awake();

            m_SoCharacterSkillData_Attack = (SoCharacterSkillData_Attack)SoCharacterSkillData;

            _attackDamage   = m_SoCharacterSkillData_Attack.AttackDamage;
            _attackInterval = m_SoCharacterSkillData_Attack.AttackInterval;
            _attackCount    = m_SoCharacterSkillData_Attack.AttackCount;
            _startDelay     = m_SoCharacterSkillData_Attack.StartDelay;

            var attackSkillTemplate = m_SoCharacterSkillData_Attack.SkillTemplate;
            if (attackSkillTemplate != null)
            {
                m_AttackSkillGO = Instantiate(attackSkillTemplate, Vector3.zero, Quaternion.identity);
                m_AttackSkillGO.gameObject.SetActive(false);
            }
        }


        public override void UseSkill(Vector3 attackCenterPosition)
        {
            m_IsAttacking = true;
            StartCoroutine(CR_Attack(attackCenterPosition));
            StartCoroutine(CR_AttackTimer());
            ResetCoolDown();
        }


        IEnumerator CR_Attack(Vector3 attackCenterPosition)
        {
            if (m_AttackSkillGO != null)
            {
                m_AttackSkillGO.SetActive(true);
                var position  = new Vector3(attackCenterPosition.x, 0f, attackCenterPosition.z);
                var direction = new Vector3(attackCenterPosition.x - transform.position.x, 0f, attackCenterPosition.z - transform.position.z);
                var rotation  = Quaternion.LookRotation(direction, Vector3.up);
                m_AttackSkillGO.transform.SetPositionAndRotation(position, rotation);
            }

            ToastManager.Instance.ToastSkill(m_SoCharacterSkillData_Attack.Name);
            yield return new WaitForSeconds(_startDelay);

            for (int i = 0; i < _attackCount; i++)
            {
                var enemies = GameManager.Instance.GameEnemyController.EnemyList.ToList();
                foreach (var enemy in enemies)
                {
                    if (Vector3.Distance(attackCenterPosition, enemy.transform.position) <= SplashRadius)
                    {
                        enemy.GetDamage(_attackDamage);
                        Debug.Log($"{enemy.name} is damaged");

                        if (m_SoCharacterSkillData_Attack.DamageEffect != null)
                        {
                            PoolManager.Instance.Spawn(m_SoCharacterSkillData_Attack.DamageEffect, enemy.transform.position, enemy.transform.rotation, true);
                        }

                        AudioManager.Instance.Play(m_SoCharacterSkillData_Attack.SoDamageSound);

                        if (m_SoCharacterSkillData_Attack.SoEnemyStatusEffect != null)
                        {
                            enemy.EnemyStatusEffect.Add(m_SoCharacterSkillData_Attack.SoEnemyStatusEffect.EnemyStatusEffect);
                        }
                    }
                }

                yield return new WaitForSeconds(_attackInterval);
            }
        }


        IEnumerator CR_AttackTimer()
        {
            const float AttackSkillDuration = 5f;
            yield return new WaitForSeconds(AttackSkillDuration);
            if (m_AttackSkillGO != null)
            {
                m_AttackSkillGO.SetActive(false);
            }
            m_IsAttacking = false;
        }
    }
}