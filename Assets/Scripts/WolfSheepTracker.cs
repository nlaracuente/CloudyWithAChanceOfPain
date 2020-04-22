using UnityEngine;

public class WolfSheepTracker : MonoBehaviour
{
    Wolf wolf;

    // Start is called before the first frame update
    void Start()
    {
        wolf = GetComponentInParent<Wolf>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var sheep = other.GetComponent<Sheep>();
        if (sheep != null)
            wolf.SetNewTarget(sheep);
    }
}
