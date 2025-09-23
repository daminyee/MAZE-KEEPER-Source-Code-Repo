using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudHpBar : MonoBehaviour
    {
        [SerializeField] Image    _hpBarMask;
        [SerializeField] Image    _hpBar;
        [SerializeField] Image    _hpBarCoverMask;
        [SerializeField] Gradient _hpColor;

        [SerializeField] CompWorldToUI _compWorldToUI;


        PlayablePlayer m_PlayablePlayer;


        public void Init(Transform parentTransform, Transform followTransform)
        {
            const float DefaultPercent = 1;
            _hpBarMask.fillAmount      = DefaultPercent;
            _hpBarCoverMask.fillAmount = DefaultPercent;
            _hpBar.color               = _hpColor.Evaluate(DefaultPercent);

            _compWorldToUI.Init(parentTransform, followTransform);

            m_PlayablePlayer = new(gameObject);
        }


        public void SetHpPercent(float percent, Vector3? direction = null)
        {
            _hpBarMask.fillAmount      = percent;
            _hpBarCoverMask.fillAmount = percent;
            _hpBar.color               = _hpColor.Evaluate(percent);

            m_PlayablePlayer.Play(direction);
        }
    }
}