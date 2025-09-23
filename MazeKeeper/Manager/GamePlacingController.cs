using System;
using System.Collections;
using MazeKeeper.Component;
using MazeKeeper.ComponentGUI;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace MazeKeeper.Manager
{
    public class GamePlacingController : MonoBehaviour
    {
        public enum PlaceMode
        {
            NormalMode,
            PlacingMode,
            SelectedMode,
            DeleteMode,
            UpgradeMode,
        }


        public CompCellObject PlacingCellObject => m_PlacingCellObject;

        public event Action OnCellObjectUpdated;

        [SerializeField] Button     _changeDeleteModeButton;
        [SerializeField] GameObject _deleteButtonSelectIndicator;

        SoCellObjectData         m_PlacingSoCellObjectData;
        CompGUICellObjectBuyInfo m_PlacingCompGUICellObjectBuyInfo;
        CompCellObject           m_PlacingCellObject;
        CompCellObject_Turret    m_SelectedCellObjectTurret;
        Camera                   m_MainCam;
        PlaceMode                m_PlaceMode;
        Vector2Int               m_PreviousMouseCellPos;
        Vector2Int               m_CurrentMouseCellPos;
        Vector2Int               m_LastPlacedMouseCellPos; // 마지막으로 배치한 위치
        bool                     m_IsValidPath;


        void Awake()
        {
            m_MainCam = Camera.main;
            _changeDeleteModeButton.onClick.AddListener(OnClickedDeleteModeButton);
            GameManager.Instance.OnModeChanged += (previousMode, currentMode) =>
                                                  {
                                                      if (previousMode == GameModeType.PlacingMode)
                                                      {
                                                          ChangePlaceMode(PlaceMode.NormalMode);
                                                      }
                                                  };
        }


        void Update()
        {
            if (GameManager.Instance.CurrentGameMode != GameModeType.PlacingMode) return;
            
            // 현재 PlaceMode에 따라 Update함수 실행
            switch (m_PlaceMode)
            {
                case PlaceMode.NormalMode:
                case PlaceMode.SelectedMode:
                    UpdateNormalModeOrSelectMode();
                    break;
                case PlaceMode.PlacingMode:
                    UpdatePlacingMode();
                    break;
                case PlaceMode.DeleteMode:
                    UpdateDeleteMode();
                    break;
                case PlaceMode.UpgradeMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void ChangePlaceMode(PlaceMode placeMode, CompGUICellObjectBuyInfo compGUICellObjectBuyInfo = null)
        {
            Debug.Log($"Previous PlaceMode : {m_PlaceMode}, New PlaceMode : {placeMode}, CompGUICellObjectBuyInfo : {compGUICellObjectBuyInfo}");

            // 이전 모드에 따라서 정리가 필요한 것들이 있다면 정리
            switch (m_PlaceMode)
            {
                case PlaceMode.PlacingMode:
                    if (m_PlacingCellObject != null)
                    {
                        Destroy(m_PlacingCellObject.gameObject);
                        if (m_PlacingSoCellObjectData.BlockPath)
                        {
                            StartCoroutine(CR_BakeNavSurface(true));
                        }
                        m_PlacingCellObject       = null;
                        m_PlacingSoCellObjectData = null;
                    }
                    if (m_PlacingCompGUICellObjectBuyInfo != null)
                    {
                        m_PlacingCompGUICellObjectBuyInfo.ShowSelectedButtonIndicator(false);
                    }
                    break;
                case PlaceMode.SelectedMode:
                    m_SelectedCellObjectTurret.SetRangeShow(false);
                    break;
                case PlaceMode.DeleteMode:
                    _deleteButtonSelectIndicator.SetActive(false);
                    break;
            }

            // 새로운 모드 적용
            m_PlaceMode = placeMode;
            switch (m_PlaceMode)
            {
                case PlaceMode.NormalMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Normal);
                    BattleUIManager.Instance.ShowWaveStartButton(true);
                    _deleteButtonSelectIndicator.SetActive(false);
                    break;
                case PlaceMode.PlacingMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Place);
                    BattleUIManager.Instance.ShowWaveStartButton(false);

                    m_PlacingCompGUICellObjectBuyInfo = compGUICellObjectBuyInfo;
                    Debug.Assert(m_PlacingCompGUICellObjectBuyInfo != null);

                    m_PlacingCompGUICellObjectBuyInfo.ShowSelectedButtonIndicator(true);
                    m_PlacingSoCellObjectData = m_PlacingCompGUICellObjectBuyInfo.SoCellObjectData;

                    var resultWorldPos = GameManager.Instance.GameMazeController.GetCellPosition(m_LastPlacedMouseCellPos.x, m_LastPlacedMouseCellPos.y);
                    var cellObject     = Instantiate(m_PlacingSoCellObjectData.CompCellObject, new(resultWorldPos.x, 0f, resultWorldPos.z), Quaternion.identity);
                    cellObject.Init(m_PlacingSoCellObjectData);

                    m_PlacingCellObject = cellObject;
                    m_PlacingCellObject.SetPlacingMode(true);
                    m_PlacingCellObject.Play_Placing();
                    m_PreviousMouseCellPos = new(int.MaxValue, int.MaxValue);
                    break;
                case PlaceMode.SelectedMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Normal);
                    m_SelectedCellObjectTurret.SetRangeShow(true);
                    break;
                case PlaceMode.DeleteMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Delete);
                    _deleteButtonSelectIndicator.SetActive(true);
                    break;
                case PlaceMode.UpgradeMode:
                    ImageCursorManager.Instance.SetImageCursor(ImageCursorManager.ImageCursorType.Normal);
                    break;
            }
        }


        void UpdateNormalModeOrSelectMode()
        {
            // 포탑 클릭 시, 사거리 표시
            if (Input.GetMouseButtonDown(0))
            {
                if (m_SelectedCellObjectTurret != null)
                {
                    m_SelectedCellObjectTurret.SetRangeShow(false);
                }

                var       ray                = m_MainCam.ScreenPointToRay(Input.mousePosition);
                const int PlacingRayHitLayer = 1 << GameMazeController.PlacingRayHit;
                if (Physics.Raycast(ray, out var hit, 1000f, PlacingRayHitLayer))
                {
                    var cellObject = GameManager.Instance.GameMazeController.GetCellObjectFromWorldPosition(hit.point);
                    if (cellObject.IsValid)
                    {
                        if (cellObject.CellObject is CompCellObject_Turret turret)
                        {
                            m_SelectedCellObjectTurret = turret;
                            ChangePlaceMode(PlaceMode.SelectedMode);
                        }
                        else
                        {
                            ChangePlaceMode(PlaceMode.NormalMode);
                        }
                    }
                }
            }
        }


        void UpdatePlacingMode()
        {
            const int PlacingRayHitLayer    = 1 << GameMazeController.PlacingRayHit;
            var       gameManager           = GameManager.Instance;
            var       ray                   = m_MainCam.ScreenPointToRay(Input.mousePosition);
            var       isValidGround         = false;
            var       worldPosY             = 0f;
            var       isCurrentCellPosEmpty = true;
            var       canPlaceWallOrFloor   = false;

            // 터렛 설치 중에 터렛이 마우스 따라오는 처리
            if (Physics.Raycast(ray, out var hit, 1000f, PlacingRayHitLayer))
            {
                var result = gameManager.GameMazeController.GetCellPosFromWorldPosition(hit.point);

                if (result.IsValid)
                {
                    isValidGround = true;
                    var isWall = gameManager.GameMazeController.CheckCell(result.CellPos);
                    // Wall에 배치 가능하다면
                    if (isWall && m_PlacingSoCellObjectData.CanPlaceWall)
                    {
                        canPlaceWallOrFloor =  true;
                        worldPosY           += 1f;
                    }
                    // Floor에 배치 가능하다면
                    else if (!isWall && m_PlacingSoCellObjectData.CanPlaceFloor)
                    {
                        canPlaceWallOrFloor = true;
                        worldPosY           = 0f;
                    }
                    else
                    {
                        ToastManager.Instance.ToastWarning("해당 위치에 배치할 수 없습니다.", 0.1f);
                    }

                    var resultWorldPos = gameManager.GameMazeController.GetCellPosition(result.CellPos.x, result.CellPos.y);
                    m_PlacingCellObject.transform.position = new(resultWorldPos.x, worldPosY, resultWorldPos.z);

                    m_PreviousMouseCellPos = m_CurrentMouseCellPos;
                    m_CurrentMouseCellPos  = result.CellPos;
                }
                // 위치가 이상한 경우
                else
                {
                    ToastManager.Instance.ToastWarning("해당 위치에 배치할 수 없습니다.", 0.1f);
                }
            }

            // 핸재 CellPos에 CellObject가 있다면 배치 못하도록
            if (gameManager.GameMazeController.GetCellObject(m_CurrentMouseCellPos) != null)
            {
                isCurrentCellPosEmpty = false;
                // 마지막으로 배치한 위치가 아니라면 경고표시
                // 마지막으로 배치한 위치에서는 필수적으로 경고가 뜰수 밖에 없는데,
                // 정상적인 과정에서 계속 경고가 뜨는 것이 좋지 않다고 판단되어 경고 표시 안함
                if (m_LastPlacedMouseCellPos != m_CurrentMouseCellPos)
                {
                    ToastManager.Instance.ToastWarning("해당 위치에 배치할 수 없습니다.", 0.1f);
                }
            }


            // 경로 갱신
            // 터렛 설치 중에 터렛 설치 위치에 따라 바뀔 경로를 보여주는 처리
            if (m_PreviousMouseCellPos != m_CurrentMouseCellPos)
            {
                if (m_PlacingSoCellObjectData.BlockPath)
                {
                    gameManager.BakeNavSurface(false);
                    m_IsValidPath = gameManager.GamePathController.InitPath();
                    if (!m_IsValidPath) ToastManager.Instance.ToastWarning("길을 막을 수 없습니다.", 2f);
                }
                else
                {
                    m_IsValidPath = true;
                }
            }

            var isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

            // CellObject 배치
            // 배치가 가능한 상황에 마우스 왼쪽 클릭시 배치
            if (Input.GetMouseButtonDown(0) && isValidGround && !isPointerOverUI && m_IsValidPath && isCurrentCellPosEmpty && canPlaceWallOrFloor)
            {
                if (GameManager.Instance.GameMazeController.PlacedCellObjectCountList[(int)m_PlacingCellObject.SoCellObjectData.CellObjectType] >= m_PlacingCellObject.SoCellObjectData.PlaceLimit)
                {
                    ToastManager.Instance.ToastWarning("최대 설치 갯수에 도달했습니다.");
                    return;
                }
                if (!PlayerDataManager.Instance.TryConsumeGold(m_PlacingCellObject.SoCellObjectData.PlacePrice)) return;

                GameManager.Instance.GameMazeController.PlacedCellObjectCountList[(int)m_PlacingCellObject.SoCellObjectData.CellObjectType]++;

                OnCellObjectUpdated?.Invoke();

                m_PlacingCellObject.SetPlacingMode(false);
                m_PlacingCellObject.Play_Placed();
                gameManager.GameMazeController.SetCellObject(m_CurrentMouseCellPos, m_PlacingCellObject);
                BattleUIManager.Instance.CreateCellObjectPointToMiniMap(m_PlacingCellObject, m_CurrentMouseCellPos);

                if (m_PlacingCellObject is CompCellObject_Turret placedTurret)
                {
                    placedTurret.SetRangeShow(false);
                }

                m_PlacingCellObject       = null;
                m_PlacingSoCellObjectData = null;
                m_LastPlacedMouseCellPos  = m_CurrentMouseCellPos;

                ChangePlaceMode(PlaceMode.PlacingMode, m_PlacingCompGUICellObjectBuyInfo);
            }

            // Cancel 처리
            if (Input.GetMouseButtonDown(1))
            {
                ChangePlaceMode(PlaceMode.NormalMode);
            }
        }


        void UpdateDeleteMode()
        {
            // 배치물 클릭 시, 삭제 후 골드 반환

            var gameManager = GameManager.Instance;
            var mousePos    = Input.mousePosition;
            var ray         = m_MainCam.ScreenPointToRay(mousePos);
            var layer       = 1 << GameMazeController.PlacingRayHit;

            if (Physics.Raycast(ray, out var hit, 1000f, layer))
            {
                var result = gameManager.GameMazeController.GetCellPosFromWorldPosition(hit.point);

                if (result.IsValid)
                {
                    m_PreviousMouseCellPos = m_CurrentMouseCellPos;
                    m_CurrentMouseCellPos  = result.CellPos;
                }
            }

            var isPointerOverUI       = EventSystem.current.IsPointerOverGameObject();
            var isCurrentCellPosEmpty = gameManager.GameMazeController.GetCellObject(m_CurrentMouseCellPos) == null;
            if (Input.GetMouseButtonDown(0) && !isPointerOverUI && !isCurrentCellPosEmpty)
            {
                var cellObject = gameManager.GameMazeController.GetCellObject(m_CurrentMouseCellPos);

                PlayerDataManager.Instance.AddGold(cellObject.SoCellObjectData.PlacePrice);

                Destroy(cellObject.gameObject);
                gameManager.GameMazeController.PlacedCellObjectCountList[(int)cellObject.SoCellObjectData.CellObjectType]--;
                gameManager.GameMazeController.SetCellObject(m_CurrentMouseCellPos, null);
                BattleUIManager.Instance.DeleteCellObjectPointToMiniMap(m_CurrentMouseCellPos);

                OnCellObjectUpdated?.Invoke();
                StartCoroutine(CR_BakeNavSurface(false));
            }


            if (Input.GetMouseButtonDown(1))
            {
                OnClickedDeleteModeButton();
            }
        }


        void OnClickedDeleteModeButton()
        {
            if (m_PlaceMode != PlaceMode.DeleteMode)
            {
                ChangePlaceMode(PlaceMode.DeleteMode);
            }
            else
            {
                ChangePlaceMode(PlaceMode.NormalMode);
            }
        }


        IEnumerator CR_BakeNavSurface(bool isBakeEnemyPath)
        {
            yield return null; // 한 프레임 쉰다. 게임오브젝트를 Destroy한 경우에 한프레임이 지나야 정상적으로 처리가 가능하다.

            GameManager.Instance.BakeNavSurface(isBakeEnemyPath);
            GameManager.Instance.GamePathController.InitPath();
        }
    }
}
