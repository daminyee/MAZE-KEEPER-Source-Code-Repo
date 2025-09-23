using System;
using System.Collections;
using System.Reflection;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MazeKeeper.Component
{
    [ExecuteAlways]
    public class CompAudioPlayer : MonoBehaviour, IPlayable
    {
        public string PlayableKey => _playableKey;

        [FormerlySerializedAs("_key")]
        [Header("IPlayable을 통해 실행될 Key")]
        [SerializeField] string _playableKey;

        [Header("실행 옵션")]
        [SerializeField] bool _playOnEnable;
        [SerializeField] float _delayTime;

        [Header("실행할 SoAudioClip")]
        [SerializeField] SoAudioClip _soAudioData;


        void OnEnable()
        {
#if UNITY_EDITOR
            // 에디터의 Timeline 편집모드에서 Audio를 재생하기 위해서
            // 에디터 클래스에서 지원하는 Audio기능을 사용해서 즉시 플레이 가능하도록 한다.
            if (_playOnEnable && AudioManager.Instance == null)
            {
                PlayClipForEditor(_soAudioData.AudioClip);
                return;
            }
#endif

            if (_playOnEnable) Play();
        }


        public void Play(Vector3? direction = null)
        {
            if (_delayTime == 0f)
            {
                AudioManager.Instance.Play(_soAudioData);
            }
            else
            {
                StartCoroutine(CR_DelayedPlay());
            }
        }


        IEnumerator CR_DelayedPlay()
        {
            yield return new WaitForSeconds(_delayTime);
            AudioManager.Instance.Play(_soAudioData);
        }


#if UNITY_EDITOR
        static void PlayClipForEditor(AudioClip clip)
        {
            var audioUtilClass = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            var method = audioUtilClass.GetMethod
            (
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, // PlayPreviewClip(AudioClip, startSample, loop)
                null
            );
            method.Invoke(null, new object[] { clip, 0, false });
        }
#endif
    }
}