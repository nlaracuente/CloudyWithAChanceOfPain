using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using System.ComponentModel;

[RequireComponent(typeof(NavMeshAgent), typeof(ThoughtBubble))]
public class Sheep : MonoBehaviour, IAttackable, IDousable, IPuddleInteractible
{
    enum State
    {
        Resting,
        Idling,
        Walking,
        Kneeling,
        Eating,
        Drinking,
        Running,
        Burning,
        Diying,
    };    

    [SerializeField] NavMeshAgent navMeshAgent;    
    [SerializeField] Animator animController;
    [SerializeField] Camera deathCamera;
    [SerializeField] int totalStrikes = 3;

    // Speeds
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float runningSpeed = 5f;
    [SerializeField] float puddleSpeed = 3f;

    // Times
    [SerializeField] float timeBurning = 5f;
    [SerializeField] float timeResting = 2f;
    [SerializeField] float timeToPreviewThought = 2f;
    [SerializeField] float timeToDisplayResourceNotFoundIcon = 1f;

    // Particle Effects
    [SerializeField] ParticleSystem fireParticleSystem;

    // Sounds Clips
    [SerializeField] List<AudioClipInfo> eatingClips;
    [SerializeField] List<AudioClipInfo> drinkingClips;
    [SerializeField] List<AudioClipInfo> restingClips;
    [SerializeField] List<AudioClipInfo> burningClips;
    [SerializeField] List<AudioClipInfo> deathClips;

    IEnumerator activeCoroutine;
    State state;

    /// <summary>
    /// Defaults to being hungry
    /// </summary>
    Resource curResource;

    /// <summary>
    /// The current consumable the sheep has targeted
    /// </summary>
    IConsumable curConsumable;

    ThoughtBubble thoughtBubble;

    ThoughtBubble.Thought curThought;

    TargetResourceIcon targetIcon;

    public bool IsRunning { get { return state == State.Running || state == State.Burning; } }
    public bool IsNotMoving { get { return navMeshAgent.velocity == Vector3.zero; } }

    public int Strikes { get; private set; }
    public bool ResourcesLow { get { return Strikes == totalStrikes; } }

    private void Awake()
    {
        // This should work but it is not so going the longer way
        // navMeshAgent = navMeshAgent ?? GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        thoughtBubble = GetComponentInChildren<ThoughtBubble>();
        deathCamera.enabled = false;
    }

    /// <summary>
    /// Initiates routine
    /// </summary>
    void Start()
    {
        targetIcon = FindObjectOfType<TargetResourceIcon>();

        targetIcon.DisableTarget();
        thoughtBubble.DisableThought();

        state = State.Resting;
        curResource = Resource.Rest;
        AutoChangeState();
    }

    /// <summary>
    /// Changes the completed routine to the routine that follows it
    /// </summary>
    void AutoChangeState(bool switchResource = true)
    {
        if (Strikes > totalStrikes)
        {
            StartCoroutine(TriggerDeath());
            return;
        }

        switch (state)
        {
            // Done Walking: Consume Resource
            case State.Walking:
                if(curConsumable.ResourceType == Resource.Rest)
                    SwitchCoroutine(RestingRoutine());
                else
                    SwitchCoroutine(ConsumeResourceRoutine(curConsumable));
                break;

            // Done consuming resource: Fetch the next one
            // To add varierity, rest areas are chosen at random
            case State.Resting:
            case State.Eating:
            case State.Drinking:
            case State.Idling:

                // Pursue the current resource - just target a new one
                if(switchResource)
                    SwitchToNextResource();

                var go = GetRandomResource(curResource);
                curConsumable = go.GetComponent<IConsumable>();
                targetIcon.SetTarget(go.transform);
                SwitchCoroutine(WalkRoutine(go.transform.position));
                break;
        }
    }

    /// <summary>
    /// Updates the current resource to the next resource to go after
    /// </summary>
    void SwitchToNextResource()
    {
        switch (curResource)
        {
            case Resource.Food:
                curResource = Resource.Water;
                break;

            case Resource.Water:
                curResource = Resource.Rest;
                break;

            case Resource.Rest:
                curResource = Resource.Food;
                break;
        }
    }

