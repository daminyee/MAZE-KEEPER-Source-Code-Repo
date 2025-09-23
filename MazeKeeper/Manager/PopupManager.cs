using System;
using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.ComponentGUI;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;


namespace MazeKeeper.Manager
{
    public class PopupManager : ManagerBase<PopupManager>
    {
        [SerializeField] CompGUIPopup_Confirm   _popupConfirm;
        [SerializeField] CompGUIPopup_Setting   _popupSetting;
        [SerializeField] CompGUIPopup_PauseMenu _pauseMenu;
        [SerializeField] Canvas                 _canvas;


        void Start()
        {
            HideAllPopup();
            SceneLoadManager.Instance.OnSceneChanged += () =>
                                                        {
                                                            HideAllPopup();
                                                            FindNewUIPopupCamera();
                                                        };
        }


        void FindNewUIPopupCamera()
        {
            var popupCamera = GameObject.FindGameObjectWithTag("UIPopupCamera");
            Debug.Assert(popupCamera != null);
            _canvas.worldCamera = popupCamera.GetComponent<Camera>();
        }


        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                OnEscapeKeyDown();
            }
        }


        public void OnEscapeKeyDown()
        {
            if (GetOpenedPopupCount() == 0)
            {
                if (PauseManager.Instance != null)
                {
                    var stageManager = GameManager.Instance;
                    if (stageManager != null)
                    {
                        _pauseMenu.Show();
                    }
                }
                return;
            }

            HideAllPopup();
        }


        public void HideAllPopup()
        {
            _popupConfirm.Hide();
            _pauseMenu.Hide();
            _popupSetting.Hide();
        }


        public void ShowPopup(string message, Action OnConfirmAction, Action OnCancelAction)
        {
            StartCoroutine(CR_ShowPopup(message, OnConfirmAction, OnCancelAction));
        }


        IEnumerator CR_ShowPopup(string message, Action OnConfirmAction, Action OnCancelAction)
        {
            _popupConfirm.Show(message);

            //입력받을 때까지 기다리기
            yield return new WaitUntil(() => _popupConfirm.IsConfirmed || _popupConfirm.IsCanceled);

            if (_popupConfirm.IsConfirmed)
            {
                OnConfirmAction?.Invoke();
            }
            else
            {
                OnCancelAction?.Invoke();
            }

            _popupConfirm.Hide();
        }


        public void ShowSettingPop()
        {
            _popupSetting.Show();
        }


        public void ShowPauseMenu()
        {
            _pauseMenu.Show();
        }


        public void HidePauseMenu()
        {
            _pauseMenu.Hide();
        }


        public int GetOpenedPopupCount()
        {
            var openedCount = 0;
            if (_popupConfirm.gameObject.activeSelf) openedCount++;
            if (_pauseMenu.gameObject.activeSelf) openedCount++;
            if (_popupSetting.gameObject.activeSelf) openedCount++;
            return openedCount;
        }
    }
}