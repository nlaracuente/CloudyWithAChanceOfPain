using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponent<IPuddleInteractible>();
        p?.OnPuddleEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        var p = other.GetComponent<IPuddleInteractible>();
        p?.OnPuddleExit();
    }
}
