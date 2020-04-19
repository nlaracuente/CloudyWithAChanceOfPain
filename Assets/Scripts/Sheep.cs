using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(ThoughtBubble))]
public class Sheep : MonoBehaviour, IAttackable, IDousable
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

    enum Resource
    {
        Food,
        Water,
        Rest,
    }

    [SerializeField]
    NavMeshAgent navMeshAgent;

    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float runningSpeed = 5f;
    [SerializeField] float timeBurning = 5f;

    [SerializeField]
    Animator animController;

    // Particle Effects
    [SerializeField] ParticleSystem fireParticleSystem;

    // Sounds Clips
    [SerializeField] List<AudioClipInfo> eatingClips;
    [SerializeField] List<AudioClipInfo> drinkingClips;
    [SerializeField] List<AudioClipInfo> restingClips;
    [SerializeField] List<AudioClipInfo> burningClips;

    IEnumerator activeCoroutine;
    IEnumerator routineBeforeBurning;
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
    
    private void Awake()
    {
        // This should work but it is not so going the longer way
        // navMeshAgent = navMeshAgent ?? GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        thoughtBubble = GetComponentInChildren<ThoughtBubble>();
    }

    /// <summary>
    /// Initiates routine
    /// </summary>
    void Start()
    {
        thoughtBubble.DisableThought();
        curResource = Resource.Rest;
        ChangeToNextState();
    }

    /// <summary>
    /// Changes the completed routine with the routine that follows it
    /// </summary>
    void ChangeToNextState()
    {
        switch (state)
        {
            // Done Walking: Consume Resource
            case State.Walking:
                var canConsume = curConsumable != null && curConsumable.IsConsumable;

                thoughtBubble.DisableThought();

                if (curResource == Resource.Food)
                    SwitchCoroutine(EatingRoutine(canConsume));
                else if (curResource == Resource.Water)
                    SwitchCoroutine(DrinkingRoutine(canConsume));
                else
                    SwitchCoroutine(RestingRoutine(canConsume));
                break;

            // Done consuming resource: Fetch the next one
            // To add varierity, rest areas are chosen at random
            case State.Resting:
            case State.Eating:
            case State.Drinking:
                SwitchToNextResource();

                GameObject go;
                //if (curResource == Resource.Rest)
                    go = GetRandomResource(curResource);
                //else
                  //  go = GetClosestResource(curResource);

                curConsumable = go.GetComponent<IConsumable>();
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
                thoughtBubble.SetThought(ThoughtBubble.Thought.Water);
                break;

            case Resource.Water:
                curResource = Resource.Rest;
                thoughtBubble.SetThought(ThoughtBubble.Thought.Rest);
                break;

            case Resource.Rest:
                curResource = Resource.Food;
                thoughtBubble.SetThought(ThoughtBubble.Thought.Food);
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
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = newRoutine;
        StartCoroutine(activeCoroutine);

        SetAnimatorToCurrentState();
    }

    void SetAnimatorToCurrentState()
    {
        animController.SetBool("Idling", state == State.Idling || state == State.Resting);
        animController.SetBool("Walking", state == State.Walking);
        animController.SetBool("Running", state == State.Running || state == State.Burning);
        animController.SetBool("Kneeling", state == State.Kneeling);
        animController.SetBool("Consuming", state == State.Eating || state == State.Drinking);
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

    public void StruckedByLightning()
    {
        if (state == State.Diying || state == State.Burning)
            return;

        routineBeforeBurning = activeCoroutine;
        SwitchCoroutine(BurningRoutine(routineBeforeBurning));
    }

    public void RainedOn()
    {
        if (state == State.Diying)
            return;

        if (state == State.Burning)
        {
            TransitionFromRunningToIdel(routineBeforeBurning);
        }
        else if (state != State.Idling)
        {
            SwitchCoroutine(RestingRoutine(false));
        }
    }

    /// <summary>
    /// Wait before resuming previous routine
    /// </summary>
    /// <returns></returns>
    IEnumerator IdlingRoutine(IEnumerator prevRoutine)
    {
        state = State.Idling;

        var info = AudioManager.Instance.GetRandomAudioClipInfo(restingClips);
        AudioManager.Instance.Play2DSound(info.clip, info.volume);
        yield return new WaitForSeconds(info.clip.length);

        SwitchCoroutine(prevRoutine);
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

        while (Vector3.Distance(navMeshAgent.destination, transform.position) > .01f)
        {
            yield return new WaitForEndOfFrame();
        }

        // Fully Stop
        navMeshAgent.velocity = Vector3.zero;
        yield return new WaitForEndOfFrame();

        // TODO: Change to smooth look at
        transform.LookAt(curConsumable.gameObject.transform);
        yield return new WaitForEndOfFrame();

        // Continue
        ChangeToNextState();
    }

    /// <summary>
    /// Essentially idleling
    /// </summary>
    /// <returns></returns>
    IEnumerator RestingRoutine(bool canConsume)
    {
        state = State.Resting;

        var info = AudioManager.Instance.GetRandomAudioClipInfo(restingClips);
        AudioManager.Instance.Play2DSound(info.clip, info.volume);
        yield return new WaitForSeconds(info.clip.length);

        ChangeToNextState();
    }

    /// <summary>
    /// Waiting until done eating
    /// </summary>
    /// <returns></returns>
    IEnumerator EatingRoutine(bool canConsume)
    {
        state = State.Eating;

        if (!canConsume)
        {
            // Wat to realize it cannot be consume and become sad
            yield return new WaitForSeconds(1f);
            thoughtBubble.SetThought(ThoughtBubble.Thought.Hurt);
            yield return new WaitForSeconds(2f);
        } 
        else
        {
            curConsumable.Consume();
            thoughtBubble.SetThought(ThoughtBubble.Thought.Happy);
            var info = AudioManager.Instance.GetRandomAudioClipInfo(eatingClips);
            AudioManager.Instance.Play2DSound(info.clip, info.volume);
            yield return new WaitForSeconds(info.clip.length);
        }

        ChangeToNextState();
    }

    /// <summary>
    /// Wait until done drinking
    /// </summary>
    /// <returns></returns>
    IEnumerator DrinkingRoutine(bool canConsume)
    {
        state = State.Drinking;

        if (!canConsume)
        {
            // Wat to realize it cannot be consume and become sad
            yield return new WaitForSeconds(1f);
            thoughtBubble.SetThought(ThoughtBubble.Thought.Hurt);
        }
        else
        {
            var info = AudioManager.Instance.GetRandomAudioClipInfo(drinkingClips);
            AudioManager.Instance.Play2DSound(info.clip, info.volume);
            yield return new WaitForSeconds(info.clip.length);
            curConsumable.Consume();
        }

        ChangeToNextState();
    }

    /// <summary>
    /// Randomly runs around the map until times up
    /// </summary>
    /// <returns></returns>
    IEnumerator BurningRoutine(IEnumerator prevRoutine)
    {
        state = State.Burning;
        thoughtBubble.DisableThought();
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

        TransitionFromRunningToIdel(prevRoutine);
    }

    void TransitionFromRunningToIdel(IEnumerator prevRoutine)
    {
        fireParticleSystem?.Stop();
        navMeshAgent.speed = walkingSpeed;
        state = State.Idling;
        SwitchCoroutine(IdlingRoutine(prevRoutine));
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
