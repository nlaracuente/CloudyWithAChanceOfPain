using UnityEngine;

public class LightningSpreader : MonoBehaviour, IShockable
{
    [SerializeField]
    Vector3 hScale = new Vector3(8f, 1f, 1f);
    [SerializeField]
    Vector3 vScale = new Vector3(1f, 1f, 8f);

    [SerializeField]
    LayerMask LightningSpreadLayer;

    [SerializeField]
    GrassFieldTile shockableParent;

    private void Start()
    {
        if (shockableParent == null)
            shockableParent = GetComponentInParent<GrassFieldTile>();
    }

    public void Shocked()
    {
        shockableParent?.GetComponent<IShockable>()?.Shocked();
    }

    public void Spread()
    {
        SpreadInDirection(vScale);
        SpreadInDirection(hScale);
    }

    void SpreadInDirection(Vector3 dir)
    {
        var colliders = Physics.OverlapBox(transform.position, dir, Quaternion.identity, LightningSpreadLayer);

        foreach (var collider in colliders)
        {
            // Skip self
            if (collider.gameObject == gameObject)
                continue;

            collider.gameObject.GetComponent<IShockable>()?.Shocked();
        }
    }
}
