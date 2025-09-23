using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace MazeKeeper.Component
{
    public class CompTimelineBrainAssigner : MonoBehaviour
    {
        PlayableDirector m_PlayableDirector;

        CinemachineBrain m_CinemachineBrain;


        void Start()
        {
            m_CinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            m_PlayableDirector = GetComponent<PlayableDirector>();
            AssignBrainToTimeline();
        }


        public void AssignBrainToTimeline()
        {
            // PlayableDirector나 CinemachineBrain이 할당되지 않았으면 오류를 방지하기 위해 중단
            if (m_PlayableDirector == null || m_CinemachineBrain == null)
            {
                Debug.LogError("PlayableDirector 또는 CinemachineBrain이 할당되지 않았습니다.");
                return;
            }

            // PlayableDirector가 사용하는 타임라인 에셋을 가져옴
            TimelineAsset timelineAsset = m_PlayableDirector.playableAsset as TimelineAsset;

            if (timelineAsset == null)
            {
                Debug.LogError("PlayableDirector에 유효한 TimelineAsset이 없습니다.");
                return;
            }

            // 타임라인의 모든 트랙을 순회
            foreach (var track in timelineAsset.GetRootTracks())
            {
                // 트랙이 CinemachineTrack 타입인지 확인
                if (track is CinemachineTrack)
                {
                    // SetGenericBinding을 사용하여 트랙에 CinemachineBrain을 바인딩(할당)
                    m_PlayableDirector.SetGenericBinding(track, m_CinemachineBrain);
                    Debug.Log($"'{timelineAsset.name}' 타임라인의 CinemachineTrack에 '{m_CinemachineBrain.gameObject.name}'의 Brain을 성공적으로 할당했습니다.");

                    // 첫 번째 Cinemachine 트랙을 찾으면 루프 중단
                    break;
                }
            }
        }
    }
}