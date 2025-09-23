using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    [RequireComponent(typeof(RectTransform))]
    public class CompTransRectPosition : MonoBehaviour, IPlayable
    {
        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("IPlayable을 통해 실행될 커브")]
        [SerializeField] Option<AnimationCurve> _transPositionXCurveOption = new Option<AnimationCurve>(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)));
        [SerializeField] Option<AnimationCurve> _transPositionYCurveOption = new Option<AnimationCurve>(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)));
        [SerializeField] float                  _transPositionMultiply     = 1f;
        [SerializeField] float                  _transDuration             = 1f;

        public string PlayableKey => _playableKey;

        RectTransform _rectTransform;

        float m_CurrentPosX;
        float m_CurrentPosY;
        float m_ElapsedTime;
        bool  m_IsPlaying;


        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }


        public void Play(Vector3? direction = null)
        {
            m_ElapsedTime = 0;
            m_IsPlaying   = true;
        }


        void Update()
        {
            if (m_IsPlaying == false)
                return;

            m_ElapsedTime += Time.deltaTime;
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

            _rectTransform.anchoredPosition = new Vector3(m_CurrentPosX, m_CurrentPosY);

            if (m_ElapsedTime >= _transDuration)
                m_IsPlaying = false;
        }
    }
}