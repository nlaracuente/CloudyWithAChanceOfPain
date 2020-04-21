using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class WolfSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoints;

    [SerializeField] GameObject wolvePrefab;
    [SerializeField] float timeBeforeFirstWolf = 15f;
    [SerializeField] float timeBetweenWaves = 30f;
    [SerializeField] int totalWaves = 10;
    [SerializeField] int totalWolvesPerWaves = 1;
    [SerializeField] float totalWolvesPerWavesMultiplier = .2f;   

    [SerializeField, Tooltip("Time to 'stagger' wolf spawning in a single wave")]
    float timeBeforeSpawningNextWolf = .1f;

    List<Transform> spawningPoints;
    int seed;

    void Start()
    {
        if (wolvePrefab == null)
        {
            Debug.Log($"{name}: has no wolf prefab found");
            return;
        }

        var rand = new System.Random(Guid.NewGuid().GetHashCode());
        seed = rand.Next();

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
        yield return new WaitForSeconds(timeBeforeFirstWolf);

        var waveNumber = 1;
        while (waveNumber < totalWaves && !LevelController.Instance.IsGameOver)
        {
            // Wave prep
            var totalWolves = Mathf.FloorToInt((waveNumber * totalWolvesPerWaves) * totalWolvesPerWavesMultiplier);
            totalWolves = Mathf.Clamp(totalWolves, totalWolvesPerWaves, spawningPoints.Count - 1);
            
            // Randomize Spawning Order
            var randPoints = ArrayUtility.ShuffleArray(spawningPoints.ToArray(), seed);

            var spawned = 0;
            while(spawned < totalWolves)
            {
                var point = randPoints[spawned++];
                Instantiate(wolvePrefab, point.position, point.rotation, transform);
                yield return new WaitForSeconds(timeBeforeSpawningNextWolf);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        if (waveNumber == totalWaves)
            LevelController.Instance.AllWolvesSpawned = true;
    }
}