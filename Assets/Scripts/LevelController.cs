using UnityEngine;

public class LevelController : Singleton<LevelController>
{
    public Camera MainCamera { get; set; }
    public bool IsGameOver { get; set; }

    private void Start()
    {
        MainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
