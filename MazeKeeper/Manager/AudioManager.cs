using System;
using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Define;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Audio;


namespace MazeKeeper.Manager
{
    public class AudioManager : ManagerBase<AudioManager>
    {
        [Serializable]
        public class AudioSourceInitInfo
        {
            public AudioChannelType AudioChannelType;
            public AudioSource      AudioSourceTemplate;
            public int              Count;
        }


        public class AudioChannelInfo
        {
            public readonly List<AudioSourceInfo> AudioSourceInfoList = new();
        }


        public class AudioSourceInfo
        {
            public AudioSource AudioSource      = new();
            public SoAudioClip PlayingAudioClip = new();
            public float       PlayStartedTime;
        }


        public static string GetVolumeKey(string audioChannelType) => $"{audioChannelType}Volume";

        [SerializeField] AudioMixer                _audioMixer;
        [SerializeField] List<AudioSourceInitInfo> _audioSourceInitInfoList;

        readonly List<AudioChannelInfo> m_AudioChannelInfoList = new();


        void Start()
        {
            for (int i = 0; i < (int)AudioChannelType.Length; i++)
            {
                var              initInfo        = _audioSourceInitInfoList[i];
                AudioChannelInfo newAudioChannel = new();
                m_AudioChannelInfoList.Add(newAudioChannel);
                for (int j = 0; j < initInfo.Count; j++)
                {
                    var newAudioSourceInfo = new AudioSourceInfo();
                    newAudioSourceInfo.AudioSource = Instantiate(initInfo.AudioSourceTemplate, transform);
                    newAudioChannel.AudioSourceInfoList.Add(newAudioSourceInfo);
                }
            }

            SetVolume(GetVolumeKey("Master"), GetVolume(GetVolumeKey("Master")));
            SetVolume(GetVolumeKey(nameof(AudioChannelType.BGM)), GetVolume(GetVolumeKey(nameof(AudioChannelType.BGM))));
            SetVolume(GetVolumeKey(nameof(AudioChannelType.FX)), GetVolume(GetVolumeKey(nameof(AudioChannelType.FX))));
            SetVolume(GetVolumeKey(nameof(AudioChannelType.UI)), GetVolume(GetVolumeKey(nameof(AudioChannelType.UI))));
        }


        public void Play(SoAudioClip audioClip)
        {
            if (audioClip == null) return;
            if (CheckCanOmit(audioClip)) return;

            var info = GetIdleOrOldestAudioSourceInfo(audioClip.AudioChannelType);
            info.AudioSource.clip   = audioClip.AudioClip;
            info.AudioSource.volume = audioClip.AudioVolume;
            info.AudioSource.Play();
            info.PlayingAudioClip = audioClip;
            info.PlayStartedTime  = Time.unscaledTime;
        }


        public void Stop(AudioChannelType audioChannelType)
        {
            var audioChannelInfo = m_AudioChannelInfoList[(int)audioChannelType];
            foreach (var info in audioChannelInfo.AudioSourceInfoList)
            {
                info.AudioSource.Stop();
            }
        }


        public void SetVolume(string volumeKey, float volume)
        {
            _audioMixer.SetFloat(volumeKey, Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat(volumeKey, volume);
            Debug.Log($"Set {volumeKey}   {volume}");
        }


        public float GetVolume(string volumeKey)
        {
            return PlayerPrefs.GetFloat(volumeKey, 1f);
        }


        /// <summary>
        /// 현재 Play 가능한 AudioSource 반환.
        /// 만약 모든 AudioSource가 재생중이라면, 가장 플레이된지 오래된 AudioSource를 반환한다.(즉, 기존 오디오는 중단된다.)
        /// </summary>
        AudioSourceInfo GetIdleOrOldestAudioSourceInfo(AudioChannelType audioChannelType)
        {
            var audioChannelInfo = m_AudioChannelInfoList[(int)audioChannelType];
            foreach (var sourceInfo in audioChannelInfo.AudioSourceInfoList)
            {
                if (sourceInfo.AudioSource.isPlaying == false) return sourceInfo;
            }

            return audioChannelInfo.AudioSourceInfoList.OrderBy(x => x.PlayStartedTime).First();
        }


        /// <summary>
        /// 현재 재생중인 Audio중에 동일한 SoAudioClip가 일정 시간안에 다시 요청되면 생략(Omit)할수 있는지 체크
        /// </summary>
        bool CheckCanOmit(SoAudioClip audioClip)
        {
            var         audioChannelInfo  = m_AudioChannelInfoList[(int)audioClip.AudioChannelType];
            const float OmitThresholdTime = 0.02f;
            var similarAudioClip = audioChannelInfo.AudioSourceInfoList.FirstOrDefault(x => x.AudioSource.isPlaying
                                                                                         && x.PlayingAudioClip == audioClip
                                                                                         && x.PlayStartedTime + OmitThresholdTime > Time.unscaledTime);
            bool alreadySimilarAudioClipExist = similarAudioClip != null;
            return alreadySimilarAudioClipExist;
        }
    }
}