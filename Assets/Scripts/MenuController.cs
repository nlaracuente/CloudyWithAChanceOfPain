using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] AudioClipInfo titleScreenClipInfo;

    void Start()
    {
        StartCoroutine(IntroRoutine());        
    }

    IEnumerator IntroRoutine()
    {
        if (GameManager.SceneId.MainMenu == GameManager.Instance.CurrentScene)
            AudioManager.Instance.Play2DSound(titleScreenClipInfo.clip, titleScreenClipInfo.volume);

        yield return StartCoroutine(SceneFader.Instance.FadeRoutine(1f, 0f, GameManager.Instance.FaderTime));        
    }

    public void Play()
    {
        GameManager.Instance.Play();
    }    

    public void Exit()
    {
        GameManager.Instance.QuitGame();
    }
}
