using System.Collections.Generic;
using UnityEngine;

public class SheepSignController : Singleton<SheepSignController>
{
    Queue<SheepSign> signs;

    void Start()
    {
        signs = new Queue<SheepSign>();

        for (var i = 0; i < transform.childCount; i++)
        {
            var sign = transform.GetChild(i).GetComponent<SheepSign>();

            if(sign != null && sign.gameObject.activeSelf)
                signs.Enqueue(sign);
        }
    }

    public void SheepSpawned(Sheep sheep)
    {
        if (signs.Count < 1)
            return;

        var sign = signs.Dequeue();        
        sign.SheepSpanwed(sheep);
    }
}
