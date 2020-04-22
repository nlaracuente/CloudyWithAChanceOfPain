using System.Collections;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject sheepPrefab;
    [SerializeField] float timeBetweenEachSheepSpawning = 30f;
    [SerializeField] int totalSheepsToSpawn = 10;

    public int TotalSheeps { get { return totalSheepsToSpawn; } }

    void Start()
    {
        if(sheepPrefab == null)
        {
            Debug.Log($"{name}: has not been assigned a prefab to spawn");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        var total = 0;
        while (total < totalSheepsToSpawn && !LevelController.Instance.IsGameOver)
        {
            yield return new WaitForSeconds(timeBetweenEachSheepSpawning);
            var sheep = Instantiate(sheepPrefab, spawnPoint.position, spawnPoint.rotation, transform).GetComponent<Sheep>();
            sheep.PlayRandomSheepNoise();
            
            total++;
        }

        if (!LevelController.Instance.IsGameOver)
            LevelController.Instance.AllSheepsSpawned = true;
    }
}
