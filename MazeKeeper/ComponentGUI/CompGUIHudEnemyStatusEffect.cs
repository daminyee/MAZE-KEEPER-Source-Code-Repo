using System;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudEnemyStatusEffect : MonoBehaviour
    {
        [SerializeField] List<Image> _statusEffectIconList;

        [SerializeField] CompWorldToUI _compWorldToUI;

        CompEnemy m_CompEnemy;

        bool m_IsInitialized;


        public void Init(CompEnemy compEnemy, Transform parentTransform, Transform followTransform)
        {
            m_CompEnemy = compEnemy;

            _compWorldToUI.Init(parentTransform, followTransform);
            foreach (var item in _statusEffectIconList)
            {
                item.gameObject.SetActive(false);
            }
            compEnemy.EnemyStatusEffect.OnStatusEffectTypeChanged += RefreshAppliedStatusEffect;

            m_IsInitialized = true;
        }


        public void OnDisable()
        {
            if (m_IsInitialized)
            {
                m_CompEnemy.EnemyStatusEffect.OnStatusEffectTypeChanged -= RefreshAppliedStatusEffect;
                m_IsInitialized                                         =  false;
            }
        }


        void RefreshAppliedStatusEffect(List<(EnemyStatusEffectType Type, bool CurrentState)> changedStatusEffectList)
        {
            foreach (var appliedStatusEffect in changedStatusEffectList)
            {
                var go = _statusEffectIconList[(int)appliedStatusEffect.Type].gameObject;
                if (go.activeSelf != appliedStatusEffect.CurrentState)
                {
                    go.SetActive(appliedStatusEffect.CurrentState);
                }
            }
        }
    }
}