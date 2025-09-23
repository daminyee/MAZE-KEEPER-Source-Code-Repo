using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Component;
using MazeKeeper.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudCharacterSkillIcon : MonoBehaviour
    {
        [SerializeField] Image    _skillImage;
        [SerializeField] Image    _skillBackGround;
        [SerializeField] TMP_Text _skillRemainingTime;


        CompCharacterSkillAttacker m_CompCharacterSkillAttacker;
        bool                       m_IsInitialized;

        PlayablePlayer m_PlayablePlayer;


        public void Init(CompCharacterSkillAttacker compCharacterSkillAttacker)
        {
            m_CompCharacterSkillAttacker                =  compCharacterSkillAttacker;
            m_CompCharacterSkillAttacker.OnCoolDownDone += PlayCoolDownDoneEffect;
            _skillImage.sprite                          =  m_CompCharacterSkillAttacker.Icon;
            _skillBackGround.sprite                     =  m_CompCharacterSkillAttacker.Icon;
            m_IsInitialized                             =  true;
            m_PlayablePlayer                            =  new(gameObject);
        }


        void Update()
        {
            if (m_IsInitialized == false) return;

            var skillRemainingTime = m_CompCharacterSkillAttacker.CoolTime - m_CompCharacterSkillAttacker.ElapsedTime;
            if (skillRemainingTime > 0)
            {
                _skillRemainingTime.text = skillRemainingTime.ToString("F1");

                if (!_skillRemainingTime.gameObject.activeSelf)
                {
                    _skillRemainingTime.gameObject.SetActive(true);
                }
            }
            else
            {
                _skillRemainingTime.gameObject.SetActive(false);
            }
            _skillImage.fillAmount = m_CompCharacterSkillAttacker.GetCoolDownPercent();
        }


        void PlayCoolDownDoneEffect() => m_PlayablePlayer.Play();
    }
}