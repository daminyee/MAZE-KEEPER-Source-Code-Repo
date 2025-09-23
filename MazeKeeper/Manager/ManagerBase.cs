using UnityEngine;


namespace MazeKeeper.Manager
{
    public class ManagerBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        [SerializeField] bool _dontDestroyOnLoad = true;


        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"{typeof(T).Name} Manager가 이미 존재합니다. 의도한 것인지 확인해주세요.");
                Destroy(Instance.gameObject);
            }

            Instance = this as T;

            if (_dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }
    }
}
