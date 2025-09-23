using System.Collections.Generic;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;


namespace MazeKeeper.Component
{
    /// <summary>
    /// 블럭의 회전을 랜덤하게하여, 불규칙하게 보이도록하기 컴포넌트.
    /// </summary>
    public class CompTransRandomizer : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [SerializeField] string _playableKey = "Create";

        [SerializeField] List<GameObject> _gameObjectList = new List<GameObject>();

        List<Vector3> m_DirectionList = new List<Vector3>()
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left,
        };


        public void Play(Vector3? direction = null)
        {
            foreach (var go in _gameObjectList)
            {
                go.SetActive(false);
            }
            _gameObjectList[Random.Range(0, _gameObjectList.Count)].SetActive(true);

            int randomIndex = Random.Range(0, m_DirectionList.Count);

            Vector3 randomDirection = m_DirectionList[randomIndex];

            transform.forward = randomDirection;
        }
    }
}