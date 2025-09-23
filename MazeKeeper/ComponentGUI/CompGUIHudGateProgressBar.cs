using MazeKeeper.Component;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudGateProgressBar : MonoBehaviour
    {
        [SerializeField] Image    _progressBar;
        [SerializeField] float    _progressBarMaxFillAmount = 0.5f;
        [SerializeField] Gradient _barColor;

        [SerializeField] CompWorldToUI _compWorldToUI;


        public void Init(Transform parentTransform, Transform followTransform)
        {
            _progressBar.fillAmount = _progressBarMaxFillAmount;
            _progressBar.color      = _barColor.Evaluate(0f);

            _compWorldToUI.Init(parentTransform, followTransform);
        }


        public void SetProgressPercent(float percent)
        {
            var lerpPercent = Mathf.Lerp(0f, _progressBarMaxFillAmount, percent);
            _progressBar.fillAmount = lerpPercent;
            _progressBar.color      = _barColor.Evaluate(percent);
        }
    }
}