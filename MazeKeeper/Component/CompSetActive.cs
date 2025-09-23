using System.Collections.Generic;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompSetActive : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("IPlayable을 통해 Play가 호출되면 켜지는 오브젝트들")]
        [SerializeField] List<GameObject> _onObjectList = new();
        [Header("IPlayable을 통해 Play가 호출되면 꺼지는 오브젝트들")]
        [SerializeField] List<GameObject> _offObjectList = new();


        public void Play(Vector3? direction = null)
        {
            foreach (var go in _onObjectList)
            {
                go.SetActive(true);
            }
            foreach (var go in _offObjectList)
            {
                go.SetActive(false);
            }
        }
    }
}