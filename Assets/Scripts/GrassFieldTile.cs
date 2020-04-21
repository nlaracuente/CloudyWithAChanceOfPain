using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassFieldTile : MonoBehaviour, IAttackable, IDousable, IBurnable, IShockable
{
    enum State
    {
        Grass,
        Fire,
        Puddle,
        Shocked,
    }

    [SerializeField] Vector3 shockPosition;
    [SerializeField] Vector3 shockScale;

    [SerializeField]
    FireSpreader fireSpreader;

    [SerializeField]
    LightningSpreader lightningSpreader;

    [SerializeField]
    FieldTileEffects fieldEffects;

    [SerializeField]
    List<AudioClipInfo> fireClipInfos;

    [SerializeField]
    List<AudioClipInfo> puddleClipInfos;

    [SerializeField]
    List<AudioClipInfo> firePutOutClipInfos;

    [SerializeField]
    List<AudioClipInfo> shockClipInfos;

    IEnumerator curRoutine;

    State state = State.Grass;

    bool IsInvisible { get; set; }

    private void Update()
    {
        if (LevelController.Instance.IsGameOver)
            StopAllCoroutines();
    }

    public void RainedOn()
    {
        if (curRoutine != null)
            StopCoroutine(curRoutine);

        switch (state)
        {
            case State.Grass:
                state = State.Puddle;
                break;

            case State.Fire:
                state = State.Grass;
                fieldEffects.PlayParticleEffect(FieldTileEffects.Effect.Smoke);
                AudioManager.Instance.PlayRandom2DClip(firePutOutClipInfos);
                //StartCoroutine(DousedRoutine());
                break;
        }

        AudioManager.Instance.PlayRandom2DClip(puddleClipInfos);
        fieldEffects.ShowEffect(FieldTileEffects.Effect.Puddle);        
    }

    IEnumerator DousedRoutine()
    {
        IsInvisible = true;
        yield return new WaitForSeconds(LevelController.Instance.TimeSheepIsInvincible);
        IsInvisible = false;
    }

    public void StruckedByLightning()
    {
        if (IsInvisible)
            return;

        switch (state)
        {
            case State.Grass:
                state = State.Fire;
                fieldEffects.ShowEffect(FieldTileEffects.Effect.Fire);
                AudioManager.Instance.PlayRandom2DClip(fireClipInfos);

                if (curRoutine != null)
                    StopCoroutine(curRoutine);
                    
                curRoutine = SpreadFireRoutine();
                StartCoroutine(curRoutine);
                break;

            case State.Puddle:
                TriggerLightningSpread();                
                break;
        }
    }

    IEnumerator SpreadFireRoutine()
    {
        yield return new WaitForSeconds(LevelController.Instance.TimeBeforeFireSpreads);

        if(state == State.Fire)
            fireSpreader?.Spread();
    }

    /// <summary>
    /// Mainly to play the shock sound once 
    /// and anything else we want to happen only once
    /// </summary>
    void TriggerLightningSpread()
    {
        fieldEffects.ShowEffect(FieldTileEffects.Effect.Shock);
        AudioManager.Instance.PlayRandom2DClip(shockClipInfos);
        Shocked();
    }

    /// <summary>
    /// The same as being struck by lightning
    /// </summary>
    public void Burn()
    {
        // Only if grass because if it has water on it we don't want to spread
        if(state == State.Grass)
            StruckedByLightning();
    }

    public void Shocked()
    {
        if (state != State.Puddle || state == State.Shocked)
            return;

        state = State.Shocked;

        // Skeep Spreading       
        curRoutine = ShockedRoutine();
        StartCoroutine(curRoutine);
    }

    /// <summary>
    /// Show the effect
    /// Enable the trigger to "shock" during that time
    /// Disable it all
    /// </summary>
    /// <returns></returns>
    IEnumerator ShockedRoutine()
    {
        state = State.Shocked;
        lightningSpreader?.Spread();
        fieldEffects.PlayParticleEffect(FieldTileEffects.Effect.Shock);
        yield return new WaitForSeconds(LevelController.Instance.TimeShockIsInEffect);

        state = State.Grass;
        fieldEffects.DisableEffects();
    }
}
