using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompTransScale : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("실행 옵션")]
        [SerializeField] bool _playOnEnable;

        [Header("Scale 변화 커브")]
        [SerializeField] AnimationCurve _transScaleCurveOption = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        [SerializeField] float _transScaleMultiply = 1f;
        [SerializeField] float _transDuration      = 1f;


        float m_CurrentScale;
        float m_ElapsedTime;
        bool  m_IsPlaying;


        void OnEnable()
        {
            if (_playOnEnable) Play();
        }


        public void Play(Vector3? direction = null)
        {
            m_ElapsedTime = 0f;
            m_IsPlaying   = true;
        }


        void Update()
        {
            if (m_IsPlaying == false) return;

            m_ElapsedTime += Time.deltaTime;
            var normalizedTime = m_ElapsedTime / _transDuration;
            var evaluatedScale = _transScaleCurveOption.Evaluate(normalizedTime);
            m_CurrentScale = (evaluatedScale - 1) * _transScaleMultiply + 1;

            gameObject.transform.localScale = new(m_CurrentScale, m_CurrentScale, m_CurrentScale);

            if (m_ElapsedTime >= _transDuration) m_IsPlaying = false;
        }
    }
}