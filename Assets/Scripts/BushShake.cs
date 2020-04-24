using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushShake : MonoBehaviour
{
    [SerializeField] Animator animator;
    Wolf wolf;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {   
        var otherWolf = other.GetComponent<Wolf>();
        if (otherWolf != null && wolf == null)
        {
            animator.SetTrigger("Shake");
            wolf = otherWolf;
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        var otherWolf = other.GetComponent<Wolf>();
        if (otherWolf != null && wolf == otherWolf)
        {
            wolf = null;
        }
    }
}
