using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompTransPosition : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("실행 옵션")]
        [SerializeField] bool _playOnEnable;
        [SerializeField] bool _useRealtime;


        [Header("위치 변화 커브")]
        [SerializeField] Option<AnimationCurve> _transPositionXCurveOption = new Option<AnimationCurve>(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)));
        [SerializeField] Option<AnimationCurve> _transPositionYCurveOption = new Option<AnimationCurve>(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)));
        [SerializeField] Option<AnimationCurve> _transPositionZCurveOption = new Option<AnimationCurve>(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)));
        [SerializeField] float                  _transPositionMultiply     = 1f;
        [SerializeField] float                  _transDuration             = 1f;

        [Header("Random Delay")]
        [SerializeField] bool _randomDelayOption;
        [SerializeField] float _delayMin;
        [SerializeField] float _delayMax = 1f;

        [Header("OnDisable시에 돌아갈 위치")]
        [SerializeField] Option<Vector3> _positionOnDisable;


        float m_PlayDelay;
        float m_CurrentPosX;
        float m_CurrentPosY;
        float m_CurrentPosZ;
        float m_ElapsedTime;
        float m_DelayedTime;
        bool  m_IsPlaying;


        void OnEnable()
        {
            if (_playOnEnable) Play();
        }


        public void Play(Vector3? direction = null)
        {
            m_ElapsedTime = 0;
            m_DelayedTime = 0;
            m_IsPlaying   = true;

            m_PlayDelay = Random.Range(_delayMin, _delayMax);
        }


        void Update()
        {
            if (m_IsPlaying == false)
                return;

            if (_randomDelayOption)
            {
                if (m_DelayedTime <= m_PlayDelay)
                {
                    m_DelayedTime += GetDeltaTime();
                    return;
                }
            }

            m_ElapsedTime += GetDeltaTime();
            
            // 각 커브옵션의 그래프에 따라 이동
            var normalizedTime = m_ElapsedTime / _transDuration;
            
            if (_transPositionXCurveOption.Enabled)
            {
                var evaluatedPosX = _transPositionXCurveOption.Value.Evaluate(normalizedTime);
                m_CurrentPosX = evaluatedPosX * _transPositionMultiply;
            }
            else
            {
                m_CurrentPosX = transform.localPosition.x;
            }

            if (_transPositionYCurveOption.Enabled)
            {
                var evaluatedPosY = _transPositionYCurveOption.Value.Evaluate(normalizedTime);
                m_CurrentPosY = evaluatedPosY * _transPositionMultiply;
            }
            else
            {
                m_CurrentPosY = transform.localPosition.y;
            }

            if (_transPositionZCurveOption.Enabled)
            {
                var evaluatedPosZ = _transPositionZCurveOption.Value.Evaluate(normalizedTime);
                m_CurrentPosZ = evaluatedPosZ * _transPositionMultiply;
            }
            else
            {
                m_CurrentPosZ = transform.localPosition.z;
            }


            transform.localPosition = new Vector3(m_CurrentPosX, m_CurrentPosY, m_CurrentPosZ);

            if (m_ElapsedTime >= _transDuration)
                m_IsPlaying = false;
        }


        void OnDisable()
        {
            Stop();
            if (_positionOnDisable.Enabled) transform.localPosition = _positionOnDisable.Value;
        }


        public void Stop()
        {
            m_IsPlaying = false;
        }


        float GetDeltaTime()
        {
            return _useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
    }
}
