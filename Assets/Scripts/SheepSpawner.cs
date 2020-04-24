using System.Collections;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject sheepPrefab;
    [SerializeField] float timeTillFirstSpawn = 2f;
    [SerializeField] float timeBetweenEachSheepSpawning = 30f;
    [SerializeField] int totalSheepsToSpawn = 10;
    

    public int TotalSheeps { get { return totalSheepsToSpawn; } }

    void Start()
    {
        if(sheepPrefab == null)
            return;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(timeTillFirstSpawn);

        var total = 0;
        while (total < totalSheepsToSpawn && !LevelController.Instance.IsGameOver)
        {
            var sheep = Instantiate(sheepPrefab, spawnPoint.position, spawnPoint.rotation, transform).GetComponent<Sheep>();
            // sheep.gameObject.name = $"Sheep_{total}";
            sheep.PlayRandomSheepNoise();
            SheepSignController.Instance.SheepSpawned(sheep);
            SheepManager.Instance.AddSheep(sheep);
            total++;

            yield return new WaitForSeconds(timeBetweenEachSheepSpawning);
        }

        if (!LevelController.Instance.IsGameOver)
            LevelController.Instance.AllSheepsSpawned = true;
    }
}
