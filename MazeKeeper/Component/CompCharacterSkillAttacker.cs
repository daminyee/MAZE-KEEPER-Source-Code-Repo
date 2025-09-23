using System;
using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.Component
{
    public abstract class CompCharacterSkillAttacker : MonoBehaviour
    {
        public Sprite             Icon         => m_Icon;
        public CharacterSkillType SkillType    => _skillType;
        public float              SplashRadius => m_SplashRadius;
        public float              CoolTime     => m_CoolTime;
        public float              ElapsedTime  => m_ElapsedTime;

        public event Action OnCoolDownDone;

        [SerializeField] CharacterSkillType _skillType;

        protected SoCharacterSkillData SoCharacterSkillData => m_SoCharacterSkillData;
        SoCharacterSkillData           m_SoCharacterSkillData;

        Sprite m_Icon;
        float  m_CoolTime;
        float  m_SplashRadius;
        float  m_ElapsedTime;


        protected virtual void Awake()
        {
            m_SoCharacterSkillData = PlayerDataManager.Instance.PlayerData.CurrentSoCharacterData.GetCurrentScoCharacterSkillData(_skillType);
            m_Icon                 = m_SoCharacterSkillData.SkillIcon;
            m_CoolTime             = m_SoCharacterSkillData.CoolTime;
            m_SplashRadius         = m_SoCharacterSkillData.SplashRadius;
            m_ElapsedTime          = m_CoolTime;
        }


        void Update()
        {
            if (m_ElapsedTime == m_CoolTime) return;

            m_ElapsedTime += Time.deltaTime;
            if (m_ElapsedTime >= m_CoolTime)
            {
                m_ElapsedTime = m_CoolTime;
                OnCoolDownDone?.Invoke();
            }
        }


        public float GetCoolDownPercent()
        {
            return m_ElapsedTime / m_CoolTime;
        }


        public bool IsCoolDownToUseSkill()
        {
            var isCoolDownDone = m_ElapsedTime >= m_CoolTime;
            if (!isCoolDownDone)
            {
                ToastManager.Instance.ToastWarning("아직 스킬을 사용할 준비가 되지 않았습니다!");
            }

            return isCoolDownDone;
        }


        public void ResetCoolDown() => m_ElapsedTime = 0f;


        public abstract void UseSkill(Vector3 attackCenterPosition);
    }
}