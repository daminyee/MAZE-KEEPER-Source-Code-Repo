using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.ComponentGUI
{
    public class CompGUIHudMiniMapMover : MonoBehaviour
    {
        [SerializeField] Transform _target;

        RectTransform m_rectTransform;

        float m_CellSize;


        void Start()
        {
            m_rectTransform = GetComponent<RectTransform>();
            int mapSize = GameManager.Instance.GameMazeController.CellCountWithBorder;
            m_CellSize = BattleUIManager.MiniMapUISize / mapSize;
        }


        void Update()
        {
            m_rectTransform.anchoredPosition = new Vector3(_target.position.x * m_CellSize, _target.position.z * m_CellSize);
            m_rectTransform.rotation         = Quaternion.Euler(0f, 0f, -_target.rotation.eulerAngles.y);
        }


        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}