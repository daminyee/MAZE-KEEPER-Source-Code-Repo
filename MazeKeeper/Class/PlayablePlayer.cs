using System.Linq;
using MazeKeeper.Define;
using MazeKeeper.Interface;
using UnityEngine;


namespace MazeKeeper.Class
{
    public class PlayablePlayer
    {
        IPlayable[] m_Playables;


        public PlayablePlayer(GameObject player, string key = null, bool includeInactive = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                m_Playables = player.GetComponentsInChildren<IPlayable>(includeInactive);
            }
            else
            {
                m_Playables = player.GetComponentsInChildren<IPlayable>(includeInactive).Where(x => x.PlayableKey == key).ToArray();
            }
        }


        public void Play(Vector3? direction = null)
        {
            foreach (var playable in m_Playables)
            {
                playable.Play(direction);
            }
        }
    }
}




