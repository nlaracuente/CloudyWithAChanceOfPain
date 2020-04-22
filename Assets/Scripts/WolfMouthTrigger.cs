using UnityEngine;

public class WolfMouthTrigger : MonoBehaviour
{
    Collider trigger;
    public Sheep SheepInTrigger { get; private set; }

    public bool EnableTrigger { set { trigger.enabled = value; } }


    private void Start()
    {
        trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Sheep>() != null)
            SheepInTrigger = other.GetComponent<Sheep>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Sheep>() != null)
            SheepInTrigger = null;
    }
}
