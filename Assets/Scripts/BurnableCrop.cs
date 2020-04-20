using UnityEngine;

public class BurnableCrop : MonoBehaviour, IBurnable
{
    [SerializeField]
    CropTile parentCropTile;

    // Start is called before the first frame update
    void Start()
    {
        if (parentCropTile == null)
            parentCropTile = GetComponentInParent<CropTile>();
    }

    public void Burn()
    {
        parentCropTile?.GetComponent<IBurnable>()?.Burn();
    }
}
