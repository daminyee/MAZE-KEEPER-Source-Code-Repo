using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MazeKeeper.Manager
{
    public class SceneLoadManager : ManagerBase<SceneLoadManager>
    {
        [SerializeField] CanvasGroup _canvasGroup;

        public event Action OnSceneChanged;


        public void ChangeScene(string sceneName, float fadeTime = 0.4f)
        {
            PauseManager.Instance.UnPause();
            StartCoroutine(CR_ChangeScene(sceneName, fadeTime));
        }


        IEnumerator CR_ChangeScene(string sceneName, float fadeTime)
        {
            yield return StartCoroutine(CR_Fade(fadeTime, 0f, 1f));
            PoolManager.Instance.DeletePoolAll();
            yield return null;
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return null;
            OnSceneChanged?.Invoke();
            yield return null;
            yield return StartCoroutine(CR_Fade(fadeTime, 1f, 0f));
        }


        IEnumerator CR_Fade(float fadeTime, float fadeStart, float fadeEnd)
        {
            _canvasGroup.alpha = fadeStart;
            var elapsedTime = 0f;

            while (true)
            {
                elapsedTime += Time.deltaTime;
                var normalTime = elapsedTime / fadeTime;
                _canvasGroup.alpha = Mathf.Lerp(fadeStart, fadeEnd, normalTime);

                if (elapsedTime > fadeTime) break;
                yield return null;
            }

            _canvasGroup.alpha = fadeEnd;
            yield return null;
        }
    }
}