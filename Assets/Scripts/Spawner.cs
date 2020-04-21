using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    Transform spawnPoints;

    Queue<Transform> spawningPoints;

    [SerializeField, Tooltip("Prefab to spawn")]
    GameObject prefab;

    [SerializeField]
    float secondsUntilFirstSpawn = 15f;

    [SerializeField]
    float secondsBetweenSpawns = 30f;

    [SerializeField]
    int totalToSpawn = 10;

    [SerializeField]
    bool isWolveSpawner = false;

    [SerializeField]
    int decreaseTimeBy = 1;

    [SerializeField]
    int minimumTimeBetweenSpawns = 2;

    private void Start()
    {
        if(prefab == null)
        {
            Debug.Log($"{name}: no prefab found");
            return;
        }

        spawningPoints = new Queue<Transform>();
        for (var i = 0; i < spawnPoints.childCount; i++)
        {
            var child = spawnPoints.GetChild(i);
            spawningPoints.Enqueue(child);
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(secondsUntilFirstSpawn);

        var total = 0;
        while (total < totalToSpawn || LevelController.Instance.IsGameOver)
        {
            yield return new WaitForSeconds(secondsBetweenSpawns);

            var point = spawningPoints.Dequeue();

            Instantiate(prefab, point.position, point.rotation, transform);
            spawningPoints.Enqueue(point);
            total++;

            if (isWolveSpawner)
                secondsBetweenSpawns = Mathf.Clamp(secondsBetweenSpawns - decreaseTimeBy, minimumTimeBetweenSpawns, 100);
        }

        if (isWolveSpawner)
            LevelController.Instance.AllWolvesSpawned = true;
    }
}
