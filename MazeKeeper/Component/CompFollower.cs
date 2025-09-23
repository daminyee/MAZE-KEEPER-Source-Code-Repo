using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public class CompFollower : MonoBehaviour
    {
        public bool FollowPosition;
        public bool FollowRotationX;
        public bool FollowRotationY;
        public bool FollowRotationZ;

        [SerializeField] Transform _targetTransform;


        void Update()
        {
            UpdatePosition();
            UpdateRotation();
        }


        void UpdatePosition()
        {
            if (_targetTransform == null) return;
            if (FollowPosition == false) return;
            transform.position = _targetTransform.position;
        }


        void UpdateRotation()
        {
            if (_targetTransform == null) return;
            if (FollowRotationX == false && FollowRotationY == false && FollowRotationZ == false) return;

            var targetRotationEuler = _targetTransform.eulerAngles;
            var myRotationEuler     = transform.eulerAngles;

            transform.rotation = Quaternion.Euler(FollowRotationX ? targetRotationEuler.x : myRotationEuler.x,
                                                  FollowRotationY ? targetRotationEuler.y : myRotationEuler.y,
                                                  FollowRotationZ ? targetRotationEuler.z : myRotationEuler.z);
        }
    }
}