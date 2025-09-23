using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompTransCanvasGroupAlpha : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("실행 옵션")]
        [SerializeField] bool _playOnEnable;
        [SerializeField] bool _loop;

        [Header("Alpha 커브")]
        [SerializeField] AnimationCurve _transAlphaCurve = new(new Keyframe(0, 1), new Keyframe(1, 1));
        [SerializeField] float _transDuration = 1f;

        [Header("OnDisable시에 돌아갈 Value")]
        [SerializeField] Option<float> _valueOnDisable;


        CanvasGroup m_CanvasGroup;

        float m_ElapsedTime;
        bool  m_IsPlaying;


        void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
        }


        void OnEnable()
        {
            if (_playOnEnable) Play();
        }


        public void Play(Vector3? direction = null)
        {
            m_ElapsedTime = 0;
            m_IsPlaying   = true;
        }


        void Update()
        {
            if (m_IsPlaying == false) return;

            m_ElapsedTime += Time.deltaTime;
            var normalizedTime = m_ElapsedTime / _transDuration;
            
            // 커브 그래프에 따라 투명도 조절
            m_CanvasGroup.alpha = _transAlphaCurve.Evaluate(normalizedTime);

            if (m_ElapsedTime >= _transDuration)
            {
                if (_loop) { m_ElapsedTime = 0f; }
                else Stop();
            }
        }


        void OnDisable()
        {
            Stop();
            if (_valueOnDisable.Enabled) m_CanvasGroup.alpha = _valueOnDisable.Value;
        }


        public void SetTransDuration(float duration) => _transDuration = duration;


        public void Stop()
        {
            m_IsPlaying = false;
        }
    }
}
