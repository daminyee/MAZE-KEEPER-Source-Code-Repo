using System.Collections.Generic;
using MazeKeeper.Define;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.Splines;


namespace MazeKeeper.Manager
{
    public class GamePathController : MonoBehaviour
    {
        public Vector3 EnemyPathStartPos { get; private set; }
        public Vector3 EnemyPathEndPos   { get; private set; }

        [SerializeField] NavMeshSurface        _enemyNavSurface;
        [SerializeField] NavMeshSurface        _arrowNavSurface;
        [SerializeField] List<SplineContainer> _previewSplineList;

        [FormerlySerializedAs("_previewMat")]
        [SerializeField] Material _previewMatPlaceMode;
        [SerializeField] Material _previewMatFail;
        [SerializeField] Material _previewMatInGame;


        NavMeshQueryFilter m_PreviewNavQueryFilter = new NavMeshQueryFilter();

        GameMazeController m_GameMazeController;


        public void Init()
        {
            m_PreviewNavQueryFilter.agentTypeID = NavMesh.GetSettingsByIndex(1).agentTypeID;
            m_PreviewNavQueryFilter.areaMask    = NavMesh.GetAreaFromName("Everything");

            m_GameMazeController = GetComponent<GameMazeController>();

            // 바닥 높이와 시작점, 끝점 높이 맞춰주기.
            EnemyPathStartPos = m_GameMazeController.TopLeft_ExcludeBorder;
            EnemyPathEndPos   = m_GameMazeController.BottomRight_ExcludeBorder;

            BakeNavSurface(true);

            InitPath();
        }


        /// <summary>
        /// 3개의 Path를 Refresh한다.
        /// 하나라도 막힌 Path가 존재한다면 false 반환.
        /// </summary>
        public bool InitPath()
        {
            var isAllPathValid = true;
            for (int i = 0; i < (int)GateType.Length; i++)
            {
                var isValid = RefreshPathIndicator(m_GameMazeController.GetGatePosition((GateType)i), m_GameMazeController.GetGoalPosition(), _previewSplineList[i]);
                if (isValid == false)
                {
                    isAllPathValid = false;
                }
            }
            return isAllPathValid;
        }


        public void ShowEnemyPath()
        {
            for (int i = 0; i < (int)GateType.Length; i++)
            {
                _previewSplineList[i].gameObject.SetActive(true);
            }
        }


        public void HideEnemyPath()
        {
            for (int i = 0; i < (int)GateType.Length; i++)
            {
                _previewSplineList[i].gameObject.SetActive(false);
            }
        }


        public bool IsValidPath()
        {
            NavMeshPath path     = new NavMeshPath();
            var         startPos = EnemyPathStartPos;
            var         endPos   = EnemyPathEndPos;
            var         result   = NavMesh.CalculatePath(startPos, endPos, m_PreviewNavQueryFilter, path);

            if (path.corners.Length > 0)
            {
                endPos.y = path.corners[^1].y;
            }

            return result && path.corners.Length > 0 && endPos == path.corners[^1];
        }


        public void BakeNavSurface(bool bakeWithEnemySurface)
        {
            Debug.Log($"BakeNavSurface {bakeWithEnemySurface}");
            if (bakeWithEnemySurface)
            {
                _enemyNavSurface.BuildNavMesh();
            }
            _arrowNavSurface.BuildNavMesh();
        }


        public void ChangeInGamePathMaterial()
        {
            for (int i = 0; i < _previewSplineList.Count; i++)
            {
                _previewSplineList[i].GetComponent<MeshRenderer>().material = _previewMatInGame;
            }
        }


        /// <summary>
        /// Path를 표시하는 Spline을 Refresh한다.
        /// 경로가 막혔다면 false를 반환.
        /// </summary>
        bool RefreshPathIndicator(Vector3 startPos, Vector3 endPos, SplineContainer splineContainer)
        {
            const float SplineHeightFromGround = 0.1f;

            NavMeshPath previewPath = new NavMeshPath();
            var         result      = NavMesh.CalculatePath(startPos, endPos, m_PreviewNavQueryFilter, previewPath);

            if (result == false) Debug.LogError($"Path {startPos} does not exist");

            // path의 마지막점이 endPos와 다르면 길막된 상태. 따라서 false반환
            splineContainer.GetComponent<MeshRenderer>().material = GameManager.Instance.CurrentGameMode == GameModeType.PlacingMode ? _previewMatPlaceMode : _previewMatInGame;


            // [주의] splineContainer.Spline 객체에 Knot의 Add를 한번 할때마다 무거운 연산이 발생하므로,
            // Spline 객체를 새로 생하여 해당 객체에 Add를 여러번 수행, 그리고 마지막에 splineContainer.Spline에 한번만 할당한다.
            Spline newSpline = new Spline();

            for (int i = 0; i < previewPath.corners.Length; i++)
            {
                var corner = previewPath.corners[i];

                // 첫번째, 마지막 점이라면,
                bool isFirstKnotOrEndKnot = (i == 0 || i == previewPath.corners.Length - 1);
                // 0.3보다 두개의 점이 가까이 있다.
                bool isKnotClose = i != 0 && Vector3.Distance(corner, previewPath.corners[i - 1]) < 0.3f;
                if (isFirstKnotOrEndKnot == false && isKnotClose)
                {
                    // 이번에 추가할 점이, 이전점과의 거리가 0.3 이하라면 추가하지않고,
                    // 이전점을 이전점과 지금 점의 평균으로 위치변경한다.
                    var knot = newSpline[^1];
                    knot.Position   = (corner + previewPath.corners[i - 1]) * 0.5f;
                    knot.Position.y = SplineHeightFromGround;
                    newSpline[^1]   = knot;
                    continue;
                }
                newSpline.Add(new Vector3(corner.x, SplineHeightFromGround, corner.z), TangentMode.AutoSmooth);

                if (i == previewPath.corners.Length - 1 && Vector3.Distance(corner, endPos) > 0.1f)
                {
                    splineContainer.GetComponent<MeshRenderer>().material = _previewMatFail;
                    return false;
                }
            }
            // splineContainer.Spline에는 마지막에 한번만 할당한다.
            splineContainer.Spline = newSpline;
            // SplineExtrude 강제 Rebuild (splineContainer.Spline을 직접적으로 할당했기때문에 SplineExtrude가 Refresh되지 않는다.) 
            splineContainer.GetComponent<SplineExtrude>().Rebuild();
            return true;
        }
    }
}
