using MazeKeeper.Define;
using UnityEngine;


namespace MazeKeeper.Interface
{
    public interface IPlayable
    {
        string PlayableKey { get; }

        void Play(Vector3? direction = null);
    }
}



