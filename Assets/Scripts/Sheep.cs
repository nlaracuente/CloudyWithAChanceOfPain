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
public class Sheep : MonoBehaviour, IAttackable, IDousable, IPuddleInteractible, IBurnable, IShockable
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
        Recovering,
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
    [SerializeField] float timeBeforeDisplayResourceNotFound = 1f;

    // Particle Effects
    [SerializeField] ParticleSystem fireParticleSystem;
    [SerializeField] ParticleSystem smokeParticleSystem;

    // Sounds Clips
    [SerializeField] List<AudioClipInfo> eatingClips;
    [SerializeField] List<AudioClipInfo> drinkingClips;
    [SerializeField] List<AudioClipInfo> restingClips;
    [SerializeField] List<AudioClipInfo> burningClips;
    [SerializeField] List<AudioClipInfo> deathClips;

    IEnumerator curRoutine;
    State state;

    Resource curResourceType;
    BaseResource curResource;

    Transform curResourceAccessPoint;

    /// <summary>
    /// The current consumable the sheep has targeted
    /// </summary>
    BaseResource curConsumable;

    ThoughtBubble thoughtBubble;
    ThoughtBubble.Thought curThought;

    public bool IsRunning { get { return state == State.Running || state == State.Burning; } }
    public bool IsNotMoving { get { return navMeshAgent.velocity == Vector3.zero; } }

    public int Strikes { get; private set; } = 0;
    public bool ResourcesLow { get { return Strikes == totalStrikes; } }
    public bool IsInvincible { get; private set; }
    public bool IsDead { get { return state == State.Diying; } }

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
        SheepManager.Instance.AddSheep(this);
        thoughtBubble.DisableThought();

        state = State.Resting;
        curResourceType = Resource.Rest;
        AutoChangeState();
    }

    public void PlayRandomSheepNoise()
    {
        AudioManager.Instance.PlayRandom2DClip(restingClips);
    }

    /// <summary>
    /// Changes the completed routine to the routine that follows it
    /// </summary>
    void AutoChangeState(bool switchResource = true)
    {
        // Ignore when dead or game over
        if (LevelController.Instance.IsGameOver || IsDead)
            return;

        if (Strikes > totalStrikes)
        {
            SheepManager.Instance.SheepDied(this);
            return;
        }

        switch (state)
        {
            // Done Walking: Consume Resource
            case State.Walking:
                if (curResource.ResourceType == Resource.Rest)
                    SwitchCoroutine(RestingRoutine());
                else
                    SwitchCoroutine(ConsumeResourceRoutine(curResource));
                break;

            // Done consuming resource: Fetch the next one
            // To add varierity, rest areas are chosen at random
            case State.Resting:
            case State.Eating:
            case State.Drinking:
            case State.Idling:
            case State.Recovering:

                // Pursue the current resource - just target a new one
                if (switchResource)
                    SwitchToNextResource();

                curResource = GetAvailableResource(curResourceType);
                SwitchCoroutine(WalkRoutine());
                break;
        }
    }

    /// <summary>
    /// Updates the current resource to the next resource to go after
    /// </summary>
    void SwitchToNextResource()
    {
        switch (curResourceType)
        {
            case Resource.Food:
                curResourceType = Resource.Water;
                break;

            case Resource.Water:
                curResourceType = Resource.Rest;
                break;

            case Resource.Rest:
                curResourceType = Resource.Food;
                break;
        }
    }

    /// <summary>
    /// Stops the current routine to enable the new one
    /// </summary>
    /// <param name="newRoutine"></param>
    void SwitchCoroutine(IEnumerator newRoutine)
    {
        // Ignore when dead or game over
        if (LevelController.Instance.IsGameOver || IsDead)
            return;

        if (curRoutine != null)
            StopCoroutine(curRoutine);

        curRoutine = newRoutine;
        StartCoroutine(curRoutine);
        SetAnimatorToCurrentState();
    }

    void SetAnimatorToCurrentState()
    {
        animController.SetBool("Idling", state == State.Idling);
        animController.SetBool("Walking", state == State.Walking);
        animController.SetBool("Running", state == State.Running || state == State.Burning || state == State.Recovering);
        animController.SetBool("Kneeling", state == State.Kneeling || state == State.Resting);
        animController.SetBool("Consuming", state == State.Eating || state == State.Drinking);
    }    

    public void StruckedByLightning()
    {
        if (LevelController.Instance.IsGameOver || IsDead || state == State.Burning)
            return;

        if (!IsInvincible)
            SwitchCoroutine(BurningRoutine());
    }

    public void RainedOn()
    {
        // Ignore when dead or game over
        if (LevelController.Instance.IsGameOver || IsDead)
            return;

        // Already rained on
        if (state == State.Idling || state == State.Resting || state == State.Recovering)
            return;

        // Stop immedeatly
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        // Restore since it was reset
        curResource?.SetAccessPoint(curResourceAccessPoint);

        var go = Cloud.Instance.GetGrassUnderMouse();
        go.GetComponent<IDousable>()?.RainedOn();

        if (state != State.Burning)
            SwitchCoroutine(IdlingRoutine());
        else
            SwitchCoroutine(RecoveryRoutine(go.transform.position));     
    }

    /// <summary>
    /// Routine for recovering from being burned where the sheep is invulnerable 
    /// as well as makes it wait a bit before trying to at it again
    /// </summary>
    /// <returns></returns>
    IEnumerator RecoveryRoutine(Vector3 destination)
    {
        state = State.Recovering;

        IsInvincible = true;
        fireParticleSystem?.Stop();
        smokeParticleSystem?.Play();

        // Run to the mouse is on since it should be safe now that there's water on it
        navMeshAgent.speed = runningSpeed;
        navMeshAgent.isStopped = false;

        var go = Cloud.Instance.GetGrassUnderMouse();
        navMeshAgent.SetDestination(destination);

        // Wait to reach the destination
        while (Vector3.Distance(navMeshAgent.destination, transform.position) > .01f)
            yield return new WaitForEndOfFrame();

        // Fully Stop
        animController.SetBool("Idling", true);        
        navMeshAgent.velocity = Vector3.zero;
        yield return new WaitForEndOfFrame();

        if (curConsumable != null) 
        {
            transform.LookAt(curConsumable.gameObject.transform);
            yield return new WaitForEndOfFrame();
        }

        IsInvincible = false;

        SwitchCoroutine(IdlingRoutine());
    }

    /// <summary>
    /// Wait before resuming previous routine
    /// </summary>
    /// <returns></returns>
    IEnumerator IdlingRoutine()
    {
        if (!IsInvincible)
        {
            state = State.Idling;
            navMeshAgent.speed = walkingSpeed;
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;

            var info = AudioManager.Instance.GetRandomAudioClipInfo(restingClips);
            AudioManager.Instance.Play2DSound(info.clip, info.volume);
            yield return new WaitForSeconds(info.clip.length);

            // Ensures the sheep can walk again
            navMeshAgent.isStopped = false;

            AutoChangeState(false);
        } 
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

        // Keep it while resources are low
        if(!ResourcesLow)
            thoughtBubble.DisableThought();
    }

    /// <summary>
    /// Walks towards the given destination
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    IEnumerator WalkRoutine()
    {
        curResourceAccessPoint = curResource.GetAccessPoint(ResourcesLow);

        state = State.Walking;
        navMeshAgent.SetDestination(curResourceAccessPoint.position);
        navMeshAgent.isStopped = false;

        ThoughtBubble.Thought thought = curThought;
        switch(curResourceType)
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
        transform.LookAt(curResource.gameObject.transform);
        yield return new WaitForEndOfFrame();

        curResource.SetAccessPoint(curResourceAccessPoint);

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

    IEnumerator ConsumeResourceRoutine(BaseResource resource)
    {
        state = resource.ResourceType == Resource.Food
                ? State.Eating
                : State.Drinking;

        // To track whether the resource was consumed and we can change it
        // Since the state of the "IsConsumable" will change once consumed
        bool consumed = true;

        if (!resource.IsConsumable)
        {
            consumed = false;
            yield return new WaitForSeconds(timeBeforeDisplayResourceNotFound);
            yield return StartCoroutine(ShowThoughtBubbleRoutine(ThoughtBubble.Thought.Hurt));
            Strikes++;
        }
        else
        {
            // Prevents others from eaiting it
            resource.SetAsNotConsumable();

            // Reset Strikes
            Strikes = 0;
            var bubbleRoutine = ShowThoughtBubbleRoutine(ThoughtBubble.Thought.Happy);
            StartCoroutine(bubbleRoutine);

            var infos = resource.ResourceType == Resource.Food
                        ? eatingClips
                        : resource.ResourceType == Resource.Water
                        ? drinkingClips
                        : restingClips;

            var s = AudioManager.Instance.PlayRandom2DClip(infos);
            yield return new WaitForSeconds(s.clip.length);
            resource.Consume();

            StopCoroutine(bubbleRoutine);
        }

        AutoChangeState(consumed);
    }

    /// <summary>
    /// Randomly runs around the map until times up
    /// </summary>
    /// <returns></returns>
    IEnumerator BurningRoutine()
    {
        // Reset access point
        curResource.SetAccessPoint(curResourceAccessPoint);

        state = State.Burning;
        thoughtBubble.DisableThought();
        var t = Time.time + timeBurning;

        fireParticleSystem?.Play();
        navMeshAgent.speed = runningSpeed;
        navMeshAgent.isStopped = false;

        while (Time.time < t)
        {
            // Get Random destination
            var accessPoint = GetRandomAvailableResource().GetAccessPoint();

            // Face it and book it!
            transform.LookAt(accessPoint.position);
            navMeshAgent.SetDestination(accessPoint.position);
            var audio = AudioManager.Instance.PlayRandom2DClip(burningClips);

            // But only run while there is still time
            while (!IsDead && Time.time < t && Vector3.Distance(navMeshAgent.destination, transform.position) > 1f)
                yield return new WaitForEndOfFrame();

            if(audio != null)
                audio.Stop();

            // We are done using it so add it back
            curResource.SetAccessPoint(accessPoint);
            navMeshAgent.velocity = Vector3.zero;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        SheepManager.Instance.SheepDied(this);
    }

    public void TriggerGameOverSequence(Wolf wolf = null)
    {
        if(!IsDead)
            StartCoroutine(TriggerDeath(true, wolf));
    }

    public void Die()
    {
        if (!IsDead)
            StartCoroutine(TriggerDeath());
    }

    IEnumerator TriggerDeath(bool isGameOver = false, Wolf wolf = null)
    {
        LevelController.Instance.IsGameOver = isGameOver;

        state = State.Diying;
        thoughtBubble.DisableThought();

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        // Restore last access point
        curResource.SetAccessPoint(curResourceAccessPoint);

        // Death by fire - keep smoking
        // Fire must have been put out
        if (fireParticleSystem.isPlaying)
        {
            var main = smokeParticleSystem.main;
            main.loop = true;

            smokeParticleSystem?.Play();
            fireParticleSystem?.Stop();
        }

        // The camera is based on how this sheep died
        if (isGameOver) 
        {
            if (wolf != null)
                wolf.SetKillCamera();
            else
            {
                deathCamera.enabled = true;
                LevelController.Instance.MainCamera = deathCamera;
                Camera.main.enabled = false;
            }
        }
        
        SetAnimatorToCurrentState();

        animController.SetTrigger("Die");
        var s = AudioManager.Instance.PlayRandom2DClip(deathClips);
        yield return new WaitForSeconds(s.clip.length);

        // Hold for a seconds before restarting
        yield return new WaitForSeconds(1f);

        if (isGameOver)
            GameManager.Instance.GameOver();
        else
            Destroy(gameObject);
    }

    ///// <summary>
    ///// Returns the closest resource to the sheep
    ///// </summary>
    ///// <param name="type"></param>
    ///// <returns></returns>
    //IConsumable GetClosestConsumable(Resource type)
    //{
    //    IConsumable consumable = curConsumable;
    //    var consumables = GetAllConsumables(type);
    //    var distance = Mathf.Infinity;        

    //    foreach (var c in consumables)
    //    {
    //        var d = Vector3.Distance(c.gameObject.transform.position, transform.position);

    //        if (d < distance)
    //        {
    //            distance = d;
    //            consumable = c;
    //        }
    //    }

    //    return consumable;
    //}

    /// <summary>
    /// Chooses a random resource from all resources
    /// </summary>
    /// <returns></returns>
    BaseResource GetRandomAvailableResource()
    {
        var max = Enum.GetNames(typeof(Resource)).Length;
        var i = Random.Range(0, max);
        var type = (Resource)i;

        return GetAvailableResource(type);
    }

    /// <summary>
    /// Randomly choses a resource by type to return
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    BaseResource GetAvailableResource(Resource type)
    {
        curResource = ResourceManager.Instance.GetAvailableResource(type, curResource);
        return curResource;
    }

    ///// <summary>
    ///// Returns all available resources of the give type
    ///// </summary>
    ///// <param name="type"></param>
    ///// <returns></returns>
    //List<IConsumable> GetAllConsumables(Resource type)
    //{
    //    List<IConsumable> resources = new List<IConsumable>();
    //    switch (type)
    //    {
    //        case Resource.Food:
    //            resources = FindObjectsOfType<CropTile>()
    //                        .Select(n => n.gameObject.GetComponent<IConsumable>()).ToList();
    //            break;

    //        case Resource.Water:
    //            resources = FindObjectsOfType<WaterResource>()
    //                        .Select(n => n.gameObject.GetComponent<IConsumable>()).ToList();
    //            break;

    //        case Resource.Rest:
    //            resources = FindObjectsOfType<RestResource>()
    //                        .Select(n => n.gameObject.GetComponent<IConsumable>()).ToList();
    //            break;
    //    }
    //    return resources;
    //}

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

    public void Burn()
    {
        StruckedByLightning();
    }

    public void Shocked()
    {
        StruckedByLightning();
    }
}
