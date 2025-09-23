using MazeKeeper.Class;
using UnityEngine;
using UnityEngine.UI;


namespace MazeKeeper.Component
{
    [RequireComponent(typeof(Button))]
    public class CompButtonFX : MonoBehaviour
    {
        [SerializeField] string _key;

        PlayablePlayer m_PlayablePlayer;


        void Start()
        {
            m_PlayablePlayer = new(gameObject, _key);
            GetComponent<Button>().onClick.AddListener(Play);
        }


        public void Play() => m_PlayablePlayer.Play();
    }
}