using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class WolfSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoints;

    [SerializeField] GameObject wolvePrefab;
    [SerializeField] float timeBeforeFirstWolfSpawns = 15f;
    [SerializeField] float timeBetweenEachWaves = 30f;
    [SerializeField] int totalWaves = 10;
    [SerializeField] int totalWolvesPerWaves = 1;
    [SerializeField] float totalWolvesPerWavesMultiplier = .2f;   

    [SerializeField, Tooltip("Time to 'stagger' wolf spawning in a single wave")]
    float timeBeforeSpawningNextWolf = .1f;

    List<Transform> spawningPoints;

    // Randomize the seed each time
    int Seed { 
        get {
            var rand = new System.Random(Guid.NewGuid().GetHashCode());
            return rand.Next();
        } 
    }

    void Start()
    {
        if (wolvePrefab == null)
        {
            Debug.Log($"{name}: has no wolf prefab found");
            return;
        }

        spawningPoints = SetSpawningPoints(spawnPoints);
        StartCoroutine(SpawnRoutine());
    }

    List<Transform> SetSpawningPoints(Transform points)
    {
        var queue = new List<Transform>();
        for (var i = 0; i < points.childCount; i++)
        {
            var child = points.GetChild(i);
            queue.Add(child);
        }

        return queue;
    }

    IEnumerator SpawnRoutine()
    {        
        yield return new WaitForSeconds(timeBeforeFirstWolfSpawns);

        var waveNumber = 1;
        while (waveNumber < totalWaves && !LevelController.Instance.IsGameOver)
        {
            // Wave prep
            var totalWolves = waveNumber++ * totalWolvesPerWaves;// Mathf.FloorToInt((waveNumber * totalWolvesPerWaves) * totalWolvesPerWavesMultiplier);
            totalWolves = Mathf.Clamp(totalWolves, totalWolvesPerWaves, spawningPoints.Count - 1);
            
            // Randomize Spawning Order
            var randPoints = ArrayUtility.ShuffleArray(spawningPoints.ToArray(), Seed);

            var spawned = 0;
            while(spawned < totalWolves)
            {
                var point = randPoints[spawned++];
                Instantiate(wolvePrefab, point.position, point.rotation, transform).GetComponent<Wolf>();
                yield return new WaitForSeconds(timeBeforeSpawningNextWolf);
            }

            if(waveNumber < totalWaves)
                yield return new WaitForSeconds(timeBetweenEachWaves);
        }

        if (!LevelController.Instance.IsGameOver)
            LevelController.Instance.AllWolvesSpawned = true;
    }
}