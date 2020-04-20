using UnityEngine;

public class LevelController : Singleton<LevelController>
{
    public Camera MainCamera { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsPaused { get; set; }

    [SerializeField, Range(1f, 30f), Tooltip("Seconds before the fire spread")]
    float timeBeforeFireSpreads = 3f;

    public float TimeBeforeFireSpreads { get { return timeBeforeFireSpreads; } }

    [SerializeField, Range(0.1f, 1f), Tooltip("Seconds the electrify fx stays in effect")]
    float timeShockIsInEffect = 1f;
    public float TimeShockIsInEffect { get { return timeShockIsInEffect; } }

    [SerializeField, Range(1f, 10f), Tooltip("Seconds the sheep cannot be affected by fire")]
    float timeSheepIsInvincible = 1f;
    public float TimeSheepIsInvincible { get { return timeSheepIsInvincible; } }

    private void Start()
    {
        MainCamera = Camera.main;
        
        StartCoroutine(SceneFader.Instance.FadeRoutine(1f, 0f, GameManager.Instance.FaderTime));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
