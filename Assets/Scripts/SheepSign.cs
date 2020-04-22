using UnityEngine;

public class SheepSign : MonoBehaviour
{
    [SerializeField] GameObject emptySign;
    [SerializeField] GameObject aliveSign;
    [SerializeField] GameObject deadSign;

    Sheep sheep;
    bool sheepHasDied = false;

    private void Start()
    {
        emptySign.SetActive(false);
        aliveSign.SetActive(false);
        deadSign.SetActive(false);
    }

    private void Update()
    {
        if (sheepHasDied)
            return;

        sheepHasDied = sheep != null && sheep.IsDead;

        emptySign.SetActive(sheep == null);
        aliveSign.SetActive(sheep != null && !sheep.IsDead);
        deadSign.SetActive(sheepHasDied);
    }

    public void SheepSpanwed(Sheep sheep)
    {
        this.sheep = sheep;
    }
}
