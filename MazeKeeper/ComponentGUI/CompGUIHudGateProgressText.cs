using MazeKeeper.Component;
using TMPro;
using UnityEngine;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudGateProgressText : MonoBehaviour
    {
        [SerializeField] CompWorldToUI _compWorldToUI;

        [SerializeField] private TextMeshProUGUI _timerText;

        RectTransform m_RectTransform;


        public void Init(Transform parentTransform, Transform followTransform)
        {
            m_RectTransform = transform as RectTransform;

            _compWorldToUI.Init(parentTransform, followTransform);
        }


        public void SetProgressPercent(float currentEnemy, float totalEnemy)
        {
            _timerText.text = $"{(int)currentEnemy} / {(int)totalEnemy}";
        }
    }
}