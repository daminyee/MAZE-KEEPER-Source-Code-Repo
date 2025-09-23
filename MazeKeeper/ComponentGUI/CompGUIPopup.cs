using UnityEngine;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIPopup : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }


        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}