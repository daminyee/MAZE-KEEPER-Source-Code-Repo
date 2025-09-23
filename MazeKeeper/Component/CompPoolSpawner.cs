using System.Collections;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompPoolSpawner : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_iPlayableKey")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("실행 옵션")]
        [SerializeField] bool _playOnEnable;
        [SerializeField] float _delayTime;

        [Header("스폰할 PoolItem")]
        [SerializeField] CompPoolItem _compPoolItem;

        [Header("스폰할 위치 (null이면 현재 위치에 스폰)")]
        [SerializeField] Transform _spawnPositionAndRotation;


        void OnEnable()
        {
            if (_playOnEnable) Play();
        }


        public void Play(Vector3? direction = null)
        {
            if (_delayTime == 0f)
            {
                DoPlay(direction);
            }
            else
            {
                StartCoroutine(CR_DelayedPlay(direction));
            }
        }


        IEnumerator CR_DelayedPlay(Vector3? direction = null)
        {
            yield return new WaitForSeconds(_delayTime);
            DoPlay(direction);
        }

        /// <summary>
        /// 스폰할 PoolItem을 스폰
        /// </summary>
        void DoPlay( Vector3? direction = null)
        {
            var position                    = (_spawnPositionAndRotation == null) ? transform.position : _spawnPositionAndRotation.position;
            var rotation                    = (_spawnPositionAndRotation == null) ? transform.rotation : _spawnPositionAndRotation.rotation;
            if (direction != null) rotation = Quaternion.LookRotation(direction.Value);
            PoolManager.Instance.Spawn(_compPoolItem, position, rotation, true);
        }
    }
}
