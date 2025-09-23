using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompTransPositionByDirection : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("IPlayable을 통해 전달된 Direction을 기준으로 커브만큼 이동")]
        [SerializeField] Option<AnimationCurve> _transPositionCurveOption = new Option<AnimationCurve>(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)));

        [SerializeField] float _transPositionMultiply = 1f;
        [SerializeField] float _transDuration         = 1f;

        [SerializeField] bool _transX;
        [SerializeField] bool _transY;
        [SerializeField] bool _transZ;

        [SerializeField] bool    _worldDirectionToLocalDirection = true;
        [SerializeField] Vector3 _direction;


        float m_ElapsedTime;
        bool  m_IsPlaying;


        public void Play(Vector3? direction = null)
        {
            m_ElapsedTime = 0;
            m_IsPlaying   = true;

            if (direction == null)
                return;

            if (_worldDirectionToLocalDirection)
            {
                _direction = transform.InverseTransformDirection(direction.Value);
            }
            else
            {
                _direction = direction.Value;
            }
        }


        void Update()
        {
            if (m_IsPlaying == false)
                return;

            m_ElapsedTime += Time.deltaTime;
            var normalizedTime = m_ElapsedTime / _transDuration;

            var position = _direction.normalized * _transPositionCurveOption.Value.Evaluate(normalizedTime) * _transPositionMultiply;
            if (_transX == false)
            {
                position.x = transform.localPosition.x;
            }
            if (_transY == false)
            {
                position.y = transform.localPosition.y;
            }
            if (_transZ == false)
            {
                position.z = transform.localPosition.z;
            }
            transform.localPosition = position;

            if (m_ElapsedTime >= _transDuration)
                m_IsPlaying = false;
        }
    }
}