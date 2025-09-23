using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.ScriptableObject
{
    [CreateAssetMenu(fileName = "SoAudioClip", menuName = "SoData/SoAudioClip")]
    public class SoAudioClip : UnityEngine.ScriptableObject
    {
        public AudioChannelType AudioChannelType = AudioChannelType.FX;
        public AudioClip        AudioClip;
        public float            AudioVolume = 1f;
    }
}