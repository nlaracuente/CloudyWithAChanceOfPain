using UnityEngine;

public class LevelController : Singleton<LevelController>
{
    public Camera MainCamera { get; set; }
    public bool IsGameOver { get; set; }

    [SerializeField, Range(1f, 30f), Tooltip("Seconds before the fire spread")]
    float timeBeforeFireSpreads = 3;

    public float TimeBeforeFireSpreads { get { return timeBeforeFireSpreads; } }

    [SerializeField, Range(0.1f, 1f), Tooltip("Seconds the electrify fx stays in effect")]
    float timeShockIsInEffect = 1;
    public float TimeShockIsInEffect { get { return timeShockIsInEffect; } }

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
