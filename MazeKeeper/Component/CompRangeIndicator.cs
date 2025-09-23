using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace MazeKeeper.Component
{
    public class CompRangeIndicator : MonoBehaviour
    {
        [SerializeField] DecalProjector _decalProjector;


        public void SetVisible(bool visible)
        {
            _decalProjector.enabled = visible;
        }


        public void SetRange(float range)
        {
            _decalProjector.size = new Vector3(range * 2, range * 2, 100);
        }
    }
}