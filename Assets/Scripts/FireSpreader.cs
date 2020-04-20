using UnityEngine;

public class FireSpreader : MonoBehaviour, IBurnable
{
    [SerializeField]
    Vector3 hScale = new Vector3(8f, 1f, 1f);
    [SerializeField]
    Vector3 vScale = new Vector3(1f, 1f, 8f);

    [SerializeField]
    LayerMask FireSpreadLayer;

    [SerializeField]
    GrassFieldTile burnableParent;

    private void Start()
    {
        if (burnableParent == null)
            burnableParent = GetComponentInParent<GrassFieldTile>();
    }

    public void Burn()
    {
        burnableParent?.GetComponent<IBurnable>()?.Burn();
    }

    public void Spread()
    {
        SpreadInDirection(vScale);
        SpreadInDirection(hScale);
    }

    void SpreadInDirection(Vector3 dir)
    {
        var colliders = Physics.OverlapBox(transform.position, dir, Quaternion.identity, FireSpreadLayer);

        foreach (var collider in colliders)
        {
            // Skip self
            if (collider.gameObject == gameObject)
                continue;

            // Debug.Log($"Collided with: {collider.name}");

            collider.gameObject.GetComponent<IBurnable>()?.Burn();
        }
    }
}
