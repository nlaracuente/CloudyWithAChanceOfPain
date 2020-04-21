using System.Collections;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    [SerializeField]
    protected Transform spawnPoint;

    [SerializeField]
    protected GameObject sheepPrefab;

    [SerializeField]
    protected float timeBeforeFirstSheep = 15f;

    [SerializeField]
    protected float timeBeforeSpawningNextWolf = 30f;

    [SerializeField]
    protected int totalSheepsToSpawn = 10;

    protected virtual void Start()
    {
        if(sheepPrefab == null)
        {
            Debug.Log($"{name}: has not been assigned a prefab to spawn");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    protected virtual IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(timeBeforeFirstSheep);

        var total = 0;
        while (total < totalSheepsToSpawn && !LevelController.Instance.IsGameOver)
        {
            yield return new WaitForSeconds(timeBeforeSpawningNextWolf);

            Instantiate(sheepPrefab, spawnPoint.position, spawnPoint.rotation, transform);
            total++;
        }

        LevelController.Instance.AllSheepsSpawned = true;
    }
}
