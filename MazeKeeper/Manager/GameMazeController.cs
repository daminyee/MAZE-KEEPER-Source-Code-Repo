using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace MazeKeeper.Manager
{
    public class GameMazeController : MonoBehaviour
    {
        enum MazeDirection
        {
            Up_Down,
            Left_Right,
            Up_Left,
            Up_Right,
            Down_Left,
            Down_Right,
            None,
        }


        enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            None,
        }


        public const int NavMeshBakeLayer = 6;
        public const int PlacingRayHit    = 7;

        const float CellWidth = 1f;


        public class BlockLineInfo
        {
            public Vector2Int CurCell;
            public bool       IsBlocked;
        }


        public int CellCountWithBorder => _cellCount * 3 + 2;

        public bool[,]   Cell_Array                => m_Cell_Array;
        public List<int> PlacedCellObjectCountList => _placedCellObjectCountList;

        public Vector3 TopLeft_ExcludeBorder     => GetCellPosition(1, 1);
        public Vector3 TopRight_ExcludeBorder    => GetCellPosition((_cellCount * 3 - 1) + (int)CellWidth, 1);
        public Vector3 BottomLeft_ExcludeBorder  => GetCellPosition(1, (_cellCount * 3 - 1) + (int)CellWidth);
        public Vector3 BottomRight_ExcludeBorder => GetCellPosition((_cellCount * 3 - 1) + (int)CellWidth, (_cellCount * 3 - 1) + (int)CellWidth);

        /// <summary>
        /// 맵의 전체 크기 (테두리 제외).
        /// [주의] 중심을 맞추기위해서 반드시 홀수를 사용해야한다.
        /// </summary>
        [SerializeField] int _cellCount = 19;

        [SerializeField] Transform  _map;
        [SerializeField] GameObject _wallPrefab;
        [SerializeField] GameObject _floorPrefab;

        [SerializeField] GameObject _borderColliderTop;
        [SerializeField] GameObject _borderColliderBottom;
        [SerializeField] GameObject _borderColliderLeft;
        [SerializeField] GameObject _borderColliderRight;

        bool[,]          m_MazeMap;
        MazeDirection[,] m_DirectionMap_Array;
        bool[,]          m_FinalMap_Array;

        /// <summary>
        /// 실제 게임에서 사용하는 bool
        /// [주의] Border를 포함하기 때문에 생성시 입력한 CellCount보다 2 크다.
        /// </summary>
        bool[,] m_Cell_Array;

        Vector3[,]        m_CellPos_Array;
        CompCellObject[,] m_CellObject_Array;

        List<BlockLineInfo> m_BlockLineInfoList;

        List<int> _placedCellObjectCountList = Enumerable.Repeat(0, (int)CellObjectType.Length).ToList();


        PlayablePlayer m_PlayablePlayer;


        public void Init()
        {
            switch (PlayerDataManager.Instance.PlayerData.CurrentStageIndex)
            {
                case 0:
                    _cellCount = 11;
                    break;
                case 1:
                case 2:
                    _cellCount = 11;
                    break;
                case 3:
                case 4:
                case 5:
                    _cellCount = 15;
                    break;
                case 6:
                case 7:
                case 8:
                    _cellCount = 17;
                    break;
                default:
                    _cellCount = 19;
                    break;
            }

            m_MazeMap            = new bool[_cellCount, _cellCount];
            m_DirectionMap_Array = new MazeDirection[_cellCount, _cellCount];
            m_FinalMap_Array     = new bool[_cellCount * 3, _cellCount * 3];
            m_Cell_Array         = new bool[CellCountWithBorder, CellCountWithBorder];
            m_CellPos_Array      = new Vector3[CellCountWithBorder, CellCountWithBorder];
            m_CellObject_Array   = new CompCellObject[CellCountWithBorder, CellCountWithBorder];

            m_BlockLineInfoList = new();

            MakeMap();
            MakeDirectionMap();
            MakeFinalMap();
            MakeFinalMapWithBorder();
            CreateFinalCellWithBorder();
        }


        /// <summary>
        /// [MakeMap Step1]
        /// </summary>
        void MakeMap()
        {
            // 상하좌우에 랜덤으로 거리 두고 초기 단계 점 찍기

            // Top (y=0)
            for (int j = 2; j < _cellCount - 2; j += Random.Range(3, 6))
            {
                m_MazeMap[0, j]            = true;
                m_DirectionMap_Array[0, j] = MazeDirection.Up_Down;
                var newBranch = new BlockLineInfo { CurCell = new Vector2Int(j, 0) }; // (x, y)
                m_BlockLineInfoList.Add(newBranch);
            }
            // Bottom (y=cellCount-1)
            for (int j = 2; j < _cellCount - 2; j += Random.Range(3, 6))
            {
                m_MazeMap[_cellCount - 1, j]            = true;
                m_DirectionMap_Array[_cellCount - 1, j] = MazeDirection.Up_Down;
                var newBranch = new BlockLineInfo { CurCell = new Vector2Int(j, _cellCount - 1) }; // (x, y)
                m_BlockLineInfoList.Add(newBranch);
            }
            // Left (x=0)
            for (int i = 2; i < _cellCount - 2; i += Random.Range(3, 6))
            {
                m_MazeMap[i, 0]            = true;
                m_DirectionMap_Array[i, 0] = MazeDirection.Left_Right;
                var newBranch = new BlockLineInfo { CurCell = new Vector2Int(0, i) }; // (x, y)
                m_BlockLineInfoList.Add(newBranch);
            }
            // Right (x=cellCount-1)
            for (int i = 2; i < _cellCount - 2; i += Random.Range(3, 6))
            {
                m_MazeMap[i, _cellCount - 1]            = true;
                m_DirectionMap_Array[i, _cellCount - 1] = MazeDirection.Left_Right;
                var newBranch = new BlockLineInfo { CurCell = new Vector2Int(_cellCount - 1, i) }; // (x, y)
                m_BlockLineInfoList.Add(newBranch);
            }

            int triedCount = 0;

            while (triedCount < _cellCount * _cellCount)
            {
                triedCount++;

                foreach (var branch in m_BlockLineInfoList)
                {
                    if (branch.IsBlocked)
                        continue;

                    switch (DecideDirection(branch.CurCell.y, branch.CurCell.x))
                    {
                        case Direction.Up:
                            m_MazeMap[branch.CurCell.y - 1, branch.CurCell.x] = true; // y-1
                            branch.CurCell.y--;                                       // y--
                            break;

                        case Direction.Down:
                            m_MazeMap[branch.CurCell.y + 1, branch.CurCell.x] = true; // y+1
                            branch.CurCell.y++;                                       // y++
                            break;

                        case Direction.Left:
                            m_MazeMap[branch.CurCell.y, branch.CurCell.x - 1] = true;
                            branch.CurCell.x--;
                            break;

                        case Direction.Right:
                            m_MazeMap[branch.CurCell.y, branch.CurCell.x + 1] = true;
                            branch.CurCell.x++;
                            break;

                        case Direction.None:
                            branch.IsBlocked = true;
                            break;
                    }
                }
            }

            Debug.Log($"triedCount : {triedCount}");

            PrintCell(_cellCount);

            // bool값을 enum으로 변경. 함수 내부에서만 사용
            Direction DecideDirection(int y, int x)
            {
                List<Direction> possibleDirections = new List<Direction>();
                //up
                if (y - 1 >= 0 && m_MazeMap[y - 1, x] == false)
                {
                    if (IsAreaEmpty(y - 1, x, y, x, Direction.Up))
                    {
                        possibleDirections.Add(Direction.Up);
                    }
                }
                //down
                if (y + 1 < _cellCount && m_MazeMap[y + 1, x] == false)
                {
                    if (IsAreaEmpty(y + 1, x, y, x, Direction.Down))
                    {
                        possibleDirections.Add(Direction.Down);
                    }
                }
                //left
                if (x - 1 >= 0 && m_MazeMap[y, x - 1] == false)
                {
                    if (IsAreaEmpty(y, x - 1, y, x, Direction.Left))
                    {
                        possibleDirections.Add(Direction.Left);
                    }
                }
                //right
                if (x + 1 < _cellCount && m_MazeMap[y, x + 1] == false)
                {
                    if (IsAreaEmpty(y, x + 1, y, x, Direction.Right))
                    {
                        possibleDirections.Add(Direction.Right);
                    }
                }

                if (possibleDirections.Count == 0)
                {
                    return Direction.None;
                }

                return possibleDirections[Random.Range(0, possibleDirections.Count)];


                bool IsAreaEmpty(int y, int x, int preYPos, int preXPos, Direction curDirection)
                {
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            int checkX = x + xOffset;
                            int checkY = y + yOffset;

                            //미로 범위 바깥이라면
                            if (checkX < 0 || checkX >= _cellCount || checkY < 0 || checkY >= _cellCount)
                            {
                                return false;
                            }

                            //방향 별 이전 좌표의 양옆 제외 처리
                            if (curDirection == Direction.Up || curDirection == Direction.Down)
                            {
                                if (checkX == preXPos - 1 && checkY == preYPos)
                                {
                                    continue;
                                }
                                if (checkX == preXPos + 1 && checkY == preYPos)
                                {
                                    continue;
                                }
                            }
                            if (curDirection == Direction.Left || curDirection == Direction.Right)
                            {
                                if (checkX == preXPos && checkY == preYPos + 1)
                                {
                                    continue;
                                }
                                if (checkX == preXPos && checkY == preYPos - 1)
                                {
                                    continue;
                                }
                            }


                            if (m_MazeMap[checkY, checkX] == true)
                            {
                                //이전 좌표는 제외
                                if (checkX == preXPos && checkY == preYPos)
                                {
                                    continue;
                                }

                                return false;
                            }
                        }
                    }
                    return true;
                }
            }

            // Debug용으로 함수 내부에서만 사용
            void PrintCell(int cellCount)
            {
                for (int i = 0; i < cellCount; i++)
                {
                    string row = ""; // 각 행을 문자열로 만들기 위해 초기화
                    for (int j = 0; j < cellCount; j++)
                    {
                        if (m_MazeMap[i, j] == true)
                        {
                            row += "□"; // 길
                        }
                        else
                        {
                            row += "■"; // 벽
                        }
                    }
                    Debug.Log(row); // 완성된 한 행을 콘솔에 출력
                }
            }
        }


        /// <summary>
        /// [MakeMap Step2]
        /// </summary>
        void MakeDirectionMap()
        {
            for (int i = 1; i < _cellCount - 1; i++)
            {
                for (int j = 1; j < _cellCount - 1; j++)
                {
                    if (m_MazeMap[i, j])
                    {
                        if (j - 1 < 0 || j + 1 >= _cellCount || i - 1 < 0 || i + 1 >= _cellCount)
                        {
                            continue;
                        }

                        //세로
                        if (m_MazeMap[i - 1, j] || m_MazeMap[i + 1, j])
                        {
                            m_DirectionMap_Array[i, j] = MazeDirection.Up_Down;
                        }
                        //가로
                        if (m_MazeMap[i, j + 1] || m_MazeMap[i, j - 1])
                        {
                            m_DirectionMap_Array[i, j] = MazeDirection.Left_Right;
                        }
                        //상,우
                        if (m_MazeMap[i - 1, j] && m_MazeMap[i, j + 1])
                        {
                            m_DirectionMap_Array[i, j] = MazeDirection.Up_Right;
                            continue;
                        }
                        //상,좌
                        if (m_MazeMap[i - 1, j] && m_MazeMap[i, j - 1])
                        {
                            m_DirectionMap_Array[i, j] = MazeDirection.Up_Left;
                            continue;
                        }
                        //하,우
                        if (m_MazeMap[i + 1, j] && m_MazeMap[i, j + 1])
                        {
                            m_DirectionMap_Array[i, j] = MazeDirection.Down_Right;
                            continue;
                        }
                        //하,좌
                        if (m_MazeMap[i + 1, j] && m_MazeMap[i, j - 1])
                        {
                            m_DirectionMap_Array[i, j] = MazeDirection.Down_Left;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// [MakeMap Step3]
        /// </summary>
        void MakeFinalMap()
        {
            for (int i = 0; i < _cellCount; i++)
            {
                for (int j = 0; j < _cellCount; j++)
                {
                    if (m_MazeMap[i, j] == true)
                    {
                        var upPos     = new Vector2Int(j * 3 + 1, i * 3);
                        var downPos   = new Vector2Int(j * 3 + 1, i * 3 + 2);
                        var leftPos   = new Vector2Int(j * 3, i * 3 + 1);
                        var rightPos  = new Vector2Int(j * 3 + 2, i * 3 + 1);
                        var centerPos = new Vector2Int(j * 3 + 1, i * 3 + 1);

                        switch (m_DirectionMap_Array[i, j])
                        {
                            case (MazeDirection.Up_Down):
                            {
                                m_FinalMap_Array[upPos.y, upPos.x]     = true;
                                m_FinalMap_Array[downPos.y, downPos.x] = true;
                                break;
                            }
                            case (MazeDirection.Left_Right):
                            {
                                m_FinalMap_Array[leftPos.y, leftPos.x]   = true;
                                m_FinalMap_Array[rightPos.y, rightPos.x] = true;
                                break;
                            }
                            case (MazeDirection.Up_Right):
                            {
                                m_FinalMap_Array[upPos.y, upPos.x]       = true;
                                m_FinalMap_Array[rightPos.y, rightPos.x] = true;
                                break;
                            }
                            case (MazeDirection.Up_Left):
                            {
                                m_FinalMap_Array[upPos.y, upPos.x]     = true;
                                m_FinalMap_Array[leftPos.y, leftPos.x] = true;
                                break;
                            }
                            case (MazeDirection.Down_Right):
                            {
                                m_FinalMap_Array[downPos.y, downPos.x]   = true;
                                m_FinalMap_Array[rightPos.y, rightPos.x] = true;
                                break;
                            }
                            case (MazeDirection.Down_Left):
                            {
                                m_FinalMap_Array[downPos.y, downPos.x] = true;
                                m_FinalMap_Array[leftPos.y, leftPos.x] = true;
                                break;
                            }
                        }
                        m_FinalMap_Array[centerPos.y, centerPos.x] = true;
                    }
                }
            }
        }


        /// <summary>
        /// [MakeMap Step4]
        /// </summary>
        void MakeFinalMapWithBorder()
        {
            for (int i = 0; i < _cellCount * 3 + 2; i++)
            {
                for (int j = 0; j < _cellCount * 3 + 2; j++)
                {
                    if (i < 1 || i > _cellCount * 3 || j < 1 || j > _cellCount * 3)
                    {
                        m_Cell_Array[i, j] = true;
                    }

                    if (i >= _cellCount * 3 || j >= _cellCount * 3)
                        continue;
                    if (m_FinalMap_Array[i, j] == true)
                    {
                        m_Cell_Array[i + 1, j + 1] = true;
                    }
                }
            }
        }


        /// <summary>
        /// [MakeMap Step5]
        /// </summary>
        void CreateFinalCellWithBorder()
        {
            float offsetX = -(CellCountWithBorder + 2) * 0.5f + 1.5f + (CellCountWithBorder % 2 == 0 ? 0.5f : 0f);
            float offsetZ = -(CellCountWithBorder + 2) * 0.5f + 0.5f + (CellCountWithBorder % 2 == 0 ? 0.5f : 0f);
            float offsetY = 0f;
            for (int i = 0; i < CellCountWithBorder; i++)
            {
                string row = ""; // 각 행을 문자열로 만들기 위해 초기화
                for (int j = 0; j < CellCountWithBorder; j++)
                {
                    if (m_Cell_Array[i, j] == true)
                    {
                        row += "□"; // 길
                    }
                    else
                    {
                        row += "■"; // 벽
                    }

                    GameObject block;

                    Vector3 blockPos = new Vector3(j + offsetX, offsetY, CellCountWithBorder - i + offsetZ);

                    if (m_Cell_Array[i, j] == true)
                    {
                        block = Instantiate(_wallPrefab, new Vector3(blockPos.x, -1, blockPos.z), Quaternion.identity);
                    }
                    else
                    {
                        block = Instantiate(_floorPrefab, blockPos, Quaternion.identity);
                    }

                    var playableComponents = block.GetComponentsInChildren<IPlayable>(true);

                    if (playableComponents != null)
                    {
                        foreach (var playable in playableComponents)
                        {
                            if (playable.PlayableKey == "Create")
                            {
                                playable.Play();
                            }
                        }
                    }
                    m_CellPos_Array[i, j] = blockPos;

                    block.transform.SetParent(_map);
                }
                Debug.Log(row); // 완성된 한 행을 콘솔에 출력
            }

            _borderColliderTop.transform.position    = new Vector3(0, 0, TopLeft_ExcludeBorder.z + CellWidth * 2);
            _borderColliderBottom.transform.position = new Vector3(0, 0, BottomLeft_ExcludeBorder.z - CellWidth * 2);
            _borderColliderLeft.transform.position   = new Vector3(TopLeft_ExcludeBorder.x - CellWidth * 2, 0, 0);
            _borderColliderRight.transform.position  = new Vector3(TopRight_ExcludeBorder.x + CellWidth * 2, 0, 0);
        }


        public Vector3 GetGatePosition(GateType gateType)
        {
            return gateType switch
                   {
                       GateType.Gate0 => TopLeft_ExcludeBorder,
                       GateType.Gate1 => BottomLeft_ExcludeBorder,
                       GateType.Gate2 => TopRight_ExcludeBorder,
                       _              => BottomRight_ExcludeBorder
                   };
        }


        public Vector3 GetGoalPosition() => BottomRight_ExcludeBorder;


        public Vector3 GetCellPosition(int x, int y)
        {
            return m_CellPos_Array[y, x];
        }


        /// <summary>
        /// 현재 위치의 wall, floor 상태를 반환한다.
        /// </summary>
        /// <param name="cellPos"></param>
        /// <returns>true이면 wall, false라면 floor</returns>
        public bool CheckCell(Vector2Int cellPos)
        {
            return m_Cell_Array[cellPos.y, cellPos.x];
        }


        public (bool IsValid, Vector2Int CellPos) GetCellPosFromWorldPosition(Vector3 worldPosition)
        {
            // 바깥을 찍는 등의 문제로 좌표를 반환할 수 없다.
            if (worldPosition.x < TopLeft_ExcludeBorder.x - CellWidth * 0.5f || worldPosition.x > TopRight_ExcludeBorder.x + CellWidth * 0.5f)
            {
                return (false, Vector2Int.zero);
            }
            if (worldPosition.z > TopLeft_ExcludeBorder.z + CellWidth * 0.5f || worldPosition.z < BottomLeft_ExcludeBorder.z - CellWidth * 0.5f)
            {
                return (false, Vector2Int.zero);
            }

            var x       = (int)(((worldPosition.x - TopLeft_ExcludeBorder.x + CellWidth) / CellWidth) + CellWidth * 0.5f);
            var y       = (int)(((TopLeft_ExcludeBorder.z - worldPosition.z + CellWidth) / CellWidth) + CellWidth * 0.5f);
            var cellPos = new Vector2Int(x, y);

            return (true, cellPos);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>isValid값이 false라면, 좌표가 잘못됨</returns>
        public (bool IsValid, CompCellObject CellObject) GetCellObjectFromWorldPosition(Vector3 worldPosition)
        {
            var cellPos = GetCellPosFromWorldPosition(worldPosition);
            if (cellPos.IsValid == false) return (false, null);
            var cellPosition = cellPos.CellPos;
            var cellObject   = m_CellObject_Array[cellPosition.y, cellPosition.x];

            return (true, cellObject);
        }


        public CompCellObject GetCellObject(Vector2Int cellPos)
        {
            return m_CellObject_Array[cellPos.y, cellPos.x];
        }


        public void SetCellObject(Vector2Int cellPos, CompCellObject cellObject)
        {
            m_CellObject_Array[cellPos.y, cellPos.x] = cellObject;
        }


        public List<CompCellObject> GetCellObjectInRadius(Vector3 worldPositionOfCenter, float radius)
        {
            List<CompCellObject> compCellObjectList = new();
            for (int i = 0; i < CellCountWithBorder; i++)
            {
                for (int j = 0; j < CellCountWithBorder; j++)
                {
                    var cellObject = m_CellObject_Array[j, i];
                    if (cellObject == null) continue;
                    var worldPositionOfCellObject = m_CellPos_Array[j, i];
                    if (Vector3.Distance(worldPositionOfCenter, worldPositionOfCellObject) <= radius)
                    {
                        compCellObjectList.Add(cellObject);
                    }
                }
            }
            return compCellObjectList;
        }


        public void ChangeCellObjects(SoCellObjectData soCellObjectData)
        {
            for (int i = 0; i < CellCountWithBorder; i++)
            {
                for (int j = 0; j < CellCountWithBorder; j++)
                {
                    var toBeDeleted = m_CellObject_Array[j, i];
                    if (toBeDeleted == null) continue;

                    if (toBeDeleted.SoCellObjectData.CellObjectType == soCellObjectData.CellObjectType)
                    {
                        var newCellObject = Instantiate(soCellObjectData.CompCellObject, toBeDeleted.gameObject.transform.position, toBeDeleted.gameObject.transform.rotation);
                        newCellObject.Init(soCellObjectData);

                        if (newCellObject is CompCellObject_Turret turret)
                        {
                            turret.SetRangeShow(false);
                        }
                        m_CellObject_Array[j, i] = newCellObject;

                        Destroy(toBeDeleted.gameObject);
                    }
                }
            }
        }
    }
}