    /// <summary>
    /// Stops the current routine to enable the new one
    /// </summary>
    /// <param name="newRoutine"></param>
    void SwitchCoroutine(IEnumerator newRoutine)
    {
        if(activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = newRoutine;
        StartCoroutine(activeCoroutine);
        SetAnimatorToCurrentState();
    }

    void SetAnimatorToCurrentState()
    {
        animController.SetBool("Idling", state == State.Idling);
        animController.SetBool("Walking", state == State.Walking);
        animController.SetBool("Running", state == State.Running || state == State.Burning);
        animController.SetBool("Kneeling", state == State.Kneeling || state == State.Resting);
        animController.SetBool("Consuming", state == State.Eating || state == State.Drinking);
    }    

    public void StruckedByLightning()
    {
        if (state == State.Diying || state == State.Burning)
            return;

        SwitchCoroutine(BurningRoutine());
    }

    public void RainedOn()
    {
        // Already rained on
        if (state == State.Diying || state == State.Idling || state == State.Resting)
            return;

        // Stop it first
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.velocity = Vector3.zero;

        TransitionToIdle();
    }

    void TransitionToIdle()
    {
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.isStopped = true; // prevent navmesh from moving the object

        fireParticleSystem?.Stop();
        navMeshAgent.speed = walkingSpeed;       

        SwitchCoroutine(IdlingRoutine());
    }

    /// <summary>
    /// Wait before resuming previous routine
    /// </summary>
    /// <returns></returns>
    IEnumerator IdlingRoutine()
    {
        state = State.Idling;

        var info = AudioManager.Instance.GetRandomAudioClipInfo(restingClips);
        AudioManager.Instance.Play2DSound(info.clip, info.volume);
        yield return new WaitForSeconds(info.clip.length);

        // Ensures the sheep can walk again
        navMeshAgent.isStopped = false;

        AutoChangeState(false);
    }

    /// <summary>
    /// Briefly shows what the sheep is thinking about
    /// </summary>
    /// <param name="thought"></param>
    /// <returns></returns>
    IEnumerator ShowThoughtBubbleRoutine(ThoughtBubble.Thought thought = default)
    {
        if (thought == ThoughtBubble.Thought.Nohting)
            thought = curThought;

        thoughtBubble.SetThought(thought);
        yield return new WaitForSeconds(timeToPreviewThought);
        thoughtBubble.DisableThought();
    }

    /// <summary>
    /// Walks towards the given destination
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    IEnumerator WalkRoutine(Vector3 destination)
    {
        state = State.Walking;
        navMeshAgent.SetDestination(destination);

        ThoughtBubble.Thought thought = curThought;
        switch(curResource)
        {
            case Resource.Food:
                thought = ThoughtBubble.Thought.Food;
                break;

            case Resource.Water:
                thought = ThoughtBubble.Thought.Water;
                break;

            case Resource.Rest:
                thought = ThoughtBubble.Thought.Rest;
                break;
        }

        StartCoroutine(ShowThoughtBubbleRoutine(thought));

        while (Vector3.Distance(navMeshAgent.destination, transform.position) > .01f)
            yield return new WaitForEndOfFrame();

        // Fully Stop
        navMeshAgent.velocity = Vector3.zero;
        yield return new WaitForEndOfFrame();

        // TODO: Change to smooth look at
        transform.LookAt(curConsumable.gameObject.transform);
        yield return new WaitForEndOfFrame();

        // Continue
        AutoChangeState();
    }    

    /// <summary>
    /// Stop for a moment and relax
    /// </summary>
    /// <returns></returns>
    IEnumerator RestingRoutine()
    {
        state = State.Resting;
        animController.SetBool("Idling", true);
        AudioManager.Instance.PlayRandom2DClip(restingClips);
        yield return new WaitForSeconds(timeResting);
        AutoChangeState();
    }

    IEnumerator ConsumeResourceRoutine(IConsumable consumable)
    {
        state = consumable.ResourceType == Resource.Food
                ? State.Eating
                : State.Drinking;

        // To track whether the resource was consumed and we can change it
        // Since the state of the "IsConsumable" will change once consumed
        bool consumed = consumable.IsConsumable;

        if (!consumable.IsConsumable)
        {
            yield return StartCoroutine(ShowThoughtBubbleRoutine(ThoughtBubble.Thought.Hurt));
            Strikes++;
        }
        else
        {
            // Reset Strikes
            Strikes = 0;
            StartCoroutine(ShowThoughtBubbleRoutine(ThoughtBubble.Thought.Happy));

            var infos = consumable.ResourceType == Resource.Food
                        ? eatingClips
                        : consumable.ResourceType == Resource.Water
                        ? drinkingClips
                        : restingClips;

            var s = AudioManager.Instance.PlayRandom2DClip(infos);
            yield return new WaitForSeconds(s.clip.length);
            curConsumable.Consume();
        }

        AutoChangeState(consumed);
    }

    /// <summary>
    /// Randomly runs around the map until times up
    /// </summary>
    /// <returns></returns>
    IEnumerator BurningRoutine()
    {
        state = State.Burning;
        thoughtBubble.DisableThought();
        targetIcon.DisableTarget();
        var t = Time.time + timeBurning;

        fireParticleSystem?.Play();
        navMeshAgent.speed = runningSpeed;
        while(Time.time < t)
        {
            // Get Random destination
            var d = GetRandomResource().transform.position;

            // Face it and book it!
            transform.LookAt(d);
            navMeshAgent.SetDestination(d);
            AudioManager.Instance.PlayRandom2DClip(burningClips);

            while (Vector3.Distance(navMeshAgent.destination, transform.position) > 1f)
                yield return new WaitForEndOfFrame();

            navMeshAgent.velocity = Vector3.zero;
        }

        StartCoroutine(TriggerDeath());
    }

    IEnumerator TriggerDeath()
    {
        LevelController.Instance.IsGameOver = true;

        state = State.Diying;
        thoughtBubble.DisableThought();
        targetIcon.DisableTarget();
        fireParticleSystem?.Stop();

        // Death Cam!
        deathCamera.enabled = true;
        LevelController.Instance.MainCamera = deathCamera;
        Camera.main.enabled = false;
        
        SetAnimatorToCurrentState();

        animController.SetTrigger("Die");
        var s = AudioManager.Instance.PlayRandom2DClip(deathClips);
        yield return new WaitForSeconds(s.clip.length);

        // Hold for a seconds before restarting
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    /// <summary>
    /// Returns the closest resource to the sheep
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    GameObject GetClosestResource(Resource type)
    {
        var resources = GetAllResourcesByType(type);

        var distance = Mathf.Infinity;
        GameObject resource = null;

        foreach (var r in resources)
        {
            var d = Vector3.Distance(r.transform.position, transform.position);

            if (d < distance)
            {
                distance = d;
                resource = r;
            }
        }

        return resource;
    }

    /// <summary>
    /// Chooses a random resource from all resources
    /// </summary>
    /// <returns></returns>
    GameObject GetRandomResource()
    {
        var max = Enum.GetNames(typeof(Resource)).Length;
        var i = Random.Range(0, max);
        var type = (Resource)i;

        return GetRandomResource(type);
    }

    /// <summary>
    /// Randomly choses a resource by type to return
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    GameObject GetRandomResource(Resource type)
    {
        var resources = GetAllResourcesByType(type);
        var i = Random.Range(0, resources.Count);
        return i < resources.Count ? resources[i] : null;
    }

    /// <summary>
    /// Returns all available resources of the give type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    List<GameObject> GetAllResourcesByType(Resource type)
    {
        List<GameObject> resources = new List<GameObject>();
        switch (type)
        {
            case Resource.Food:
                resources = FindObjectsOfType<CropTile>().Select(n => n.gameObject).ToList();
                break;

            case Resource.Water:
                resources = FindObjectsOfType<WaterResource>().Select(n => n.gameObject).ToList();
                break;

            case Resource.Rest:
                resources = FindObjectsOfType<RestResource>().Select(n => n.gameObject).ToList();
                break;
        }
        return resources;
    }

    public void OnPuddleEnter()
    {
        if (state == State.Walking)
            navMeshAgent.speed = puddleSpeed;
    }

    public void OnPuddleExit()
    {
        if (!IsRunning)
            navMeshAgent.speed = walkingSpeed;
    }

    public void OnPuddleStruckByLightning()
    {
        StruckedByLightning();
    }

    //Vector3 RandomNavmeshLocation(float radius)
    //{
    //    Vector3 randomDirection = Random.insideUnitSphere * radius;
    //    randomDirection += transform.position;
    //    NavMeshHit hit;
    //    Vector3 finalPosition = Vector3.zero;
    //    if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
    //    {
    //        finalPosition = hit.position;
    //    }
    //    return finalPosition;
    //}

    //void LookAtDestination()
    //{
    //    var dir = transform.position - navMeshAgent.destination;
    //    var magnitude = dir.magnitude;
    //    var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    //    curRotationAngle = Mathf.LerpAngle(curRotationAngle, angle, Time.deltaTime * rotationSpeed * magnitude);
    //}
}
