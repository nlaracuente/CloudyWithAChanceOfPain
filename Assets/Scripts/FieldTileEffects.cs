using UnityEngine;

public class FieldTileEffects : MonoBehaviour
{
    public enum Effect
    {
        Fire,
        Puddle,
    }

    [SerializeField]
    GameObject fireEffect;

    [SerializeField]
    GameObject puddleEffect;

    public void ShowEffect(Effect effect)
    {
        fireEffect.SetActive(effect == Effect.Fire);
        puddleEffect.SetActive(effect == Effect.Puddle);
    }

    public void DisableEffects()
    {
        fireEffect.SetActive(false);
        puddleEffect.SetActive(false);
    }
}