using MazeKeeper.Class;
using MazeKeeper.Interface;
using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompGoal : MonoBehaviour
    {
        PlayablePlayer m_PlayablePlayer_Goal;
        PlayablePlayer m_PlayablePlayer_Explode;


        public void Init()
        {
            transform.position = GameManager.Instance.GameMazeController.GetGoalPosition();

            m_PlayablePlayer_Goal    = new(gameObject, "Goal");
            m_PlayablePlayer_Explode = new(gameObject, "Explode");
        }


        public void Goal() => m_PlayablePlayer_Goal.Play();

        public void Explode() => m_PlayablePlayer_Explode.Play();
    }
}