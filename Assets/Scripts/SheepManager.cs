using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class SheepManager : Singleton<SheepManager>
{
    List<Sheep> sheeps;
    List<Sheep> sheepsToTarget = new List<Sheep>();

    // Start is called before the first frame update
    void Start()
    {
        sheeps = FindObjectsOfType<Sheep>().ToList();
        sheepsToTarget = new List<Sheep>(sheeps);
    }

    public void AddSheep(Sheep sheep)
    {
        if (!sheeps.Contains(sheep))
            sheeps.Add(sheep);

        if (!sheepsToTarget.Contains(sheep))
            sheepsToTarget.Add(sheep);
    }

    public void SheepDied(Sheep sheep)
    {
        // Already handled
        if (!sheeps.Contains(sheep))
            return;

        if (sheeps.Count <= 1)
            sheep.TriggerGameOverSequence();
        else
            sheep.Die();

        sheeps.Remove(sheep);
    }

    public Sheep GetTargetSheep()
    {
        if (sheepsToTarget.Count < 1)
            return null;

        var sheep = sheepsToTarget[0];
        sheepsToTarget.RemoveAt(0);
        return sheep;
    }

    public void SheepNoLongerTargerted(Sheep sheep)
    {
        if (!sheepsToTarget.Contains(sheep))
            sheepsToTarget.Add(sheep);
    }
}
