using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField, Tooltip("How long to wait in between each timer cycle")]
    float faderTime = 0.5f;
    public float FaderTime { get { return faderTime; } }

    public enum SceneId
    {
        MainMenu,
        Level,
        GameOver,
        GameCompleted,
    }

    public SceneId CurrentScene { get { return (SceneId)SceneManager.GetActiveScene().buildIndex; } }

    public void Play()
    {
        StartCoroutine(TransitionToScene(SceneId.Level));
    }

    public void GameOver()
    {
        StartCoroutine(TransitionToScene(SceneId.GameOver));
    }

    public void GameCompleted()
    {
        StartCoroutine(TransitionToScene(SceneId.GameCompleted));
    }

    public void ReloadScene()
    {
        var scene = (SceneId)SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(TransitionToScene(scene));
    }

    IEnumerator TransitionToScene(SceneId sceneId)
    {
        if(SceneFader.Instance != null)
            yield return StartCoroutine(SceneFader.Instance.FadeRoutine(0f, 1f, GameManager.Instance.FaderTime));

        var index = (int)sceneId;

        if (Application.CanStreamedLevelBeLoaded(index))
            SceneManager.LoadScene(index, LoadSceneMode.Single);
        else
        {
            Debug.LogErrorFormat("Scene '{0}' cannot be loaded", sceneId);
            ReloadScene();
        }
    }    

    public void QuitGame()
    {
        Application.Quit();
    }
}