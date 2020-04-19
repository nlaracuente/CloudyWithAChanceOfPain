using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassFieldTile : MonoBehaviour, IAttackable, IDousable
{
    enum State
    {
        Grass,
        Fire,
        Puddle
    }
    [SerializeField]
    float burningTime = 3f;

    [SerializeField]
    FieldTileEffects fieldEffects;

    [SerializeField]
    List<AudioClipInfo> fireClipInfos;

    [SerializeField]
    List<AudioClipInfo> puddleClipInfos;

    [SerializeField]
    List<AudioClipInfo> firePutOutClipInfos;

    IEnumerator curRoutine;

    State state = State.Grass;

    public void RainedOn()
    {
        if (curRoutine != null)
            StopCoroutine(curRoutine);

        switch (state)
        {
            case State.Grass:
                state = State.Puddle;
                fieldEffects.ShowEffect(FieldTileEffects.Effect.Puddle);
                AudioManager.Instance.PlayRandom2DClip(puddleClipInfos);
                break;

            case State.Fire:
                state = State.Grass;
                fieldEffects.DisableEffects();
                AudioManager.Instance.PlayRandom2DClip(firePutOutClipInfos);
                break;
        }
    }

    public void StruckedByLightning()
    {
        switch (state)
        {
            case State.Grass:
                state = State.Fire;
                fieldEffects.ShowEffect(FieldTileEffects.Effect.Fire);

                if (curRoutine != null)
                    StopCoroutine(curRoutine);
                    
                curRoutine = FireEffectRoutine();
                StartCoroutine(curRoutine);
                break;

            case State.Puddle:
                state = State.Grass;
                fieldEffects.DisableEffects();
                break;
        }
    }

    IEnumerator FireEffectRoutine()
    {
        AudioManager.Instance.PlayRandom2DClip(fireClipInfos);
        yield return new WaitForSeconds(burningTime);
        fieldEffects.DisableEffects();
    }
}
