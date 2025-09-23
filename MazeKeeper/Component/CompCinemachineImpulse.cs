using MazeKeeper.Define;
using MazeKeeper.Interface;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompCinemachineImpulse : MonoBehaviour, IPlayable
    {
        [SerializeField] CinemachineImpulseSource _cinemachineImpulseSource;

        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [SerializeField] string _playableKey;
        [SerializeField] bool _playOnEnable;


        void OnEnable()
        {
            if (_playOnEnable) DoPlay();
        }


        public void Play(Vector3? direction = null)
        {
            DoPlay();
        }


        void DoPlay()
        {
            _cinemachineImpulseSource.GenerateImpulse();
        }
    }
}