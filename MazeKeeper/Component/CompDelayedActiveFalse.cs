using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompDelayedActiveFalse : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("Delay(초)후에 Active가 False됨")]
        [SerializeField] float _delay;

        float m_ElapsedTime;
        bool  m_IsPlaying;


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

            if (m_ElapsedTime >= _delay)
            {
                m_IsPlaying = false;
                gameObject.SetActive(false);
            }
        }
    }
}