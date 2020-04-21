using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    GameObject menu;

    bool isOpened = true;

    private void Start()
    {
        ToggleMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelController.Instance.IsGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMenu();
    }

    void ToggleMenu()
    {
        isOpened = !isOpened;
        menu.SetActive(isOpened);
        Time.timeScale = isOpened ? 0f : 1f;
        LevelController.Instance.IsPaused = isOpened;
    }

    public void Play()
    {
        isOpened = true;
        ToggleMenu();
    }

    public void Exit()
    {
        GameManager.Instance.Title();
        isOpened = true;
        isOpened = !isOpened;
        Time.timeScale = isOpened ? 0f : 1f;
    }
}

