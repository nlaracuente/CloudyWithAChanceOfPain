using UnityEngine;

public class FieldTileEffects : MonoBehaviour
{
    public enum Effect
    {
        Fire,
        Puddle,
        Shock,
        None,
    }

    [SerializeField]
    GameObject fireEffect;

    [SerializeField]
    GameObject puddleEffect;

    [SerializeField]
    ParticleSystem smokeParticles;

    [SerializeField]
    ParticleSystem shockedParticles;

    Effect curEffect = Effect.None;

    private void Start()
    {
        ShowEffect(Effect.None);
    }

    public void ShowEffect(Effect effect)
    {
        curEffect = effect;        
        fireEffect.SetActive(effect == Effect.Fire);
        puddleEffect.SetActive(effect == Effect.Puddle);
    }

    /// <summary>
    /// Plays an effect without disabling the others
    /// </summary>
    public void PlayParticleEffect(Effect effect)
    {
        curEffect = effect;
        if(effect == Effect.Shock)
            shockedParticles?.Play();
    }

    public void DisableEffects()
    {
        fireEffect.SetActive(false);
        puddleEffect.SetActive(false);

        if (curEffect == Effect.Fire)
            smokeParticles?.Play();

        curEffect = Effect.None;
    }

    private void OnTriggerStay(Collider other)
    {
        if (curEffect == Effect.Fire)
            other.GetComponent<IBurnable>()?.Burn();

        if (curEffect == Effect.Shock)
            other.GetComponent<IShockable>()?.Shocked();
    }
}