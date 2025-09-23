using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.Component
{
    public class CompCharacterSkillAttacker_Buff : CompCharacterSkillAttacker
    {
        SoCharacterSkillData_Buff m_SoCharacterSkillData_Buff;

        GameObject m_UniqueSkillGO;

        List<CompCellObject> m_TargetCompCellObjectList;


        protected override void Awake()
        {
            base.Awake();

            m_SoCharacterSkillData_Buff = (SoCharacterSkillData_Buff)SoCharacterSkillData;

            var uniqueSkillTemplate = m_SoCharacterSkillData_Buff.SkillTemplate;
            if (uniqueSkillTemplate != null)
            {
                m_UniqueSkillGO = Instantiate(uniqueSkillTemplate, Vector3.zero, Quaternion.identity);
                m_UniqueSkillGO.gameObject.SetActive(false);
            }
        }


        public override void UseSkill(Vector3 buffPosition)
        {
            if (m_UniqueSkillGO != null)
            {
                m_UniqueSkillGO.SetActive(true);
                var position  = new Vector3(buffPosition.x, 0f, buffPosition.z);
                var direction = new Vector3(buffPosition.x - transform.position.x, 0f, buffPosition.z - transform.position.z);
                var rotation  = Quaternion.LookRotation(direction, Vector3.up);
                m_UniqueSkillGO.transform.SetPositionAndRotation(position, rotation);
                m_UniqueSkillGO.transform.localScale = Vector3.one * SplashRadius;
                StartCoroutine(CR_AttackTimer());
            }

            m_TargetCompCellObjectList = GameManager.Instance.GameMazeController.GetCellObjectInRadius(buffPosition, SplashRadius);
            foreach (var cellObject in m_TargetCompCellObjectList)
            {
                var compAttackerStat = cellObject.GetComponent<CompAttackerStat>();
                if (compAttackerStat == null) continue;
                Debug.Log($"{compAttackerStat.name}'s previous Attack Damage: {compAttackerStat.GetAttackDamage()}");
                Debug.Log($"{compAttackerStat.name}'s previous Attack Interval: {compAttackerStat.GetAttackInterval()}");

                compAttackerStat.AddBuffStat(m_SoCharacterSkillData_Buff.AttackMultiplierBuffItem.Clone());
                compAttackerStat.AddBuffStat(m_SoCharacterSkillData_Buff.AttackSpeedBuffItem.Clone());
                Debug.Log($"{compAttackerStat.name}'s current Attack Damage: {compAttackerStat.GetAttackDamage()}");
                Debug.Log($"{compAttackerStat.name}'s current Attack Interval: {compAttackerStat.GetAttackInterval()}");
            }
            ResetCoolDown();
        }


        IEnumerator CR_AttackTimer()
        {
            yield return new WaitForSeconds(Mathf.Max(m_SoCharacterSkillData_Buff.AttackMultiplierBuffItem.RemainingTime, m_SoCharacterSkillData_Buff.AttackSpeedBuffItem.RemainingTime));
            m_UniqueSkillGO.SetActive(false);
        }
    }
}