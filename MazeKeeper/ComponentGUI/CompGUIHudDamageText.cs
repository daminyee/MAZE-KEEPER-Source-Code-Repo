using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudDamageText : MonoBehaviour
    {
        [SerializeField] CompWorldToUI _compWorldToUI;

        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private float           _floatSpeed = 80f;

        RectTransform m_RectTransform;

        PlayablePlayer m_PlayablePlayer;

        bool m_IsPlaying;


        private void OnEnable()
        {
            m_IsPlaying = false;
        }


        public void Init(Transform parentTransform, Transform followTransform, float damageAmount, Vector3? direction = null)
        {
            m_RectTransform = transform as RectTransform;

            _compWorldToUI.Init(parentTransform, followTransform);

            _damageText.text = damageAmount.ToString("F1");

            m_IsPlaying = true;

            m_PlayablePlayer = new(gameObject);
            m_PlayablePlayer.Play(direction);
        }


        void LateUpdate()
        {
            if (m_IsPlaying == false) return;

            m_RectTransform.anchoredPosition += Vector2.up * _floatSpeed * Time.deltaTime;
        }
    }
}