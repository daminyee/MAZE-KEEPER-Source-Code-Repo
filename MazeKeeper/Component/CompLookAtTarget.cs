using System.Collections.Generic;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompLookAtTarget : MonoBehaviour
    {
        [SerializeField] List<Transform> _lookTransformList = new List<Transform>();

        Transform m_LookTarget;
        public Transform LookTarget
            {
                set => m_LookTarget = value;
            }


        void Update()
        {
            if (m_LookTarget == null)
                return;

            foreach (Transform t in _lookTransformList)
            {
                t.LookAt(m_LookTarget);
            }
        }
    }
}