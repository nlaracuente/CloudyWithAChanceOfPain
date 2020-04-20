using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    Sheep sheep;

    /// <summary>
    /// Used for Scene Transitions
    /// </summary>
    [SerializeField]
    private GameObject transitionPanel;
    [SerializeField]
    private Animator sceneTransitionAnimation;

    private  enum SceneInex
    {
        Title = 0,
        Level = 1,
        Lose = 2,
        Win = 3
    }


    /// <summary>
    /// The layer mask where the floor we care for collision is at
    /// </summary>
    [SerializeField]
    LayerMask clickableLayer;

    bool IsGameOver { get; set; }

    private void Start()
    {
        transitionPanel = GameObject.Find("SceneTransitionPanel");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }
    }

    /// <summary>
    /// Initializes the game and starts the gameplay loop
    /// </summary>
    public void StartGame()
    {
        //Start Transition process and load the game
        LoadLevel();
    }

    private void LoadLevel()
    {
        //transitionPanel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sceneTransitionAnimation = transitionPanel.GetComponent<Animator>();
        sceneTransitionAnimation.enabled = true;
        //Starts the animation to laod the level
        StartCoroutine(LoadSceneAFterTransition((int)SceneInex.Level));
    }

    /// <summary>
    /// Load any scene after using transiton animation
    /// </summary>
    /// <param name="sceneIndex">int number matching to scene number in build settings</param>
    /// <returns></returns>
    private IEnumerator LoadSceneAFterTransition(int sceneIndex)
    {
        //show animate out animation
        //sceneTransitionAnimation.SetBool("animateOut", true);
        yield return new WaitForSeconds(1.0f);

        //load the scene we want
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Method to control what happens when you lose the game
    /// </summary>
    public void GameLost()
    {
        //Load the the you lost scene
        StartCoroutine(LoadSceneAFterTransition((int)SceneInex.Lose));
    }

    /// <summary>
    /// Method to control what happens when you win the game
    /// </summary>
    public void GameWon()
    {
        //Load the you won scene
        StartCoroutine(LoadSceneAFterTransition((int)SceneInex.Win));
    }

    public void LoadTitle()
    {
        //Load the Title Menu Scene
        StartCoroutine(LoadSceneAFterTransition((int)SceneInex.Title));
    }

    /// <summary>
    /// Terminates the application
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    IConsumable GetConsumableStriked()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        IConsumable consumable = default;
        if(Physics.Raycast (ray, out hit, Mathf.Infinity, clickableLayer))
        {
            consumable = hit.collider.GetComponent<IConsumable>();
        }

        return consumable;
    }

    void OnSceneLoaded()
    {
        IsGameOver = false;
        sheep = FindObjectOfType<Sheep>();
    }
}
