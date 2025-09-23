using System;
using MazeKeeper.Class;
using UnityEngine;


namespace MazeKeeper.Manager
{
    public class ImageCursorManager : ManagerBase<ImageCursorManager>
    {
        public enum ImageCursorType
        {
            Normal,
            Attack,
            Place,
            Delete,
        }


        [SerializeField] RectTransform _cursorImageRect;
        [SerializeField] Canvas        _canvas;
        [SerializeField] GameObject    _cursorImage_Normal;
        [SerializeField] GameObject    _cursorImage_Attack;
        [SerializeField] GameObject    _cursorImage_Build;
        [SerializeField] GameObject    _cursorImage_Delete;

        RectTransform   m_CanvasRectTransform;
        Camera          m_Camera;
        PlayablePlayer  m_PlayablePlayer;
        ImageCursorType m_ImageCursorType;


        void Start()
        {
            SceneLoadManager.Instance.OnSceneChanged += FindNewUIPopupCamera;

            m_CanvasRectTransform = _canvas.GetComponent<RectTransform>();
            m_PlayablePlayer      = new(_cursorImageRect.gameObject);
            Cursor.visible        = false;

            FindNewUIPopupCamera();
        }


        void FindNewUIPopupCamera()
        {
            var popupCamera = GameObject.FindGameObjectWithTag("UIPopupCamera");
            Debug.Assert(popupCamera != null);
            m_Camera            = popupCamera.GetComponent<Camera>();
            _canvas.worldCamera = m_Camera;
        }


        void Update()
        {
            if (m_Camera == null || Cursor.lockState != CursorLockMode.None)
            {
                _cursorImageRect.gameObject.SetActive(false);
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                m_PlayablePlayer.Play();
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_CanvasRectTransform,
                Input.mousePosition,
                m_Camera,
                out Vector2 localPoint
            );
            _cursorImageRect.anchoredPosition = localPoint;

            if (_cursorImageRect.gameObject.activeSelf == false)
            {
                _cursorImageRect.gameObject.SetActive(true);
            }
        }


        public void SetImageCursor(ImageCursorType imageCursorType)
        {
            if (m_ImageCursorType == imageCursorType) return;
            m_ImageCursorType = imageCursorType;
            switch (imageCursorType)
            {
                case ImageCursorType.Normal:
                    _cursorImage_Normal.gameObject.SetActive(true);
                    _cursorImage_Attack.gameObject.SetActive(false);
                    _cursorImage_Build.gameObject.SetActive(false);
                    _cursorImage_Delete.gameObject.SetActive(false);
                    break;
                case ImageCursorType.Attack:
                    _cursorImage_Normal.gameObject.SetActive(false);
                    _cursorImage_Attack.gameObject.SetActive(true);
                    _cursorImage_Build.gameObject.SetActive(false);
                    _cursorImage_Delete.gameObject.SetActive(false);
                    break;
                case ImageCursorType.Place:
                    _cursorImage_Normal.gameObject.SetActive(false);
                    _cursorImage_Attack.gameObject.SetActive(false);
                    _cursorImage_Build.gameObject.SetActive(true);
                    _cursorImage_Delete.gameObject.SetActive(false);
                    break;
                case ImageCursorType.Delete:
                    _cursorImage_Normal.gameObject.SetActive(false);
                    _cursorImage_Attack.gameObject.SetActive(false);
                    _cursorImage_Build.gameObject.SetActive(false);
                    _cursorImage_Delete.gameObject.SetActive(true);
                    break;
            }
        }
    }
}