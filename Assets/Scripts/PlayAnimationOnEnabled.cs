using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayAnimationOnEnabled : MonoBehaviour
{
    [SerializeField]
    Animator anim;

    [SerializeField]
    string animTrigger = "Play";

    void OnEnable()
    {
        if(anim == null)
            anim = GetComponent<Animator>();

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        yield return new WaitForEndOfFrame();
        anim.SetTrigger(animTrigger);
    }
}
