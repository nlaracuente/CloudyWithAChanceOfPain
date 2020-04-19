using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(ThoughtBubble))]
public class Sheep : MonoBehaviour
{
    enum Routine
    {
        Resting,
        Walking,
        Eating,
        Drinking,
    };

    enum Resource
    {
        Food,
        Water,
        Rest,
    }

    [SerializeField]
    NavMeshAgent navMeshAgent;

    // Sounds Clips
    [SerializeField] List<AudioClipInfo> eatingClips;
    [SerializeField] List<AudioClipInfo> drinkingClips;
    [SerializeField] List<AudioClipInfo> restingClips;
        

    IEnumerator activeCoroutine;
    Routine curRoutine;

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
        CurrentRoutineFinished();
    }

    /// <summary>
    /// Changes the completed routine with the routine that follows it
    /// </summary>
    void CurrentRoutineFinished()
    {
        

        switch (curRoutine)
        {
            // Done Walking: Consume Resource
            case Routine.Walking:
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
            case Routine.Resting:
            case Routine.Eating:
            case Routine.Drinking:
                SwitchToNextResource();

                GameObject go;
                //if (curResource == Resource.Rest)
                    go = GetRandomResourceByType(curResource);
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
    /// Randomly choses a resource by type to return
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    GameObject GetRandomResourceByType(Resource type)
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

    /// <summary>
    /// Walks towards the given destination
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    IEnumerator WalkRoutine(Vector3 destination)
    {
        curRoutine = Routine.Walking;
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
        CurrentRoutineFinished();
    }

    /// <summary>
    /// Essentially idleling
    /// </summary>
    /// <returns></returns>
    IEnumerator RestingRoutine(bool canConsume)
    {
        curRoutine = Routine.Resting;

        var info = AudioManager.Instance.GetRandomAudioClipInfo(restingClips);
        AudioManager.Instance.Play2DSound(info.clip, info.volume);
        yield return new WaitForSeconds(info.clip.length);

        CurrentRoutineFinished();
    }

    /// <summary>
    /// Waiting until done eating
    /// </summary>
    /// <returns></returns>
    IEnumerator EatingRoutine(bool canConsume)
    {
        curRoutine = Routine.Eating;

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

        CurrentRoutineFinished();
    }

    /// <summary>
    /// Wait until done drinking
    /// </summary>
    /// <returns></returns>
    IEnumerator DrinkingRoutine(bool canConsume)
    {
        curRoutine = Routine.Drinking;

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
            var info = AudioManager.Instance.GetRandomAudioClipInfo(drinkingClips);
            AudioManager.Instance.Play2DSound(info.clip, info.volume);
            yield return new WaitForSeconds(info.clip.length);
        }

        CurrentRoutineFinished();
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
