using MazeKeeper.Component;
using MazeKeeper.ComponentGUI;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class ToastManager : ManagerBase<ToastManager>
    {
        [SerializeField] Canvas _canvas;

        [SerializeField] CompGUIToastWarning _toastWarning;
        [SerializeField] CompGUIToastSkill   _toastSkill;
        [SerializeField] CompGUIToastTitle   _toastTitle;
        [SerializeField] CompGUIToastMessage _toastMessage;
        [SerializeField] CompGUIToastBoss    _toastBoss;


        void Start()
        {
            SceneLoadManager.Instance.OnSceneChanged += FindNewUIPopupCamera;
            _toastWarning.gameObject.SetActive(false);
            _toastSkill.gameObject.SetActive(false);
            _toastTitle.gameObject.SetActive(false);
            _toastMessage.gameObject.SetActive(false);
        }


        void FindNewUIPopupCamera()
        {
            var popupCamera = GameObject.FindGameObjectWithTag("UIPopupCamera");
            Debug.Assert(popupCamera != null);
            _canvas.worldCamera = popupCamera.GetComponent<Camera>();
        }


        public void ToastWarning(string message, float lifeTime = CompGUIToastWarning.DefaultLifeTime)
        {
            _toastWarning.Show(message, lifeTime);
        }


        public void ToastSkill(string message)
        {
            _toastSkill.Show(message);
        }


        public void ToastTitle(string title, string message = "", float lifeTime = CompGUIToastTitle.DefaultLifeTime)
        {
            _toastTitle.Show(title, message, lifeTime);
        }


        public void ToastMessage(string message, float lifeTime = CompGUIToastTitle.DefaultLifeTime)
        {
            _toastMessage.Show(message, lifeTime);
        }


        public void ToastBoss(string message = "", float lifeTime = CompGUIToastTitle.DefaultLifeTime)
        {
            _toastBoss.Show(message, lifeTime);
        }
    }
}