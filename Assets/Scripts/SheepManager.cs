using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class SheepManager : Singleton<SheepManager>
{
    List<Sheep> sheeps;
    Queue<Sheep> sheepsToTarget;

    List<Sheep> Sheeps
    {
        get
        {
            sheeps = sheeps.Where(s => s != null && !s.IsDead).ToList();
            return sheeps;
        }
    }

    /// <summary>
    /// Count of remaining sheeps
    /// </summary>
    public int TotalSheeps 
    { 
        get 
        {
            return Sheeps.Count;
        } 
    }

    int Seed
    {
        get
        {
            var rand = new System.Random(Guid.NewGuid().GetHashCode());
            return rand.Next();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sheeps = new List<Sheep>();
    }

    void BuildQueue()
    {
        sheepsToTarget = new Queue<Sheep>(ArrayUtility.ShuffleArray(Sheeps.ToArray(), Seed));
    }

    public void AddSheep(Sheep sheep)
    {
        if (Sheeps.Contains(sheep))
            return;

        Sheeps.Add(sheep);
        BuildQueue();        
    }   

    public void SheepDied(Sheep sheep, Wolf wolf = null)
    {
        Sheeps.Remove(sheep);

        // Clean up
        BuildQueue();

        if (TotalSheeps > 0)
            sheep.Die();
        else
            sheep.TriggerGameOverSequence(wolf);
    }

    public Sheep GetSheepToTarget()
    {
        if (TotalSheeps < 1)
            return null;

        if (sheepsToTarget.Count < 1)
            BuildQueue();

        return sheepsToTarget.Dequeue();
    }
}
